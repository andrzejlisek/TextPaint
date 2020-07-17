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
		public ScreenConsole(Core Core__)
		{
			Core_ = Core__;
		}

        public override void PutChar_(int X, int Y, char C, int ColorBack, int ColorFore)
        {
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
			Console.MoveBufferArea(SrcX, SrcY, W, H, DstX, DstY);
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
        
        public override void StartApp()
        {
        	Console.OutputEncoding = System.Text.Encoding.Unicode;
        	Console.InputEncoding = System.Text.Encoding.Unicode;
        	
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
