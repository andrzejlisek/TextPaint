using System;
using System.Collections.Generic;

namespace TextPaint
{
    public partial class Core
    {
        public int AnsiMaxX = 0;
        public int AnsiMaxY = 0;

        public int ANSIReverseMode = 0;

        bool AnsiRingBell = true;
        bool AnsiScreenWork = true;

        string CommandEndChar_ = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz@`{}~";
        List<int> CommandEndChar;

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
        bool ANSIPrintTab = false;
        bool ANSIIgnoreBlink = false;
        bool ANSIIgnoreBold = false;
        bool ANSIIgnoreConcealed = false;

        // 0 - Do not change
        // 1 - Use as CRLF
        // 2 - Ommit
        public int ANSI_CR = 0;
        public int ANSI_LF = 0;

        public long __AnsiProcessDelayFactor = 0;

        public bool __AnsiTestCmd = false;
        int __AnsiTest = 0;
        bool __AnsiScreen = false;

        int __AnsiLineOccupyFactor = 5;

        bool __AnsiLineOccupy1_Use = false;
        bool __AnsiLineOccupy2_Use = false;


        int[] VT100_SemigraphChars = { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

        int[] VT52_SemigraphChars = {  0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };


        public List<int> AnsiBuffer = new List<int>();


        public int ReportCursorX()
        {
            return AnsiState_.__AnsiX + 1;
        }

        public int ReportCursorY()
        {
            if (AnsiState_.__AnsiOrigin)
            {
                return AnsiState_.__AnsiY - AnsiState_.__AnsiScrollFirst + 1;
            }
            else
            {
                return AnsiState_.__AnsiY + 1;
            }
        }

        public bool AnsiTerminalResize(int NewW, int NewH)
        {
            if (NewW <= 0)
            {
                NewW = 80;
            }
            if (NewH <= 0)
            {
                if (ANSIDOS)
                {
                    NewH = 25;
                }
                else
                {
                    NewH = 24;
                }
            }

            if ((AnsiMaxX == NewW) && (AnsiMaxY == NewH))
            {
                return false;
            }

            if ((AnsiState_.__AnsiScrollFirst == 0) && (AnsiState_.__AnsiScrollLast == (AnsiMaxY - 1)))
            {
                AnsiState_.__AnsiScrollLast = (NewH - 1);
            }
            if ((AnsiState_.__AnsiMarginLeft == 0) && (AnsiState_.__AnsiMarginRight == (AnsiMaxX - 1)))
            {
                AnsiState_.__AnsiMarginRight = (NewW - 1);
            }

            if (CursorX >= NewW)
            {
                CursorX = NewW - 1;
            }
            if (CursorY >= NewH)
            {
                CursorY = NewH - 1;
            }

            if (SeekState.Count > 1)
            {
                SeekState.RemoveRange(1, SeekState.Count - 1);
            }

            AnsiMaxX = NewW;
            AnsiMaxY = NewH;
            return true;
        }

        public AnsiState AnsiState_ = new AnsiState();

        public void AnsiTerminalReset()
        {
            AnsiState_.Reset(AnsiMaxX, AnsiMaxY);

            __AnsiResponse.Clear();
            if (AnsiScreenWork && ((WorkMode == 1) || (WorkMode == 2) || (WorkMode == 4)))
            {
                __AnsiScreen = true;
            }
            else
            {
                __AnsiScreen = false;
            }

            if (__AnsiScreen)
            {
                AnsiRepaint(false);
            }
        }


        // 0 - No seek
        // 1 - Clear and save state
        // 2 - Save state without clear
        int SeekMode = 0;

        int SeekPeriod = 1023;
        int SeekPeriod0 = 2048;

        List<AnsiState> SeekState = new List<AnsiState>();

        long SeekStateSaveLast = -1;

        private bool SeekStateSaveRequest = false;

        private void SeekStateSave(bool Instant)
        {
            if ((SeekMode > 0) && (SeekStateSaveLast < AnsiState_.__AnsiProcessStep))
            {
                if (Instant)
                {
                    if (SeekStateSaveRequest || ((AnsiState_.__AnsiProcessStep & SeekPeriod) == 0))
                    {
                        {
                            SeekState.Add(AnsiState_.Clone());
                            SeekStateSaveLast = AnsiState_.__AnsiProcessStep + SeekPeriod;
                        }
                        SeekStateSaveRequest = false;
                    }
                }
                else
                {
                    if (((AnsiState_.__AnsiProcessStep & SeekPeriod) == 0))
                    {
                        SeekStateSaveRequest = true;
                    }
                }
            }
        }

        public void AnsiProcessReset(bool __AnsiUseEOF_, bool AnsiScreenWork_, int SeekMode_)
        {
            if (SeekMode_ <= 1)
            {
                SeekStateSaveLast = -1;
                SeekState.Clear();
            }
            SeekMode = SeekMode_;
            AnsiRingBell = AnsiScreenWork_;
            AnsiScreenWork = AnsiScreenWork_;
            AnsiBuffer.Clear();
            AnsiState_.Zero(__AnsiUseEOF_);
            AnsiTerminalReset();
            SeekStateSaveRequest = true;
            SeekStateSave(true);
        }

        public void AnsiProcessSupply(List<int> TextFileLine)
        {
            if (TextFileLine.Count > 0)
            {
                AnsiBuffer.AddRange(TextFileLine);
            }
        }

        public bool AnsiSeek(int StepCount)
        {
            bool ForceSeek = false;

            bool NeedRepaint = false;
            bool AnsiRingBell_ = AnsiRingBell;
            AnsiRingBell = false;
            int RingBellCount = AnsiState_.AnsiRingBellCount;
            long NewProcessStep = AnsiState_.__AnsiProcessStep + StepCount;
            if (ForceSeek || (StepCount < 0) || (StepCount >= SeekPeriod0))
            {
                if (!ForceSeek)
                {
                    NeedRepaint = true;
                }
                int SeekIdx = SeekState.Count - 1;
                while (SeekState[SeekIdx].__AnsiProcessStep > NewProcessStep)
                {
                    SeekIdx--;
                }
                AnsiState.Copy(SeekState[SeekIdx], AnsiState_);
                AnsiRepaint(false);
            }
            AnsiProcess((int)(NewProcessStep - AnsiState_.__AnsiProcessStep));

            AnsiRingBell = AnsiRingBell_;
            if (AnsiRingBell)
            {
                if (RingBellCount != AnsiState_.AnsiRingBellCount)
                {
                    Screen_.Bell();
                }
            }

            return NeedRepaint;
        }

        public int AnsiProcess(int ProcessCount)
        {
            int Processed = 0;
            if (ProcessCount < 0)
            {
                ProcessCount = int.MaxValue;
            }
            if ((ProcessCount == 0) || (AnsiState_.__AnsiBeyondEOF))
            {
                return 0;
            }
            bool StdProc = true;
            bool ProcAdditionalChars = false;
            bool SeekStateSaveInstant = false;
            while (ProcessCount > 0)
            {
                AnsiState_.__AnsiProcessStep++;

                if (AnsiState_.__AnsiBeyondEOF)
                {
                    break;
                }
                StdProc = true;
                if (AnsiCharPrintRepeater > 0)
                {
                    Processed++;
                    StdProc = false;
                    AnsiCharPrint(AnsiCharPrintLast);
                    AnsiCharPrintRepeater--;
                    if (AnsiCharPrintRepeater == 0)
                    {
                        AnsiCharPrintLast = -1;
                    }
                    ProcAdditionalChars = true;
                }
                if (AnsiState_.__AnsiProcessStep <= AnsiState_.__AnsiProcessDelay)
                {
                    Processed++;
                    StdProc = false;
                }
                if (AnsiState_.AnsiScrollCounter > 0)
                {
                    Processed++;
                    StdProc = false;
                    if (AnsiScrollProcess())
                    {
                    }
                    ProcAdditionalChars = true;
                }
                SeekStateSaveInstant = StdProc;
                if (StdProc)
                {
                    if (AnsiState_.AnsiBufferI >= AnsiBuffer.Count)
                    {
                        AnsiState_.__AnsiAdditionalChars = 0;
                        return Processed;
                    }
                    Processed++;


                    if (AnsiState_.__AnsiCommand)
                    {
                        if (AnsiBuffer[AnsiState_.AnsiBufferI] < 32)
                        {
                            if (AnsiBuffer[AnsiState_.AnsiBufferI] == 27)
                            {
                                AnsiState_.__AnsiCmd.Clear();
                            }
                            else
                            {
                                AnsiCharPrint(AnsiBuffer[AnsiState_.AnsiBufferI]);
                            }
                        }
                        else
                        {
                            AnsiState_.__AnsiCmd.Add(AnsiBuffer[AnsiState_.AnsiBufferI]);
                            if (__AnsiTestCmd)
                            {
                                if (AnsiState_.__AnsiCmd.Count > 20)
                                {
                                    Console.WriteLine("ANSI long command " + TextWork.IntToStr(AnsiState_.__AnsiCmd));
                                }
                            }
                        }

                        if (AnsiState_.__AnsiCmd.Count > 0)
                        {
                            if (AnsiState_.__AnsiVT52)
                            {
                                AnsiProcess_VT52();
                            }
                            else
                            {
                                AnsiProcess_Fixed(AnsiBuffer[AnsiState_.AnsiBufferI]);

                                if (AnsiState_.__AnsiCommand && (AnsiState_.__AnsiCmd[0] != ']') && (AnsiState_.__AnsiCmd.Count >= 2) && (CommandEndChar.Contains(AnsiBuffer[AnsiState_.AnsiBufferI])))
                                {
                                    AnsiState_.__AnsiCommand = false;
                                    string AnsiCmd_ = TextWork.IntToStr(AnsiState_.__AnsiCmd);
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
                        if (AnsiBuffer[AnsiState_.AnsiBufferI] == 27)
                        {
                            AnsiState_.__AnsiCmd.Clear();
                            AnsiState_.__AnsiCommand = true;
                        }
                        else
                        {
                            if (AnsiState_.__AnsiDCS_)
                            {
                                AnsiState_.__AnsiDCS = AnsiState_.__AnsiDCS + TextWork.IntToStr(AnsiBuffer[AnsiState_.AnsiBufferI]);
                            }
                            else
                            {
                                AnsiCharPrint(AnsiBuffer[AnsiState_.AnsiBufferI]);
                            }
                        }
                    }
                    AnsiState_.AnsiBufferI++;
                    if (AnsiState_.__AnsiAdditionalChars == 0)
                    {
                        ProcessCount--;
                        AnsiState_.__AnsiCounter++;
                    }
                    else
                    {
                        AnsiState_.__AnsiAdditionalChars--;
                        Processed--;
                        AnsiState_.__AnsiProcessStep--;
                        SeekStateSaveInstant = false;
                    }
                }
                else
                {
                    if (ProcAdditionalChars && (AnsiState_.__AnsiAdditionalChars < ANSIScrollBuffer))
                    {
                        AnsiState_.__AnsiAdditionalChars++;
                    }
                    ProcessCount--;
                    AnsiState_.__AnsiCounter++;
                }
                SeekStateSave(SeekStateSaveInstant);
            }
            if (Processed == 0)
            {
                if (!StdProc)
                {
                    Processed = -1;
                }
            }
            return Processed;
        }


        public void AnsiEnd()
        {
            if ((__AnsiTestCmd) && AnsiState_.__AnsiCommand)
            {
                Console.WriteLine("ANSI End of file inside command " + TextWork.IntToStr(AnsiState_.__AnsiCmd));
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
            if (AnsiState_.__AnsiLineOccupy.Count > Y)
            {
                if ((AnsiState_.__AnsiLineOccupy[Y].Count / __AnsiLineOccupyFactor) > X)
                {
                    Ch = AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 0];
                    ColB = AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 1];
                    ColF = AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 2];
                    FontW = AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 3];
                    FontH = AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 4];
                }
            }
        }


        public void AnsiRepaintLine(int Y)
        {
            if (__AnsiScreen)
            {
                if (Y < AnsiState_.__AnsiLineOccupy.Count)
                {
                    int L = (AnsiState_.__AnsiLineOccupy[Y].Count / __AnsiLineOccupyFactor);
                    for (int X = 0; X < WinW; X++)
                    {
                        if (X < L)
                        {
                            int ColorB = AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 1];
                            int ColorF = AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 2];
                            if (ColorB < 0)
                            {
                                ColorB = TextNormalBack;
                            }
                            if (ColorF < 0)
                            {
                                ColorF = TextNormalFore;
                            }
                            Screen_.PutChar(X, Y, AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 0], ColorB, ColorF, AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 3], AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 4]);
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
                    __AnsiLineOccupyX = AnsiState_.__AnsiLineOccupy;
                }
                else
                {
                    if (BufI == 0)
                    {
                        __AnsiLineOccupyX = AnsiState_.__AnsiLineOccupy1;
                    }
                    else
                    {
                        __AnsiLineOccupyX = AnsiState_.__AnsiLineOccupy2;
                    }
                }
                for (int Y = 0; Y < __AnsiLineOccupyX.Count; Y++)
                {
                    int Y__ = Y + Bufoffset;
                    for (int X = 0; X < (__AnsiLineOccupyX[Y].Count / __AnsiLineOccupyFactor); X++)
                    {
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
                if (BufI == 1)
                {
                    AnsiState_.__AnsiScrollFirst += Bufoffset;
                    AnsiState_.__AnsiScrollLast += Bufoffset;
                    AnsiScrollSetOffset(AnsiState_.ScrollLastOffset);
                    AnsiState_.__AnsiScrollFirst -= Bufoffset;
                    AnsiState_.__AnsiScrollLast -= Bufoffset;
                }
                Bufoffset = Bufoffset + __AnsiLineOccupyX.Count;
            }
        }

        public void AnsiCharFUnprotected1(int X, int Y, int Ch)
        {
            int S = AnsiGetFontSize(Y);
            int T = 1;
            if (S > 0)
            {
                T = 2;
            }

            if (!AnsiState_.CharProtection1Get(X * T, Y))
            {
                AnsiCharF(X, Y, Ch);
            }
        }

        public void AnsiCharFUnprotected2(int X, int Y, int Ch)
        {
            int S = AnsiGetFontSize(Y);
            int T = 1;
            if (S > 0)
            {
                T = 2;
            }

            if (!AnsiState_.CharProtection2Get(X * T, Y))
            {
                AnsiCharF(X, Y, Ch);
            }
        }

        public void AnsiCharF(int X, int Y, int Ch)
        {
            AnsiCharF(X, Y, Ch, AnsiState_.__AnsiBackWork, AnsiState_.__AnsiForeWork);
        }

        public void AnsiCharFI(int X, int Y, int Ch, int ColB, int ColF)
        {
            if (AnsiState_.__AnsiInsertMode)
            {
                if (AnsiState_.__AnsiLineOccupy.Count > Y)
                {
                    if (AnsiGetFontSize(Y) > 0)
                    {
                        if (AnsiState_.__AnsiLineOccupy[Y].Count >= (X * __AnsiLineOccupyFactor))
                        {
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 0);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 0);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, TextNormalFore);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, TextNormalBack);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 32);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 0);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 0);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, TextNormalFore);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, TextNormalBack);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 32);
                        }
                        if (AnsiState_.__AnsiLineOccupy[Y].Count > (AnsiMaxX * __AnsiLineOccupyFactor))
                        {
                            AnsiState_.__AnsiLineOccupy[Y].RemoveRange(AnsiMaxX * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor * 2);
                        }
                    }
                    else
                    {
                        if (AnsiState_.__AnsiLineOccupy[Y].Count >= (X * __AnsiLineOccupyFactor))
                        {
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 0);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 0);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, TextNormalFore);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, TextNormalBack);
                            AnsiState_.__AnsiLineOccupy[Y].Insert(X * __AnsiLineOccupyFactor, 32);
                        }
                        if (AnsiState_.__AnsiLineOccupy[Y].Count > (AnsiMaxX * __AnsiLineOccupyFactor))
                        {
                            AnsiState_.__AnsiLineOccupy[Y].RemoveRange(AnsiMaxX * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor);
                        }
                    }
                    AnsiState_.PrintCharInsDel++;
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
                AnsiChar(X * 2 + 0, Y, Ch, AnsiState_.__AnsiBackWork, AnsiState_.__AnsiForeWork, 1, S - 1);
                AnsiChar(X * 2 + 1, Y, Ch, AnsiState_.__AnsiBackWork, AnsiState_.__AnsiForeWork, 2, S - 1);
            }
            else
            {
                AnsiChar(X, Y, Ch, AnsiState_.__AnsiBackWork, AnsiState_.__AnsiForeWork, AnsiState_.__AnsiFontSizeW, AnsiState_.__AnsiFontSizeH);
                AnsiState_.__AnsiFontSizeW = FontCounter(AnsiState_.__AnsiFontSizeW);
            }
        }

        public void AnsiCharUnprotected1(int X, int Y, int Ch)
        {
            if (!AnsiState_.CharProtection1Get(X, Y))
            {
                AnsiChar(X, Y, Ch);
            }
        }

        public void AnsiCharUnprotected2(int X, int Y, int Ch)
        {
            if (!AnsiState_.CharProtection2Get(X, Y))
            {
                AnsiChar(X, Y, Ch);
            }
        }

        public void AnsiChar(int X, int Y, int Ch)
        {
            AnsiChar(X, Y, Ch, AnsiState_.__AnsiBackWork, AnsiState_.__AnsiForeWork, 0, 0);
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
            while (AnsiState_.__AnsiLineOccupy.Count <= Y)
            {
                AnsiState_.__AnsiLineOccupy.Add(new List<int>());
            }
            while ((AnsiState_.__AnsiLineOccupy[Y].Count / __AnsiLineOccupyFactor) <= X)
            {
                AnsiState_.__AnsiLineOccupy[Y].Add(32);
                AnsiState_.__AnsiLineOccupy[Y].Add(-1);
                AnsiState_.__AnsiLineOccupy[Y].Add(-1);
                AnsiState_.__AnsiLineOccupy[Y].Add(0);
                AnsiState_.__AnsiLineOccupy[Y].Add(0);
            }
            int __Ch = AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 0];
            int __ColB = AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 1];
            int __ColF = AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 2];
            AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 0] = Ch;
            AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 1] = ColB;
            AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 2] = ColF;
            AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 3] = FontW;
            AnsiState_.__AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 4] = FontH;
            bool IsCharOver = false;
            if ((__ColB >= 0) && (__ColB != TextNormalBack) && (__ColB != ColB))
            {
                IsCharOver = true;
            }
            if (!TextWork.SpaceChars.Contains(__Ch))
            {
                if ((__ColF >= 0) && (__ColF != TextNormalFore) && (__ColF != ColF))
                {
                    IsCharOver = true;
                }
                if ((__Ch != Ch))
                {
                    IsCharOver = true;
                }
            }
            if (IsCharOver)
            {
                AnsiState_.PrintCharCounterOver++;
            }
            AnsiState_.PrintCharCounter++;
            AnsiState_.CharProtection1Set(X, Y, AnsiState_.CharProtection1Print);
            AnsiState_.CharProtection2Set(X, Y, AnsiState_.CharProtection2Print);
        }
    }
}
