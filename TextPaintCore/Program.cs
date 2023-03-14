using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Text;

namespace TextPaint;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.


    public static ScreenWindowGUI ScreenWindowGUI_ = null;

    [STAThread]
    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        Core Core_ = new Core();
        if (args.Length > 0)
        {
            Core_.Init(args[0], args);
        }
        else
        {
            Core_.Init("", args);
        }
    }
}
