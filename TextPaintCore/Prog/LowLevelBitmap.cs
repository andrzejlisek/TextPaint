using System;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;

namespace TextPaint
{
    public class LowLevelBitmap : LowLevelBitmapBase
    {
        public LowLevelBitmap(string FileName)
        {
            FileStream BmpFile = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            SKBitmap Bmp = SKBitmap.Decode(BmpFile);
            BmpFile.Close();
            CreateBase(Bmp.Width, Bmp.Height, 0);

            int P = 0;
            for (int Y = 0; Y < Height; Y++)
            {
                for (int X = 0; X < Width; X++)
                {
                    SKColor Color_ = Bmp.GetPixel(X, Y);
                    Data[P + 0] = Color_.Blue;
                    Data[P + 1] = Color_.Green;
                    Data[P + 2] = Color_.Red;
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

        public WriteableBitmap ToBitmap()
        {
            Monitor.Enter(Data);
            if (ToBitmapChanged)
            {
                if ((StretchW != Width) || (StretchH != Height))
                {
                    ToBitmapBmp = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
                    StretchW = Width;
                    StretchH = Height;
                }
                using (var frameBuffer = ToBitmapBmp.Lock()) 
                {
                    System.Runtime.InteropServices.Marshal.Copy(Data, 0, frameBuffer.Address, DataLength); 
                }
                ToBitmapChanged = false;
            }
            Monitor.Exit(Data);
            return ToBitmapBmp;
        }

        public WriteableBitmap ToBitmap(int W, int H)
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
                    ToBitmapBmp = new WriteableBitmap(new PixelSize(W, H), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
                }
                using (var frameBuffer = ToBitmapBmp.Lock()) 
                {
                    System.Runtime.InteropServices.Marshal.Copy(StretchData, 0, frameBuffer.Address, StretchDataL); 
                }
                ToBitmapChanged = false;
            }
            Monitor.Exit(Data);
            return ToBitmapBmp;
        }

        private SKBitmap ToBitmapX()
        {
            Monitor.Enter(Data);
            SKBitmap ToBitmapBmpX = new SKBitmap(Width, Height, SKColorType.Bgra8888, SKAlphaType.Opaque);
            System.Runtime.InteropServices.Marshal.Copy(Data, 0, ToBitmapBmpX.GetPixels(), DataLength); 
            Monitor.Exit(Data);
            return ToBitmapBmpX;
        }
 

         public void SaveToFile(string FileName)
        {
            SKBitmap Bmp = ToBitmapX();
            FileStream BmpFile = new FileStream(FileName, FileMode.Create, FileAccess.Write);
            SKData FileData = Bmp.Encode(SKEncodedImageFormat.Png, 100);
            FileData.SaveTo(BmpFile);
            BmpFile.Close();
        }

        public void SetTextFont(string FontName, int FontSize, int CharRender)
        {
            switch (FontName)
            {
                case "GenericSerif":
                    FontName = "serif";
                    break;
                case "GenericSansSerif":
                    FontName = "sans-serif";
                    break;
                case "GenericMonospace":
                    FontName = "monospace";
                    break;
            }
            WinFontFace[0x0] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.Normal);
            WinFontFace[0x1] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.Bold);
            WinFontFace[0x2] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.Italic);
            WinFontFace[0x3] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.BoldItalic);
            WinFontFace[0x4] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.Normal);
            WinFontFace[0x5] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.Bold);
            WinFontFace[0x6] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.Italic);
            WinFontFace[0x7] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.BoldItalic);
            WinFontFace[0x8] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.Normal);
            WinFontFace[0x9] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.Bold);
            WinFontFace[0xA] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.Italic);
            WinFontFace[0xB] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.BoldItalic);
            WinFontFace[0xC] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.Normal);
            WinFontFace[0xD] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.Bold);
            WinFontFace[0xE] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.Italic);
            WinFontFace[0xF] = SKTypeface.FromFamilyName(FontName, SkiaSharp.SKFontStyle.BoldItalic);
            for (int I = 0; I < 16; I++)
            {
                WinFont[I] = new SKFont(WinFontFace[I], FontSize, 1, 0);
                WinPaint[I] = new SKPaint(WinFont[I]);
                WinPaint[I].TextAlign = SKTextAlign.Center;
                switch (CharRender)
                {
                    default:
                        WinPaint[I].IsAntialias = false;
                        WinPaint[I].SubpixelText = false;
                        break;
                    case 1:
                        WinPaint[I].IsAntialias = true;
                        WinPaint[I].SubpixelText = false;
                        break;
                }
                SKRect TextBounds = new SKRect();
                CharW[I] = 0;
                CharH[I] = 0;
                int[] MeasureChars = new int[] { 'A', 'X', 'T', 'Y' };
                for (int II = 0; II < MeasureChars.Length; II++)
                {
                    WinPaint[I].MeasureText(((char)MeasureChars[II]).ToString(), ref TextBounds);
                    CharW[I] = CharW[I] + TextBounds.Width;
                    CharH[I] = CharH[I] + TextBounds.Height;
                }
                CharW[I] = CharW[I] / (float)(MeasureChars.Length);
                CharH[I] = CharH[I] / (float)(MeasureChars.Length);
            }
        }

        public void DrawText(float X, float Y, float W, float H, float ScaleH, float ScaleV, string T, byte R, byte G, byte B, bool FontB, bool FontI, bool FontU, bool FontS)
        {
            try
            {
                int N = (FontB ? 1 : 0) + (FontI ? 2 : 0) + (FontU ? 4 : 0) + (FontU ? 8 : 0);

                WinPaint[N].StrokeWidth = 0;

                X = 0 - (X / ScaleH);
                Y = 0 - (Y / ScaleV);
                W = W / ScaleH;
                H = H / ScaleV;

                SKBitmap ToBitmapBmpX = ToBitmapX();
                WinPaint[N].Color = new SKColor(R, G, B);
                SKCanvas BmpG = new SKCanvas(ToBitmapBmpX);
                BmpG.Scale(ScaleH, ScaleV, 0, 0);

                float X_ = (W) / 2.0f;
                float Y_ = H - ((H - CharH[N]) / 2.0f);
                BmpG.DrawText(T, X + X_, Y + Y_, WinPaint[N]);

                if (FontU || FontS)
                {
                    float PosB = (Y + Y_);
                    //float Pos1 = X + X_ - (CharW[N] / 2);
                    //float Pos2 = X + X_ + (CharW[N] / 2);
                    float Pos1 = 0;
                    float Pos2 = W;

                    if (FontU)
                    {
                        WinPaint[N].StrokeWidth = (float)WinFont[N].Metrics.UnderlineThickness;
                        BmpG.DrawLine(Pos1, PosB + (float)WinFont[N].Metrics.UnderlinePosition, Pos2, PosB + (float)WinFont[N].Metrics.UnderlinePosition, WinPaint[N]);
                    }

                    if (FontS)
                    {
                        WinPaint[N].StrokeWidth = (float)WinFont[N].Metrics.StrikeoutThickness;
                        BmpG.DrawLine(Pos1, PosB + (float)WinFont[N].Metrics.StrikeoutPosition, Pos2, PosB + (float)WinFont[N].Metrics.StrikeoutPosition, WinPaint[N]);
                    }
                }

                System.Runtime.InteropServices.Marshal.Copy(ToBitmapBmpX.GetPixels(), Data, 0, DataLength); 
                ToBitmapChanged = true;
            }
            catch (Exception E)
            {
            }
        }



        private SKTypeface[] WinFontFace = new SKTypeface[16];
        private SKFont[] WinFont = new SKFont[16];
        private SKPaint[] WinPaint = new SKPaint[16];
        float[] CharW = new float[16];
        float[] CharH = new float[16];
        private WriteableBitmap ToBitmapBmp;
    }
}
