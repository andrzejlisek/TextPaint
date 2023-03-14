using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TextPaint
{
    public class ScreenWindowGUI : ScreenWindow
    {
        bool BitmapStretch = false;

        public ScreenWindowGUI(Core Core__, int WinFixed_, ConfigFile CF, int ConsoleW, int ConsoleH, bool ColorBlending_, List<string> ColorBlendingConfig_, bool DummyScreen) : base(Core__, WinFixed_, CF, ConsoleW, ConsoleH, ColorBlending_, ColorBlendingConfig_, DummyScreen)
        {
            if (!DummyScreen)
            {
                XWidth = ConsoleW * CellW;
                XHeight = ConsoleH * CellH;
                Form_ = new Form();
                Form_.Shown += Form__Shown;
                Form_.Text = "TextPaint";
                Form_.BackColor = Color.Black;
                Form_.Width = ConsoleW * CellW;
                Form_.Height = ConsoleH * CellH;
                int DW = Form_.ClientSize.Width - Form_.Width;
                int DH = Form_.ClientSize.Height - Form_.Height;
                Form_.Width -= DW;
                Form_.Height -= DH;
                FormAllowClose = false;
                Form_.KeyDown += WinKey1;
                Form_.KeyPress += WinKey2;

                ConsoleCursor_ = new Panel();
                Form_.Controls.Add(ConsoleCursor_);
                ConsoleCursor_.DoubleClick += Ctrl_DoubleClick;
                ConsoleCursor_.ForeColor = Color.Green;
                ConsoleCursor_.BackColor = Color.Red;

                BitmapStretch = (WinFixed == 2);
                if (WinPicturePanel)
                {
                    ConsoleScreen_Panel = new Window_PictureBoxPanel(BitmapStretch);
                    ConsoleScreen_ = ConsoleScreen_Panel;
                }
                else
                {
                    ConsoleScreen_PictureBox = new Window_PictureBoxEx(BitmapStretch);
                    ConsoleScreen_ = ConsoleScreen_PictureBox;
                }
                ConsoleScreen_.DoubleClick += Ctrl_DoubleClick;
                Form_.ResizeBegin += Form__ResizeBegin;
                Form_.ResizeEnd += Form__ResizeEnd;
                Form_.Controls.Add(ConsoleScreen_);

                Core_.WindowResize();
                Core_.ScreenRefresh(true);
            }
            else
            {
                AppWorking = true;
            }
        }

        void Form__ResizeBegin(object sender, EventArgs e)
        {
        }

        void Form__ResizeEnd(object sender, EventArgs e)
        {
            DuringResizeEnd = true;
        }


        void Form__Shown(object sender, EventArgs e)
        {
            StartAppFormShown();
            Form_.FormClosing += Form__FormClosing;

            WindowResizeForce = true;
            DispTimer = new Timer();
            DispTimer.Interval = 100;
            DispTimer.Tick += CursorTimer_Tick;
            DispTimer.Enabled = true;

            ConsoleScreen_.AllowDrop = true;
            ConsoleScreen_.DragEnter += DragFileEnter;
            ConsoleScreen_.DragOver += DragFileEnter;
            ConsoleScreen_.DragDrop += DragFileExit;
        }

        Form Form_;
        Timer DispTimer;
        int DuringResize = 0;
        bool DuringResizeEnd = false;

        void CursorTimer_Tick(object sender, EventArgs e)
        {
            if (DuringResize > 0)
            {
                DuringResize--;
                if (DuringResizeEnd || (DuringResize == 1))
                {
                    //Console.WriteLine("Zmiana rozmiaru stop 1___" + XWidth + "__" + Form_.ClientSize.Width);
                    //Console.WriteLine("Zmiana rozmiaru stop 2");
                    XWidth = Form_.ClientSize.Width;
                    XHeight = Form_.ClientSize.Height;
                    CursorTimerEvent(true);
                    DuringResizeEnd = false;
                    DuringResize = 0;
                }
                return;
            }
            if ((XWidth != Form_.ClientSize.Width) || (XHeight != Form_.ClientSize.Height))
            {
                //Console.WriteLine("Zmiana rozmiaru start 1___" + XWidth + "__" + Form_.ClientSize.Width);
                DuringResize = 6;
                //Console.WriteLine("Zmiana rozmiaru start 2");
            }
            else
            {
                CursorTimerEvent(false);
            }
        }

        protected override void StartAppForm()
        {
            Application.Run(Form_);
        }

        int XWidth = 640;
        int XHeight = 480;
        int WinBmpW = 640;
        int WinBmpH = 480;

        public override int FormGetWidth()
        {
            //return Form_.ClientSize.Width;
            return XWidth;
        }

        public override int FormGetHeight()
        {
            //return Form_.ClientSize.Height;
            return XHeight;
        }

        protected override int FormCtrlGetParam(int Param)
        {
            switch (Param)
            {
                case 0:
                    return ConsoleScreen_.Left;
                case 1:
                    return ConsoleScreen_.Top;
                case 2:
                    return ConsoleScreen_.Width;
                case 3:
                    return ConsoleScreen_.Height;
                case 4:
                    return ConsoleCursor_.Left;
                case 5:
                    return ConsoleCursor_.Top;
                case 6:
                    return ConsoleCursor_.Width;
                case 7:
                    return ConsoleCursor_.Height;
                case 8:
                    return ConsoleCursor_.Visible ? 1 : 0;
            }
            return 0;
        }

        protected override void FormCtrlSetParam(int Param, int Value)
        {
            switch (Param)
            {
                case 0:
                    ConsoleScreen_.Left = Value;
                    break;
                case 1:
                    ConsoleScreen_.Top = Value;
                    break;
                case 2:
                    ConsoleScreen_.Width = Value;
                    WinBmpW = Value;
                    break;
                case 3:
                    ConsoleScreen_.Height = Value;
                    WinBmpH = Value;
                    break;
                case 4:
                    ConsoleCursor_.Left = Value;
                    break;
                case 5:
                    ConsoleCursor_.Top = Value;
                    break;
                case 6:
                    ConsoleCursor_.Width = Value;
                    break;
                case 7:
                    ConsoleCursor_.Height = Value;
                    break;
                case 8:
                    if (Value >= 0)
                    {
                        ConsoleCursor_.Visible = (Value != 0);
                    }
                    else
                    {
                        ConsoleCursor_.Visible = !ConsoleCursor_.Visible;
                    }
                    break;
            }
        }

        Control ConsoleScreen_;
        Window_PictureBoxEx ConsoleScreen_PictureBox;
        Window_PictureBoxPanel ConsoleScreen_Panel;
        Panel ConsoleCursor_;
        LowLevelBitmap ConsoleBitmap_;

        protected override void FormCtrlRefresh()
        {
            if (WinPicturePanel)
            {
                ConsoleScreen_Panel.DrawW = WinBmpW;
                ConsoleScreen_Panel.DrawH = WinBmpH;
            }
            else
            {
                ConsoleScreen_PictureBox.DrawW = WinBmpW;
                ConsoleScreen_PictureBox.DrawH = WinBmpH;
            }
            ConsoleScreen_.Refresh();
        }

        protected override void FormCtrlSetColor(byte ColorR, byte ColorG, byte ColorB)
        {
            ConsoleCursor_.BackColor = Color.FromArgb(ColorR, ColorG, ColorB);
            ConsoleCursor_.ForeColor = Color.FromArgb(ColorR, ColorG, ColorB);
        }

        private delegate void D();

        protected override void RefreshFuncCtrl()
        {
            if (DuringResize == 0)
            {
                Form_.Invoke((D)delegate
                {
                    RefreshFunc();
                });
            }
        }

        public override void FormCtrlSetBitmap(LowLevelBitmap Bmp)
        {
            if (Form_ == null)
            {
                return;
            }
            ConsoleBitmap_ = Bmp;
            if (WinPicturePanel)
            {
                ConsoleScreen_Panel.DrawW = WinBmpW;
                ConsoleScreen_Panel.DrawH = WinBmpH;
                ConsoleScreen_Panel.Image_ = Bmp;
            }
            else
            {
                ConsoleScreen_PictureBox.DrawW = WinBmpW;
                ConsoleScreen_PictureBox.DrawH = WinBmpH;
                ConsoleScreen_PictureBox.Image_ = Bmp;
            }
        }


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

        protected override void FormClose()
        {
            Form_.Close();
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
                    for (int i = 0; i < F[0].Length; i++)
                    {
                        CoreEvent("", F[0][i], false, false, false);
                    }
                }
            }
        }

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

        void Ctrl_DoubleClick(object sender, EventArgs e)
        {
            Form_.WindowState = FormWindowState.Normal;
            if (Form_.FormBorderStyle == FormBorderStyle.None)
            {
                Form_.FormBorderStyle = FormBorderStyle.Sizable;
            }
            else
            {
                Form_.FormBorderStyle = FormBorderStyle.None;
                Form_.WindowState = FormWindowState.Maximized;
            }
        }
    }
}
