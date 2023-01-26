using System;
using System.Collections.Generic;

namespace TextPaint
{
    public partial class Core
    {
        bool __AnsiSmoothScroll = false;

        int ANSIScrollChars = 0;
        bool ANSIScrollBuffer = false;
        bool AnsiScrollRev = false;
        int AnsiScrollLinesI = 0;
        int ANSIScrollSmooth = 0;

        enum AnsiScrollCommandDef { None, Char, FirstLast, Tab };
        int AnsiScrollCounter = 0;
        AnsiScrollCommandDef AnsiScrollCommand = AnsiScrollCommandDef.None;
        int AnsiScrollParam1 = 0;
        int AnsiScrollParam2 = 0;
        int AnsiScrollParam3 = 0;

        int[] AnsiScrollPosition = null;
        int[] AnsiScrollOffset = null;

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

            if (__AnsiSmoothScroll && (ANSIScrollSmooth > 0))
            {
                AnsiScrollCounter = ANSIScrollChars + 1;

                if (AnsiScrollPosition == null)
                {
                    int ScrollTime = (AnsiScrollCounter - 1);

                    switch (ANSIScrollSmooth)
                    {
                        case 1:
                            AnsiScrollPosition = new int[1];
                            AnsiScrollOffset = new int[1];

                            AnsiScrollPosition[0] = ScrollTime / 2;
                            AnsiScrollOffset[0] = 8;
                            break;
                        case 2:
                            AnsiScrollPosition = new int[2];
                            AnsiScrollOffset = new int[2];

                            AnsiScrollPosition[0] = ScrollTime - (ScrollTime / 4);
                            AnsiScrollOffset[0] = 4;

                            AnsiScrollPosition[1] = (ScrollTime / 4);
                            AnsiScrollOffset[1] = 8;
                            break;
                        case 3:
                            AnsiScrollPosition = new int[4];
                            AnsiScrollOffset = new int[4];

                            AnsiScrollPosition[0] = ScrollTime - (ScrollTime / 8);
                            AnsiScrollOffset[0] = 2;

                            AnsiScrollPosition[1] = ScrollTime - ((3 * ScrollTime) / 8);
                            AnsiScrollOffset[1] = 4;

                            AnsiScrollPosition[2] = ((3 * ScrollTime) / 8);
                            AnsiScrollOffset[2] = 6;

                            AnsiScrollPosition[3] = (ScrollTime / 8);
                            AnsiScrollOffset[3] = 8;
                            break;
                        case 4:
                            AnsiScrollPosition = new int[8];
                            AnsiScrollOffset = new int[8];

                            AnsiScrollPosition[0] = ScrollTime - (ScrollTime / 16);
                            AnsiScrollOffset[0] = 1;

                            AnsiScrollPosition[1] = ScrollTime - ((3 * ScrollTime) / 16);
                            AnsiScrollOffset[1] = 2;

                            AnsiScrollPosition[2] = ScrollTime - ((5 * ScrollTime) / 16);
                            AnsiScrollOffset[2] = 3;

                            AnsiScrollPosition[3] = ScrollTime - ((7 * ScrollTime) / 16);
                            AnsiScrollOffset[3] = 4;

                            AnsiScrollPosition[4] = ((7 * ScrollTime) / 16);
                            AnsiScrollOffset[4] = 5;

                            AnsiScrollPosition[5] = ((5 * ScrollTime) / 16);
                            AnsiScrollOffset[5] = 6;

                            AnsiScrollPosition[6] = ((3 * ScrollTime) / 16);
                            AnsiScrollOffset[6] = 7;

                            AnsiScrollPosition[7] = (ScrollTime / 16);
                            AnsiScrollOffset[7] = 8;
                            break;
                    }
                }

                AnsiScrollProcess();
            }
            else
            {
                AnsiScrollCounter = 0;
                AnsiScrollLines(AnsiScrollRev ? 0 - AnsiScrollLinesI : AnsiScrollLinesI);
                AnsiScrollFinish(false);
            }
        }

        bool AnsiScrollFinish(bool ScrollDisp)
        {
            AnsiScrollSetOffset(0);
            switch (AnsiScrollCommand)
            {
                case AnsiScrollCommandDef.Char:
                    AnsiCharFI(__AnsiX, __AnsiY, AnsiScrollParam1, AnsiScrollParam2, AnsiScrollParam3);
                    __AnsiX++;
                    ScrollDisp = true;
                    break;
                case AnsiScrollCommandDef.FirstLast:
                    __AnsiScrollFirst = AnsiScrollParam1;
                    __AnsiScrollLast = AnsiScrollParam2;
                    break;
                case AnsiScrollCommandDef.Tab:
                    AnsiDoTab(AnsiScrollParam1);
                    break;
            }
            AnsiScrollCommand = AnsiScrollCommandDef.None;
            return ScrollDisp;
        }

        bool AnsiScrollProcess()
        {
            bool ScrollDisp = false;
            bool ScrollMove = false;

            for (int i = 0; i < AnsiScrollPosition.Length; i++)
            {
                if (AnsiScrollCounter == AnsiScrollPosition[i])
                {
                    if (AnsiScrollOffset[i] == 8)
                    {
                        ScrollMove = true;
                        AnsiScrollSetOffset(0);
                    }
                    else
                    {
                        if (AnsiScrollRev)
                        {
                            AnsiScrollSetOffset(0 - AnsiScrollOffset[i]);
                        }
                        else
                        {
                            AnsiScrollSetOffset(AnsiScrollOffset[i]);
                        }
                    }
                    ScrollDisp = true;
                }
            }

            //if (AnsiScrollCounter == ((ANSIScrollChars2) + 1))
            if (ScrollMove)
            {
                AnsiScrollLines(AnsiScrollRev ? -1 : 1);
                ScrollDisp = true;
            }
            if (AnsiScrollCounter == 1)
            {
                AnsiScrollLinesI--;
                if (AnsiScrollLinesI > 0)
                {
                    AnsiScrollCounter = ANSIScrollChars + 1;
                }
                else
                {
                    ScrollDisp = AnsiScrollFinish(ScrollDisp);
                }
            }
            AnsiScrollCounter--;
            return ScrollDisp;
        }

        int ScrollLastOffset = 0;

        private void AnsiScrollSetOffset(int Offset)
        {
            ScrollLastOffset = Offset;
            if (Offset < 0)
            {
                Screen_.SetLineOffset(__AnsiScrollLast, Offset, true, __AnsiBackScroll > 0 ? __AnsiBackScroll : TextNormalBack, __AnsiForeScroll > 0 ? __AnsiForeScroll : TextNormalFore);
                for (int Y = __AnsiScrollLast - 1; Y > __AnsiScrollFirst; Y--)
                {
                    Screen_.SetLineOffset(Y, Offset, false, __AnsiBackScroll > 0 ? __AnsiBackScroll : TextNormalBack, __AnsiForeScroll > 0 ? __AnsiForeScroll : TextNormalFore);
                }
                Screen_.SetLineOffset(__AnsiScrollFirst, Offset, true, __AnsiBackScroll > 0 ? __AnsiBackScroll : TextNormalBack, __AnsiForeScroll > 0 ? __AnsiForeScroll : TextNormalFore);
                if (__AnsiScrollLast < (WinH - 1))
                {
                    for (int X = 0; X < WinW; X++)
                    {
                        Screen_.CharRepaint(X, __AnsiScrollLast + 1);
                    }
                }
            }
            else
            {
                Screen_.SetLineOffset(__AnsiScrollFirst, Offset, true, __AnsiBackScroll > 0 ? __AnsiBackScroll : TextNormalBack, __AnsiForeScroll > 0 ? __AnsiForeScroll : TextNormalFore);
                for (int Y = __AnsiScrollFirst + 1; Y < __AnsiScrollLast; Y++)
                {
                    Screen_.SetLineOffset(Y, Offset, false, __AnsiBackScroll > 0 ? __AnsiBackScroll : TextNormalBack, __AnsiForeScroll > 0 ? __AnsiForeScroll : TextNormalFore);
                }
                Screen_.SetLineOffset(__AnsiScrollLast, Offset, true, __AnsiBackScroll > 0 ? __AnsiBackScroll : TextNormalBack, __AnsiForeScroll > 0 ? __AnsiForeScroll : TextNormalFore);
                if (__AnsiScrollFirst > 0)
                {
                    for (int X = 0; X < WinW; X++)
                    {
                        Screen_.CharRepaint(X, __AnsiScrollFirst - 1);
                    }
                }
            }
        }

        public void AnsiScrollColumns(int Columns)
        {
            AnsiCalcColor();
            int MaxPos0 = (AnsiProcessGetXMax(true) * (__AnsiLineOccupyFactor));
            int MaxPos1 = ((AnsiProcessGetXMax(true) - 1) * (__AnsiLineOccupyFactor));
            int MaxPos2 = ((AnsiProcessGetXMax(true) - 2) * (__AnsiLineOccupyFactor));
            for (int i = __AnsiScrollFirst; i <= __AnsiScrollLast; i++)
            {
                if (__AnsiLineOccupy.Count > i)
                {
                    int Columns_ = Columns;
                    while (Columns_ < 0)
                    {
                        if (AnsiGetFontSize(i) > 0)
                        {
                            if (__AnsiLineOccupy[i].Count >= (MaxPos0))
                            {
                                __AnsiLineOccupy[i].RemoveRange((AnsiProcessGetXMax(true) - 2) * (__AnsiLineOccupyFactor), (__AnsiLineOccupyFactor + __AnsiLineOccupyFactor));
                            }
                            __AnsiLineOccupy[i].Insert(0, AnsiGetFontH(i));
                            __AnsiLineOccupy[i].Insert(0, 2);
                            __AnsiLineOccupy[i].Insert(0, __AnsiForeScroll);
                            __AnsiLineOccupy[i].Insert(0, __AnsiBackScroll);
                            __AnsiLineOccupy[i].Insert(0, 32);
                            __AnsiLineOccupy[i].Insert(0, AnsiGetFontH(i));
                            __AnsiLineOccupy[i].Insert(0, 1);
                            __AnsiLineOccupy[i].Insert(0, __AnsiForeScroll);
                            __AnsiLineOccupy[i].Insert(0, __AnsiBackScroll);
                            __AnsiLineOccupy[i].Insert(0, 32);
                        }
                        else
                        {
                            if (__AnsiLineOccupy[i].Count >= (MaxPos0))
                            {
                                __AnsiLineOccupy[i].RemoveRange((AnsiProcessGetXMax(true) - 1) * (__AnsiLineOccupyFactor), (__AnsiLineOccupyFactor));
                            }
                            __AnsiLineOccupy[i].Insert(0, 0);
                            __AnsiLineOccupy[i].Insert(0, 0);
                            __AnsiLineOccupy[i].Insert(0, __AnsiForeScroll);
                            __AnsiLineOccupy[i].Insert(0, __AnsiBackScroll);
                            __AnsiLineOccupy[i].Insert(0, 32);
                        }
                        Columns_++;
                    }
                    while (Columns_ > 0)
                    {
                        if (AnsiGetFontSize(i) > 0)
                        {
                            if (__AnsiLineOccupy[i].Count >= (MaxPos0))
                            {
                                __AnsiLineOccupy[i].Insert(MaxPos2, AnsiGetFontH(i));
                                __AnsiLineOccupy[i].Insert(MaxPos2, 2);
                                __AnsiLineOccupy[i].Insert(MaxPos2, __AnsiForeScroll);
                                __AnsiLineOccupy[i].Insert(MaxPos2, __AnsiBackScroll);
                                __AnsiLineOccupy[i].Insert(MaxPos2, 32);
                                __AnsiLineOccupy[i].Insert(MaxPos2, AnsiGetFontH(i));
                                __AnsiLineOccupy[i].Insert(MaxPos2, 1);
                                __AnsiLineOccupy[i].Insert(MaxPos2, __AnsiForeScroll);
                                __AnsiLineOccupy[i].Insert(MaxPos2, __AnsiBackScroll);
                                __AnsiLineOccupy[i].Insert(MaxPos2, 32);
                            }
                            if (__AnsiLineOccupy[i].Count >= (__AnsiLineOccupyFactor + __AnsiLineOccupyFactor))
                            {
                                __AnsiLineOccupy[i].RemoveRange(0, (__AnsiLineOccupyFactor + __AnsiLineOccupyFactor));
                            }
                        }
                        else
                        {
                            if (__AnsiLineOccupy[i].Count >= (AnsiProcessGetXMax(true) * (__AnsiLineOccupyFactor)))
                            {
                                __AnsiLineOccupy[i].Insert(MaxPos1, 0);
                                __AnsiLineOccupy[i].Insert(MaxPos1, 0);
                                __AnsiLineOccupy[i].Insert(MaxPos1, __AnsiForeScroll);
                                __AnsiLineOccupy[i].Insert(MaxPos1, __AnsiBackScroll);
                                __AnsiLineOccupy[i].Insert(MaxPos1, 32);
                            }
                            if (__AnsiLineOccupy[i].Count >= (__AnsiLineOccupyFactor))
                            {
                                __AnsiLineOccupy[i].RemoveRange(0, (__AnsiLineOccupyFactor));
                            }
                        }
                        Columns_--;
                    }
                    AnsiRepaintLine(i);
                }
            }
        }

        public void AnsiScrollLines(int Lines)
        {
            AnsiCalcColor();
            while (Lines < 0)
            {
                int ScrollMarginL = AnsiProcessGetXMin(false);
                int ScrollMarginR = AnsiProcessGetXMax(false);
                if ((ScrollMarginL == 0) && (ScrollMarginR == AnsiMaxX))
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
                }
                else
                {
                    while (__AnsiLineOccupy.Count <= __AnsiScrollLast)
                    {
                        __AnsiLineOccupy.Add(new List<int>());
                    }
                    for (int YY = __AnsiScrollLast; YY > __AnsiScrollFirst; YY--)
                    {
                        while (__AnsiLineOccupy[YY].Count <= (ScrollMarginR * __AnsiLineOccupyFactor))
                        {
                            __AnsiLineOccupy[YY].Add(32);
                            __AnsiLineOccupy[YY].Add(__AnsiBackScroll);
                            __AnsiLineOccupy[YY].Add(__AnsiForeScroll);
                            __AnsiLineOccupy[YY].Add(0);
                            __AnsiLineOccupy[YY].Add(0);
                        }
                        while (__AnsiLineOccupy[YY - 1].Count <= (ScrollMarginR * __AnsiLineOccupyFactor))
                        {
                            __AnsiLineOccupy[YY - 1].Add(32);
                            __AnsiLineOccupy[YY - 1].Add(__AnsiBackScroll);
                            __AnsiLineOccupy[YY - 1].Add(__AnsiForeScroll);
                            __AnsiLineOccupy[YY - 1].Add(0);
                            __AnsiLineOccupy[YY - 1].Add(0);
                        }
                        for (int XX = ScrollMarginL; XX < ScrollMarginR; XX++)
                        {
                            __AnsiLineOccupy[YY][XX * __AnsiLineOccupyFactor + 0] = __AnsiLineOccupy[YY - 1][XX * __AnsiLineOccupyFactor + 0];
                            __AnsiLineOccupy[YY][XX * __AnsiLineOccupyFactor + 1] = __AnsiLineOccupy[YY - 1][XX * __AnsiLineOccupyFactor + 1];
                            __AnsiLineOccupy[YY][XX * __AnsiLineOccupyFactor + 2] = __AnsiLineOccupy[YY - 1][XX * __AnsiLineOccupyFactor + 2];
                            __AnsiLineOccupy[YY][XX * __AnsiLineOccupyFactor + 3] = __AnsiLineOccupy[YY - 1][XX * __AnsiLineOccupyFactor + 3];
                            __AnsiLineOccupy[YY][XX * __AnsiLineOccupyFactor + 4] = __AnsiLineOccupy[YY - 1][XX * __AnsiLineOccupyFactor + 4];
                        }
                    }
                }
                if (__AnsiScrollLast > __AnsiScrollFirst)
                {
                    Screen_.Move(ScrollMarginL, __AnsiScrollFirst, ScrollMarginL, __AnsiScrollFirst + 1, ScrollMarginR - ScrollMarginL, __AnsiScrollLast - __AnsiScrollFirst);
                }

                for (int i = __AnsiScrollLast; i > __AnsiScrollFirst; i--)
                {
                    AnsiSetFontSize(i, AnsiGetFontSize(i - 1), false);
                }
                AnsiSetFontSize(__AnsiScrollFirst, 0, true);

                for (int i = ScrollMarginL; i < ScrollMarginR; i++)
                {
                    AnsiChar(i, __AnsiScrollFirst, 32, __AnsiBackScroll, __AnsiForeScroll, 0, 0);
                }

                for (int i = (WinH - 1); i <= __AnsiScrollLast; i++)
                {
                    AnsiRepaintLine(i);
                }

                Lines++;
            }
            while (Lines > 0)
            {
                int ScrollMarginL = AnsiProcessGetXMin(false);
                int ScrollMarginR = AnsiProcessGetXMax(false);
                if ((ScrollMarginL == 0) && (ScrollMarginR == AnsiMaxX))
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
                }
                else
                {
                    while (__AnsiLineOccupy.Count <= __AnsiScrollLast)
                    {
                        __AnsiLineOccupy.Add(new List<int>());
                    }
                    for (int YY = __AnsiScrollFirst; YY < __AnsiScrollLast; YY++)
                    {
                        while (__AnsiLineOccupy[YY].Count <= (ScrollMarginR * __AnsiLineOccupyFactor))
                        {
                            __AnsiLineOccupy[YY].Add(32);
                            __AnsiLineOccupy[YY].Add(__AnsiBackScroll);
                            __AnsiLineOccupy[YY].Add(__AnsiForeScroll);
                            __AnsiLineOccupy[YY].Add(0);
                            __AnsiLineOccupy[YY].Add(0);
                        }
                        while (__AnsiLineOccupy[YY + 1].Count <= (ScrollMarginR * __AnsiLineOccupyFactor))
                        {
                            __AnsiLineOccupy[YY + 1].Add(32);
                            __AnsiLineOccupy[YY + 1].Add(__AnsiBackScroll);
                            __AnsiLineOccupy[YY + 1].Add(__AnsiForeScroll);
                            __AnsiLineOccupy[YY + 1].Add(0);
                            __AnsiLineOccupy[YY + 1].Add(0);
                        }
                        for (int XX = ScrollMarginL; XX < ScrollMarginR; XX++)
                        {
                            __AnsiLineOccupy[YY][XX * __AnsiLineOccupyFactor + 0] = __AnsiLineOccupy[YY + 1][XX * __AnsiLineOccupyFactor + 0];
                            __AnsiLineOccupy[YY][XX * __AnsiLineOccupyFactor + 1] = __AnsiLineOccupy[YY + 1][XX * __AnsiLineOccupyFactor + 1];
                            __AnsiLineOccupy[YY][XX * __AnsiLineOccupyFactor + 2] = __AnsiLineOccupy[YY + 1][XX * __AnsiLineOccupyFactor + 2];
                            __AnsiLineOccupy[YY][XX * __AnsiLineOccupyFactor + 3] = __AnsiLineOccupy[YY + 1][XX * __AnsiLineOccupyFactor + 3];
                            __AnsiLineOccupy[YY][XX * __AnsiLineOccupyFactor + 4] = __AnsiLineOccupy[YY + 1][XX * __AnsiLineOccupyFactor + 4];
                        }
                    }
                }
                if (__AnsiScrollLast > __AnsiScrollFirst)
                {
                    Screen_.Move(ScrollMarginL, __AnsiScrollFirst + 1, ScrollMarginL, __AnsiScrollFirst, ScrollMarginR - ScrollMarginL, __AnsiScrollLast - __AnsiScrollFirst);
                }

                for (int i = __AnsiScrollFirst; i < __AnsiScrollLast; i++)
                {
                    AnsiSetFontSize(i, AnsiGetFontSize(i + 1), false);
                }
                AnsiSetFontSize(__AnsiScrollLast, 0, true);

                for (int i = ScrollMarginL; i < ScrollMarginR; i++)
                {
                    AnsiChar(i, __AnsiScrollLast, 32, __AnsiBackScroll, __AnsiForeScroll, 0, 0);
                }

                for (int i = (WinH - 1); i <= __AnsiScrollLast; i++)
                {
                    AnsiRepaintLine(i);
                }

                Lines--;
            }


            /*for (int i = (WinH - 1); i >= 0; i--)
            {
                AnsiRepaintLine(i);
            }*/
        }



        void AnsiDoTab(int TabTimes)
        {
            int TabLimitL = 0;
            int TabLimitR = AnsiMaxX;
            if (__AnsiMarginLeftRight)
            {
                TabLimitL = __AnsiMarginLeft;
                TabLimitR = __AnsiMarginRight;
            }



            if (TabTimes > 0)
            {
                while (TabTimes > 0)
                {
                    if (__AnsiX > (TabLimitR - 1))
                    {
                        __AnsiX = TabLimitL;
                        __AnsiY++;
                        if ((AnsiMaxY > 0) && (__AnsiY > __AnsiScrollLast))
                        {
                            AnsiScrollInit(__AnsiY - __AnsiScrollLast, AnsiScrollCommandDef.Tab, TabTimes - 1, 0, 0);
                            __AnsiY = __AnsiScrollLast;
                            return;
                        }
                    }
                    else
                    {
                        __AnsiX++;
                        if (__AnsiTabs[__AnsiTabs.Count - 1] > __AnsiX)
                        {
                            while (!__AnsiTabs.Contains(__AnsiX))
                            {
                                __AnsiX++;
                            }
                        }
                        else
                        {
                            while ((__AnsiX % 8) > 0)
                            {
                                __AnsiX++;
                            }
                        }
                        if (AnsiGetFontSize(__AnsiY) > 0)
                        {
                            if (__AnsiX >= (TabLimitR / 2))
                            {
                                __AnsiX = (TabLimitR / 2) - 1;
                            }
                        }
                        else
                        {
                            if (__AnsiX >= TabLimitR)
                            {
                                __AnsiX = TabLimitR - 1;
                            }
                        }
                    }
                    TabTimes--;
                }
            }
            else
            {
                while (TabTimes < 0)
                {
                    __AnsiX--;
                    if (__AnsiTabs[__AnsiTabs.Count - 1] > __AnsiX)
                    {
                        while ((!__AnsiTabs.Contains(__AnsiX)) && (__AnsiX > TabLimitL))
                        {
                            __AnsiX--;
                        }
                    }
                    else
                    {
                        while (((__AnsiX % 8) > 0) && (__AnsiX > TabLimitL))
                        {
                            __AnsiX--;
                        }
                    }
                    if (__AnsiX < TabLimitL)
                    {
                        __AnsiX = TabLimitL;
                    }
                    TabTimes++;
                }
            }
        }
    }
}
