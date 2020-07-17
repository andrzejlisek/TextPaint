/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-07
 * Time: 23:03
 * 
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace TextPaint
{
	/// <summary>
	/// Description of ScreenWindow.
	/// </summary>
	public class ScreenWindow : Screen
	{
		Form Form_;
		Bitmap Bitmap_;
		Graphics Bitmap_G;
		int InternalW;
		int InternalH;
		
		public ScreenWindow(Core Core__, ConfigFile CF, int ConsoleW, int ConsoleH)
		{
			if (ConsoleW < 1) { ConsoleW = 80; }
			if (ConsoleH < 1) { ConsoleH = 25; }
			
			CellW = CF.ParamGetI("WinCellW");
			CellH = CF.ParamGetI("WinCellH");
			WinFontName = CF.ParamGetS("WinFontName");
			WinFontSize = CF.ParamGetI("WinFontSize");
			
			WinFont = new Font(WinFontName, WinFontSize, FontStyle.Regular);
			WinStrFormat = new StringFormat();
			WinStrFormat.LineAlignment = StringAlignment.Center;
			WinStrFormat.Alignment = StringAlignment.Center;

			Core_ = Core__;
			Core_.Screen_ = this;
			Form_ = new Form();
			Form_.BackColor = Color.Black;
			Form_.Width = ConsoleW * CellW;
			Form_.Height = ConsoleH * CellH;
			Form_.ClientSize.ToString();
			int DW = Form_.ClientSize.Width - Form_.Width;
			int DH = Form_.ClientSize.Height - Form_.Height;
			Form_.Width -= DW;
			Form_.Height -= DH;
			Form_.KeyDown += WinKey1;
			Form_.KeyPress += WinKey2;
			Core_.WindowResize();
			Core_.ScreenRefresh(true);
		}
		
		public override void StartApp()
		{
			AppWorking = true;
			Application.Run(Form_);
		}
		
		int CellW;
		int CellH;
		string WinFontName;
		int WinFontSize;
		
		Label[,] FormCtrl;
		int[,] FormCtrlB;
		int[,] FormCtrlF;
		char[,] FormCtrlC;
		Rectangle[,] FormCtrlR;
		Font WinFont;
		StringFormat WinStrFormat;
		PictureBox PictureBox_;
		
		public override bool WindowResize()
		{
			if ((InternalW == Form_.Width) && (InternalH == Form_.Height))
			{
				return false;
			}
			
			InternalW = Form_.Width;
			InternalH = Form_.Height;
			
			WinW = Form_.ClientSize.Width / CellW;
			WinH = Form_.ClientSize.Height / CellH;
			
			Bitmap_ = new Bitmap(WinW * CellW, WinH * CellH, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			Bitmap_G = Graphics.FromImage(Bitmap_);
			
			FormCtrl = new Label[WinW, WinH];
			FormCtrlB = new int[WinW, WinH];
			FormCtrlF = new int[WinW, WinH];
			FormCtrlC = new char[WinW, WinH];
			FormCtrlR = new Rectangle[WinW, WinH];
			Form_.Controls.Clear();
			PictureBox_ = new PictureBox();
			PictureBox_.Left = 0;
			PictureBox_.Top = 0;
			PictureBox_.Width = Bitmap_.Width;
			PictureBox_.Height = Bitmap_.Height;
			PictureBox_.Image = Bitmap_;
			Form_.Controls.Add(PictureBox_);
			for (int Y = 0; Y < WinH; Y++)
			{
				for (int X = 0; X < WinW; X++)
				{
					FormCtrlB[X, Y] = -1;
					FormCtrlF[X, Y] = -1;
					FormCtrlC[X, Y] = ' ';
					FormCtrlR[X, Y] = new Rectangle(X * CellW, Y * CellH, CellW, CellH);
					Label Label_ = new Label();
					FormCtrl[X, Y] = Label_;
					//Form_.Controls.Add(Label_);
					Label_.AutoSize = false;
					Label_.Font = new Font(WinFontName, WinFontSize, FontStyle.Regular);
					Label_.Left = X * CellW;
					Label_.Top = Y * CellH;
					Label_.Width = CellW;
					Label_.Height = CellH;
					Label_.TextAlign = ContentAlignment.MiddleCenter;
					Label_.Text = " ";
				}
			}
			
			return true;
		}
		


		public void WinKey1(object sender, KeyEventArgs e)
		{
			Core_.CoreEvent(e.KeyCode.ToString(), (char)(0));
			if (!AppWorking)
			{
				Form_.Close();
			}
		}
		
		public void WinKey2(object sender, KeyPressEventArgs e)
		{
			Core_.CoreEvent("", (char)(e.KeyChar));
			if (!AppWorking)
			{
				Form_.Close();
			}
		}


        public override void PutChar_(int X, int Y, char C, int ColorBack, int ColorFore)
        {
        	Brush ColorBack_ = null;
        	Brush ColorFore_ = null;
        	switch (ColorBack)
        	{
    			case 0: { ColorBack_ = Brushes.Black; } break;
    			case 1: { ColorBack_ = Brushes.Gray; } break;
    			case 2: { ColorBack_ = Brushes.Silver; } break;
    			case 3: { ColorBack_ = Brushes.White; } break;
        	}
        	switch (ColorFore)
        	{
    			case 0: { ColorFore_ = Brushes.Black; } break;
    			case 1: { ColorFore_ = Brushes.Gray; } break;
    			case 2: { ColorFore_ = Brushes.Silver; } break;
    			case 3: { ColorFore_ = Brushes.White; } break;
        	}
        	FormCtrlB[X, Y] = ColorBack;
        	FormCtrlF[X, Y] = ColorFore;
        	FormCtrlC[X, Y] = C;
        	Bitmap_G.FillRectangle(ColorBack_, X * CellW, Y * CellH, CellW, CellH);
        	Bitmap_G.DrawString(C.ToString(), WinFont, ColorFore_, FormCtrlR[X, Y], WinStrFormat);
        }
        
        public override void SetStatusText(string StatusText)
        {
        	StatusText = StatusText.PadRight(WinW, ' ');
        	for (int i = 0; i < StatusText.Length; i++)
        	{
        		PutChar(i, WinH - 1, StatusText[i], 3, 0);
        	}
        }

        public override void Move(int SrcX, int SrcY, int DstX, int DstY, int W, int H)
        {
        	if (SrcY > DstY)
        	{
	        	if (SrcX > DstX)
	    		{
		        	for (int Y = 0; Y < H; Y++)
		        	{
			        	for (int X = 0; X < W; X++)
			        	{
			        		PutChar(DstX + X, DstY + Y, FormCtrlC[SrcX + X, SrcY + Y], FormCtrlB[SrcX + X, SrcY + Y], FormCtrlF[SrcX + X, SrcY + Y]);
			        	}
		        	}
	    		}
	    		else
	    		{
		        	for (int Y = 0; Y < H; Y++)
		        	{
		        		for (int X = (W - 1); X >= 0; X--)
			        	{
			        		PutChar(DstX + X, DstY + Y, FormCtrlC[SrcX + X, SrcY + Y], FormCtrlB[SrcX + X, SrcY + Y], FormCtrlF[SrcX + X, SrcY + Y]);
			        	}
		        	}
	    		}
        	}
        	else
        	{
	        	if (SrcX > DstX)
	    		{
	        		for (int Y = (H - 1); Y >= 0; Y--)
		        	{
			        	for (int X = 0; X < W; X++)
			        	{
			        		PutChar(DstX + X, DstY + Y, FormCtrlC[SrcX + X, SrcY + Y], FormCtrlB[SrcX + X, SrcY + Y], FormCtrlF[SrcX + X, SrcY + Y]);
			        	}
		        	}
	    		}
	    		else
	    		{
	        		for (int Y = (H - 1); Y >= 0; Y--)
		        	{
		        		for (int X = (W - 1); X >= 0; X--)
			        	{
			        		PutChar(DstX + X, DstY + Y, FormCtrlC[SrcX + X, SrcY + Y], FormCtrlB[SrcX + X, SrcY + Y], FormCtrlF[SrcX + X, SrcY + Y]);
			        	}
		        	}
	    		}
        	}
        }

        public override void SetCursorPosition(int X, int Y)
        {
        	PictureBox_.Refresh();
        }
	}
}
