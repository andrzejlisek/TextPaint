using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace TextPaint;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.


    public static ScreenWindowAvalonia ScreenWindowAvalonia_ = null;

    [STAThread]
    public static void Main(string[] args)
    {
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
