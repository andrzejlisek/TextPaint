using System;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;

namespace TextPaint
{
    public class LowLevelBitmap : LowLevelBitmapBase
    {
        PixelFormat ColorDataFormat = PixelFormat.Format32bppArgb;

        public LowLevelBitmap(string FileName)
        {
            Bitmap Bmp = new Bitmap(FileName);
            CreateBase(Bmp.Width, Bmp.Height, 0);

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

        public LowLevelBitmap(int W, int H, byte R, byte G, byte B)
        {
            CreateBase(W, H, R, G, B);
        }

        public LowLevelBitmap(int W, int H, byte Val)
        {
            CreateBase(W, H, Val);
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

        public Bitmap ToBitmap(int W, int H)
        {
            if ((W == Width) && (H == Height))
            {
                return ToBitmap();
            }

            Monitor.Enter(Data);
            if (ToBitmapChanged)
            {
                if (PrepareStretch(W, H))
                {
                    ToBitmapBmp = new Bitmap(W, H, ColorDataFormat);
                }
                BitmapData Bmp_ = ToBitmapBmp.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.ReadWrite, ColorDataFormat);
                System.Runtime.InteropServices.Marshal.Copy(StretchData, 0, Bmp_.Scan0, StretchDataL);
                ToBitmapBmp.UnlockBits(Bmp_);
                ToBitmapChanged = false;
            }
            Monitor.Exit(Data);
            return ToBitmapBmp;
        }

        public void SaveToFile(string FileName)
        {
            Bitmap Bmp = ToBitmap();
            Bmp.Save(FileName, ImageFormat.Png);
        }

        public void SetTextFont(string FontName, int FontSize, int CharRender)
        {
            WinCharRender = CharRender;
            switch (FontName)
            {
                case "GenericSerif":
                    WinFont = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Regular);
                    break;
                case "GenericSansSerif":
                    WinFont = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Regular);
                    break;
                case "GenericMonospace":
                    WinFont = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Regular);
                    break;
                default:
                    WinFont = new Font(FontName, FontSize, FontStyle.Regular);
                    break;
            }
            WinStrFormat = new StringFormat();
            WinStrFormat.LineAlignment = StringAlignment.Center;
            WinStrFormat.Alignment = StringAlignment.Center;
            WinStrFormat.Trimming = StringTrimming.None;
            WinStrFormat.FormatFlags = StringFormatFlags.NoWrap;
        }

        public void DrawText(float X, float Y, float W, float H, float ScaleH, float ScaleV, string T, byte R, byte G, byte B)
        {
            try
            {
                X = 0 - (X / ScaleH);
                Y = 0 - (Y / ScaleV);
                W = W / ScaleH;
                H = H / ScaleV;
                Graphics BmpG = Graphics.FromImage(ToBitmap());
                BmpG.ScaleTransform(ScaleH, ScaleV);
                switch (WinCharRender)
                {
                    default:
                        BmpG.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                        break;
                    case 1:
                        BmpG.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        break;
                    case 2:
                        BmpG.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                        break;
                }
                BmpG.DrawString(T, WinFont, new SolidBrush(Color.FromArgb(R, G, B)), new RectangleF(X, Y, W, H), WinStrFormat);

                BitmapData Bmp_ = ToBitmapBmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ColorDataFormat);
                System.Runtime.InteropServices.Marshal.Copy(Bmp_.Scan0, Data, 0, DataLength);
                ToBitmapBmp.UnlockBits(Bmp_);
                ToBitmapChanged = true;
            }
            catch (Exception E)
            {
            }
        }

        int WinCharRender = 0;
        private Font WinFont;
        private StringFormat WinStrFormat;
        private Bitmap ToBitmapBmp;
    }
}
