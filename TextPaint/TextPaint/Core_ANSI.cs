using System;
using System.Collections.Generic;

namespace TextPaint
{
    public partial class Core
    {
        public int AnsiMaxX = 0;
        public int AnsiMaxY = 0;

        public int ANSIReverseMode = 0;

        int __AnsiBack_ = -1;
        int __AnsiFore_ = -1;
        int __AnsiX_ = 0;
        int __AnsiY_ = 0;
        bool __AnsiFontBold_ = false;
        bool __AnsiFontInverse_ = false;
        bool __AnsiFontBlink_ = false;
        bool __AnsiFontInvisible_ = false;

        List<int> __AnsiCmd = new List<int>();

        List<int> __AnsiTabs = new List<int>();

        public List<string> __AnsiResponse = new List<string>();

        // Printable character replacement for standard DOS character from 00h to 1Fh
        int[] DosControl = { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                             0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                             0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                             0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

        // Color subsitution from 256-color palette - filled in in constructor
        int[] Color256 = new int[256];

        bool ANSIDOS = false;
        bool ANSIPrintBackspace = false;
        bool ANSIIgnoreBlink = false;
        bool ANSIIgnoreBold = false;
        bool ANSIIgnoreConcealed = false;

        // 0 - Do not change
        // 1 - Use as CRLF
        // 2 - Ommit
        public int ANSI_CR = 0;
        public int ANSI_LF = 0;

        int __AnsiBack = -1;
        int __AnsiFore = -1;
        int __AnsiBackWork = -1;
        int __AnsiForeWork = -1;
        bool __AnsiFontBold = false;
        bool __AnsiFontInverse = false;
        bool __AnsiFontBlink = false;
        bool __AnsiFontInvisible = false;
        int __AnsiFontSizeW = 0;
        int __AnsiFontSizeH = 0;

        public bool __AnsiTestCmd = false;
        int __AnsiTest = 0;
        bool __AnsiCommand = false;
        int __AnsiCounter = 0;
        bool __AnsiScreen = false;

        int __AnsiScrollFirst = 0;
        int __AnsiScrollLast = 0;
        bool __AnsiOrigin = false;

        int __AnsiLineOccupyFactor = 5;

        public bool __AnsiNewLineKey = false;
        bool __AnsiInsertMode = false;

        public int __AnsiX = 0;
        public int __AnsiY = 0;
        List<List<int>> __AnsiLineOccupy = new List<List<int>>();
        List<List<int>> __AnsiLineOccupy1 = new List<List<int>>();
        List<List<int>> __AnsiLineOccupy2 = new List<List<int>>();
        bool __AnsiLineOccupy1_Use = false;
        bool __AnsiLineOccupy2_Use = false;

        bool __AnsiMusic = false;
        bool __AnsiUseEOF = false;
        bool __AnsiBeyondEOF = false;
        bool __AnsiNoWrap = false;

        public bool __AnsiVT52 = false;
        bool[] VT100_SemigraphDef = new bool[4];
        int VT100_SemigraphNum = 0;
        bool[] VT100_SemigraphDef_ = new bool[4];
        int VT100_SemigraphNum_ = 0;
        int[] VT100_SemigraphChars = { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

        bool VT52_SemigraphDef = false;
        int[] VT52_SemigraphChars = {  0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };


        List<int> AnsiBuffer = new List<int>();
        int AnsiBufferI = 0;

        public long __AnsiProcessStep = 0;
        long __AnsiProcessDelay = 0;
        public long __AnsiProcessDelayFactor = 0;
        public long __AnsiProcessDelayMin = 1;
        public long __AnsiProcessDelayMax = -1;

        public int ReportCursorX()
        {
            return __AnsiX + 1;
        }

        public int ReportCursorY()
        {
            if (__AnsiOrigin)
            {
                return __AnsiY - __AnsiScrollFirst + 1;
            }
            else
            {
                return __AnsiY + 1;
            }
        }

        public void AnsiTerminalReset()
        {
            VT100_SemigraphDef[0] = false;
            VT100_SemigraphDef[1] = false;
            VT100_SemigraphDef[2] = false;
            VT100_SemigraphDef[3] = false;
            VT100_SemigraphNum = 0;
            VT100_SemigraphDef_[0] = false;
            VT100_SemigraphDef_[1] = false;
            VT100_SemigraphDef_[2] = false;
            VT100_SemigraphDef_[3] = false;
            VT100_SemigraphNum_ = 0;
            VT52_SemigraphDef = false;
            __AnsiResponse.Clear();
            __AnsiTabs.Clear();
            __AnsiTabs.Add(-1);
            __AnsiBeyondEOF = false;
            if ((WorkMode == 1) || (WorkMode == 2))
            {
                __AnsiScreen = true;
            }
            else
            {
                __AnsiScreen = false;
            }
            __AnsiVT52 = false;
            __AnsiX_ = 0;
            __AnsiY_ = 0;
            __AnsiBack_ = -1;
            __AnsiFore_ = -1;
            __AnsiFontBold_ = false;
            __AnsiFontInverse_ = false;
            __AnsiFontBlink_ = false;
            __AnsiFontInvisible_ = false;
            __AnsiOrigin = false;
            __AnsiNewLineKey = false;
            __AnsiInsertMode = false;
            __AnsiFontSizeW = 0;
            __AnsiFontSizeH = 0;

            __AnsiCmd = new List<int>();

            __AnsiBack = -1;
            __AnsiFore = -1;
            __AnsiFontBold = false;
            __AnsiFontInverse = false;
            __AnsiFontBlink = false;
            __AnsiFontInvisible = false;

            __AnsiTest = 0;
            __AnsiCommand = false;
            __AnsiX = 0;
            __AnsiY = 0;
            __AnsiLineOccupy.Clear();
            __AnsiLineOccupy1.Clear();
            __AnsiLineOccupy2.Clear();

            __AnsiCounter = 0;

            __AnsiScrollFirst = 0;
            __AnsiScrollLast = AnsiMaxY - 1;
            __AnsiMusic = false;
            __AnsiNoWrap = false;

            AnsiScrollReset();
            AnsiFontReset();
            if (__AnsiScreen)
            {
                AnsiRepaint(false);
            }
        }

        public void AnsiProcessReset(bool __AnsiUseEOF_)
        {
            AnsiBuffer.Clear();
            AnsiBufferI = 0;
            __AnsiProcessStep = 0;
            __AnsiProcessDelay = 0;
            __AnsiProcessDelayMin = 1;
            __AnsiProcessDelayMax = -1;
            __AnsiUseEOF = __AnsiUseEOF_;
            AnsiTerminalReset();
        }

        public void AnsiProcessSupply(List<int> TextFileLine)
        {
            if (TextFileLine.Count > 0)
            {
                AnsiBuffer.AddRange(TextFileLine);
            }
        }

        public int AnsiProcess(int ProcessCount)
        {
            int Processed = 0;
            if (ProcessCount < 0)
            {
                ProcessCount = int.MaxValue;
            }
            if ((ProcessCount == 0) || (__AnsiBeyondEOF))
            {
                return 0;
            }
            if (AnsiBufferI >= AnsiBuffer.Count)
            {
                return 0;
            }
            bool WasScroll = false;
            if (WorkMode == 4)
            {
                WasScroll = true;
            }
            while (ProcessCount > 0)
            {
                __AnsiProcessStep++;

                if (__AnsiBeyondEOF)
                {
                    break;
                }
                if (__AnsiProcessStep > __AnsiProcessDelay)
                {

                    if (AnsiScrollCounter == 0)
                    {
                        if (AnsiBufferI >= AnsiBuffer.Count)
                        {
                            if (Processed == 0)
                            {
                                if (WasScroll)
                                {
                                    Processed = -1;
                                }
                            }
                            return Processed;
                        }

                        if (__AnsiCommand)
                        {
                            if (AnsiBuffer[AnsiBufferI] < 32)
                            {
                                if (AnsiBuffer[AnsiBufferI] == 27)
                                {
                                    __AnsiCmd.Clear();
                                }
                                else
                                {
                                    AnsiCharPrint(AnsiBuffer[AnsiBufferI]);
                                }
                            }
                            else
                            {
                                __AnsiCmd.Add(AnsiBuffer[AnsiBufferI]);
                                if (__AnsiTestCmd)
                                {
                                    if (__AnsiCmd.Count > 20)
                                    {
                                        Console.WriteLine("ANSI long command " + TextWork.IntToStr(__AnsiCmd));
                                    }
                                }
                            }

                            if (__AnsiCmd.Count > 0)
                            {
                                if (__AnsiVT52)
                                {
                                    AnsiProcess_VT52();
                                }
                                else
                                {
                                    AnsiProcess_Fixed(AnsiBuffer[AnsiBufferI]);

                                    if (__AnsiCommand && (__AnsiCmd[0] != ']') && (__AnsiCmd.Count >= 2) && (((AnsiBuffer[AnsiBufferI] >= 0x41) && (AnsiBuffer[AnsiBufferI] <= 0x5A)) || ((AnsiBuffer[AnsiBufferI] >= 0x61) && (AnsiBuffer[AnsiBufferI] <= 0x7A)) || (AnsiBuffer[AnsiBufferI] == '@')))
                                    {
                                        __AnsiCommand = false;
                                        string AnsiCmd_ = TextWork.IntToStr(__AnsiCmd);
                                        if (AnsiCmd_.StartsWith("[?"))
                                        {
                                            AnsiProcess_CSI_Question(AnsiCmd_);
                                        }
                                        if (AnsiCmd_.StartsWith("[") && (!AnsiCmd_.StartsWith("[?")))
                                        {
                                            AnsiProcess_CSI_Fixed(AnsiCmd_);
                                        }
                                        else
                                        {
                                            if (__AnsiTestCmd)
                                            {
                                                Console.WriteLine("ANSI non-standard command " + AnsiCmd_);
                                            }
                                        }
                                        string AnsiCmd_0 = "";
                                        for (int i0 = 0; i0 < AnsiCmd_.Length; i0++)
                                        {
                                            if (AnsiCmd_[i0] >= 32)
                                            {
                                                AnsiCmd_0 += AnsiCmd_[i0].ToString();
                                            }
                                            else
                                            {
                                                AnsiCmd_0 = AnsiCmd_0 + "{" + ((int)AnsiCmd_[i0]).ToString("X") + "}";
                                            }
                                        }
                                        if (__AnsiTestCmd)
                                        {
                                            Console.WriteLine("ANSI command: " + AnsiCmd_0);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (AnsiBuffer[AnsiBufferI] == 27)
                            {
                                __AnsiCmd.Clear();
                                __AnsiCommand = true;
                            }
                            else
                            {
                                AnsiCharPrint(AnsiBuffer[AnsiBufferI]);
                            }
                        }
                        Processed++;
                        AnsiBufferI++;
                    }
                    else
                    {
                        if (AnsiScrollProcess())
                        {
                            WasScroll = true;
                        }
                    }
                }


                ProcessCount--;
                __AnsiCounter++;
            }
            if (Processed == 0)
            {
                if (WasScroll)
                {
                    Processed = -1;
                }
            }
            return Processed;
        }


        public void AnsiEnd()
        {
            if ((__AnsiTestCmd) && __AnsiCommand)
            {
                Console.WriteLine("ANSI End of file inside command " + TextWork.IntToStr(__AnsiCmd));
            }
            if (__AnsiTestCmd)
            {
                Console.WriteLine("ANSI OK");
            }
        }

        public void AnsiGetF(int X, int Y, out int Ch, out int ColB, out int ColF)
        {
            int FontW;
            int FontH;
            if (AnsiGetFontSize(Y) > 0)
            {
                AnsiGet(X * 2, Y, out Ch, out ColB, out ColF, out FontW, out FontH);
            }
            else
            {
                AnsiGet(X, Y, out Ch, out ColB, out ColF, out FontW, out FontH);
            }
        }

        public void AnsiGet(int X, int Y, out int Ch, out int ColB, out int ColF, out int FontW, out int FontH)
        {
            Ch = 32;
            ColB = TextNormalBack;
            ColF = TextNormalFore;
            FontW = 0;
            FontH = 0;
            if (__AnsiLineOccupy.Count > Y)
            {
                if ((__AnsiLineOccupy[Y].Count / __AnsiLineOccupyFactor) > X)
                {
                    Ch = __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 0];
                    ColB = __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 1];
                    ColF = __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 2];
                    FontW = __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 3];
                    FontH = __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 4];
                }
            }
        }


        public void AnsiRepaintLine(int Y)
        {
            if (__AnsiScreen)
            {
                if (Y < __AnsiLineOccupy.Count)
                {
                    int L = (__AnsiLineOccupy[Y].Count / __AnsiLineOccupyFactor);
                    for (int X = 0; X < WinW; X++)
                    {
                        if (X < L)
                        {
                            int ColorB = __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 1];
                            int ColorF = __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 2];
                            if (ColorB < 0)
                            {
                                ColorB = TextNormalBack;
                            }
                            if (ColorF < 0)
                            {
                                ColorF = TextNormalFore;
                            }
                            Screen_.PutChar(X, Y, __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 0], ColorB, ColorF, __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 3], __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 4]);
                        }
                        else
                        {
                            Screen_.PutChar(X, Y, ' ', TextNormalBack, TextNormalFore, 0, 0);
                        }
                    }
                }
            }
        }

        int __ScreenMinX = 0;
        int __ScreenMinY = 0;
        int __ScreenMaxX = 0;
        int __ScreenMaxY = 0;
        public void AnsiRepaint(bool AdditionalBuffers)
        {
            Screen_.Clear(TextNormalBack, TextNormalFore);
            __ScreenMinX = 0;
            __ScreenMinY = 0;
            __ScreenMaxX = 0;
            __ScreenMaxY = 0;
            int BufMin = AdditionalBuffers ? 0 : 1;
            int BufMax = AdditionalBuffers ? 2 : 1;
            int Bufoffset = 0;
            for (int BufI = BufMin; BufI <= BufMax; BufI++)
            {
                List<List<int>> __AnsiLineOccupyX;
                if (BufI == 1)
                {
                    __AnsiLineOccupyX = __AnsiLineOccupy;
                }
                else
                {
                    if (BufI == 0)
                    {
                        __AnsiLineOccupyX = __AnsiLineOccupy1;
                    }
                    else
                    {
                        __AnsiLineOccupyX = __AnsiLineOccupy2;
                    }
                }
                for (int Y = 0; Y < __AnsiLineOccupyX.Count; Y++)
                {
                    for (int X = 0; X < (__AnsiLineOccupyX[Y].Count / __AnsiLineOccupyFactor); X++)
                    {
                        int Y__ = Y + Bufoffset;
                        if (__ScreenMinX > X) { __ScreenMinX = X; }
                        if (__ScreenMinY > Y__) { __ScreenMinY = Y__; }
                        if (__ScreenMaxX < X) { __ScreenMaxX = X; }
                        if (__ScreenMaxY < Y__) { __ScreenMaxY = Y__; }

                        int ColorB = __AnsiLineOccupyX[Y][X * __AnsiLineOccupyFactor + 1];
                        int ColorF = __AnsiLineOccupyX[Y][X * __AnsiLineOccupyFactor + 2];
                        if (ColorB < 0) ColorB = TextNormalBack;
                        if (ColorF < 0) ColorF = TextNormalFore;
                        Screen_.PutChar(X, Y__, __AnsiLineOccupyX[Y][X * __AnsiLineOccupyFactor + 0], ColorB, ColorF, __AnsiLineOccupyX[Y][X * __AnsiLineOccupyFactor + 3], __AnsiLineOccupyX[Y][X * __AnsiLineOccupyFactor + 4]);
                    }
                }
                Bufoffset = Bufoffset + __AnsiLineOccupyX.Count;
            }
        }


        public void AnsiCharF(int X, int Y, int Ch)
        {
            AnsiCharF(X, Y, Ch, __AnsiBackWork, __AnsiForeWork);
        }

        public void AnsiCharFI(int X, int Y, int Ch, int ColB, int ColF)
        {
            if (__AnsiInsertMode)
            {
                if (__AnsiLineOccupy.Count > Y)
                {
                    if (AnsiGetFontSize(Y) > 0)
                    {
                        if (__AnsiLineOccupy[Y].Count >= (X * __AnsiLineOccupyFactor))
                        {
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 0);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 0);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, TextNormalFore);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, TextNormalBack);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 32);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 0);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 0);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, TextNormalFore);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, TextNormalBack);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 32);
                        }
                        if (__AnsiLineOccupy[Y].Count > (AnsiMaxX * __AnsiLineOccupyFactor))
                        {
                            __AnsiLineOccupy[Y].RemoveRange(AnsiMaxX * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor * 2);
                        }
                    }
                    else
                    {
                        if (__AnsiLineOccupy[Y].Count >= (X * __AnsiLineOccupyFactor))
                        {
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 0);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 0);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, TextNormalFore);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, TextNormalBack);
                            __AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 32);
                        }
                        if (__AnsiLineOccupy[Y].Count > (AnsiMaxX * __AnsiLineOccupyFactor))
                        {
                            __AnsiLineOccupy[Y].RemoveRange(AnsiMaxX * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor);
                        }
                    }
                    AnsiRepaintLine(Y);
                }
            }
            AnsiCharF(X, Y, Ch, ColB, ColF);
        }

        public void AnsiCharF(int X, int Y, int Ch, int ColB, int ColF)
        {
            int S = AnsiGetFontSize(Y);
            if (S > 0)
            {
                AnsiChar(X * 2 + 0, Y, Ch, __AnsiBackWork, __AnsiForeWork, 1, S - 1);
                AnsiChar(X * 2 + 1, Y, Ch, __AnsiBackWork, __AnsiForeWork, 2, S - 1);
            }
            else
            {
                AnsiChar(X, Y, Ch, __AnsiBackWork, __AnsiForeWork, __AnsiFontSizeW, __AnsiFontSizeH);
                if (__AnsiFontSizeW > 0)
                {
                    __AnsiFontSizeW++;
                    switch (__AnsiFontSizeW)
                    {
                        case 3:
                            __AnsiFontSizeW -= 2;
                            break;
                        case 6:
                            __AnsiFontSizeW -= 3;
                            break;
                        case 10:
                            __AnsiFontSizeW -= 4;
                            break;
                    }
                }
            }
        }

        public void AnsiChar(int X, int Y, int Ch)
        {
            AnsiChar(X, Y, Ch, __AnsiBackWork, __AnsiForeWork, 0, 0);
        }

        public void AnsiChar(int X, int Y, int Ch, int ColB, int ColF, int FontW, int FontH)
        {
            if (X < 0)
            {
                return;
            }
            if (Y < 0)
            {
                return;
            }
            if (__AnsiTest == 2)
            {
                if ((X >= 0) && (X < 79) && (Y >= 0) && (Y < 23))
                {
                    Console.SetCursorPosition(X, Y);
                    if (ColB < 0)
                    {
                        ColB = 0;
                    }
                    if (ColF < 0)
                    {
                        ColF = 7;
                    }
                    Console.BackgroundColor = (ConsoleColor)ColB;
                    Console.ForegroundColor = (ConsoleColor)ColF;
                    Console.Write((char)Ch);
                }
            }

            if (__AnsiScreen)
            {
                if (ColB < 0)
                {
                    ColB = TextNormalBack;
                }
                if (ColF < 0)
                {
                    ColF = TextNormalFore;
                }
                Screen_.PutChar(X, Y, Ch, ColB, ColF, FontW, FontH);
            }
            while (__AnsiLineOccupy.Count <= Y)
            {
                __AnsiLineOccupy.Add(new List<int>());
            }
            while ((__AnsiLineOccupy[Y].Count / __AnsiLineOccupyFactor) <= X)
            {
                __AnsiLineOccupy[Y].Add(32);
                __AnsiLineOccupy[Y].Add(-1);
                __AnsiLineOccupy[Y].Add(-1);
                __AnsiLineOccupy[Y].Add(0);
                __AnsiLineOccupy[Y].Add(0);
            }
            __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 0] = Ch;
            __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 1] = ColB;
            __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 2] = ColF;
            __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 3] = FontW;
            __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 4] = FontH;
        }

        private void AnsiCalcColor()
        {
            AnsiCalcColor(__AnsiBack, __AnsiFore);
        }

        private void AnsiCalcColor(int B, int F)
        {
            __AnsiBackWork = B;
            __AnsiForeWork = F;

            if (__AnsiBackWork < 0)
            {
                __AnsiBackWork = TextNormalBack;
            }
            if (__AnsiForeWork < 0)
            {
                __AnsiForeWork = TextNormalFore;
            }

            if (ANSIReverseMode == 0)
            {
                if (__AnsiFontInverse)
                {
                    int Temp = __AnsiForeWork;
                    __AnsiForeWork = __AnsiBackWork;
                    __AnsiBackWork = Temp;
                }
            }

            if (__AnsiFontBold && (!ANSIIgnoreBold))
            {
                if (__AnsiForeWork < 8)
                {
                    if ((__AnsiForeWork >= 0) && (__AnsiForeWork < 8))
                    {
                        __AnsiForeWork += 8;
                    }
                }
                else
                {
                    if ((__AnsiForeWork >= 8) && (__AnsiForeWork < 16))
                    {
                        __AnsiForeWork -= 8;
                    }
                }
            }

            if (__AnsiFontBlink && (!ANSIIgnoreBlink))
            {
                if (__AnsiBackWork < 8)
                {
                    if ((__AnsiBackWork >= 0) && (__AnsiBackWork < 8))
                    {
                        __AnsiBackWork += 8;
                    }
                }
                else
                {
                    if ((__AnsiBackWork >= 8) && (__AnsiBackWork < 16))
                    {
                        __AnsiBackWork -= 8;
                    }
                }
            }

            if (ANSIReverseMode == 1)
            {
                if (__AnsiFontInverse)
                {
                    int Temp = __AnsiForeWork;
                    __AnsiForeWork = __AnsiBackWork;
                    __AnsiBackWork = Temp;
                }
            }

            if (__AnsiFontInvisible && (!ANSIIgnoreConcealed))
            {
                __AnsiForeWork = __AnsiBackWork;
            }

            if ((B < 0) && (__AnsiBackWork == TextNormalBack))
            {
                __AnsiBackWork = -1;
            }
            if ((F < 0) && (__AnsiForeWork == TextNormalFore))
            {
                __AnsiForeWork = -1;
            }
        }
    }
}
