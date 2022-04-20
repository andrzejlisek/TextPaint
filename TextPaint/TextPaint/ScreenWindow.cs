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
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TextPaint
{
    /// <summary>
    /// Description of ScreenWindow.
    /// </summary>
    public class ScreenWindow : Screen
    {
        Form Form_;
        Bitmap Bitmap_;
        Graphics Bitmap_G;


        int InternalW;
        int InternalH;
        bool WinIsBitmapFont = false;
        bool[,] WinBitmapGlyph;
        Dictionary<int, int> WinBitmapPage;
        bool ColorBlending = false;

        void CalcBlend(int C1, int C2, double Prop1, double Prop2, out Color C3, out Brush C3B)
        {
            double C1_R = Math.Pow(((double)DrawColor[C1].R) / 255.0, 2.2) * Prop1;
            double C1_G = Math.Pow(((double)DrawColor[C1].G) / 255.0, 2.2) * Prop1;
            double C1_B = Math.Pow(((double)DrawColor[C1].B) / 255.0, 2.2) * Prop1;
            double C2_R = Math.Pow(((double)DrawColor[C2].R) / 255.0, 2.2) * Prop2;
            double C2_G = Math.Pow(((double)DrawColor[C2].G) / 255.0, 2.2) * Prop2;
            double C2_B = Math.Pow(((double)DrawColor[C2].B) / 255.0, 2.2) * Prop2;
            double C3_R = Math.Pow((C1_R + C2_R) / (Prop1 + Prop2), 1 / 2.2);
            double C3_G = Math.Pow((C1_G + C2_G) / (Prop1 + Prop2), 1 / 2.2);
            double C3_B = Math.Pow((C1_B + C2_B) / (Prop1 + Prop2), 1 / 2.2);
            int C_R = (int)(C3_R * 255.0);
            int C_G = (int)(C3_G * 255.0);
            int C_B = (int)(C3_B * 255.0);
            C3 = Color.FromArgb(C_R, C_G, C_B);
            C3B = new SolidBrush(C3);
        }

        /// <summary>
        /// Glybh bank used to drawing characters on the screen
        /// </summary>
        Dictionary<long, Bitmap> GlyphBank = new Dictionary<long, Bitmap>();

        Bitmap GlyphBankGet(int ColorB, int ColorF, int Char)
        {
            // Characters above 0xFFFFF are very rarely used
            // and resignation from buffering there saves 1 bit in index number
            if (Char > 0xFFFFF)
            {
                return null;
            }
            long Idx = Char + (ColorB << 20) + (ColorF << 24);
            if (GlyphBank.ContainsKey(Idx))
            {
                return GlyphBank[Idx];
            }
            else
            {
                return null;
            }
        }

        void GlyphBankSet(int ColorB, int ColorF, int Char, Bitmap Glyph)
        {
            // Characters above 0xFFFFF are very rarely used
            // and resignation from buffering there saves 1 bit in index number
            if (Char > 0xFFFFF)
            {
                return;
            }
            long Idx = Char + (ColorB << 20) + (ColorF << 24);
            GlyphBank.Add(Idx, Glyph);
        }

        class PictureBoxEx
        {
            public PictureBoxEx(int Custom)
            {
                switch (Custom)
                {
                    case 0:
                        Ctrl = new PictureBox();
                        break;
                    case 1:
                        Ctrl = new PictureBoxMonitor();
                        break;
                    case 2:
                        Ctrl = new PictureBoxExPanel();
                        break;
                }
            }

            public Control Ctrl;
            public Bitmap Image
            {
                set
                {
                    if (Ctrl is PictureBox)
                    {
                        ((PictureBox)Ctrl).Image = value;
                    }
                    if (Ctrl is PictureBoxMonitor)
                    {
                        ((PictureBoxMonitor)Ctrl).Image = value;
                    }
                    if (Ctrl is PictureBoxExPanel)
                    {
                        ((PictureBoxExPanel)Ctrl).Image__ = value;
                    }
                }
            }
        }

        class PictureBoxMonitor : PictureBox
        {
            Bitmap Image_;
            public Bitmap Image__
            {
                set
                {
                    Monitor.Enter(value);
                    Image_ = value;
                    Image = value;
                    Monitor.Exit(value);
                }
            }

            bool Mon = false;

            bool Monitor_Enter()
            {
                if (Image_ != null)
                {
                    Monitor.Enter(Image_);
                    Mon = true;
                }
                else
                {
                    Mon = false;
                }
                return Mon;
            }

            void Monitor_Exit()
            {
                if (Mon)
                {
                    Monitor.Exit(Image_);
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Monitor_Enter();
                try
                {
                    base.OnPaint(e);
                }
                catch
                {

                }
                Monitor_Exit();
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                Monitor_Enter();
                try
                {
                    base.OnPaintBackground(e);
                }
                catch
                {

                }
                Monitor_Exit();
            }

            public override void Refresh()
            {
                Monitor_Enter();
                try
                {
                    base.Refresh();
                }
                catch
                {

                }
                Monitor_Exit();
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                Monitor_Enter();
                base.OnSizeChanged(e);
                Monitor_Exit();
            }

            protected override void OnAutoSizeChanged(EventArgs e)
            {
                Monitor_Enter();
                base.OnAutoSizeChanged(e);
                Monitor_Exit();
            }

            protected override void OnClientSizeChanged(EventArgs e)
            {
                Monitor_Enter();
                base.OnClientSizeChanged(e);
                Monitor_Exit();
            }
        }

        class PictureBoxExPanel : Panel
        {
            Bitmap Image_;
            Graphics ImageG_;

            public Bitmap Image__
            {
                set
                {
                    Image_ = value;
                    Repaint();
                }
            }

            public PictureBoxExPanel()
            {
                Image_ = null;
                ImageG_ = this.CreateGraphics();
            }

            void RepaintS()
            {
                ImageG_ = this.CreateGraphics();
                Repaint();
            }

            void Repaint()
            {
                if (Image_ != null)
                {
                    Monitor.Enter(Image_);
                    try
                    {
                        ImageG_.DrawImageUnscaledAndClipped(Image_, new Rectangle(0, 0, this.Width, this.Height));
                    }
                    catch
                    {
                    }
                    Monitor.Exit(Image_);
                }
                else
                {
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                //base.OnPaint(e);
                Repaint();
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                //base.OnPaintBackground(e);
                Repaint();
            }

            public override void Refresh()
            {
                //base.Refresh();
                Repaint();
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);
                RepaintS();
            }

            protected override void OnAutoSizeChanged(EventArgs e)
            {
                base.OnAutoSizeChanged(e);
                RepaintS();
            }

            protected override void OnClientSizeChanged(EventArgs e)
            {
                base.OnClientSizeChanged(e);
                RepaintS();
            }
        }

        bool BmpGetPixel(Bitmap Bmp, int X, int Y)
        {
            Color C = Bmp.GetPixel(X, Y);
            int R = C.R;
            int G = C.G;
            int B = C.B;
            return ((R + G + B) >= 383);
        }

        Brush[] DrawColorB;
        Color[] DrawColor;

        public ScreenWindow(Core Core__, ConfigFile CF, int ConsoleW, int ConsoleH, bool ColorBlending_)
        {
            UseMemo = 0;
            ColorBlending = ColorBlending_;
            CursorTimer = new System.Windows.Forms.Timer();
            CursorTimer.Interval = 100;
            CursorTimer.Tick += CursorTimer_Tick;

            DrawColor = new Color[16];
            DrawColor[0] = Color.FromArgb(0, 0, 0);
            DrawColor[1] = Color.FromArgb(170, 0, 0);
            DrawColor[2] = Color.FromArgb(0, 170, 0);
            DrawColor[3] = Color.FromArgb(170, 170, 0);
            DrawColor[4] = Color.FromArgb(0, 0, 170);
            DrawColor[5] = Color.FromArgb(170, 0, 170);
            DrawColor[6] = Color.FromArgb(0, 170, 170);
            DrawColor[7] = Color.FromArgb(170, 170, 170);
            DrawColor[8] = Color.FromArgb(85, 85, 85);
            DrawColor[9] = Color.FromArgb(255, 85, 85);
            DrawColor[10] = Color.FromArgb(85, 255, 85);
            DrawColor[11] = Color.FromArgb(255, 255, 85);
            DrawColor[12] = Color.FromArgb(85, 85, 255);
            DrawColor[13] = Color.FromArgb(255, 85, 255);
            DrawColor[14] = Color.FromArgb(85, 255, 255);
            DrawColor[15] = Color.FromArgb(255, 255, 255);

            DrawColorB = new Brush[16];
            for (int i = 0; i < 16; i++)
            {
                DrawColorB[i] = new SolidBrush(DrawColor[i]);
            }


            CellW = CF.ParamGetI("WinCellW");
            CellH = CF.ParamGetI("WinCellH");
            WinFontName = CF.ParamGetS("WinFontName");
            WinFontSize = CF.ParamGetI("WinFontSize");
            WinPicturePanel = (CF.ParamGetI("WinUse") == 2);

            if (File.Exists(Core.FullPath(WinFontName)))
            {
                WinIsBitmapFont = true;
                Bitmap FontTempBmp = new Bitmap(Core.FullPath(WinFontName));
                int CellW_ = (int)Math.Floor((FontTempBmp.Width - 16.0) / 256.0);
                List<int> WinBitmapPage_ = new List<int>();
                int Idx = 0;
                int Val0 = -1;
                int ValPlane = 0;
                for (int i = 0; i < FontTempBmp.Height; i++)
                {
                    int Val = 0;
                    if (BmpGetPixel(FontTempBmp, 0, i)) { Val += 32768; }
                    if (BmpGetPixel(FontTempBmp, 1, i)) { Val += 16384; }
                    if (BmpGetPixel(FontTempBmp, 2, i)) { Val += 8192; }
                    if (BmpGetPixel(FontTempBmp, 3, i)) { Val += 4096; }
                    if (BmpGetPixel(FontTempBmp, 4, i)) { Val += 2048; }
                    if (BmpGetPixel(FontTempBmp, 5, i)) { Val += 1024; }
                    if (BmpGetPixel(FontTempBmp, 6, i)) { Val += 512; }
                    if (BmpGetPixel(FontTempBmp, 7, i)) { Val += 256; }
                    if (BmpGetPixel(FontTempBmp, 8, i)) { Val += 128; }
                    if (BmpGetPixel(FontTempBmp, 9, i)) { Val += 64; }
                    if (BmpGetPixel(FontTempBmp, 10, i)) { Val += 32; }
                    if (BmpGetPixel(FontTempBmp, 11, i)) { Val += 16; }
                    if (BmpGetPixel(FontTempBmp, 12, i)) { Val += 8; }
                    if (BmpGetPixel(FontTempBmp, 13, i)) { Val += 4; }
                    if (BmpGetPixel(FontTempBmp, 14, i)) { Val += 2; }
                    if (BmpGetPixel(FontTempBmp, 15, i)) { Val += 1; }
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
                                bool PxlState = (BmpGetPixel(FontTempBmp, CellW_ * ii + 16 + X_, CellH_ * i + Y_));
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
                WinFont = new Font(WinFontName, WinFontSize, FontStyle.Regular);
                WinStrFormat = new StringFormat();
                WinStrFormat.LineAlignment = StringAlignment.Center;
                WinStrFormat.Alignment = StringAlignment.Center;
                WinStrFormat.Trimming = StringTrimming.None;
                WinStrFormat.FormatFlags = StringFormatFlags.NoWrap;
            }

            Core_ = Core__;
            Core_.Screen_ = this;
            Form_ = new Form();
            Form_.Shown += Form__Shown;
            Form_.Text = "TextPaint";
            Form_.BackColor = Color.Black;
            Form_.Width = ConsoleW * CellW;
            Form_.Height = ConsoleH * CellH;
            Form_.ClientSize.ToString();
            int DW = Form_.ClientSize.Width - Form_.Width;
            int DH = Form_.ClientSize.Height - Form_.Height;
            Form_.Width -= DW;
            Form_.Height -= DH;
            Form_.FormClosing += Form__FormClosing;
            FormAllowClose = false;
            Form_.KeyDown += WinKey1;
            Form_.KeyPress += WinKey2;
            Core_.WindowResize();
            Core_.ScreenRefresh(true);
        }


        void Form__Shown(object sender, EventArgs e)
        {
        }


        bool FormAllowClose = false;

        void Form__FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!FormAllowClose)
            {
                CoreEvent("WindowClose", (char)(0), false, false, false);
                if (!FormAllowClose)
                {
                    e.Cancel = true;
                }
            }
        }


        public override void StartApp()
        {
            AppWorking = true;
            Core_.StartUp();
            CursorBox.Ctrl.Visible = true;
            Application.Run(Form_);
        }

        int CellW;
        int CellH;
        int CellW_F;
        int CellH_F;
        string WinFontName;
        int WinFontSize;

        int[,] FormCtrlB;
        int[,] FormCtrlF;
        int[,] FormCtrlC;
        Rectangle[,] FormCtrlR;
        Font WinFont;
        StringFormat WinStrFormat;
        PictureBoxEx PictureBox_;
        bool WinPicturePanel = false;
        PictureBoxEx CursorBox;
        Bitmap CursorBmp;
        Graphics CursorBmp_G;
        System.Windows.Forms.Timer CursorTimer;


        public override bool WindowResize()
        {
            if ((InternalW == Form_.Width) && (InternalH == Form_.Height))
            {
                return false;
            }

            if (Monitor.TryEnter(GraphMutex, 200))
            {
                InternalW = Form_.Width;
                InternalH = Form_.Height;

                WinW = Form_.ClientSize.Width / CellW;
                WinH = Form_.ClientSize.Height / CellH;

                Bitmap_ = new Bitmap(WinW * CellW, WinH * CellH, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Bitmap_G = Graphics.FromImage(Bitmap_);

                FormCtrlB = new int[WinW, WinH];
                FormCtrlF = new int[WinW, WinH];
                FormCtrlC = new int[WinW, WinH];
                FormCtrlR = new Rectangle[WinW, WinH];
                Form_.Controls.Clear();
                PictureBox_ = new PictureBoxEx(WinPicturePanel ? 2 : 1);
                PictureBox_.Ctrl.Left = (Form_.ClientSize.Width - Bitmap_.Width) / 2;
                PictureBox_.Ctrl.Top = (Form_.ClientSize.Height - Bitmap_.Height) / 2;
                PictureBox_.Ctrl.Width = Bitmap_.Width;
                PictureBox_.Ctrl.Height = Bitmap_.Height;
                PictureBox_.Image = Bitmap_;


                CursorBmp = new Bitmap(CellW, CellH, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                CursorBmp_G = Graphics.FromImage(CursorBmp);
                CursorBox = new PictureBoxEx(WinPicturePanel ? 2 : 0);
                CursorBox.Ctrl.Left = 0 - CellW - CellW;
                CursorBox.Ctrl.Top = 0 - CellH - CellH;
                CursorBox.Ctrl.Width = CellW;
                CursorBox.Ctrl.Height = CellH;
                CursorBox.Image = CursorBmp;
                Form_.Controls.Add(CursorBox.Ctrl);
                PictureBox_.Ctrl.Refresh();

                Form_.Controls.Add(PictureBox_.Ctrl);
                PictureBox_.Ctrl.Refresh();
                PictureBox_.Ctrl.AllowDrop = true;
                PictureBox_.Ctrl.DragEnter += DragFileEnter;
                PictureBox_.Ctrl.DragOver += DragFileEnter;
                PictureBox_.Ctrl.DragDrop += DragFileExit;

                CursorTimer.Enabled = true;

                for (int Y = 0; Y < WinH; Y++)
                {
                    for (int X = 0; X < WinW; X++)
                    {
                        FormCtrlB[X, Y] = -1;
                        FormCtrlF[X, Y] = -1;
                        FormCtrlC[X, Y] = ' ';
                        FormCtrlR[X, Y] = new Rectangle(X * CellW, Y * CellH, CellW, CellH);
                    }
                }
                CursorB = -1;
                CursorF = -1;
                CursorC = -1;
                CursorNeedRepaint = true;

                Monitor.Exit(GraphMutex);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DragFileEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        public void DragFileExit(object sender, DragEventArgs e)
        {
            string[] F = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (F != null)
            {
                if (F[0] != null)
                {
                    CoreEvent("FileDrop1", (char)(0), false, false, false);
                    for (int i = 0; i < F[0].Length; i++)
                    {
                        CoreEvent("", F[0][i], false, false, false);
                    }
                    CoreEvent("FileDrop2", (char)(0), false, false, false);
                }
            }
        }

        string KeyCode_ = "";
        bool KeyShift_ = false;
        bool KeyCtrl_ = false;
        bool KeyAlt_ = false;

        public void WinKey1(object sender, KeyEventArgs e)
        {
            KeyCode_ = e.KeyCode.ToString();
            KeyShift_ = e.Shift;
            KeyCtrl_ = e.Control;
            KeyAlt_ = e.Alt;
            switch (KeyCode_)
            {
                case "F1":
                case "F2":
                case "F3":
                case "F4":
                case "F5":
                case "F6":
                case "F7":
                case "F8":
                case "F9":
                case "F10":
                case "F11":
                case "F12":
                case "Up":
                case "Down":
                case "Left":
                case "Right":
                case "Insert":
                case "Delete":
                case "Home":
                case "End":
                case "Prior":
                case "Next":
                case "Scroll":
                case "Pause":
                case "PageUp":
                case "PageDown":
                    CoreEvent(e.KeyCode.ToString(), (char)(0), KeyShift_, KeyCtrl_, KeyAlt_);
                    KeyCode_ = "";
                    KeyShift_ = false;
                    KeyCtrl_ = false;
                    KeyAlt_ = false;
                    break;
            }
        }

        public void WinKey2(object sender, KeyPressEventArgs e)
        {
            CoreEvent(KeyCode_, (char)(e.KeyChar), KeyShift_, KeyCtrl_, KeyAlt_);
            KeyCode_ = "";
            KeyShift_ = false;
            KeyCtrl_ = false;
            KeyAlt_ = false;
        }

        void CoreEvent(string KeyName, char KeyChar, bool ModShift, bool ModCtrl, bool ModAlt)
        {
            Core_.CoreEvent(UniKeyName(KeyName), KeyChar, ModShift, ModCtrl, ModAlt);
            if (!AppWorking)
            {
                FormAllowClose = true;
                Form_.Close();
            }
        }

        protected override void PutChar_(int X, int Y, int C, int ColorBack, int ColorFore)
        {
            Monitor.Enter(GraphMutex);
            PutChar_Work(X, Y, C, ColorBack, ColorFore);
            Monitor.Exit(GraphMutex);
        }

        private void PutChar_Work(int X, int Y, int C, int ColorBack, int ColorFore)
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
            Diff = true;
            if (Diff)
            {
                Color DrawBack = DrawColor[ColorBack];
                Color DrawFore = DrawColor[ColorFore];
                Brush DrawBackB = DrawColorB[ColorBack];
                Brush DrawForeB = DrawColorB[ColorFore];
                if (ColorBlending)
                {
                    switch (C)
                    {
                        case 0x2591:
                            {
                                C = 0x2588;
                                ColorFore += 32;
                            }
                            break;
                        case 0x2592:
                            {
                                C = 0x2588;
                                ColorFore += 16;
                            }
                            break;
                        case 0x2593:
                            {
                                C = 0x2588;
                                ColorFore += 48;
                            }
                            break;

                        case 0x1FB8C:
                            {
                                C = 0x258C;
                                ColorFore += 16;
                            }
                            break;
                        case 0x1FB8D:
                            {
                                C = 0x2590;
                                ColorFore += 16;
                            }
                            break;
                        case 0x1FB8E:
                            {
                                C = 0x2580;
                                ColorFore += 16;
                            }
                            break;
                        case 0x1FB8F:
                            {
                                C = 0x2584;
                                ColorFore += 16;
                            }
                            break;

                        case 0x1FB9C:
                            {
                                C = 0x25E4;
                                ColorFore += 16;
                            }
                            break;
                        case 0x1FB9D:
                            {
                                C = 0x25E5;
                                ColorFore += 16;
                            }
                            break;
                        case 0x1FB9E:
                            {
                                C = 0x25E2;
                                ColorFore += 16;
                            }
                            break;
                        case 0x1FB9F:
                            {
                                C = 0x25E3;
                                ColorFore += 16;
                            }
                            break;



                        case 0x1FB90:
                            {
                                C = 0x0020;
                                ColorFore += 64;
                            }
                            break;

                        case 0x1FB91:
                            {
                                C = 0x2580;
                                ColorFore += 64;
                            }
                            break;
                        case 0x1FB92:
                            {
                                C = 0x2584;
                                ColorFore += 64;
                            }
                            break;
                        case 0x1FB93:
                            {
                                C = 0x258C;
                                ColorFore += 64;
                            }
                            break;
                        case 0x1FB94:
                            {
                                C = 0x2590;
                                ColorFore += 64;
                            }
                            break;
                    }
                }
                Bitmap TempGlyph = GlyphBankGet(ColorBack, ColorFore, C);
                if (TempGlyph == null)
                {
                    TempGlyph = new Bitmap(CellW, CellH, PixelFormat.Format24bppRgb);
                    if (ColorFore >= 16)
                    {
                        switch (ColorFore >> 4)
                        {
                            case 1:
                                CalcBlend(ColorBack, ColorFore - 16, 1, 1, out DrawFore, out DrawForeB);
                                break;
                            case 2:
                                CalcBlend(ColorBack, ColorFore - 32, 3, 1, out DrawFore, out DrawForeB);
                                break;
                            case 3:
                                CalcBlend(ColorBack, ColorFore - 48, 1, 3, out DrawFore, out DrawForeB);
                                break;
                            case 4:
                                CalcBlend(ColorBack, ColorFore - 64, 1, 1, out DrawBack, out DrawBackB);
                                break;
                        }
                    }
                    if (WinIsBitmapFont)
                    {
                        int X__ = X * CellW;
                        int Y__ = Y * CellH;
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
                        for (int Y_ = 0; Y_ < CellH; Y_++)
                        {
                            for (int X_ = 0; X_ < CellW; X_++)
                            {
                                if (WinBitmapGlyph[CPI + Y_, C__ + X_])
                                {
                                    TempGlyph.SetPixel(X_, Y_, DrawFore);
                                }
                                else
                                {
                                    TempGlyph.SetPixel(X_, Y_, DrawBack);
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            Graphics TempGlyphG = Graphics.FromImage(TempGlyph);
                            TempGlyphG.FillRectangle(DrawBackB, 0, 0, CellW, CellH);
                            if (((C >= 0x20) && (C < 0xD800)) || (C > 0xDFFF))
                            {
                                TempGlyphG.DrawString(char.ConvertFromUtf32(C), WinFont, DrawForeB, FormCtrlR[0, 0], WinStrFormat);
                            }
                        }
                        catch
                        {

                        }
                    }
                    GlyphBankSet(ColorBack, ColorFore, C, TempGlyph);
                }
                if (MultiThread) Monitor.Enter(Bitmap_);
                Bitmap_G.DrawImageUnscaled(TempGlyph, X * CellW, Y * CellH);
                if (MultiThread) Monitor.Exit(Bitmap_);
            }
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
            Bitmap Bitmap_Temp = Bitmap_.Clone(new Rectangle(SrcX * CellW, SrcY * CellH, W * CellW, H * CellH), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Bitmap_G.DrawImage(Bitmap_Temp, DstX * CellW, DstY * CellH);
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
                        }
                    }
                }
            }
            Monitor.Exit(GraphMutex);
        }

        private delegate void D();

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
                if (CursorC != FormCtrlC[CursorX, CursorY])
                {
                    CursorNeedRepaint = true;
                    CursorC = FormCtrlC[CursorX, CursorY];
                }
                if (MultiThread)
                {
                    try
                    {
                        CursorBox.Ctrl.Invoke((D)delegate
                        {
                            RefreshFunc();
                        });
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

        void RefreshFunc()
        {
            if (CursorNeedRepaint)
            {
                CursorBox.Ctrl.Visible = false;
            }
            else
            {
                CursorBox.Ctrl.Left = PictureBox_.Ctrl.Left + (CursorX * CellW);
                CursorBox.Ctrl.Top = PictureBox_.Ctrl.Top + (CursorY * CellH);
            }
            Monitor.Enter(Bitmap_);
            PictureBox_.Ctrl.Refresh();
            Monitor.Exit(Bitmap_);
        }

        void CursorTimer_Tick(object sender, EventArgs e)
        {
            if (Monitor.TryEnter(GraphMutex))
            {
                if (!CursorBox.Ctrl.Visible)
                {
                    if (CursorNeedRepaint)
                    {
                        CursorBmp_G.DrawImage(Bitmap_, 0 - (CursorX * CellW), 0 - (CursorY * CellH));

                        int CursorThick = (((CellH + 7) / 8));
                        if (WinIsBitmapFont)
                        {
                            CursorThick = CellH_F * ((((CellH / CellH_F) + 7) / 8));
                        }

                        try
                        {
                            Color CurC = DrawColor[FormCtrlF[CursorX, CursorY]];
                            for (int Y0 = CellH - CursorThick; Y0 < CellH; Y0++)
                            {
                                for (int X0 = 0; X0 < CellW; X0++)
                                {
                                    CursorBmp.SetPixel(X0, Y0, CurC);
                                }
                            }

                            CursorBox.Ctrl.Refresh();

                            CursorNeedRepaint = false;
                            CursorBox.Ctrl.Left = PictureBox_.Ctrl.Left + (CursorX * CellW);
                            CursorBox.Ctrl.Top = PictureBox_.Ctrl.Top + (CursorY * CellH);
                        }
                        catch
                        {

                        }
                    }
                }
                CursorBox.Ctrl.Visible = !CursorBox.Ctrl.Visible;
                Monitor.Exit(GraphMutex);
            }

        }
    }
}