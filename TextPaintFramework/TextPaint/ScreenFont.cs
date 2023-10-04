using System;
using System.Collections.Generic;

namespace TextPaint
{
    public class ScreenFont
    {
        public bool[,] WinBitmapGlyph;
        public Dictionary<int, int> WinBitmapPage;
        public int CellW_F;
        public int CellH_F;
        public int CellW;
        public int CellH;
        public int[] PlaneSecond = new int[17];

        public ScreenFont(string FileName, int CellW___, int CellH___, bool DoubleSize)
        {
            for (int i = 0; i < 17; i++)
            {
                PlaneSecond[i] = -1;
            }
            LowLevelBitmap FontTempBmp = new LowLevelBitmap(Core.FullPath(FileName));
            int CellW_ = (int)Math.Floor((FontTempBmp.Width - 16.0) / 256.0);
            List<int> WinBitmapPage_ = new List<int>();
            int Idx = 0;
            int Val0 = -1;
            int ValPlane = 0;
            for (int i = 0; i < FontTempBmp.Height; i++)
            {
                int Val = 0;
                if (FontTempBmp.GetPixelBinary(0, i)) { Val += 32768; }
                if (FontTempBmp.GetPixelBinary(1, i)) { Val += 16384; }
                if (FontTempBmp.GetPixelBinary(2, i)) { Val += 8192; }
                if (FontTempBmp.GetPixelBinary(3, i)) { Val += 4096; }
                if (FontTempBmp.GetPixelBinary(4, i)) { Val += 2048; }
                if (FontTempBmp.GetPixelBinary(5, i)) { Val += 1024; }
                if (FontTempBmp.GetPixelBinary(6, i)) { Val += 512; }
                if (FontTempBmp.GetPixelBinary(7, i)) { Val += 256; }
                if (FontTempBmp.GetPixelBinary(8, i)) { Val += 128; }
                if (FontTempBmp.GetPixelBinary(9, i)) { Val += 64; }
                if (FontTempBmp.GetPixelBinary(10, i)) { Val += 32; }
                if (FontTempBmp.GetPixelBinary(11, i)) { Val += 16; }
                if (FontTempBmp.GetPixelBinary(12, i)) { Val += 8; }
                if (FontTempBmp.GetPixelBinary(13, i)) { Val += 4; }
                if (FontTempBmp.GetPixelBinary(14, i)) { Val += 2; }
                if (FontTempBmp.GetPixelBinary(15, i)) { Val += 1; }
                if (Val0 != Val)
                {
                    WinBitmapPage_.Add(ValPlane + Val);
                    Idx++;
                    Val0 = Val;
                }
            }
            int CellH_ = (int)Math.Floor(FontTempBmp.Height * 1.0 / Idx);
            WinBitmapPage = new Dictionary<int, int>();
            int CellW_2 = CellW_;
            if (DoubleSize)
            {
                CellW___ = CellW___ << 1;
                CellW_ = CellW_ << 1;
                CellW = CellW << 1;
            }
            CellW_F = (int)Math.Round(((double)CellW___) / ((double)CellW_));
            CellH_F = (int)Math.Round(((double)CellH___) / ((double)CellH_));
            if (CellW_F < 1) { CellW_F = 1; }
            if (CellH_F < 1) { CellH_F = 1; }
            CellW = CellW_ * CellW_F;
            CellH = CellH_ * CellH_F;
            WinBitmapGlyph = new bool[Idx * CellH, 256 * CellW];
            for (int i = 0; i < Idx; i++)
            {
                WinBitmapPage.Add(WinBitmapPage_[i], i);
                for (int ii = 0; ii < 256; ii++)
                {
                    if (DoubleSize)
                    {
                        for (int Y_ = 0; Y_ < CellH_; Y_++)
                        {
                            for (int X_ = 0; X_ < CellW_2; X_++)
                            {
                                for (int Y_F = 0; Y_F < CellH_F; Y_F++)
                                {
                                    for (int X_F = 0; X_F < CellW_F; X_F++)
                                    {
                                        bool PxlVal = FontTempBmp.GetPixelBinary((CellW_2) * ii + 16 + X_, CellH_ * i + Y_);
                                        WinBitmapGlyph[CellH * i + Y_ * CellH_F + Y_F, CellW * ii + (X_ << 1) * CellW_F + X_F] = PxlVal;
                                        WinBitmapGlyph[CellH * i + Y_ * CellH_F + Y_F, CellW * ii + (X_ << 1) * CellW_F + X_F + 1] = PxlVal;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int Y_ = 0; Y_ < CellH_; Y_++)
                        {
                            for (int X_ = 0; X_ < CellW_; X_++)
                            {
                                for (int Y_F = 0; Y_F < CellH_F; Y_F++)
                                {
                                    for (int X_F = 0; X_F < CellW_F; X_F++)
                                    {
                                        bool PxlVal = (FontTempBmp.GetPixelBinary(CellW_ * ii + 16 + X_, CellH_ * i + Y_));
                                        WinBitmapGlyph[CellH * i + Y_ * CellH_F + Y_F, CellW * ii + X_ * CellW_F + X_F] = PxlVal;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        int CalcSecondPlane(int Page)
        {
            return (PlaneSecond[Page >> 8] << 8) + (Page & 255);
        }

        public bool MergeFont(ScreenFont Dbl, bool IsDouble, int[] CharDoubleTable)
        {
            bool[] PlaneOccupied1 = new bool[17];
            bool[] PlaneOccupied2 = new bool[17];
            bool[] PlaneOccupied_ = new bool[17];
            int[] PlaneSearch = new int[] { 8, 9, 10, 11, 12, 13, 7, 6, 5, 4 };
            for (int i = 0; i < 17; i++)
            {
                PlaneOccupied1[i] = false;
                PlaneOccupied2[i] = false;
                PlaneOccupied_[i] = false;
                PlaneSecond[i] = -1;
            }
            foreach (var item in WinBitmapPage)
            {
                PlaneOccupied1[item.Key >> 8] = true;
            }
            foreach (var item in Dbl.WinBitmapPage)
            {
                PlaneOccupied2[item.Key >> 8] = true;
                PlaneOccupied_[item.Key >> 8] = true;
            }

            // Creating plane map for second half
            for (int i = 0; i < PlaneSearch.Length; i++)
            {
                if ((!PlaneOccupied1[PlaneSearch[i]]) && (!PlaneOccupied2[PlaneSearch[i]]))
                {
                    for (int ii = 0; ii < 17; ii++)
                    {
                        if (PlaneOccupied_[ii])
                        {
                            PlaneSecond[ii] = PlaneSearch[i];
                            PlaneOccupied_[ii] = false;
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < 17; i++)
            {
                if (PlaneOccupied2[i])
                {
                    if (PlaneSecond[i] < 0)
                    {
                        return false;
                    }
                }
            }

            // Appending merged pages
            int Idx = WinBitmapPage.Count;
            int Idx0 = WinBitmapPage.Count;
            foreach (var item in Dbl.WinBitmapPage)
            {
                if (WinBitmapPage.ContainsKey(item.Key))
                {
                    if (IsDouble)
                    {
                        WinBitmapPage.Add(CalcSecondPlane(item.Key), Idx);
                        Idx++;
                    }
                }
                else
                {
                    WinBitmapPage.Add(item.Key, Idx);
                    Idx++;
                    if (IsDouble)
                    {
                        WinBitmapPage.Add(CalcSecondPlane(item.Key), Idx);
                        Idx++;
                    }
                }
            }

            // Expanding glyph array
            if (Idx0 != Idx)
            {
                bool[,] WinBitmapGlyph_Temp = new bool[Idx * CellH, 256 * CellW];
                int BitH = Idx0 * CellH;
                int BitW = 256 * CellW;
                for (int i = 0; i < BitH; i++)
                {
                    for (int ii = 0; ii < BitW; ii++)
                    {
                        WinBitmapGlyph_Temp[i, ii] = WinBitmapGlyph[i, ii];
                    }
                }
                WinBitmapGlyph = WinBitmapGlyph_Temp;
            }

            // Drawing glyphs
            if (IsDouble)
            {
                foreach (var item in Dbl.WinBitmapPage)
                {
                    int OffsetYSrc = item.Value * CellH;
                    int OffsetYDs1 = WinBitmapPage[item.Key] * CellH;
                    int OffsetYDs2 = WinBitmapPage[CalcSecondPlane(item.Key)] * CellH;
                    int ArrayWidth = 256 * CellW;
                    for (int Y = 0; Y < CellH; Y++)
                    {
                        int X1 = 0;
                        int X2 = 0;
                        for (int XX = 0; XX < 256; XX++)
                        {
                            for (int i = 0; i < CellW; i++)
                            {
                                if (Dbl.WinBitmapGlyph[OffsetYSrc + Y, X1])
                                {
                                    if (CharDoubleTable[(item.Key << 8) + XX] == 0)
                                    {
                                        CharDoubleTable[(item.Key << 8) + XX] = (CalcSecondPlane(item.Key) << 8) + XX;
                                    }
                                    WinBitmapGlyph[OffsetYDs1 + Y, X2] = true;
                                }
                                X1++;
                                X2++;
                            }
                            X2 -= CellW;
                            for (int i = 0; i < CellW; i++)
                            {
                                if (Dbl.WinBitmapGlyph[OffsetYSrc + Y, X1])
                                {
                                    if (CharDoubleTable[(item.Key << 8) + XX] == 0)
                                    {
                                        CharDoubleTable[(item.Key << 8) + XX] = (CalcSecondPlane(item.Key) << 8) + XX;
                                    }
                                    WinBitmapGlyph[OffsetYDs2 + Y, X2] = true;
                                }
                                X1++;
                                X2++;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var item in Dbl.WinBitmapPage)
                {
                    int OffsetYSrc = item.Value * CellH;
                    int OffsetYDst = WinBitmapPage[item.Key] * CellH;
                    int ArrayWidth = 256 * CellW;
                    for (int Y = 0; Y < CellH; Y++)
                    {
                        for (int X = 0; X < ArrayWidth; X++)
                        {
                            if (Dbl.WinBitmapGlyph[OffsetYSrc + Y, X])
                            {
                                WinBitmapGlyph[OffsetYDst + Y, X] = true;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
