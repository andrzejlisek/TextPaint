using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TextPaint.Views;

public partial class MainWindow : Window
{
    public Avalonia.Controls.Image ConsoleScreen = null;
    public Avalonia.Controls.Shapes.Rectangle ConsoleCursor = null;
    public Avalonia.Controls.Canvas ConsoleCanvas = null;
    public Avalonia.Controls.TextBox ConsoleKeyCapture = null;
    public Avalonia.Media.Imaging.WriteableBitmap BitmapW;

    public MainWindow()
    {
        InitializeComponent();
        ConsoleScreen = this.FindControl<Avalonia.Controls.Image>("ConsoleScreen_");
        ConsoleCursor = this.FindControl<Avalonia.Controls.Shapes.Rectangle>("ConsoleCursor_");
        ConsoleCanvas = this.FindControl<Avalonia.Controls.Canvas>("ConsoleCanvas_");
        ConsoleKeyCapture = this.FindControl<Avalonia.Controls.TextBox>("ConsoleKeyCapture_");
        ConsoleScreen.Stretch = Avalonia.Media.Stretch.Fill;
        this.Opened += WinShown;
        this.Width = 640;
        this.Height = 480;
    }

    void WinShown(object sender, EventArgs e)
    {
        Program.ScreenWindowAvalonia_.StartAppFormFinish(this);
        ConsoleKeyCapture.AddHandler(KeyDownEvent, WinKey1, RoutingStrategies.Tunnel);
        ConsoleKeyCapture.AddHandler(TextInputEvent, WinKey2, RoutingStrategies.Tunnel);
        this.DoubleTapped += DbClick;
    }

    void DbClick(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.FullScreen)
        {
            WindowState = WindowState.Normal;
        }
        else
        {
            WindowState = WindowState.FullScreen;
        }
    }

    public int GetWidth()
    {
        return (int)this.ClientSize.Width;
    }

    public int GetHeight()
    {
        return (int)this.ClientSize.Height;
    }

    public void SetSize(int W, int H)
    {
        Width = W;
        Height = H;
    }


    protected string KeyCode_ = "";
    protected bool KeyShift_ = false;
    protected bool KeyCtrl_ = false;
    protected bool KeyAlt_ = false;

    void WinKey1(object sender, KeyEventArgs e)
    {
        KeyAlt_ = false;
        KeyCtrl_ = false;
        KeyShift_ = false;
        if ((e.KeyModifiers & KeyModifiers.Shift) > 0) KeyShift_ = true;
        if ((e.KeyModifiers & KeyModifiers.Control) > 0) KeyCtrl_ = true;
        if ((e.KeyModifiers & KeyModifiers.Alt) > 0) KeyAlt_ = true;


        KeyCode_ = e.Key.ToString();
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
                Program.ScreenWindowAvalonia_.WinKey(KeyCode_, '\0', KeyShift_, KeyCtrl_, KeyAlt_);
                break;
            case "Escape":
                Program.ScreenWindowAvalonia_.WinKey(KeyCode_, (char)27, KeyShift_, KeyCtrl_, KeyAlt_);
                break;
            case "Tab":
                Program.ScreenWindowAvalonia_.WinKey(KeyCode_, (char)9, KeyShift_, KeyCtrl_, KeyAlt_);
                break;
            case "Back":
                Program.ScreenWindowAvalonia_.WinKey(KeyCode_, (char)8, KeyShift_, KeyCtrl_, KeyAlt_);
                break;
            case "Return":
                Program.ScreenWindowAvalonia_.WinKey(KeyCode_, (char)13, KeyShift_, KeyCtrl_, KeyAlt_);
                break;
        }
    }

    void WinKey2(object sender, TextInputEventArgs e)
    {
        string Txt = e.Text;
        ConsoleKeyCapture.Text = "";
        for (int I = 0; I < Txt.Length; I++)
        {
            Program.ScreenWindowAvalonia_.WinKey(KeyCode_, Txt[I], KeyShift_, KeyCtrl_, KeyAlt_);
        }
    }

}