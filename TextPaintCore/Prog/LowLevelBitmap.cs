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
                    WinFontFace = SKTypeface.FromFamilyName("serif");
                    break;
                case "GenericSansSerif":
                    WinFontFace = SKTypeface.FromFamilyName("sans-serif");
                    break;
                case "GenericMonospace":
                    WinFontFace = SKTypeface.FromFamilyName("monospace");
                    break;
                default:
                    WinFontFace = SKTypeface.FromFamilyName(FontName);
                    break;
            }
            WinFont = new SKFont(WinFontFace, FontSize, 1, 0);
            WinPaint = new SKPaint(WinFont);
            WinPaint.TextAlign = SKTextAlign.Center;
            switch (CharRender)
            {
                default:
                    WinPaint.IsAntialias = false;
                    WinPaint.SubpixelText = false;
                    break;
                case 1:
                    WinPaint.IsAntialias = true;
                    WinPaint.SubpixelText = false;
                    break;
            }
            SKRect TextBounds = new SKRect();
            CharW = 0;
            CharH = 0;
            int[] MeasureChars = new int[] { 'A', 'X', 'T', 'Y' };
            for (int I = 0; I < MeasureChars.Length; I++)
            {
                WinPaint.MeasureText(((char)MeasureChars[I]).ToString(), ref TextBounds);
                CharW = CharW + TextBounds.Width;
                CharH = CharH + TextBounds.Height;
            }
            CharW = CharW / (float)(MeasureChars.Length);
            CharH = CharH / (float)(MeasureChars.Length);
        }

        public void DrawText(float X, float Y, float W, float H, float ScaleH, float ScaleV, string T, byte R, byte G, byte B)
        {
            try
            {
                X = 0 - (X / ScaleH);
                Y = 0 - (Y / ScaleV);
                W = W / ScaleH;
                H = H / ScaleV;

                SKBitmap ToBitmapBmpX = ToBitmapX();
                WinPaint.Color = new SKColor(R, G, B);
                SKCanvas BmpG = new SKCanvas(ToBitmapBmpX);
                BmpG.Scale(ScaleH, ScaleV, 0, 0);

                float X_ = (W) / 2.0f;
                float Y_ = H - ((H - CharH) / 2.0f);
                BmpG.DrawText(T, X + X_, Y + Y_, WinPaint);

                System.Runtime.InteropServices.Marshal.Copy(ToBitmapBmpX.GetPixels(), Data, 0, DataLength); 
                ToBitmapChanged = true;
            }
            catch (Exception E)
            {
            }
        }



        private SKTypeface WinFontFace;
        private SKFont WinFont;
        private SKPaint WinPaint;
        float CharW = 0;
        float CharH = 0;
        private WriteableBitmap ToBitmapBmp;
    }
}
