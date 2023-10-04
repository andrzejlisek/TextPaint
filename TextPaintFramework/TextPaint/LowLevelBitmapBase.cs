using System;
using System.Threading;

namespace TextPaint
{
    public class LowLevelBitmapBase
    {
        public LowLevelBitmapBase()
        {
        }

        public static int ColorRgbToInt(int R, int G, int B)
        {
            return R + (G << 8) + (B << 16);
        }

        public static int[] ColorIntToRgb(int RGB)
        {
            int[] RGB_ = new int[3];
            RGB_[0] = RGB & 255;
            RGB = RGB >> 8;
            RGB_[1] = RGB & 255;
            RGB = RGB >> 8;
            RGB_[2] = RGB & 255;
            return RGB_;
        }

        protected void CreateBase(int W, int H, byte R, byte G, byte B)
        {
            Width = W;
            Height = H;
            StretchW = -1;
            StretchH = -1;
            Data = new byte[W * H * ColorDataFactor];
            DataLength = Data.Length;
            ToBitmapChanged = true;
            for (int i = 0; i < DataLength; i += ColorDataFactor)
            {
                Data[i + 0] = B;
                Data[i + 1] = G;
                Data[i + 2] = R;
                Data[i + 3] = (byte)255;
            }
        }

        protected void CreateBase(int W, int H, byte Val)
        {
            Width = W;
            Height = H;
            StretchW = -1;
            StretchH = -1;
            Data = new byte[(W * H) << ColorDataFactorSh];
            DataLength = Data.Length;
            ToBitmapChanged = true;
            for (int i = 0; i < DataLength; i++)
            {
                Data[i] = Val;
                if ((i % 4) == 3)
                {
                    Data[i] = (byte)255;
                }
            }
        }

        protected int ColorDataFactor = 4;
        protected int ColorDataFactorSh = 2;

        public int GetPixelLevel(int X, int Y)
        {
            Monitor.Enter(Data);
            int B = Data[((Y * Width + X) << ColorDataFactorSh) + 0];
            int G = Data[((Y * Width + X) << ColorDataFactorSh) + 1];
            int R = Data[((Y * Width + X) << ColorDataFactorSh) + 2];
            Monitor.Exit(Data);
            return ((R + G + B) / 3);
        }

        public bool GetPixelBinary(int X, int Y)
        {
            Monitor.Enter(Data);
            int B = Data[((Y * Width + X) << ColorDataFactorSh) + 0];
            int G = Data[((Y * Width + X) << ColorDataFactorSh) + 1];
            int R = Data[((Y * Width + X) << ColorDataFactorSh) + 2];
            Monitor.Exit(Data);
            return ((R + G + B) >= 383);
        }

        public int GetPixel(int X, int Y)
        {
            Monitor.Enter(Data);
            int B = Data[((Y * Width + X) << ColorDataFactorSh) + 0];
            int G = Data[((Y * Width + X) << ColorDataFactorSh) + 1];
            int R = Data[((Y * Width + X) << ColorDataFactorSh) + 2];
            Monitor.Exit(Data);
            return ColorRgbToInt(R, G, B);
        }

        public void GetPixel(int X, int Y, out byte R, out byte G, out byte B)
        {
            Monitor.Enter(Data);
            B = Data[((Y * Width + X) << ColorDataFactorSh) + 0];
            G = Data[((Y * Width + X) << ColorDataFactorSh) + 1];
            R = Data[((Y * Width + X) << ColorDataFactorSh) + 2];
            Monitor.Exit(Data);
        }

        public void SetPixel(int X, int Y, byte R, byte G, byte B)
        {
            Monitor.Enter(Data);
            ToBitmapChanged = true;
            Data[((Y * Width + X) << ColorDataFactorSh) + 0] = B;
            Data[((Y * Width + X) << ColorDataFactorSh) + 1] = G;
            Data[((Y * Width + X) << ColorDataFactorSh) + 2] = R;
            Monitor.Exit(Data);
        }

        public void SetPixel(int X, int Y, int RGB)
        {
            Monitor.Enter(Data);
            byte R = (byte)(RGB & 255);
            RGB = RGB >> 8;
            byte G = (byte)(RGB & 255);
            RGB = RGB >> 8;
            byte B = (byte)(RGB & 255);
            ToBitmapChanged = true;
            Data[((Y * Width + X) << ColorDataFactorSh) + 0] = B;
            Data[((Y * Width + X) << ColorDataFactorSh) + 1] = G;
            Data[((Y * Width + X) << ColorDataFactorSh) + 2] = R;
            Monitor.Exit(Data);
        }

        public void SetPixelGray(int X, int Y, byte L)
        {
            Monitor.Enter(Data);
            ToBitmapChanged = true;
            Data[((Y * Width + X) << ColorDataFactorSh) + 0] = L;
            Data[((Y * Width + X) << ColorDataFactorSh) + 1] = L;
            Data[((Y * Width + X) << ColorDataFactorSh) + 2] = L;
            Monitor.Exit(Data);
        }

        public void Clear(byte R, byte G, byte B)
        {
            Monitor.Enter(Data);
            ToBitmapChanged = true;
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

        public void DrawRectangle(int X, int Y, int W, int H, byte R, byte G, byte B)
        {
            Monitor.Enter(Data);
            ToBitmapChanged = true;
            for (int Y_ = 0; Y_ < H; Y_++)
            {
                for (int X_ = 0; X_ < W; X_++)
                {
                    SetPixel(X + X_, Y + Y_, R, G, B);
                }
            }
            Monitor.Exit(Data);
        }

        public void DrawImageCtrl(LowLevelBitmap Bmp, int SrcX, int SrcY, int DstX, int DstY, int W, int H)
        {
            if ((SrcX < 0) || (DstX < 0))
            {
                int T = 0 - Math.Min(SrcX, DstX);
                SrcX += T;
                DstX += T;
                W -= T;
            }
            if ((SrcX + W) > Bmp.Width)
            {
                W = Bmp.Width - SrcX;
            }
            if ((DstX + W) > Width)
            {
                W = Width - DstX;
            }
            if ((SrcY < 0) || (DstY < 0))
            {
                int T = 0 - Math.Min(SrcY, DstY);
                SrcY += T;
                DstY += T;
                H -= T;
            }
            if ((SrcY + H) > Bmp.Height)
            {
                H = Bmp.Height - SrcY;
            }
            if ((DstY + H) > Height)
            {
                H = Height - DstY;
            }
            DrawImage(Bmp, SrcX, SrcY, DstX, DstY, W, H);
        }

        public void DrawImage(LowLevelBitmap Bmp, int SrcX, int SrcY, int DstX, int DstY, int W, int H)
        {
            Monitor.Enter(Data);
            ToBitmapChanged = true;
            int W_ = W << ColorDataFactorSh;
            int Width0 = Bmp.Width << ColorDataFactorSh;
            int Width_ = Width << ColorDataFactorSh;
            int SrcP = (((SrcY) * Bmp.Width) + SrcX) << ColorDataFactorSh;
            int DstP = (((DstY) * Width) + DstX) << ColorDataFactorSh;
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

        public LowLevelBitmap Clone()
        {
            LowLevelBitmap _ = new LowLevelBitmap(Width, Height, 0);
            for (int i = 0; i < DataLength; i++)
            {
                _.Data[i] = Data[i];
            }
            ToBitmapChanged = true;
            return _;
        }

        protected bool PrepareStretch(int W, int H)
        {
            bool NewBmp = false;
            if ((StretchW != W) || (StretchH != H))
            {
                NewBmp = true;
                StretchW = W;
                StretchH = H;
                StretchDataL = (W * H) << ColorDataFactorSh;
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
                for (int Y = 0; Y < H; Y++)
                {
                    for (int X = 0; X < W; X++)
                    {
                        int PtrI = (StretchY[Y] * Width + StretchX[X]) << ColorDataFactorSh;
                        int PtrO = (Y * W + X) << ColorDataFactorSh;
                        StretchData[PtrO + 3] = (byte)255;
                    }
                }
            }
            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int PtrI = (StretchY[Y] * Width + StretchX[X]) << ColorDataFactorSh;
                    int PtrO = (Y * W + X) << ColorDataFactorSh;
                    StretchData[PtrO + 0] = Data[PtrI + 0];
                    StretchData[PtrO + 1] = Data[PtrI + 1];
                    StretchData[PtrO + 2] = Data[PtrI + 2];
                }
            }
            return NewBmp;
        }

        public int Width;
        public int Height;
        protected byte[] Data;
        protected int DataLength;

        protected bool ToBitmapChanged;
        protected byte[] StretchData;
        protected int StretchDataL;
        protected int StretchW = -1;
        protected int StretchH = -1;
        protected int[] StretchX;
        protected int[] StretchY;
    }
}
