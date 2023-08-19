using System;
using System.Collections.Generic;
using System.Threading;

namespace TextPaint
{
    public partial class Core
    {
        public int AnsiMaxX = 0;
        public int AnsiMaxY = 0;

        bool AnsiRingBell = true;
        bool AnsiScreenWork = true;

        string CommandEndChar_ = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz@`{}~|";
        List<int> CommandEndChar;

        public List<string> __AnsiResponse = new List<string>();

        // Printable character replacement for standard DOS character from 00h to 1Fh
        int[] DosControl = { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                             0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                             0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                             0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

        // Color subsitution from 256-color palette - filled in in constructor
        int[] Color256 = new int[256];

        public bool ANSIDOS = false;
        public bool ANSIPrintBackspace = false;
        public bool ANSIPrintTab = false;

        public bool ANSI8bit = false;

        // 0 - Do not change
        // 1 - Use as CRLF
        // 2 - Ommit
        public int ANSI_CR = 0;
        public int ANSI_LF = 0;

        public long __AnsiProcessDelayFactor = 0;

        public bool __AnsiTestCmd = false;
        int __AnsiTest = 0;
        bool __AnsiScreen = false;

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
                //SeekState.RemoveRange(1, SeekState.Count - 1);
            }

            AnsiMaxX = NewW;
            AnsiMaxY = NewH;
            AnsiState_.TerminalW = NewW;
            AnsiState_.TerminalH = NewH;
            return true;
        }

        public AnsiState AnsiState_ = new AnsiState();

        public void AnsiTerminalReset()
        {
            AnsiState_.Reset(AnsiMaxX, AnsiMaxY, -1, -1);

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
            if ((SeekMode > 0) && ((SeekStateSaveLast < AnsiState_.__AnsiProcessStep)))
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
                bool MustResize = false;
                if ((SeekState[SeekIdx].TerminalW != AnsiState_.TerminalW) || (SeekState[SeekIdx].TerminalH != AnsiState_.TerminalH))
                {
                    MustResize = true;
                }
                AnsiState.Copy(SeekState[SeekIdx], AnsiState_);
                if (MustResize)
                {
                    AnsiResize(AnsiState_.TerminalW, AnsiState_.TerminalH);
                }
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

                    int CharToPrint = AnsiBuffer[AnsiState_.AnsiBufferI];
                    AnsiState_.AnsiBufferI++;
                    if (AnsiState_.__AnsiCommand)
                    {
                        if (CharToPrint < 32)
                        {
                            if (CharToPrint == 27)
                            {
                                AnsiState_.__AnsiCmd.Clear();
                            }
                            else
                            {
                                if (AnsiCharNotCmd(CharToPrint))
                                {
                                    AnsiCharPrint(CharToPrint);
                                }
                            }
                        }
                        else
                        {
                            AnsiState_.__AnsiCmd.Add(CharToPrint);
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
                                int ResoX = AnsiMaxX;
                                int ResoY = AnsiMaxY;
                                AnsiProcess_Fixed(CharToPrint);

                                if (AnsiState_.__AnsiCommand && (AnsiState_.__AnsiCmd[0] != ']') && (AnsiState_.__AnsiCmd.Count >= 2) && (CommandEndChar.Contains(CharToPrint)))
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
                                if ((ResoX != AnsiMaxX) || (ResoY != AnsiMaxY))
                                {
                                    SeekStateSaveLast = AnsiState_.__AnsiProcessStep - 1;
                                    SeekStateSaveRequest = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (CharToPrint == 27)
                        {
                            AnsiState_.__AnsiCmd.Clear();
                            AnsiState_.__AnsiCommand = true;
                        }
                        else
                        {
                            if (AnsiCharNotCmd(CharToPrint))
                            {
                                if (AnsiState_.__AnsiDCS_)
                                {
                                    AnsiState_.__AnsiDCS = AnsiState_.__AnsiDCS + TextWork.IntToStr(CharToPrint);
                                }
                                else
                                {
                                    AnsiCharPrint(CharToPrint);
                                }
                            }
                        }
                    }
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

        public void AnsiGetF(int X, int Y, out int Ch, out int ColB, out int ColF, out int FontAttrib)
        {
            int FontW;
            int FontH;
            if (AnsiGetFontSize(Y) > 0)
            {
                AnsiGet(X * 2, Y, out Ch, out ColB, out ColF, out FontW, out FontH, out FontAttrib);
            }
            else
            {
                AnsiGet(X, Y, out Ch, out ColB, out ColF, out FontW, out FontH, out FontAttrib);
            }
        }

        public void AnsiGet(int X, int Y, out int Ch, out int ColB, out int ColF, out int FontW, out int FontH, out int FontAttrib)
        {
            Ch = 32;
            ColB = -1;
            ColF = -1;
            FontW = 0;
            FontH = 0;
            FontAttrib = 0;
            if (AnsiState_.__AnsiLineOccupy__.CountLines() > Y)
            {
                if ((AnsiState_.__AnsiLineOccupy__.CountItems(Y)) > X)
                {
                    AnsiState_.__AnsiLineOccupy__.Get(Y, X);
                    Ch = AnsiState_.__AnsiLineOccupy__.Item_Char;
                    ColB = AnsiState_.__AnsiLineOccupy__.Item_ColorB;
                    ColF = AnsiState_.__AnsiLineOccupy__.Item_ColorF;
                    FontW = AnsiState_.__AnsiLineOccupy__.Item_FontW;
                    FontH = AnsiState_.__AnsiLineOccupy__.Item_FontH;
                    FontAttrib = AnsiState_.__AnsiLineOccupy__.Item_ColorA;
                }
            }
        }


        public void AnsiRepaintLine(int Y)
        {
            if (__AnsiScreen)
            {
                if (Y < AnsiState_.__AnsiLineOccupy__.CountLines())
                {
                    int L = (AnsiState_.__AnsiLineOccupy__.CountItems(Y));
                    for (int X = 0; X < WinW; X++)
                    {
                        if (X < L)
                        {
                            AnsiState_.__AnsiLineOccupy__.Get(Y, X);
                            Screen_.PutChar(X, Y, AnsiState_.__AnsiLineOccupy__.Item_Char, AnsiState_.__AnsiLineOccupy__.Item_ColorB, AnsiState_.__AnsiLineOccupy__.Item_ColorF, AnsiState_.__AnsiLineOccupy__.Item_FontW, AnsiState_.__AnsiLineOccupy__.Item_FontH, AnsiState_.__AnsiLineOccupy__.Item_ColorA);
                        }
                        else
                        {
                            Screen_.PutChar(X, Y, ' ', -1, -1, 0, 0, 0);
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
            Screen_.Clear(-1, -1);
            __ScreenMinX = 0;
            __ScreenMinY = 0;
            __ScreenMaxX = 0;
            __ScreenMaxY = 0;
            int BufMin = AdditionalBuffers ? 0 : 1;
            int BufMax = AdditionalBuffers ? 2 : 1;
            int Bufoffset = 0;
            for (int BufI = BufMin; BufI <= BufMax; BufI++)
            {
                AnsiLineOccupy __AnsiLineOccupyX;
                if (BufI == 1)
                {
                    __AnsiLineOccupyX = AnsiState_.__AnsiLineOccupy__;
                }
                else
                {
                    if (BufI == 0)
                    {
                        __AnsiLineOccupyX = AnsiState_.__AnsiLineOccupy1__;
                    }
                    else
                    {
                        __AnsiLineOccupyX = AnsiState_.__AnsiLineOccupy2__;
                    }
                }
                for (int Y = 0; Y < __AnsiLineOccupyX.CountLines(); Y++)
                {
                    int Y__ = Y + Bufoffset;
                    for (int X = 0; X < (__AnsiLineOccupyX.CountItems(Y)); X++)
                    {
                        if (__ScreenMinX > X) { __ScreenMinX = X; }
                        if (__ScreenMinY > Y__) { __ScreenMinY = Y__; }
                        if (__ScreenMaxX < X) { __ScreenMaxX = X; }
                        if (__ScreenMaxY < Y__) { __ScreenMaxY = Y__; }

                        __AnsiLineOccupyX.Get(Y, X);
                        Screen_.PutChar(X, Y__, __AnsiLineOccupyX.Item_Char, __AnsiLineOccupyX.Item_ColorB, __AnsiLineOccupyX.Item_ColorF, __AnsiLineOccupyX.Item_FontW, __AnsiLineOccupyX.Item_FontH, __AnsiLineOccupyX.Item_ColorA);
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
                Bufoffset = Bufoffset + __AnsiLineOccupyX.CountLines();
            }
        }

        public void AnsiScreenNegative(bool IsNega)
        {
            if (IsNega)
            {
                AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr | 0x80;
                AnsiState_.__AnsiAttr_ = AnsiState_.__AnsiAttr_ | 0x80;
            }
            else
            {
                AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr & 0x7F;
                AnsiState_.__AnsiAttr_ = AnsiState_.__AnsiAttr_ & 0x7F;
            }
            while (AnsiState_.__AnsiLineOccupy__.CountLines() < AnsiMaxY)
            {
                AnsiState_.__AnsiLineOccupy__.AppendLine();
            }
            for (int Y = 0; Y < AnsiMaxY; Y++)
            {
                while (AnsiState_.__AnsiLineOccupy__.CountItems(Y) < AnsiMaxX)
                {
                    AnsiState_.__AnsiLineOccupy__.BlankChar();
                    AnsiState_.__AnsiLineOccupy__.Append(Y);
                }
                for (int X = 0; X < AnsiMaxX; X++)
                {
                    AnsiState_.__AnsiLineOccupy__.Get(Y, X);
                    if (IsNega)
                    {
                        AnsiState_.__AnsiLineOccupy__.Item_ColorA = AnsiState_.__AnsiLineOccupy__.Item_ColorA | 0x80;
                    }
                    else
                    {
                        AnsiState_.__AnsiLineOccupy__.Item_ColorA = AnsiState_.__AnsiLineOccupy__.Item_ColorA & 0x7F;
                    }
                    AnsiState_.__AnsiLineOccupy__.Set(Y, X);
                    Screen_.PutChar(X, Y, AnsiState_.__AnsiLineOccupy__.Item_Char, AnsiState_.__AnsiLineOccupy__.Item_ColorB, AnsiState_.__AnsiLineOccupy__.Item_ColorF, AnsiState_.__AnsiLineOccupy__.Item_FontW, AnsiState_.__AnsiLineOccupy__.Item_FontH, AnsiState_.__AnsiLineOccupy__.Item_ColorA);
                }
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
            AnsiCharF(X, Y, Ch, AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);
        }

        public void AnsiCharFI(int X, int Y, int Ch, int ColB, int ColF, int FonA)
        {
            if (AnsiState_.__AnsiInsertMode)
            {
                if (AnsiState_.__AnsiLineOccupy__.CountLines() > Y)
                {
                    if (AnsiGetFontSize(Y) > 0)
                    {
                        if (AnsiState_.__AnsiLineOccupy__.CountItems(Y) >= X)
                        {
                            AnsiState_.__AnsiLineOccupy__.BlankChar();
                            AnsiState_.__AnsiLineOccupy__.Insert(Y, X);
                            AnsiState_.__AnsiLineOccupy__.Insert(Y, X);
                        }
                        if (AnsiState_.__AnsiLineOccupy__.CountItems(Y) > AnsiMaxX)
                        {
                            AnsiState_.__AnsiLineOccupy__.Delete(Y, AnsiMaxX);
                            AnsiState_.__AnsiLineOccupy__.Delete(Y, AnsiMaxX);
                        }
                    }
                    else
                    {
                        if (AnsiState_.__AnsiLineOccupy__.CountItems(Y) >= X)
                        {
                            AnsiState_.__AnsiLineOccupy__.BlankChar();
                            AnsiState_.__AnsiLineOccupy__.Insert(Y, X);
                        }
                        if (AnsiState_.__AnsiLineOccupy__.CountItems(Y) > AnsiMaxX)
                        {
                            AnsiState_.__AnsiLineOccupy__.Delete(Y, AnsiMaxX);
                        }
                    }
                    AnsiState_.PrintCharInsDel++;
                    AnsiRepaintLine(Y);
                }
            }
            AnsiCharF(X, Y, Ch, ColB, ColF, FonA);
        }

        public void AnsiCharF(int X, int Y, int Ch, int ColB, int ColF, int FonA)
        {
            int S = AnsiGetFontSize(Y);
            if (S > 0)
            {
                AnsiChar(X * 2 + 0, Y, Ch, AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, 1, S - 1, AnsiState_.__AnsiAttr);
                AnsiChar(X * 2 + 1, Y, Ch, AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, 2, S - 1, AnsiState_.__AnsiAttr);
            }
            else
            {
                AnsiChar(X, Y, Ch, AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiFontSizeW, AnsiState_.__AnsiFontSizeH, AnsiState_.__AnsiAttr);
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
            AnsiChar(X, Y, Ch, AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, 0, 0, AnsiState_.__AnsiAttr);
        }

        public void AnsiChar(int X, int Y, int Ch, int ColB, int ColF, int FontW, int FontH, int ColA)
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
                Screen_.PutChar(X, Y, Ch, ColB, ColF, FontW, FontH, ColA);
            }
            while (AnsiState_.__AnsiLineOccupy__.CountLines() <= Y)
            {
                AnsiState_.__AnsiLineOccupy__.AppendLine();
            }
            while ((AnsiState_.__AnsiLineOccupy__.CountItems(Y)) <= X)
            {
                AnsiState_.__AnsiLineOccupy__.BlankChar();
                AnsiState_.__AnsiLineOccupy__.Append(Y);
            }
            AnsiState_.__AnsiLineOccupy__.Get(Y, X);

            int __Ch = AnsiState_.__AnsiLineOccupy__.Item_Char;
            int __ColB = AnsiState_.__AnsiLineOccupy__.Item_ColorB;
            int __ColF = AnsiState_.__AnsiLineOccupy__.Item_ColorF;
            int __ColA = AnsiState_.__AnsiLineOccupy__.Item_ColorA & 127;

            AnsiState_.__AnsiLineOccupy__.Item_Char = Ch;
            AnsiState_.__AnsiLineOccupy__.Item_ColorB = ColB;
            AnsiState_.__AnsiLineOccupy__.Item_ColorF = ColF;
            AnsiState_.__AnsiLineOccupy__.Item_ColorA = ColA;
            AnsiState_.__AnsiLineOccupy__.Item_FontW = FontW;
            AnsiState_.__AnsiLineOccupy__.Item_FontH = FontH;
            AnsiState_.__AnsiLineOccupy__.Set(Y, X);
            ColA = ColA & 127;
            bool IsCharOver = false;
            if ((__ColB != ColB) && (__ColB >= 0) && (__ColB != TextNormalBack))
            {
                IsCharOver = true;
            }
            if (!TextWork.SpaceChars.Contains(__Ch))
            {
                if ((__Ch != Ch))
                {
                    IsCharOver = true;
                }
                if ((__ColF != ColF) && (__ColF >= 0) && (__ColF != TextNormalFore))
                {
                    IsCharOver = true;
                }
                if ((__ColA != ColA) && (__ColA > 0))
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

        object AnsiResizeMonitor = new object();

        public void AnsiResize(int NewW, int NewH)
        {
            if (WorkMode != 2)
            {
                Monitor.Enter(AnsiResizeMonitor);
            }
            if (NewW < 0)
            {
                NewW = AnsiMaxX;
            }
            if (NewH < 0)
            {
                NewH = AnsiMaxY;
            }
            if ((NewW != AnsiMaxX) || (NewH != AnsiMaxY))
            {
                if (Screen_.WinAuto)
                {
                    if (__AnsiScreen)
                    {
                        Screen_.AppResize(NewW, NewH);
                        AnsiTerminalResize(Screen_.WinW, Screen_.WinH);
                    }
                    else
                    {
                        AnsiTerminalResize(NewW, NewH);
                    }
                }
            }
            if (WorkMode != 2)
            {
                Monitor.Exit(AnsiResizeMonitor);
            }
        }
    }
}
