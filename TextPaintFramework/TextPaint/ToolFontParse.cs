using System;
using System.Collections.Generic;
using System.IO;

namespace TextPaint
{
    public class ToolFontParse : Tool
    {
        public ToolFontParse(ConfigFile CF) : base(CF)
        {
        }

        LowLevelBitmap BmpBlank = null;
        int Char0;
        int Char1;
        int Back0;
        int Back1;

        int CellType(LowLevelBitmap BmpX, int X, int Y)
        {

            int CharW = BmpBlank.Width;
            int CharH = BmpBlank.Height;
            X = X * CharW;
            Y = Y * CharH;

            bool GoodBack = true;
            bool IsBack = false;
            bool IsChar = false;
            for (int YY = 0; YY < CharH; YY++)
            {
                for (int XX = 0; XX < CharW; XX++)
                {
                    int C0 = BmpX.GetPixel(XX + X, YY + Y);
                    int C1 = BmpBlank.GetPixel(XX, YY);
                    if (C0 != C1)
                    {
                        GoodBack = false;
                    }
                    if ((C0 == Back0) || (C0 == Back1))
                    {
                        IsBack = true;
                    }
                    if ((C0 == Char0) || (C0 == Char1))
                    {
                        IsChar = true;
                    }
                }
            }

            // Background cell
            if ((IsBack) && (!IsChar) && GoodBack)
            {
                return 1;
            }

            // Character cell
            if ((!IsBack) && (IsChar))
            {
                return 2;
            }

            // Unknown or improper type
            return 0;
        }

        public override void Start()
        {
            string BitmapDir = CF.ParamGetS("FilteredDirectory");
            string HexFile = CF.ParamGetS("HexFile");
            int CharW = CF.ParamGetI("CellW");
            int CharH = CF.ParamGetI("CellH");
            List<int> BlankChars = new List<int>();
            string[] BlankCharsS = CF.ParamGetS("BlankChars").Split(',');
            for (int i = 0; i < BlankCharsS.Length; i++)
            {
                BlankChars.Add(TextWork.CodeChar(BlankCharsS[i]));
            }


            List<string> BlankChars_ = new List<string>();

            int CharW2 = CF.ParamGetI("CellX");
            int CharH2 = CF.ParamGetI("CellY");

            List<string> FileList = new List<string>();
            string[] TempList = Directory.GetFiles(BitmapDir, "*", SearchOption.TopDirectoryOnly);
            FileList.AddRange(TempList);
            FileList.Sort();

            int PageMin = 0;
            int PageMax = FileList.Count - 1;

            FileStream FS_ = new FileStream(HexFile, FileMode.Create, FileAccess.Write);
            StreamWriter FS = new StreamWriter(FS_);

            for (int i = PageMin; i <= PageMax; i++)
            {
                Console.WriteLine((i + 1) + "/" + FileList.Count + "  " + FileList[i]);
                {
                    LowLevelBitmap BmpX = new LowLevelBitmap(CharW * 80, CharH * 24, 0);

                    LowLevelBitmap BmpFile = new LowLevelBitmap(FileList[i]);
                    if ((BmpX.Width != BmpFile.Width) || (BmpX.Height != BmpFile.Height))
                    {
                        throw new Exception("Expected bitmap size: " + BmpX.Width + "x" + BmpX.Height);
                    }
                    BmpX.DrawImage(BmpFile, 0, 0, 0, 0, BmpFile.Width, BmpFile.Height);

                    // Aquire backgroung image and detect used colors
                    if (BmpBlank == null)
                    {
                        BmpBlank = new LowLevelBitmap(CharW, CharH, 0);
                        {
                            BmpBlank.DrawImage(BmpX, (CharW * 10), 0, 0, 0, CharW, CharH);
                            Char0 = BmpBlank.GetPixel(0, 0);
                            BmpBlank.DrawImage(BmpX, (CharW * 2), 0, 0, 0, CharW, CharH);
                            for (int YY = 0; YY < CharH; YY++)
                            {
                                for (int XX = 0; XX < CharW; XX++)
                                {
                                    Char1 = BmpBlank.GetPixel(XX, YY);
                                    if (Char0 != Char1)
                                    {
                                        YY = CharH;
                                        break;
                                    }
                                }
                            }
                            BmpBlank.DrawImage(BmpX, CharW, CharH, 0, 0, CharW, CharH);
                            Back0 = BmpBlank.GetPixel(0, 0);
                            for (int YY = 0; YY < CharH; YY++)
                            {
                                for (int XX = 0; XX < CharW; XX++)
                                {
                                    Back1 = BmpBlank.GetPixel(XX, YY);
                                    if (Back0 != Back1)
                                    {
                                        YY = CharH;
                                        break;
                                    }
                                }
                            }
                        }
                    }


                    // Read page number
                    int Page = 0;
                    int Level = 128;
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

                    bool BackgroundGood = true;

                    // Background in lines
                    for (int XX = 2; XX <= 77; XX++)
                    {
                        for (int YY = 1; YY < 24; YY += 2)
                        {
                            if (CellType(BmpX, XX, YY) != 1)
                            {
                                BackgroundGood = false;
                            }
                        }
                    }

                    // Background in columns
                    for (int XX = 2; XX <= 77; XX += 3)
                    {
                        for (int YY = 1; YY < 24; YY++)
                        {
                            if (CellType(BmpX, XX, YY) != 1)
                            {
                                BackgroundGood = false;
                            }
                        }
                    }

                    Console.WriteLine("Page " + TextWork.CharCode(Page, 2));

                    // Detecting characters
                    int PtrX = 3;
                    int PtrY = 2;
                    for (int ii = 0; ii < 256; ii++)
                    {
                        int CharCell0 = CellType(BmpX, PtrX, PtrY);
                        int CharCell1 = CellType(BmpX, PtrX + 1, PtrY);
                        int CharacterType = 0;

                        // Background
                        if ((CharCell0 == 1) && (CharCell1 == 1))
                        {
                            CharacterType = 1;
                        }

                        // Single character
                        if ((CharCell0 == 2) && (CharCell1 == 1))
                        {
                            CharacterType = 2;
                        }

                        // Double character
                        if ((CharCell0 == 2) && (CharCell1 == 2))
                        {
                            CharacterType = 3;
                        }

                        int CharVectorL = 0;
                        if (CharW <= 16) CharVectorL = 2;
                        if (CharW <= 8) CharVectorL = 1;
                        int[] CharVector = null;

                        int MaskStart = 128;
                        if ((CharW % 8) > 0)
                        {
                            MaskStart = MaskStart >>(8 - (CharW % 8));
                        }

                        switch (CharacterType)
                        {
                            case 0:
                                BackgroundGood = false;
                                Console.WriteLine("Invalid character: " + ii);
                                break;
                            case 1:
                                break;
                            case 2:
                                {
                                    CharVector = new int[CharVectorL * CharH];
                                    for (int iii = 0; iii < CharVector.Length; iii++)
                                    {
                                        CharVector[iii] = 0;
                                    }
                                    for (int YY = 0; YY < CharH; YY++)
                                    {
                                        int Mask = MaskStart;
                                        int XX_ = 0;
                                        for (int XX = 0; XX < CharW; XX++)
                                        {
                                            if (Char1 == BmpX.GetPixel(PtrX * CharW + XX, PtrY * CharH + YY))
                                            {
                                                CharVector[YY * CharVectorL + XX_] += Mask;
                                            }
                                            if (Mask == 1)
                                            {
                                                XX_++;
                                                Mask = 128;
                                            }
                                            else
                                            {
                                                Mask = Mask >> 1;
                                            }
                                        }
                                    }
                                }
                                break;
                            case 3:
                                {
                                    CharVector = new int[CharVectorL * CharH * 2];
                                    for (int iii = 0; iii < CharVector.Length; iii++)
                                    {
                                        CharVector[iii] = 0;
                                    }
                                    for (int YY = 0; YY < CharH; YY++)
                                    {
                                        int Mask = MaskStart;
                                        int XX_ = 0;
                                        for (int XX = 0; XX < (CharW * 2); XX++)
                                        {
                                            if (Char1 == BmpX.GetPixel(PtrX * CharW + XX, PtrY * CharH + YY))
                                            {
                                                CharVector[YY * CharVectorL * 2 + XX_] += Mask;
                                            }
                                            if (Mask == 1)
                                            {
                                                XX_++;
                                                Mask = 128;
                                            }
                                            else
                                            {
                                                Mask = Mask >> 1;
                                            }
                                        }
                                    }
                                }
                                break;
                        }

                        if (CharVector != null)
                        {
                            int CharCode = (ii + (Page << 8));
                            FS.Write(CharCode.ToString("X").PadLeft(4, '0') + ":");
                            for (int iii = 0; iii < CharVector.Length; iii++)
                            {
                                FS.Write(CharVector[iii].ToString("X").PadLeft(2, '0'));
                            }
                            FS.WriteLine();

                            if (BlankChars.Contains(CharCode))
                            {
                                string BlankChars__ = "";
                                for (int iii = 0; iii < CharVector.Length; iii++)
                                {
                                    BlankChars__ += (CharVector[iii].ToString("X").PadLeft(2, '0'));
                                }
                                BlankChars_.Add(BlankChars__);
                            }
                        }

                        PtrX += 3;
                        if (PtrX >= 78)
                        {
                            PtrX = 3;
                            PtrY += 2;
                        }

                        if (!BackgroundGood)
                        {
                            throw new Exception("");
                        }

                    }

                }
            }

            for (int i = 0; i < BlankChars_.Count; i++)
            {
                FS.Write(":");
                FS.Write(BlankChars_[i]);
                FS.WriteLine();
            }


            FS.Close();
            FS_.Close();

        }
    }
}
