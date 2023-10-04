using System;
using System.Collections.Generic;
using System.Threading;

namespace TextPaint
{
    public partial class CoreAnsi
    {
        public int AnsiMaxX = 0;
        public int AnsiMaxY = 0;

        public bool AnsiRingBell = true;
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

        public int ANSIDOS = 0;
        public bool ANSIDOS_ = false;
        public bool ANSIPrintBackspace = false;
        public bool ANSIPrintTab = false;

        public bool ANSI8bit = false;

        // 0 - Do not change
        // 1 - Use as CRLF
        // 2 - Ommit
        public int ANSI_CR = 0;
        public int ANSI_LF = 0;

        public long __AnsiProcessDelayFactor = 0;

        bool __AnsiScreen = false;

        int[] VT52_SemigraphChars = {  0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                                       0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

        bool __AnsiLineOccupy1_Use = false;
        bool __AnsiLineOccupy2_Use = false;

        public List<int> AnsiBuffer = new List<int>();

        public void CreateColor256()
        {
            int C11 = 63 - 1;
            int C12 = 64 + 1;
            int C21 = 191 - 1;
            int C22 = 192 + 2;
            int[] Val6 = new int[] { 0, 51, 102, 153, 204, 255 };
            Color256[0] = AnsiColor16(0, 0, 0);
            Color256[1] = AnsiColor16(255, C11, C11);
            Color256[2] = AnsiColor16(C11, 255, C11);
            Color256[3] = AnsiColor16(C21, C21, 0);
            Color256[4] = AnsiColor16(C11, C11, 255);
            Color256[5] = AnsiColor16(C21, 0, C21);
            Color256[6] = AnsiColor16(0, C21, C21);
            Color256[7] = AnsiColor16(128, 128, 128);
            Color256[8] = AnsiColor16(127, 127, 127);
            Color256[9] = AnsiColor16(255, C12, C12);
            Color256[10] = AnsiColor16(C12, 255, C12);
            Color256[11] = AnsiColor16(C22, C22, 0);
            Color256[12] = AnsiColor16(C12, C12, 255);
            Color256[13] = AnsiColor16(C22, 0, C22);
            Color256[14] = AnsiColor16(0, C22, C22);
            Color256[15] = AnsiColor16(255, 255, 255);
            for (int i_R = 0; i_R < 6; i_R++)
            {
                for (int i_G = 0; i_G < 6; i_G++)
                {
                    for (int i_B = 0; i_B < 6; i_B++)
                    {
                        int i_ = i_R * 36 + i_G * 6 + i_B + 16;
                        Color256[i_] = AnsiColor16(Val6[i_R], Val6[i_G], Val6[i_B]);
                    }
                }
            }
            Color256[232 + 0] = AnsiColor16(0, 0, 0);
            Color256[232 + 1] = AnsiColor16(11, 11, 11);
            Color256[232 + 2] = AnsiColor16(22, 22, 22);
            Color256[232 + 3] = AnsiColor16(33, 33, 33);
            Color256[232 + 4] = AnsiColor16(44, 44, 44);
            Color256[232 + 5] = AnsiColor16(55, 55, 55);
            Color256[232 + 6] = AnsiColor16(67, 67, 67);
            Color256[232 + 7] = AnsiColor16(78, 78, 78);
            Color256[232 + 8] = AnsiColor16(89, 89, 89);
            Color256[232 + 9] = AnsiColor16(100, 100, 100);
            Color256[232 + 10] = AnsiColor16(111, 111, 111);
            Color256[232 + 11] = AnsiColor16(122, 122, 122);
            Color256[232 + 12] = AnsiColor16(133, 133, 133);
            Color256[232 + 13] = AnsiColor16(144, 144, 144);
            Color256[232 + 14] = AnsiColor16(155, 155, 155);
            Color256[232 + 15] = AnsiColor16(166, 166, 166);
            Color256[232 + 16] = AnsiColor16(177, 177, 177);
            Color256[232 + 17] = AnsiColor16(188, 188, 188);
            Color256[232 + 18] = AnsiColor16(200, 200, 200);
            Color256[232 + 19] = AnsiColor16(211, 211, 211);
            Color256[232 + 20] = AnsiColor16(222, 222, 222);
            Color256[232 + 21] = AnsiColor16(233, 233, 233);
            Color256[232 + 22] = AnsiColor16(244, 244, 244);
            Color256[232 + 23] = AnsiColor16(255, 255, 255);
        }

        Core Core_;

        public CoreAnsi(Core Core__, ConfigFile CF)
        {
            Core_ = Core__;
            CommandEndChar = TextWork.StrToInt(CommandEndChar_);

            __AnsiLineOccupy1_Use = CF.ParamGetB("ANSIBufferAbove");
            __AnsiLineOccupy2_Use = CF.ParamGetB("ANSIBufferBelow");

            AnsiMaxX = CF.ParamGetI("ANSIWidth");
            AnsiMaxY = CF.ParamGetI("ANSIHeight");
            ANSI_CR = CF.ParamGetI("ANSIReadCR");
            ANSI_LF = CF.ParamGetI("ANSIReadLF");
            ANSIDOS = CF.ParamGetI("ANSIDOS");
            if ((ANSIDOS < 0) || (ANSIDOS >= 3)) ANSIDOS = 0;
            ANSIDOS_ = (ANSIDOS == 1);
            ANSI8bit = CF.ParamGetB("ANSI8bit");
            ANSIPrintBackspace = CF.ParamGetB("ANSIPrintBackspace");
            ANSIPrintTab = CF.ParamGetB("ANSIPrintTab");

            AnsiTerminalResize(AnsiMaxX, AnsiMaxY);

            ANSIScrollChars = CF.ParamGetI("ANSIScrollChars");
            ANSIScrollBuffer = CF.ParamGetI("ANSIScrollBuffer");
            ANSIScrollSmooth = CF.ParamGetI("ANSIScrollSmooth");

            ColorThresholdBlackWhite = CF.ParamGetI("ANSIColorThresholdBlackWhite");
            ColorThresholdGray = CF.ParamGetI("ANSIColorThresholdGray");

            CreateColor256();

            if (Core_.WorkMode == 2)
            {
                if (CF.ParamGetL("TerminalTimeResolution") <= 0)
                {
                    ANSIScrollChars = 0;
                    ANSIScrollBuffer = 0;
                    ANSIScrollSmooth = 0;
                }
                if (CF.ParamGetI("TerminalStep") <= 0)
                {
                    ANSIScrollChars = 0;
                    ANSIScrollBuffer = 0;
                    ANSIScrollSmooth = 0;
                }
            }

            if (ANSIScrollChars <= 0)
            {
                ANSIScrollSmooth = 0;
            }
            if (ANSIScrollSmooth > 4)
            {
                ANSIScrollSmooth = 0;
            }




            // Load character maps
            string[] CharDOS = CF.ParamGetS("ANSICharsDOS").Split(',');
            string[] CharsVT52 = CF.ParamGetS("ANSICharsVT52").Split(',');
            for (int i = 0; i < 32; i++)
            {
                if (CharDOS.Length >= 32)
                {
                    int T = TextWork.CodeChar(CharDOS[i]);
                    if (T >= 32)
                    {
                        DosControl[i] = T;
                    }
                    else
                    {
                        DosControl[i] = 32;
                    }
                }
                if (CharsVT52.Length >= 32)
                {
                    int T = TextWork.CodeChar(CharsVT52[i]);
                    if (T >= 32)
                    {
                        VT52_SemigraphChars[i] = T;
                    }
                    else
                    {
                        VT52_SemigraphChars[i] = 32;
                    }
                }
            }
            AnsiState.InitCharMap(CF);
        }

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
                if (ANSIDOS_)
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

            if (Core_.CursorX >= NewW)
            {
                Core_.CursorX = NewW - 1;
            }
            if (Core_.CursorY >= NewH)
            {
                Core_.CursorY = NewH - 1;
            }

            if (SeekState.Count > 1)
            {
                //SeekState.RemoveRange(1, SeekState.Count - 1);
            }

            AnsiMaxX = NewW;
            AnsiMaxY = NewH;
            AnsiState_.TerminalW = NewW;
            AnsiState_.TerminalH = NewH;
            if (__AnsiScreen)
            {
                AnsiRepaint(false);
            }
            return true;
        }

        public AnsiState AnsiState_ = new AnsiState();

        public void AnsiTerminalReset()
        {
            AnsiState_.Reset(AnsiMaxX, AnsiMaxY, -1, -1, ANSIDOS);

            __AnsiResponse.Clear();
            if (AnsiScreenWork && ((Core_.WorkMode == 1) || (Core_.WorkMode == 2) || (Core_.WorkMode == 4)))
            {
                __AnsiScreen = true;
                Core_.Screen_.MouseActive(false);
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
                    Core_.Screen_.Bell();
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
                    for (int X = 0; X < AnsiMaxX; X++)
                    {
                        if (X < L)
                        {
                            AnsiState_.__AnsiLineOccupy__.Get(Y, X);
                            Core_.Screen_.PutChar(X, Y, AnsiState_.__AnsiLineOccupy__.Item_Char, AnsiState_.__AnsiLineOccupy__.Item_ColorB, AnsiState_.__AnsiLineOccupy__.Item_ColorF, AnsiState_.__AnsiLineOccupy__.Item_FontW, AnsiState_.__AnsiLineOccupy__.Item_FontH, AnsiState_.__AnsiLineOccupy__.Item_ColorA);
                        }
                        else
                        {
                            Core_.Screen_.PutChar(X, Y, ' ', -1, -1, 0, 0, 0);
                        }
                    }
                }
            }
        }

        public int __ScreenMinX = 0;
        public int __ScreenMinY = 0;
        public int __ScreenMaxX = 0;
        public int __ScreenMaxY = 0;
        public void AnsiRepaint(bool AdditionalBuffers)
        {
            Core_.Screen_.Clear(-1, -1);
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
                        Core_.Screen_.PutChar(X, Y__, __AnsiLineOccupyX.Item_Char, __AnsiLineOccupyX.Item_ColorB, __AnsiLineOccupyX.Item_ColorF, __AnsiLineOccupyX.Item_FontW, __AnsiLineOccupyX.Item_FontH, __AnsiLineOccupyX.Item_ColorA);
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
                    Core_.Screen_.PutChar(X, Y, AnsiState_.__AnsiLineOccupy__.Item_Char, AnsiState_.__AnsiLineOccupy__.Item_ColorB, AnsiState_.__AnsiLineOccupy__.Item_ColorF, AnsiState_.__AnsiLineOccupy__.Item_FontW, AnsiState_.__AnsiLineOccupy__.Item_FontH, AnsiState_.__AnsiLineOccupy__.Item_ColorA);
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
                AnsiState_.__AnsiFontSizeW = CoreStatic.FontCounter(AnsiState_.__AnsiFontSizeW);
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

            if (__AnsiScreen)
            {
                Core_.Screen_.PutChar(X, Y, Ch, ColB, ColF, FontW, FontH, ColA);
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
            if ((__ColB != ColB) && (__ColB >= 0) && (__ColB != Core_.TextNormalBack))
            {
                IsCharOver = true;
            }
            if (!TextWork.SpaceChars.Contains(__Ch))
            {
                if ((__Ch != Ch))
                {
                    IsCharOver = true;
                }
                if ((__ColF != ColF) && (__ColF >= 0) && (__ColF != Core_.TextNormalFore))
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
            if (Core_.WorkMode != 2)
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
                if (Core_.Screen_.WinAuto)
                {
                    if (__AnsiScreen)
                    {
                        Core_.Screen_.AppResize(NewW, NewH, false);
                        AnsiTerminalResize(Core_.Screen_.WinW, Core_.Screen_.WinH);
                    }
                    else
                    {
                        AnsiTerminalResize(NewW, NewH);
                    }
                }
            }
            if (Core_.WorkMode != 2)
            {
                Monitor.Exit(AnsiResizeMonitor);
            }
        }
    }
}
