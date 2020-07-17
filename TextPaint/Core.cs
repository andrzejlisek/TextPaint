/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-07
 * Time: 23:08
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextPaint
{
	/// <summary>
	/// Description of Core.
	/// </summary>
	public class Core
	{
		public Screen Screen_;

		public bool UseWindow = false;
		
		public Core()
		{
		}

		const int MaxlineSize = 10000;
		
		List<StringBuilder> TextBuffer = new List<StringBuilder>();
		List<string> ScrCharType;
		List<string> ScrCharStr;

		List<List<char>> ScrCharTypeDisp;
		List<List<char>> ScrCharStrDisp;

		public int CursorXSize = 0;
		public int CursorYSize = 0;
		
		public int CursorX = 0;
		public int CursorY = 0;
		int DisplayX = 0;
		int DisplayY = 0;
		int WinW;
		int WinH;
		int WinTxtW;
		int WinTxtH;
		int WorkMode = 0;

		int CursorType = 0;
		
		int FrameLastCross = 0;
		int TextMoveDir = 0;
		
		int TextInsDelMode = 0;
		
		public List<char> BlankDispLine()
		{
			List<char> T = new List<char>();
			for (int i = 0; i < WinTxtW; i++)
			{
				T.Add('\t');
			}
			return T;
		}
		
		public void FileLoad(string FileName)
		{
			TextBuffer.Clear();
			if (FileName == "")
			{
				return;
			}
			try
			{
				FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read);
				StreamReader SR = new StreamReader(FS);
				string Buf = SR.ReadLine();
				while (Buf != null)
				{
					TextBuffer.Add(new StringBuilder(Buf.TrimEnd()));
					Buf = SR.ReadLine();
				}
				SR.Close();
				FS.Close();
				TextBufferTrim();
			}
			catch
			{
				
			}
		}
		
		public void FileSave(string FileName)
		{
			if (FileName == "")
			{
				return;
			}
			try
			{
				File.Delete(FileName);
				FileStream FS = new FileStream(FileName, FileMode.Create, FileAccess.Write);
				StreamWriter SW = new StreamWriter(FS);
				for (int i = 0; i < TextBuffer.Count; i++)
				{
					SW.WriteLine(TextBuffer[i]);
				}
				SW.Close();
				FS.Close();
			}
			catch
			{
				
			}
		}
		
		public void TextDisplay(int Mode)
		{
			int I1 = 0;
			int I2 = (WinTxtH - 1);
			if (Mode == 1)
			{
				I2 = 0;
			}
			if (Mode == 2)
			{
				I1 = (WinTxtH - 1);
			}
			if (Mode >= MaxlineSize)
			{
				I1 = Mode - MaxlineSize;
				I2 = Mode - MaxlineSize;
			}
			
			if ((Mode < 3) || (Mode >= MaxlineSize))
			{
				for (int i = I1; i <= I2; i++)
				{
					if ((i + DisplayY) < TextBuffer.Count)
					{
						string S = TextBuffer[i + DisplayY].ToString();
						if (DisplayX > 0)
						{
							if (S.Length > DisplayX) 
							{
								S = S.Substring(DisplayX);
							}
							else
							{
								S = "";
							}
						}
						if (S.Length < WinTxtW)
						{
							ScrCharStr[i] = "".PadLeft(WinTxtW - S.Length, ' ');
							ScrCharStr[i] = S + ScrCharStr[i];
							ScrCharType[i] = ("".PadLeft(S.Length, '0')) + ("".PadLeft(WinTxtW - S.Length, '1'));
						}
						else
						{
							ScrCharStr[i] = S.Substring(0, WinTxtW);
							ScrCharType[i] = ("".PadLeft(WinTxtW, '0'));
						}
					}
					else
					{
						ScrCharStr[i] = "".PadLeft(WinTxtW, ' ');
						ScrCharType[i] = ("".PadLeft(WinTxtW, '2'));
					}
				}
			}
			else
			{
				int CurOffset = (Mode == 3) ? 0 : WinTxtW - 1;
				for (int i = I1; i <= I2; i++)
				{
					char ChType = '0';
					char ChStr = ' ';
					if ((i + DisplayY) < TextBuffer.Count)
					{
						if (TextBuffer[i + DisplayY].Length > (DisplayX + CurOffset))
						{
							ChStr = TextBuffer[i + DisplayY][DisplayX + CurOffset];
						}
						else
						{
							ChType = '1';
						}
					}
					else
					{
						ChType = '2';
					}
					if (Mode == 3)
					{
						ScrCharType[i] = ChType + ScrCharType[i].Substring(1);
						ScrCharStr[i] = ChStr + ScrCharStr[i].Substring(1);
					}
					else
					{
						ScrCharType[i] = ScrCharType[i].Substring(0, WinTxtW - 1) + ChType;
						ScrCharStr[i] = ScrCharStr[i].Substring(0, WinTxtW - 1) + ChStr;
					}
				}
			}
		}
		
		public void TextDisplayLine(int Y)
		{
			if (((Y - DisplayY) >= 0) && ((Y - DisplayY) < WinTxtH))
			{
				TextDisplay(MaxlineSize + (Y - DisplayY));
			}
		}
		
		public char CharGet(int X, int Y)
		{
			if ((TextBuffer.Count > Y) && (Y >= 0))
			{
				if ((TextBuffer[Y].Length > X) && (X >= 0))
				{
					return TextBuffer[Y][X];
				}
			}
			return ' ';
		}
		
		public void CharPut(int X, int Y, char C)
		{
			if (X < 0)
			{
				return;
			}
			if (Y < 0)
			{
				return;
			}
			while (TextBuffer.Count <= Y)
			{
				TextBuffer.Add(new StringBuilder());
				TextDisplayLine(TextBuffer.Count - 1);
			}
			if (TextBuffer[Y].Length > X)
			{
				TextBuffer[Y][X] = C;
				if (C == ' ')
				{
					TextBufferTrimLine(Y);
				}
			}
			else
			{
				if (C != ' ')
				{
					if (TextBuffer[Y].Length < X)
					{
						TextBuffer[Y].Append("".PadLeft(X - TextBuffer[Y].Length));
					}
					TextBuffer[Y].Append(C);
				}
			}
			while ((TextBuffer.Count > 0) && (TextBuffer[TextBuffer.Count - 1].Length == 0))
			{
				TextBuffer.RemoveAt(TextBuffer.Count - 1);
				TextDisplayLine(TextBuffer.Count);
			}
			TextDisplayLine(Y);
		}
		
		
		
		
		

		public void CursorChar_(int X, int Y, bool Show)
		{
			if ((X >= 0) && (Y >= 0) && (X < WinTxtW) && (Y < WinTxtH))
			{
				CursorChar(X, Y, Show);
			}
		}

		public void CursorChar(int X, int Y, bool Show)
		{
			StringBuilder Sb = new StringBuilder(ScrCharType[Y]);
			if (Show)
			{
				if (Sb[X] < '3')
				{
					Sb[X] = (char)((int)(Sb[X]) + 3);
				}
			}
			else
			{
				if (Sb[X] > '2')
				{
					Sb[X] = (char)((int)(Sb[X]) - 3);
				}
			}
			ScrCharType[Y] = Sb.ToString();
		}
		
		public void CursorLine(bool Show)
		{
			int XX = CursorX - DisplayX;
			int YY = CursorY - DisplayY;
			if (WorkMode == 3)
			{
				int X1 = Math.Min(XX, XX + CursorXSize);
				int X2 = Math.Max(XX, XX + CursorXSize);
				int Y1 = Math.Min(YY, YY + CursorYSize);
				int Y2 = Math.Max(YY, YY + CursorYSize);
				
				X1 = Math.Max(Math.Min(X1, WinTxtW - 1), 0);
				X2 = Math.Max(Math.Min(X2, WinTxtW - 1), 0);
				Y1 = Math.Max(Math.Min(Y1, WinTxtH - 1), 0);
				Y2 = Math.Max(Math.Min(Y2, WinTxtH - 1), 0);
				
				for (int Y = Y1; Y <= Y2; Y++)
				{
					for (int X = X1; X <= X2; X++)
					{
						CursorChar(X, Y, Show);
					}
				}
			}
			if (WorkMode == 4)
			{
				int X1 = Math.Min(XX, XX + CursorXSize);
				int X2 = Math.Max(XX, XX + CursorXSize);
				int Y1 = Math.Min(YY, YY + CursorYSize);
				int Y2 = Math.Max(YY, YY + CursorYSize);
				
				
				for (int X_ = X1; X_ <= X2; X_++)
				{
					for (int Y_ = Y1; Y_ <= Y2; Y_++)
					{
						int X__ = X_ - Y_ + YY;
						int Y__ = Y_ + X_ - XX;
						CursorChar_(X__, Y__, Show);
						
						switch (Semigraphics_.DiamondType)
						{
							case 0:
								if ((X_ < X2) && (Y_ > Y1))
								{
									CursorChar_(X__ + 1, Y__, Show);
								}
								break;
							case 1:
								CursorChar_(X__ + 1, Y__, Show);
								break;
							case 2:
								CursorChar_(X__, Y__ + 1, Show);
								break;
							case 3:
								CursorChar_(X__ - 1, Y__, Show);
								break;
							case 4:
								CursorChar_(X__, Y__ - 1, Show);
								break;
							case 5:
								CursorChar_(X__ + 1, Y__, Show);
								CursorChar_(X__, Y__ - 1, Show);
								CursorChar_(X__ + 1, Y__ - 1, Show);
								break;
							case 6:
								CursorChar_(X__ + 1, Y__, Show);
								CursorChar_(X__, Y__ + 1, Show);
								CursorChar_(X__ + 1, Y__ + 1, Show);
								break;
							case 7:
								CursorChar_(X__ - 1, Y__, Show);
								CursorChar_(X__, Y__ + 1, Show);
								CursorChar_(X__ - 1, Y__ + 1, Show);
								break;
							case 8:
								CursorChar_(X__ - 1, Y__, Show);
								CursorChar_(X__, Y__ - 1, Show);
								CursorChar_(X__ - 1, Y__ - 1, Show);
								break;
						}
					}
				}
				

			}

			CursorChar(XX, YY, Show);
			if ((CursorType == 1) || (CursorType == 3))
			{
				for (int X = 0; X < WinTxtW; X++)
				{
					CursorChar(X, YY, Show);
				}
				for (int Y = 0; Y < WinTxtH; Y++)
				{
					CursorChar(XX, Y, Show);
				}
			}
			if ((CursorType == 2) || (CursorType == 3))
			{
				int I1 = 0 - Math.Min(XX, YY);
				int I2 = 0 + Math.Min(WinTxtW - XX - 1, WinTxtH - YY - 1);
				for (int i = I1; i <= I2; i++)
				{
					CursorChar(XX + i, YY + i, Show);
				}
				I1 = 0 - Math.Min(WinTxtW - XX - 1, YY);
				I2 = 0 + Math.Min(XX, WinTxtH - YY - 1);
				for (int i = I1; i <= I2; i++)
				{
					CursorChar(XX - i, YY + i, Show);
				}
			}
		}

		
		public void CursorEquivPos(int Dir)
		{
			if (WorkMode == 3)
			{
				if (((CursorXSize < 0) & (CursorYSize < 0)) || ((CursorXSize > 0) & (CursorYSize > 0)))
				{
					if (Dir > 0) { CursorX += CursorXSize; CursorXSize = 0 - CursorXSize; }
					if (Dir < 0) { CursorY += CursorYSize; CursorYSize = 0 - CursorYSize; }
					CursorLimit();
					return;
				}
				if (((CursorXSize > 0) & (CursorYSize < 0)) || ((CursorXSize < 0) & (CursorYSize > 0)))
				{
					if (Dir > 0) { CursorY += CursorYSize; CursorYSize = 0 - CursorYSize; }
					if (Dir < 0) { CursorX += CursorXSize; CursorXSize = 0 - CursorXSize; }
					CursorLimit();
					return;
				}
				if ((CursorXSize == 0) || (CursorYSize == 0))
				{
					CursorX += CursorXSize; CursorXSize = 0 - CursorXSize;
					CursorY += CursorYSize; CursorYSize = 0 - CursorYSize;
					CursorLimit();
					return;
				}
			}
			if (WorkMode == 4)
			{
				if (((CursorXSize < 0) & (CursorYSize < 0)) || ((CursorXSize > 0) & (CursorYSize > 0)))
				{
					if (Dir > 0) { CursorX += CursorXSize; CursorY += CursorXSize; CursorXSize = 0 - CursorXSize; }
					if (Dir < 0) { CursorX -= CursorYSize; CursorY += CursorYSize; CursorYSize = 0 - CursorYSize; }
					CursorLimit();
					return;
				}
				if (((CursorXSize > 0) & (CursorYSize < 0)) || ((CursorXSize < 0) & (CursorYSize > 0)))
				{
					if (Dir > 0) { CursorX -= CursorYSize; CursorY += CursorYSize; CursorYSize = 0 - CursorYSize; }
					if (Dir < 0) { CursorX += CursorXSize; CursorY += CursorXSize; CursorXSize = 0 - CursorXSize; }
					CursorLimit();
					return;
				}
				if (((CursorXSize == 0) & (CursorYSize != 0)) || ((CursorXSize != 0) & (CursorYSize == 0)))
				{
					CursorX += (CursorXSize - CursorYSize);
					CursorY += (CursorXSize + CursorYSize);
					CursorXSize = 0 - CursorXSize;
					CursorYSize = 0 - CursorYSize;
					CursorLimit();
					return;
				}
			}
		}

		
		public void CursorLimit()
		{
			if (CursorX < 0)
			{
				CursorX = 0;
			}
			if (CursorY < 0)
			{
				CursorY = 0;
			}
			MoveCursor(-1);
		}


		public void MoveCursor(int Direction)
		{
			switch (Direction)
			{
				case 0:
					if (CursorY > 0)
					{
						CursorY--;
					}
					break;
				case 1:
					{
						CursorY++;
					}
					break;
				case 2:
					if (CursorX > 0)
					{
						CursorX--;
					}
					break;
				case 3:
					{
						CursorX++;
					}
					break;
				case 4:
					MoveCursor(0);
					MoveCursor(3);
					break;
				case 5:
					MoveCursor(1);
					MoveCursor(2);
					break;
				case 6:
					MoveCursor(0);
					MoveCursor(2);
					break;
				case 7:
					MoveCursor(1);
					MoveCursor(3);
					break;
			}
			while (DisplayY > CursorY)
			{
				DisplayY--;
				Screen_.Move(0, 0, 0, 1, WinTxtW, WinTxtH - 1);
				ScrCharType.Insert(0, "");
				ScrCharType.RemoveAt(WinTxtH);
				ScrCharStr.Insert(0, "");
				ScrCharStr.RemoveAt(WinTxtH);
				ScrCharTypeDisp.Insert(0, BlankDispLine());
				ScrCharTypeDisp.RemoveAt(WinTxtH);
				ScrCharStrDisp.Insert(0, BlankDispLine());
				ScrCharStrDisp.RemoveAt(WinTxtH);
				TextDisplay(1);
			}
			while (DisplayY < (CursorY - WinTxtH + 1))
			{
				DisplayY++;
				Screen_.Move(0, 1, 0, 0, WinTxtW, WinTxtH - 1);
				ScrCharType.RemoveAt(0);
				ScrCharType.Add("");
				ScrCharStr.RemoveAt(0);
				ScrCharStr.Add("");
				ScrCharTypeDisp.RemoveAt(0);
				ScrCharTypeDisp.Add(BlankDispLine());
				ScrCharStrDisp.RemoveAt(0);
				ScrCharStrDisp.Add(BlankDispLine());
				TextDisplay(2);
			}
			while (DisplayX > CursorX)
			{
				DisplayX--;
				Screen_.Move(0, 0, 1, 0, WinTxtW - 1, WinTxtH);
				for (int i = 0; i < WinTxtH; i++)
				{
					ScrCharType[i] = "0" + ScrCharType[i].Substring(0, WinTxtW - 1);
					ScrCharStr[i] = " " + ScrCharStr[i].Substring(0, WinTxtW - 1);
					ScrCharTypeDisp[i].Insert(0, ' ');
					ScrCharTypeDisp[i].RemoveAt(WinTxtW);
					ScrCharStrDisp[i].Insert(0, ' ');
					ScrCharStrDisp[i].RemoveAt(WinTxtW);
				}
				TextDisplay(3);
			}
			while (DisplayX < (CursorX - WinTxtW + 1))
			{
				DisplayX++;
				Screen_.Move(1, 0, 0, 0, WinTxtW - 1, WinTxtH);
				for (int i = 0; i < WinTxtH; i++)
				{
					ScrCharType[i] = ScrCharType[i].Substring(1) + "0";
					ScrCharStr[i] = ScrCharStr[i].Substring(1) + " ";
					ScrCharTypeDisp[i].Add(' ');
					ScrCharTypeDisp[i].RemoveAt(0);
					ScrCharStrDisp[i].Add(' ');
					ScrCharStrDisp[i].RemoveAt(0);
				}
				TextDisplay(4);
			}
		}



		Semigraphics Semigraphics_;
		Clipboard Clipboard_;
		
		string CurrentFileName = "";
		
		public void Init(string CurrentFileName_)
		{
			Semigraphics_ = new Semigraphics(this);
			Clipboard_ = new Clipboard(this);
			
			ConfigFile CF = new ConfigFile();
			CF.FileLoad("Config.txt");
			Semigraphics_.Init(CF);
			
			CurrentFileName = CurrentFileName_;
			
			WinW = -1;
			WinH = -1;
			ScrCharType = new List<string>();
			ScrCharStr = new List<string>();
			TextBuffer = new List<StringBuilder>();
			ScrCharTypeDisp = new List<List<char>>();
			ScrCharStrDisp = new List<List<char>>();
			TextBuffer.Clear();
			FileLoad(CurrentFileName);
			UseWindow = CF.ParamGetB("WinUse");
			
			if (UseWindow)
			{
				Screen_ = new ScreenWindow(this, CF, Console.WindowWidth, Console.WindowHeight);
				Screen_.UseMemo = false;
			}
			else
			{
				Screen_ = new ScreenConsole(this);
				Screen_.UseMemo = CF.ParamGetB("ConUseMemo");
			}
			Screen_.StartApp();
		}

		
		public void TextRepaint(bool Force)
		{
			for (int Y = 0; Y < WinTxtH; Y++)
			{
				for (int X = 0; X < WinTxtW; X++)
				{
					if (Force || (ScrCharStrDisp[Y][X] != ScrCharStr[Y][X]) || (ScrCharTypeDisp[Y][X] != ScrCharType[Y][X]))
					{
						int ColorB = 0;
						int ColorF = 0;
						switch (ScrCharType[Y][X])
						{
							case '0':
								ColorB = 0;
								ColorF = 3;
								break;
							case '1':
								ColorB = 1;
								ColorF = 1;
								break;
							case '2':
								ColorB = 2;
								ColorF = 2;
								break;
							case '3':
							case '4':
							case '5':
								ColorB = 3;
								ColorF = 0;
								break;
						}
						ScrCharTypeDisp[Y][X] = ScrCharType[Y][X];
						ScrCharStrDisp[Y][X] = ScrCharStr[Y][X];
						Screen_.PutChar(X, Y, ScrCharStr[Y][X], ColorB, ColorF);
					}
				}
			}
		}


		public void WindowResize()
		{
			if (Screen_.WindowResize())
			{
				WinW = Screen_.WinW;
				WinH = Screen_.WinH;
				WinTxtW = WinW;
				WinTxtH = WinH - 1;
				
				ScrCharType.Clear();
				ScrCharStr.Clear();
				ScrCharTypeDisp.Clear();
				ScrCharStrDisp.Clear();
				for (int i = 0; i < WinTxtH; i++)
				{
					ScrCharType.Add("");
					ScrCharStr.Add("");
					ScrCharTypeDisp.Add(BlankDispLine());
					ScrCharStrDisp.Add(BlankDispLine());
				}
				TextDisplay(0);
				CursorLimit();
			}
		}
		
		public void ScreenRefresh(bool Force)
		{
			WindowResize();
			CursorLine(true);
			TextRepaint(Force);
			StringBuilder StatusText = new StringBuilder();
			StatusText.Append(CursorX + ":" + CursorY + "   ");

			switch (WorkMode)
			{
				case 0:
					if (TextMoveDir == 0) { StatusText.Append("Text-R"); }
					if (TextMoveDir == 1) { StatusText.Append("Text-D"); }
					if (TextMoveDir == 2) { StatusText.Append("Text-L"); }
					if (TextMoveDir == 3) { StatusText.Append("Text-U"); }
					break;
				case 1:
					StatusText.Append("Frame   " + Semigraphics_.FrameName);
					break;
				case 2:
					StatusText.Append("Char   ");
					if (Semigraphics_.DrawCharEraseFrame)
					{
						StatusText.Append(Semigraphics_.FrameName);
					}
					break;
				case 3:
					StatusText.Append("Rect " + (Math.Abs(CursorXSize) + 1) + "x" + (Math.Abs(CursorYSize) + 1) + "   " + Semigraphics_.FrameName1);
					break;
				case 4:
					StatusText.Append("Dia " + (Math.Abs(CursorXSize) + 1) + "x" + (Math.Abs(CursorYSize) + 1) + "   " + Semigraphics_.FrameName2);
					break;
			}
			if ((WorkMode == 0) || (WorkMode == 3))
			{
				switch (TextInsDelMode)
				{
					case 0: StatusText.Append("  H-block"); break;
					case 1: StatusText.Append("  V-block"); break;
					case 2: StatusText.Append("  H-line"); break;
					case 3: StatusText.Append("  V-line"); break;
				}
			}
			
			StatusText.Append("    Draw: " + Semigraphics_.DrawCharI.ToString("X").PadLeft(4, '0') + " " + Semigraphics_.DrawCharC);


			Screen_.SetStatusText(StatusText.ToString());
			Screen_.SetCursorPosition(CursorX - DisplayX, CursorY - DisplayY);
		}
		
		
		public void CoreEvent(string KeyName, char KeyChar)
		{
			if (Semigraphics_.SelectCharState)
			{
				Semigraphics_.SelectCharEvent(KeyName, KeyChar);
				return;
			}
			
			CursorLine(false);
			switch (KeyName)
			{
				case "F11":
					Semigraphics_.SelectCharInit();
					return;
				case "Tab":
					CursorType++;
					if (CursorType == 4)
					{
						CursorType = 0;
					}
					break;
				case "Escape":
					Clipboard_.TextClipboardClear();
					if (WorkMode == 0)
					{
						TextMoveDir++;
						if (TextMoveDir == 4)
						{
							TextMoveDir = 0;
						}
					}
					WorkMode = 0;
					break;
				case "F1":
					Clipboard_.TextClipboardClear();
					WorkMode = 1;
					break;
				case "F2":
					Clipboard_.TextClipboardClear();
					if (WorkMode == 2)
					{
						Semigraphics_.DrawCharEraseFrame = !Semigraphics_.DrawCharEraseFrame;
					}
					WorkMode = 2;
					break;
				case "F3":
					Clipboard_.TextClipboardClear();
					WorkMode = 3;
					break;
				case "F4":
					Clipboard_.TextClipboardClear();
					if (WorkMode == 4)
					{
						Semigraphics_.DiamondType++;
						if (Semigraphics_.DiamondType == 9)
						{
							Semigraphics_.DiamondType = 0;
						}
					}
					WorkMode = 4;
					break;
					
				case "F7":
					FileSave(CurrentFileName);
					break;
				case "F8":
					FileLoad(CurrentFileName);
					TextDisplay(0);
					ScreenRefresh(true);
					break;
					
				case "F12":
					Screen_.AppWorking = false;
					return;
			}

			switch (WorkMode)
			{
				case 0: // Edit text
					{
						switch (KeyName)
						{
							case "UpArrow":
							case "Up":
								MoveCursor(0);
								break;
							case "DownArrow":
							case "Down":
								MoveCursor(1);
								break;
							case "LeftArrow":
							case "Left":
								MoveCursor(2);
								break;
							case "RightArrow":
							case "Right":
								MoveCursor(3);
								break;
							case "PageUp":
								MoveCursor(4);
								break;
							case "End":
								MoveCursor(5);
								break;
							case "Home":
								MoveCursor(6);
								break;
							case "PageDown":
							case "Next":
								MoveCursor(7);
								break;

							case "Enter":
							case "Return":
								TextInsDelMode++;
								if (TextInsDelMode == 4)
								{
									TextInsDelMode = 0;
								}
								break;
							case "Insert":
								TextInsert(CursorX, CursorY, 0, 0);
								break;
							case "Delete":
								TextDelete(CursorX, CursorY, 0, 0);
								break;

							case "Backspace":
							case "Back":
								{
									if (TextMoveDir == 0) { MoveCursor(2); }
									if (TextMoveDir == 1) { MoveCursor(0); }
									if (TextMoveDir == 2) { MoveCursor(3); }
									if (TextMoveDir == 3) { MoveCursor(1); }
								}
								break;
								
							default:
								if (KeyChar >= 32)
								{
									CharPut(CursorX, CursorY, KeyChar);
									if (TextMoveDir == 0) { MoveCursor(3); }
									if (TextMoveDir == 1) { MoveCursor(1); }
									if (TextMoveDir == 2) { MoveCursor(2); }
									if (TextMoveDir == 3) { MoveCursor(0); }
								}
								break;
						}
					}
					break;
				case 1: // Draw frame line
					{
						switch (KeyName)
						{
							case "D1":
								Semigraphics_.SetFrameNext(1);
								break;
							case "D2":
								Semigraphics_.SetFrameNext(2);
								break;
							case "UpArrow":
							case "Up":
								switch (FrameLastCross)
								{
									case 0:
										{
											Semigraphics_.FrameCharPut(0);
										}
										break;
									case 3:
										{
											Semigraphics_.FrameCharPut(6);
										}
										break;
									case 1:
										{
											Semigraphics_.FrameCharPut(4);
										}
										break;
								}
								MoveCursor(0);
								FrameLastCross = 0;
								break;
							case "DownArrow":
							case "Down":
								switch (FrameLastCross)
								{
									case 0:
										{
											Semigraphics_.FrameCharPut(1);
										}
										break;
									case 2:
										{
											Semigraphics_.FrameCharPut(5);
										}
										break;
									case 4:
										{
											Semigraphics_.FrameCharPut(7);
										}
										break;
								}
								MoveCursor(1);
								FrameLastCross = 0;
								break;
							case "LeftArrow":
							case "Left":
								switch (FrameLastCross)
								{
									case 0:
										{
											Semigraphics_.FrameCharPut(2);
										}
										break;
									case 2:
										{
											Semigraphics_.FrameCharPut(5);
										}
										break;
									case 3:
										{
											Semigraphics_.FrameCharPut(6);
										}
										break;
								}
								MoveCursor(2);
								FrameLastCross = 0;
								break;
							case "RightArrow":
							case "Right":
								switch (FrameLastCross)
								{
									case 0:
										{
											Semigraphics_.FrameCharPut(3);
										}
										break;
									case 1:
										{
											Semigraphics_.FrameCharPut(4);
										}
										break;
									case 4:
										{
											Semigraphics_.FrameCharPut(7);
										}
										break;
								}
								MoveCursor(3);
								FrameLastCross = 0;
								break;
							case "PageUp":
								Semigraphics_.FrameCharPut(4);
								MoveCursor(4);
								FrameLastCross = 1;
								break;
							case "End":
								Semigraphics_.FrameCharPut(5);
								MoveCursor(5);
								FrameLastCross = 2;
								break;
							case "Home":
								Semigraphics_.FrameCharPut(6);
								MoveCursor(6);
								FrameLastCross = 3;
								break;
							case "PageDown":
							case "Next":
								Semigraphics_.FrameCharPut(7);
								MoveCursor(7);
								FrameLastCross = 4;
								break;
						}
					}
					break;
				case 2: // Erase frame line or draw character
					{
						switch (KeyName)
						{
							case "D1":
								Semigraphics_.SetFrameNext(1);
								break;
							case "D2":
								Semigraphics_.SetFrameNext(2);
								break;
							case "UpArrow":
							case "Up":
								Semigraphics_.FrameCharErase();
								MoveCursor(0);
								break;
							case "DownArrow":
							case "Down":
								Semigraphics_.FrameCharErase();
								MoveCursor(1);
								break;
							case "LeftArrow":
							case "Left":
								Semigraphics_.FrameCharErase();
								MoveCursor(2);
								break;
							case "RightArrow":
							case "Right":
								Semigraphics_.FrameCharErase();
								MoveCursor(3);
								break;
							case "PageUp":
								Semigraphics_.FrameCharErase();
								MoveCursor(4);
								break;
							case "End":
								Semigraphics_.FrameCharErase();
								MoveCursor(5);
								break;
							case "Home":
								Semigraphics_.FrameCharErase();
								MoveCursor(6);
								break;
							case "PageDown":
							case "Next":
								Semigraphics_.FrameCharErase();
								MoveCursor(7);
								break;
						}
					}
					break;
				case 3: // Paint rentangle with cursor and size
				case 4: // Paint diamond with cursor and size
					{
						switch (KeyName)
						{
							case "UpArrow":
							case "Up":
								MoveCursor(0);
								break;
							case "DownArrow":
							case "Down":
								MoveCursor(1);
								break;
							case "LeftArrow":
							case "Left":
								MoveCursor(2);
								break;
							case "RightArrow":
							case "Right":
								MoveCursor(3);
								break;
							case "PageUp":
								MoveCursor(4);
								break;
							case "End":
								MoveCursor(5);
								break;
							case "Home":
								MoveCursor(6);
								break;
							case "PageDown":
							case "Next":
								MoveCursor(7);
								break;
								
							case "W":
								Clipboard_.TextClipboardClear();
								CursorYSize--;
								break;
							case "S":
								Clipboard_.TextClipboardClear();
								CursorYSize++;
								break;
							case "A":
								Clipboard_.TextClipboardClear();
								CursorXSize--;
								break;
							case "D":
								Clipboard_.TextClipboardClear();
								CursorXSize++;
								break;
								
							case "Q":
								CursorEquivPos(-1);
								break;
							case "E":
								CursorEquivPos(1);
								break;

								
							case "D1":
								if (WorkMode == 3)
								{
									Semigraphics_.SetFrameNext(1);
								}
								if (WorkMode == 4)
								{
									Semigraphics_.SetFrameNext(2);
								}
								break;
							case "D2": // Frame
								if (WorkMode == 3)
								{
									Semigraphics_.RectangleDraw(0, 0, CursorXSize, CursorYSize, 0);
								}
								if (WorkMode == 4)
								{
									Semigraphics_.DiamondDraw(0, 0, CursorXSize, CursorYSize, 0, -1);
								}
								break;
							case "D3": // Hollow
								if (WorkMode == 3)
								{
									Semigraphics_.RectangleDraw(0, 0, CursorXSize, CursorYSize, 1);
								}
								if (WorkMode == 4)
								{
									Semigraphics_.DiamondDraw(0, 0, CursorXSize, CursorYSize, 1, -1);
								}
								break;
							case "D4": // Filled
								if (WorkMode == 3)
								{
									Semigraphics_.RectangleDraw(0, 0, CursorXSize, CursorYSize, 2);
								}
								if (WorkMode == 4)
								{
									Semigraphics_.DiamondDraw(0, 0, CursorXSize, CursorYSize, 2, -1);
								}
								break;

							case "T":
							case "NumPad7":
								if (WorkMode == 3)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[2]);
								}
								if (WorkMode == 4)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[13]);
								}
								break;
							case "Y":
							case "NumPad8":
								if (WorkMode == 3)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[3]);
								}
								if (WorkMode == 4)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[14]);
								}
								break;
							case "U":
							case "NumPad9":
								if (WorkMode == 3)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[4]);
								}
								if (WorkMode == 4)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[15]);
								}
								break;
							case "G":
							case "NumPad4":
								if (WorkMode == 3)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[5]);
								}
								if (WorkMode == 4)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[16]);
								}
								break;
							case "H":
							case "NumPad5":
								if (WorkMode == 3)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[6]);
								}
								if (WorkMode == 4)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[17]);
								}
								break;
							case "J":
							case "NumPad6":
								if (WorkMode == 3)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[7]);
								}
								if (WorkMode == 4)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[18]);
								}
								break;
							case "B":
							case "NumPad1":
								if (WorkMode == 3)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[8]);
								}
								if (WorkMode == 4)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[19]);
								}
								break;
							case "N":
							case "NumPad2":
								if (WorkMode == 3)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[9]);
								}
								if (WorkMode == 4)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[20]);
								}
								break;
							case "M":
							case "NumPad3":
								if (WorkMode == 3)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[10]);
								}
								if (WorkMode == 4)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[21]);
								}
								break;

							case "I":
							case "Add":
								if (WorkMode == 3)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[1]);
								}
								if (WorkMode == 4)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[12]);
								}
								break;
							case "K":
							case "Subtract":
								if (WorkMode == 3)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[0]);
								}
								if (WorkMode == 4)
								{
									CharPut(CursorX, CursorY, Semigraphics_.FrameChar[11]);
								}
								break;
							case "Spacebar":
							case "Space":
							case "NumPad0":
								CharPut(CursorX, CursorY, ' ');
								break;

							case "Enter":
							case "Return":
								if (WorkMode == 3)
								{
									TextInsDelMode++;
									if (TextInsDelMode == 4)
									{
										TextInsDelMode = 0;
									}
								}
								break;
							case "Insert":
								if (WorkMode == 3)
								{
									TextInsert(CursorX, CursorY, CursorXSize, CursorYSize);
								}
								break;
							case "Delete":
								if (WorkMode == 3)
								{
									TextDelete(CursorX, CursorY, CursorXSize, CursorYSize);
								}
								break;
								
							case "C":
								Clipboard_.DiamondType = Semigraphics_.DiamondType;
								Clipboard_.TextClipboardWork(0, 0, CursorXSize, CursorYSize, (WorkMode == 4), false);
								break;
							case "V":
								Clipboard_.TextClipboardWork(0, 0, CursorXSize, CursorYSize, (WorkMode == 4), true);
								break;
						}
					}
					break;
			}
			ScreenRefresh(false);
		}
		
		
		
		void TextInsert(int X, int Y, int W, int H)
		{
			if (W < 0)
			{
				TextInsert(X + W, Y, 0 - W, H);
				return;
			}
			if (H < 0)
			{
				TextInsert(X, Y + H, W, 0 - H);
				return;
			}
			

			switch (TextInsDelMode)
			{
				case 0:
					for (int i = Y; i <= (Y + H); i++)
					{
						if (TextBuffer.Count > i)
						{
							if (TextBuffer[i].Length > X)
							{
								TextBuffer[i].Insert(X, "".PadLeft(W + 1, ' '));
							}
						}
					}
					break;
				case 1:
					if (Y < TextBuffer.Count)
					{
						for (int i = Y; i < TextBuffer.Count; i++)
						{
							if (TextBuffer[i].Length < (X + W + 2))
							{
								TextBuffer[i].Append("".PadLeft((X + W + 2) - TextBuffer[i].Length, ' '));
							}
						}
						for (int i = 0; i <= H; i++)
						{
							TextBuffer.Add(new StringBuilder("".PadLeft(X + W + 2)));
						}
						for (int i = (TextBuffer.Count - H - 1); i > Y; i--)
						{
							TextBuffer[i + H].Remove(X, W + 1);
							TextBuffer[i + H].Insert(X, TextBuffer[i - 1].ToString(X, W + 1));
							TextBufferTrimLine(i + H);
						}
						for (int i = Y; i <= (Y + H); i++)
						{
							TextBuffer[i].Remove(X, W + 1);
							TextBuffer[i].Insert(X, "".PadLeft(W + 1));
							TextBufferTrimLine(i);
						}
					}
					TextBufferTrim();
					break;
				case 2:
					for (int i = 0; i < TextBuffer.Count; i++)
					{
						if (TextBuffer[i].Length > X)
						{
							TextBuffer[i].Insert(X, "".PadLeft(W + 1, ' '));
						}
					}
					break;
				case 3:
					if (Y < TextBuffer.Count)
					{
						for (int i = 0; i <= H; i++)
						{
							TextBuffer.Insert(Y, new StringBuilder());
						}
					}
					break;
			}
			TextDisplay(0);
		}
		
		void TextDelete(int X, int Y, int W, int H)
		{
			if (W < 0)
			{
				TextDelete(X + W, Y, 0 - W, H);
				return;
			}
			if (H < 0)
			{
				TextDelete(X, Y + H, W, 0 - H);
				return;
			}
			
			switch (TextInsDelMode)
			{
				case 0:
					for (int i = Y; i <= (Y + H); i++)
					{
						if (TextBuffer.Count > i)
						{
							if (TextBuffer[i].Length > X)
							{
								if (TextBuffer[i].Length > (X + W + 1))
								{
									TextBuffer[i].Remove(X, W + 1);
								}
								else
								{
									TextBuffer[i].Remove(X, TextBuffer[i].Length - X);
								}
								TextBufferTrimLine(i);
							}
						}
					}
					break;
				case 1:
					if (Y < TextBuffer.Count)
					{
						for (int i = Y; i < TextBuffer.Count; i++)
						{
							if (TextBuffer[i].Length < (X + W + 2))
							{
								TextBuffer[i].Append("".PadLeft((X + W + 2) - TextBuffer[i].Length, ' '));
							}
						}
						for (int i = Y; i < TextBuffer.Count; i++)
						{
							TextBuffer[i].Remove(X, W + 1);
							if ((i + H + 1) < TextBuffer.Count)
							{
								TextBuffer[i].Insert(X, TextBuffer[i + H + 1].ToString(X, W + 1));
							}
							else
							{
								TextBuffer[i].Insert(X, "".PadLeft(W + 1));
							}
							TextBufferTrimLine(i);
						}
					}
					break;
				case 2:
					for (int i = 0; i < TextBuffer.Count; i++)
					{
						if (TextBuffer[i].Length > X)
						{
							if (TextBuffer[i].Length > (X + W + 1))
							{
								TextBuffer[i].Remove(X, W + 1);
							}
							else
							{
								TextBuffer[i].Remove(X, TextBuffer[i].Length - X);
							}
							TextBufferTrimLine(i);
						}
					}
					break;
				case 3:
					if (Y < TextBuffer.Count)
					{
						for (int i = 0; i <= H; i++)
						{
							if (TextBuffer.Count > Y)
							{
								TextBuffer.RemoveAt(Y);
							}
						}
					}
					break;
			}
			TextBufferTrim();
			TextDisplay(0);
		}
		
		void TextBufferTrim()
		{
			while ((TextBuffer.Count > 0) && (TextBuffer[TextBuffer.Count - 1].Length == 0))
			{
				TextBuffer.RemoveAt(TextBuffer.Count - 1);
			}
		}
		
		void TextBufferTrimLine(int i)
		{
			while ((TextBuffer[i].Length > 0) && (TextBuffer[i][TextBuffer[i].Length - 1] == ' '))
			{
				TextBuffer[i].Remove(TextBuffer[i].Length - 1, 1);
			}
		}
	}
}
