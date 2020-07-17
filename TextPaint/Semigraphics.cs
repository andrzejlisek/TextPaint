/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-07
 * Time: 21:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TextPaint
{
	/// <summary>
	/// Description of Semigraphics.
	/// </summary>
	public class Semigraphics
	{
		public Core Core_;
		
		public char[] FrameChar = new char[22];
		public int[] FavChar = new int[256];
		
		public List<int[]> Frame1C = new List<int[]>();
		public List<string> Frame1N = new List<string>();
		public List<int[]> Frame2C = new List<int[]>();
		public List<string> Frame2N = new List<string>();

		public Semigraphics(Core Core__)
		{
			Core_ = Core__;
		}
		
		public void Init(ConfigFile CF)
		{
			/*
				0 -
				1 |
				
				2---3---4
				|   |   |
				5---6---7
				|   |   |
				8---9---10
				
				11 \
				12 /
				
				      13
				     /  \
				   16    14
				  /  \  /  \
				19    17    15
				  \  /  \  /
				   20    18
				     \  /
				      21
			 */
			
			Frame1N.Add("None");
			Frame2N.Add("None");
			Frame1C.Add(new int[11]);
			Frame2C.Add(new int[11]);
			for (int i = 0; i < 11; i++)
			{
				Frame1C[Frame1C.Count - 1][i] = 32;
				Frame2C[Frame2C.Count - 1][i] = 32;
			}
			
			for (int i = 0; i >= 0; i++)
			{
				string[] Buf = CF.ParamGetS("Frame1_" + i).Split(',');
				if (Buf.Length == 12)
				{
					Frame1N.Add(Buf[0]);
					int[] Temp = new int[11];
					for (int ii = 0; ii < 11; ii++)
					{
						Temp[ii] = int.Parse(Buf[ii + 1], NumberStyles.HexNumber);
					}
					Frame1C.Add(Temp);
				}
				else
				{
					i = -2;
				}
			}
			for (int i = 0; i >= 0; i++)
			{
				string[] Buf = CF.ParamGetS("Frame2_" + i).Split(',');
				if (Buf.Length == 12)
				{
					Frame2N.Add(Buf[0]);
					int[] Temp = new int[11];
					for (int ii = 0; ii < 11; ii++)
					{
						Temp[ii] = int.Parse(Buf[ii + 1], NumberStyles.HexNumber);
					}
					Frame2C.Add(Temp);
				}
				else
				{
					i = -2;
				}
			}
			
			string[] FavChar_ = CF.ParamGetS("FavChar").Split(',');
			for (int i = 0; i < FavChar.Length; i++)
			{
				FavChar[i] = ' ';
			}
			for (int i = 0; i < Math.Min(FavChar_.Length, 256); i++)
			{
				FavChar[i] = int.Parse(FavChar_[i], NumberStyles.HexNumber);
			}
			DrawCharI = FavChar[0];
			DrawCharC = (char)DrawCharI;
			
			SetFrame(0, 0);
		}
		
		public string FrameName;
		public string FrameName1;
		public string FrameName2;
		public int Frame1I;
		public int Frame2I;
		
		public void SetFrame(int F1, int F2)
		{
			Frame1I = F1;
			Frame2I = F2;
			for (int i = 0; i < 11; i++)
			{
				FrameChar[i] = (char)Frame1C[F1][i];
				FrameChar[i + 11] = (char)Frame2C[F2][i];
			}
			FrameName = Frame1N[F1] + "/" + Frame2N[F2];
			FrameName1 = Frame1N[F1];
			FrameName2 = Frame2N[F2];
		}
		
		public void SetFrameNext(int N)
		{
			if (N == 1)
			{
				Frame1I++;
				if (Frame1I >= Frame1C.Count)
				{
					Frame1I = 0;
				}
			}
			if (N == 2)
			{
				Frame2I++;
				if (Frame2I >= Frame2C.Count)
				{
					Frame2I = 0;
				}
			}
			SetFrame(Frame1I, Frame2I);
		}
		
		public bool SelectCharState = false;
		public int SelectCharPage = 0;
		public int SelectCharX = 0;
		public int SelectCharY = 0;
		
		public void SelectCharInit()
		{
			SelectCharPage = (DrawCharC / 256);
			SelectCharY = (DrawCharC - (SelectCharPage * 256)) / 16;
			SelectCharX = (DrawCharC - (SelectCharPage * 256)) % 16;
			SelectCharState = true;
			SelectCharRepaintBack();
			SelectCharRepaint();
		}
		
		public void SelectCharCode()
		{
			int C = SelectCharGetCode();
			string C_ = C.ToString("X").PadLeft(4, '0');
			Core_.Screen_.PutChar(1, 1, C_[0], 0, 3);
			Core_.Screen_.PutChar(2, 1, C_[1], 0, 3);
			Core_.Screen_.PutChar(3, 1, C_[2], 0, 3);
			Core_.Screen_.PutChar(4, 1, C_[3], 0, 3);
			Core_.Screen_.PutChar(6, 1, (char)C, 0, 3);
			Core_.Screen_.PutChar(SelectCharX * 2 + 2, SelectCharY + 2, (char)C, 1, 3);
			Core_.Screen_.SetCursorPosition(SelectCharX * 2 + 2, SelectCharY + 2);
		}

		public void SelectCharRepaintBack()
		{
			int CharW = 35;
			int CharH = 19;
			for (int Y = 0; Y < CharH; Y++)
			{
				for (int X = 0; X < CharW; X++)
				{
					Core_.Screen_.PutChar(X, Y, ' ', 0, 3);
				}
			}
			for (int i = 0; i < CharW; i++)
			{
				Core_.Screen_.PutChar(i, 0, ' ', 3, 0);
				Core_.Screen_.PutChar(i, CharH - 1, ' ', 3, 0);
			}
			for (int i = 0; i < CharH; i++)
			{
				Core_.Screen_.PutChar(0, i, ' ', 3, 0);
				Core_.Screen_.PutChar(CharW - 1, i, ' ', 3, 0);
			}
			if (SelectCharPage < 0)
			{
				Core_.Screen_.PutChar(8, 1, 'F', 0, 3);
				Core_.Screen_.PutChar(9, 1, 'A', 0, 3);
				Core_.Screen_.PutChar(10, 1, 'V', 0, 3);
			}
		}
		
		public void SelectCharRepaint()
		{
			SelectCharRepaintBack();
			if (SelectCharPage >= 0)
			{
				int C = SelectCharPage * 256;
				for (int Y = 0; Y < 16; Y++)
				{
					for (int X = 0; X < 16; X++)
					{
						Core_.Screen_.PutChar(X * 2 + 2, Y + 2, (char)C, 0, 3);
						C++;
					}
				}
			}
			else
			{
				int C = 0;
				for (int Y = 0; Y < 16; Y++)
				{
					for (int X = 0; X < 16; X++)
					{
						Core_.Screen_.PutChar(X * 2 + 2, Y + 2, (char)FavChar[C], 0, 3);
						C++;
					}
				}
			}
			SelectCharCode();
		}
		
		int SelectCharGetCode()
		{
			if (SelectCharPage >= 0)
			{
				return (SelectCharPage * 256) + (SelectCharY * 16) + SelectCharX;
			}
			else
			{
				return FavChar[(SelectCharY * 16) + SelectCharX];
			}
		}
		
		void SelectCharChange(int T)
		{
			int C = SelectCharGetCode();
			Core_.Screen_.PutChar(SelectCharX * 2 + 2, SelectCharY + 2, (char)C, 0, 3);
			switch (T)
			{
				case -1:
					if (SelectCharX > 0)
					{
						SelectCharX--;
					}
					else
					{
						SelectCharX = 15;
						SelectCharChange(-2);
					}
					break;
				case 1:
					if (SelectCharX < 15)
					{
						SelectCharX++;
					}
					else
					{
						SelectCharX = 0;
						SelectCharChange(2);
					}
					break;
				case -2:
					if (SelectCharY > 0)
					{
						SelectCharY--;
					}
					else
					{
						if (SelectCharPage >= 0)
						{
							SelectCharPage--;
							SelectCharY = 15;
							SelectCharRepaint();
						}
						else
						{
							SelectCharPage = 255;
							SelectCharY = 15;
						}
						SelectCharRepaint();
					}
					break;
				case 2:
					if (SelectCharY < 15)
					{
						SelectCharY++;
					}
					else
					{
						if (SelectCharPage < 255)
						{
							SelectCharPage++;
							SelectCharY = 0;
						}
						else
						{
							SelectCharPage = -1;
							SelectCharY = 0;
						}
						SelectCharRepaint();
					}
					break;
				case -3:
					if (SelectCharPage >= 0)
					{
						SelectCharPage--;
					}
					else
					{
						SelectCharPage = 255;
					}
					SelectCharRepaint();
					break;
				case 3:
					if (SelectCharPage < 255)
					{
						SelectCharPage++;
					}
					else
					{
						SelectCharPage = -1;
					}
					SelectCharRepaint();
					break;
				case -4:
					if (SelectCharPage >= 0)
					{
						if (SelectCharPage < 16)
						{
							SelectCharPage += 256;
						}
						SelectCharPage -= 16;
					}
					else
					{
						SelectCharPage = 240;
					}
					SelectCharRepaint();
					break;
				case 4:
					if (SelectCharPage >= 0)
					{
						if (SelectCharPage > 239)
						{
							SelectCharPage -= 256;
						}
						SelectCharPage += 16;
					}
					else
					{
						SelectCharPage = 16;
					}
					SelectCharRepaint();
					break;
			}
		}
		
		public void SelectCharEvent(string KeyName, char KeyChar)
		{
			switch (KeyName)
			{
				case "UpArrow":
				case "Up":
					SelectCharChange(-2);
					SelectCharCode();
					break;
				case "DownArrow":
				case "Down":
					SelectCharChange(2);
					SelectCharCode();
					break;
				case "LeftArrow":
				case "Left":
					SelectCharChange(-1);
					SelectCharCode();
					break;
				case "RightArrow":
				case "Right":
					SelectCharChange(1);
					SelectCharCode();
					break;
				case "PageUp":
					SelectCharChange(-3);
					SelectCharCode();
					break;
				case "PageDown":
				case "Next":
					SelectCharChange(3);
					SelectCharCode();
					break;
				case "Home":
					SelectCharChange(-4);
					SelectCharCode();
					break;
				case "End":
					SelectCharChange(4);
					SelectCharCode();
					break;
				case "Escape":
					SelectCharClose(false);
					Core_.ScreenRefresh(true);
					return;
				case "Enter":
				case "Return":
					SelectCharClose(true);
					Core_.ScreenRefresh(true);
					return;
				case "Insert":
				case "Delete":
					{
						int C = SelectCharGetCode();
						if (SelectCharPage >= 0)
						{
							for (int i = 0; i < FavChar.Length; i++)
							{
								if (FavChar[i] == C)
								{
									SelectCharPage = -1;
									SelectCharY = i / 16;
									SelectCharX = i % 16;
									i = FavChar.Length;
								}
							}
						}
						else
						{
							SelectCharPage = C / 256;
							C = C % 256;
							SelectCharY = C / 16;
							SelectCharX = C % 16;
						}
						SelectCharRepaint();
						SelectCharCode();
					}
					return;
			}
		}
		
		public void SelectCharClose(bool Set)
		{
			if (Set)
			{
				DrawCharI = SelectCharGetCode();
				DrawCharC = (char)DrawCharI;
			}
			SelectCharState = false;
			Core_.CoreEvent("", '\0');
		}
		
		public int DrawCharI;
		public char DrawCharC;



		
		
		public void FrameCharPut1(int X, int Y, bool ForceT, bool ForceB, bool ForceL, bool ForceR)
		{
			int CharSide = 0;
			List<char> CharT  = new List<char>(new char[] { FrameChar[1],  FrameChar[2],  FrameChar[3],  FrameChar[4],  FrameChar[5],  FrameChar[6],  FrameChar[7]  });
			List<char> CharB  = new List<char>(new char[] { FrameChar[1],  FrameChar[5],  FrameChar[6],  FrameChar[7],  FrameChar[8],  FrameChar[9],  FrameChar[10] });
			List<char> CharL  = new List<char>(new char[] { FrameChar[0],  FrameChar[2],  FrameChar[3],  FrameChar[5],  FrameChar[6],  FrameChar[8],  FrameChar[9]  });
			List<char> CharR  = new List<char>(new char[] { FrameChar[0],  FrameChar[3],  FrameChar[4],  FrameChar[6],  FrameChar[7],  FrameChar[9],  FrameChar[10] });
			CharSide += ((ForceT || CharT.Contains(CharGet(X + 0, Y - 1))) ? 1 : 0);
			CharSide += ((ForceB || CharB.Contains(CharGet(X + 0, Y + 1))) ? 2 : 0);
			CharSide += ((ForceL || CharL.Contains(CharGet(X - 1, Y + 0))) ? 4 : 0);
			CharSide += ((ForceR || CharR.Contains(CharGet(X + 1, Y + 0))) ? 8 : 0);
			switch (CharSide)
			{
				case 1:  CharPut(X, Y, FrameChar[1]);  break;
				case 2:  CharPut(X, Y, FrameChar[1]);  break;
				case 3:  CharPut(X, Y, FrameChar[1]);  break;
				case 4:  CharPut(X, Y, FrameChar[0]);  break;
				case 5:  CharPut(X, Y, FrameChar[10]); break;
				case 6:  CharPut(X, Y, FrameChar[4]);  break;
				case 7:  CharPut(X, Y, FrameChar[7]);  break;
				case 8:  CharPut(X, Y, FrameChar[0]);  break;
				case 9:  CharPut(X, Y, FrameChar[8]);  break;
				case 10: CharPut(X, Y, FrameChar[2]);  break;
				case 11: CharPut(X, Y, FrameChar[5]);  break;
				case 12: CharPut(X, Y, FrameChar[0]);  break;
				case 13: CharPut(X, Y, FrameChar[9]);  break;
				case 14: CharPut(X, Y, FrameChar[3]);  break;
				case 15: CharPut(X, Y, FrameChar[6]);  break;
			}
		}

		public void FrameCharPut2(int X, int Y, bool ForceTR, bool ForceBL, bool ForceTL, bool ForceBR)
		{
			int CharSide = 0;
			List<char> CharTR = new List<char>(new char[] { FrameChar[12], FrameChar[13], FrameChar[14], FrameChar[15], FrameChar[16], FrameChar[17], FrameChar[18] });
			List<char> CharBL = new List<char>(new char[] { FrameChar[12], FrameChar[16], FrameChar[17], FrameChar[18], FrameChar[19], FrameChar[20], FrameChar[21] });
			List<char> CharTL = new List<char>(new char[] { FrameChar[11], FrameChar[13], FrameChar[14], FrameChar[16], FrameChar[17], FrameChar[19], FrameChar[20] });
			List<char> CharBR = new List<char>(new char[] { FrameChar[11], FrameChar[14], FrameChar[15], FrameChar[17], FrameChar[18], FrameChar[20], FrameChar[21] });
			CharSide += ((ForceTR || CharTR.Contains(CharGet(X + 1, Y - 1))) ? 1 : 0);
			CharSide += ((ForceBL || CharBL.Contains(CharGet(X - 1, Y + 1))) ? 2 : 0);
			CharSide += ((ForceTL || CharTL.Contains(CharGet(X - 1, Y - 1))) ? 4 : 0);
			CharSide += ((ForceBR || CharBR.Contains(CharGet(X + 1, Y + 1))) ? 8 : 0);
			switch (CharSide)
			{
				case 1:  CharPut(X, Y, FrameChar[12]);  break;
				case 2:  CharPut(X, Y, FrameChar[12]);  break;
				case 3:  CharPut(X, Y, FrameChar[12]);  break;
				case 4:  CharPut(X, Y, FrameChar[11]);  break;
				case 5:  CharPut(X, Y, FrameChar[21]); break;
				case 6:  CharPut(X, Y, FrameChar[15]);  break;
				case 7:  CharPut(X, Y, FrameChar[18]);  break;
				case 8:  CharPut(X, Y, FrameChar[11]);  break;
				case 9:  CharPut(X, Y, FrameChar[19]);  break;
				case 10: CharPut(X, Y, FrameChar[13]);  break;
				case 11: CharPut(X, Y, FrameChar[16]);  break;
				case 12: CharPut(X, Y, FrameChar[11]);  break;
				case 13: CharPut(X, Y, FrameChar[20]);  break;
				case 14: CharPut(X, Y, FrameChar[14]);  break;
				case 15: CharPut(X, Y, FrameChar[17]);  break;
			}
		}


		public void FrameCharPut(int Dir)
		{
			int CharSide = 0;
			
			List<char> CharT  = new List<char>(new char[] { FrameChar[1],  FrameChar[2],  FrameChar[3],  FrameChar[4],  FrameChar[5],  FrameChar[6],  FrameChar[7]  });
			List<char> CharB  = new List<char>(new char[] { FrameChar[1],  FrameChar[5],  FrameChar[6],  FrameChar[7],  FrameChar[8],  FrameChar[9],  FrameChar[10] });
			List<char> CharL  = new List<char>(new char[] { FrameChar[0],  FrameChar[2],  FrameChar[3],  FrameChar[5],  FrameChar[6],  FrameChar[8],  FrameChar[9]  });
			List<char> CharR  = new List<char>(new char[] { FrameChar[0],  FrameChar[3],  FrameChar[4],  FrameChar[6],  FrameChar[7],  FrameChar[9],  FrameChar[10] });

			List<char> CharTR = new List<char>(new char[] { FrameChar[12], FrameChar[13], FrameChar[14], FrameChar[15], FrameChar[16], FrameChar[17], FrameChar[18] });
			List<char> CharBL = new List<char>(new char[] { FrameChar[12], FrameChar[16], FrameChar[17], FrameChar[18], FrameChar[19], FrameChar[20], FrameChar[21] });
			List<char> CharTL = new List<char>(new char[] { FrameChar[11], FrameChar[13], FrameChar[14], FrameChar[16], FrameChar[17], FrameChar[19], FrameChar[20] });
			List<char> CharBR = new List<char>(new char[] { FrameChar[11], FrameChar[14], FrameChar[15], FrameChar[17], FrameChar[18], FrameChar[20], FrameChar[21] });
			
			switch (Dir)
			{
				case 0:
					{
						CharSide += (CharB.Contains(CharGet( 0,  1)) ? 1 : 0);
						CharSide += (CharL.Contains(CharGet(-1,  0)) ? 2 : 0);
						CharSide += (CharR.Contains(CharGet( 1,  0)) ? 4 : 0);
						switch (CharSide)
						{
							case 0: CharPut(0, 0, FrameChar[1]);  break;
							case 1: CharPut(0, 0, FrameChar[1]);  break;
							case 2: CharPut(0, 0, FrameChar[10]); break;
							case 3: CharPut(0, 0, FrameChar[7]);  break;
							case 4: CharPut(0, 0, FrameChar[8]);  break;
							case 5: CharPut(0, 0, FrameChar[5]);  break;
							case 6: CharPut(0, 0, FrameChar[9]);  break;
							case 7: CharPut(0, 0, FrameChar[6]);  break;
						}
					}
					break;
				case 1:
					{
						CharSide += (CharT.Contains(CharGet( 0, -1)) ? 1 : 0);
						CharSide += (CharR.Contains(CharGet( 1,  0)) ? 2 : 0);
						CharSide += (CharL.Contains(CharGet(-1,  0)) ? 4 : 0);
						switch (CharSide)
						{
							case 0: CharPut(0, 0, FrameChar[1]);  break;
							case 1: CharPut(0, 0, FrameChar[1]);  break;
							case 2: CharPut(0, 0, FrameChar[2]);  break;
							case 3: CharPut(0, 0, FrameChar[5]);  break;
							case 4: CharPut(0, 0, FrameChar[4]);  break;
							case 5: CharPut(0, 0, FrameChar[7]);  break;
							case 6: CharPut(0, 0, FrameChar[3]);  break;
							case 7: CharPut(0, 0, FrameChar[6]);  break;
						}
					}
					break;
				case 2:
					{
						CharSide += (CharR.Contains(CharGet( 1,  0)) ? 1 : 0);
						CharSide += (CharB.Contains(CharGet( 0,  1)) ? 2 : 0);
						CharSide += (CharT.Contains(CharGet( 0, -1)) ? 4 : 0);
						switch (CharSide)
						{
							case 0: CharPut(0, 0, FrameChar[0]);  break;
							case 1: CharPut(0, 0, FrameChar[0]);  break;
							case 2: CharPut(0, 0, FrameChar[4]);  break;
							case 3: CharPut(0, 0, FrameChar[3]);  break;
							case 4: CharPut(0, 0, FrameChar[10]); break;
							case 5: CharPut(0, 0, FrameChar[9]);  break;
							case 6: CharPut(0, 0, FrameChar[7]);  break;
							case 7: CharPut(0, 0, FrameChar[6]);  break;
						}
					}
					break;
				case 3:
					{
						CharSide += (CharL.Contains(CharGet(-1,  0)) ? 1 : 0);
						CharSide += (CharT.Contains(CharGet( 0, -1)) ? 2 : 0);
						CharSide += (CharB.Contains(CharGet( 0,  1)) ? 4 : 0);
						switch (CharSide)
						{
							case 0: CharPut(0, 0, FrameChar[0]);  break;
							case 1: CharPut(0, 0, FrameChar[0]);  break;
							case 2: CharPut(0, 0, FrameChar[8]);  break;
							case 3: CharPut(0, 0, FrameChar[9]);  break;
							case 4: CharPut(0, 0, FrameChar[2]);  break;
							case 5: CharPut(0, 0, FrameChar[3]);  break;
							case 6: CharPut(0, 0, FrameChar[5]);  break;
							case 7: CharPut(0, 0, FrameChar[6]);  break;
						}
					}
					break;
				case 4:
					{
						CharSide += (CharBL.Contains(CharGet(-1,  1)) ? 1 : 0);
						CharSide += (CharTL.Contains(CharGet(-1, -1)) ? 2 : 0);
						CharSide += (CharBR.Contains(CharGet( 1,  1)) ? 4 : 0);
						switch (CharSide)
						{
							case 0: CharPut(0, 0, FrameChar[12]); break;
							case 1: CharPut(0, 0, FrameChar[12]); break;
							case 2: CharPut(0, 0, FrameChar[21]); break;
							case 3: CharPut(0, 0, FrameChar[18]); break;
							case 4: CharPut(0, 0, FrameChar[19]); break;
							case 5: CharPut(0, 0, FrameChar[16]); break;
							case 6: CharPut(0, 0, FrameChar[20]); break;
							case 7: CharPut(0, 0, FrameChar[17]); break;
						}
					}
					break;
				case 5:
					{
						CharSide += (CharTR.Contains(CharGet( 1, -1)) ? 1 : 0);
						CharSide += (CharBR.Contains(CharGet( 1,  1)) ? 2 : 0);
						CharSide += (CharTL.Contains(CharGet(-1, -1)) ? 4 : 0);
						switch (CharSide)
						{
							case 0: CharPut(0, 0, FrameChar[12]); break;
							case 1: CharPut(0, 0, FrameChar[12]); break;
							case 2: CharPut(0, 0, FrameChar[13]); break;
							case 3: CharPut(0, 0, FrameChar[16]); break;
							case 4: CharPut(0, 0, FrameChar[15]); break;
							case 5: CharPut(0, 0, FrameChar[18]); break;
							case 6: CharPut(0, 0, FrameChar[14]); break;
							case 7: CharPut(0, 0, FrameChar[17]); break;
						}
					}
					break;
				case 6:
					{
						CharSide += (CharTL.Contains(CharGet( 1,  1)) ? 1 : 0);
						CharSide += (CharTR.Contains(CharGet(-1,  1)) ? 2 : 0);
						CharSide += (CharBL.Contains(CharGet( 1, -1)) ? 4 : 0);
						switch (CharSide)
						{
							case 0: CharPut(0, 0, FrameChar[11]); break;
							case 1: CharPut(0, 0, FrameChar[11]); break;
							case 2: CharPut(0, 0, FrameChar[15]); break;
							case 3: CharPut(0, 0, FrameChar[14]); break;
							case 4: CharPut(0, 0, FrameChar[21]); break;
							case 5: CharPut(0, 0, FrameChar[20]); break;
							case 6: CharPut(0, 0, FrameChar[18]); break;
							case 7: CharPut(0, 0, FrameChar[17]); break;
						}
					}
					break;
				case 7:
					{
						CharSide += (CharTL.Contains(CharGet(-1, -1)) ? 1 : 0);
						CharSide += (CharTR.Contains(CharGet( 1, -1)) ? 2 : 0);
						CharSide += (CharBL.Contains(CharGet(-1,  1)) ? 4 : 0);
						switch (CharSide)
						{
							case 0: CharPut(0, 0, FrameChar[11]); break;
							case 1: CharPut(0, 0, FrameChar[11]); break;
							case 2: CharPut(0, 0, FrameChar[19]); break;
							case 3: CharPut(0, 0, FrameChar[20]); break;
							case 4: CharPut(0, 0, FrameChar[13]); break;
							case 5: CharPut(0, 0, FrameChar[14]); break;
							case 6: CharPut(0, 0, FrameChar[16]); break;
							case 7: CharPut(0, 0, FrameChar[17]); break;
						}
					}
					break;
			}
		}
		
		public bool DrawCharEraseFrame = true;
		
		public void FrameCharErase()
		{
			CharPut(0, 0, DrawCharC);
			if (DrawCharEraseFrame)
			{
				int FrameCharN_T = -1;
				int FrameCharN_B = -1;
				int FrameCharN_L = -1;
				int FrameCharN_R = -1;
				int FrameCharN_TR = -1;
				int FrameCharN_BL = -1;
				int FrameCharN_TL = -1;
				int FrameCharN_BR = -1;
				for (int i = 0; i < FrameChar.Length; i++)
				{
					if (CharGet( 0, -1) == FrameChar[i]) { FrameCharN_T  = i; }
					if (CharGet( 0,  1) == FrameChar[i]) { FrameCharN_B  = i; }
					if (CharGet(-1,  0) == FrameChar[i]) { FrameCharN_L  = i; }
					if (CharGet( 1,  0) == FrameChar[i]) { FrameCharN_R  = i; }
	
					if (CharGet( 1, -1) == FrameChar[i]) { FrameCharN_TR = i; }
					if (CharGet(-1,  1) == FrameChar[i]) { FrameCharN_BL = i; }
					if (CharGet(-1, -1) == FrameChar[i]) { FrameCharN_TL = i; }
					if (CharGet( 1,  1) == FrameChar[i]) { FrameCharN_BR = i; }
				}
				switch (FrameCharN_T)
				{
					case 2:  CharPut( 0, -1, FrameChar[0]);  break;
					case 5:  CharPut( 0, -1, FrameChar[8]);  break;
					case 3:  CharPut( 0, -1, FrameChar[0]);  break;
					case 6:  CharPut( 0, -1, FrameChar[9]);  break;
					case 4:  CharPut( 0, -1, FrameChar[0]);  break;
					case 7:  CharPut( 0, -1, FrameChar[10]); break;
				}
				switch (FrameCharN_B)
				{
					case 8:  CharPut( 0,  1, FrameChar[0]);  break;
					case 5:  CharPut( 0,  1, FrameChar[2]);  break;
					case 9:  CharPut( 0,  1, FrameChar[0]);  break;
					case 6:  CharPut( 0,  1, FrameChar[3]);  break;
					case 10: CharPut( 0,  1, FrameChar[0]);  break;
					case 7:  CharPut( 0,  1, FrameChar[4]);  break;
				}
				switch (FrameCharN_L)
				{
					case 2:  CharPut(-1,  0, FrameChar[1]);  break;
					case 3:  CharPut(-1,  0, FrameChar[4]);  break;
					case 5:  CharPut(-1,  0, FrameChar[1]);  break;
					case 6:  CharPut(-1,  0, FrameChar[7]);  break;
					case 8:  CharPut(-1,  0, FrameChar[1]);  break;
					case 9:  CharPut(-1,  0, FrameChar[10]); break;
				}
				switch (FrameCharN_R)
				{
					case 4:  CharPut( 1,  0, FrameChar[1]);  break;
					case 3:  CharPut( 1,  0, FrameChar[2]);  break;
					case 7:  CharPut( 1,  0, FrameChar[1]);  break;
					case 6:  CharPut( 1,  0, FrameChar[5]);  break;
					case 10: CharPut( 1,  0, FrameChar[1]);  break;
					case 9:  CharPut( 1,  0, FrameChar[8]);  break;
				}
				switch (FrameCharN_TR)
				{
					case 13: CharPut( 1, -1, FrameChar[11]); break;
					case 16: CharPut( 1, -1, FrameChar[19]); break;
					case 14: CharPut( 1, -1, FrameChar[11]); break;
					case 17: CharPut( 1, -1, FrameChar[20]); break;
					case 15: CharPut( 1, -1, FrameChar[11]); break;
					case 18: CharPut( 1, -1, FrameChar[21]); break;
				}
				switch (FrameCharN_BL)
				{
					case 19: CharPut(-1,  1, FrameChar[11]); break;
					case 16: CharPut(-1,  1, FrameChar[13]); break;
					case 20: CharPut(-1,  1, FrameChar[11]); break;
					case 17: CharPut(-1,  1, FrameChar[14]); break;
					case 21: CharPut(-1,  1, FrameChar[11]); break;
					case 18: CharPut(-1,  1, FrameChar[15]); break;
				}
				switch (FrameCharN_TL)
				{
					case 13: CharPut(-1, -1, FrameChar[12]); break;
					case 14: CharPut(-1, -1, FrameChar[15]); break;
					case 16: CharPut(-1, -1, FrameChar[12]); break;
					case 17: CharPut(-1, -1, FrameChar[18]); break;
					case 19: CharPut(-1, -1, FrameChar[12]); break;
					case 20: CharPut(-1, -1, FrameChar[21]); break;
				}
				switch (FrameCharN_BR)
				{
					case 15: CharPut( 1,  1, FrameChar[12]); break;
					case 14: CharPut( 1,  1, FrameChar[13]); break;
					case 18: CharPut( 1,  1, FrameChar[12]); break;
					case 17: CharPut( 1,  1, FrameChar[16]); break;
					case 21: CharPut( 1,  1, FrameChar[12]); break;
					case 20: CharPut( 1,  1, FrameChar[19]); break;
				}
			}
		}
		
		public void CharPut(int X, int Y, char C)
		{
			Core_.CharPut(Core_.CursorX + X, Core_.CursorY + Y, C);
		}

		public char CharGet(int X, int Y)
		{
			return Core_.CharGet(Core_.CursorX + X, Core_.CursorY + Y);
		}
		
		public void RectangleDraw(int X, int Y, int W, int H, int T)
		{
			if (W < 0)
			{
				RectangleDraw(X + W, Y, 0 - W, H, T);
				return;
			}
			if (H < 0)
			{
				RectangleDraw(X, Y + H, W, 0 - H, T);
				return;
			}
			
			if (T == 0)
			{
				for (int i = 1; i < W; i++)
				{
					FrameCharPut1(X + i, Y + 0, false, false, true,  true );
					FrameCharPut1(X + i, Y + H, false, false, true,  true );
				}
				for (int i = 1; i < H; i++)
				{
					FrameCharPut1(X + 0, Y + i, true,  true,  false, false);
					FrameCharPut1(X + W, Y + i, true,  true,  false, false);
				}
				if ((W != 0) && (H != 0))
				{
					FrameCharPut1(X + 0, Y + 0, false, true,  false, true );
					FrameCharPut1(X + W, Y + 0, false, true,  true,  false);
					FrameCharPut1(X + 0, Y + H, true,  false, false, true );
					FrameCharPut1(X + W, Y + H, true,  false, true,  false);
				}
				if ((W == 0) && (H != 0))
				{
					FrameCharPut1(X + 0, Y + 0, false, true,  false, false);
					FrameCharPut1(X + 0, Y + H, true,  false, false, false);
				}
				if ((W != 0) && (H == 0))
				{
					FrameCharPut1(X + 0, Y + 0, false, false, false, true );
					FrameCharPut1(X + W, Y + 0, false, false, true,  false);
				}
				if ((W == 0) && (H == 0))
				{
					FrameCharPut1(X + 0, Y + 0, false, false, false, false);
				}
			}
			if (T == 1)
			{
				for (int i = 0; i <= W; i++)
				{
					CharPut(X + i, Y + 0, DrawCharC);
					CharPut(X + i, Y + H, DrawCharC);
				}
				for (int i = 0; i <= H; i++)
				{
					CharPut(X + 0, Y + i, DrawCharC);
					CharPut(X + W, Y + i, DrawCharC);
				}
			}
			if (T == 2)
			{
				for (int i = 0; i <= H; i++)
				{
					for (int ii = 0; ii <= W; ii++)
					{
						CharPut(X + ii, Y + i, DrawCharC);
					}
				}
			}
		}

		public int DiamondType = 0;

		public void DiamondDraw(int X, int Y, int W, int H, int T, int TT)
		{
			if (W < 0)
			{
				DiamondDraw(X + W, Y + W, 0 - W, H, T, TT);
				return;
			}
			if (H < 0)
			{
				DiamondDraw(X - H, Y + H, W, 0 - H, T, TT);
				return;
			}
			
			if (TT < 0)
			{
				switch (DiamondType)
				{
					case 0: DiamondDraw(X + 0, Y + 0, W, H, T, 0); return;
					case 1: DiamondDraw(X + 0, Y + 0, W, H, T, 1); return;
					case 2: DiamondDraw(X + 0, Y + 0, W, H, T, 2); return;
					case 3: DiamondDraw(X - 1, Y + 0, W, H, T, 1); return;
					case 4: DiamondDraw(X + 0, Y - 1, W, H, T, 2); return;
					case 5: DiamondDraw(X + 0, Y + 0, W, H, T, 3); return;
					case 6: DiamondDraw(X + 0, Y + 1, W, H, T, 3); return;
					case 7: DiamondDraw(X - 1, Y + 1, W, H, T, 3); return;
					case 8: DiamondDraw(X - 1, Y + 0, W, H, T, 3); return;
				}
			}

			if (T == 0)
			{
				if (TT == 3)
				{
					for (int i = 0; i <= W; i++)
					{
						FrameCharPut2(X + i + 1, Y + i - 1, false, false, true, true);
						FrameCharPut2(X + i - H, Y + i + H, false, false, true, true);
					}

					for (int i = 0; i <= H; i++)
					{
						FrameCharPut2(X + 0 - i + 0, Y + i - 1, true, true, false, false);
						FrameCharPut2(X + W - i + 1, Y + i + W, true, true, false, false);
					}
				}
				else
				{
					int TX = 0;
					int TY = 0;
					if (TT == 1)
					{
						TX = 1;
					}
					if (TT == 2)
					{
						TY = 1;
					}

					for (int i = 1; i < W; i++)
					{
						FrameCharPut2(X + i + TX, Y + i + 0 + 0,  false, false, true, true);
						FrameCharPut2(X + i - H,  Y + i + H + TY, false, false, true, true);
					}
					if (TT == 1)
					{
						FrameCharPut2(X + 0 + TX, Y + 0 + 0 + 0,  false, false, true, true);
						FrameCharPut2(X + W - H,  Y + W + H + TY, false, false, true, true);
					}
					if (TT == 2)
					{
						FrameCharPut2(X + 0 - H,  Y + 0 + H + TY, false, false, true, true);
						FrameCharPut2(X + W + TX, Y + W + 0 + 0,  false, false, true, true);
					}

					for (int i = 1; i < H; i++)
					{
						FrameCharPut2(X + 0 - i + 0,  Y + i + 0 + 0,  true, true, false, false);
						FrameCharPut2(X + W - i + TX, Y + i + W + TY, true, true, false, false);
					}
					if (TT == 1)
					{
						FrameCharPut2(X + 0 - 0 + 0,  Y + 0 + 0 + 0,  true, true, false, false);
						FrameCharPut2(X + W - H + TX, Y + H + W + TY, true, true, false, false);
					}
					if (TT == 2)
					{
						FrameCharPut2(X + W - 0 + TX, Y + 0 + W + TY, true, true, false, false);
						FrameCharPut2(X + 0 - H + 0,  Y + H + 0 + 0,  true, true, false, false);
					}

					if (TT != 1)
					{
						FrameCharPut2(X, Y, false, true, false, true);
						FrameCharPut2(X + W - H, Y + W + H + TY, true, false, true, false);
					}
					if (TT != 2)
					{
						FrameCharPut2(X + W + TX, Y + W, false, true, true, false);
						FrameCharPut2(X - H, Y + H, true, false, false, true);
					}
				}
			}
			if (T == 1)
			{
				if (TT == 3)
				{
					for (int i = 0; i <= W; i++)
					{
						CharPut(X + i + 1, Y + i - 1, DrawCharC);
						CharPut(X + i - H, Y + i + H, DrawCharC);
					}
					for (int i = 0; i <= H; i++)
					{
						CharPut(X - i, Y + i - 1, DrawCharC);
						CharPut(X + W - i + 1, Y + i + W + 0, DrawCharC);
					}
				}
				else
				{
					int TX = 0;
					int TY = 0;
					if (TT == 1)
					{
						TX = 1;
					}
					if (TT == 2)
					{
						TY = 1;
					}
					for (int i = 0; i <= W; i++)
					{
						CharPut(X + i + TX, Y + i, DrawCharC);
						CharPut(X + i - H, Y + i + H + TY, DrawCharC);
					}
					for (int i = 0; i <= H; i++)
					{
						CharPut(X - i, Y + i, DrawCharC);
						CharPut(X + W - i + TX, Y + i + W + TY, DrawCharC);
					}
				}
			}
			if (T == 2)
			{
				if (TT == 3)
				{
					for (int ii = -1; ii <= H; ii++)
					{
						for (int i = 0; i <= W; i++)
						{
							CharPut(X + i - ii, Y + i + ii, DrawCharC);
						}
					}
					for (int ii = -1; ii < H; ii++)
					{
						for (int i = -1; i <= W; i++)
						{
							CharPut(X + i - ii, Y + i + ii + 1, DrawCharC);
						}
					}
				}
				else
				{
					for (int ii = 0; ii <= H; ii++)
					{
						for (int i = 0; i <= W; i++)
						{
							CharPut(X + i - ii, Y + i + ii, DrawCharC);
						}
					}
					if (TT == 0)
					{
						for (int ii = 1; ii <= H; ii++)
						{
							for (int i = 0; i < W; i++)
							{
								CharPut(X + i - ii + 1, Y + i + ii, DrawCharC);
							}
						}
					}
					if (TT == 1)
					{
						for (int ii = 0; ii <= H; ii++)
						{
							for (int i = 0; i <= W; i++)
							{
								CharPut(X + i - ii + 1, Y + i + ii, DrawCharC);
							}
						}
					}
					if (TT == 2)
					{
						for (int ii = 0; ii <= H; ii++)
						{
							for (int i = 0; i <= W; i++)
							{
								CharPut(X + i - ii, Y + i + ii + 1, DrawCharC);
							}
						}
					}
				}
			}
		}
	}
}
