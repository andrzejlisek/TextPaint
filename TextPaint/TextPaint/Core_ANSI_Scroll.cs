﻿using System;
using System.Collections.Generic;

namespace TextPaint
{
    public partial class Core
    {
        bool __AnsiSmoothScroll = false;

        int ANSIScrollDist1 = 0;
        int ANSIScrollDist2 = 0;
        bool AnsiScrollRev = false;
        int AnsiScrollLinesI = 0;

        enum AnsiScrollCommandDef { None, Char, FirstLast };
        int AnsiScrollCounter = 0;
        AnsiScrollCommandDef AnsiScrollCommand = AnsiScrollCommandDef.None;
        int AnsiScrollParam1 = 0;
        int AnsiScrollParam2 = 0;
        int AnsiScrollParam3 = 0;

        void AnsiScrollReset()
        {
            __AnsiSmoothScroll = false;
            AnsiScrollRev = false;
            AnsiScrollCounter = 0;
            AnsiScrollCommand = AnsiScrollCommandDef.None;
            AnsiScrollParam1 = 0;
            AnsiScrollParam2 = 0;
            AnsiScrollParam3 = 0;
            AnsiScrollLinesI = 0;
        }

        void AnsiScrollInit(int Lines, AnsiScrollCommandDef Command)
        {
            AnsiScrollInit(Lines, Command, 0, 0, 0);
        }

        void AnsiScrollInit(int Lines, AnsiScrollCommandDef Command, int Param1, int Param2, int Param3)
        {
            if (Lines == 0)
            {
                return;
            }
            AnsiScrollCommand = Command;
            AnsiScrollParam1 = Param1;
            AnsiScrollParam2 = Param2;
            AnsiScrollParam3 = Param3;
            AnsiScrollRev = (Lines < 0);
            AnsiScrollLinesI = (Lines >= 0) ? Lines : (0 - Lines);

            if (__AnsiSmoothScroll)
            {
                AnsiScrollCounter = ANSIScrollDist1 + ANSIScrollDist2 + 1;
                AnsiScrollProcess();
            }
            else
            {
                AnsiScrollCounter = 0;
                AnsiScrollLines(AnsiScrollRev ? 0 - AnsiScrollLinesI : AnsiScrollLinesI);
                switch (AnsiScrollCommand)
                {
                    case AnsiScrollCommandDef.Char:
                        AnsiChar(__AnsiX, __AnsiY, AnsiScrollParam1, AnsiScrollParam2, AnsiScrollParam3, 0, 0);
                        __AnsiX++;
                        break;
                    case AnsiScrollCommandDef.FirstLast:
                        __AnsiScrollFirst = AnsiScrollParam1;
                        __AnsiScrollLast = AnsiScrollParam2;
                        break;
                }
                AnsiScrollCommand = AnsiScrollCommandDef.None;
            }
        }

        bool AnsiScrollProcess()
        {
            bool ScrollDisp = false;
            if (AnsiScrollCounter == (ANSIScrollDist2) + 1)
            {
                AnsiScrollLines(AnsiScrollRev ? -1 : 1);
                ScrollDisp = true;
            }
            if (AnsiScrollCounter == 1)
            {
                AnsiScrollLinesI--;
                if (AnsiScrollLinesI > 0)
                {
                    AnsiScrollCounter = ANSIScrollDist1 + ANSIScrollDist2 + 1;
                }
                else
                {
                    switch (AnsiScrollCommand)
                    {
                        case AnsiScrollCommandDef.Char:
                            //AnsiCharF(__AnsiX, __AnsiY, AnsiScrollParam1, AnsiScrollParam2, AnsiScrollParam3, AnsiGetFontW(__AnsiY, __AnsiX), AnsiGetFontH(__AnsiY));
                            AnsiCharFI(__AnsiX, __AnsiY, AnsiScrollParam1, AnsiScrollParam2, AnsiScrollParam3);
                            __AnsiX++;
                            ScrollDisp = true;
                            break;
                    }
                    AnsiScrollCommand = AnsiScrollCommandDef.None;
                }
            }
            AnsiScrollCounter--;
            return ScrollDisp;
        }

        public void AnsiScrollLines(int Lines)
        {
            AnsiCalcColor();
            while (Lines < 0)
            {
                if (__AnsiLineOccupy2_Use)
                {
                    if (__AnsiScrollLast == (AnsiMaxY - 1))
                    {
                        while (__AnsiLineOccupy.Count <= (AnsiMaxY - 1))
                        {
                            __AnsiLineOccupy.Add(new List<int>());
                        }
                        __AnsiLineOccupy2.Add(__AnsiLineOccupy[AnsiMaxY - 1]);
                    }
                }

                if (__AnsiLineOccupy.Count > __AnsiScrollLast)
                {
                    __AnsiLineOccupy.RemoveAt(__AnsiScrollLast);
                }
                if (__AnsiLineOccupy.Count > __AnsiScrollFirst)
                {
                    __AnsiLineOccupy.Insert(__AnsiScrollFirst, new List<int>());
                }
                if (__AnsiScrollLast > __AnsiScrollFirst)
                {
                    Screen_.Move(0, __AnsiScrollFirst, 0, __AnsiScrollFirst + 1, Screen_.WinW, __AnsiScrollLast - __AnsiScrollFirst);
                }

                for (int i = __AnsiScrollLast; i > __AnsiScrollFirst; i--)
                {
                    AnsiSetFontSize(i, AnsiGetFontSize(i - 1), false);
                }
                AnsiSetFontSize(__AnsiScrollFirst, 0, true);

                for (int i = 0; i < AnsiMaxX; i++)
                {
                    AnsiChar(i, __AnsiScrollFirst, 32, __AnsiBackWork, __AnsiForeWork, 0, 0);
                }

                for (int i = (WinH - 1); i <= __AnsiScrollLast; i++)
                {
                    AnsiRepaintLine(i);
                }

                Lines++;
            }
            while (Lines > 0)
            {
                if (__AnsiLineOccupy1_Use)
                {
                    if (__AnsiScrollFirst == 0)
                    {
                        while (__AnsiLineOccupy.Count <= 0)
                        {
                            __AnsiLineOccupy.Add(new List<int>());
                        }
                        __AnsiLineOccupy1.Add(__AnsiLineOccupy[0]);
                    }
                }

                if (__AnsiLineOccupy.Count > __AnsiScrollFirst)
                {
                    __AnsiLineOccupy.RemoveAt(__AnsiScrollFirst);
                }
                if (__AnsiLineOccupy.Count > __AnsiScrollLast)
                {
                    __AnsiLineOccupy.Insert(__AnsiScrollLast, new List<int>());
                }
                if (__AnsiScrollLast > __AnsiScrollFirst)
                {
                    Screen_.Move(0, __AnsiScrollFirst + 1, 0, __AnsiScrollFirst, Screen_.WinW, __AnsiScrollLast - __AnsiScrollFirst);
                }

                for (int i = __AnsiScrollFirst; i < __AnsiScrollLast; i++)
                {
                    AnsiSetFontSize(i, AnsiGetFontSize(i + 1), false);
                }
                AnsiSetFontSize(__AnsiScrollLast, 0, true);

                for (int i = 0; i < AnsiMaxX; i++)
                {
                    AnsiChar(i, __AnsiScrollLast, 32, __AnsiBackWork, __AnsiForeWork, 0, 0);
                }

                for (int i = (WinH - 1); i <= __AnsiScrollLast; i++)
                {
                    AnsiRepaintLine(i);
                }

                Lines--;
            }
        }

    }
}