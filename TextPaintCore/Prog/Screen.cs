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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TextPaint
{
    /// <summary>
    /// Description of Screen.
    /// </summary>
    public class Screen
    {
        public const int BlankDoubleChar = (0x10FFFF + 1);
        public const int BlankDoubleCharVis = 32;

        protected object GraphMutex = new object();

        public bool MultiThread = false;

        public const int TerminalCellW = 8;
        public const int TerminalCellH = 16;
        public const int TerminalCellW2 = 4;
        public const int TerminalCellH2 = 8;

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

        protected int DuospaceMode = 0;
        protected string DuospaceFontName = "";
        protected string DuospaceDoubleChars = "";

        protected bool TimerFast = true;

        public void LoadConfig(ConfigFile CF)
        {
            TimerFast = !CF.ParamGetB("WinTimer100", false);

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

            DuospaceMode = CF.ParamGetI("DuospaceMode", 0);
            DuospaceFontName = CF.ParamGetS("DuospaceFontName", "");
            DuospaceDoubleChars = CF.ParamGetS("DuospaceDoubleChars", "");
        }

        protected void LoadDuospaceList()
        {
            if ((DuospaceDoubleChars != "") && (DuospaceMode == 1))
            {
                try
                {
                    FileStream FS = new FileStream(DuospaceDoubleChars, FileMode.Open, FileAccess.Read);
                    StreamReader FS_ = new StreamReader(FS);
                    string Buf = FS_.ReadLine();
                    while (Buf != null)
                    {
                        if (Buf.Length >= 2)
                        {
                            int Code1 = 0;
                            int Code2 = 0;
                            string[] BufX = Buf.Split('.');
                            if (BufX.Length == 1)
                            {
                                Code1 = TextWork.CodeChar(BufX[0]);
                                Code2 = TextWork.CodeChar(BufX[0]);
                            }
                            if (BufX.Length == 3)
                            {
                                Code1 = TextWork.CodeChar(BufX[0]);
                                Code2 = TextWork.CodeChar(BufX[2]);
                            }
                            for (int i = Code1; i <= Code2; i++)
                            {
                                if (CharDoubleTable[i] == 0)
                                {
                                    CharDoubleTable[i] = BlankDoubleChar;
                                }
                            }
                        }
                        Buf = FS_.ReadLine();
                    }
                    FS_.Close();
                    FS.Close();
                }
                catch
                {

                }
            }
            CharDoubleTableInvRefresh();
        }

        public virtual void SetLineOffset(int Y, int Offset, bool Blank, int ColorBack, int ColorFore, int FontAttr)
        {

        }

        public virtual void RepaintOffset(int Y)
        {

        }

        public virtual bool AppResize(int NewW, int NewH, bool Force)
        {
            return Force;
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

        public int[] CharDoubleTable;
        public int[] CharDoubleTableInv;

        protected void CharDoubleTableInvRefresh()
        {
            for (int i = 0; i < CharDoubleTableInv.Length; i++)
            {
                CharDoubleTableInv[i] = 0;
            }
            CharDoubleTableInv[BlankDoubleChar] = BlankDoubleChar;
            for (int i = 0; i < CharDoubleTable.Length; i++)
            {
                if ((CharDoubleTable[i] >= 0) && (CharDoubleTable[i] != BlankDoubleChar))
                {
                    CharDoubleTableInv[CharDoubleTable[i]] = i;
                }
            }
        }

        public Screen()
        {
            CharDoubleTable = new int[18 * 65536];
            CharDoubleTableInv = new int[18 * 65536];
            for (int i = 0; i < CharDoubleTable.Length; i++)
            {
                CharDoubleTable[i] = 0;
                CharDoubleTableInv[i] = 0;
            }
        }

        public int CharDoubleInv(int C)
        {
            if (C != BlankDoubleChar)
            {
                return CharDoubleTableInv[C];
            }
            else
            {
                return BlankDoubleCharVis;
            }
        }

        public int CharDouble(int C)
        {
            return CharDoubleTable[C];
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
            PutChar(X, Y, C, ColorBack, ColorFore, 0, 0);
        }

        public void PutChar(int X, int Y, int C, int ColorBack, int ColorFore, int FontW, int FontH)
        {
            if ((X >= 0) && (Y >= 0) && (X < WinW) && (Y < WinH) && (C != BlankDoubleChar))
            {
                if (ColorBack < 0)
                {
                    ColorBack = Core_.TextNormalBack;
                }
                if (ColorFore < 0)
                {
                    ColorFore = Core_.TextNormalFore;
                }
                PutChar_(X, Y, C, ColorBack, ColorFore, FontW, FontH, 0);
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
                    ScrChrFontW[X, Y] = FontW;
                    ScrChrFontH[X, Y] = FontH;
                    return;
                }
            }
        }

        public void PutChar(int X, int Y, int C, int ColorBack, int ColorFore, int FontW, int FontH, int ColorAttr)
        {
            if ((X >= 0) && (Y >= 0) && (X < WinW) && (Y < WinH) && (C != BlankDoubleChar))
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
                    PutChar_(X, Y, 32, MouseCalcColor(X, Y, CalcColor_Back), MouseCalcColor(X, Y, CalcColor_Fore), 0, 0, 0);
                }
            }
        }

        public virtual void Move(int SrcX, int SrcY, int DstX, int DstY, int W, int H)
        {
            
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
            int CharDbl = 0;
            for (int i = 0; i < WinW; i++)
            {
                if (CharDbl != 0)
                {
                    if (CharDbl > 0)
                    {
                        PutChar(i, WinH - 1, CharDbl, ColorBack, ColorFore);
                    }
                    CharDbl = 0;
                }
                else
                {
                    if (i < StatusText.Count)
                    {
                        PutChar(i, WinH - 1, StatusText[i], ColorBack, ColorFore);
                        CharDbl = CharDouble(StatusText[i]);
                    }
                    else
                    {
                        PutChar(i, WinH - 1, ' ', ColorBack, ColorFore);
                        CharDbl = 0;
                    }
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

        protected void MouseCursorDrawPrepare(bool Show)
        {
            if (Show)
            {
                if (MouseIsActiveX && (MouseX >= 0) && (MouseY >= 0) && (MouseX < WinW) && (MouseY < WinH))
                {
                    MouseCursorDraw(MouseX, MouseY);
                    MouseXScr = MouseX;
                    MouseYScr = MouseY;
                }
            }
            else
            {
                if ((MouseXScr >= 0) && (MouseYScr >= 0) && (MouseXScr < WinW) && (MouseYScr < WinH))
                {
                    MouseCursorDraw(MouseXScr, MouseYScr);
                    MouseXScr = -1;
                    MouseYScr = -1;
                }
            }
        }

        int MouseXScr = -1;
        int MouseYScr = -1;
        public int MouseX = -1;
        public int MouseY = -1;
        public bool MouseIsActive = false;
        public bool MouseIsActiveX = false;

        private Dictionary<int, bool> MouseParam = new Dictionary<int, bool>();

        public int MouseZero = 65535;

        public void MouseReset()
        {
            Monitor.Enter(MouseParam);
            MouseParam.Clear();
            Monitor.Exit(MouseParam);
        }

        public void MouseSet(int Param, bool Val)
        {
            Monitor.Enter(MouseParam);
            if (!MouseParam.ContainsKey(Param))
            {
                MouseParam.Add(Param, Val);
            }
            else
            {
                MouseParam[Param] = Val;
            }
            MouseActive(MouseIsActive);
            Monitor.Exit(MouseParam);
        }

        public bool MouseGet(int Param)
        {
            Monitor.Enter(MouseParam);
            bool X = false;
            if (MouseParam.ContainsKey(Param))
            {
                X = MouseParam[Param];
            }
            Monitor.Exit(MouseParam);
            return X;
        }

        public void MouseActive(bool X)
        {
            MouseIsActive = X;
            bool XX = MouseIsActive;
            if (XX)
            {
                bool ParX = false;
                if (MouseGet(9)) { ParX = true; }
                if (MouseGet(1000)) { ParX = true; }
                if (MouseGet(1001)) { ParX = true; }
                if (MouseGet(1002)) { ParX = true; }
                if (MouseGet(1003)) { ParX = true; }
                if (!ParX)
                {
                    XX = false;
                }
            }
            if (MouseIsActiveX != XX)
            {
                MouseCursorDrawPrepare(false);
                MouseIsActiveX = XX;
                MouseCursorDrawPrepare(true);
            }
        }

        protected int MouseCalcColor(int X, int Y, int ColorX)
        {
            if ((MouseX == X) && (MouseY == Y))
            {
                if (ColorX < 8)
                {
                    return 7 - ColorX;
                }
                else
                {
                    return 23 - ColorX;
                }
            }
            else
            {
                return ColorX;
            }
        }

        protected virtual void MouseCursorDraw(int X, int Y)
        {

        }
    }
}
