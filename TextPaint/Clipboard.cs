/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-12
 * Time: 21:53
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace TextPaint
{
	/// <summary>
	/// Description of Clipboard.
	/// </summary>
	public class Clipboard
	{
		public Core Core_;

		public Clipboard(Core Core__)
		{
			Core_ = Core__;
		}

		public void CharPut(int X, int Y, char C)
		{
			Core_.CharPut(Core_.CursorX + X, Core_.CursorY + Y, C);
		}

		public char CharGet(int X, int Y)
		{
			return Core_.CharGet(Core_.CursorX + X, Core_.CursorY + Y);
		}

		List<string> TextClipboard = new List<string>();
		public int DiamondType = 0;

		public void TextClipboardClear()
		{
			TextClipboard.Clear();
		}

		public void TextClipboardPutChar(int X, int Y, int W, int H, bool Diamond, int XX, int YY, char C)
		{
			if (Diamond)
			{
				// Top left edge
				int D = 0;
				if ((DiamondType == 3) || (DiamondType == 4) || (DiamondType == 5) || (DiamondType == 7))
				{
					D = 1;
				}
				if ((DiamondType == 8))
				{
					D = 2;
				}
				if ((XX + YY + D - X - Y) < 0)
				{
					return;
				}
				
				// Top right edge
				D = 0;
				if ((DiamondType == 1) || (DiamondType == 4) || (DiamondType == 6) || (DiamondType == 8))
				{
					D = 1;
				}
				if ((DiamondType == 5))
				{
					D = 2;
				}
				if ((XX - YY - D - X + Y) > 0)
				{
					return;
				}
				
				// Bottom left edge
				D = 0;
				if ((DiamondType == 2) || (DiamondType == 3) || (DiamondType == 6) || (DiamondType == 8))
				{
					D = 1;
				}
				if ((DiamondType == 7))
				{
					D = 2;
				}
				if ((YY - XX - H - H - D + X - Y) > 0)
				{
					return;
				}
				
				// Bottom right edge
				D = 0;
				if ((DiamondType == 1) || (DiamondType == 2) || (DiamondType == 5) || (DiamondType == 7))
				{
					D = 1;
				}
				if ((DiamondType == 6))
				{
					D = 2;
				}
				if ((YY - W + XX - W - D - X - Y) > 0)
				{
					return;
				}
			}
			CharPut(XX, YY, C);
		}
		
		public void TextClipboardWork(int X, int Y, int W, int H, bool Diamond, bool Paste)
		{
			int X1, X2, Y1, Y2;
			if (Diamond)
			{
				if (W < 0)
				{
					TextClipboardWork(X + W, Y + W, 0 - W, H, Diamond, Paste);
					return;
				}
				if (H < 0)
				{
					TextClipboardWork(X - H, Y + H, W, 0 - H, Diamond, Paste);
					return;
				}

				Y1 = Y;
				Y2 = Y + W + H;
				X1 = X - H;
				X2 = X + W;
				if ((DiamondType == 1) || (DiamondType == 5) || (DiamondType == 6))
				{
					X2++;
				}
				if ((DiamondType == 2) || (DiamondType == 6) || (DiamondType == 7))
				{
					Y2++;
				}
				if ((DiamondType == 3) || (DiamondType == 7) || (DiamondType == 8))
				{
					X1--;
				}
				if ((DiamondType == 4) || (DiamondType == 8) || (DiamondType == 5))
				{
					Y1--;
				}
			}
			else
			{
				if (W < 0)
				{
					TextClipboardWork(X + W, Y, 0 - W, H, Diamond, Paste);
					return;
				}
				if (H < 0)
				{
					TextClipboardWork(X, Y + H, W, 0 - H, Diamond, Paste);
					return;
				}

				Y1 = Y;
				Y2 = Y + H;
				X1 = X;
				X2 = X + W;
			}


			if (Paste)
			{
				if (TextClipboard.Count > 0)
				{
					for (int YY = Y1; YY <= Y2; YY++)
					{
						for (int XX = X1; XX <= X2; XX++)
						{
							TextClipboardPutChar(X, Y, W, H, Diamond, XX, YY, TextClipboard[YY - Y1][XX - X1]);
						}
					}
				}
			}
			else
			{
				TextClipboard.Clear();
				for (int YY = Y1; YY <= Y2; YY++)
				{
					string S = "";
					for (int XX = X1; XX <= X2; XX++)
					{
						S = S + (CharGet(XX, YY)).ToString();
					}
					TextClipboard.Add(S);
				}
			}
		}
	}
}
