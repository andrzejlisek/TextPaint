using System;
using System.Collections.Generic;
using System.IO;

namespace TextPaint
{
    public class ToolFontHex : Tool
    {
        LowLevelBitmap[] Hex = new LowLevelBitmap[16];

        int ColorX;
        int Color0;
        int Color1;

        public ToolFontHex(ConfigFile CF_) : base(CF_)
        {
            ColorX = LowLevelBitmap.ColorRgbToInt(64, 64, 64);
            Color0 = LowLevelBitmap.ColorRgbToInt(0, 0, 128);
            Color1 = LowLevelBitmap.ColorRgbToInt(255, 255, 128);
        }

        static public string FontName(int FontW, int FontH)
        {
            return FontW.ToString().PadLeft(2, '0') + "x" + FontH.ToString().PadLeft(2, '0') + ".png";
        }



        public List<int[]> CharsBlank;
        public List<List<int>> CharsBlankCode;
        public Dictionary<int, int[]> Chars;
        public Dictionary<int, int> CharC0;
        public Dictionary<int, int> CharC1;
        public List<int> CharDouble1 = new List<int>();
        public List<int> CharDouble2 = new List<int>();
        int FontSizeW = 0;
        int FontSizeH = 0;
        int FontSizeH2 = 0;
        int FontSizeH3 = 0;
        int FontSizeH4 = 0;

        int CharColor0;
        int CharColor1;

        bool ArrayEquals(int[] Array1, int[] Array2)
        {
            if (Array1.Length != Array2.Length)
            {
                return false;
            }
            for (int i = 0; i < Array1.Length; i++)
            {
                if (Array1[i] != Array2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void LoadFile(string FileName, bool LoadLast)
        {
            try
            {
                Console.WriteLine(FileName);
                FileStream FR = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                StreamReader SR = new StreamReader(FR);
                string Buf = SR.ReadLine();
                while (Buf != null)
                {
                    string[] Buf_ = Buf.Split(':');
                    if (Buf_.Length == 2)
                    {
                        int N = -1;
                        if (Buf_[0].Length > 0)
                        {
                            N = int.Parse(Buf_[0], System.Globalization.NumberStyles.HexNumber);
                        }
                        int ValsL = Buf_[1].Length / 2;
                        int[] Vals = new int[ValsL];
                        for (int i = 0; i < ValsL; i++)
                        {
                            Vals[i] = int.Parse(Buf_[1].Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                        }

                        if (N < 0)
                        {
                            CharsBlank.Add(Vals);
                            CharsBlankCode.Add(new List<int>());
                        }
                        else
                        {
                            if (Chars.ContainsKey(N))
                            {
                                if (!ArrayEquals(Chars[N], Vals))
                                {
                                    if (LoadLast)
                                    {
                                        Console.WriteLine("Duplicate character, used last: " + TextWork.CharCode(N, 1));
                                    }
                                    else
                                    {
                                        Console.WriteLine("Duplicate character, used first: " + TextWork.CharCode(N, 1));
                                    }
                                }
                                if (LoadLast)
                                {
                                    Chars[N] = Vals;
                                    CharC0[N] = CharColor0;
                                    CharC1[N] = CharColor1;
                                }
                            }
                            else
                            {
                                Chars.Add(N, Vals);
                                CharC0.Add(N, CharColor0);
                                CharC1.Add(N, CharColor1);
                            }
                        }
                    }
                    Buf = SR.ReadLine();
                }
                SR.Close();
                FR.Close();
                Console.WriteLine("OK");
            }
            catch
            {
                Console.WriteLine("ERROR");
            }
        }

        bool PaintCharLine(LowLevelBitmap BmpX_, int X, int Y, int[] Data, int DataIdx, bool DoubleChar, bool DoubleSize, int Color0_, int Color1_)
        {
            if (Data == null)
            {
                return false;
            }
            if (Data.Length == 0)
            {
                return false;
            }
            int DrawFontW = FontSizeW;
            if (DoubleChar)
            {
                DrawFontW = FontSizeW << 1;
            }

            int DrawOffset = 8 - (FontSizeW % 8);
            if ((FontSizeW % 8) == 0)
            {
                DrawOffset = 0;
            }
            int DrawOffset2 = DrawOffset + DrawOffset;
            bool DrawOffset_____ = true;

            int DrawFontW0 = DrawFontW + DrawOffset;
            int DrawFontW1 = DrawFontW - 8 + DrawOffset;
            int DrawFontW2 = DrawFontW - 16 + DrawOffset;
            int DrawFontW3 = DrawFontW - 24 + DrawOffset;
            int DrawFontW4 = DrawFontW - 32 + DrawOffset;
            int DrawFontW5 = DrawFontW - 40 + DrawOffset;
            int DrawFontW6 = DrawFontW - 48 + DrawOffset;
            int DrawFontW7 = DrawFontW - 56 + DrawOffset;
            int DataColumns = Data.Length / FontSizeH;
            if ((Data.Length % FontSizeH) > 0) DataColumns++;
            int BitMask = 128;
            if (DoubleSize)
            {
                DataColumns = DataColumns + 10;
            }
            switch (DataColumns)
            {
                case 1:
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + i + 0 - DrawOffset, Y, (Data[DataIdx * 1 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        BitMask = BitMask >> 1;
                    }
                    break;
                case 2:
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + i + 0 - DrawOffset, Y, (Data[DataIdx * 2 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 8 - DrawOffset, Y, (Data[DataIdx * 2 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        BitMask = BitMask >> 1;
                    }
                    break;
                case 3:
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + i + 0 - DrawOffset, Y, (Data[DataIdx * 3 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 8 - DrawOffset, Y, (Data[DataIdx * 3 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW2) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 16 - DrawOffset, Y, (Data[DataIdx * 3 + 2] & BitMask) > 0 ? Color1_ : Color0_);
                        BitMask = BitMask >> 1;
                    }
                    break;
                case 4:
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + i + 0 - DrawOffset, Y, (Data[DataIdx * 4 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 8 - DrawOffset, Y, (Data[DataIdx * 4 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW2) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 16 - DrawOffset, Y, (Data[DataIdx * 4 + 2] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW3) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 24 - DrawOffset, Y, (Data[DataIdx * 4 + 3] & BitMask) > 0 ? Color1_ : Color0_);
                        BitMask = BitMask >> 1;
                    }
                    break;
                case 5:
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + i + 0 - DrawOffset, Y, (Data[DataIdx * 5 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 8 - DrawOffset, Y, (Data[DataIdx * 5 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW2) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 16 - DrawOffset, Y, (Data[DataIdx * 5 + 2] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW3) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 24 - DrawOffset, Y, (Data[DataIdx * 5 + 3] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW4) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 32 - DrawOffset, Y, (Data[DataIdx * 5 + 4] & BitMask) > 0 ? Color1_ : Color0_);
                        BitMask = BitMask >> 1;
                    }
                    break;
                case 6:
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + i + 0 - DrawOffset, Y, (Data[DataIdx * 6 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 8 - DrawOffset, Y, (Data[DataIdx * 6 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW2) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 16 - DrawOffset, Y, (Data[DataIdx * 6 + 2] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW3) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 24 - DrawOffset, Y, (Data[DataIdx * 6 + 3] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW4) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 32 - DrawOffset, Y, (Data[DataIdx * 6 + 4] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW5) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 40 - DrawOffset, Y, (Data[DataIdx * 6 + 5] & BitMask) > 0 ? Color1_ : Color0_);
                        BitMask = BitMask >> 1;
                    }
                    break;
                case 7:
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + i + 0 - DrawOffset, Y, (Data[DataIdx * 7 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 8 - DrawOffset, Y, (Data[DataIdx * 7 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW2) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 16 - DrawOffset, Y, (Data[DataIdx * 7 + 2] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW3) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 24 - DrawOffset, Y, (Data[DataIdx * 7 + 3] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW4) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 32 - DrawOffset, Y, (Data[DataIdx * 7 + 4] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW5) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 40 - DrawOffset, Y, (Data[DataIdx * 7 + 5] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW6) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 48 - DrawOffset, Y, (Data[DataIdx * 7 + 6] & BitMask) > 0 ? Color1_ : Color0_);
                        BitMask = BitMask >> 1;
                    }
                    break;
                case 8:
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + i + 0 - DrawOffset, Y, (Data[DataIdx * 8 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 8 - DrawOffset, Y, (Data[DataIdx * 8 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW2) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 16 - DrawOffset, Y, (Data[DataIdx * 8 + 2] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW3) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 24 - DrawOffset, Y, (Data[DataIdx * 8 + 3] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW4) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 32 - DrawOffset, Y, (Data[DataIdx * 8 + 4] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW5) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 40 - DrawOffset, Y, (Data[DataIdx * 8 + 5] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW6) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 48 - DrawOffset, Y, (Data[DataIdx * 8 + 6] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW7) && (DrawOffset_____)) BmpX_.SetPixel(X + i + 56 - DrawOffset, Y, (Data[DataIdx * 8 + 7] & BitMask) > 0 ? Color1_ : Color0_);
                        BitMask = BitMask >> 1;
                    }
                    break;
                case 11:
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + (i * 2 + 0) + 0 - DrawOffset2, Y, (Data[DataIdx * 1 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + (i * 2 + 1) + 0 - DrawOffset2, Y, (Data[DataIdx * 1 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        BitMask = BitMask >> 1;
                    }
                    break;
                case 12:
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + (i * 2 + 0) + 0 - DrawOffset2, Y, (Data[DataIdx * 2 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + (i * 2 + 1) + 0 - DrawOffset2, Y, (Data[DataIdx * 2 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + (i * 2 + 0) + 16 - DrawOffset2, Y, (Data[DataIdx * 2 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + (i * 2 + 1) + 16 - DrawOffset2, Y, (Data[DataIdx * 2 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        BitMask = BitMask >> 1;
                    }
                    break;
                case 13:
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + (i * 2 + 0) + 0 - DrawOffset2, Y, (Data[DataIdx * 3 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + (i * 2 + 1) + 0 - DrawOffset2, Y, (Data[DataIdx * 3 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + (i * 2 + 0) + 16 - DrawOffset2, Y, (Data[DataIdx * 3 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + (i * 2 + 1) + 16 - DrawOffset2, Y, (Data[DataIdx * 3 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW2) && (DrawOffset_____)) BmpX_.SetPixel(X + (i * 2 + 0) + 32 - DrawOffset2, Y, (Data[DataIdx * 3 + 2] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW2) && (DrawOffset_____)) BmpX_.SetPixel(X + (i * 2 + 1) + 32 - DrawOffset2, Y, (Data[DataIdx * 3 + 2] & BitMask) > 0 ? Color1_ : Color0_);
                        BitMask = BitMask >> 1;
                    }
                    break;
                case 14:
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + (i * 2 + 0) + 0 - DrawOffset2, Y, (Data[DataIdx * 4 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW0) && (i >= DrawOffset)) BmpX_.SetPixel(X + (i * 2 + 1) + 0 - DrawOffset2, Y, (Data[DataIdx * 4 + 0] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + (i * 2 + 0) + 16 - DrawOffset2, Y, (Data[DataIdx * 4 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW1) && (DrawOffset_____)) BmpX_.SetPixel(X + (i * 2 + 1) + 16 - DrawOffset2, Y, (Data[DataIdx * 4 + 1] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW2) && (DrawOffset_____)) BmpX_.SetPixel(X + (i * 2 + 0) + 32 - DrawOffset2, Y, (Data[DataIdx * 4 + 2] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW2) && (DrawOffset_____)) BmpX_.SetPixel(X + (i * 2 + 1) + 32 - DrawOffset2, Y, (Data[DataIdx * 4 + 2] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW3) && (DrawOffset_____)) BmpX_.SetPixel(X + (i * 2 + 0) + 48 - DrawOffset2, Y, (Data[DataIdx * 4 + 3] & BitMask) > 0 ? Color1_ : Color0_);
                        if ((i < DrawFontW3) && (DrawOffset_____)) BmpX_.SetPixel(X + (i * 2 + 1) + 48 - DrawOffset2, Y, (Data[DataIdx * 4 + 3] & BitMask) > 0 ? Color1_ : Color0_);
                        BitMask = BitMask >> 1;
                    }
                    break;
            }
            return true;
        }

        public void ToBitmap(string FileName1, string FileName2, bool PageNumber, bool AllCharsIn16)
        {
            int HexSize = PageNumber ? 32 : 0;

            List<int> Pages = new List<int>();
            int MinCharCode = 0x000000;
            int MaxCharCode = 0x110000;
            int ArrayDoubleSize = 0;
            if (FontSizeW <= 8)
            {
                ArrayDoubleSize = FontSizeH2;
            }
            if ((FontSizeW > 8) && (FontSizeW <= 16))
            {
                ArrayDoubleSize = FontSizeH4;
            }
            for (int i = MinCharCode; i < MaxCharCode; i += 256)
            {
                bool RowExists = false;
                for (int ii = i; ii < (i + 256); ii++)
                {
                    if (Chars.ContainsKey(ii))
                    {
                        if (Chars[ii].Length == ArrayDoubleSize)
                        {
                            CharDoubleInsert(ii);
                        }
                        bool CharAllow = true;
                        for (int iii = 0; iii < CharsBlank.Count; iii++)
                        {
                            if (ArrayEquals(Chars[ii], CharsBlank[iii]))
                            {
                                CharsBlankCode[iii].Add(ii);
                                CharAllow = false;
                            }
                        }
                        if (CharAllow)
                        {
                            RowExists = true;
                        }
                        else
                        {
                            Chars.Remove(ii);
                        }
                    }
                }
                if (RowExists)
                {
                    Pages.Add(i);
                }
            }

            LowLevelBitmap BmpX1 = new LowLevelBitmap(16 + (256 * FontSizeW) + HexSize, Pages.Count * FontSizeH, 64);
            LowLevelBitmap BmpX2 = new LowLevelBitmap(16 + (512 * FontSizeW) + HexSize, Pages.Count * FontSizeH, 64);

            int YPos1 = 0;
            int YPos2 = 0;

            for (int i = 0; i < Pages.Count; i++)
            {
                Console.WriteLine("Page " + TextWork.CharCode(Pages[i], 2).Substring(0, 3));
                int PageBits = (Pages[i] & 16777215) >> 8;
                bool WasDrawn1 = false;
                bool WasDrawn2 = false;


                // Page visual number for test
                if (HexSize > 0)
                {
                    string HexPage = PageBits.ToString("X").PadLeft(4, '0');
                    for (int ii = 0; ii < HexPage.Length; ii++)
                    {
                        int ii_N = int.Parse(HexPage[ii].ToString(), System.Globalization.NumberStyles.HexNumber);
                        BmpX1.DrawImage(Hex[ii_N], 0, 0, BmpX1.Width - HexSize + 0 + (ii * 8), YPos1, Hex[ii_N].Width, Hex[ii_N].Height);
                        BmpX2.DrawImage(Hex[ii_N], 0, 0, BmpX2.Width - HexSize + 0 + (ii * 8), YPos2, Hex[ii_N].Width, Hex[ii_N].Height);
                    }
                }

                // Page number code
                for (int ii = 0; ii < FontSizeH; ii++)
                {
                    BmpX1.SetPixelGray(0, YPos1 + ii, (PageBits & 0x8000) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(1, YPos1 + ii, (PageBits & 0x4000) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(2, YPos1 + ii, (PageBits & 0x2000) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(3, YPos1 + ii, (PageBits & 0x1000) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(4, YPos1 + ii, (PageBits & 0x0800) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(5, YPos1 + ii, (PageBits & 0x0400) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(6, YPos1 + ii, (PageBits & 0x0200) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(7, YPos1 + ii, (PageBits & 0x0100) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(8, YPos1 + ii, (PageBits & 0x0080) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(9, YPos1 + ii, (PageBits & 0x0040) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(10, YPos1 + ii, (PageBits & 0x0020) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(11, YPos1 + ii, (PageBits & 0x0010) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(12, YPos1 + ii, (PageBits & 0x0008) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(13, YPos1 + ii, (PageBits & 0x0004) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(14, YPos1 + ii, (PageBits & 0x0002) > 0 ? (byte)255 : (byte)0);
                    BmpX1.SetPixelGray(15, YPos1 + ii, (PageBits & 0x0001) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(0, YPos2 + ii, (PageBits & 0x8000) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(1, YPos2 + ii, (PageBits & 0x4000) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(2, YPos2 + ii, (PageBits & 0x2000) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(3, YPos2 + ii, (PageBits & 0x1000) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(4, YPos2 + ii, (PageBits & 0x0800) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(5, YPos2 + ii, (PageBits & 0x0400) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(6, YPos2 + ii, (PageBits & 0x0200) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(7, YPos2 + ii, (PageBits & 0x0100) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(8, YPos2 + ii, (PageBits & 0x0080) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(9, YPos2 + ii, (PageBits & 0x0040) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(10, YPos2 + ii, (PageBits & 0x0020) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(11, YPos2 + ii, (PageBits & 0x0010) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(12, YPos2 + ii, (PageBits & 0x0008) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(13, YPos2 + ii, (PageBits & 0x0004) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(14, YPos2 + ii, (PageBits & 0x0002) > 0 ? (byte)255 : (byte)0);
                    BmpX2.SetPixelGray(15, YPos2 + ii, (PageBits & 0x0001) > 0 ? (byte)255 : (byte)0);
                }

                int XPos1 = 16;
                int XPos2 = 16;
                for (int ii = 0; ii < 256; ii++)
                {
                    int ChNum = Pages[i] + ii;
                    if (Chars.ContainsKey(ChNum))
                    {
                        int Color0_ = CharC0[ChNum];
                        int Color1_ = CharC1[ChNum];
                        int InfoSize = 0;

                        for (int YY = 0; YY < FontSizeH; YY++)
                        {
                            if (FontSizeW <= 8)
                            {
                                if (Chars[ChNum].Length < FontSizeH)
                                {
                                    InfoSize = 1;
                                }
                                if (Chars[ChNum].Length == FontSizeH)
                                {
                                    if (PaintCharLine(BmpX1, XPos1, YPos1 + YY, Chars[ChNum], YY, false, false, Color0_, Color1_))
                                    {
                                        WasDrawn1 = true;
                                        if (AllCharsIn16)
                                        {
                                            PaintCharLine(BmpX2, XPos2, YPos2 + YY, Chars[ChNum], YY, false, true, Color0_, Color1_);
                                            WasDrawn2 = true;
                                        }
                                    }
                                }
                                if ((Chars[ChNum].Length > FontSizeH) && (Chars[ChNum].Length < FontSizeH2))
                                {
                                    InfoSize = 2;
                                }
                                if (Chars[ChNum].Length == FontSizeH2)
                                {
                                    if (PaintCharLine(BmpX2, XPos2, YPos2 + YY, Chars[ChNum], YY, true, false, Color0_, Color1_))
                                    {
                                        WasDrawn2 = true;
                                    }
                                }
                                if (Chars[ChNum].Length > FontSizeH2)
                                {
                                    InfoSize = 3;
                                }
                            }
                            if ((FontSizeW > 8) && (FontSizeW <= 16))
                            {
                                if (Chars[ChNum].Length < FontSizeH2)
                                {
                                    InfoSize = 1;
                                }
                                if (Chars[ChNum].Length == FontSizeH2)
                                {
                                    if (PaintCharLine(BmpX1, XPos1, YPos1 + YY, Chars[ChNum], YY, false, false, Color0_, Color1_))
                                    {
                                        WasDrawn1 = true;
                                        if (AllCharsIn16)
                                        {
                                            PaintCharLine(BmpX2, XPos2, YPos2 + YY, Chars[ChNum], YY, false, true, Color0_, Color1_);
                                            WasDrawn2 = true;
                                        }
                                    }
                                }
                                if ((Chars[ChNum].Length > FontSizeH2) && (Chars[ChNum].Length < FontSizeH4))
                                {
                                    InfoSize = 2;
                                }
                                if (Chars[ChNum].Length == FontSizeH4)
                                {
                                    if (PaintCharLine(BmpX2, XPos2, YPos2 + YY, Chars[ChNum], YY, true, false, Color0_, Color1_))
                                    {
                                        WasDrawn2 = true;
                                    }
                                }
                                if (Chars[ChNum].Length > FontSizeH4)
                                {
                                    InfoSize = 3;
                                }
                            }
                        }

                        switch (InfoSize)
                        {
                            case 1:
                                Console.WriteLine("character " + TextWork.CharCode(ChNum, 1) + " - size below single");
                                break;
                            case 2:
                                Console.WriteLine("character " + TextWork.CharCode(ChNum, 1) + " - size between single and double");
                                break;
                            case 3:
                                Console.WriteLine("character " + TextWork.CharCode(ChNum, 1) + " - size above double");
                                break;
                        }
                    }
                    XPos1 += FontSizeW;
                    XPos2 += FontSizeW;
                    XPos2 += FontSizeW;
                }
                if (WasDrawn1)
                {
                    YPos1 += FontSizeH;
                }
                if (WasDrawn2)
                {
                    YPos2 += FontSizeH;
                }
            }

            if ((YPos1 > 0) && (FileName1 != ""))
            {
                LowLevelBitmap BmpX1_ = new LowLevelBitmap(BmpX1.Width, YPos1, 0);
                BmpX1_.DrawImage(BmpX1, 0, 0, 0, 0, BmpX1.Width, YPos1);
                BmpX1_.SaveToFile(FileName1);
                Console.WriteLine("Single cell font saved");
            }
            else
            {
                Console.WriteLine("Single cell font not saved");
            }

            if ((YPos2 > 0) && (FileName2 != ""))
            {
                LowLevelBitmap BmpX2_ = new LowLevelBitmap(BmpX2.Width, YPos2, 0);
                BmpX2_.DrawImage(BmpX2, 0, 0, 0, 0, BmpX2.Width, YPos2);
                BmpX2_.SaveToFile(FileName2);
                Console.WriteLine("Double cell font saved");
            }
            else
            {
                Console.WriteLine("Double cell font not saved");
            }
        }

        void CharDoubleInsert(int Code)
        {
            bool NewItem = true;

            for (int i = 0; i < CharDouble1.Count; i++)
            {
                if ((CharDouble1[i] - 1) == Code)
                {
                    CharDouble1[i]--;
                    return;
                }
                if ((CharDouble2[i] + 1) == Code)
                {
                    CharDouble2[i]++;
                    if (CharDouble1.Count > (i + 1))
                    {
                        if ((CharDouble1[i + 1] - 1) == Code)
                        {
                            CharDouble2[i] = CharDouble2[i + 1];
                            CharDouble1.RemoveAt(i + 1);
                            CharDouble2.RemoveAt(i + 1);
                        }
                    }
                    return;
                }
            }


            if (NewItem)
            {
                for (int i = 0; i < CharDouble1.Count; i++)
                {
                    if (CharDouble2[i] > Code)
                    {
                        CharDouble1.Insert(i, Code);
                        CharDouble2.Insert(i, Code);
                        return;
                    }
                }
                CharDouble1.Add(Code);
                CharDouble2.Add(Code);
                return;
            }
        }

        public override void Start()
        {
            string ListFile = CF.ParamGetS("InputFileList");
            string Bmp8 = CF.ParamGetS("FontSingle");
            string Bmp16 = CF.ParamGetS("FontDouble");
            string DblList = CF.ParamGetS("DoubleList");
            string GenHexFile = CF.ParamGetS("OutputHex");
            bool PageNumber = CF.ParamGetB("DrawPageNumber");
            bool AllCharsIn16 = CF.ParamGetB("AllCharsInDouble");
            int FontSizeW_ = CF.ParamGetI("CellW");
            int FontSizeH_ = CF.ParamGetI("CellH");

            int HexNumY1 = 0;
            int HexNumY3 = FontSizeH_ - 1;
            if (FontSizeH_ > 5)
            {
                HexNumY3--;
            }
            if (FontSizeH_ > 7)
            {
                HexNumY1++;
                HexNumY3--;
            }
            if (((HexNumY3 - HexNumY1) % 2) == 1) { HexNumY3--; }
            int HexNumY2 = ((HexNumY3 + HexNumY1) / 2);

            string Segment0 = "# ## ###### # ##";
            string Segment1 = "#####  ####  #  ";
            string Segment2 = "## ######### #  ";
            string Segment3 = "# ## ## ## #### ";
            string Segment4 = "# #   # # ######";
            string Segment5 = "#   ### ##### ##";
            string Segment6 = "  ##### #### ###";

            for (int ii = 0; ii < 16; ii++)
            {
                Hex[ii] = new LowLevelBitmap(8, FontSizeH_, 0);
                Hex[ii].SetPixelGray(1, HexNumY1, 255);
                Hex[ii].SetPixelGray(1, HexNumY2, 255);
                Hex[ii].SetPixelGray(6, HexNumY2, 255);
                Hex[ii].SetPixelGray(6, HexNumY3, 255);
                for (int i = 1; i <= 6; i++)
                {
                    if (Segment0[ii] != ' ')
                    {
                        Hex[ii].SetPixelGray(i, HexNumY1, 255);
                    }
                    if (Segment3[ii] != ' ')
                    {
                        Hex[ii].SetPixelGray(i, HexNumY3, 255);
                    }
                    if (Segment6[ii] != ' ')
                    {
                        Hex[ii].SetPixelGray(i, HexNumY2, 255);
                    }
                }
                for (int i = HexNumY1; i <= HexNumY2; i++)
                {
                    if (Segment1[ii] != ' ')
                    {
                        Hex[ii].SetPixelGray(6, i, 255);
                    }
                    if (Segment5[ii] != ' ')
                    {
                        Hex[ii].SetPixelGray(1, i, 255);
                    }
                }
                for (int i = HexNumY2; i <= HexNumY3; i++)
                {
                    if (Segment2[ii] != ' ')
                    {
                        Hex[ii].SetPixelGray(6, i, 255);
                    }
                    if (Segment4[ii] != ' ')
                    {
                        Hex[ii].SetPixelGray(1, i, 255);
                    }
                }
            }

            FontSizeW = FontSizeW_;
            FontSizeH = FontSizeH_;
            FontSizeH2 = FontSizeH_ * 2;
            FontSizeH3 = FontSizeH_ * 3;
            FontSizeH4 = FontSizeH_ * 4;
            CharsBlank = new List<int[]>();
            CharsBlankCode = new List<List<int>>();
            Chars = new Dictionary<int, int[]>();
            CharC0 = new Dictionary<int, int>();
            CharC1 = new Dictionary<int, int>();
            CharDouble1 = new List<int>();
            CharDouble2 = new List<int>();

            FileStream Lst = new FileStream(ListFile, FileMode.Open, FileAccess.Read);
            StreamReader Lst_ = new StreamReader(Lst);
            bool LoadLast = false;
            string Item = Lst_.ReadLine();
            int InsideComment = 0;
            string CurrentPath = Path.GetDirectoryName(ListFile);
            if (!CurrentPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                CurrentPath = CurrentPath + Path.DirectorySeparatorChar.ToString();
            }
            CharColor0 = LowLevelBitmap.ColorRgbToInt(0, 0, 0);
            CharColor1 = LowLevelBitmap.ColorRgbToInt(255, 255, 255);
            while (Item != null)
            {
                if (Item.Length >= 1)
                {
                    switch (Item[0])
                    {
                        case '*':
                            CurrentPath = Item.Substring(1);
                            break;
                        case ':':
                            if (InsideComment == 0)
                            {
                                string[] _ = Item.Split(':');
                                CharColor0 = LowLevelBitmap.ColorRgbToInt(int.Parse(_[1]), int.Parse(_[2]), int.Parse(_[3]));
                                CharColor1 = LowLevelBitmap.ColorRgbToInt(int.Parse(_[4]), int.Parse(_[5]), int.Parse(_[6]));
                            }
                            break;
                        case '@':
                            if (InsideComment == 0)
                            {
                                if ("@FIRST".Equals(Item.ToUpperInvariant()))
                                {
                                    LoadLast = false;
                                }
                                if ("@LAST".Equals(Item.ToUpperInvariant()))
                                {
                                    LoadLast = true;
                                }
                            }
                            break;
                        case '#':
                            {

                            }
                            break;
                        case '{':
                            InsideComment++;
                            break;
                        case '}':
                            InsideComment--;
                            break;
                        default:
                            if (InsideComment == 0)
                            {
                                LoadFile(CurrentPath + Item, LoadLast);
                            }
                            break;
                    }
                }

                Item = Lst_.ReadLine();
            }
            Lst_.Close();
            Lst.Close();

            ToBitmap(Bmp8, Bmp16, PageNumber, AllCharsIn16);

            if (DblList != "")
            {
                FileStream DblFS = new FileStream(DblList, FileMode.Create, FileAccess.Write);
                StreamWriter DblFS_ = new StreamWriter(DblFS);
                for (int i = 0; i < CharDouble1.Count; i++)
                {
                    if (CharDouble1[i] != CharDouble2[i])
                    {
                        DblFS_.WriteLine(CharDouble1[i].ToString("X").PadLeft(4, '0') + ".." + CharDouble2[i].ToString("X").PadLeft(4, '0'));
                    }
                    else
                    {
                        DblFS_.WriteLine(CharDouble1[i].ToString("X").PadLeft(4, '0'));
                    }
                }
                DblFS_.Close();
                DblFS.Close();
            }

            if (GenHexFile != "")
            {
                FileStream HexFS = new FileStream(GenHexFile, FileMode.Create, FileAccess.Write);
                StreamWriter HexFS_ = new StreamWriter(HexFS);
                foreach (var item in Chars)
                {
                    HexFS_.Write(item.Key.ToString("X").PadLeft(4, '0') + ":");
                    for (int iii = 0; iii < item.Value.Length; iii++)
                    {
                        HexFS_.Write(item.Value[iii].ToString("X").PadLeft(2, '0'));
                    }
                    HexFS_.WriteLine();
                }
                HexFS_.Close();
                HexFS.Close();
            }
        }
    }
}
