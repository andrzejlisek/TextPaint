﻿/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-07
 * Time: 23:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Threading;

namespace TextPaint
{
    /// <summary>
    /// Description of ScreenConsole.
    /// </summary>
    public class ScreenConsole : Screen
    {
        bool UseTerminalColorCodes = false;
        string ConIEnc = "";
        string ConOEnc = "";

        ConsoleColor[] ConsoleColor_ = new ConsoleColor[16];
        string[] ConsoleColor_B = new string[16];
        string[] ConsoleColor_F = new string[16];
        int LastColorBack = 0;
        int LastColorFore = 7;
        int DefaultBack = 0;
        int DefaultFore = 7;

        bool LastFormatB = false;
        bool LastFormatI = false;
        bool LastFormatU = false;
        bool LastFormatS = false;
        bool LastFormatX = false;

        Timer ConsoleTimer;

        void SetBackColor(int N)
        {
            if (UseTerminalColorCodes)
            {
                Console.Write(ConsoleColor_B[N]);
            }
            else
            {
                Console.BackgroundColor = ConsoleColor_[N];
            }
        }

        void SetForeColor(int N)
        {
            if (UseTerminalColorCodes)
            {
                Console.Write(ConsoleColor_F[N]);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor_[N];
            }
        }

        public ScreenConsole(Core Core__, int WinFixed_, ConfigFile CF, int DefBack, int DefFore) : base()
        {
            LoadConfig(CF);

            LoadDuospaceList();

            UseTerminalColorCodes = CF.ParamGetB("ConUseEscapeCodes");

            WinFixed = WinFixed_;
            Core_ = Core__;
            ConIEnc = CF.ParamGetS("ConInputEncoding");
            ConOEnc = CF.ParamGetS("ConOutputEncoding");
            ConsoleColor_[0] = ConsoleColor.Black;
            ConsoleColor_[1] = ConsoleColor.DarkRed;
            ConsoleColor_[2] = ConsoleColor.DarkGreen;
            ConsoleColor_[3] = ConsoleColor.DarkYellow;
            ConsoleColor_[4] = ConsoleColor.DarkBlue;
            ConsoleColor_[5] = ConsoleColor.DarkMagenta;
            ConsoleColor_[6] = ConsoleColor.DarkCyan;
            ConsoleColor_[7] = ConsoleColor.Gray;
            ConsoleColor_[8] = ConsoleColor.DarkGray;
            ConsoleColor_[9] = ConsoleColor.Red;
            ConsoleColor_[10] = ConsoleColor.Green;
            ConsoleColor_[11] = ConsoleColor.Yellow;
            ConsoleColor_[12] = ConsoleColor.Blue;
            ConsoleColor_[13] = ConsoleColor.Magenta;
            ConsoleColor_[14] = ConsoleColor.Cyan;
            ConsoleColor_[15] = ConsoleColor.White;

            ConsoleColor_B[0] = "\x1b[40m";
            ConsoleColor_B[1] = "\x1b[41m";
            ConsoleColor_B[2] = "\x1b[42m";
            ConsoleColor_B[3] = "\x1b[43m";
            ConsoleColor_B[4] = "\x1b[44m";
            ConsoleColor_B[5] = "\x1b[45m";
            ConsoleColor_B[6] = "\x1b[46m";
            ConsoleColor_B[7] = "\x1b[47m";
            ConsoleColor_B[8] = "\x1b[100m";
            ConsoleColor_B[9] = "\x1b[101m";
            ConsoleColor_B[10] = "\x1b[102m";
            ConsoleColor_B[11] = "\x1b[103m";
            ConsoleColor_B[12] = "\x1b[104m";
            ConsoleColor_B[13] = "\x1b[105m";
            ConsoleColor_B[14] = "\x1b[106m";
            ConsoleColor_B[15] = "\x1b[107m";

            ConsoleColor_F[0] = "\x1b[30m";
            ConsoleColor_F[1] = "\x1b[31m";
            ConsoleColor_F[2] = "\x1b[32m";
            ConsoleColor_F[3] = "\x1b[33m";
            ConsoleColor_F[4] = "\x1b[34m";
            ConsoleColor_F[5] = "\x1b[35m";
            ConsoleColor_F[6] = "\x1b[36m";
            ConsoleColor_F[7] = "\x1b[37m";
            ConsoleColor_F[8] = "\x1b[90m";
            ConsoleColor_F[9] = "\x1b[91m";
            ConsoleColor_F[10] = "\x1b[92m";
            ConsoleColor_F[11] = "\x1b[93m";
            ConsoleColor_F[12] = "\x1b[94m";
            ConsoleColor_F[13] = "\x1b[95m";
            ConsoleColor_F[14] = "\x1b[96m";
            ConsoleColor_F[15] = "\x1b[97m";

            DefaultBack = DefBack;
            DefaultFore = DefFore;
        }

        void ConsoleTimerEvent(object o)
        {
            int TempW = Console.WindowWidth;
            int TempH = Console.WindowHeight;
            if ((TempW > 0) && (TempH > 0))
            {
                if ((WinW != TempW) || (WinH != TempH))
                {
                    Core_.CoreEvent("ResizeW", (char)TempW, false, false, false);
                    Core_.CoreEvent("ResizeH", (char)TempH, false, false, false);
                    Core_.CoreEvent("Resize", '\0', false, false, false);
                }
            }
        }

        protected override void PutChar_(int X, int Y, int C, int ColorBack, int ColorFore, int FontW, int FontH, int ColorAttr)
        {
            Monitor.Enter(GraphMutex);
            if ((Y == (WinH - 1)) && (X == (WinW - 1)))
            {
                Monitor.Exit(GraphMutex);
                return;
            }
            try
            {
                Console.SetCursorPosition(X, Y);
            }
            catch
            {
                Monitor.Exit(GraphMutex);
                return;
            }
            if (LastColorBack != ColorBack)
            {
                LastColorBack = ColorBack;
                SetBackColor(ColorBack);
            }
            if (LastColorFore != ColorFore)
            {
                LastColorFore = ColorFore;
                SetForeColor(ColorFore);
            }
            bool IsFormatB = ((ColorAttr & 1) > 0);
            bool IsFormatI = ((ColorAttr & 2) > 0);
            bool IsFormatU = ((ColorAttr & 4) > 0);
            bool IsFormatS = ((ColorAttr & 64) > 0);
            bool IsFormatX = ((ColorAttr & 8) > 0);

            if (LastFormatB != IsFormatB)
            {
                if (FontModeBold > 0)
                {
                    if (IsFormatB)
                    {
                        Console.Write("\x1b[1m");
                    }
                    else
                    {
                        Console.Write("\x1b[22m");
                    }
                }
                LastFormatB = IsFormatB;
            }

            if (LastFormatI != IsFormatI)
            {
                if (FontModeItalic > 0)
                {
                    if (IsFormatI)
                    {
                        Console.Write("\x1b[3m");
                    }
                    else
                    {
                        Console.Write("\x1b[23m");
                    }
                }
                LastFormatI = IsFormatI;
            }

            if (LastFormatU != IsFormatU)
            {
                if (FontModeUnderline > 0)
                {
                    if (IsFormatU)
                    {
                        Console.Write("\x1b[4m");
                    }
                    else
                    {
                        Console.Write("\x1b[24m");
                    }
                }
                LastFormatU = IsFormatU;
            }

            if (LastFormatS != IsFormatS)
            {
                if (FontModeStrike > 0)
                {
                    if (IsFormatS)
                    {
                        Console.Write("\x1b[9m");
                    }
                    else
                    {
                        Console.Write("\x1b[29m");
                    }
                }
                LastFormatS = IsFormatS;
            }

            if (LastFormatX != IsFormatX)
            {
                if (FontModeBlink > 0)
                {
                    if (IsFormatX)
                    {
                        Console.Write("\x1b[5m");
                    }
                    else
                    {
                        Console.Write("\x1b[25m");
                    }
                }
                LastFormatX = IsFormatX;
            }

            if (UseMemo != 0)
            {
                ScrChrC[X, Y] = C;
                ScrChrB[X, Y] = ColorBack;
                ScrChrF[X, Y] = ColorFore;
                ScrChrA[X, Y] = ColorAttr;
            }
            if (((C >= 0x20) && (C < 0xD800)) || (C > 0xDFFF))
            {
                Console.Write(char.ConvertFromUtf32(C));
            }
            else
            {
                Console.Write(" ");
            }
            Monitor.Exit(GraphMutex);
        }


        bool CharCopy(int SrcX, int SrcY, int DstX, int DstY)
        {
            ScrChrC[DstX, DstY] = ScrChrC[SrcX, SrcY];
            ScrChrB[DstX, DstY] = ScrChrB[SrcX, SrcY];
            ScrChrF[DstX, DstY] = ScrChrF[SrcX, SrcY];
            ScrChrA[DstX, DstY] = ScrChrA[SrcX, SrcY];
            if (ScrChrC[DstX, DstY] == 0)
            {
                return false;
            }
            if ((ScrChrC[DstX, DstY] < 32) || (ScrChrC[DstX, DstY] > 127))
            {
                return true;
            }
            return (UseMemo == 2);
        }

        public override void Move(int SrcX, int SrcY, int DstX, int DstY, int W, int H)
        {
            while ((SrcX < 0) || (DstX < 0))
            {
                SrcX++;
                DstX++;
                W--;
            }
            while ((SrcY < 0) || (DstY < 0))
            {
                SrcY++;
                DstY++;
                H--;
            }
            while (((SrcX + W) > WinW) || ((DstX + W) > WinW))
            {
                W--;
            }
            while (((SrcY + H) > WinH) || ((DstY + H) > WinH))
            {
                H--;
            }


            Monitor.Enter(GraphMutex);
            if (UseMemo != 2)
            {
                try
                {
                    Console.MoveBufferArea(SrcX, SrcY, W, H, DstX, DstY);
                }
                catch
                {
                }
            }
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
                                if (CharCopy(X + SrcX, Y + SrcY, X_, Y_))
                                {
                                    PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_], ScrChrFontW[X_, Y_], ScrChrFontH[X_, Y_], ScrChrA[X_, Y_]);
                                }
                            }
                        }
                        else
                        {
                            for (int X = (W - 1); X >= 0; X--)
                            {
                                X_ = X + DstX;
                                Y_ = Y + DstY;
                                if (CharCopy(X + SrcX, Y + SrcY, X_, Y_))
                                {
                                    PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_], ScrChrFontW[X_, Y_], ScrChrFontH[X_, Y_], ScrChrA[X_, Y_]);
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
                                if (CharCopy(X + SrcX, Y + SrcY, X_, Y_))
                                {
                                    PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_], ScrChrFontW[X_, Y_], ScrChrFontH[X_, Y_], ScrChrA[X_, Y_]);
                                }
                            }
                        }
                        else
                        {
                            for (int X = (W - 1); X >= 0; X--)
                            {
                                X_ = X + DstX;
                                Y_ = Y + DstY;
                                if (CharCopy(X + SrcX, Y + SrcY, X_, Y_))
                                {
                                    PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_], ScrChrFontW[X_, Y_], ScrChrFontH[X_, Y_], ScrChrA[X_, Y_]);
                                }
                            }
                        }
                    }
                }
            }
            Monitor.Exit(GraphMutex);
        }

        //ConsoleColor ToolBack = ConsoleColor.White;
        //ConsoleColor ToolFore = ConsoleColor.Black;

        public override bool AppResize(int NewW, int NewH, bool Force)
        {
            if (Force)
            {
                Monitor.Enter(GraphMutex);
                if ((WinW != NewW) || (WinH != NewH) || Force)
                {
                    WinW = NewW;
                    WinH = NewH;
                    SetBackColor(DefaultBack);
                    SetForeColor(DefaultFore);
                    LastColorBack = DefaultBack;
                    LastColorFore = DefaultFore;
                    Console.Clear();
                    MemoPrepare();
                }
                Monitor.Exit(GraphMutex);
                return true;
            }
            return false;
        }

        public void DetectSize()
        {
            Console.Clear();
            WinW = -1;
            WinH = -1;
            while (WinH < Console.CursorTop)
            {
                WinH = Console.CursorTop;
                Console.WriteLine();
            }
            while (WinW < Console.CursorLeft)
            {
                WinW = Console.CursorLeft;
                Console.Write(" ");
            }
            Console.Clear();
            WinW++; 
            WinH++;
        }

        public override void StartApp()
        {
            if (!("".Equals(ConIEnc)))
            {
                Console.InputEncoding = TextWork.EncodingFromName(ConIEnc);
            }
            if (!("".Equals(ConOEnc)))
            {
                Console.OutputEncoding = TextWork.EncodingFromName(ConOEnc);
            }

            AppWorking = true;
            Core_.StartUp();
            ConsoleTimer = new Timer(ConsoleTimerEvent, null, 0, 500);
            while (AppWorking)
            {
                ConsoleKeyInfo CKI = Console.ReadKey(true);
                Core_.CoreEvent(UniKeyName(CKI.Key.ToString()), CKI.KeyChar, CKI.Modifiers.HasFlag(ConsoleModifiers.Shift), CKI.Modifiers.HasFlag(ConsoleModifiers.Control), CKI.Modifiers.HasFlag(ConsoleModifiers.Alt));
            }
            SetBackColor(DefaultBack);
            SetForeColor(DefaultFore);
            LastColorBack = DefaultBack;
            LastColorFore = DefaultFore;
            Console.Clear();
        }

        public override void SetCursorPositionNoRefresh(int X, int Y)
        {
            SetCursorPosition(X, Y);
        }

        public override void SetCursorPosition(int X, int Y)
        {
            Monitor.Enter(GraphMutex);

            if (X >= WinW)
            {
                X = WinW - 1;
            }
            if (Y >= WinH)
            {
                Y = WinH - 1;
            }
            if (X < 0)
            {
                X = 0;
            }
            if (Y < 0)
            {
                Y = 0;
            }
            CursorX = X;
            CursorY = Y;
            try
            {
                Console.SetCursorPosition(X, Y);
            }
            catch
            {
                Console.SetCursorPosition(0, 0);
            }
            Monitor.Exit(GraphMutex);
        }
    }
}
