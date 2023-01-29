using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace TextPaint
{
    public class LowLevelBitmap
    {
        int ColorDataFactor = 3;
        PixelFormat ColorDataFormat = PixelFormat.Format24bppRgb;

        public static Brush GetBrush(byte R, byte G, byte B)
        {
            return new SolidBrush(Color.FromArgb(R, G, B));
        }

        private void Create(int W, int H)
        {
            Width = W;
            Height = H;
            Data = new byte[W * H * ColorDataFactor];
            DataLength = Data.Length;
            ToBitmapBmp = new Bitmap(Width, Height, ColorDataFormat);
            ToBitmapChanged = true;
        }

        public LowLevelBitmap(string FileName)
        {
            Bitmap Bmp = new Bitmap(FileName);
            Create(Bmp.Width, Bmp.Height);

            // !!!!!!!!!!!!!!!!!!!
            int P = 0;
            for (int Y = 0; Y < Height; Y++)
            {
                for (int X = 0; X < Width; X++)
                {
                    Color Color_ = Bmp.GetPixel(X, Y);
                    Data[P + 0] = Color_.B;
                    Data[P + 1] = Color_.G;
                    Data[P + 2] = Color_.R;
                    P += ColorDataFactor;
                }
            }
        }

        public LowLevelBitmap(int W, int H)
        {
            Create(W, H);
            for (int i = 0; i < DataLength; i++)
            {
                Data[i] = 0;
            }
        }

        public LowLevelBitmap(Bitmap Bmp)
        {
            Create(Bmp.Width, Bmp.Height);

            // !!!!!!!!!!!!!!!!!!!
            int P = 0;
            for (int Y = 0; Y < Height; Y++)
            {
                for (int X = 0; X < Width; X++)
                {
                    Color Color_ = Bmp.GetPixel(X, Y);
                    Data[P + 0] = Color_.B;
                    Data[P + 1] = Color_.G;
                    Data[P + 2] = Color_.R;
                    P += ColorDataFactor;
                }
            }
        }

        public Bitmap ToBitmap()
        {
            Monitor.Enter(Data);
            if (ToBitmapChanged)
            {
                BitmapData Bmp_ = ToBitmapBmp.LockBits(new Rectangle(0, 0, ToBitmapBmp.Width, ToBitmapBmp.Height), ImageLockMode.ReadWrite, ColorDataFormat);
                System.Runtime.InteropServices.Marshal.Copy(Data, 0, Bmp_.Scan0, DataLength);
                ToBitmapBmp.UnlockBits(Bmp_);
                ToBitmapChanged = false;
            }
            Monitor.Exit(Data);
            return ToBitmapBmp;
        }

        public bool GetPixelBinary(int X, int Y)
        {
            Monitor.Enter(Data);
            int B = Data[(Y * Width + X) * ColorDataFactor + 0];
            int G = Data[(Y * Width + X) * ColorDataFactor + 1];
            int R = Data[(Y * Width + X) * ColorDataFactor + 2];
            Monitor.Exit(Data);
            return ((R + G + B) >= 383);
        }

        public void GetPixel(int X, int Y, out byte R, out byte G, out byte B)
        {
            Monitor.Enter(Data);
            B = Data[(Y * Width + X) * ColorDataFactor + 0];
            G = Data[(Y * Width + X) * ColorDataFactor + 1];
            R = Data[(Y * Width + X) * ColorDataFactor + 2];
            Monitor.Exit(Data);
        }

        public void SetPixel(int X, int Y, byte R, byte G, byte B)
        {
            Monitor.Enter(Data);
            ToBitmapChanged = true;
            Data[(Y * Width + X) * ColorDataFactor + 0] = B;
            Data[(Y * Width + X) * ColorDataFactor + 1] = G;
            Data[(Y * Width + X) * ColorDataFactor + 2] = R;
            Monitor.Exit(Data);
        }

        public void SetPixel(int X, int Y, Color C)
        {
            Monitor.Enter(Data);
            ToBitmapChanged = true;
            Data[(Y * Width + X) * ColorDataFactor + 0] = C.B;
            Data[(Y * Width + X) * ColorDataFactor + 1] = C.G;
            Data[(Y * Width + X) * ColorDataFactor + 2] = C.R;
            Monitor.Exit(Data);
        }

        public void DrawImage(LowLevelBitmap Bmp, int SrcX, int SrcY, int DstX, int DstY, int W, int H)
        {
            Monitor.Enter(Data);
            ToBitmapChanged = true;
            int W_ = W * ColorDataFactor;
            int Width0 = Bmp.Width * ColorDataFactor;
            int Width_ = Width * ColorDataFactor;
            int SrcP = (((SrcY) * Bmp.Width) + SrcX) * ColorDataFactor;
            int DstP = (((DstY) * Width) + DstX) * ColorDataFactor;
            if (SrcP > DstP)
            {
                for (int Y = 0; Y < H; Y++)
                {
                    Array.Copy(Bmp.Data, SrcP, Data, DstP, W_);
                    SrcP += Width0;
                    DstP += Width_;
                }
            }
            else
            {
                SrcP += (Width0 * (H - 1));
                DstP += (Width_ * (H - 1));

                for (int Y = 0; Y < H; Y++)
                {
                    Array.Copy(Bmp.Data, SrcP, Data, DstP, W_);
                    SrcP -= Width0;
                    DstP -= Width_;
                }
            }
            Monitor.Exit(Data);
        }



        public int Width;
        public int Height;
        private byte[] Data;
        private int DataLength;
        public Bitmap ToBitmapBmp;
        bool ToBitmapChanged;
    }
}
