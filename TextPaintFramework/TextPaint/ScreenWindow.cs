/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-07
 * Time: 23:03
 * 
 */
using System;
using System.Collections.Generic;
using System.Drawing;
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


        LowLevelBitmap BitmapX_;

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

        LowLevelBitmap GlyphBankGet(int ColorB, int ColorF, int Char, int FontWH)
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
            long Idx = Char + (ColorB << 20) + (ColorF << 24);
            if (GlyphBankX[FontWH].ContainsKey(Idx))
            {
                return GlyphBankX[FontWH][Idx];
            }
            else
            {
                return null;
            }
        }

        void GlyphBankSet(int ColorB, int ColorF, int Char, LowLevelBitmap Glyph, int FontWH)
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
            long Idx = Char + (ColorB << 20) + (ColorF << 24);
            GlyphBankX[FontWH].Add(Idx, Glyph);
        }

        byte[] DrawColor_R;
        byte[] DrawColor_G;
        byte[] DrawColor_B;

        int[] FontW_Num_Min;
        int[] FontW_Num_Max;

        float GetRectPos(int FontSize, float CellSize)
        {
            float FontSize_ = 0;
            if ((FontSize >= 1) && (FontSize <= 2)) { FontSize_ = (FontSize - 1); return 0 - ((CellSize * FontSize_) / 2.0f); }
            if ((FontSize >= 3) && (FontSize <= 5)) { FontSize_ = (FontSize - 3); return 0 - ((CellSize * FontSize_) / 3.0f); }
            if ((FontSize >= 6) && (FontSize <= 9)) { FontSize_ = (FontSize - 6); return 0 - ((CellSize * FontSize_) / 4.0f); }
            if ((FontSize >= 10) && (FontSize <= 14)) { FontSize_ = (FontSize - 10); return 0 - ((CellSize * FontSize_) / 5.0f); }
            if ((FontSize >= 15) && (FontSize <= 20)) { FontSize_ = (FontSize - 15); return 0 - ((CellSize * FontSize_) / 6.0f); }
            if ((FontSize >= 21) && (FontSize <= 27)) { FontSize_ = (FontSize - 21); return 0 - ((CellSize * FontSize_) / 7.0f); }
            if ((FontSize >= 28) && (FontSize <= 35)) { FontSize_ = (FontSize - 28); return 0 - ((CellSize * FontSize_) / 8.0f); }
            if ((FontSize >= 36) && (FontSize <= 44)) { FontSize_ = (FontSize - 36); return 0 - ((CellSize * FontSize_) / 9.0f); }
            if ((FontSize >= 45) && (FontSize <= 54)) { FontSize_ = (FontSize - 45); return 0 - ((CellSize * FontSize_) / 10.0f); }
            if ((FontSize >= 55) && (FontSize <= 65)) { FontSize_ = (FontSize - 55); return 0 - ((CellSize * FontSize_) / 11.0f); }
            if ((FontSize >= 66) && (FontSize <= 77)) { FontSize_ = (FontSize - 66); return 0 - ((CellSize * FontSize_) / 12.0f); }
            if ((FontSize >= 78) && (FontSize <= 90)) { FontSize_ = (FontSize - 78); return 0 - ((CellSize * FontSize_) / 13.0f); }
            if ((FontSize >= 91) && (FontSize <= 104)) { FontSize_ = (FontSize - 91); return 0 - ((CellSize * FontSize_) / 14.0f); }
            if ((FontSize >= 105) && (FontSize <= 119)) { FontSize_ = (FontSize - 105); return 0 - ((CellSize * FontSize_) / 15.0f); }
            if ((FontSize >= 120) && (FontSize <= 135)) { FontSize_ = (FontSize - 120); return 0 - ((CellSize * FontSize_) / 16.0f); }
            if ((FontSize >= 136) && (FontSize <= 152)) { FontSize_ = (FontSize - 136); return 0 - ((CellSize * FontSize_) / 17.0f); }
            if ((FontSize >= 153) && (FontSize <= 170)) { FontSize_ = (FontSize - 153); return 0 - ((CellSize * FontSize_) / 18.0f); }
            if ((FontSize >= 171) && (FontSize <= 189)) { FontSize_ = (FontSize - 171); return 0 - ((CellSize * FontSize_) / 19.0f); }
            if ((FontSize >= 190) && (FontSize <= 209)) { FontSize_ = (FontSize - 190); return 0 - ((CellSize * FontSize_) / 20.0f); }
            if ((FontSize >= 210) && (FontSize <= 230)) { FontSize_ = (FontSize - 210); return 0 - ((CellSize * FontSize_) / 21.0f); }
            if ((FontSize >= 231) && (FontSize <= 252)) { FontSize_ = (FontSize - 231); return 0 - ((CellSize * FontSize_) / 22.0f); }
            if ((FontSize >= 253) && (FontSize <= 275)) { FontSize_ = (FontSize - 253); return 0 - ((CellSize * FontSize_) / 23.0f); }
            if ((FontSize >= 276) && (FontSize <= 299)) { FontSize_ = (FontSize - 276); return 0 - ((CellSize * FontSize_) / 24.0f); }
            if ((FontSize >= 300) && (FontSize <= 324)) { FontSize_ = (FontSize - 300); return 0 - ((CellSize * FontSize_) / 25.0f); }
            if ((FontSize >= 325) && (FontSize <= 350)) { FontSize_ = (FontSize - 325); return 0 - ((CellSize * FontSize_) / 26.0f); }
            if ((FontSize >= 351) && (FontSize <= 377)) { FontSize_ = (FontSize - 351); return 0 - ((CellSize * FontSize_) / 27.0f); }
            if ((FontSize >= 378) && (FontSize <= 405)) { FontSize_ = (FontSize - 378); return 0 - ((CellSize * FontSize_) / 28.0f); }
            if ((FontSize >= 406) && (FontSize <= 434)) { FontSize_ = (FontSize - 406); return 0 - ((CellSize * FontSize_) / 29.0f); }
            if ((FontSize >= 435) && (FontSize <= 464)) { FontSize_ = (FontSize - 435); return 0 - ((CellSize * FontSize_) / 30.0f); }
            if ((FontSize >= 465) && (FontSize <= 495)) { FontSize_ = (FontSize - 465); return 0 - ((CellSize * FontSize_) / 31.0f); }
            if ((FontSize >= 496) && (FontSize <= 527)) { FontSize_ = (FontSize - 496); return 0 - ((CellSize * FontSize_) / 32.0f); }
            return 0;
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


            FormCtrlR = new RectangleF[MaxFontSizeDraw + 1, MaxFontSizeDraw + 1];
            FormCtrlR_Trans = new int[MaxFontSizeDraw + 1];
            for (int W = 0; W <= MaxFontSizeDraw; W++)
            {
                FormCtrlR_Trans[W] = 1;
                for (int i = 1; i < FontW_Num_Min.Length; i++)
                {
                    if ((W >= FontW_Num_Min[i]) && (W <= FontW_Num_Max[i])) FormCtrlR_Trans[W] = i;
                }
                for (int H = 0; H <= MaxFontSizeDraw; H++)
                {
                    float RectX = GetRectPos(W, CellW);
                    float RectY = GetRectPos(H, CellH);
                    FormCtrlR[W, H] = new RectangleF(RectX, RectY, CellW, CellH);
                }
            }
        }

        protected void ScreenWindowPrepare(Core Core__, int WinFixed_, ConfigFile CF, int ConsoleW, int ConsoleH, bool ColorBlending_, List<string> ColorBlendingConfig_, bool DummyScreen)
        {
            WinFixed = WinFixed_;
            if ((WinFixed == 2) || (WinFixed == 3))
            {
                ScreenBorder = 0;
            }
            GlyphBankX = new Dictionary<long, LowLevelBitmap>[(MaxFontSize + 1) * (MaxFontSize + 1)];
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

            DrawColor_R = new byte[16];
            DrawColor_G = new byte[16];
            DrawColor_B = new byte[16];
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
            string PalFile = CF.ParamGetS("WinPaletteFile");
            if (File.Exists(PalFile))
            {
                ConfigFile CF_Pal = new ConfigFile();
                CF_Pal.FileLoad(PalFile);
                PalR = CF_Pal.ParamGetS("WinPaletteR", PalR);
                PalG = CF_Pal.ParamGetS("WinPaletteG", PalG);
                PalB = CF_Pal.ParamGetS("WinPaletteB", PalB);
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

            CellW = CF.ParamGetI("WinCellW");
            CellH = CF.ParamGetI("WinCellH");
            WinFontName = CF.ParamGetS("WinFontName");
            WinFontSize = CF.ParamGetI("WinFontSize");
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
                switch (WinFontName)
                {
                    case "GenericSerif":
                        WinFont = new Font(FontFamily.GenericSerif, WinFontSize, FontStyle.Regular);
                        break;
                    case "GenericSansSerif":
                        WinFont = new Font(FontFamily.GenericSansSerif, WinFontSize, FontStyle.Regular);
                        break;
                    case "GenericMonospace":
                        WinFont = new Font(FontFamily.GenericMonospace, WinFontSize, FontStyle.Regular);
                        break;
                    default:
                        WinFont = new Font(WinFontName, WinFontSize, FontStyle.Regular);
                        break;
                }
                WinStrFormat = new StringFormat();
                WinStrFormat.LineAlignment = StringAlignment.Center;
                WinStrFormat.Alignment = StringAlignment.Center;
                WinStrFormat.Trimming = StringTrimming.None;
                WinStrFormat.FormatFlags = StringFormatFlags.NoWrap;
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

        int[,] FormCtrlB;
        int[,] FormCtrlF;
        int[,] FormCtrlC;
        int[,] FormCtrlFontW;
        int[,] FormCtrlFontH;
        public int[] LineOffset;
        int[] LineOffsetBack;
        int[] LineOffsetFore;
        public bool[] LineOffsetBlank;
        RectangleF[,] FormCtrlR;
        int[] FormCtrlR_Trans;
        Font WinFont;
        StringFormat WinStrFormat;
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
            WinW = W;
            WinH = H;

            BitmapX_ = new LowLevelBitmap(WinW * CellW, WinH * CellH, 0);

            FormCtrlB = new int[WinW, WinH];
            FormCtrlF = new int[WinW, WinH];
            FormCtrlC = new int[WinW, WinH];
            FormCtrlFontW = new int[WinW, WinH];
            FormCtrlFontH = new int[WinW, WinH];
            LineOffset = new int[WinH];
            LineOffsetBlank = new bool[WinH];
            LineOffsetBack = new int[WinH];
            LineOffsetFore = new int[WinH];
            SetTextRectangles();

            for (int Y = 0; Y < WinH; Y++)
            {
                for (int X = 0; X < WinW; X++)
                {
                    FormCtrlB[X, Y] = -1;
                    FormCtrlF[X, Y] = -1;
                    FormCtrlC[X, Y] = ' ';
                    FormCtrlFontW[X, Y] = 0;
                    FormCtrlFontH[X, Y] = 0;
                }
                LineOffset[Y] = 0;
            }
            CursorB = -1;
            CursorF = -1;
        }

        public string DummyGetScreenText(int IsANSI, int DefBack, int DefFore)
        {
            List<int>[] S_Text = new List<int>[WinH];
            List<int>[] S_Colo = new List<int>[WinH];
            List<int>[] S_Font = new List<int>[WinH];

            StringBuilder S = new StringBuilder();
            for (int Y = 0; Y < WinH; Y++)
            {
                S_Text[Y] = new List<int>();
                S_Colo[Y] = new List<int>();
                S_Font[Y] = new List<int>();
                for (int X = 0; X < WinW; X++)
                {
                    if (FormCtrlC[X, Y] >= 32)
                    {
                        S_Text[Y].Add(FormCtrlC[X, Y]);
                        int TempB = FormCtrlB[X, Y];
                        int TempF = FormCtrlF[X, Y];
                        if (TempB == DefBack) { TempB = -1; }
                        if (TempF == DefFore) { TempF = -1; }
                        S_Colo[Y].Add(Core.ColorToInt(TempB, TempF));
                        S_Font[Y].Add(Core.FontSToInt(FormCtrlFontW[X, Y], FormCtrlFontH[X, Y]));
                    }
                }
                if (IsANSI == 0)
                {
                    while ((S_Text[Y].Count > 0) && TextWork.SpaceChars.Contains(S_Text[Y][S_Text[Y].Count - 1]))
                    {
                        S_Text[Y].RemoveAt(S_Text[Y].Count - 1);
                    }
                }
            }
            S = new StringBuilder();
            int LastLine = (WinH - 1);
            if (IsANSI == 0)
            {
                while ((LastLine >= 0) && (S_Text[LastLine].Count == 0))
                {
                    LastLine--;
                }
            }

            if (IsANSI > 0)
            {
                AnsiFile AnsiFile_ = new AnsiFile();
                AnsiFile_.Reset();
                for (int i = 0; i <= LastLine; i++)
                {
                    bool Prefix = (i == 0);
                    bool Postfix = (i == LastLine);
                    S.Append(TextWork.IntToStr(AnsiFile_.Process(S_Text[i], S_Colo[i], S_Font[i], Prefix, Postfix, WinW, false, false)));
                }
            }
            else
            {
                for (int i = 0; i <= LastLine; i++)
                {
                    S.Append(TextWork.IntToStr(S_Text[i]));
                    if (i < LastLine)
                    {
                        S.Append('\r');
                        S.Append('\n');
                    }
                }
            }
            return S.ToString();
        }

        public Bitmap DummyGetScreenBitmap(bool DrawCursor)
        {
            Bitmap BmpExp = BitmapX_.ToBitmap();
            Graphics BmpExpG = Graphics.FromImage(BmpExp);
            if (DrawCursor)
            {
                CursorColorR = DrawColor_R[FormCtrlF[CursorX, CursorY]];
                CursorColorG = DrawColor_G[FormCtrlF[CursorX, CursorY]];
                CursorColorB = DrawColor_B[FormCtrlF[CursorX, CursorY]];
                Brush CursorColor_ = new SolidBrush(Color.FromArgb(CursorColorR, CursorColorG, CursorColorB));
                BmpExpG.FillRectangle(CursorColor_, CursorX * CellW, CursorY * CellH + CursorDispOffset, CellW, CursorThick);
            }
            return BmpExp;
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

        public virtual void FormCtrlSetBitmap(LowLevelBitmap Bmp)
        {
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

                BitmapX_ = new LowLevelBitmap((WinW * CellW) + ScreenBorder + ScreenBorder, (WinH * CellH) + ScreenBorder + ScreenBorder, 255);

                FormCtrlB = new int[WinW, WinH];
                FormCtrlF = new int[WinW, WinH];
                FormCtrlC = new int[WinW, WinH];
                FormCtrlFontW = new int[WinW, WinH];
                FormCtrlFontH = new int[WinW, WinH];
                LineOffset = new int[WinH];
                LineOffsetBlank = new bool[WinH];
                LineOffsetBack = new int[WinH];
                LineOffsetFore = new int[WinH];
                SetTextRectangles();

                int ScreenWidth = 0;
                int ScreenHeight = 0;
                int ScreenLeft = 0;
                int ScreenTop = 0;

                int FormW = FormGetWidth();
                int FormH = FormGetHeight();
                int DispCursorSize = (CursorThick * FormH) / BitmapX_.Height;
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
                    ScreenWidth = BitmapX_.Width;
                    ScreenHeight = BitmapX_.Height;
                    ScreenLeft = (FormW - BitmapX_.Width) / 2;
                    ScreenTop = (FormH - BitmapX_.Height) / 2;
                }
                else
                {
                    for (int i = 0; i <= WinW; i++)
                    {
                        WinCursorPosX[i] = (((i * CellW + CellW) * FormW) / BitmapX_.Width);
                        while ((((WinCursorPosX[i]) * BitmapX_.Width) / FormW) >= (i * CellW))
                        {
                            WinCursorPosX[i]--;
                        }
                        WinCursorPosX[i]++;
                    }
                    for (int i = 0; i <= WinH; i++)
                    {
                        WinCursorPosY[i] = (((i * CellH + CellH + CellH) * FormH) / BitmapX_.Height);
                        while ((((WinCursorPosY[i]) * BitmapX_.Height) / FormH) >= (i * CellH + CellH))
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
                FormCtrlSetBitmap(BitmapX_);
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
                    ScreenWidth = (CellW * FormW) / BitmapX_.Width;
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
                        FormCtrlB[X, Y] = -1;
                        FormCtrlF[X, Y] = -1;
                        FormCtrlC[X, Y] = ' ';
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

        public override void SetLineOffset(int Y, int Offset, bool Blank, int ColorBack, int ColorFore)
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

        protected override void PutChar_(int X, int Y, int C, int ColorBack, int ColorFore, int FontW, int FontH)
        {
            Monitor.Enter(GraphMutex);
            PutChar_Work(X, Y, C, ColorBack, ColorFore, FontW, FontH);
            Monitor.Exit(GraphMutex);
        }

        public override void CharRepaint(int X, int Y)
        {
            int C = FormCtrlC[X, Y];
            FormCtrlC[X, Y] = C + 1;
            PutChar_Work(X, Y, C, FormCtrlB[X, Y], FormCtrlF[X, Y], FormCtrlFontW[X, Y], FormCtrlFontH[X, Y]);
        }

        private void PutChar_Work(int X, int Y, int C, int ColorBack, int ColorFore, int FontW, int FontH)
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
                LowLevelBitmap TempGlyph = PutChar_GetGlyph(ColorBack, ColorFore, C, FontW, FontH);

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

                if (MultiThread) Monitor.Enter(BitmapX_);

                if (OffsetMode == 0)
                {
                    BitmapX_.DrawImage(TempGlyph, 0, 0, (X * CellW) + ScreenBorder, (Y * CellH) + ScreenBorder, TempGlyph.Width, TempGlyph.Height);
                }
                else
                {
                    if (OffsetMode == 1)
                    {
                        int Offset = LineOffset[Y];

                        // Current line - upper half
                        BitmapX_.DrawImage(TempGlyph, 0, Offset, (X * CellW) + ScreenBorder, (Y * CellH) + ScreenBorder, CellW, CellH - Offset);
                        if (OffsetOtherHalf)
                        {
                            TempGlyph = PutChar_GetGlyph(FormCtrlB[X, Y + 1], FormCtrlF[X, Y + 1], FormCtrlC[X, Y + 1], FormCtrlFontW[X, Y + 1], FormCtrlFontH[X, Y + 1]);
                        }
                        else
                        {
                            TempGlyph = PutChar_GetGlyph(LineOffsetBack[Y], LineOffsetFore[Y], 32, FormCtrlFontW[X, Y], FormCtrlFontH[X, Y]);
                        }
                        // Current line - lower half
                        BitmapX_.DrawImage(TempGlyph, 0, 0, (X * CellW) + ScreenBorder, (Y * CellH + CellH - Offset) + ScreenBorder, CellW, Offset);
                    }
                    if (OffsetMode == 2)
                    {
                        int Offset = CellH - (0 - LineOffset[Y]);

                        // Current line - lower half
                        BitmapX_.DrawImage(TempGlyph, 0, 0, (X * CellW) + ScreenBorder, (Y * CellH + CellH - Offset) + ScreenBorder, CellW, Offset);
                        if (OffsetOtherHalf)
                        {
                            TempGlyph = PutChar_GetGlyph(FormCtrlB[X, Y - 1], FormCtrlF[X, Y - 1], FormCtrlC[X, Y - 1], FormCtrlFontW[X, Y - 1], FormCtrlFontH[X, Y - 1]);
                        }
                        else
                        {
                            TempGlyph = PutChar_GetGlyph(LineOffsetBack[Y], LineOffsetFore[Y], 32, FormCtrlFontW[X, Y], FormCtrlFontH[X, Y]);
                        }
                        // Current line - upper half
                        BitmapX_.DrawImage(TempGlyph, 0, Offset, (X * CellW) + ScreenBorder, (Y * CellH) + ScreenBorder, CellW, CellH - Offset);
                    }
                }

                if (MultiThread) Monitor.Exit(BitmapX_);
            }
        }

        private LowLevelBitmap PutChar_GetGlyph(int ColorBack, int ColorFore, int C, int FontW, int FontH)
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
            LowLevelBitmap TempGlyph = GlyphBankGet(ColorBack, ColorFore, C_, FontWH);
            if (TempGlyph == null)
            {
                TempGlyph = new LowLevelBitmap(CellW, CellH, 0);
                if (WinIsBitmapFont)
                {
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
                        for (int X_ = 0; X_ < CellW; X_++)
                        {
                            if (WinBitmapGlyph[CPI + ((Y_ + FontOffH) / FontDivH), C__ + ((X_ + FontOffW) / FontDivW)])
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
                    try
                    {
                        Bitmap TempGlyphB = TempGlyph.ToBitmap();
                        Graphics TempGlyphG = Graphics.FromImage(TempGlyphB);
                        TempGlyphG.FillRectangle(LowLevelBitmap.GetBrush(DrawBack_R, DrawBack_G, DrawBack_B), 0, 0, CellW, CellH);
                        if (((C >= 0x20) && (C < 0xD800)) || (C > 0xDFFF))
                        {
                            TempGlyphG.ScaleTransform(FormCtrlR_Trans[FontW], FormCtrlR_Trans[FontH]);
                            TempGlyphG.DrawString(char.ConvertFromUtf32(C), WinFont, LowLevelBitmap.GetBrush(DrawFore_R, DrawFore_G, DrawFore_B), FormCtrlR[FontW, FontH], WinStrFormat);
                        }
                        TempGlyph = new LowLevelBitmap(TempGlyphB);
                    }
                    catch
                    {

                    }
                }
                GlyphBankSet(ColorBack, ColorFore, C_, TempGlyph, FontWH);
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
            BitmapX_.DrawImage(BitmapX_, (SrcX * CellW) + ScreenBorder, (SrcY * CellH) + ScreenBorder, (DstX * CellW) + ScreenBorder, (DstY * CellH) + ScreenBorder, W * CellW, H * CellH);
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