using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextPaint
{
    public class XBIN
    {
        public XBIN()
        {
        }

        Encoding BinEncI;
        Encoding BinEncO;

        int ByteToNum(byte B)
        {
            return (int)(BinEncI.GetString(new byte[] { B })[0]);
        }

        public void RenderXBIN(string SrcFile, string DstFile, bool IsXBIN, string EncodingR, string EncodingW)
        {
            BinEncI = TextWork.EncodingFromName(EncodingR);
            BinEncO = TextWork.EncodingFromName(EncodingW);

            AnsiSauce AnsiSauce_ = new AnsiSauce();

            try
            {
                FileStream FSI = new FileStream(SrcFile, FileMode.Open, FileAccess.Read);
                byte[] FileRaw = new byte[FSI.Length];
                FSI.Read(FileRaw, 0, (int)FSI.Length);
                AnsiSauce_.LoadRaw(FileRaw);
                FSI.Close();

                FSI = new FileStream(SrcFile, FileMode.Open, FileAccess.Read);
                BinaryReader FSB = new BinaryReader(FSI);

                int FileW = 65536;
                int FileH = 65536;

                Core.SaveFileDirectory(DstFile + ".xxx");

                bool DataCompression = false;
                if (IsXBIN)
                {
                    byte[] FileHeader = FSB.ReadBytes(11);

                    FileW = ((int)FileHeader[6] * 256) + (int)FileHeader[5];
                    FileH = ((int)FileHeader[8] * 256) + (int)FileHeader[7];
                    int FileFont = (int)FileHeader[9];
                    int FileFlag = (int)FileHeader[10];

                    DataCompression = ((FileFlag & 4) != 0);

                    // Palette
                    if ((FileFlag & 1) != 0)
                    {
                        byte[] FilePalette = FSB.ReadBytes(48);
                        string S1 = "WinPaletteR=";
                        string S2 = "WinPaletteG=";
                        string S3 = "WinPaletteB=";
                        int[] PalIdx = { 0x0, 0x4, 0x2, 0x6, 0x1, 0x5, 0x3, 0x7, 0x8, 0xC, 0xA, 0xE, 0x9, 0xD, 0xB, 0xF };
                        for (int i = 0; i < 16; i++)
                        {
                            S1 = S1 + (((int)FilePalette[PalIdx[i] * 3 + 0]) * 4).ToString("X").PadLeft(2, '0');
                            S2 = S2 + (((int)FilePalette[PalIdx[i] * 3 + 1]) * 4).ToString("X").PadLeft(2, '0');
                            S3 = S3 + (((int)FilePalette[PalIdx[i] * 3 + 2]) * 4).ToString("X").PadLeft(2, '0');
                        }
                        FileStream FS = new FileStream(DstFile + ".txt", FileMode.Create, FileAccess.Write);
                        StreamWriter FSW = new StreamWriter(FS);
                        FSW.WriteLine(S1);
                        FSW.WriteLine(S2);
                        FSW.WriteLine(S3);
                        FSW.Close();
                        FS.Close();
                        Console.WriteLine("Palette saved to " + DstFile + ".txt");
                    }
                    else
                    {
                        Console.WriteLine("Palette does not exist");
                    }

                    // Font
                    if ((FileFlag & 2) != 0)
                    {
                        int FileFontSize = 256 * FileFont;
                        bool TwoFonts = ((FileFlag & 16) != 0);
                        if (TwoFonts)
                        {
                            FileFontSize = 512 * FileFont;
                        }
                        byte[] FileFontArray = FSB.ReadBytes(FileFontSize);
                        LowLevelBitmap FontBmp = new LowLevelBitmap(8 * 256, TwoFonts ? (FileFont + FileFont) : FileFont, 0);
                        int P = 0;
                        for (int i = 0; i < 256; i++)
                        {
                            for (int ii = 0; ii < FileFont; ii++)
                            {
                                FontBmp.SetPixel(i * 8 + 0, ii, ((FileFontArray[P] & 0x80) > 0) ? (byte)255 : (byte)0);
                                FontBmp.SetPixel(i * 8 + 1, ii, ((FileFontArray[P] & 0x40) > 0) ? (byte)255 : (byte)0);
                                FontBmp.SetPixel(i * 8 + 2, ii, ((FileFontArray[P] & 0x20) > 0) ? (byte)255 : (byte)0);
                                FontBmp.SetPixel(i * 8 + 3, ii, ((FileFontArray[P] & 0x10) > 0) ? (byte)255 : (byte)0);
                                FontBmp.SetPixel(i * 8 + 4, ii, ((FileFontArray[P] & 0x08) > 0) ? (byte)255 : (byte)0);
                                FontBmp.SetPixel(i * 8 + 5, ii, ((FileFontArray[P] & 0x04) > 0) ? (byte)255 : (byte)0);
                                FontBmp.SetPixel(i * 8 + 6, ii, ((FileFontArray[P] & 0x02) > 0) ? (byte)255 : (byte)0);
                                FontBmp.SetPixel(i * 8 + 7, ii, ((FileFontArray[P] & 0x01) > 0) ? (byte)255 : (byte)0);
                                if (TwoFonts)
                                {
                                    FontBmp.SetPixel(i * 8 + 0, ii + FileFont, ((FileFontArray[P + 256] & 0x80) > 0) ? (byte)255 : (byte)0);
                                    FontBmp.SetPixel(i * 8 + 1, ii + FileFont, ((FileFontArray[P + 256] & 0x40) > 0) ? (byte)255 : (byte)0);
                                    FontBmp.SetPixel(i * 8 + 2, ii + FileFont, ((FileFontArray[P + 256] & 0x20) > 0) ? (byte)255 : (byte)0);
                                    FontBmp.SetPixel(i * 8 + 3, ii + FileFont, ((FileFontArray[P + 256] & 0x10) > 0) ? (byte)255 : (byte)0);
                                    FontBmp.SetPixel(i * 8 + 4, ii + FileFont, ((FileFontArray[P + 256] & 0x08) > 0) ? (byte)255 : (byte)0);
                                    FontBmp.SetPixel(i * 8 + 5, ii + FileFont, ((FileFontArray[P + 256] & 0x04) > 0) ? (byte)255 : (byte)0);
                                    FontBmp.SetPixel(i * 8 + 6, ii + FileFont, ((FileFontArray[P + 256] & 0x02) > 0) ? (byte)255 : (byte)0);
                                    FontBmp.SetPixel(i * 8 + 7, ii + FileFont, ((FileFontArray[P + 256] & 0x01) > 0) ? (byte)255 : (byte)0);
                                }
                                P++;
                            }
                        }
                        //FontBmp.SaveToFile(FileO + "_test.png");

                        List<int> FontPage = new List<int>();
                        for (int i = 0; i < 256; i++)
                        {
                            int Num = ByteToNum((byte)i) / 256;
                            if (!FontPage.Contains(Num))
                            {
                                FontPage.Add(Num);
                            }
                        }
                        FontPage.Sort();
                        LowLevelBitmap FontBmp1 = new LowLevelBitmap(256 * 8 + 16, FontPage.Count * FileFont, 0);
                        LowLevelBitmap FontBmp2 = new LowLevelBitmap(256 * 8 + 16, FontPage.Count * FileFont, 0);
                        FontBmp1.DrawRectangle(0, 0, FontBmp1.Width, FontBmp1.Height, 64, 64, 64);
                        FontBmp2.DrawRectangle(0, 0, FontBmp2.Width, FontBmp2.Height, 64, 64, 64);
                        P = 0;
                        for (int i = 0; i < FontPage.Count; i++)
                        {
                            for (int ii = 0; ii < FileFont; ii++)
                            {
                                FontBmp1.SetPixel(0, P, ((FontPage[i] & 0x8000) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(1, P, ((FontPage[i] & 0x4000) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(2, P, ((FontPage[i] & 0x2000) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(3, P, ((FontPage[i] & 0x1000) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(4, P, ((FontPage[i] & 0x0800) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(5, P, ((FontPage[i] & 0x0400) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(6, P, ((FontPage[i] & 0x0200) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(7, P, ((FontPage[i] & 0x0100) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(8, P, ((FontPage[i] & 0x0080) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(9, P, ((FontPage[i] & 0x0040) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(10, P, ((FontPage[i] & 0x0020) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(11, P, ((FontPage[i] & 0x0010) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(12, P, ((FontPage[i] & 0x0008) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(13, P, ((FontPage[i] & 0x0004) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(14, P, ((FontPage[i] & 0x0002) > 0) ? (byte)255 : (byte)0);
                                FontBmp1.SetPixel(15, P, ((FontPage[i] & 0x0001) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(0, P, ((FontPage[i] & 0x8000) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(1, P, ((FontPage[i] & 0x4000) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(2, P, ((FontPage[i] & 0x2000) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(3, P, ((FontPage[i] & 0x1000) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(4, P, ((FontPage[i] & 0x0800) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(5, P, ((FontPage[i] & 0x0400) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(6, P, ((FontPage[i] & 0x0200) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(7, P, ((FontPage[i] & 0x0100) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(8, P, ((FontPage[i] & 0x0080) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(9, P, ((FontPage[i] & 0x0040) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(10, P, ((FontPage[i] & 0x0020) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(11, P, ((FontPage[i] & 0x0010) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(12, P, ((FontPage[i] & 0x0008) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(13, P, ((FontPage[i] & 0x0004) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(14, P, ((FontPage[i] & 0x0002) > 0) ? (byte)255 : (byte)0);
                                FontBmp2.SetPixel(15, P, ((FontPage[i] & 0x0001) > 0) ? (byte)255 : (byte)0);
                                P++;
                            }
                        }
                        for (int i = 0; i < 256; i++)
                        {
                            int CharCode = ByteToNum((byte)i);
                            int i_X = CharCode % 256;
                            int i_Y = FontPage.IndexOf(CharCode / 256);
                            FontBmp1.DrawImage(FontBmp, 8 * i, 0, i_X * 8 + 16, i_Y * FileFont, 8, FileFont);
                            if (TwoFonts)
                            {
                                FontBmp2.DrawImage(FontBmp, 8 * i, FileFont, i_X * 8 + 16, i_Y * FileFont, 8, FileFont);
                            }
                        }
                        FontBmp1.SaveToFile(DstFile + ".png");
                        Console.WriteLine("Primary font saved to " + DstFile + ".png");
                        if (TwoFonts)
                        {
                            FontBmp2.SaveToFile(DstFile + "_.png");
                            Console.WriteLine("Secondary font saved to " + DstFile + "_.png");
                        }
                        else
                        {
                            Console.WriteLine("Secondary font does not exist");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Primary font does not exist");
                        Console.WriteLine("Secondary font does not exist");
                    }
                }

                // Image data
                int DataLength = (int)(FSI.Length - FSI.Position);

                byte[] DataRaw = null;
                if (DataCompression)
                {
                    byte[] DataRaw_ = FSB.ReadBytes(DataLength);
                    DataRaw = new byte[FileH * FileW * 2];
                    int IdxI = 0;
                    int IdxO = 0;
                    while (IdxO < (FileH * FileW * 2))
                    {
                        int Rep = DataRaw_[IdxI] & 0x3F;
                        switch (DataRaw_[IdxI] & 0xC0)
                        {
                            case 0x00:
                                {
                                    IdxI++;
                                    while (Rep >= 0)
                                    {
                                        DataRaw[IdxO] = DataRaw_[IdxI];
                                        IdxI++;
                                        IdxO++;
                                        DataRaw[IdxO] = DataRaw_[IdxI];
                                        IdxI++;
                                        IdxO++;
                                        Rep--;
                                    }
                                }
                                break;
                            case 0x40:
                                {
                                    IdxI++;
                                    byte Temp = DataRaw_[IdxI];
                                    IdxI++;
                                    while (Rep >= 0)
                                    {
                                        DataRaw[IdxO] = Temp;
                                        IdxO++;
                                        DataRaw[IdxO] = DataRaw_[IdxI];
                                        IdxI++;
                                        IdxO++;
                                        Rep--;
                                    }
                                }
                                break;
                            case 0x80:
                                {
                                    IdxI++;
                                    byte Temp = DataRaw_[IdxI];
                                    IdxI++;
                                    while (Rep >= 0)
                                    {
                                        DataRaw[IdxO] = DataRaw_[IdxI];
                                        IdxI++;
                                        IdxO++;
                                        DataRaw[IdxO] = Temp;
                                        IdxO++;
                                        Rep--;
                                    }
                                }
                                break;
                            case 0xC0:
                                {
                                    IdxI++;
                                    byte Temp1 = DataRaw_[IdxI];
                                    IdxI++;
                                    byte Temp2 = DataRaw_[IdxI];
                                    IdxI++;
                                    while (Rep >= 0)
                                    {
                                        DataRaw[IdxO] = Temp1;
                                        IdxO++;
                                        DataRaw[IdxO] = Temp2;
                                        IdxO++;
                                        Rep--;
                                    }
                                }
                                break;
                        }
                    }
                    DataLength = FileW * FileH * 2;
                }
                else
                {
                    if (IsXBIN)
                    {
                        DataLength = FileW * FileH * 2;
                    }
                    DataRaw = FSB.ReadBytes(DataLength);
                }

                FileStream FileText = new FileStream(DstFile + ".ans", FileMode.Create, FileAccess.Write);
                StreamWriter FileTextW;
                if ("".Equals(EncodingW))
                {
                    FileTextW = new StreamWriter(FileText);
                }
                else
                {
                    FileTextW = new StreamWriter(FileText, BinEncO);
                }

                char ESC = (char)27;
                FileTextW.Write(ESC + "[0m");
                int LastB = -1;
                int LastF = -1;
                bool LastBI = false;
                bool LastFI = false;
                for (int i = 0; i < DataLength; i += 2)
                {
                    if (DataRaw.Length <= (i + 1))
                    {
                        break;
                    }
                    int NewB = DataRaw[i + 1] & 0x70;
                    int NewF = DataRaw[i + 1] & 0x07;
                    bool NewBI = ((DataRaw[i + 1] & 0x80) != 0);
                    bool NewFI = ((DataRaw[i + 1] & 0x08) != 0);

                    if ((NewB != LastB) || (NewF != LastF) || (NewBI != LastBI) || (NewFI != LastFI))
                    {
                        bool AttrNum = false;
                        FileTextW.Write(ESC + "[");

                        if (LastBI != NewBI)
                        {
                            if (AttrNum) FileTextW.Write(";");

                            if (NewBI)
                            {
                                FileTextW.Write("5");
                            }
                            else
                            {
                                FileTextW.Write("25");
                            }

                            LastBI = NewBI;
                            AttrNum = true;
                        }

                        if (LastB != NewB)
                        {
                            if (AttrNum) FileTextW.Write(";");

                            switch (NewB)
                            {
                                case 0x00: FileTextW.Write("40"); break;
                                case 0x40: FileTextW.Write("41"); break;
                                case 0x20: FileTextW.Write("42"); break;
                                case 0x60: FileTextW.Write("43"); break;
                                case 0x10: FileTextW.Write("44"); break;
                                case 0x50: FileTextW.Write("45"); break;
                                case 0x30: FileTextW.Write("46"); break;
                                case 0x70: FileTextW.Write("47"); break;
                                case 0x80: FileTextW.Write("100"); break;
                                case 0xC0: FileTextW.Write("101"); break;
                                case 0xA0: FileTextW.Write("102"); break;
                                case 0xE0: FileTextW.Write("103"); break;
                                case 0x90: FileTextW.Write("104"); break;
                                case 0xD0: FileTextW.Write("105"); break;
                                case 0xB0: FileTextW.Write("106"); break;
                                case 0xF0: FileTextW.Write("107"); break;
                            }

                            LastB = NewB;
                            AttrNum = true;
                        }


                        if (LastFI != NewFI)
                        {
                            if (AttrNum) FileTextW.Write(";");

                            if (NewFI)
                            {
                                FileTextW.Write("1");
                            }
                            else
                            {
                                FileTextW.Write("22");
                            }

                            LastFI = NewFI;
                            AttrNum = true;
                        }

                        if (LastF != NewF)
                        {
                            if (AttrNum) FileTextW.Write(";");

                            switch (NewF)
                            {
                                case 0x00: FileTextW.Write("30"); break;
                                case 0x04: FileTextW.Write("31"); break;
                                case 0x02: FileTextW.Write("32"); break;
                                case 0x06: FileTextW.Write("33"); break;
                                case 0x01: FileTextW.Write("34"); break;
                                case 0x05: FileTextW.Write("35"); break;
                                case 0x03: FileTextW.Write("36"); break;
                                case 0x07: FileTextW.Write("37"); break;
                                case 0x08: FileTextW.Write("90"); break;
                                case 0x0C: FileTextW.Write("91"); break;
                                case 0x0A: FileTextW.Write("92"); break;
                                case 0x0E: FileTextW.Write("93"); break;
                                case 0x09: FileTextW.Write("94"); break;
                                case 0x0D: FileTextW.Write("95"); break;
                                case 0x0B: FileTextW.Write("96"); break;
                                case 0x0F: FileTextW.Write("97"); break;
                            }

                            LastF = NewF;
                            AttrNum = true;
                        }

                        FileTextW.Write("m");
                    }

                    if ((ByteToNum(DataRaw[i]) < 32))
                    {
                        FileTextW.Write(" ");
                    }
                    else
                    {
                        FileTextW.Write((char)ByteToNum(DataRaw[i]));
                    }
                }
                FileTextW.Write(ESC + "[0m");
                FileTextW.Close();
                FileText.Close();

                // Sauce info
                if (AnsiSauce_.Exists)
                {
                    FileText = new FileStream(DstFile + ".ans", FileMode.Append, FileAccess.Write);
                    BinaryWriter FileTextW2 = new BinaryWriter(FileText);

                    FileTextW2.Write((byte)26);
                    for (int i = 0; i < AnsiSauce_.InfoRaw.Length; i++)
                    {
                        FileTextW2.Write(AnsiSauce_.InfoRaw[i]);
                    }

                    FileTextW2.Close();
                    FileText.Close();
                }

                Console.WriteLine("Text saved to " + DstFile + ".ans");
                if (IsXBIN)
                {
                    Console.WriteLine("Text size: " + FileW + "x" + FileH);
                }

                FSB.Close();
                FSI.Close();
            }
            catch (Exception E)
            {
                Console.WriteLine("Render error: " + E.Message);
            }
        }
    }
}
