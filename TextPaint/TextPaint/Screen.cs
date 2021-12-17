/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-07
 * Time: 23:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

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
        protected int[,] ScrChrC;
        protected int[,] ScrChrB;
        protected int[,] ScrChrF;
        
        public void MemoPrepare()
        {
            if (UseMemo != 0)
            {
                ScrChrB = new int[WinW, WinH];
                ScrChrF = new int[WinW, WinH];
                ScrChrC = new int[WinW, WinH];
                for (int Y = 0; Y < WinH; Y++)
                {
                    for (int X = 0; X < WinW; X++)
                    {
                        ScrChrC[X, Y] = 0;
                    }
                }
            }
        }

        protected virtual void PutChar_(int X, int Y, int C, int ColorBack, int ColorFore)
        {
            
        }
        
        public void PutChar(int X, int Y, int C, int ColorBack, int ColorFore)
        {
            if ((X >= 0) && (Y >= 0) && (X < WinW) && (Y < WinH))
            {
                PutChar_(X, Y, C, ColorBack, ColorFore);
                if (UseMemo > 0)
                {
                    if (C == 0)
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

        public void SetStatusText(string StatusText)
        {
            SetStatusText(TextWork.StrToInt(StatusText));
        }

        public void SetStatusText(List<int> StatusText)
        {
            for (int i = 0; i < (WinW - 1); i++)
            {
                if (i < StatusText.Count)
                {
                    PutChar(i, WinH - 1, StatusText[i], 3, 0);
                }
                else
                {
                    PutChar(i, WinH - 1, ' ', 3, 0);
                }
            }
        }

        public virtual void SetCursorPosition(int X, int Y)
        {
            
        }
    }
}
