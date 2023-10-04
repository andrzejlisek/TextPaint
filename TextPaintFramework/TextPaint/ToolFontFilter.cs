using System;
using System.Collections.Generic;
using System.IO;

namespace TextPaint
{
    public class ToolFontFilter : Tool
    {
        public ToolFontFilter(ConfigFile CF_) : base(CF_)
        {
        }

        void DrawMarker(LowLevelBitmap BmpX, int X, int Y)
        {
            if ((X < 0) || (Y < 0) || (X >= BmpX.Width) || (Y >= BmpX.Height))
            {
                return;
            }
            BmpX.SetPixelGray(X, Y, 255);
            if (X > 0) { BmpX.SetPixelGray(X - 1, Y, 0); }
            if (X < (BmpX.Width - 1)) { BmpX.SetPixelGray(X + 1, Y, 0); }
            if (Y > 0) { BmpX.SetPixelGray(X, Y - 1, 0); }
            if (Y < (BmpX.Height - 1)) { BmpX.SetPixelGray(X, Y + 1, 0); }
        }

        public override void Start()
        {
            int X = CF.ParamGetI("FrameX");
            int Y = CF.ParamGetI("FrameY");
            int CharW = CF.ParamGetI("CellW");
            int CharH = CF.ParamGetI("CellH");
            string Src = CF.ParamGetS("RawDirectory");
            string Dst = CF.ParamGetS("FilteredDirectory");
            int FrameMin = CF.ParamGetI("FrameFirst");
            int FrameMax = CF.ParamGetI("FrameLast");
            bool OnlyMinMax = CF.ParamGetB("FrameTest");

            int CharW2 = CF.ParamGetI("CellX");
            int CharH2 = CF.ParamGetI("CellY");

            if (!Directory.Exists(Dst))
            {
                Directory.CreateDirectory(Dst);
            }



            List<string> FileList = new List<string>();
            string FileDir = Src;
            string[] TempList = Directory.GetFiles(FileDir, "*", SearchOption.TopDirectoryOnly);
            FileList.AddRange(TempList);
            FileList.Sort();


            if (FrameMin < 0) { FrameMin = 0; }
            if (FrameMax < 0) { FrameMax = 0; }

            if (FrameMin >= FileList.Count) FrameMin = FileList.Count - 1;
            if (FrameMax >= FileList.Count) FrameMax = FileList.Count - 1;

            Console.WriteLine("Number of frames: " + FileList.Count);
            bool LastState = false;
            LowLevelBitmap LastBitmap = null;
            for (int i = FrameMin; i <= FrameMax; i++)
            {
                GC.Collect(2, GCCollectionMode.Forced);

                Console.WriteLine("Frame: " + i + "  First:" + FrameMin + "  Last:" + FrameMax);
                LowLevelBitmap BmpX = new LowLevelBitmap(CharW * 80, CharH * 24, 0);

                LowLevelBitmap BmpFile = new LowLevelBitmap(FileList[i]);

                BmpX.DrawImage(BmpFile, X, Y, 0, 0, BmpX.Width, BmpX.Height);

                if (OnlyMinMax)
                {
                    DrawMarker(BmpX, CharW * 24, CharH2);

                    DrawMarker(BmpX, CharW * 21 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 20 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 19 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 18 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 17 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 16 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 15 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 14 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 13 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 12 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 11 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 10 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 9 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 8 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 7 + CharW2, CharH2);
                    DrawMarker(BmpX, CharW * 6 + CharW2, CharH2);
                    if (i == FrameMin)
                    {
                        BmpX.SaveToFile(Path.Combine(Dst, "Test1.png"));
                    }
                    if (i == ((FrameMin + FrameMax) / 2))
                    {
                        BmpX.SaveToFile(Path.Combine(Dst, "Test2.png"));
                    }
                    if (i == FrameMax)
                    {
                        BmpX.SaveToFile(Path.Combine(Dst, "Test3.png"));
                    }
                }
                else
                {
                    int Level = 128;
                    if (LastState != (BmpX.GetPixelLevel(CharW * 24, CharH2) >= Level))
                    {
                        int Page = 0;
                        if ((BmpX.GetPixelLevel(CharW * 21 + CharW2, CharH2)) >= Level) Page += 1;
                        if ((BmpX.GetPixelLevel(CharW * 20 + CharW2, CharH2)) >= Level) Page += 2;
                        if ((BmpX.GetPixelLevel(CharW * 19 + CharW2, CharH2)) >= Level) Page += 4;
                        if ((BmpX.GetPixelLevel(CharW * 18 + CharW2, CharH2)) >= Level) Page += 8;
                        if ((BmpX.GetPixelLevel(CharW * 17 + CharW2, CharH2)) >= Level) Page += 16;
                        if ((BmpX.GetPixelLevel(CharW * 16 + CharW2, CharH2)) >= Level) Page += 32;
                        if ((BmpX.GetPixelLevel(CharW * 15 + CharW2, CharH2)) >= Level) Page += 64;
                        if ((BmpX.GetPixelLevel(CharW * 14 + CharW2, CharH2)) >= Level) Page += 128;
                        if ((BmpX.GetPixelLevel(CharW * 13 + CharW2, CharH2)) >= Level) Page += 256;
                        if ((BmpX.GetPixelLevel(CharW * 12 + CharW2, CharH2)) >= Level) Page += 512;
                        if ((BmpX.GetPixelLevel(CharW * 11 + CharW2, CharH2)) >= Level) Page += 1024;
                        if ((BmpX.GetPixelLevel(CharW * 10 + CharW2, CharH2)) >= Level) Page += 2048;
                        if ((BmpX.GetPixelLevel(CharW * 9 + CharW2, CharH2)) >= Level) Page += 4096;
                        if ((BmpX.GetPixelLevel(CharW * 8 + CharW2, CharH2)) >= Level) Page += 8192;
                        if ((BmpX.GetPixelLevel(CharW * 7 + CharW2, CharH2)) >= Level) Page += 16384;
                        if ((BmpX.GetPixelLevel(CharW * 6 + CharW2, CharH2)) >= Level) Page += 32768;
                        if (LastState)
                        {
                            bool IsTheSame = true;
                            for (int YY = (1 * CharH); YY < (24 * CharH); YY++)
                            {
                                for (int XX = 0; XX < (80 * CharW); XX++)
                                {
                                    if ((BmpX.GetPixelLevel(XX, YY) >= Level) != (LastBitmap.GetPixelLevel(XX, YY) >= Level))
                                    {
                                        IsTheSame = false;
                                    }
                                }
                            }
                            if (!IsTheSame)
                            {
                                LastBitmap.SaveToFile(Dst + Page.ToString().PadLeft(4, '0') + "_difference_" + i.ToString() + ".png");
                            }
                        }
                        else
                        {
                            LastBitmap = BmpX;
                            BmpX.SaveToFile(Dst + Page.ToString().PadLeft(4, '0') + ".png");
                            Console.WriteLine("Page: " + Page);
                        }

                        LastState = (BmpX.GetPixelLevel(CharW * 24, CharH2) >= Level);
                    }
                }

                if (OnlyMinMax)
                {
                    if (i == FrameMin)
                    {
                        i = (((FrameMin + FrameMax) / 2) - 1);
                    }
                    else
                    {
                        if (i == ((FrameMin + FrameMax) / 2))
                        {
                            i = FrameMax - 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
