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
        struct DummyCharItemDef
        {
            public int X;
            public int Y;
            public int C;
            public int B;
            public int F;
            public int FontW;
            public int FontH;
        }
        List<DummyCharItemDef> DummyCharList = new List<DummyCharItemDef>();

        protected object GraphMutex = new object();

        public bool MultiThread = false;

        protected Core Core_;
        public bool AppWorking;
        public int WinW;
        public int WinH;
        public bool RawKeyName = false;

        public int UseMemo;
        protected int[,] ScrChrC;
        protected int[,] ScrChrB;
        protected int[,] ScrChrF;
        protected int[,] ScrChrFontW;
        protected int[,] ScrChrFontH;


        public void MemoPrepare()
        {
            if (UseMemo != 0)
            {
                ScrChrB = new int[WinW, WinH];
                ScrChrF = new int[WinW, WinH];
                ScrChrC = new int[WinW, WinH];
                ScrChrFontW = new int[WinW, WinH];
                ScrChrFontH = new int[WinW, WinH];
                for (int Y = 0; Y < WinH; Y++)
                {
                    for (int X = 0; X < WinW; X++)
                    {
                        ScrChrC[X, Y] = 0;
                    }
                }
            }
        }

        protected virtual void PutChar_(int X, int Y, int C, int ColorBack, int ColorFore, int FontW, int FontH)
        {
            
        }

        public void PutChar(int X, int Y, int C, int ColorBack, int ColorFore, int FontW, int FontH)
        {
            if ((X >= 0) && (Y >= 0) && (X < WinW) && (Y < WinH))
            {
                PutChar_(X, Y, C, ColorBack, ColorFore, FontW, FontH);
                if (UseMemo > 0)
                {
                    if (C == 0)
                    {
                        C = ' ';
                    }
                    ScrChrC[X, Y] = C;
                    ScrChrB[X, Y] = ColorBack;
                    ScrChrF[X, Y] = ColorFore;
                    ScrChrFontW[X, Y] = FontW;
                    ScrChrFontH[X, Y] = FontH;
                    return;
                }
            }
        }

        public void CloseApp(int ColorB, int ColorF)
        {
            Clear(ColorB, ColorF);
            SetCursorPosition(0, 0);
            AppWorking = false;
        }

        public void Clear(int ColorB, int ColorF)
        {
            for (int Y = 0; Y < WinH; Y++)
            {
                for (int X = 0; X < WinW; X++)
                {
                    PutChar_(X, Y, 32, ColorB, ColorF, 0, 0);
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

        public void SetStatusText(string StatusText, int ColorBack, int ColorFore)
        {
            SetStatusText(TextWork.StrToInt(StatusText), ColorBack, ColorFore);
        }

        public void SetStatusText(List<int> StatusText, int ColorBack, int ColorFore)
        {
            for (int i = 0; i < WinW; i++)
            {
                if (i < StatusText.Count)
                {
                    PutChar(i, WinH - 1, StatusText[i], ColorBack, ColorFore, 0, 0);
                }
                else
                {
                    PutChar(i, WinH - 1, ' ', ColorBack, ColorFore, 0, 0);
                }
            }
        }

        protected int CursorX = 0;
        protected int CursorY = 0;
        protected int CursorC = -1;
        protected int CursorB = -1;
        protected int CursorF = -1;
        protected int CursorFontW = 0;
        protected int CursorFontH = 0;
        protected bool CursorNeedRepaint = false;

        public virtual void SetCursorPositionNoRefresh(int X, int Y)
        {

        }

        public virtual void SetCursorPosition(int X, int Y)
        {
            
        }

        public void WriteLine()
        {
            CursorX = 0;
            CursorY++;
        }

        public void WriteText(List<int> Text, int ColorB, int ColorF)
        {
            for (int i = 0; i < Text.Count; i++)
            {
                PutChar(CursorX, CursorY, Text[i], ColorB, ColorF, 0, 0);
                CursorX++;
                if (CursorX == WinW)
                {
                    CursorX = 0;
                    CursorY++;
                }
            }
        }

        public void BackText(int ColorB, int ColorF)
        {
            if (CursorX > 0)
            {
                CursorX--;
            }
            else
            {
                CursorY--;
                CursorX = WinW - 1;
            }
            PutChar(CursorX, CursorY, 32, ColorB, ColorF, 0, 0);
        }

        public void WriteText(string Text, int ColorB, int ColorF)
        {
            WriteText(TextWork.StrToInt(Text), ColorB, ColorF);
        }

        public void Refresh()
        {
            SetCursorPosition(CursorX, CursorY);
        }


        /// <summary>
        /// Unify key name due to different names from ScreenConsole and ScreenWindow
        /// </summary>
        /// <returns>The key name.</returns>
        /// <param name="Name">Name.</param>
        protected string UniKeyName(string Name)
        {
            if (RawKeyName)
            {
                switch (Name)
                {
                    case "Return": return Name + "\"->\"Enter";
                    case "Up": return Name + "\"->\"UpArrow";
                    case "Down": return Name + "\"->\"DownArrow";
                    case "Left": return Name + "\"->\"LeftArrow";
                    case "Right": return Name + "\"->\"RightArrow";
                    case "Prior": return Name + "\"->\"PageUp";
                    case "Next": return Name + "\"->\"PageDown";
                    case "Back": return Name + "\"->\"Backspace";
                    case "Spacebar": return Name + "\"->\"Space";
                }
            }
            else
            {
                switch (Name)
                {
                    case "Return": return "Enter";
                    case "Up": return "UpArrow";
                    case "Down": return "DownArrow";
                    case "Left": return "LeftArrow";
                    case "Right": return "RightArrow";
                    case "Prior": return "PageUp";
                    case "Next": return "PageDown";
                    case "Back": return "Backspace";
                    case "Spacebar": return "Space";
                }
            }
            return Name;
        }
    }
}
