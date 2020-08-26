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


        class PictureBoxEx
        {
            public PictureBoxEx(bool Custom)
            {
                if (Custom)
                {
                    Ctrl = new PictureBoxExPanel();
                }
                else
                {
                    Ctrl = new PictureBox();
                }
            }

            public Control Ctrl;
            public Bitmap Image
            {
                set
                {
                    if (Ctrl is PictureBox)
                    {
                        ((PictureBox)Ctrl).Image = value;
                    }
                    if (Ctrl is PictureBoxExPanel)
                    {
                        ((PictureBoxExPanel)Ctrl).Image__ = value;
                    }
                }
            }
        }

        class PictureBoxExPanel : Panel
        {
            Bitmap Image_;
            Graphics ImageG_;

            public Bitmap Image__
            {
                set
                {
                    Image_ = value;
                    Repaint();
                }
            }

            public PictureBoxExPanel()
            {
                Image_ = null;
                ImageG_ = this.CreateGraphics();
            }

            void RepaintS()
            {
                ImageG_ = this.CreateGraphics();
                Repaint();
            }

            void Repaint()
            {
                if (Image_ != null)
                {
                    try
                    {
                        ImageG_.DrawImageUnscaledAndClipped(Image_, new Rectangle(0, 0, this.Width, this.Height));
                    }
                    catch
                    {
                    }
                }
                else
                {
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                //base.OnPaint(e);
                Repaint();
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                //base.OnPaintBackground(e);
                Repaint();
            }

            public override void Refresh()
            {
                //base.Refresh();
                Repaint();
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);
                RepaintS();
            }

            protected override void OnAutoSizeChanged(EventArgs e)
            {
                base.OnAutoSizeChanged(e);
                RepaintS();
            }

            protected override void OnClientSizeChanged(EventArgs e)
            {
                base.OnClientSizeChanged(e);
                RepaintS();
            }
        }

        public ScreenWindow(Core Core__, ConfigFile CF, int ConsoleW, int ConsoleH)
		{
			if (ConsoleW < 1) { ConsoleW = 80; }
			if (ConsoleH < 1) { ConsoleH = 25; }

            CellW = CF.ParamGetI("WinCellW");
			CellH = CF.ParamGetI("WinCellH");
			WinFontName = CF.ParamGetS("WinFontName");
			WinFontSize = CF.ParamGetI("WinFontSize");
            WinPicturePanel = (CF.ParamGetI("WinUse") == 2);

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
		
		int[,] FormCtrlB;
		int[,] FormCtrlF;
		char[,] FormCtrlC;
		Rectangle[,] FormCtrlR;
		Font WinFont;
		StringFormat WinStrFormat;
        PictureBoxEx PictureBox_;
        bool WinPicturePanel = false;


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

            FormCtrlB = new int[WinW, WinH];
			FormCtrlF = new int[WinW, WinH];
			FormCtrlC = new char[WinW, WinH];
			FormCtrlR = new Rectangle[WinW, WinH];
			Form_.Controls.Clear();
			PictureBox_ = new PictureBoxEx(WinPicturePanel);
			PictureBox_.Ctrl.Left = (Form_.ClientSize.Width - Bitmap_.Width) / 2;
			PictureBox_.Ctrl.Top = (Form_.ClientSize.Height - Bitmap_.Height) / 2;
			PictureBox_.Ctrl.Width = Bitmap_.Width;
			PictureBox_.Ctrl.Height = Bitmap_.Height;
			PictureBox_.Image = Bitmap_;

            Form_.Controls.Add(PictureBox_.Ctrl);
            PictureBox_.Ctrl.Refresh();

            for (int Y = 0; Y < WinH; Y++)
			{
				for (int X = 0; X < WinW; X++)
				{
					FormCtrlB[X, Y] = -1;
					FormCtrlF[X, Y] = -1;
					FormCtrlC[X, Y] = ' ';
					FormCtrlR[X, Y] = new Rectangle(X * CellW, Y * CellH, CellW, CellH);
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
            try
            {
                Bitmap_G.FillRectangle(ColorBack_, X * CellW, Y * CellH, CellW, CellH);
                Bitmap_G.DrawString(C.ToString(), WinFont, ColorFore_, FormCtrlR[X, Y], WinStrFormat);
            }
            catch
            {
                Bitmap_G = Graphics.FromImage(Bitmap_);
                PictureBox_.Image = Bitmap_;
            }
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
            Bitmap Bitmap_Temp = Bitmap_.Clone(new Rectangle(SrcX * CellW, SrcY * CellH, W * CellW, H * CellH), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Bitmap_G.DrawImage(Bitmap_Temp, DstX * CellW, DstY * CellH);
        }

        public override void SetCursorPosition(int X, int Y)
        {
        	PictureBox_.Ctrl.Refresh();
        }
	}
}
