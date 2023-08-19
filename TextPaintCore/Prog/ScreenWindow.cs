/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-07
 * Time: 23:03
 * 
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace TextPaint
{
    /// <summary>
    /// Description of ScreenWindow.
    /// </summary>
    public abstract class ScreenWindow : Screen
    {
        public bool SteadyCursor = false;

        int ScreenBorder = 2;
        int[] WinCursorPosX = new int[256];
        int[] WinCursorPosY = new int[256];

        LowLevelBitmap[] BitmapX_;
        LowLevelBitmap Glyph_;
        object BitmapX_mutex = new object();

        int MaxFontSize = 9;
        int MaxFontSizeDraw = 527;

        int InternalW;
        int InternalH;
        bool WinIsBitmapFont = false;
        bool[,] WinBitmapGlyph;
        Dictionary<int, int> WinBitmapPage;
        bool ColorBlending = false;
        List<int> ColorBlendingChar = new List<int>();
        List<int> ColorBlendingReplacement = new List<int>();
        List<int> ColorBlendingProp1 = new List<int>();
        List<int> ColorBlendingProp2 = new List<int>();
        List<bool> ColorBlendingBackground = new List<bool>();

        void CalcBlend(int C1, int C2, double Prop1, double Prop2, out byte C_R, out byte C_G, out byte C_B)
        {
            double C1_R = Math.Pow(((double)DrawColor_R[C1]) / 255.0, 2.2) * Prop1;
            double C1_G = Math.Pow(((double)DrawColor_G[C1]) / 255.0, 2.2) * Prop1;
            double C1_B = Math.Pow(((double)DrawColor_B[C1]) / 255.0, 2.2) * Prop1;
            double C2_R = Math.Pow(((double)DrawColor_R[C2]) / 255.0, 2.2) * Prop2;
            double C2_G = Math.Pow(((double)DrawColor_G[C2]) / 255.0, 2.2) * Prop2;
            double C2_B = Math.Pow(((double)DrawColor_B[C2]) / 255.0, 2.2) * Prop2;
            double C3_R = Math.Pow((C1_R + C2_R) / (Prop1 + Prop2), 1 / 2.2);
            double C3_G = Math.Pow((C1_G + C2_G) / (Prop1 + Prop2), 1 / 2.2);
            double C3_B = Math.Pow((C1_B + C2_B) / (Prop1 + Prop2), 1 / 2.2);
            C_R = (byte)(C3_R * 255.0);
            C_G = (byte)(C3_G * 255.0);
            C_B = (byte)(C3_B * 255.0);
        }

        /// <summary>
        /// Glybh bank used to drawing characters on the screen
        /// </summary>
        Dictionary<long, LowLevelBitmap>[] GlyphBankX;

        LowLevelBitmap GlyphBankGet(int ColorB, int ColorF, int Char, int FontWH, int FontBIU)
        {
            if (FontWH < 0)
            {
                return null;
            }

            // Characters above 0xFFFFF are very rarely used
            // and resignation from buffering there saves 1 bit in index number
            if (Char > 0xFFFFF)
            {
                return null;
            }
            long Idx = Char + (ColorB << 20) + (ColorF << 25);
            if (GlyphBankX[FontWH + FontBIU].ContainsKey(Idx))
            {
                return GlyphBankX[FontWH + FontBIU][Idx];
            }
            else
            {
                return null;
            }
        }

        void GlyphBankSet(int ColorB, int ColorF, int Char, LowLevelBitmap Glyph, int FontWH, int FontBIU)
        {
            if (FontWH < 0)
            {
                return;
            }

            // Characters above 0xFFFFF are very rarely used
            // and resignation from buffering there saves 1 bit in index number
            if (Char > 0xFFFFF)
            {
                return;
            }
            long Idx = Char + (ColorB << 20) + (ColorF << 25);
            GlyphBankX[FontWH + FontBIU].Add(Idx, Glyph);
        }

        byte[] DrawColor_R;
        byte[] DrawColor_G;
        byte[] DrawColor_B;

        int[] FontW_Num_Min;
        int[] FontW_Num_Max;

        float GetRectPos(int FontSize)
        {
            if ((FontSize >= 1) && (FontSize <= 2)) { return (FontSize - 1); }
            if ((FontSize >= 3) && (FontSize <= 5)) { return (FontSize - 3); }
            if ((FontSize >= 6) && (FontSize <= 9)) { return (FontSize - 6); }
            if ((FontSize >= 10) && (FontSize <= 14)) { return (FontSize - 10); }
            if ((FontSize >= 15) && (FontSize <= 20)) { return (FontSize - 15); }
            if ((FontSize >= 21) && (FontSize <= 27)) { return (FontSize - 21); }
            if ((FontSize >= 28) && (FontSize <= 35)) { return (FontSize - 28); }
            if ((FontSize >= 36) && (FontSize <= 44)) { return (FontSize - 36); }
            if ((FontSize >= 45) && (FontSize <= 54)) { return (FontSize - 45); }
            if ((FontSize >= 55) && (FontSize <= 65)) { return (FontSize - 55); }
            if ((FontSize >= 66) && (FontSize <= 77)) { return (FontSize - 66); }
            if ((FontSize >= 78) && (FontSize <= 90)) { return (FontSize - 78); }
            if ((FontSize >= 91) && (FontSize <= 104)) { return (FontSize - 91); }
            if ((FontSize >= 105) && (FontSize <= 119)) { return (FontSize - 105); }
            if ((FontSize >= 120) && (FontSize <= 135)) { return (FontSize - 120); }
            if ((FontSize >= 136) && (FontSize <= 152)) { return (FontSize - 136); }
            if ((FontSize >= 153) && (FontSize <= 170)) { return (FontSize - 153); }
            if ((FontSize >= 171) && (FontSize <= 189)) { return (FontSize - 171); }
            if ((FontSize >= 190) && (FontSize <= 209)) { return (FontSize - 190); }
            if ((FontSize >= 210) && (FontSize <= 230)) { return (FontSize - 210); }
            if ((FontSize >= 231) && (FontSize <= 252)) { return (FontSize - 231); }
            if ((FontSize >= 253) && (FontSize <= 275)) { return (FontSize - 253); }
            if ((FontSize >= 276) && (FontSize <= 299)) { return (FontSize - 276); }
            if ((FontSize >= 300) && (FontSize <= 324)) { return (FontSize - 300); }
            if ((FontSize >= 325) && (FontSize <= 350)) { return (FontSize - 325); }
            if ((FontSize >= 351) && (FontSize <= 377)) { return (FontSize - 351); }
            if ((FontSize >= 378) && (FontSize <= 405)) { return (FontSize - 378); }
            if ((FontSize >= 406) && (FontSize <= 434)) { return (FontSize - 406); }
            if ((FontSize >= 435) && (FontSize <= 464)) { return (FontSize - 435); }
            if ((FontSize >= 465) && (FontSize <= 495)) { return (FontSize - 465); }
            if ((FontSize >= 496) && (FontSize <= 527)) { return (FontSize - 496); }
            return 0;
        }

        int GetRectSize(int FontSize)
        {
            if ((FontSize >= 1) && (FontSize <= 2)) { return 2; }
            if ((FontSize >= 3) && (FontSize <= 5)) { return 3; }
            if ((FontSize >= 6) && (FontSize <= 9)) { return 4; }
            if ((FontSize >= 10) && (FontSize <= 14)) { return 5; }
            if ((FontSize >= 15) && (FontSize <= 20)) { return 6; }
            if ((FontSize >= 21) && (FontSize <= 27)) { return 7; }
            if ((FontSize >= 28) && (FontSize <= 35)) { return 8; }
            if ((FontSize >= 36) && (FontSize <= 44)) { return 9; }
            if ((FontSize >= 45) && (FontSize <= 54)) { return 10; }
            if ((FontSize >= 55) && (FontSize <= 65)) { return 11; }
            if ((FontSize >= 66) && (FontSize <= 77)) { return 12; }
            if ((FontSize >= 78) && (FontSize <= 90)) { return 13; }
            if ((FontSize >= 91) && (FontSize <= 104)) { return 14; }
            if ((FontSize >= 105) && (FontSize <= 119)) { return 15; }
            if ((FontSize >= 120) && (FontSize <= 135)) { return 16; }
            if ((FontSize >= 136) && (FontSize <= 152)) { return 17; }
            if ((FontSize >= 153) && (FontSize <= 170)) { return 18; }
            if ((FontSize >= 171) && (FontSize <= 189)) { return 19; }
            if ((FontSize >= 190) && (FontSize <= 209)) { return 20; }
            if ((FontSize >= 210) && (FontSize <= 230)) { return 21; }
            if ((FontSize >= 231) && (FontSize <= 252)) { return 22; }
            if ((FontSize >= 253) && (FontSize <= 275)) { return 23; }
            if ((FontSize >= 276) && (FontSize <= 299)) { return 24; }
            if ((FontSize >= 300) && (FontSize <= 324)) { return 25; }
            if ((FontSize >= 325) && (FontSize <= 350)) { return 26; }
            if ((FontSize >= 351) && (FontSize <= 377)) { return 27; }
            if ((FontSize >= 378) && (FontSize <= 405)) { return 28; }
            if ((FontSize >= 406) && (FontSize <= 434)) { return 29; }
            if ((FontSize >= 435) && (FontSize <= 464)) { return 30; }
            if ((FontSize >= 465) && (FontSize <= 495)) { return 31; }
            if ((FontSize >= 496) && (FontSize <= 527)) { return 32; }
            return 1;
        }

        void SetTextRectangles()
        {
            FontW_Num_Min = new int[33];
            FontW_Num_Max = new int[33];
            FontW_Num_Min[1] = 0; FontW_Num_Max[1] = 0;
            FontW_Num_Min[2] = 1; FontW_Num_Max[2] = 2;
            FontW_Num_Min[3] = 3; FontW_Num_Max[3] = 5;
            FontW_Num_Min[4] = 6; FontW_Num_Max[4] = 9;
            FontW_Num_Min[5] = 10; FontW_Num_Max[5] = 14;
            FontW_Num_Min[6] = 15; FontW_Num_Max[6] = 20;
            FontW_Num_Min[7] = 21; FontW_Num_Max[7] = 27;
            FontW_Num_Min[8] = 28; FontW_Num_Max[8] = 35;
            FontW_Num_Min[9] = 36; FontW_Num_Max[9] = 44;
            FontW_Num_Min[10] = 45; FontW_Num_Max[10] = 54;
            FontW_Num_Min[11] = 55; FontW_Num_Max[11] = 65;
            FontW_Num_Min[12] = 66; FontW_Num_Max[12] = 77;
            FontW_Num_Min[13] = 78; FontW_Num_Max[13] = 90;
            FontW_Num_Min[14] = 91; FontW_Num_Max[14] = 104;
            FontW_Num_Min[15] = 105; FontW_Num_Max[15] = 119;
            FontW_Num_Min[16] = 120; FontW_Num_Max[16] = 135;
            FontW_Num_Min[17] = 136; FontW_Num_Max[17] = 152;
            FontW_Num_Min[18] = 153; FontW_Num_Max[18] = 170;
            FontW_Num_Min[19] = 171; FontW_Num_Max[19] = 189;
            FontW_Num_Min[20] = 190; FontW_Num_Max[20] = 209;
            FontW_Num_Min[21] = 210; FontW_Num_Max[21] = 230;
            FontW_Num_Min[22] = 231; FontW_Num_Max[22] = 252;
            FontW_Num_Min[23] = 253; FontW_Num_Max[23] = 275;
            FontW_Num_Min[24] = 276; FontW_Num_Max[24] = 299;
            FontW_Num_Min[25] = 300; FontW_Num_Max[25] = 324;
            FontW_Num_Min[26] = 325; FontW_Num_Max[26] = 351;
            FontW_Num_Min[27] = 352; FontW_Num_Max[27] = 377;
            FontW_Num_Min[28] = 378; FontW_Num_Max[28] = 405;
            FontW_Num_Min[29] = 406; FontW_Num_Max[29] = 434;
            FontW_Num_Min[30] = 435; FontW_Num_Max[30] = 464;
            FontW_Num_Min[31] = 465; FontW_Num_Max[31] = 495;
            FontW_Num_Min[32] = 496; FontW_Num_Max[32] = 527;


            FormCtrlR_X = new float[MaxFontSizeDraw + 1];
            FormCtrlR_Y = new float[MaxFontSizeDraw + 1];
            FormCtrlR_W = new float[MaxFontSizeDraw + 1];
            FormCtrlR_H = new float[MaxFontSizeDraw + 1];
            FormCtrlR_Trans = new int[MaxFontSizeDraw + 1];
            for (int i = 0; i <= MaxFontSizeDraw; i++)
            {
                FormCtrlR_Trans[i] = GetRectSize(i);
                FormCtrlR_X[i] = GetRectPos(i) * CellW;
                FormCtrlR_Y[i] = GetRectPos(i) * CellH;
                FormCtrlR_W[i] = GetRectSize(i) * CellW;
                FormCtrlR_H[i] = GetRectSize(i) * CellH;
            }
        }

        protected void ScreenWindowPrepare(Core Core__, int WinFixed_, ConfigFile CF, int ConsoleW, int ConsoleH, bool ColorBlending_, List<string> ColorBlendingConfig_, bool DummyScreen)
        {
            BitmapX_ = new LowLevelBitmap[2];
            WinFixed = WinFixed_;
            if ((WinFixed == 2) || (WinFixed == 3))
            {
                ScreenBorder = 0;
            }
            //GlyphBankX = new Dictionary<long, LowLevelBitmap>[(MaxFontSize + 1) * (MaxFontSize + 1) * 4];
            GlyphBankX = new Dictionary<long, LowLevelBitmap>[128 * 16];
            for (int i = 0; i < GlyphBankX.Length; i++)
            {
                GlyphBankX[i] = new Dictionary<long, LowLevelBitmap>();
            }

            UseMemo = 0;
            ColorBlending = ColorBlending_;
            for (int i = 0; i < ColorBlendingConfig_.Count; i++)
            {
                string[] LineData = ColorBlendingConfig_[i].Split(',');
                if (LineData.Length == 5)
                {
                    ColorBlendingChar.Add(TextWork.CodeChar(LineData[0]));
                    ColorBlendingReplacement.Add(TextWork.CodeChar(LineData[1]));
                    ColorBlendingProp1.Add(int.Parse(LineData[2]));
                    ColorBlendingProp2.Add(int.Parse(LineData[3]));
                    ColorBlendingBackground.Add(int.Parse(LineData[4]) != 0);
                }
            }

            if (DummyScreen)
            {
                ScreenBorder = 0;
            }

            DrawColor_R = new byte[32];
            DrawColor_G = new byte[32];
            DrawColor_B = new byte[32];
            DrawColor_R[0] = 0; DrawColor_G[0] = 0; DrawColor_B[0] = 0;
            DrawColor_R[1] = 170; DrawColor_G[1] = 0; DrawColor_B[1] = 0;
            DrawColor_R[2] = 0; DrawColor_G[2] = 170; DrawColor_B[2] = 0;
            DrawColor_R[3] = 170; DrawColor_G[3] = 170; DrawColor_B[3] = 0;
            DrawColor_R[4] = 0; DrawColor_G[4] = 0; DrawColor_B[4] = 170;
            DrawColor_R[5] = 170; DrawColor_G[5] = 0; DrawColor_B[5] = 170;
            DrawColor_R[6] = 0; DrawColor_G[6] = 170; DrawColor_B[6] = 170;
            DrawColor_R[7] = 170; DrawColor_G[7] = 170; DrawColor_B[7] = 170;
            DrawColor_R[8] = 85; DrawColor_G[8] = 85; DrawColor_B[8] = 85;
            DrawColor_R[9] = 255; DrawColor_G[9] = 85; DrawColor_B[9] = 85;
            DrawColor_R[10] = 85; DrawColor_G[10] = 255; DrawColor_B[10] = 85;
            DrawColor_R[11] = 255; DrawColor_G[11] = 255; DrawColor_B[11] = 85;
            DrawColor_R[12] = 85; DrawColor_G[12] = 85; DrawColor_B[12] = 255;
            DrawColor_R[13] = 255; DrawColor_G[13] = 85; DrawColor_B[13] = 255;
            DrawColor_R[14] = 85; DrawColor_G[14] = 255; DrawColor_B[14] = 255;
            DrawColor_R[15] = 255; DrawColor_G[15] = 255; DrawColor_B[15] = 255;

            string PalR = CF.ParamGetS("WinPaletteR");
            string PalG = CF.ParamGetS("WinPaletteG");
            string PalB = CF.ParamGetS("WinPaletteB");
            string PalR_ = CF.ParamGetS("WinPaletteBlinkR");
            string PalG_ = CF.ParamGetS("WinPaletteBlinkG");
            string PalB_ = CF.ParamGetS("WinPaletteBlinkB");
            string PalFile = CF.ParamGetS("WinPaletteFile");
            if (File.Exists(PalFile))
            {
                ConfigFile CF_Pal = new ConfigFile();
                CF_Pal.FileLoad(PalFile);
                PalR = CF_Pal.ParamGetS("WinPaletteR", PalR);
                PalG = CF_Pal.ParamGetS("WinPaletteG", PalG);
                PalB = CF_Pal.ParamGetS("WinPaletteB", PalB);

                PalR_ = CF_Pal.ParamGetS("WinPaletteBlinkR", PalR_);
                PalG_ = CF_Pal.ParamGetS("WinPaletteBlinkG", PalG_);
                PalB_ = CF_Pal.ParamGetS("WinPaletteBlinkB", PalB_);
            }
            if ((PalR.Length == 32) && (PalG.Length == 32) && (PalB.Length == 32))
            {
                for (int i = 0; i < 16; i++)
                {
                    DrawColor_R[i] = (byte)int.Parse(PalR.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                    DrawColor_G[i] = (byte)int.Parse(PalG.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                    DrawColor_B[i] = (byte)int.Parse(PalB.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                }
            }

            for (int i = 0; i < 16; i++)
            {
                DrawColor_R[i + 16] = (byte)((int)DrawColor_R[i] * 2 / 3);
                DrawColor_G[i + 16] = (byte)((int)DrawColor_G[i] * 2 / 3);
                DrawColor_B[i + 16] = (byte)((int)DrawColor_B[i] * 2 / 3);
            }

            if ((PalR_.Length == 32) && (PalG_.Length == 32) && (PalB_.Length == 32))
            {
                for (int i = 0; i < 16; i++)
                {
                    DrawColor_R[i + 16] = (byte)int.Parse(PalR_.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                    DrawColor_G[i + 16] = (byte)int.Parse(PalG_.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                    DrawColor_B[i + 16] = (byte)int.Parse(PalB_.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                }
            }

            CellW = CF.ParamGetI("WinCellW");
            CellH = CF.ParamGetI("WinCellH");
            WinFontName = CF.ParamGetS("WinFontName");
            WinFontSize = CF.ParamGetI("WinFontSize");
            WinCharRender = CF.ParamGetI("WinCharRender");
            WinPicturePanel = (CF.ParamGetI("WinUse") == 2);

            if (File.Exists(Core.FullPath(WinFontName)))
            {
                WinIsBitmapFont = true;
                LowLevelBitmap FontTempBmp = new LowLevelBitmap(Core.FullPath(WinFontName));
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
                CellW_F = (int)Math.Round(((double)CellW) / ((double)CellW_));
                CellH_F = (int)Math.Round(((double)CellH) / ((double)CellH_));
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
                        for (int Y_ = 0; Y_ < CellH_; Y_++)
                        {
                            for (int X_ = 0; X_ < CellW_; X_++)
                            {
                                bool PxlState = (FontTempBmp.GetPixelBinary(CellW_ * ii + 16 + X_, CellH_ * i + Y_));
                                for (int Y_F = 0; Y_F < CellH_F; Y_F++)
                                {
                                    for (int X_F = 0; X_F < CellW_F; X_F++)
                                    {
                                        WinBitmapGlyph[CellH * i + Y_ * CellH_F + Y_F, CellW * ii + X_ * CellW_F + X_F] = PxlState;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                WinIsBitmapFont = false;
                Glyph_ = new LowLevelBitmap(CellW, CellH, 0);
                Glyph_.SetTextFont(WinFontName, WinFontSize, WinCharRender);
            }

            CursorThick = (((CellH + 7) / 8));
            if (WinIsBitmapFont)
            {
                CursorThick = CellH_F * ((((CellH / CellH_F) + 7) / 8));
            }
            CursorDispOffset = CellH - CursorThick;


            Core_ = Core__;
            Core_.Screen_ = this;
        }

        public ScreenWindow(Core Core__, int WinFixed_, ConfigFile CF, int ConsoleW, int ConsoleH, bool ColorBlending_, List<string> ColorBlendingConfig_, bool DummyScreen)
        {
            LoadConfig(CF);
            ScreenWindowPrepare(Core__, WinFixed_, CF, ConsoleW, ConsoleH, ColorBlending_, ColorBlendingConfig_, DummyScreen);
        }

        protected virtual void StartAppForm()
        {

        }

        protected virtual void FormCtrlRefresh()
        {

        }

        protected virtual void FormCtrlSetColor(byte ColorR, byte ColorG, byte ColorB)
        {

        }

        protected bool FormAllowClose = false;

        public override void StartApp()
        {
            StartAppForm();
        }

        protected void StartAppFormShown()
        {
            AppWorking = true;
            Core_.StartUp();
            FormCtrlSetParam(8, 1);
        }

        protected int CellW;
        protected int CellH;
        int CellW_F;
        int CellH_F;
        string WinFontName;
        int WinFontSize;
        int WinCharRender;

        int[,] FormCtrlB;
        int[,] FormCtrlF;
        int[,] FormCtrlA;
        int[,] FormCtrlC;
        int[,] FormCtrlFontW;
        int[,] FormCtrlFontH;
        public int[] LineOffset;
        int[] LineOffsetBack;
        int[] LineOffsetFore;
        int[] LineOffsetAttr;
        public bool[] LineOffsetBlank;
        float[] FormCtrlR_X;
        float[] FormCtrlR_Y;
        float[] FormCtrlR_W;
        float[] FormCtrlR_H;
        int[] FormCtrlR_Trans;
        protected bool WinPicturePanel = false;
        byte CursorColorR = 0;
        byte CursorColorG = 0;
        byte CursorColorB = 0;

        int CursorDispOffset = 0;
        int CursorThick = 0;



        /// <summary>
        /// Function used in rendering
        /// </summary>
        /// <param name="W">W.</param>
        /// <param name="H">H.</param>
        public void DummyResize(int W, int H)
        {
            Monitor.Enter(BitmapX_mutex);
            WinW = W;
            WinH = H;

            BitmapX_[0] = new LowLevelBitmap(WinW * CellW, WinH * CellH, 0);
            BitmapX_[1] = new LowLevelBitmap(WinW * CellW, WinH * CellH, 0);

            FormCtrlB = new int[WinW, WinH];
            FormCtrlF = new int[WinW, WinH];
            FormCtrlA = new int[WinW, WinH];
            FormCtrlC = new int[WinW, WinH];
            FormCtrlFontW = new int[WinW, WinH];
            FormCtrlFontH = new int[WinW, WinH];
            LineOffset = new int[WinH];
            LineOffsetBlank = new bool[WinH];
            LineOffsetBack = new int[WinH];
            LineOffsetFore = new int[WinH];
            LineOffsetAttr = new int[WinH];
            SetTextRectangles();

            for (int Y = 0; Y < WinH; Y++)
            {
                for (int X = 0; X < WinW; X++)
                {
                    FormCtrlB[X, Y] = -1;
                    FormCtrlF[X, Y] = -1;
                    FormCtrlA[X, Y] = 0;
                    FormCtrlC[X, Y] = ' ';
                    FormCtrlFontW[X, Y] = 0;
                    FormCtrlFontH[X, Y] = 0;
                }
                LineOffset[Y] = 0;
            }
            CursorB = -1;
            CursorF = -1;
            Monitor.Exit(BitmapX_mutex);
        }

        public string DummyGetScreenText(int IsANSI, int DefBack, int DefFore, int X, int Y, int W, int H)
        {
            AnsiLineOccupyEx S_ = new AnsiLineOccupyEx();
            for (int i = 0; i < H; i++)
            {
                S_.AppendLine();
            }

            StringBuilder S = new StringBuilder();
            int Y0 = 0;
            for (int YY = Y; YY < (H + Y); YY++)
            {
                if ((YY >= 0) && (YY < WinH))
                {
                    for (int XX = X; XX < (W + X); XX++)
                    {
                        S_.BlankChar();
                        if ((XX >= 0) && (XX < WinW))
                        {
                            if (FormCtrlC[XX, YY] >= 32)
                            {
                                S_.Item_Char = FormCtrlC[XX, YY];
                            }
                            else
                            {
                                S_.Item_Char = ' ';
                            }
                            S_.Item_ColorB = FormCtrlB[XX, YY];
                            S_.Item_ColorF = FormCtrlF[XX, YY];
                            S_.Item_ColorA = FormCtrlA[XX, YY];
                            if (S_.Item_ColorB == DefBack) { S_.Item_ColorB = -1; }
                            if (S_.Item_ColorF == DefFore) { S_.Item_ColorF = -1; }
                            S_.Item_FontW = FormCtrlFontW[XX, YY];
                            S_.Item_FontH = FormCtrlFontH[XX, YY];
                        }
                        S_.Append(Y0);
                    }
                    if (IsANSI == 0)
                    {
                        S_.Trim(Y0);
                    }
                }
                Y0++;
            }
            S = new StringBuilder();
            if (IsANSI == 0)
            {
                S_.TrimLines();
            }
            int LastLine = S_.CountLines() - 1;

            if (IsANSI > 0)
            {
                AnsiFile AnsiFile_ = new AnsiFile();
                AnsiFile_.Reset();
                for (int i = 0; i <= LastLine; i++)
                {
                    bool Prefix = (i == 0);
                    bool Postfix = (i == LastLine);
                    S.Append(TextWork.IntToStr(AnsiFile_.Process(S_, i, Prefix, Postfix, WinW)));
                }
            }
            else
            {
                for (int i = 0; i <= LastLine; i++)
                {
                    S.Append(TextWork.IntToStr(S_.GetLineString(i)));
                    if (i < LastLine)
                    {
                        S.Append('\r');
                        S.Append('\n');
                    }
                }
            }
            return S.ToString();
        }

        public LowLevelBitmap DummyGetScreenBitmap(bool Blink, bool DrawCursor, int DefBack, int DefFore, int X, int Y, int W, int H)
        {
            LowLevelBitmap BmpExp = BitmapX_[Blink ? 1 : 0].Clone();
            if (DrawCursor)
            {
                CursorColorR = DrawColor_R[FormCtrlF[CursorX, CursorY]];
                CursorColorG = DrawColor_G[FormCtrlF[CursorX, CursorY]];
                CursorColorB = DrawColor_B[FormCtrlF[CursorX, CursorY]];
                BmpExp.DrawRectangle(CursorX * CellW, CursorY * CellH + CursorDispOffset, CellW, CursorThick, CursorColorR, CursorColorG, CursorColorB);
            }
            byte Draw_R = DrawColor_R[DefBack];
            byte Draw_G = DrawColor_G[DefBack];
            byte Draw_B = DrawColor_B[DefBack];
            LowLevelBitmap BmpScr = new LowLevelBitmap(W * CellW, H * CellH, Draw_R, Draw_G, Draw_B);
            BmpScr.DrawImageCtrl(BmpExp, 0, 0, 0 - (X * CellW), 0 - (Y * CellH), BmpExp.Width, BmpExp.Height);
            return BmpScr;
        }

        protected bool WindowResizeForce = false;

        public virtual int FormGetWidth()
        {
            return 0;
        }

        public virtual int FormGetHeight()
        {
            return 0;
        }

        public virtual void FormCtrlSetBitmap(LowLevelBitmap Bmp0, LowLevelBitmap Bmp1)
        {
        }

        public override void AppResize(int NewW, int NewH)
        {
            if (WinFixed > 0)
            {
                WinW = NewW;
                WinH = NewH;
                WindowResizeForce = true;
                Core_.CoreEvent("Resize", '\0', false, false, false);
            }
        }

        public override bool WindowResize()
        {
            if ((InternalW == FormGetWidth()) && (InternalH == FormGetHeight()))
            {
                if (!WindowResizeForce)
                {
                    return false;
                }
            }

            if (Monitor.TryEnter(GraphMutex, 200))
            {
                if ((WinFixed > 0) && (WinW > 0) && (WinH > 0))
                {
                }
                else
                {
                    WinW = FormGetWidth() / CellW;
                    WinH = FormGetHeight() / CellH;
                }

                if (WinCursorPosX.Length <= WinW)
                {
                    WinCursorPosX = new int[WinW + 1];
                }
                if (WinCursorPosY.Length <= WinH)
                {
                    WinCursorPosY = new int[WinH + 1];
                }

                BitmapX_[0] = new LowLevelBitmap((WinW * CellW) + ScreenBorder + ScreenBorder, (WinH * CellH) + ScreenBorder + ScreenBorder, 255);
                BitmapX_[1] = new LowLevelBitmap((WinW * CellW) + ScreenBorder + ScreenBorder, (WinH * CellH) + ScreenBorder + ScreenBorder, 255);

                FormCtrlB = new int[WinW, WinH];
                FormCtrlF = new int[WinW, WinH];
                FormCtrlA = new int[WinW, WinH];
                FormCtrlC = new int[WinW, WinH];
                FormCtrlFontW = new int[WinW, WinH];
                FormCtrlFontH = new int[WinW, WinH];
                LineOffset = new int[WinH];
                LineOffsetBlank = new bool[WinH];
                LineOffsetBack = new int[WinH];
                LineOffsetFore = new int[WinH];
                LineOffsetAttr = new int[WinH];
                SetTextRectangles();

                int ScreenWidth = 0;
                int ScreenHeight = 0;
                int ScreenLeft = 0;
                int ScreenTop = 0;

                int FormW = FormGetWidth();
                int FormH = FormGetHeight();
                int DispCursorSize = (CursorThick * FormH) / BitmapX_[0].Height;
                if (WinFixed < 2)
                {
                    for (int i = 0; i <= WinW; i++)
                    {
                        WinCursorPosX[i] = i * CellW;
                    }
                    for (int i = 0; i <= WinH; i++)
                    {
                        WinCursorPosY[i] = i * CellH + CursorDispOffset;
                    }
                    ScreenWidth = BitmapX_[0].Width;
                    ScreenHeight = BitmapX_[0].Height;
                    ScreenLeft = (FormW - BitmapX_[0].Width) / 2;
                    ScreenTop = (FormH - BitmapX_[0].Height) / 2;
                }
                else
                {
                    for (int i = 0; i <= WinW; i++)
                    {
                        WinCursorPosX[i] = (((i * CellW + CellW) * FormW) / BitmapX_[0].Width);
                        while ((((WinCursorPosX[i]) * BitmapX_[0].Width) / FormW) >= (i * CellW))
                        {
                            WinCursorPosX[i]--;
                        }
                        WinCursorPosX[i]++;
                    }
                    for (int i = 0; i <= WinH; i++)
                    {
                        WinCursorPosY[i] = (((i * CellH + CellH + CellH) * FormH) / BitmapX_[0].Height);
                        while ((((WinCursorPosY[i]) * BitmapX_[0].Height) / FormH) >= (i * CellH + CellH))
                        {
                            WinCursorPosY[i]--;
                        }
                        WinCursorPosY[i]++;
                        WinCursorPosY[i] -= DispCursorSize;
                    }
                    ScreenWidth = FormW;
                    ScreenHeight = FormH;
                    ScreenLeft = 0;
                    ScreenTop = 0;
                }
                FormCtrlSetBitmap(BitmapX_[0], BitmapX_[1]);
                FormCtrlSetParam(0, ScreenLeft);
                FormCtrlSetParam(1, ScreenTop);
                FormCtrlSetParam(2, ScreenWidth);
                FormCtrlSetParam(3, ScreenHeight);
                FormCtrlRefresh();

                ScreenLeft = 0 - CellW - CellW;
                ScreenTop = 0 - CellH - CellH;
                if (WinFixed < 2)
                {
                    ScreenWidth = CellW;
                    ScreenHeight = CursorThick;
                }
                else
                {
                    ScreenWidth = (CellW * FormW) / BitmapX_[0].Width;
                    ScreenHeight = DispCursorSize;
                }
                FormCtrlSetParam(4, ScreenLeft);
                FormCtrlSetParam(5, ScreenTop);
                FormCtrlSetParam(6, ScreenWidth);
                FormCtrlSetParam(7, ScreenHeight);

                for (int Y = 0; Y < WinH; Y++)
                {
                    for (int X = 0; X < WinW; X++)
                    {
                        FormCtrlB[X, Y] = Core_.TextNormalBack;
                        FormCtrlF[X, Y] = Core_.TextNormalFore;
                        FormCtrlA[X, Y] = 0;
                        FormCtrlC[X, Y] = -1;
                        FormCtrlFontW[X, Y] = 0;
                        FormCtrlFontH[X, Y] = 0;
                    }
                    LineOffset[Y] = 0;
                }
                CursorB = -1;
                CursorF = -1;
                CursorNeedRepaint = true;

                InternalW = FormGetWidth();
                InternalH = FormGetHeight();

                WindowResizeForce = false;
                Monitor.Exit(GraphMutex);
                return true;
            }
            else
            {
                WindowResizeForce = true;
                return false;
            }
        }


        protected string KeyCode_ = "";
        protected bool KeyShift_ = false;
        protected bool KeyCtrl_ = false;
        protected bool KeyAlt_ = false;

        protected virtual void FormClose()
        {

        }

        protected void CoreEvent(string KeyName, char KeyChar, bool ModShift, bool ModCtrl, bool ModAlt)
        {
            Core_.CoreEvent(UniKeyName(KeyName), KeyChar, ModShift, ModCtrl, ModAlt);
            if (!AppWorking)
            {
                FormAllowClose = true;
                FormClose();
            }
        }

        public override void SetLineOffset(int Y, int Offset, bool Blank, int ColorBack, int ColorFore, int FontAttr)
        {
            if ((Y < 0) || (Y >= WinH))
            {
                return;
            }
            int OldOffset = LineOffset[Y];
            switch (CellH)
            {
                case 8:
                    LineOffset[Y] = Offset;
                    break;
                case 16:
                    LineOffset[Y] = Offset << 1;
                    break;
                case 24:
                    LineOffset[Y] = Offset + Offset + Offset;
                    break;
                case 32:
                    LineOffset[Y] = Offset << 2;
                    break;
                default:
                    LineOffset[Y] = (Offset * CellH) / 8;
                    break;
            }
            LineOffsetBlank[Y] = Blank;
            LineOffsetBack[Y] = ColorBack;
            LineOffsetFore[Y] = ColorFore;
            LineOffsetAttr[Y] = FontAttr;
            if (OldOffset != LineOffset[Y])
            {
                for (int X = 0; X < WinW; X++)
                {
                    CharRepaint(X, Y);
                }
            }
        }

        public override void RepaintOffset(int Y)
        {
            if (LineOffset[Y] != 0)
            {
                for (int X = 0; X < WinW; X++)
                {
                    CharRepaint(X, Y);
                }
            }
        }

        public override void Clear(int ColorB, int ColorF)
        {
            Monitor.Enter(GraphMutex);
            CalcColor(ColorB, ColorF, 0);
            for (int Y = 0; Y < WinH; Y++)
            {
                SetLineOffset(Y, 0, false, ColorB, ColorF, 0);
                for (int X = 0; X < WinW; X++)
                {
                    PutChar_Work(X, Y, 32, CalcColor_Back, CalcColor_Fore, 0, 0, 0);
                }
            }
            Monitor.Exit(GraphMutex);
        }

        protected override void PutChar_(int X, int Y, int C, int ColorBack, int ColorFore, int FontW, int FontH, int FontAttr)
        {
            Monitor.Enter(GraphMutex);
            PutChar_Work(X, Y, C, ColorBack, ColorFore, FontW, FontH, FontAttr);
            Monitor.Exit(GraphMutex);
        }

        public override void CharRepaint(int X, int Y)
        {
            int C = FormCtrlC[X, Y];
            FormCtrlC[X, Y] = C + 1;
            PutChar_Work(X, Y, C, FormCtrlB[X, Y], FormCtrlF[X, Y], FormCtrlFontW[X, Y], FormCtrlFontH[X, Y], FormCtrlA[X, Y]);
        }

        private int CalcFontBIU(int FontAttr)
        {
            return (((((FontAttr & 1) > 0) ? 1 : 0) * FontModeBold) + ((((FontAttr & 2) > 0) ? 2 : 0) * FontModeItalic) + ((((FontAttr & 4) > 0) ? 4 : 0) * FontModeUnderline) + ((((FontAttr & 64) > 0) ? 8 : 0) * FontModeStrike)) << 7;
        }

        private void PutChar_Work(int X, int Y, int C, int ColorBack, int ColorFore, int FontW, int FontH, int ColorAttr)
        {
            bool Diff = false;
            if (FormCtrlC[X, Y] != C)
            {
                FormCtrlC[X, Y] = C;
                Diff = true;
            }
            if (FormCtrlB[X, Y] != ColorBack)
            {
                FormCtrlB[X, Y] = ColorBack;
                Diff = true;
            }
            if (FormCtrlF[X, Y] != ColorFore)
            {
                FormCtrlF[X, Y] = ColorFore;
                Diff = true;
            }
            if (FormCtrlA[X, Y] != ColorAttr)
            {
                FormCtrlA[X, Y] = ColorAttr;
                Diff = true;
            }
            if (FormCtrlFontW[X, Y] != FontW)
            {
                FormCtrlFontW[X, Y] = FontW;
                Diff = true;
            }
            if (FormCtrlFontH[X, Y] != FontH)
            {
                FormCtrlFontH[X, Y] = FontH;
                Diff = true;
            }
            if (Diff)
            {
                int FontAttrBIU = CalcFontBIU(ColorAttr);
                CalcBlink(ColorBack, ColorFore, ColorAttr);
                LowLevelBitmap TempGlyph0 = PutChar_GetGlyph(ColorBack, ColorFore, C, FontW, FontH, FontAttrBIU);
                LowLevelBitmap TempGlyph1 = PutChar_GetGlyph(CalcBlink_Back, CalcBlink_Fore, C, FontW, FontH, FontAttrBIU);

                int OffsetMode = 0;
                bool OffsetOtherHalf = false;

                if (LineOffset[Y] > 0)
                {
                    OffsetMode = 1;
                    if ((Y < (WinH - 1)) && (!LineOffsetBlank[Y]))
                    {
                        if ((FormCtrlB[X, Y + 1] >= 0) && (FormCtrlF[X, Y + 1] >= 0))
                        {
                            OffsetOtherHalf = true;
                        }
                    }
                }
                if (LineOffset[Y] < 0)
                {
                    OffsetMode = 2;
                    if ((Y > 0) && (!LineOffsetBlank[Y]))
                    {
                        if ((FormCtrlB[X, Y - 1] >= 0) && (FormCtrlF[X, Y - 1] >= 0))
                        {
                            OffsetOtherHalf = true;
                        }
                    }
                }

                if (MultiThread) Monitor.Enter(BitmapX_mutex);

                if (OffsetMode == 0)
                {
                    BitmapX_[0].DrawImage(TempGlyph0, 0, 0, (X * CellW) + ScreenBorder, (Y * CellH) + ScreenBorder, TempGlyph0.Width, TempGlyph0.Height);
                    BitmapX_[1].DrawImage(TempGlyph1, 0, 0, (X * CellW) + ScreenBorder, (Y * CellH) + ScreenBorder, TempGlyph1.Width, TempGlyph1.Height);
                }
                else
                {
                    if (OffsetMode == 1)
                    {
                        int Offset = LineOffset[Y];

                        // Current line - upper half
                        BitmapX_[0].DrawImage(TempGlyph0, 0, Offset, (X * CellW) + ScreenBorder, (Y * CellH) + ScreenBorder, CellW, CellH - Offset);
                        BitmapX_[1].DrawImage(TempGlyph1, 0, Offset, (X * CellW) + ScreenBorder, (Y * CellH) + ScreenBorder, CellW, CellH - Offset);
                        if (OffsetOtherHalf)
                        {
                            CalcBlink(FormCtrlB[X, Y + 1], FormCtrlF[X, Y + 1], FormCtrlA[X, Y + 1]);
                            TempGlyph0 = PutChar_GetGlyph(FormCtrlB[X, Y + 1], FormCtrlF[X, Y + 1], FormCtrlC[X, Y + 1], FormCtrlFontW[X, Y + 1], FormCtrlFontH[X, Y + 1], CalcFontBIU(FormCtrlA[X, Y + 1]));
                            TempGlyph1 = PutChar_GetGlyph(CalcBlink_Back, CalcBlink_Fore, FormCtrlC[X, Y + 1], FormCtrlFontW[X, Y + 1], FormCtrlFontH[X, Y + 1], CalcFontBIU(FormCtrlA[X, Y + 1]));
                        }
                        else
                        {
                            CalcColor(LineOffsetBack[Y], LineOffsetFore[Y], LineOffsetAttr[Y]);
                            CalcBlink(CalcColor_Back, CalcColor_Fore, LineOffsetAttr[Y]);
                            TempGlyph0 = PutChar_GetGlyph(CalcColor_Back, CalcColor_Fore, 32, FormCtrlFontW[X, Y], FormCtrlFontH[X, Y], CalcFontBIU(LineOffsetAttr[Y]));
                            TempGlyph1 = PutChar_GetGlyph(CalcBlink_Back, CalcBlink_Fore, 32, FormCtrlFontW[X, Y], FormCtrlFontH[X, Y], CalcFontBIU(LineOffsetAttr[Y]));
                        }
                        // Current line - lower half
                        BitmapX_[0].DrawImage(TempGlyph0, 0, 0, (X * CellW) + ScreenBorder, (Y * CellH + CellH - Offset) + ScreenBorder, CellW, Offset);
                        BitmapX_[1].DrawImage(TempGlyph1, 0, 0, (X * CellW) + ScreenBorder, (Y * CellH + CellH - Offset) + ScreenBorder, CellW, Offset);
                    }
                    if (OffsetMode == 2)
                    {
                        int Offset = CellH - (0 - LineOffset[Y]);

                        // Current line - lower half
                        BitmapX_[0].DrawImage(TempGlyph0, 0, 0, (X * CellW) + ScreenBorder, (Y * CellH + CellH - Offset) + ScreenBorder, CellW, Offset);
                        BitmapX_[1].DrawImage(TempGlyph1, 0, 0, (X * CellW) + ScreenBorder, (Y * CellH + CellH - Offset) + ScreenBorder, CellW, Offset);
                        if (OffsetOtherHalf)
                        {
                            CalcBlink(FormCtrlB[X, Y - 1], FormCtrlF[X, Y - 1], FormCtrlA[X, Y - 1]);
                            TempGlyph0 = PutChar_GetGlyph(FormCtrlB[X, Y - 1], FormCtrlF[X, Y - 1], FormCtrlC[X, Y - 1], FormCtrlFontW[X, Y - 1], FormCtrlFontH[X, Y - 1], CalcFontBIU(FormCtrlA[X, Y - 1]));
                            TempGlyph1 = PutChar_GetGlyph(CalcBlink_Back, CalcBlink_Fore, FormCtrlC[X, Y - 1], FormCtrlFontW[X, Y - 1], FormCtrlFontH[X, Y - 1], CalcFontBIU(FormCtrlA[X, Y - 1]));
                        }
                        else
                        {
                            CalcColor(LineOffsetBack[Y], LineOffsetFore[Y], LineOffsetAttr[Y]);
                            CalcBlink(CalcColor_Back, CalcColor_Fore, LineOffsetAttr[Y]);
                            TempGlyph0 = PutChar_GetGlyph(CalcColor_Back, CalcColor_Fore, 32, FormCtrlFontW[X, Y], FormCtrlFontH[X, Y], CalcFontBIU(LineOffsetAttr[Y]));
                            TempGlyph1 = PutChar_GetGlyph(CalcBlink_Back, CalcBlink_Fore, 32, FormCtrlFontW[X, Y], FormCtrlFontH[X, Y], CalcFontBIU(LineOffsetAttr[Y]));
                        }
                        // Current line - upper half
                        BitmapX_[0].DrawImage(TempGlyph0, 0, Offset, (X * CellW) + ScreenBorder, (Y * CellH) + ScreenBorder, CellW, CellH - Offset);
                        BitmapX_[1].DrawImage(TempGlyph1, 0, Offset, (X * CellW) + ScreenBorder, (Y * CellH) + ScreenBorder, CellW, CellH - Offset);
                    }
                }

                if (MultiThread) Monitor.Exit(BitmapX_mutex);
            }
        }

        private LowLevelBitmap PutChar_GetGlyph(int ColorBack, int ColorFore, int C, int FontW, int FontH, int FontBIU)
        {
            byte DrawBack_R = DrawColor_R[ColorBack];
            byte DrawBack_G = DrawColor_G[ColorBack];
            byte DrawBack_B = DrawColor_B[ColorBack];
            byte DrawFore_R = DrawColor_R[ColorFore];
            byte DrawFore_G = DrawColor_G[ColorFore];
            byte DrawFore_B = DrawColor_B[ColorFore];
            int BlendIdx = -1;
            int C_ = C;
            if (ColorBlending)
            {
                BlendIdx = ColorBlendingChar.IndexOf(C);
                if (BlendIdx >= 0)
                {
                    C = ColorBlendingReplacement[BlendIdx];

                    if (ColorBlendingBackground[BlendIdx])
                    {
                        CalcBlend(ColorBack, ColorFore, ColorBlendingProp1[BlendIdx], ColorBlendingProp2[BlendIdx], out DrawBack_R, out DrawBack_G, out DrawBack_B);
                    }
                    else
                    {
                        CalcBlend(ColorBack, ColorFore, ColorBlendingProp1[BlendIdx], ColorBlendingProp2[BlendIdx], out DrawFore_R, out DrawFore_G, out DrawFore_B);
                    }
                }
            }
            int FontWH = ((FontW <= MaxFontSize) && (FontH <= MaxFontSize)) ? (FontH * (MaxFontSize + 1) + FontW) : -1;
            LowLevelBitmap TempGlyph = GlyphBankGet(ColorBack, ColorFore, C_, FontWH, FontBIU);
            if (TempGlyph == null)
            {
                bool IsFontB = (FontBIU & 128) > 0;
                bool IsFontI = (FontBIU & 256) > 0;
                bool IsFontU = (FontBIU & 512) > 0;
                bool IsFontS = (FontBIU & 1024) > 0;
                if (WinIsBitmapFont)
                {
                    TempGlyph = new LowLevelBitmap(CellW, CellH, 0);
                    int C__ = C;
                    int CP_ = C__ >> 8;
                    C__ = C__ & 255;
                    C__ = C__ * CellW;
                    int CPI = 0;
                    if (WinBitmapPage.ContainsKey(CP_))
                    {
                        CPI = WinBitmapPage[CP_] * CellH;
                    }
                    else
                    {
                        C__ = 32 * CellW;
                    }
                    int FontDivW = 1;
                    int FontDivH = 1;
                    int FontOffW = 0;
                    int FontOffH = 0;
                    if (FontW > 0)
                    {
                        for (int i = 1; i < FontW_Num_Min.Length; i++)
                        {
                            if ((FontW >= FontW_Num_Min[i]) && (FontW <= FontW_Num_Max[i]))
                            {
                                FontDivW = i;
                                FontOffW = (FontW - FontW_Num_Min[i]) * CellW;
                                break;
                            }
                        }
                    }
                    if (FontH >= 0)
                    {
                        for (int i = 1; i < FontW_Num_Min.Length; i++)
                        {
                            if ((FontH >= FontW_Num_Min[i]) && (FontH <= FontW_Num_Max[i]))
                            {
                                FontDivH = i;
                                FontOffH = (FontH - FontW_Num_Min[i]) * CellH;
                                break;
                            }
                        }
                    }

                    for (int Y_ = 0; Y_ < CellH; Y_++)
                    {
                        int PxlY = ((Y_ + FontOffH) / FontDivH);
                        if (IsFontB)
                        {
                            for (int X_ = 0; X_ < CellW; X_++)
                            {
                                int PxlX = ((X_ + FontOffW) / FontDivW);
                                if (PxlX >= CellW_F)
                                {
                                    if ((WinBitmapGlyph[CPI + PxlY, C__ + PxlX]) || (WinBitmapGlyph[CPI + PxlY, C__ + PxlX - CellW_F]))
                                    {
                                        TempGlyph.SetPixel(X_, Y_, DrawFore_R, DrawFore_G, DrawFore_B);
                                    }
                                    else
                                    {
                                        TempGlyph.SetPixel(X_, Y_, DrawBack_R, DrawBack_G, DrawBack_B);
                                    }
                                }
                                else
                                {
                                    if ((WinBitmapGlyph[CPI + PxlY, C__ + PxlX]))
                                    {
                                        TempGlyph.SetPixel(X_, Y_, DrawFore_R, DrawFore_G, DrawFore_B);
                                    }
                                    else
                                    {
                                        TempGlyph.SetPixel(X_, Y_, DrawBack_R, DrawBack_G, DrawBack_B);
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int X_ = 0; X_ < CellW; X_++)
                            {
                                int PxlX = ((X_ + FontOffW) / FontDivW);
                                if (WinBitmapGlyph[CPI + PxlY, C__ + PxlX])
                                {
                                    TempGlyph.SetPixel(X_, Y_, DrawFore_R, DrawFore_G, DrawFore_B);
                                }
                                else
                                {
                                    TempGlyph.SetPixel(X_, Y_, DrawBack_R, DrawBack_G, DrawBack_B);
                                }
                            }
                        }
                    }
                    if (IsFontI)
                    {
                        int CharHalf = (((CellH / 2) * FontDivH) - FontOffH);
                        if (CharHalf < 0) CharHalf = 0;

                        for (int Y_ = CharHalf; Y_ < CellH; Y_++)
                        {
                            int PxlY = ((Y_ + FontOffH) / FontDivH);
                            if (IsFontB)
                            {
                                for (int X_ = 0; X_ < CellW; X_++)
                                {
                                    int PxlX = ((X_ + FontOffW) / FontDivW);
                                    if (PxlX < (CellW - CellH_F))
                                    {
                                        if ((WinBitmapGlyph[CPI + PxlY, C__ + PxlX + CellH_F]) || (WinBitmapGlyph[CPI + PxlY, C__ + PxlX]))
                                        {
                                            TempGlyph.SetPixel(X_, Y_, DrawFore_R, DrawFore_G, DrawFore_B);
                                        }
                                        else
                                        {
                                            TempGlyph.SetPixel(X_, Y_, DrawBack_R, DrawBack_G, DrawBack_B);
                                        }
                                    }
                                    else
                                    {
                                        if (WinBitmapGlyph[CPI + PxlY, C__ + PxlX])
                                        {
                                            TempGlyph.SetPixel(X_, Y_, DrawFore_R, DrawFore_G, DrawFore_B);
                                        }
                                        else
                                        {
                                            TempGlyph.SetPixel(X_, Y_, DrawBack_R, DrawBack_G, DrawBack_B);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int X_ = 0; X_ < CellW; X_++)
                                {
                                    int PxlX = ((X_ + FontOffW) / FontDivW);
                                    if (PxlX < (CellW - CellH_F))
                                    {
                                        if (WinBitmapGlyph[CPI + PxlY, C__ + PxlX + CellH_F])
                                        {
                                            TempGlyph.SetPixel(X_, Y_, DrawFore_R, DrawFore_G, DrawFore_B);
                                        }
                                        else
                                        {
                                            TempGlyph.SetPixel(X_, Y_, DrawBack_R, DrawBack_G, DrawBack_B);
                                        }
                                    }
                                    else
                                    {
                                        TempGlyph.SetPixel(X_, Y_, DrawBack_R, DrawBack_G, DrawBack_B);
                                    }
                                }
                            }
                        }
                    }
                    if (IsFontU)
                    {
                        int T1 = (CellH - CellH_F) * FontDivH - FontOffH;
                        int T2 = (T1 + (CellH_F * FontDivH));
                        if ((T1 >= 0) && (T2 <= CellH))
                        {
                            for (int Y_ = T1; Y_ < T2; Y_++)
                            {
                                for (int X_ = 0; X_ < CellW; X_++)
                                {
                                    TempGlyph.SetPixel(X_, Y_, DrawFore_R, DrawFore_G, DrawFore_B);
                                }
                            }
                        }
                    }
                    if (IsFontS)
                    {
                        int T1 = ((CellH / 2) - CellH_F) * FontDivH - FontOffH;
                        int T2 = (T1 + (CellH_F * FontDivH));
                        if ((T1 >= 0) && (T2 <= CellH))
                        {
                            for (int Y_ = T1; Y_ < T2; Y_++)
                            {
                                for (int X_ = 0; X_ < CellW; X_++)
                                {
                                    TempGlyph.SetPixel(X_, Y_, DrawFore_R, DrawFore_G, DrawFore_B);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Glyph_.DrawRectangle(0, 0, CellW, CellH, DrawBack_R, DrawBack_G, DrawBack_B);
                    if (((C >= 0x20) && (C < 0xD800)) || (C > 0xDFFF))
                    {
                        Glyph_.DrawText(FormCtrlR_X[FontW], FormCtrlR_Y[FontH], FormCtrlR_W[FontW], FormCtrlR_H[FontH], FormCtrlR_Trans[FontW], FormCtrlR_Trans[FontH], char.ConvertFromUtf32(C), DrawFore_R, DrawFore_G, DrawFore_B, IsFontB, IsFontI, IsFontU, IsFontS);
                    }
                    TempGlyph = Glyph_.Clone();
                }
                GlyphBankSet(ColorBack, ColorFore, C_, TempGlyph, FontWH, FontBIU);
            }
            return TempGlyph;
        }

        public override void Move(int SrcX, int SrcY, int DstX, int DstY, int W, int H)
        {
            while ((SrcX < 0) || (DstX < 0))
            {
                SrcX++;
                DstX++;
                W--;
            }
            while ((SrcY < 0) || (DstY < 0))
            {
                SrcY++;
                DstY++;
                H--;
            }
            while (((SrcX + W) > WinW) || ((DstX + W) > WinW))
            {
                W--;
            }
            while (((SrcY + H) > WinH) || ((DstY + H) > WinH))
            {
                H--;
            }
            Monitor.Enter(GraphMutex);
            BitmapX_[0].DrawImage(BitmapX_[0], (SrcX * CellW) + ScreenBorder, (SrcY * CellH) + ScreenBorder, (DstX * CellW) + ScreenBorder, (DstY * CellH) + ScreenBorder, W * CellW, H * CellH);
            BitmapX_[1].DrawImage(BitmapX_[1], (SrcX * CellW) + ScreenBorder, (SrcY * CellH) + ScreenBorder, (DstX * CellW) + ScreenBorder, (DstY * CellH) + ScreenBorder, W * CellW, H * CellH);
            int X_D, Y_D, X_S, Y_S;
            if (SrcY > DstY)
            {
                for (int Y = 0; Y < H; Y++)
                {
                    if (SrcX > DstX)
                    {
                        for (int X = 0; X < W; X++)
                        {
                            X_S = X + SrcX;
                            Y_S = Y + SrcY;
                            X_D = X + DstX;
                            Y_D = Y + DstY;
                            FormCtrlC[X_D, Y_D] = FormCtrlC[X_S, Y_S];
                            FormCtrlB[X_D, Y_D] = FormCtrlB[X_S, Y_S];
                            FormCtrlF[X_D, Y_D] = FormCtrlF[X_S, Y_S];
                            FormCtrlA[X_D, Y_D] = FormCtrlA[X_S, Y_S];
                            FormCtrlFontW[X_D, Y_D] = FormCtrlFontW[X_S, Y_S];
                            FormCtrlFontH[X_D, Y_D] = FormCtrlFontH[X_S, Y_S];
                        }
                    }
                    else
                    {
                        for (int X = (W - 1); X >= 0; X--)
                        {
                            X_S = X + SrcX;
                            Y_S = Y + SrcY;
                            X_D = X + DstX;
                            Y_D = Y + DstY;
                            FormCtrlC[X_D, Y_D] = FormCtrlC[X_S, Y_S];
                            FormCtrlB[X_D, Y_D] = FormCtrlB[X_S, Y_S];
                            FormCtrlF[X_D, Y_D] = FormCtrlF[X_S, Y_S];
                            FormCtrlA[X_D, Y_D] = FormCtrlA[X_S, Y_S];
                            FormCtrlFontW[X_D, Y_D] = FormCtrlFontW[X_S, Y_S];
                            FormCtrlFontH[X_D, Y_D] = FormCtrlFontH[X_S, Y_S];
                        }
                    }
                }
            }
            else
            {
                for (int Y = (H - 1); Y >= 0; Y--)
                {
                    if (SrcX > DstX)
                    {
                        for (int X = 0; X < W; X++)
                        {
                            X_S = X + SrcX;
                            Y_S = Y + SrcY;
                            X_D = X + DstX;
                            Y_D = Y + DstY;
                            FormCtrlC[X_D, Y_D] = FormCtrlC[X_S, Y_S];
                            FormCtrlB[X_D, Y_D] = FormCtrlB[X_S, Y_S];
                            FormCtrlF[X_D, Y_D] = FormCtrlF[X_S, Y_S];
                            FormCtrlA[X_D, Y_D] = FormCtrlA[X_S, Y_S];
                            FormCtrlFontW[X_D, Y_D] = FormCtrlFontW[X_S, Y_S];
                            FormCtrlFontH[X_D, Y_D] = FormCtrlFontH[X_S, Y_S];
                        }
                    }
                    else
                    {
                        for (int X = (W - 1); X >= 0; X--)
                        {
                            X_S = X + SrcX;
                            Y_S = Y + SrcY;
                            X_D = X + DstX;
                            Y_D = Y + DstY;
                            FormCtrlC[X_D, Y_D] = FormCtrlC[X_S, Y_S];
                            FormCtrlB[X_D, Y_D] = FormCtrlB[X_S, Y_S];
                            FormCtrlF[X_D, Y_D] = FormCtrlF[X_S, Y_S];
                            FormCtrlA[X_D, Y_D] = FormCtrlA[X_S, Y_S];
                            FormCtrlFontW[X_D, Y_D] = FormCtrlFontW[X_S, Y_S];
                            FormCtrlFontH[X_D, Y_D] = FormCtrlFontH[X_S, Y_S];
                        }
                    }
                }
            }
            if ((LineOffset[SrcY] != 0) || (LineOffset[DstY] != 0) || (LineOffset[SrcY + H - 1] != 0) || (LineOffset[DstY + H - 1] != 0))
            {
                int RepaintX1 = Math.Min(SrcX, DstX);
                int RepaintX2 = Math.Max(SrcX, DstX) + W;
                for (int X = RepaintX1; X < RepaintX2; X++)
                {
                    int Y = SrcY;
                    CharRepaint(X, SrcY);
                    CharRepaint(X, DstY);
                    CharRepaint(X, SrcY + H - 1);
                    CharRepaint(X, DstY + H - 1);
                }
            }
            Monitor.Exit(GraphMutex);
        }

        public override void SetCursorPositionNoRefresh(int X, int Y)
        {
            Monitor.Enter(GraphMutex);
            if (AppWorking)
            {
                if (X >= WinW)
                {
                    X = WinW - 1;
                }
                if (Y >= WinH)
                {
                    Y = WinH - 1;
                }
                if (X < 0)
                {
                    X = 0;
                }
                if (Y < 0)
                {
                    Y = 0;
                }
                CursorX = X;
                CursorY = Y;
            }
            Monitor.Exit(GraphMutex);
        }

        public override void SetCursorPosition(int X, int Y)
        {
            Monitor.Enter(GraphMutex);
            if (AppWorking)
            {
                if (X >= WinW)
                {
                    X = WinW - 1;
                }
                if (Y >= WinH)
                {
                    Y = WinH - 1;
                }
                if (X < 0)
                {
                    X = 0;
                }
                if (Y < 0)
                {
                    Y = 0;
                }
                CursorX = X;
                CursorY = Y;
                if (CursorB != FormCtrlB[CursorX, CursorY])
                {
                    CursorNeedRepaint = true;
                    CursorB = FormCtrlB[CursorX, CursorY];
                }
                if (CursorF != FormCtrlF[CursorX, CursorY])
                {
                    CursorNeedRepaint = true;
                    CursorF = FormCtrlF[CursorX, CursorY];
                }
                if (MultiThread)
                {
                    try
                    {
                        RefreshFuncCtrl();
                    }
                    catch
                    {

                    }
                }
                else
                {
                    RefreshFunc();
                }
            }
            Monitor.Exit(GraphMutex);
        }

        protected virtual int FormCtrlGetParam(int Param)
        {
            return 0;
        }

        protected virtual void FormCtrlSetParam(int Param, int Value)
        {

        }

        protected virtual void RefreshFuncCtrl()
        {

        }

        protected void RefreshFunc()
        {
            FormCtrlSetParam(4, FormCtrlGetParam(0) + WinCursorPosX[CursorX] + ScreenBorder);
            FormCtrlSetParam(5, FormCtrlGetParam(1) + WinCursorPosY[CursorY] + ScreenBorder);
            FormCtrlSetParam(6, WinCursorPosX[CursorX + 1] - WinCursorPosX[CursorX]);
            FormCtrlRefresh();
        }

        protected void CursorTimerEvent(bool ForceResize)
        {
            if (Monitor.TryEnter(GraphMutex))
            {
                if (CursorNeedRepaint)
                {
                    try
                    {
                        CursorColorR = DrawColor_R[FormCtrlF[CursorX, CursorY]];
                        CursorColorG = DrawColor_G[FormCtrlF[CursorX, CursorY]];
                        CursorColorB = DrawColor_B[FormCtrlF[CursorX, CursorY]];
                        FormCtrlSetColor(CursorColorR, CursorColorG, CursorColorB);
                        FormCtrlSetParam(4, FormCtrlGetParam(0) + WinCursorPosX[CursorX] + ScreenBorder);
                        FormCtrlSetParam(5, FormCtrlGetParam(1) + WinCursorPosY[CursorY] + ScreenBorder);
                        FormCtrlSetParam(6, WinCursorPosX[CursorX + 1] - WinCursorPosX[CursorX]);
                        CursorNeedRepaint = false;
                    }
                    catch
                    {

                    }
                }
                if (SteadyCursor)
                {
                    FormCtrlSetParam(8, 1);
                }
                else
                {
                    FormCtrlSetParam(8, -1);
                }
                Monitor.Exit(GraphMutex);
            }
            if (WindowResizeForce || ForceResize)
            {
                CoreEvent("Resize", '\0', false, false, false);
                FormCtrlRefresh();
            }
        }
    }
}