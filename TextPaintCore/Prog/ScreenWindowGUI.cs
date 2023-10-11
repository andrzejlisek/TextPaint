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
            if (TimerFast)
            {
                DispTimer.Interval = new System.TimeSpan(0, 0, 0, 0, 50);
            }
            else
            {
                DispTimer.Interval = new System.TimeSpan(0, 0, 0, 0, 100);
            }
            DispTimer.Tick += CursorTimer_Tick;
            DispTimer.IsEnabled = true;

            DragDrop.SetAllowDrop(ConsoleScreen, true);
            ConsoleScreen.AddHandler(DragDrop.DragEnterEvent, DragFileEnter);
            ConsoleScreen.AddHandler(DragDrop.DragOverEvent, DragFileEnter);
            ConsoleScreen.AddHandler(DragDrop.DropEvent, DragFileExit);

            ConsoleScreen.PointerMoved += OnMouseMove;
            ConsoleScreen.PointerWheelChanged += OnMouseWheel;
            ConsoleScreen.PointerLeave += OnMouseMove0;
            ConsoleScreen.PointerPressed += OnMouseDown;
            ConsoleScreen.PointerReleased += OnMouseUp;

            ConsoleKeyCapture.GotFocus += OnFocus1;
            ConsoleKeyCapture.LostFocus += OnFocus0;
        }

        protected void OnFocus1(object sender, GotFocusEventArgs e)
        {
            FormFocusEvent(true);
        }

        protected void OnFocus0(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            FormFocusEvent(false);
        }

        protected void OnMouseDown(object sender, PointerPressedEventArgs e)
        {
            switch (e.MouseButton)
            {
                case MouseButton.Left:
                    MouseDown(1);
                    break;
                case MouseButton.Middle:
                    MouseDown(2);
                    break;
                case MouseButton.Right:
                    MouseDown(3);
                    break;
            }
        }
        protected void OnMouseUp(object sender, PointerReleasedEventArgs e)
        {
            switch (e.MouseButton)
            {
                case MouseButton.Left:
                    MouseUp(1);
                    break;
                case MouseButton.Middle:
                    MouseUp(2);
                    break;
                case MouseButton.Right:
                    MouseUp(3);
                    break;
            }
        }

        protected void OnMouseMove0(object sender, PointerEventArgs e)
        {
            MouseMove(-1, -1);
        }

        protected void OnMouseMove(object sender, PointerEventArgs e)
        {
            MouseMove((int)(e.GetPosition(ConsoleScreen).X), (int)(e.GetPosition(ConsoleScreen).Y));
        }

        protected void OnMouseWheel(object sender, PointerWheelEventArgs e)
        {
            if (e.Delta.Y > 0) { MouseDown(4); }
            if (e.Delta.Y < 0) { MouseDown(5); }
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

            while (FormCtrlSetParamQueueV.Count > 0)
            {
                FormCtrlSetParam(FormCtrlSetParamQueueP[0], FormCtrlSetParamQueueV[0]);
                FormCtrlSetParamQueueP.RemoveAt(0);
                FormCtrlSetParamQueueV.RemoveAt(0);
            }

            bool NeedRefresh = false;

            if (TimerTick2)
            {
                ScreenBmpCounter++;
                if (ScreenBmpCounter >= 5)
                {
                    ScreenBmpCounter = 0;
                    ScreenBmpDisp = 1 - ScreenBmpDisp;
                    NeedRefresh = true;
                }
            }
            Form_.MouseIsActiveX = MouseIsActiveX;
            if (MouseIsActiveDisp != MouseIsActiveX)
            {
                if (MouseIsActiveX)
                {
                    Form_.Cursor = new Cursor(StandardCursorType.None);
                }
                else
                {
                    Form_.Cursor = Cursor.Default;
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
                FormCtrlRefresh();
            }
            if (TimerTick2)
            {
                if ((!ConsoleKeyCapture.IsFocused) && (Form_.IsFocused))
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
            ConsoleKeyCapture.GotFocus -= OnFocus1;
            ConsoleKeyCapture.LostFocus -= OnFocus0;
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
