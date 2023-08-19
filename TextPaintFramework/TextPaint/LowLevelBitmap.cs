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
                    WinFont[0x00] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Regular);
                    WinFont[0x01] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Bold);
                    WinFont[0x02] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Italic);
                    WinFont[0x03] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Bold | FontStyle.Italic);
                    WinFont[0x04] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Underline);
                    WinFont[0x05] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Bold | FontStyle.Underline);
                    WinFont[0x06] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Italic | FontStyle.Underline);
                    WinFont[0x07] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline);
                    WinFont[0x08] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Strikeout);
                    WinFont[0x09] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Bold | FontStyle.Strikeout);
                    WinFont[0x0A] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Italic | FontStyle.Strikeout);
                    WinFont[0x0B] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Strikeout);
                    WinFont[0x0C] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Underline | FontStyle.Strikeout);
                    WinFont[0x0D] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Bold | FontStyle.Underline | FontStyle.Strikeout);
                    WinFont[0x0E] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout);
                    WinFont[0x0F] = new Font(FontFamily.GenericSerif, FontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout);
                    break;
                case "GenericSansSerif":
                    WinFont[0x00] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Regular);
                    WinFont[0x01] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Bold);
                    WinFont[0x02] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Italic);
                    WinFont[0x03] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Bold | FontStyle.Italic);
                    WinFont[0x04] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Underline);
                    WinFont[0x05] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Bold | FontStyle.Underline);
                    WinFont[0x06] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Italic | FontStyle.Underline);
                    WinFont[0x07] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline);
                    WinFont[0x08] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Strikeout);
                    WinFont[0x09] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Bold | FontStyle.Strikeout);
                    WinFont[0x0A] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Italic | FontStyle.Strikeout);
                    WinFont[0x0B] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Strikeout);
                    WinFont[0x0C] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Underline | FontStyle.Strikeout);
                    WinFont[0x0D] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Bold | FontStyle.Underline | FontStyle.Strikeout);
                    WinFont[0x0E] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout);
                    WinFont[0x0F] = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout);
                    break;
                case "GenericMonospace":
                    WinFont[0x00] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Regular);
                    WinFont[0x01] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Bold);
                    WinFont[0x02] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Italic);
                    WinFont[0x03] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Bold | FontStyle.Italic);
                    WinFont[0x04] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Underline);
                    WinFont[0x05] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Bold | FontStyle.Underline);
                    WinFont[0x06] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Italic | FontStyle.Underline);
                    WinFont[0x07] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline);
                    WinFont[0x08] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Strikeout);
                    WinFont[0x09] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Bold | FontStyle.Strikeout);
                    WinFont[0x0A] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Italic | FontStyle.Strikeout);
                    WinFont[0x0B] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Strikeout);
                    WinFont[0x0C] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Underline | FontStyle.Strikeout);
                    WinFont[0x0D] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Bold | FontStyle.Underline | FontStyle.Strikeout);
                    WinFont[0x0E] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout);
                    WinFont[0x0F] = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout);
                    break;
                default:
                    WinFont[0x00] = new Font(FontName, FontSize, FontStyle.Regular);
                    WinFont[0x01] = new Font(FontName, FontSize, FontStyle.Bold);
                    WinFont[0x02] = new Font(FontName, FontSize, FontStyle.Italic);
                    WinFont[0x03] = new Font(FontName, FontSize, FontStyle.Bold | FontStyle.Italic);
                    WinFont[0x04] = new Font(FontName, FontSize, FontStyle.Underline);
                    WinFont[0x05] = new Font(FontName, FontSize, FontStyle.Bold | FontStyle.Underline);
                    WinFont[0x06] = new Font(FontName, FontSize, FontStyle.Italic | FontStyle.Underline);
                    WinFont[0x07] = new Font(FontName, FontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline);
                    WinFont[0x08] = new Font(FontName, FontSize, FontStyle.Strikeout);
                    WinFont[0x09] = new Font(FontName, FontSize, FontStyle.Bold | FontStyle.Strikeout);
                    WinFont[0x0A] = new Font(FontName, FontSize, FontStyle.Italic | FontStyle.Strikeout);
                    WinFont[0x0B] = new Font(FontName, FontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Strikeout);
                    WinFont[0x0C] = new Font(FontName, FontSize, FontStyle.Underline | FontStyle.Strikeout);
                    WinFont[0x0D] = new Font(FontName, FontSize, FontStyle.Bold | FontStyle.Underline | FontStyle.Strikeout);
                    WinFont[0x0E] = new Font(FontName, FontSize, FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout);
                    WinFont[0x0F] = new Font(FontName, FontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout);
                    break;
            }
            WinStrFormat = new StringFormat();
            WinStrFormat.LineAlignment = StringAlignment.Center;
            WinStrFormat.Alignment = StringAlignment.Center;
            WinStrFormat.Trimming = StringTrimming.None;
            WinStrFormat.FormatFlags = StringFormatFlags.NoWrap;
        }

        public void DrawText(float X, float Y, float W, float H, float ScaleH, float ScaleV, string T, byte R, byte G, byte B, bool FontB, bool FontI, bool FontU, bool FontS)
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
                float RectExp = 0;
                if (FontU || FontS)
                {
                    T = "|     " + T + "     |";
                    RectExp = BmpG.MeasureString(T, WinFont[15], 1000).Width;
                }
                BmpG.DrawString(T, WinFont[(FontB ? 1 : 0) + (FontI ? 2 : 0) + (FontU ? 4 : 0) + (FontU ? 8 : 0)], new SolidBrush(Color.FromArgb(R, G, B)), new RectangleF(X - RectExp, Y, W + RectExp + RectExp, H), WinStrFormat);

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
        private Font[] WinFont = new Font[16];
        private StringFormat WinStrFormat;
        private Bitmap ToBitmapBmp;
    }
}
