using System;
using System.Collections.Generic;

namespace TextPaint
{
    public partial class Core
    {
        public int AnsiMaxX = 0;
        public int AnsiMaxY = 0;

        int __AnsiBack_ = -1;
        int __AnsiFore_ = -1;
        int __AnsiX_ = 0;
        int __AnsiY_ = 0;
        bool __AnsiFontBold_ = false;
        bool __AnsiFontUnderline_ = false;
        bool __AnsiFontInverse_ = false;
        bool __AnsiFontBlink1_ = false;
        bool __AnsiFontBlink2_ = false;


        List<int> __AnsiCmd = new List<int>();

        // Printable character replacement for standard DOS character from 00h to 1Fh
        int[] DosControl = { 0x0020, 0x263A, 0x263B, 0x2665, 0x2666, 0x2663, 0x2660, 0x2022,
                             0x25D8, 0x25CB, 0x25D9, 0x2642, 0x2640, 0x266A, 0x266B, 0x263C,
                             0x25BA, 0x25C4, 0x2195, 0x203C, 0x00B6, 0x00A7, 0x25AC, 0x21A8,
                             0x2191, 0x2193, 0x2192, 0x2190, 0x221F, 0x2194, 0x25B2, 0x25BC };

        bool ANSIPrintControlChars = false;
        bool ANSIDOSNewLine = false;
        bool ANSIMusic = false;
        bool ANSIIgnoreBlink = false;
        bool ANSIMoveRightWrapLine = false;
        bool ANSIIgnoreVerticalTab = false;
        bool ANSIIgnoreHorizontalTab = false;

        // 0 - Do not change
        // 1 - Use as CRLF
        // 2 - Ommit
        int ANSI_CR = 0;
        int ANSI_LF = 0;

        int __AnsiBack = -1;
        int __AnsiFore = -1;
        int __AnsiBackWork = -1;
        int __AnsiForeWork = -1;
        bool __AnsiFontBold = false;
        bool __AnsiFontUnderline = false;
        bool __AnsiFontInverse = false;
        bool __AnsiFontBlink1 = false;
        bool __AnsiFontBlink2 = false;

        public bool __AnsiTestCmd = false;
        int __AnsiTest = 0;
        bool __AnsiCommand = false;
        bool __AnsiCommandPrint = false;
        int __AnsiCounter = 0;
        bool __AnsiScreen = false;

        int __AnsiScrollFirst = 0;
        int __AnsiScrollLast = 0;

        int __AnsiLineOccupyFactor = 3;

        public int __AnsiX = 0;
        public int __AnsiY = 0;
        List<List<int>> __AnsiLineOccupy = new List<List<int>>();

        bool __AnsiMusic = false;
        bool __AnsiUseEOF = false;
        bool __AnsiBeyondEOF = false;
        bool __AnsiNoWrap = false;

        public void AnsiProcessReset(bool __AnsiUseEOF_)
        {
            __AnsiUseEOF = __AnsiUseEOF_;
            __AnsiBeyondEOF = false;
            if ((WorkMode == 1) || (WorkMode == 2))
            {
                __AnsiScreen = true;
            }
            else
            {
                __AnsiScreen = false;
            }
            __AnsiX_ = 0;
            __AnsiY_ = 0;
            __AnsiBack_ = -1;
            __AnsiFore_ = -1;
            __AnsiFontBold_ = false;
            __AnsiFontUnderline_ = false;
            __AnsiFontInverse_ = false;
            __AnsiFontBlink1_ = false;
            __AnsiFontBlink2_ = false;

            __AnsiCmd = new List<int>();

            __AnsiBack = -1;
            __AnsiFore = -1;
            __AnsiFontBold = false;
            __AnsiFontUnderline = false;
            __AnsiFontInverse = false;
            __AnsiFontBlink1 = false;
            __AnsiFontBlink2 = false;

            __AnsiTest = 0;
            __AnsiCommand = false;
            __AnsiX = 0;
            __AnsiY = 0;
            __AnsiLineOccupy.Clear();

            __AnsiCounter = 0;

            __AnsiScrollFirst = 0;
            __AnsiScrollLast = AnsiMaxY - 1;
            __AnsiMusic = false;
            __AnsiNoWrap = false;
        }

        public bool AnsiProcess(List<int> TextFileLine)
        {
            if ((TextFileLine.Count == 0) || (__AnsiBeyondEOF))
            {
                return false;
            }
            for (int i = 0; i < TextFileLine.Count; i++)
            {
                if (__AnsiBeyondEOF)
                {
                    break;
                }
                if (__AnsiCommand)
                {

                    if (TextFileLine[i] < 32)
                    {
                        __AnsiCommandPrint = true;
                    }
                    else
                    {
                        __AnsiCmd.Add(TextFileLine[i]);
                        __AnsiCommandPrint = false;
                    }

                    if (__AnsiTestCmd)
                    {
                        if (__AnsiCmd.Count > 20)
                        {
                            Console.WriteLine("ANSI long command " + TextWork.IntToStr(__AnsiCmd));
                        }
                    }

                    switch (__AnsiCmd[0])
                    {
                        case '#':
                            if ((TextFileLine[i] >= 0x30) && (TextFileLine[i] <= 0x39))
                            {
                                if (TextFileLine[i] == 0x38)
                                {
                                    for (int YY = 0; YY < AnsiMaxY; YY++)
                                    {
                                        for (int XX = 0; XX < AnsiMaxX; XX++)
                                        {
                                            AnsiChar(XX, YY, 'E');
                                        }
                                    }
                                }
                                __AnsiCommand = false;
                            }
                            break;
                        case ']':
                            if (TextFileLine[i] == 0x07)
                            {
                                __AnsiCommand = false;
                            }
                            break;
                        case '7':
                            __AnsiX_ = __AnsiX;
                            __AnsiY_ = __AnsiY;
                            __AnsiBack_ = __AnsiBack;
                            __AnsiFore_ = __AnsiFore;
                            __AnsiFontBold_ = __AnsiFontBold;
                            __AnsiFontUnderline_ = __AnsiFontUnderline;
                            __AnsiFontInverse_ = __AnsiFontInverse;
                            __AnsiFontBlink1_ = __AnsiFontBlink1;
                            __AnsiFontBlink2_ = __AnsiFontBlink2;
                            __AnsiCommand = false;
                            break;
                        case '8':
                            __AnsiX = __AnsiX_;
                            __AnsiY = __AnsiY_;
                            __AnsiBack = __AnsiBack_;
                            __AnsiFore = __AnsiFore_;
                            __AnsiFontBold = __AnsiFontBold_;
                            __AnsiFontUnderline = __AnsiFontUnderline_;
                            __AnsiFontInverse = __AnsiFontInverse_;
                            __AnsiFontBlink1 = __AnsiFontBlink1_;
                            __AnsiFontBlink2 = __AnsiFontBlink2_;
                            __AnsiCommand = false;
                            break;
                        case 'D':
                            if (ANSIMusic)
                            {

                            }
                            else
                            {
                                __AnsiY += 1;
                                while (__AnsiY > __AnsiScrollLast)
                                {
                                    AnsiScrollLines(1);
                                    __AnsiY--;
                                }
                            }
                            __AnsiCommand = false;
                            break;
                        case 'M':
                            if (ANSIMusic)
                            {
                                __AnsiMusic = true;
                            }
                            else
                            {
                                __AnsiY -= 1;
                                while (__AnsiY < __AnsiScrollFirst)
                                {
                                    AnsiScrollLines(-1);
                                    __AnsiY++;
                                }
                            }
                            __AnsiCommand = false;
                            break;
                        case 'E':
                            __AnsiY += 1;
                            if (__AnsiY >= AnsiMaxY)
                            {
                                AnsiScrollLines(1);
                                __AnsiY--;
                            }
                            __AnsiX = 0;
                            __AnsiCommand = false;
                            break;
                    }

                    if (__AnsiCommand && (__AnsiCmd[0] != ']') && (((TextFileLine[i] >= 0x41) && (TextFileLine[i] <= 0x5A)) || ((TextFileLine[i] >= 0x61) && (TextFileLine[i] <= 0x7A)) || (TextFileLine[i] == 0x3E)))
                    {
                        __AnsiCommand = false;
                        string AnsiCmd_ = TextWork.IntToStr(__AnsiCmd);
                        if (AnsiCmd_.StartsWith("[?"))
                        {
                            switch (AnsiCmd_)
                            {
                                case "[?3l":
                                    {
                                        for (int i_ = 0; i_ < AnsiLineCount(); i_++)
                                        {
                                            for (int ii_ = 0; ii_ < AnsiLineLength(i_); ii_++)
                                            {
                                                AnsiChar(ii_, i_, 32);
                                            }
                                        }
                                        __AnsiX = 0;
                                        if (AnsiMaxY > 0)
                                        {
                                            __AnsiY = __AnsiScrollFirst;
                                        }
                                        else
                                        {
                                            __AnsiY = 0;
                                        }
                                    }
                                    break;
                                case "[?3h":
                                    {
                                        for (int i_ = 0; i_ < AnsiLineCount(); i_++)
                                        {
                                            for (int ii_ = 0; ii_ < AnsiLineLength(i_); ii_++)
                                            {
                                                AnsiChar(ii_, i_, 32);
                                            }
                                        }
                                        __AnsiX = 0;
                                        if (AnsiMaxY > 0)
                                        {
                                            __AnsiY = __AnsiScrollFirst;
                                        }
                                        else
                                        {
                                            __AnsiY = 0;
                                        }
                                    }
                                    break;
                                case "[?6h":
                                    {
                                        __AnsiX = 0;
                                        __AnsiY = __AnsiScrollFirst;
                                    }
                                    break;
                                case "[?6l":
                                    {
                                        __AnsiScrollFirst = 0;
                                        __AnsiScrollLast = AnsiMaxY - 1;
                                        __AnsiX = 0;
                                        __AnsiY = 0;
                                    }
                                    break;

                                case "[?7h":
                                    {
                                        __AnsiNoWrap = false;
                                    }
                                    break;
                                case "[?7l":
                                    {
                                        __AnsiNoWrap = true;
                                    }
                                    break;
                            }
                        }
                        if (AnsiCmd_.StartsWith("[") && (!AnsiCmd_.StartsWith("[?")))
                        {
                            switch (AnsiCmd_)
                            {
                                case "[s":
                                    __AnsiX_ = __AnsiX;
                                    __AnsiY_ = __AnsiY;
                                    __AnsiBack_ = __AnsiBack;
                                    __AnsiFore_ = __AnsiFore;
                                    __AnsiFontBold_ = __AnsiFontBold;
                                    __AnsiFontUnderline_ = __AnsiFontUnderline;
                                    __AnsiFontInverse_ = __AnsiFontInverse;
                                    __AnsiFontBlink1_ = __AnsiFontBlink1;
                                    __AnsiFontBlink2_ = __AnsiFontBlink2;
                                    break;
                                case "[u":
                                    __AnsiX = __AnsiX_;
                                    __AnsiY = __AnsiY_;
                                    __AnsiBack = __AnsiBack_;
                                    __AnsiFore = __AnsiFore_;
                                    __AnsiFontBold = __AnsiFontBold_;
                                    __AnsiFontUnderline = __AnsiFontUnderline_;
                                    __AnsiFontInverse = __AnsiFontInverse_;
                                    __AnsiFontBlink1 = __AnsiFontBlink1_;
                                    __AnsiFontBlink2 = __AnsiFontBlink2_;
                                    break;
                                case "[H":
                                    __AnsiX = 0;
                                    __AnsiY = 0;
                                    break;
                                case "[J":
                                case "[0J":
                                    AnsiCalcColor();
                                    for (int i_ = __AnsiY + 1; i_ < AnsiLineCount(); i_++)
                                    {
                                        for (int ii_ = 0; ii_ < AnsiLineLength(i_); ii_++)
                                        {
                                            AnsiChar(ii_, i_, 32);
                                        }
                                    }
                                    if (AnsiLineCount() > __AnsiY)
                                    {
                                        for (int ii_ = __AnsiX; ii_ < AnsiLineLength(__AnsiY); ii_++)
                                        {
                                            AnsiChar(ii_, __AnsiY, 32);
                                        }
                                    }
                                    break;
                                case "[1J":
                                    AnsiCalcColor();
                                    for (int i_ = 0; i_ < __AnsiY; i_++)
                                    {
                                        if (AnsiLineCount() > __AnsiY)
                                        {
                                            for (int ii_ = 0; ii_ < AnsiLineLength(i_); ii_++)
                                            {
                                                AnsiChar(ii_, i_, 32);
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    for (int ii_ = 0; ii_ <= __AnsiX; ii_++)
                                    {
                                        AnsiChar(ii_, __AnsiY, 32);
                                    }
                                    break;
                                case "[2J":
                                    AnsiCalcColor();
                                    if (__AnsiTest == 2)
                                    {
                                        Console.Clear();
                                    }
                                    for (int i_ = 0; i_ < AnsiLineCount(); i_++)
                                    {
                                        for (int ii_ = 0; ii_ < AnsiLineLength(i_); ii_++)
                                        {
                                            AnsiChar(ii_, i_, 32);
                                        }
                                    }
                                    __AnsiX = 0;
                                    __AnsiY = 0;
                                    break;
                                case "[K":
                                case "[0K":
                                    if (AnsiLineCount() > __AnsiY)
                                    {
                                        AnsiCalcColor();
                                        for (int ii_ = __AnsiX; ii_ < AnsiLineLength(__AnsiY); ii_++)
                                        {
                                            AnsiChar(ii_, __AnsiY, 32);
                                        }
                                    }
                                    break;
                                case "[1K":
                                    if (AnsiLineCount() > __AnsiY)
                                    {
                                        AnsiCalcColor();
                                        for (int ii_ = 0; ii_ <= __AnsiX; ii_++)
                                        {
                                            AnsiChar(ii_, __AnsiY, 32);
                                        }
                                    }
                                    break;
                                case "[2K":
                                    if (AnsiLineCount() > __AnsiY)
                                    {
                                        AnsiCalcColor();
                                        for (int ii_ = 0; ii_ < AnsiLineLength(__AnsiY); ii_++)
                                        {
                                            AnsiChar(ii_, __AnsiY, 32);
                                        }
                                    }
                                    break;
                                default:
                                    {
                                        string[] AnsiParams = AnsiCmd_.Substring(1, AnsiCmd_.Length - 2).Split(';');
                                        switch (AnsiCmd_[AnsiCmd_.Length - 1])
                                        {
                                            case 'H':
                                            case 'f':
                                                if (AnsiParams.Length == 1)
                                                {
                                                    AnsiParams = new string[] { AnsiParams[0], "1" };
                                                }
                                                if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                if (AnsiParams[1] == "") { AnsiParams[1] = "1"; }
                                                __AnsiY = int.Parse(AnsiParams[0]) - 1;
                                                __AnsiX = int.Parse(AnsiParams[1]) - 1;
                                                if (__AnsiY >= AnsiMaxY)
                                                {
                                                    __AnsiY = AnsiMaxY - 1;
                                                }
                                                if (__AnsiY > __AnsiScrollLast)
                                                {
                                                    __AnsiY = __AnsiScrollLast;
                                                }
                                                if (__AnsiY < __AnsiScrollFirst)
                                                {
                                                    __AnsiY = __AnsiScrollFirst;
                                                }
                                                break;
                                            case 'A':
                                                if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                __AnsiY -= Math.Max(int.Parse(AnsiParams[0]), 1);
                                                if (__AnsiY < 0)
                                                {
                                                    __AnsiY = 0;
                                                }
                                                break;
                                            case 'B':
                                                if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                __AnsiY += Math.Max(int.Parse(AnsiParams[0]), 1);
                                                break;
                                            case 'C':
                                                if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                __AnsiX += Math.Max(int.Parse(AnsiParams[0]), 1);
                                                if ((AnsiMaxX > 0) && (__AnsiX >= AnsiMaxX))
                                                {
                                                    if (ANSIMoveRightWrapLine)
                                                    {
                                                        __AnsiY++;
                                                        __AnsiX = __AnsiX - AnsiMaxX;
                                                    }
                                                    else
                                                    {
                                                        __AnsiX = AnsiMaxX - 1;
                                                    }
                                                }
                                                break;
                                            case 'D':
                                                if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                if (AnsiMaxX > 0)
                                                {
                                                    if (__AnsiX >= AnsiMaxX)
                                                    {
                                                        __AnsiX = AnsiMaxX - 1;
                                                    }
                                                }
                                                __AnsiX -= Math.Max(int.Parse(AnsiParams[0]), 1);
                                                if (__AnsiX < 0)
                                                {
                                                    __AnsiX = 0;
                                                }
                                                break;

                                            case 'd':
                                                if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                __AnsiY = int.Parse(AnsiParams[0]) - 1;
                                                break;
                                            case 'e':
                                                if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                __AnsiY += int.Parse(AnsiParams[0]);
                                                break;

                                            case 'E':
                                                __AnsiX = 0;
                                                __AnsiY += int.Parse(AnsiParams[0]);
                                                break;
                                            case 'F':
                                                __AnsiX = 0;
                                                __AnsiY -= int.Parse(AnsiParams[0]);
                                                break;
                                            case 'G':
                                                __AnsiX = int.Parse(AnsiParams[0]) - 1;
                                                break;
                                            case 'S':
                                                if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                AnsiScrollLines(int.Parse(AnsiParams[0]));
                                                break;
                                            case 'T':
                                                if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                AnsiScrollLines(0 - int.Parse(AnsiParams[0]));
                                                break;
                                            case 'r':
                                                if (AnsiParams.Length == 1)
                                                {
                                                    AnsiParams = new string[] { AnsiParams[0], (AnsiMaxY + 1).ToString() };
                                                }
                                                if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                if (AnsiParams[1] == "") { AnsiParams[1] = (AnsiMaxY + 1).ToString(); }
                                                __AnsiScrollFirst = int.Parse(AnsiParams[0]) - 1;
                                                __AnsiScrollLast = int.Parse(AnsiParams[1]) - 1;
                                                if (__AnsiY < __AnsiScrollFirst)
                                                {
                                                    __AnsiY = __AnsiScrollFirst;
                                                }
                                                if (__AnsiY > __AnsiScrollLast)
                                                {
                                                    __AnsiY = __AnsiScrollLast;
                                                }
                                                break;
                                            case 'L':
                                                if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                if (ANSIMusic)
                                                {

                                                }
                                                else
                                                {
                                                    AnsiScrollLines(0 - int.Parse(AnsiParams[0]));
                                                }
                                                break;
                                            case 'M':
                                                if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                if (ANSIMusic)
                                                {
                                                    __AnsiMusic = true;
                                                }
                                                else
                                                {
                                                    AnsiScrollLines(int.Parse(AnsiParams[0]));
                                                }
                                                break;
                                            case 'm':
                                                {
                                                    for (int i_ = 0; i_ < AnsiParams.Length; i_++)
                                                    {

                                                        switch (AnsiParams[i_])
                                                        {
                                                            case "0":
                                                            case "":
                                                                __AnsiFore = -1;
                                                                __AnsiBack = -1;
                                                                __AnsiFontBold = false;
                                                                __AnsiFontUnderline = false;
                                                                __AnsiFontInverse = false;
                                                                __AnsiFontBlink1 = false;
                                                                __AnsiFontBlink2 = false;
                                                                break;

                                                            case "39": __AnsiFore = -1; break;
                                                            case "49": __AnsiBack = -1; break;

                                                            case "30": __AnsiFore = 0; break;
                                                            case "31": __AnsiFore = 1; break;
                                                            case "32": __AnsiFore = 2; break;
                                                            case "33": __AnsiFore = 3; break;
                                                            case "34": __AnsiFore = 4; break;
                                                            case "35": __AnsiFore = 5; break;
                                                            case "36": __AnsiFore = 6; break;
                                                            case "37": __AnsiFore = 7; break;

                                                            case "90": __AnsiFore = 8; break;
                                                            case "91": __AnsiFore = 9; break;
                                                            case "92": __AnsiFore = 10; break;
                                                            case "93": __AnsiFore = 11; break;
                                                            case "94": __AnsiFore = 12; break;
                                                            case "95": __AnsiFore = 13; break;
                                                            case "96": __AnsiFore = 14; break;
                                                            case "97": __AnsiFore = 15; break;

                                                            case "40": __AnsiBack = 0; break;
                                                            case "41": __AnsiBack = 1; break;
                                                            case "42": __AnsiBack = 2; break;
                                                            case "43": __AnsiBack = 3; break;
                                                            case "44": __AnsiBack = 4; break;
                                                            case "45": __AnsiBack = 5; break;
                                                            case "46": __AnsiBack = 6; break;
                                                            case "47": __AnsiBack = 7; break;

                                                            case "100": __AnsiBack = 8; break;
                                                            case "101": __AnsiBack = 9; break;
                                                            case "102": __AnsiBack = 10; break;
                                                            case "103": __AnsiBack = 11; break;
                                                            case "104": __AnsiBack = 12; break;
                                                            case "105": __AnsiBack = 13; break;
                                                            case "106": __AnsiBack = 14; break;
                                                            case "107": __AnsiBack = 15; break;

                                                            case "1": __AnsiFontBold = true; break;
                                                            case "22": __AnsiFontBold = false; break;
                                                            case "4": __AnsiFontUnderline = true; break;
                                                            case "24": __AnsiFontUnderline = false; break;
                                                            case "5": __AnsiFontBlink1 = true; break;
                                                            case "25": __AnsiFontBlink1 = true; break;
                                                            case "6": __AnsiFontBlink2 = true; break;
                                                            case "26": __AnsiFontBlink2 = true; break;
                                                            case "7": __AnsiFontInverse = true; break;
                                                            case "27": __AnsiFontInverse = false; break;

                                                            default:
                                                                //Console.BackgroundColor = ConsoleColor.Black;
                                                                //Console.ForegroundColor = ConsoleColor.White;
                                                                //Console.WriteLine("{" + AnsiParams[i_] + "}");
                                                                break;
                                                        }
                                                    }
                                                }
                                                break;
                                            case 'P':
                                                {
                                                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                    int DelCycle = int.Parse(AnsiParams[0]);
                                                    while (DelCycle > 0)
                                                    {
                                                        int TempC = 0, TempB = 0, TempF = 0;
                                                        for (int i_ = __AnsiX + 1; i_ < (__AnsiLineOccupy[__AnsiY].Count / __AnsiLineOccupyFactor); i_++)
                                                        {
                                                            AnsiGet(i_, __AnsiY, out TempC, out TempB, out TempF);
                                                            AnsiCalcColor(TempB, TempF);
                                                            AnsiChar(i_ - 1, __AnsiY, TempC);
                                                        }
                                                        if (TempC > 0)
                                                        {
                                                            AnsiCalcColor();
                                                            AnsiChar((__AnsiLineOccupy[__AnsiY].Count / __AnsiLineOccupyFactor) - 1, __AnsiY, 32);
                                                        }
                                                        DelCycle--;
                                                    }
                                                }
                                                break;
                                            case 'X':
                                                {
                                                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                                                    int DelCycle = int.Parse(AnsiParams[0]);
                                                    AnsiCalcColor();
                                                    while (DelCycle > 0)
                                                    {
                                                        AnsiChar(__AnsiX + DelCycle - 1, __AnsiY, 32);
                                                        DelCycle--;
                                                    }
                                                }
                                                break;


                                            default:
                                                if (__AnsiTestCmd)
                                                {
                                                    Console.WriteLine("ANSI unsupported command " + AnsiCmd_);
                                                }
                                                break;

                                        }
                                    }
                                    break;
                            }
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
                    if (__AnsiCommandPrint)
                    {
                        AnsiCharPrint(TextFileLine[i]);
                    }
                }
                else
                {
                    if (TextFileLine[i] == 27)
                    {
                        __AnsiCmd.Clear();
                        __AnsiCommand = true;
                        __AnsiCommandPrint = true;
                    }
                    else
                    {
                        AnsiCharPrint(TextFileLine[i]);
                    }
                }
                __AnsiCounter++;
            }
            return true;
        }

        private void AnsiCharPrint(int TextFileLine_i)
        {
            if ((!__AnsiMusic) && (TextFileLine_i < 32) && (ANSIPrintControlChars))
            {
                switch (TextFileLine_i)
                {
                    case 8:
                    case 13:
                    case 10:
                        break;
                    case 26:
                        if (__AnsiUseEOF)
                        {
                            __AnsiBeyondEOF = true;
                        }
                        break;
                    case 9:
                        if (ANSIIgnoreHorizontalTab)
                        {
                            TextFileLine_i = DosControl[TextFileLine_i];
                        }
                        break;
                    case 11:
                        if (ANSIIgnoreVerticalTab)
                        {
                            TextFileLine_i = DosControl[TextFileLine_i];
                        }
                        break;
                    default:
                        TextFileLine_i = DosControl[TextFileLine_i];
                        break;
                }
            }
            AnsiCalcColor();
            if ((TextFileLine_i >= 32))
            {
                if (!__AnsiMusic)
                {
                    if (ANSIDOSNewLine)
                    {
                        AnsiChar(__AnsiX, __AnsiY, TextFileLine_i, __AnsiBackWork, __AnsiForeWork);
                        __AnsiX++;
                        if ((AnsiMaxX > 0) && (__AnsiX == AnsiMaxX))
                        {
                            if (__AnsiNoWrap)
                            {
                                __AnsiX--;
                            }
                            else
                            {
                                __AnsiX = 0;
                                __AnsiY++;
                                if (AnsiMaxY > 0)
                                {
                                    while (__AnsiY > __AnsiScrollLast)
                                    {
                                        AnsiScrollLines(1);
                                        __AnsiY--;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((AnsiMaxX > 0) && (__AnsiX == AnsiMaxX))
                        {
                            if (__AnsiNoWrap)
                            {
                                __AnsiX--;
                            }
                            else
                            {
                                __AnsiX = 0;
                                __AnsiY++;
                                if (AnsiMaxY > 0)
                                {
                                    while (__AnsiY > __AnsiScrollLast)
                                    {
                                        AnsiScrollLines(1);
                                        __AnsiY--;
                                    }
                                }
                            }
                        }
                        AnsiChar(__AnsiX, __AnsiY, TextFileLine_i, __AnsiBackWork, __AnsiForeWork);
                        __AnsiX++;
                    }
                }
            }
            else
            {
                if (__AnsiMusic)
                {
                    switch (TextFileLine_i)
                    {
                        case 3:
                        case 14:
                            {
                                if (__AnsiMusic)
                                {
                                    __AnsiMusic = false;
                                }
                            }
                            break;
                    }
                }
                else
                {
                    switch (TextFileLine_i)
                    {
                        case 8:
                            {
                                if (__AnsiX > 0)
                                {
                                    __AnsiX--;
                                    int TempC, TempB, TempF;
                                    AnsiGet(__AnsiX, __AnsiY, out TempC, out TempB, out TempF);
                                    //AnsiChar(__AnsiX, __AnsiY, 32, TempB, TempF);
                                }
                            }
                            break;
                        case 9:
                            {
                                if (!ANSIIgnoreHorizontalTab)
                                {
                                    while (((__AnsiX == 0) || ((__AnsiX % 6) > 0)) && (__AnsiX < AnsiMaxX))
                                    {
                                        __AnsiX++;
                                    }
                                }
                            }
                            break;
                        case 13:
                            {
                                __AnsiX = 0;
                            }
                            break;
                        case 10:
                            {
                                __AnsiY++;
                                if (AnsiMaxY > 0)
                                {
                                    while (__AnsiY > __AnsiScrollLast)
                                    {
                                        AnsiScrollLines(1);
                                        __AnsiY--;
                                    }
                                }
                            }
                            break;
                        case 11:
                            if (!ANSIIgnoreVerticalTab)
                            {
                                __AnsiY++;
                                if (AnsiMaxY > 0)
                                {
                                    while (__AnsiY > __AnsiScrollLast)
                                    {
                                        AnsiScrollLines(1);
                                        __AnsiY--;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
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

        public void AnsiGet(int X, int Y, out int Ch, out int ColB, out int ColF)
        {
            Ch = 32;
            ColB = TextNormalBack;
            ColF = TextNormalFore;
            if (!__AnsiScreen)
            {
                Ch = CharGet(X, Y, true);
                int Col = ColoGet(X, Y, true);
                ColorFromInt(Col, out ColB, out ColF);
            }
            if (__AnsiScreen)
            {
                if (__AnsiLineOccupy.Count > Y)
                {
                    if ((__AnsiLineOccupy[Y].Count / __AnsiLineOccupyFactor) > 0)
                    {
                        Ch = __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 0];
                        ColB = __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 1];
                        ColF = __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 2];
                    }
                }
            }
        }


        public void AnsiRepaintLine(int Y)
        {
            if (Y < __AnsiLineOccupy.Count)
            {
                int L = (__AnsiLineOccupy[Y].Count / __AnsiLineOccupyFactor);
                for (int X = 0; X < WinW; X++)
                {
                    if (X < L)
                    {
                        Screen_.PutChar(X, Y, __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 0], __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 1], __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 2]);
                    }
                    else
                    {
                        Screen_.PutChar(X, Y, ' ', TextNormalBack, TextNormalFore);
                    }
                }
            }
        }

        public void AnsiRepaint()
        {
            if (__AnsiScreen)
            {
                Screen_.Clear(TextNormalBack, TextNormalFore);
                for (int Y = 0; Y < __AnsiLineOccupy.Count; Y++)
                {
                    for (int X = 0; X < (__AnsiLineOccupy[Y].Count / __AnsiLineOccupyFactor); X++)
                    {
                        Screen_.PutChar(X, Y, __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 0], __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 1], __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 2]);
                    }
                }
            }
        }

        public void AnsiChar(int X, int Y, int Ch)
        {
            AnsiChar(X, Y, Ch, __AnsiBackWork, __AnsiForeWork);
        }

        public void AnsiChar(int X, int Y, int Ch, int ColB, int ColF)
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

            if (!__AnsiScreen)
            {
                CharPut(X, Y, Ch, ColorToInt(ColB, ColF));
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
                Screen_.PutChar(X, Y, Ch, ColB, ColF);
            }
            while (__AnsiLineOccupy.Count <= Y)
            {
                __AnsiLineOccupy.Add(new List<int>());
            }
            while ((__AnsiLineOccupy[Y].Count / __AnsiLineOccupyFactor) <= X)
            {
                __AnsiLineOccupy[Y].Add(32);
                __AnsiLineOccupy[Y].Add(TextNormalBack);
                __AnsiLineOccupy[Y].Add(TextNormalFore);
            }
            __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 0] = Ch;
            __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 1] = ColB;
            __AnsiLineOccupy[Y][X * __AnsiLineOccupyFactor + 2] = ColF;
        }

        public void AnsiScrollLines(int Lines)
        {
            AnsiCalcColor();
            if (__AnsiScreen)
            {
                while (Lines < 0)
                {
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
                    for (int i = 0; i < AnsiMaxX; i++)
                    {
                        AnsiChar(i, __AnsiScrollFirst, 32, __AnsiBackWork, __AnsiForeWork);
                    }

                    Lines++;
                }
                while (Lines > 0)
                {
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
                    for (int i = 0; i < AnsiMaxX; i++)
                    {
                        AnsiChar(i, __AnsiScrollLast, 32, __AnsiBackWork, __AnsiForeWork);
                    }

                    for (int i = (WinH - 1); i <= __AnsiScrollLast; i++)
                    {
                        AnsiRepaintLine(i);
                    }

                    Lines--;
                }
            }
            else
            {
                while (Lines < 0)
                {
                    if (TextBuffer.Count > __AnsiScrollLast)
                    {
                        TextBuffer.RemoveAt(__AnsiScrollLast);
                        TextColBuf.RemoveAt(__AnsiScrollLast);
                    }
                    if (TextBuffer.Count > __AnsiScrollFirst)
                    {
                        TextBuffer.Insert(__AnsiScrollFirst, new List<int>());
                        TextColBuf.Insert(__AnsiScrollFirst, new List<int>());
                    }
                    Lines++;
                }
                while (Lines > 0)
                {
                    if (TextBuffer.Count > __AnsiScrollFirst)
                    {
                        TextBuffer.RemoveAt(__AnsiScrollFirst);
                        TextColBuf.RemoveAt(__AnsiScrollFirst);
                    }
                    if (TextBuffer.Count > __AnsiScrollLast)
                    {
                        TextBuffer.Insert(__AnsiScrollLast, new List<int>());
                        TextColBuf.Insert(__AnsiScrollLast, new List<int>());
                    }
                    Lines--;
                }
            }
        }

        private int AnsiLineCount()
        {
            if (!__AnsiScreen)
            {
                return TextBuffer.Count;
            }
            return AnsiMaxY;
        }

        private int AnsiLineLength(int N)
        {
            if (!__AnsiScreen)
            {
                if (TextBuffer.Count > N)
                {
                    return TextBuffer[N].Count;
                }
                else
                {
                    return 0;
                }
            }
            return AnsiMaxX;
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

            if (__AnsiFontInverse)
            {
                int Temp = __AnsiForeWork;
                __AnsiForeWork = __AnsiBackWork;
                __AnsiBackWork = Temp;
            }


            if (__AnsiFontBold)
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

            if ((__AnsiFontBlink1 || __AnsiFontBlink2) && (!ANSIIgnoreBlink))
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

            if ((B < 0) && (__AnsiBackWork == TextNormalBack))
            {
                __AnsiBackWork = -1;
            }
            if ((B < 0) && (__AnsiForeWork == TextNormalFore))
            {
                __AnsiForeWork = -1;
            }
        }
    }
}
