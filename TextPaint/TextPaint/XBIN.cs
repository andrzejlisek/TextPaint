using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace TextPaint
{
    public class XBIN
    {
        public XBIN(bool B, bool F)
        {
            DarkBackground = B;
            DarkForeground = F;
        }

        Encoding BinEncI;
        Encoding BinEncO;
        bool DarkBackground = false;
        bool DarkForeground = false;

        int ByteToNum(byte B)
        {
            return (int)(BinEncI.GetString(new byte[] { B })[0]);
        }

        public void RenderXBIN(string SrcFile, string DstFile, bool IsXBIN, string EncodingR, string EncodingW)
        {
            BinEncI = TextWork.EncodingFromName(EncodingR);
            BinEncO = TextWork.EncodingFromName(EncodingW);

            FileStream FSI = new FileStream(SrcFile, FileMode.Open, FileAccess.Read);
            BinaryReader FSB = new BinaryReader(FSI);

            int FileW = 65536;
            int FileH = 65536;

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
                    Bitmap FontBmp = new Bitmap(8 * 256, TwoFonts ? (FileFont + FileFont) : FileFont, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    int P = 0;
                    for (int i = 0; i < 256; i++)
                    {
                        for (int ii = 0; ii < FileFont; ii++)
                        {
                            FontBmp.SetPixel(i * 8 + 0, ii, ((FileFontArray[P] & 0x80) > 0) ? Color.White : Color.Black);
                            FontBmp.SetPixel(i * 8 + 1, ii, ((FileFontArray[P] & 0x40) > 0) ? Color.White : Color.Black);
                            FontBmp.SetPixel(i * 8 + 2, ii, ((FileFontArray[P] & 0x20) > 0) ? Color.White : Color.Black);
                            FontBmp.SetPixel(i * 8 + 3, ii, ((FileFontArray[P] & 0x10) > 0) ? Color.White : Color.Black);
                            FontBmp.SetPixel(i * 8 + 4, ii, ((FileFontArray[P] & 0x08) > 0) ? Color.White : Color.Black);
                            FontBmp.SetPixel(i * 8 + 5, ii, ((FileFontArray[P] & 0x04) > 0) ? Color.White : Color.Black);
                            FontBmp.SetPixel(i * 8 + 6, ii, ((FileFontArray[P] & 0x02) > 0) ? Color.White : Color.Black);
                            FontBmp.SetPixel(i * 8 + 7, ii, ((FileFontArray[P] & 0x01) > 0) ? Color.White : Color.Black);
                            if (TwoFonts)
                            {
                                FontBmp.SetPixel(i * 8 + 0, ii + FileFont, ((FileFontArray[P + 256] & 0x80) > 0) ? Color.White : Color.Black);
                                FontBmp.SetPixel(i * 8 + 1, ii + FileFont, ((FileFontArray[P + 256] & 0x40) > 0) ? Color.White : Color.Black);
                                FontBmp.SetPixel(i * 8 + 2, ii + FileFont, ((FileFontArray[P + 256] & 0x20) > 0) ? Color.White : Color.Black);
                                FontBmp.SetPixel(i * 8 + 3, ii + FileFont, ((FileFontArray[P + 256] & 0x10) > 0) ? Color.White : Color.Black);
                                FontBmp.SetPixel(i * 8 + 4, ii + FileFont, ((FileFontArray[P + 256] & 0x08) > 0) ? Color.White : Color.Black);
                                FontBmp.SetPixel(i * 8 + 5, ii + FileFont, ((FileFontArray[P + 256] & 0x04) > 0) ? Color.White : Color.Black);
                                FontBmp.SetPixel(i * 8 + 6, ii + FileFont, ((FileFontArray[P + 256] & 0x02) > 0) ? Color.White : Color.Black);
                                FontBmp.SetPixel(i * 8 + 7, ii + FileFont, ((FileFontArray[P + 256] & 0x01) > 0) ? Color.White : Color.Black);
                            }
                            P++;
                        }
                    }
                    //FontBmp.Save(FileO + "_test.png");

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
                    Bitmap FontBmp1 = new Bitmap(256 * 8 + 16, FontPage.Count * FileFont, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    Bitmap FontBmp2 = new Bitmap(256 * 8 + 16, FontPage.Count * FileFont, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    Graphics FontBmp1_ = Graphics.FromImage(FontBmp1);
                    Graphics FontBmp2_ = Graphics.FromImage(FontBmp2);
                    FontBmp1_.FillRectangle(new SolidBrush(Color.FromArgb(64, 64, 64)), 0, 0, FontBmp1.Width, FontBmp1.Height);
                    FontBmp2_.FillRectangle(new SolidBrush(Color.FromArgb(64, 64, 64)), 0, 0, FontBmp2.Width, FontBmp2.Height);
                    P = 0;
                    for (int i = 0; i < FontPage.Count; i++)
                    {
                        for (int ii = 0; ii < FileFont; ii++)
                        {
                            FontBmp1.SetPixel(0, P, ((FontPage[i] & 0x8000) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(1, P, ((FontPage[i] & 0x4000) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(2, P, ((FontPage[i] & 0x2000) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(3, P, ((FontPage[i] & 0x1000) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(4, P, ((FontPage[i] & 0x0800) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(5, P, ((FontPage[i] & 0x0400) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(6, P, ((FontPage[i] & 0x0200) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(7, P, ((FontPage[i] & 0x0100) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(8, P, ((FontPage[i] & 0x0080) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(9, P, ((FontPage[i] & 0x0040) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(10, P, ((FontPage[i] & 0x0020) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(11, P, ((FontPage[i] & 0x0010) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(12, P, ((FontPage[i] & 0x0008) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(13, P, ((FontPage[i] & 0x0004) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(14, P, ((FontPage[i] & 0x0002) > 0) ? Color.White : Color.Black);
                            FontBmp1.SetPixel(15, P, ((FontPage[i] & 0x0001) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(0, P, ((FontPage[i] & 0x8000) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(1, P, ((FontPage[i] & 0x4000) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(2, P, ((FontPage[i] & 0x2000) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(3, P, ((FontPage[i] & 0x1000) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(4, P, ((FontPage[i] & 0x0800) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(5, P, ((FontPage[i] & 0x0400) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(6, P, ((FontPage[i] & 0x0200) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(7, P, ((FontPage[i] & 0x0100) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(8, P, ((FontPage[i] & 0x0080) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(9, P, ((FontPage[i] & 0x0040) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(10, P, ((FontPage[i] & 0x0020) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(11, P, ((FontPage[i] & 0x0010) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(12, P, ((FontPage[i] & 0x0008) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(13, P, ((FontPage[i] & 0x0004) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(14, P, ((FontPage[i] & 0x0002) > 0) ? Color.White : Color.Black);
                            FontBmp2.SetPixel(15, P, ((FontPage[i] & 0x0001) > 0) ? Color.White : Color.Black);
                            P++;
                        }
                    }
                    for (int i = 0; i < 256; i++)
                    {
                        int CharCode = ByteToNum((byte)i);
                        int i_X = CharCode % 256;
                        int i_Y = FontPage.IndexOf(CharCode / 256);
                        Bitmap FontGlyph = new Bitmap(8, FileFont, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        Graphics.FromImage(FontGlyph).DrawImage(FontBmp, -8 * i, 0);
                        FontBmp1_.DrawImage(FontGlyph, i_X * 8 + 16, i_Y * FileFont);
                        if (TwoFonts)
                        {
                            FontGlyph = new Bitmap(8, FileFont, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                            Graphics.FromImage(FontGlyph).DrawImage(FontBmp, -8 * i, 0 - FileFont);
                            FontBmp2_.DrawImage(FontGlyph, i_X * 8 + 16, i_Y * FileFont);
                        }
                    }
                    FontBmp1.Save(DstFile + ".png");
                    Console.WriteLine("Primary font saved to " + DstFile + ".png");
                    if (TwoFonts)
                    {
                        FontBmp2.Save(DstFile + "_.png");
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
            if (EncodingW != "")
            {
                FileTextW = new StreamWriter(FileText, BinEncO);
            }
            else
            {
                FileTextW = new StreamWriter(FileText);
            }

            int LastB = -1;
            int LastF = -1;
            for (int i = 0; i < DataLength; i += 2)
            {
                if (DataRaw.Length <= (i + 1))
                {
                    break;
                }
                int NewB = DataRaw[i + 1] & 0xF0;
                int NewF = DataRaw[i + 1] & 0x0F;
                if (DarkBackground) { NewB = NewB & 0x70; }
                if (DarkForeground) { NewB = NewB & 0x07; }

                if (NewB != LastB)
                {
                    FileTextW.Write((char)27);
                    switch (NewB)
                    {
                        case 0x00: FileTextW.Write("[40m"); break;
                        case 0x40: FileTextW.Write("[41m"); break;
                        case 0x20: FileTextW.Write("[42m"); break;
                        case 0x60: FileTextW.Write("[43m"); break;
                        case 0x10: FileTextW.Write("[44m"); break;
                        case 0x50: FileTextW.Write("[45m"); break;
                        case 0x30: FileTextW.Write("[46m"); break;
                        case 0x70: FileTextW.Write("[47m"); break;
                        case 0x80: FileTextW.Write("[100m"); break;
                        case 0xC0: FileTextW.Write("[101m"); break;
                        case 0xA0: FileTextW.Write("[102m"); break;
                        case 0xE0: FileTextW.Write("[103m"); break;
                        case 0x90: FileTextW.Write("[104m"); break;
                        case 0xD0: FileTextW.Write("[105m"); break;
                        case 0xB0: FileTextW.Write("[106m"); break;
                        case 0xF0: FileTextW.Write("[107m"); break;
                    }
                    LastB = NewB;
                }

                if (NewF != LastF)
                {
                    FileTextW.Write((char)27);
                    switch (NewF)
                    {
                        case 0x00: FileTextW.Write("[30m"); break;
                        case 0x04: FileTextW.Write("[31m"); break;
                        case 0x02: FileTextW.Write("[32m"); break;
                        case 0x06: FileTextW.Write("[33m"); break;
                        case 0x01: FileTextW.Write("[34m"); break;
                        case 0x05: FileTextW.Write("[35m"); break;
                        case 0x03: FileTextW.Write("[36m"); break;
                        case 0x07: FileTextW.Write("[37m"); break;
                        case 0x08: FileTextW.Write("[90m"); break;
                        case 0x0C: FileTextW.Write("[91m"); break;
                        case 0x0A: FileTextW.Write("[92m"); break;
                        case 0x0E: FileTextW.Write("[93m"); break;
                        case 0x09: FileTextW.Write("[94m"); break;
                        case 0x0D: FileTextW.Write("[95m"); break;
                        case 0x0B: FileTextW.Write("[96m"); break;
                        case 0x0F: FileTextW.Write("[97m"); break;
                    }
                    LastF = NewF;
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

            FileTextW.Close();
            FileText.Close();
            Console.WriteLine("Text saved to " + DstFile + ".ans");
            if (IsXBIN)
            {
                Console.WriteLine("Text size: " + FileW + "x" + FileH);
            }

            FSB.Close();
            FSI.Close();

        }
    }
}
