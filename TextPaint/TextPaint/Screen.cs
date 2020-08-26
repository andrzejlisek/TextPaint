/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-07
 * Time: 23:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace TextPaint
{
	/// <summary>
	/// Description of Screen.
	/// </summary>
	public class Screen
	{
		public Core Core_;
		public bool AppWorking;
		public int WinW;
		public int WinH;

		public int UseMemo;
		char[,] ScrChrC;
		int[,] ScrChrB;
		int[,] ScrChrF;
		
		public void MemoPrepare()
		{
			if (UseMemo != 0)
			{
				ScrChrB = new int[WinW, WinH];
				ScrChrF = new int[WinW, WinH];
				ScrChrC = new char[WinW, WinH];
				for (int Y = 0; Y < WinH; Y++)
				{
					for (int X = 0; X < WinW; X++)
					{
						ScrChrC[X, Y] = '\0';
					}
				}
			}
		}
		
		public void MemoRepaint(int SrcX, int SrcY, int DstX, int DstY, int W, int H)
		{
			if (UseMemo != 0)
			{
				int X_;
				int Y_;
				if (SrcY > DstY)
				{
					for (int Y = 0; Y < H; Y++)
					{
						if (SrcX > DstX)
						{
							for (int X = 0; X < W; X++)
							{
								X_ = X + DstX;
								Y_ = Y + DstY;
								ScrChrC[X_, Y_] = ScrChrC[X + SrcX, Y + SrcY];
								ScrChrB[X_, Y_] = ScrChrB[X + SrcX, Y + SrcY];
								ScrChrF[X_, Y_] = ScrChrF[X + SrcX, Y + SrcY];

								if (ScrChrC[X_, Y_] != '\0')
								{
									PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_]);
								}
							}
						}
						else
						{
							for (int X = (W - 1); X >= 0; X--)
							{
								X_ = X + DstX;
								Y_ = Y + DstY;
								ScrChrC[X_, Y_] = ScrChrC[X + SrcX, Y + SrcY];
								ScrChrB[X_, Y_] = ScrChrB[X + SrcX, Y + SrcY];
								ScrChrF[X_, Y_] = ScrChrF[X + SrcX, Y + SrcY];

								if (ScrChrC[X_, Y_] != '\0')
								{
									PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_]);
								}
							}
						}
					}
				}
				else
				{
					for (int Y = (H - 1); Y >= 0; Y--)
					{
						if (SrcX > DstX)
						{
							for (int X = 0; X < W; X++)
							{
								X_ = X + DstX;
								Y_ = Y + DstY;
								ScrChrC[X_, Y_] = ScrChrC[X + SrcX, Y + SrcY];
								ScrChrB[X_, Y_] = ScrChrB[X + SrcX, Y + SrcY];
								ScrChrF[X_, Y_] = ScrChrF[X + SrcX, Y + SrcY];

								if (ScrChrC[X_, Y_] != '\0')
								{
									PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_]);
								}
							}
						}
						else
						{
							for (int X = (W - 1); X >= 0; X--)
							{
								X_ = X + DstX;
								Y_ = Y + DstY;
								ScrChrC[X_, Y_] = ScrChrC[X + SrcX, Y + SrcY];
								ScrChrB[X_, Y_] = ScrChrB[X + SrcX, Y + SrcY];
								ScrChrF[X_, Y_] = ScrChrF[X + SrcX, Y + SrcY];

								if (ScrChrC[X_, Y_] != '\0')
								{
									PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_]);
								}
							}
						}
					}
				}
			}
		}
		
        public virtual void PutChar_(int X, int Y, char C, int ColorBack, int ColorFore)
        {
        	
        }
        
        public void PutChar(int X, int Y, char C, int ColorBack, int ColorFore)
        {
        	if ((X >= 0) && (Y >= 0) && (X < WinW) && (Y < WinH))
        	{
        		PutChar_(X, Y, C, ColorBack, ColorFore);
	        	if (UseMemo == 1)
	        	{
	        		if (C == '\0')
	        		{
	        			C = ' ';
	        		}
	        		if ((((int)C) < 32) || (((int)C) > 127))
	        		{
	        			ScrChrC[X, Y] = C;
	        			ScrChrB[X, Y] = ColorBack;
	        			ScrChrF[X, Y] = ColorFore;
	        		}
	        		else
	        		{
	        			ScrChrC[X, Y] = '\0';
	        		}
                    return;
	        	}
                if (UseMemo == 2)
                {
                    if (C == '\0')
                    {
                        C = ' ';
                    }
                    ScrChrC[X, Y] = C;
                    ScrChrB[X, Y] = ColorBack;
                    ScrChrF[X, Y] = ColorFore;
                    return;
                }
            }
        }
        
        public virtual void Move(int SrcX, int SrcY, int DstX, int DstY, int W, int H)
        {
        	
        }
        
        public virtual bool WindowResize()
        {
        	return true;
        }
        
        public virtual void StartApp()
        {
        	
        }
        
        public virtual void SetStatusText(string StatusText)
        {
        	
        }
        
        public virtual void SetCursorPosition(int X, int Y)
        {
        	
        }
	}
}
