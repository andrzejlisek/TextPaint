/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-07
 * Time: 23:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace TextPaint
{
    /// <summary>
    /// Description of ScreenConsole.
    /// </summary>
    public class ScreenConsole : Screen
    {
        string ConIEnc = "";
        string ConOEnc = "";

        ConsoleColor[] ConsoleColor_ = new ConsoleColor[4];
        ConsoleColor LastColorBack = ConsoleColor.Green;
        ConsoleColor LastColorFore = ConsoleColor.Green;

        public ScreenConsole(Core Core__, ConfigFile CF)
        {
            Core_ = Core__;
            ConIEnc = CF.ParamGetS("ConInputEncoding");
            ConOEnc = CF.ParamGetS("ConOutputEncoding");
            ConsoleColor_[0] = ConsoleColor.Black;
            ConsoleColor_[1] = ConsoleColor.DarkGray;
            ConsoleColor_[2] = ConsoleColor.Gray;
            ConsoleColor_[3] = ConsoleColor.White;
        }

        protected override void PutChar_(int X, int Y, int C, int ColorBack, int ColorFore)
        {
            if ((Y == (WinH - 1)) && (X == (WinW - 1)))
            {
                return;
            }
            try
            {
                Console.SetCursorPosition(X, Y);
            }
            catch
            {
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

            if (((C >= 0x20) && (C < 0xD800)) || (C > 0xDFFF))
            {
                Console.Write(char.ConvertFromUtf32(C));
            }
            else
            {
                Console.Write(" ");
            }
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
                                if (CharCopy(X + SrcX, Y + SrcY, X_, Y_))
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
                                if (CharCopy(X + SrcX, Y + SrcY, X_, Y_))
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
                                if (CharCopy(X + SrcX, Y + SrcY, X_, Y_))
                                {
                                    PutChar_(X_, Y_, ScrChrC[X_, Y_], ScrChrB[X_, Y_], ScrChrF[X_, Y_]);
                                }
                            }
                        }
                    }
                }
            }
        }

        ConsoleColor ToolBack = ConsoleColor.White;
        ConsoleColor ToolFore = ConsoleColor.Black;
        
        public override bool WindowResize()
        {
            if ((WinW != Console.WindowWidth) || (WinH != Console.WindowHeight))
            {
                WinW = Console.WindowWidth;
                WinH = Console.WindowHeight;
                
                MemoPrepare();

                if ((Console.BufferHeight > WinW) || (Console.BufferHeight > WinH))
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
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public override void StartApp()
        {
            if (ConIEnc != "")
            {
                Console.InputEncoding = Core_.StrToEnc(ConIEnc);
            }
            if (ConOEnc != "")
            {
                Console.OutputEncoding = Core_.StrToEnc(ConOEnc);
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
                Core_.CoreEvent(CKI.Key.ToString(), CKI.KeyChar);
            }
            Console.Clear();
        }

        public override void SetCursorPosition(int X, int Y)
        {
            if (X < 0)
            {
                X = 0;
            }
            if (Y < 0)
            {
                Y = 0;
            }
            if (X >= WinW)
            {
                X = WinW - 1;
            }
            if (Y >= WinH)
            {
                Y = WinH - 1;
            }
            Console.SetCursorPosition(X, Y);
        }
    }
}
