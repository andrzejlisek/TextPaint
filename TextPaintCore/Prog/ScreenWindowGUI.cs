using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Avalonia;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Avalonia.Media;
using Avalonia.Threading;

namespace TextPaint
{
    public class ScreenWindowGUI : ScreenWindow
    {
        Avalonia.Controls.Image ConsoleScreen = null;
        Avalonia.Controls.Shapes.Rectangle ConsoleCursor = null;
        Avalonia.Controls.Canvas ConsoleCanvas = null;
        Avalonia.Controls.TextBox ConsoleKeyCapture = null;
        TextPaint.Views.MainWindow Form_ = null;
        Avalonia.Threading.DispatcherTimer DispTimer;

        LowLevelBitmap[] ScreenBmp = new LowLevelBitmap[2];
        int ScreenBmpDisp = 0;
        int ScreenBmpCounter = 0;

        bool LowLevelStretch = false;

        public ScreenWindowGUI(Core Core__, int WinFixed_, ConfigFile CF, int ConsoleW, int ConsoleH, bool ColorBlending_, List<string> ColorBlendingConfig_, bool DummyScreen) : base(Core__, WinFixed_, CF, ConsoleW, ConsoleH, ColorBlending_, ColorBlendingConfig_, DummyScreen)
        {
            if (WinFixed_ > 0)
            {
                WinAutoAllowed = true;
                WinAuto = CF.ParamGetB("ANSIAutoSize");
            }
            if (!DummyScreen)
            {
                WinFixed = WinFixed_;
                LowLevelStretch = (WinFixed == 2);
                XWidth = ConsoleW * CellW;
                XHeight = ConsoleH * CellH;
                Program.ScreenWindowGUI_ = this;
                FormAllowClose = false;
                Core_.WindowResize();
                Core_.ScreenRefresh(true);
            }
            else
            {
                AppWorking = true;
            }
        }

        public void StartAppFormFinish(TextPaint.Views.MainWindow Form__)
        {
            Form_ = Form__;
            Form_.SetSize(XWidth, XHeight);
            ConsoleScreen = Form__.ConsoleScreen;
            ConsoleCursor = Form__.ConsoleCursor;
            ConsoleCanvas = Form__.ConsoleCanvas;
            ConsoleKeyCapture = Form__.ConsoleKeyCapture;

            StartAppFormShown();

            Form_.Closing += Form__FormClosing;
            RefreshFuncCtrlEvent += RefreshFuncCtrlEvent_;

            WindowResizeForce = true;
            DispTimer = new Avalonia.Threading.DispatcherTimer();
            DispTimer.Interval = new System.TimeSpan(0, 0, 0, 0, 100);
            DispTimer.Tick += CursorTimer_Tick;
            DispTimer.IsEnabled = true;

            DragDrop.SetAllowDrop(ConsoleScreen, true);
            ConsoleScreen.AddHandler(DragDrop.DragEnterEvent, DragFileEnter);
            ConsoleScreen.AddHandler(DragDrop.DragOverEvent, DragFileEnter);
            ConsoleScreen.AddHandler(DragDrop.DropEvent, DragFileExit);
        }

        public override void FormCtrlSetBitmap(LowLevelBitmap Bmp0, LowLevelBitmap Bmp1)
        {
            if (Form_ == null)
            {
                return;
            }
            ScreenBmp[0] = Bmp0;
            ScreenBmp[1] = Bmp1;
            FormCtrlRefresh();
        }

        void CursorTimer_Tick(object sender, EventArgs e)
        {
            while (FormCtrlSetParamQueueV.Count > 0)
            {
                FormCtrlSetParam(FormCtrlSetParamQueueP[0], FormCtrlSetParamQueueV[0]);
                FormCtrlSetParamQueueP.RemoveAt(0);
                FormCtrlSetParamQueueV.RemoveAt(0);
            }

            ScreenBmpCounter++;
            if (ScreenBmpCounter >= 5)
            {
                ScreenBmpCounter = 0;
                ScreenBmpDisp = 1 - ScreenBmpDisp;
                FormCtrlRefresh();
            }

            if (!ConsoleKeyCapture.IsFocused)
            {
                ConsoleKeyCapture.Focus();
            }
            if ((XWidth != Form_.GetWidth()) || (XHeight != Form_.GetHeight()))
            {
                XWidth = Form_.GetWidth();
                XHeight = Form_.GetHeight();
                CursorTimerEvent(true);
            }
            else
            {
                CursorTimerEvent(false);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().UseReactiveUI();
        }

        protected override void StartAppForm()
        {
            // https://docs.avaloniaui.net/docs/getting-started/application-lifetimes
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(new string[1]);
        }

        int XWidth = 640;
        int XHeight = 480;

        public override int FormGetWidth()
        {
            if (Form_ != null)
            {
                //return Form_.GetWidth();
            }
            return XWidth;
        }

        public override int FormGetHeight()
        {
            if (Form_ != null)
            {
                //return Form_.GetHeight();
            }
            return XHeight;
        }

        protected override int FormCtrlGetParam(int Param)
        {
            switch (Param)
            {
                case 0:
                    return (int)Avalonia.Controls.Canvas.GetLeft(ConsoleScreen);
                case 1:
                    return (int)Avalonia.Controls.Canvas.GetTop(ConsoleScreen);
                case 2:
                    return (int)ConsoleScreen.Width;
                case 3:
                    return (int)ConsoleScreen.Height;
                case 4:
                    return (int)Avalonia.Controls.Canvas.GetLeft(ConsoleCursor);
                case 5:
                    return (int)Avalonia.Controls.Canvas.GetTop(ConsoleCursor);
                case 6:
                    return (int)ConsoleCursor.Width;
                case 7:
                    return (int)ConsoleCursor.Height;
                case 8:
                    return (ConsoleCursor.IsVisible ? 1 : 0);
            }
            return 0;
        }

        List<int> FormCtrlSetParamQueueP = new List<int>();
        List<int> FormCtrlSetParamQueueV = new List<int>();

        protected override void FormCtrlSetParam(int Param, int Value)
        {
            if (Form_ == null)
            {
                return;
            }
            try
            {
                switch (Param)
                {
                    case 0:
                        Avalonia.Controls.Canvas.SetLeft(ConsoleScreen, Value);
                        break;
                    case 1:
                        Avalonia.Controls.Canvas.SetTop(ConsoleScreen, Value);
                        break;
                    case 2:
                        ConsoleScreen.Width = Value;
                        break;
                    case 3:
                        ConsoleScreen.Height = Value;
                        break;
                    case 4:
                        Avalonia.Controls.Canvas.SetLeft(ConsoleCursor, Value);
                        break;
                    case 5:
                        Avalonia.Controls.Canvas.SetTop(ConsoleCursor, Value);
                        break;
                    case 6:
                        ConsoleCursor.Width = Value;
                        break;
                    case 7:
                        ConsoleCursor.Height = Value;
                        break;
                    case 8:
                        if (Value >= 0)
                        {
                            ConsoleCursor.IsVisible = (Value != 0);
                        }
                        else
                        {
                            ConsoleCursor.IsVisible = !ConsoleCursor.IsVisible;
                        }
                        break;
                }
            }
            catch
            {
                FormCtrlSetParamQueueP.Add(Param);
                FormCtrlSetParamQueueV.Add(Value);
            }
        }

        protected override void FormCtrlRefresh()
        {
            if ((ScreenBmp != null) && (ConsoleScreen != null))
            {
                try
                {
                    if (LowLevelStretch)
                    {
                        ConsoleScreen.Source = ScreenBmp[ScreenBmpDisp].ToBitmap(XWidth, XHeight);
                    }
                    else
                    {
                        ConsoleScreen.Source = ScreenBmp[ScreenBmpDisp].ToBitmap();
                    }
                }
                catch
                {
                }
                ConsoleScreen.InvalidateVisual();
            }
        }

        protected override void FormCtrlSetColor(byte ColorR, byte ColorG, byte ColorB)
        {
            ConsoleCursor.Fill = new SolidColorBrush(Avalonia.Media.Color.FromArgb(255, ColorR, ColorG, ColorB), 1);
        }

        public delegate void RefreshFuncCtrlEventHandler();
        public event RefreshFuncCtrlEventHandler RefreshFuncCtrlEvent;

        void RefreshFuncCtrlEvent_()
        {
            try
            {
                Dispatcher.UIThread.InvokeAsync(RefreshFunc, DispatcherPriority.Background);
                //Dispatcher.UIThread.Post(() => {
                //    RefreshFunc();
                //}, DispatcherPriority.MaxValue);

            }
            catch (Exception e)
            {
            }
        }

        protected override void RefreshFuncCtrl()
        {
            RefreshFuncCtrlEvent();
        }

        public void WinKey(string KeyName, char KeyChar, bool ModShift, bool ModCtrl, bool ModAlt)
        {
            CoreEvent(KeyName, KeyChar, ModShift, ModCtrl, ModAlt);
        }



        void Form__FormClosing(object sender, CancelEventArgs e)
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
            if (e.Data.Contains(DataFormats.FileNames))
            {
                e.DragEffects = DragDropEffects.Link;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
        }

        public void DragFileExit(object sender, DragEventArgs e)
        {
            var T = e.Data.GetFileNames();
            if (T != null)
            {
                string F = null;
                foreach (string item in T)
                {   
                    if (F == null)
                    {
                        F = item;
                    }
                }
                if (F != null)
                {
                    for (int i = 0; i < F.Length; i++)
                    {
                        CoreEvent("", F[i], false, false, false);
                    }
                }
            }
        }
    }
}
