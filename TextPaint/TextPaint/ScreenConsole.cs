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
        string ConIEnc = "";
        string ConOEnc = "";

        ConsoleColor[] ConsoleColor_ = new ConsoleColor[16];
        ConsoleColor LastColorBack = ConsoleColor.Black;
        ConsoleColor LastColorFore = ConsoleColor.Gray;
        ConsoleColor DefaultBack = ConsoleColor.Black;
        ConsoleColor DefaultFore = ConsoleColor.Gray;

        public ScreenConsole(Core Core__, ConfigFile CF, int DefBack, int DefFore)
        {
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
            DefaultBack = ConsoleColor_[DefBack];
            DefaultFore = ConsoleColor_[DefFore];
        }

        protected override void PutChar_(int X, int Y, int C, int ColorBack, int ColorFore, int FontW, int FontH)
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
            if (LastColorBack != ConsoleColor_[ColorBack])
            {
                LastColorBack = ConsoleColor_[ColorBack];
                Console.BackgroundColor = ConsoleColor_[ColorBack];
            }
            if (LastColorFore != ConsoleColor_[ColorFore])
            {
                LastColorFore = ConsoleColor_[ColorFore];
                Console.ForegroundColor = ConsoleColor_[ColorFore];
            }
            if (UseMemo != 0)
            {
                ScrChrC[X, Y] = C;
                ScrChrB[X, Y] = ColorBack;
                ScrChrF[X, Y] = ColorFore;
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
                                    PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_], ScrChrFontW[X_, Y_], ScrChrFontH[X_, Y_]);
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
                                    PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_], ScrChrFontW[X_, Y_], ScrChrFontH[X_, Y_]);
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
                                    PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_], ScrChrFontW[X_, Y_], ScrChrFontH[X_, Y_]);
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
                                    PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_], ScrChrFontW[X_, Y_], ScrChrFontH[X_, Y_]);
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

        public override bool WindowResize()
        {
            Monitor.Enter(GraphMutex);
            if ((WinW != Console.WindowWidth) || (WinH != Console.WindowHeight))
            {
                WinW = Console.WindowWidth;
                WinH = Console.WindowHeight;
                
                MemoPrepare();

                /*if ((Console.BufferHeight > WinW) || (Console.BufferHeight > WinH))
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Clear();
                    Console.BackgroundColor = ToolBack;
                    Console.ForegroundColor = ToolFore;
                    Console.SetCursorPosition(WinW - 1, WinH - 1);
                    Console.Write(" ");
                }
                else
                {
                    Console.BackgroundColor = ToolBack;
                    Console.ForegroundColor = ToolFore;
                    Console.Clear();
                }*/
                Console.BackgroundColor = DefaultBack;
                Console.ForegroundColor = DefaultFore;
                LastColorBack = DefaultBack;
                LastColorFore = DefaultFore;
                Console.Clear();

                Monitor.Exit(GraphMutex);
                return true;
            }
            else
            {
                Monitor.Exit(GraphMutex);
                return false;
            }
        }

        public override void StartApp()
        {
            if (ConIEnc != "")
            {
                Console.InputEncoding = TextWork.EncodingFromName(ConIEnc);
            }
            if (ConOEnc != "")
            {
                Console.OutputEncoding = TextWork.EncodingFromName(ConOEnc);
            }

            WinW = -1;
            WinH = -1;
            AppWorking = true;
            Core_.WindowResize();
            Core_.ScreenRefresh(true);
            Core_.StartUp();
            while (AppWorking)
            {
                ConsoleKeyInfo CKI = Console.ReadKey(true);
                Core_.CoreEvent(UniKeyName(CKI.Key.ToString()), CKI.KeyChar, CKI.Modifiers.HasFlag(ConsoleModifiers.Shift), CKI.Modifiers.HasFlag(ConsoleModifiers.Control), CKI.Modifiers.HasFlag(ConsoleModifiers.Alt));
            }
            Console.BackgroundColor = DefaultBack;
            Console.ForegroundColor = DefaultFore;
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
            Console.SetCursorPosition(X, Y);
            Monitor.Exit(GraphMutex);
        }
    }
}
