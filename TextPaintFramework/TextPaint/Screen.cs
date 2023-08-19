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
using System.Threading.Tasks;

namespace TextPaint
{
    /// <summary>
    /// Description of Screen.
    /// </summary>
    public class Screen
    {
        protected object GraphMutex = new object();

        public bool MultiThread = false;

        public const int TerminalCellW = 8;
        public const int TerminalCellH = 16;

        protected Core Core_;
        public bool AppWorking;
        public int WinW;
        public int WinH;
        public bool WinAuto = false;
        public bool WinAutoAllowed = false;
        public bool RawKeyName = false;
        public int WinFixed = 0;

        public int UseMemo;
        protected int[,] ScrChrC;
        protected int[,] ScrChrB;
        protected int[,] ScrChrF;
        protected int[,] ScrChrA;
        protected int[,] ScrChrFontW;
        protected int[,] ScrChrFontH;

        public void LoadConfig(ConfigFile CF)
        {
            ANSIColors = CF.ParamGetB("ANSIColors", true);
            ANSIColorBold = CF.ParamGetB("ANSIColorBold", false);
            ANSIColorBlink = CF.ParamGetB("ANSIColorBlink", false);
            ANSIIgnoreConcealed = CF.ParamGetB("ANSIIgnoreConcealed", false);
            ANSIReverseMode = CF.ParamGetI("ANSIReverseMode", 2);

            FontModeBold = ((CF.ParamGetI("DisplayAttrib", 15) & 1) > 0) ? 1 : 0;
            FontModeItalic = ((CF.ParamGetI("DisplayAttrib", 15) & 1) > 0) ? 1 : 0;
            FontModeUnderline = ((CF.ParamGetI("DisplayAttrib", 15) & 1) > 0) ? 1 : 0;
            FontModeStrike = ((CF.ParamGetI("DisplayAttrib", 15) & 1) > 0) ? 1 : 0;

            FontModeBlink = CF.ParamGetI("DisplayBlink", 1);
        }

        public virtual void SetLineOffset(int Y, int Offset, bool Blank, int ColorBack, int ColorFore, int FontAttr)
        {

        }

        public virtual void RepaintOffset(int Y)
        {

        }

        public virtual void AppResize(int NewW, int NewH)
        {
            Core_.CoreEvent("Resize", '\0', false, false, false);
        }

        public int BellMethod = 0;

        public bool BellOccured = false;

        public void Bell()
        {
            BellOccured = true;
            if (BellMethod == 0)
            {
                return;
            }
            Task Thr = new Task(Bell_);
            Thr.Start();
        }

        private void Bell_()
        {
            try
            {
                if (BellMethod == 1) { Console.Beep(800, 100); }
                if (BellMethod == 2) { Console.Write((char)(0x07)); }
            }
            catch
            {

            }
        }

        public void MemoPrepare()
        {
            if (UseMemo != 0)
            {
                ScrChrB = new int[WinW, WinH];
                ScrChrF = new int[WinW, WinH];
                ScrChrA = new int[WinW, WinH];
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

        public virtual void CharRepaint(int X, int Y)
        {

        }


        // 0 - Bes mrugania
        // 1 - styl DOS
        // 2 - styl VT100
        public int FontModeBlink = 1;
        public int FontModeBold = 1;
        public int FontModeItalic = 1;
        public int FontModeUnderline = 1;
        public int FontModeStrike = 1;

        public bool ANSIIgnoreConcealed = false;
        public int ANSIReverseMode = 0;
        public bool ANSIColors = true;
        public bool ANSIColorBlink = false;
        public bool ANSIColorBold = false;

        protected int CalcBlink_Back;
        protected int CalcBlink_Fore;

        protected void CalcBlink(int FontBack, int FontFore, int FontAttr)
        {
            CalcBlink_Back = FontBack;
            CalcBlink_Fore = FontFore;
            if (FontModeBlink == 0)
            {
                return;
            }
            switch (FontModeBlink)
            {
                case 1:
                    {
                        if ((FontAttr & 8) > 0)
                        {
                            CalcBlink_Fore += 16;
                            CalcBlink_Back += 16;
                        }
                        /*if ((FontAttr & 128) > 0)
                        {
                            if (((FontAttr & 8) > 0) && ((FontAttr & 16) > 0))
                            {
                                CalcBlink_Fore += 16;
                            }

                            if (((FontAttr & 8) > 0) && ((FontAttr & 16) == 0))
                            {
                                CalcBlink_Back += 16;
                            }
                        }
                        else
                        {
                            if (((FontAttr & 8) > 0) && ((FontAttr & 16) > 0))
                            {
                                CalcBlink_Back += 16;
                            }

                            if (((FontAttr & 8) > 0) && ((FontAttr & 16) == 0))
                            {
                                CalcBlink_Fore += 16;
                            }
                        }*/
                    }
                    return;
                case 2:
                    {
                        if ((FontAttr & 8) > 0)
                        {
                            CalcBlink_Fore = FontBack;
                        }
                    }
                    return;
            }
        }


        public int CalcColor_Back;
        public int CalcColor_Fore;

        public void CalcColor0(int ColorBack, int ColorFore)
        {
            if (ColorBack < 0)
            {
                if (CalcColor_Back == Core_.TextNormalBack)
                {
                    CalcColor_Back = -1;
                }
            }
            if (ColorFore < 0)
            {
                if (CalcColor_Fore == Core_.TextNormalFore)
                {
                    CalcColor_Fore = -1;
                }
            }
        }

        public void CalcColor(int ColorBack, int ColorFore, int FontAttr)
        {
            if (ANSIColors)
            {
                CalcColor_Back = ColorBack;
                CalcColor_Fore = ColorFore;
            }
            else
            {
                CalcColor_Back = -1;
                CalcColor_Fore = -1;
            }


            if (CalcColor_Back < 0)
            {
                CalcColor_Back = Core_.TextNormalBack;
            }
            if (CalcColor_Fore < 0)
            {
                CalcColor_Fore = Core_.TextNormalFore;
            }

            if (FontAttr < 0)
            {
                return;
            }

            if (ANSIReverseMode == 1)
            {
                if ((FontAttr & 16) != 0)
                {
                    int Temp = CalcColor_Fore;
                    CalcColor_Fore = CalcColor_Back;
                    CalcColor_Back = Temp;
                }
            }

            if (((FontAttr & 1) != 0) && (ANSIColorBold))
            {
                if (CalcColor_Fore < 8)
                {
                    if ((CalcColor_Fore >= 0) && (CalcColor_Fore < 8))
                    {
                        CalcColor_Fore += 8;
                    }
                }
                else
                {
                    if ((CalcColor_Fore >= 8) && (CalcColor_Fore < 16))
                    {
                        CalcColor_Fore -= 8;
                    }
                }
            }

            if (((FontAttr & 8) != 0) && (ANSIColorBlink))
            {
                if (CalcColor_Back < 8)
                {
                    if ((CalcColor_Back >= 0) && (CalcColor_Back < 8))
                    {
                        CalcColor_Back += 8;
                    }
                }
                else
                {
                    if ((CalcColor_Back >= 8) && (CalcColor_Back < 16))
                    {
                        CalcColor_Back -= 8;
                    }
                }
            }

            if (ANSIReverseMode == 2)
            {
                if ((FontAttr & 16) != 0)
                {
                    int Temp = CalcColor_Fore;
                    CalcColor_Fore = CalcColor_Back;
                    CalcColor_Back = Temp;
                }
            }

            if ((FontAttr & 128) > 0)
            {
                int Temp = CalcColor_Fore;
                CalcColor_Fore = CalcColor_Back;
                CalcColor_Back = Temp;
            }

            if (((FontAttr & 32) != 0) && (!ANSIIgnoreConcealed))
            {
                CalcColor_Fore = CalcColor_Back;
            }
        }

        protected virtual void PutChar_(int X, int Y, int C, int ColorBack, int ColorFore, int FontW, int FontH, int FontAttr)
        {
            
        }

        public void PutChar(int X, int Y, int C, int ColorBack, int ColorFore)
        {
            if ((X >= 0) && (Y >= 0) && (X < WinW) && (Y < WinH))
            {
                if (ColorBack < 0)
                {
                    ColorBack = Core_.TextNormalBack;
                }
                if (ColorFore < 0)
                {
                    ColorFore = Core_.TextNormalFore;
                }
                PutChar_(X, Y, C, ColorBack, ColorFore, 0, 0, 0);
                if (UseMemo > 0)
                {
                    if (C == 0)
                    {
                        C = ' ';
                    }
                    ScrChrC[X, Y] = C;
                    ScrChrB[X, Y] = ColorBack;
                    ScrChrF[X, Y] = ColorFore;
                    ScrChrA[X, Y] = 0;
                    ScrChrFontW[X, Y] = 0;
                    ScrChrFontH[X, Y] = 0;
                    return;
                }
            }
        }

        public void PutChar(int X, int Y, int C, int ColorBack, int ColorFore, int FontW, int FontH, int ColorAttr)
        {
            if ((X >= 0) && (Y >= 0) && (X < WinW) && (Y < WinH))
            {
                CalcColor(ColorBack, ColorFore, ColorAttr);

                PutChar_(X, Y, C, CalcColor_Back, CalcColor_Fore, FontW, FontH, ColorAttr);
                if (UseMemo > 0)
                {
                    if (C == 0)
                    {
                        C = ' ';
                    }
                    ScrChrC[X, Y] = C;
                    ScrChrB[X, Y] = CalcColor_Back;
                    ScrChrF[X, Y] = CalcColor_Fore;
                    ScrChrA[X, Y] = ColorAttr;
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

        public virtual void Clear(int ColorB, int ColorF)
        {
            CalcColor(ColorB, ColorF, 0);
            for (int Y = 0; Y < WinH; Y++)
            {
                SetLineOffset(Y, 0, false, ColorB, ColorF, 0);
                for (int X = 0; X < WinW; X++)
                {
                    PutChar_(X, Y, 32, CalcColor_Back, CalcColor_Fore, 0, 0, 0);
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
                    PutChar(i, WinH - 1, StatusText[i], ColorBack, ColorFore);
                }
                else
                {
                    PutChar(i, WinH - 1, ' ', ColorBack, ColorFore);
                }
            }
        }

        protected int CursorX = 0;
        protected int CursorY = 0;
        protected int CursorB = -1;
        protected int CursorF = -1;
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
                PutChar(CursorX, CursorY, Text[i], ColorB, ColorF);
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
            PutChar(CursorX, CursorY, 32, ColorB, ColorF);
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
