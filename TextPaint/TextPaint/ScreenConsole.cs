/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-07
 * Time: 23:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace TextPaint
{
	/// <summary>
	/// Description of ScreenConsole.
	/// </summary>
	public class ScreenConsole : Screen
	{
        string ConIEnc = "";
        string ConOEnc = "";

        public ScreenConsole(Core Core__, ConfigFile CF)
		{
			Core_ = Core__;
            ConIEnc = CF.ParamGetS("ConInputEncoding");
            ConOEnc = CF.ParamGetS("ConOutputEncoding");
        }

        public override void PutChar_(int X, int Y, char C, int ColorBack, int ColorFore)
        {
            if (C < ' ')
            {
                return;
            }
            Console.SetCursorPosition(X, Y);
        	switch (ColorBack)
        	{
        		case 0: Console.BackgroundColor = ConsoleColor.Black; break;
        		case 1: Console.BackgroundColor = ConsoleColor.DarkGray; break;
        		case 2: Console.BackgroundColor = ConsoleColor.Gray; break;
        		case 3: Console.BackgroundColor = ConsoleColor.White; break;
        	}
        	switch (ColorFore)
        	{
        		case 0: Console.ForegroundColor = ConsoleColor.Black; break;
        		case 1: Console.ForegroundColor = ConsoleColor.DarkGray; break;
        		case 2: Console.ForegroundColor = ConsoleColor.Gray; break;
        		case 3: Console.ForegroundColor = ConsoleColor.White; break;
        	}
        	Console.Write(C);
        }

        public override void Move(int SrcX, int SrcY, int DstX, int DstY, int W, int H)
        {
            if (UseMemo != 2)
            {
                try
                {
                    Console.MoveBufferArea(SrcX, SrcY, W, H, DstX, DstY);
                }
                catch
                {
                }
            }
			MemoRepaint(SrcX, SrcY, DstX, DstY, W, H);
        }

        public override void SetStatusText(string StatusText)
        {
			Console.BackgroundColor = ToolBack;
			Console.ForegroundColor = ToolFore;
			Console.SetCursorPosition(0, WinH - 1);
			Console.Write("".PadLeft(WinW - 1));
			Console.SetCursorPosition(0, WinH - 1);
			Console.Write(StatusText);
        }

		ConsoleColor ToolBack = ConsoleColor.White;
		ConsoleColor ToolFore = ConsoleColor.Black;
        
        public override bool WindowResize()
        {
			if ((WinW != Console.WindowWidth) || (WinH != Console.WindowHeight))
			{
				WinW = Console.WindowWidth;
				WinH = Console.WindowHeight;
				
				MemoPrepare();

				if ((Console.BufferHeight > WinW) || (Console.BufferHeight > WinH))
				{
					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.Black;
					Console.Clear();
					Console.BackgroundColor = ToolBack;
					Console.ForegroundColor = ToolFore;
					Console.SetCursorPosition(WinW - 1, WinH - 1);
					Console.Write(" ");
				}
				else
				{
					Console.BackgroundColor = ToolBack;
					Console.ForegroundColor = ToolFore;
					Console.Clear();
				}

				return true;
			}
			else
			{
				return false;
			}
        }

        private System.Text.Encoding StrToEnc(string Val)
        {
            if (Val == "")
            {
                return System.Text.Encoding.Default;
            }
            bool DigitOnly = true;
            for (int i = 0; i < Val.Length; i++)
            {
                if ((Val[i] < '0') || (Val[i] > '9'))
                {
                    DigitOnly = false;
                }
            }
            try
            {
                if (DigitOnly)
                {
                    return System.Text.Encoding.GetEncoding(int.Parse(Val));
                }
                else
                {
                    return System.Text.Encoding.GetEncoding(Val);
                }
            }
            catch
            {
                return System.Text.Encoding.Default;
            }
        }

        public override void StartApp()
        {
            if (ConIEnc != "")
            {
                Console.InputEncoding = StrToEnc(ConIEnc);
            }
            if (ConOEnc != "")
            {
                Console.OutputEncoding = StrToEnc(ConOEnc);
            }

            WinW = -1;
        	WinH = -1;
        	AppWorking = true;
			Core_.WindowResize();
			Core_.ScreenRefresh(true);
            while (AppWorking)
        	{
                ConsoleKeyInfo CKI = Console.ReadKey(true);
                Core_.CoreEvent(CKI.Key.ToString(), CKI.KeyChar);
            }
            Console.Clear();
        }

        public override void SetCursorPosition(int X, int Y)
        {
        	if (X < 0)
        	{
        		X = 0;
        	}
        	if (Y < 0)
        	{
        		Y = 0;
        	}
        	if (X >= WinW)
        	{
        		X = WinW - 1;
        	}
        	if (Y >= WinH)
        	{
        		Y = WinH - 1;
        	}
			Console.SetCursorPosition(X, Y);
        }
	}
}
