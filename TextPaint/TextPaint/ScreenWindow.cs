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


        class PictureBoxEx
        {
            public PictureBoxEx(bool Custom)
            {
                if (Custom)
                {
                    Ctrl = new PictureBoxExPanel();
                }
                else
                {
                    Ctrl = new PictureBox();
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
                    if (Ctrl is PictureBoxExPanel)
                    {
                        ((PictureBoxExPanel)Ctrl).Image__ = value;
                    }
                }
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
                    try
                    {
                        ImageG_.DrawImageUnscaledAndClipped(Image_, new Rectangle(0, 0, this.Width, this.Height));
                    }
                    catch
                    {
                    }
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

        public ScreenWindow(Core Core__, ConfigFile CF, int ConsoleW, int ConsoleH)
        {
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
                int CellW_F = (int)Math.Round(((double)CellW) / ((double)CellW_));
                int CellH_F = (int)Math.Round(((double)CellH) / ((double)CellH_));
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
            Form_.Text = "TextPaint";
            Form_.BackColor = Color.Black;
            Form_.Width = ConsoleW * CellW;
            Form_.Height = ConsoleH * CellH;
            Form_.ClientSize.ToString();
            int DW = Form_.ClientSize.Width - Form_.Width;
            int DH = Form_.ClientSize.Height - Form_.Height;
            Form_.Width -= DW;
            Form_.Height -= DH;
            Form_.KeyDown += WinKey1;
            Form_.KeyPress += WinKey2;
            Core_.WindowResize();
            Core_.ScreenRefresh(true);
            Core_.StartUp();
        }

        public override void StartApp()
        {
            AppWorking = true;
            Application.Run(Form_);
        }

        int CellW;
        int CellH;
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


        public override bool WindowResize()
        {
            if ((InternalW == Form_.Width) && (InternalH == Form_.Height))
            {
                return false;
            }

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
            PictureBox_ = new PictureBoxEx(WinPicturePanel);
            PictureBox_.Ctrl.Left = (Form_.ClientSize.Width - Bitmap_.Width) / 2;
            PictureBox_.Ctrl.Top = (Form_.ClientSize.Height - Bitmap_.Height) / 2;
            PictureBox_.Ctrl.Width = Bitmap_.Width;
            PictureBox_.Ctrl.Height = Bitmap_.Height;
            PictureBox_.Image = Bitmap_;

            Form_.Controls.Add(PictureBox_.Ctrl);
            PictureBox_.Ctrl.Refresh();

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
            
            return true;
        }
        


        public void WinKey1(object sender, KeyEventArgs e)
        {
            Core_.CoreEvent(e.KeyCode.ToString(), (char)(0));
            if (!AppWorking)
            {
                Form_.Close();
            }
        }
        
        public void WinKey2(object sender, KeyPressEventArgs e)
        {
            Core_.CoreEvent("", (char)(e.KeyChar));
            if (!AppWorking)
            {
                Form_.Close();
            }
        }


        protected override void PutChar_(int X, int Y, int C, int ColorBack, int ColorFore)
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
            if (Diff)
            {
                Brush ColorBack_ = null;
                Brush ColorFore_ = null;
                switch (ColorBack)
                {
                    case 0: { ColorBack_ = Brushes.Black; } break;
                    case 1: { ColorBack_ = Brushes.Gray; } break;
                    case 2: { ColorBack_ = Brushes.Silver; } break;
                    case 3: { ColorBack_ = Brushes.White; } break;
                }
                switch (ColorFore)
                {
                    case 0: { ColorFore_ = Brushes.Black; } break;
                    case 1: { ColorFore_ = Brushes.Gray; } break;
                    case 2: { ColorFore_ = Brushes.Silver; } break;
                    case 3: { ColorFore_ = Brushes.White; } break;
                }
                try
                {
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
                                    Bitmap_.SetPixel(X__ + X_, Y__ + Y_, ((SolidBrush)ColorFore_).Color);
                                }
                                else
                                {
                                    Bitmap_.SetPixel(X__ + X_, Y__ + Y_, ((SolidBrush)ColorBack_).Color);
                                }
                            }
                        }
                    }
                    else
                    {
                        Bitmap_G.FillRectangle(ColorBack_, X * CellW, Y * CellH, CellW, CellH);
                        if (C <= 65535)
                        {
                            if (((C >= 0x20) && (C < 0xD800)) || (C > 0xDFFF))
                            {
                                Bitmap_G.DrawString(char.ConvertFromUtf32(C), WinFont, ColorFore_, FormCtrlR[X, Y], WinStrFormat);
                            }
                        }
                        else
                        {
                            Graphics Bitmap_G_ = Graphics.FromImage(Bitmap_);
                            Bitmap_G_.DrawString(char.ConvertFromUtf32(C), WinFont, ColorFore_, FormCtrlR[X, Y], WinStrFormat);
                        }
                    }
                }
                catch
                {
                    Bitmap_G = Graphics.FromImage(Bitmap_);
                    PictureBox_.Image = Bitmap_;
                }
            }
        }
        
        public override void Move(int SrcX, int SrcY, int DstX, int DstY, int W, int H)
        {
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
        }

        public override void SetCursorPosition(int X, int Y)
        {
            PictureBox_.Ctrl.Refresh();
        }
    }
}
