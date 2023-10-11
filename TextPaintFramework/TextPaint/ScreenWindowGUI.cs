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
            if (WinFixed_ > 0)
            {
                WinAutoAllowed = true;
                WinAuto = CF.ParamGetB("ANSIAutoSize");
            }
            ConsoleBitmap_ = new LowLevelBitmap[2];
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
            }
            else
            {
                AppWorking = true;
            }
        }

        void ConsoleScreen__GotFocus(object sender, EventArgs e)
        {
            FormFocusEvent(true);
        }

        void ConsoleScreen__LostFocus(object sender, EventArgs e)
        {
            FormFocusEvent(false);
        }


        void ConsoleScreen__MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove(e.X, e.Y);
        }

        void ConsoleScreen__MouseOut(object sender, EventArgs e)
        {
            MouseMove(-1, -1);
        }

        void ConsoleScreen__MouseDown(object sender, MouseEventArgs e)
        {
            if (((e.Button & MouseButtons.Left) > 0)) { MouseDown(1); }
            if (((e.Button & MouseButtons.Middle) > 0)) { MouseDown(2); }
            if (((e.Button & MouseButtons.Right) > 0)) { MouseDown(3); }
        }

        void ConsoleScreen__MouseUp(object sender, MouseEventArgs e)
        {
            if (((e.Button & MouseButtons.Left) > 0)) { MouseUp(1); }
            if (((e.Button & MouseButtons.Middle) > 0)) { MouseUp(2); }
            if (((e.Button & MouseButtons.Right) > 0)) { MouseUp(3); }
        }

        void ConsoleScreen__MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) { MouseDown(4); }
            if (e.Delta < 0) { MouseDown(5); }
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
            if (TimerFast)
            {
                DispTimer.Interval = 50;
            }
            else
            {
                DispTimer.Interval = 100;
            }
            DispTimer.Tick += CursorTimer_Tick;
            DispTimer.Enabled = true;

            ConsoleScreen_.AllowDrop = true;
            ConsoleScreen_.DragEnter += DragFileEnter;
            ConsoleScreen_.DragOver += DragFileEnter;
            ConsoleScreen_.DragDrop += DragFileExit;

            ConsoleScreen_.MouseMove += ConsoleScreen__MouseMove;
            ConsoleScreen_.MouseDown += ConsoleScreen__MouseDown;
            ConsoleScreen_.MouseUp += ConsoleScreen__MouseUp;
            ConsoleScreen_.MouseLeave += ConsoleScreen__MouseOut;
            Form_.MouseWheel += ConsoleScreen__MouseWheel;
            Form_.GotFocus += ConsoleScreen__GotFocus;
            Form_.LostFocus += ConsoleScreen__LostFocus;
        }

        Form Form_;
        Timer DispTimer;
        int DuringResize = 0;
        bool DuringResizeEnd = false;
        int MouseXDisp = -1;
        int MouseYDisp = -1;
        bool MouseIsActiveDisp = false;
        bool TimerTick2 = true;

        void CursorTimer_Tick(object sender, EventArgs e)
        {
            if (TimerFast)
            {
                TimerTick2 = !TimerTick2;
            }

            bool NeedRefresh = false;

            if (TimerTick2)
            {
                ConsoleBitmap_Counter++;
                if (ConsoleBitmap_Counter >= 5)
                {
                    ConsoleBitmap_Counter = 0;
                    ConsoleBitmap_Disp = 1 - ConsoleBitmap_Disp;
                    NeedRefresh = true;
                    if (WinPicturePanel)
                    {
                        ConsoleScreen_Panel.Image_ = ConsoleBitmap_[ConsoleBitmap_Disp];
                    }
                    else
                    {
                        ConsoleScreen_PictureBox.Image_ = ConsoleBitmap_[ConsoleBitmap_Disp];
                    }
                }
            }
            if (MouseIsActiveDisp != MouseIsActiveX)
            {
                if (MouseIsActiveX)
                {
                    Form_.Cursor = Cursors.Cross;
                    Cursor.Hide();
                }
                else
                {
                    Form_.Cursor = Cursors.Default;
                    Cursor.Show();
                }
                MouseIsActiveDisp = MouseIsActiveX;
                NeedRefresh = true;
            }
            if (MouseIsActiveX)
            {
                if ((MouseXDisp != MouseX) || (MouseYDisp != MouseY))
                {
                    MouseXDisp = MouseX;
                    MouseYDisp = MouseY;
                    NeedRefresh = true;
                }
            }
            if (NeedRefresh)
            {
                if (WinPicturePanel)
                {
                    ConsoleScreen_Panel.Refresh();
                }
                else
                {
                    ConsoleScreen_PictureBox.Refresh();
                }
            }
            if (TimerTick2)
            {
                if (DuringResize > 0)
                {
                    DuringResize--;
                    if (DuringResizeEnd || (DuringResize == 1))
                    {
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
                    DuringResize = 6;
                }
                else
                {
                    CursorTimerEvent(false);
                }
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
        LowLevelBitmap[] ConsoleBitmap_;
        int ConsoleBitmap_Disp = 0;
        int ConsoleBitmap_Counter = 0;

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

        public override void FormCtrlSetBitmap(LowLevelBitmap Bmp0, LowLevelBitmap Bmp1)
        {
            if (Form_ == null)
            {
                return;
            }
            ConsoleBitmap_[0] = Bmp0;
            ConsoleBitmap_[1] = Bmp1;
            if (WinPicturePanel)
            {
                ConsoleScreen_Panel.DrawW = WinBmpW;
                ConsoleScreen_Panel.DrawH = WinBmpH;
                ConsoleScreen_Panel.Image_ = ConsoleBitmap_[ConsoleBitmap_Disp];
            }
            else
            {
                ConsoleScreen_PictureBox.DrawW = WinBmpW;
                ConsoleScreen_PictureBox.DrawH = WinBmpH;
                ConsoleScreen_PictureBox.Image_ = ConsoleBitmap_[ConsoleBitmap_Disp];
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
            Form_.GotFocus -= ConsoleScreen__GotFocus;
            Form_.LostFocus -= ConsoleScreen__LostFocus;
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
            if (!MouseIsActiveX)
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
}
