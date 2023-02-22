using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace TextPaint
{
    public class LowLevelBitmap
    {
        int ColorDataFactor = 4;
        PixelFormat ColorDataFormat = PixelFormat.Format32bppArgb;

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
            ToBitmapChangedA = true;
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
                    Data[P + 3] = (byte)255;
                    P += ColorDataFactor;
                }
            }
        }

        public LowLevelBitmap(int W, int H, byte Val)
        {
            Create(W, H);
            for (int i = 0; i < DataLength; i++)
            {
                Data[i] = Val;
                if ((i % 4) == 3)
                {
                    Data[i] = (byte)255;
                }
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
                    Data[P + 3] = (byte)255;
                    P += ColorDataFactor;
                }
            }
        }

        public Bitmap ToBitmap()
        {
            Monitor.Enter(Data);
            if (ToBitmapChanged)
            {
                if ((StretchW != Width) || (StretchH != Height))
                {
                    ToBitmapBmp = new Bitmap(Width, Height, ColorDataFormat);
                    StretchW = Width;
                    StretchH = Height;
                }
                BitmapData Bmp_ = ToBitmapBmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ColorDataFormat);
                System.Runtime.InteropServices.Marshal.Copy(Data, 0, Bmp_.Scan0, DataLength);
                ToBitmapBmp.UnlockBits(Bmp_);
                ToBitmapChanged = false;
            }
            Monitor.Exit(Data);
            return ToBitmapBmp;
        }

        public Bitmap ToBitmapStretch(int W, int H)
        {
            if ((W == Width) && (H == Height))
            {
                return ToBitmap();
            }

            Monitor.Enter(Data);
            if (ToBitmapChanged)
            {
                if ((StretchW != W) || (StretchH != H))
                {
                    ToBitmapBmp = new Bitmap(W, H, ColorDataFormat);
                    StretchW = W;
                    StretchH = H;
                    StretchDataL = W * H * ColorDataFactor;
                    StretchData = new byte[StretchDataL];

                    StretchX = new int[W];
                    for (int I = 0; I < W; I++)
                    {
                        StretchX[I] = (I * Width) / W;
                        if (StretchX[I] >= Width)
                        {
                            StretchX[I] = Width - 1;
                        }
                    }
                    StretchY = new int[H];
                    for (int I = 0; I < H; I++)
                    {
                        StretchY[I] = (I * Height) / H;
                        if (StretchY[I] >= Height)
                        {
                            StretchY[I] = Height - 1;
                        }
                    }
                }
                for (int Y = 0; Y < H; Y++)
                {
                    for (int X = 0; X < W; X++)
                    {
                        int PtrI = (StretchY[Y] * Width + StretchX[X]) * ColorDataFactor;
                        int PtrO = (Y * W + X) * ColorDataFactor;
                        StretchData[PtrO + 0] = Data[PtrI + 0];
                        StretchData[PtrO + 1] = Data[PtrI + 1];
                        StretchData[PtrO + 2] = Data[PtrI + 2];
                        StretchData[PtrO + 3] = (byte)255;
                    }
                }
                BitmapData Bmp_ = ToBitmapBmp.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.ReadWrite, ColorDataFormat);
                System.Runtime.InteropServices.Marshal.Copy(StretchData, 0, Bmp_.Scan0, StretchDataL);
                ToBitmapBmp.UnlockBits(Bmp_);
                ToBitmapChanged = false;
            }
            Monitor.Exit(Data);
            return ToBitmapBmp;
        }

        public Avalonia.Media.Imaging.WriteableBitmap ToBitmapAvalonia()
        {
            Monitor.Enter(Data);
            if (ToBitmapChangedA)
            {
                if ((StretchWA != Width) || (StretchHA != Height))
                {
                    ToBitmapBmpA = new Avalonia.Media.Imaging.WriteableBitmap(new Avalonia.PixelSize(Width, Height), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Opaque);
                    StretchWA = Width;
                    StretchHA = Height;
                }
                using (var frameBuffer = ToBitmapBmpA.Lock()) 
                {
                    System.Runtime.InteropServices.Marshal.Copy(Data, 0, frameBuffer.Address, DataLength); 
                }
                ToBitmapChangedA = false;
            }         
            Monitor.Exit(Data);
            return ToBitmapBmpA;
        }

        public Avalonia.Media.Imaging.WriteableBitmap ToBitmapAvaloniaStretch(int W, int H)
        {
            if ((W == Width) && (H == Height))
            {
                return ToBitmapAvalonia();
            }

            Monitor.Enter(Data);
            if (ToBitmapChangedA)
            {
                if ((StretchWA != W) || (StretchHA != H))
                {
                    ToBitmapBmpA = new Avalonia.Media.Imaging.WriteableBitmap(new Avalonia.PixelSize(W, H), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Opaque);
                    StretchWA = W;
                    StretchHA = H;
                    StretchDataLA = W * H * ColorDataFactor;
                    StretchDataA = new byte[StretchDataLA];

                    StretchXA = new int[W];
                    for (int I = 0; I < W; I++)
                    {
                        StretchXA[I] = (I * Width) / W;
                        if (StretchXA[I] >= Width)
                        {
                            StretchXA[I] = Width - 1;
                        }
                    }
                    StretchYA = new int[H];
                    for (int I = 0; I < H; I++)
                    {
                        StretchYA[I] = (I * Height) / H;
                        if (StretchYA[I] >= Height)
                        {
                            StretchYA[I] = Height - 1;
                        }
                    }
                }
                for (int Y = 0; Y < H; Y++)
                {
                    for (int X = 0; X < W; X++)
                    {
                        int PtrI = (StretchYA[Y] * Width + StretchXA[X]) * ColorDataFactor;
                        int PtrO = (Y * W + X) * ColorDataFactor;
                        StretchDataA[PtrO + 0] = Data[PtrI + 0];
                        StretchDataA[PtrO + 1] = Data[PtrI + 1];
                        StretchDataA[PtrO + 2] = Data[PtrI + 2];
                        StretchDataA[PtrO + 3] = (byte)255;
                    }
                }
                using (var frameBuffer = ToBitmapBmpA.Lock()) 
                {
                    System.Runtime.InteropServices.Marshal.Copy(StretchDataA, 0, frameBuffer.Address, StretchDataLA); 
                }
                ToBitmapChangedA = false;
            }         
            Monitor.Exit(Data);
            return ToBitmapBmpA;
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
            ToBitmapChangedA = true;
            Data[(Y * Width + X) * ColorDataFactor + 0] = B;
            Data[(Y * Width + X) * ColorDataFactor + 1] = G;
            Data[(Y * Width + X) * ColorDataFactor + 2] = R;
            Monitor.Exit(Data);
        }

        public void SetPixel(int X, int Y, Color C)
        {
            Monitor.Enter(Data);
            ToBitmapChanged = true;
            ToBitmapChangedA = true;
            Data[(Y * Width + X) * ColorDataFactor + 0] = C.B;
            Data[(Y * Width + X) * ColorDataFactor + 1] = C.G;
            Data[(Y * Width + X) * ColorDataFactor + 2] = C.R;
            Monitor.Exit(Data);
        }

        public void Clear(byte R, byte G, byte B)
        {
            Monitor.Enter(Data);
            ToBitmapChanged = true;
            ToBitmapChangedA = true;
            int P = 0;
            for (int Y = 0; Y < Height; Y++)
            {
                for (int X = 0; X < Width; X++)
                {
                    Data[P + 0] = B;
                    Data[P + 1] = G;
                    Data[P + 2] = R;
                    Data[P + 3] = (byte)255;
                    P += ColorDataFactor;
                }
            }
            Monitor.Exit(Data);
        }

        public void DrawImage(LowLevelBitmap Bmp, int SrcX, int SrcY, int DstX, int DstY, int W, int H)
        {
            Monitor.Enter(Data);
            ToBitmapChanged = true;
            ToBitmapChangedA = true;
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

        private Bitmap ToBitmapBmp;
        private bool ToBitmapChanged;
        private byte[] StretchData;
        private int StretchDataL;
        private int StretchW = -1;
        private int StretchH = -1;
        private int[] StretchX;
        private int[] StretchY;

        private Avalonia.Media.Imaging.WriteableBitmap ToBitmapBmpA;
        private bool ToBitmapChangedA;
        private byte[] StretchDataA;
        private int StretchDataLA;
        private int StretchWA = -1;
        private int StretchHA = -1;
        private int[] StretchXA;
        private int[] StretchYA;
    }
}
