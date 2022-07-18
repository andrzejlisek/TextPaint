using System;
namespace TextPaint
{
    public partial class Core
    {
        void AnsiProcess_VT52()
        {
            switch (__AnsiCmd[0])
            {
                case 'F':
                    VT52_SemigraphDef = true;
                    __AnsiCommand = false;
                    break;
                case 'G':
                    VT52_SemigraphDef = false;
                    __AnsiCommand = false;
                    break;
                case '<':
                    __AnsiVT52 = false;
                    __AnsiCommand = false;
                    break;
                case 'A':
                    __AnsiY -= 1;
                    if (__AnsiY < 0)
                    {
                        __AnsiY = 0;
                    }
                    __AnsiCommand = false;
                    break;
                case 'B':
                    __AnsiY += 1;
                    __AnsiCommand = false;
                    break;
                case 'C':
                    __AnsiX += 1;
                    if ((AnsiMaxX > 0) && (__AnsiX >= AnsiMaxX))
                    {
                        if (ANSIDOS)
                        {
                            __AnsiY++;
                            __AnsiX = __AnsiX - AnsiMaxX;
                        }
                        else
                        {
                            __AnsiX = AnsiMaxX - 1;
                        }
                    }
                    __AnsiCommand = false;
                    break;
                case 'D':
                    if (AnsiMaxX > 0)
                    {
                        if (__AnsiX >= AnsiMaxX)
                        {
                            __AnsiX = AnsiMaxX - 1;
                        }
                    }
                    __AnsiX -= 1;
                    if (__AnsiX < 0)
                    {
                        __AnsiX = 0;
                    }
                    __AnsiCommand = false;
                    break;
                case 'H':
                    __AnsiX = 0;
                    __AnsiY = 0;
                    __AnsiCommand = false;
                    break;
                case 'Y':
                    if (__AnsiCmd.Count == 3)
                    {
                        __AnsiY = __AnsiCmd[1] - 32;
                        __AnsiX = __AnsiCmd[2] - 32;
                        __AnsiCommand = false;
                    }
                    break;
                case 'd':
                    AnsiCalcColor();
                    for (int i_ = 0; i_ < __AnsiY; i_++)
                    {
                        if (AnsiMaxY > __AnsiY)
                        {
                            for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
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
                    __AnsiCommand = false;
                    break;
                case 'E':
                case 'J':
                    if (__AnsiCmd[0] == 'E')
                    {
                        __AnsiX = 0;
                        __AnsiY = 0;
                    }
                    AnsiCalcColor();
                    for (int i_ = __AnsiY + 1; i_ < AnsiMaxY; i_++)
                    {
                        for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiChar(ii_, i_, 32);
                        }
                    }
                    if (AnsiMaxY > __AnsiY)
                    {
                        for (int ii_ = __AnsiX; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiChar(ii_, __AnsiY, 32);
                        }
                    }
                    __AnsiCommand = false;
                    break;
                case 'Z':
                    __AnsiResponse.Add("VT52:Z");
                    __AnsiCommand = false;
                    break;
                case 'I':
                    __AnsiY -= 1;
                    if (__AnsiY < __AnsiScrollFirst)
                    {
                        AnsiScrollInit(__AnsiY - __AnsiScrollFirst, AnsiScrollCommandDef.None);
                        __AnsiY = __AnsiScrollFirst;
                    }
                    __AnsiCommand = false;
                    break;
                case 'K':
                    for (int ii_ = __AnsiX; ii_ < AnsiMaxX; ii_++)
                    {
                        AnsiChar(ii_, __AnsiY, 32);
                    }
                    __AnsiCommand = false;
                    break;
                case 'b':
                case 'c':
                    if (__AnsiCmd.Count == 2)
                    {
                        __AnsiCommand = false;
                    }
                    break;
                default:
                    __AnsiCommand = false;
                    break;
            }
        }

        void AnsiProcess_Fixed(int TextFileLine_i)
        {
            switch (__AnsiCmd[0])
            {
                case '(':
                case ')':
                case '*':
                case '+':
                    if (__AnsiCmd.Count == 2)
                    {
                        int CharNum = 0;
                        switch (__AnsiCmd[0])
                        {
                            case '(':
                                CharNum = 0;
                                break;
                            case ')':
                                CharNum = 1;
                                break;
                            case '*':
                                CharNum = 2;
                                break;
                            case '+':
                                CharNum = 3;
                                break;
                        }
                        if (__AnsiCmd[1] == '0')
                        {
                            VT100_SemigraphDef[CharNum] = true;
                        }
                        else
                        {
                            VT100_SemigraphDef[CharNum] = false;
                        }
                        __AnsiCommand = false;
                    }
                    break;
                case 'n':
                    VT100_SemigraphNum = 2;
                    __AnsiCommand = false;
                    break;
                case 'o':
                    VT100_SemigraphNum = 3;
                    __AnsiCommand = false;
                    break;
                case '#':
                    if (__AnsiCmd.Count == 2)
                    {
                        switch (__AnsiCmd[1])
                        {
                            case '3':
                                {
                                    AnsiSetFontSize(__AnsiY, 2, true);
                                }
                                break;
                            case '4':
                                {
                                    AnsiSetFontSize(__AnsiY, 3, true);
                                }
                                break;
                            case '5':
                                {
                                    AnsiSetFontSize(__AnsiY, 0, true);
                                }
                                break;
                            case '6':
                                {
                                    AnsiSetFontSize(__AnsiY, 1, true);
                                }
                                break;
                            case '8':
                                {
                                    for (int YY = 0; YY < AnsiMaxY; YY++)
                                    {
                                        for (int XX = 0; XX < AnsiMaxX; XX++)
                                        {
                                            AnsiChar(XX, YY, 'E');
                                        }
                                    }
                                }
                                break;
                        }
                        __AnsiCommand = false;
                    }
                    break;
                case ']':
                    if (TextFileLine_i == 0x07)
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
                    __AnsiFontInverse_ = __AnsiFontInverse;
                    __AnsiFontBlink_ = __AnsiFontBlink;
                    __AnsiFontInvisible_ = __AnsiFontInvisible;
                    VT100_SemigraphDef_[0] = VT100_SemigraphDef[0];
                    VT100_SemigraphDef_[1] = VT100_SemigraphDef[1];
                    VT100_SemigraphDef_[2] = VT100_SemigraphDef[2];
                    VT100_SemigraphDef_[3] = VT100_SemigraphDef[3];
                    VT100_SemigraphNum_ = VT100_SemigraphNum;
                    __AnsiCommand = false;
                    break;
                case '8':
                    __AnsiX = __AnsiX_;
                    __AnsiY = __AnsiY_;
                    __AnsiBack = __AnsiBack_;
                    __AnsiFore = __AnsiFore_;
                    __AnsiFontBold = __AnsiFontBold_;
                    __AnsiFontInverse = __AnsiFontInverse_;
                    __AnsiFontBlink = __AnsiFontBlink_;
                    __AnsiFontInvisible = __AnsiFontInvisible_;
                    VT100_SemigraphDef[0] = VT100_SemigraphDef_[0];
                    VT100_SemigraphDef[1] = VT100_SemigraphDef_[1];
                    VT100_SemigraphDef[2] = VT100_SemigraphDef_[2];
                    VT100_SemigraphDef[3] = VT100_SemigraphDef_[3];
                    VT100_SemigraphNum = VT100_SemigraphNum_;
                    __AnsiCommand = false;
                    break;
                case 'D':
                    if (ANSIDOS)
                    {

                    }
                    else
                    {
                        __AnsiY += 1;
                        if (__AnsiY > __AnsiScrollLast)
                        {
                            AnsiScrollInit(__AnsiY - __AnsiScrollLast, AnsiScrollCommandDef.None);
                            __AnsiY = __AnsiScrollLast;
                        }
                    }
                    __AnsiCommand = false;
                    break;
                case 'M':
                    if (ANSIDOS)
                    {
                        __AnsiMusic = true;
                    }
                    else
                    {
                        __AnsiY -= 1;
                        if (__AnsiY < __AnsiScrollFirst)
                        {
                            AnsiScrollInit(__AnsiY - __AnsiScrollFirst, AnsiScrollCommandDef.None);
                            __AnsiY = __AnsiScrollFirst;
                        }
                    }
                    __AnsiCommand = false;
                    break;
                case 'E':
                    __AnsiY += 1;
                    __AnsiX = 0;
                    if (__AnsiY >= AnsiMaxY)
                    {
                        AnsiScrollInit(1, AnsiScrollCommandDef.None);
                        __AnsiY--;
                    }
                    __AnsiCommand = false;
                    break;
                case 'H':
                    if (!__AnsiTabs.Contains(__AnsiX))
                    {
                        __AnsiTabs.Add(__AnsiX);
                        __AnsiTabs.Sort();
                    }
                    __AnsiCommand = false;
                    break;
                case '\\':
                case '=':
                case '>':
                    __AnsiCommand = false;
                    break;
                case 'c':
                    AnsiTerminalReset();
                    break;
            }
        }

        void AnsiProcess_CSI_Question(string AnsiCmd_)
        {
            string[] AnsiParams = AnsiCmd_.Substring(2, AnsiCmd_.Length - 3).Split(';');
            switch (AnsiCmd_[AnsiCmd_.Length - 1])
            {
                case 'h':
                    {
                        for (int i = 0; i < AnsiParams.Length; i++)
                        {
                            switch (AnsiParams[i])
                            {
                                case "3":
                                    {
                                        for (int i_ = 0; i_ < AnsiMaxY; i_++)
                                        {
                                            for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
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
                                case "4":
                                    __AnsiSmoothScroll = true;
                                    break;
                                case "6":
                                    __AnsiOrigin = true;
                                    __AnsiX = 0;
                                    __AnsiY = __AnsiScrollFirst;
                                    break;
                                case "7":
                                    __AnsiNoWrap = false;
                                    break;
                            }
                        }
                    }
                    break;
                case 'l':
                    {
                        for (int i = 0; i < AnsiParams.Length; i++)
                        {
                            switch (AnsiParams[i])
                            {
                                case "2":
                                    __AnsiVT52 = true;
                                    __AnsiCommand = false;
                                    break;
                                case "3":
                                    {
                                        for (int i_ = 0; i_ < AnsiMaxY; i_++)
                                        {
                                            for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
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
                                case "4":
                                    __AnsiSmoothScroll = false;
                                    break;
                                case "6":
                                    __AnsiOrigin = false;
                                    __AnsiX = 0;
                                    __AnsiY = 0;
                                    break;
                                case "7":
                                    __AnsiNoWrap = true;
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        void AnsiProcess_CSI_Fixed(string AnsiCmd_)
        {
            switch (AnsiCmd_)
            {
                case "[!p":
                    AnsiTerminalReset();
                    break;
                case "[5n":
                case "[6n":
                case "[>c":
                case "[>0c":
                case "[=c":
                case "[=0c":
                case "[0x":
                case "[1x":
                    __AnsiResponse.Add(AnsiCmd_);
                    break;
                case "[s":
                    __AnsiX_ = __AnsiX;
                    __AnsiY_ = __AnsiY;
                    __AnsiBack_ = __AnsiBack;
                    __AnsiFore_ = __AnsiFore;
                    __AnsiFontBold_ = __AnsiFontBold;
                    __AnsiFontInverse_ = __AnsiFontInverse;
                    __AnsiFontBlink_ = __AnsiFontBlink;
                    __AnsiFontInvisible_ = __AnsiFontInvisible;
                    VT100_SemigraphDef_[0] = VT100_SemigraphDef[0];
                    VT100_SemigraphDef_[1] = VT100_SemigraphDef[1];
                    VT100_SemigraphDef_[2] = VT100_SemigraphDef[2];
                    VT100_SemigraphDef_[3] = VT100_SemigraphDef[3];
                    VT100_SemigraphNum_ = VT100_SemigraphNum;
                    break;
                case "[u":
                    __AnsiX = __AnsiX_;
                    __AnsiY = __AnsiY_;
                    __AnsiBack = __AnsiBack_;
                    __AnsiFore = __AnsiFore_;
                    __AnsiFontBold = __AnsiFontBold_;
                    __AnsiFontInverse = __AnsiFontInverse_;
                    __AnsiFontBlink = __AnsiFontBlink_;
                    __AnsiFontInvisible = __AnsiFontInvisible_;
                    VT100_SemigraphDef[0] = VT100_SemigraphDef_[0];
                    VT100_SemigraphDef[1] = VT100_SemigraphDef_[1];
                    VT100_SemigraphDef[2] = VT100_SemigraphDef_[2];
                    VT100_SemigraphDef[3] = VT100_SemigraphDef_[3];
                    VT100_SemigraphNum = VT100_SemigraphNum_;
                    break;
                case "[J":
                case "[0J":
                    AnsiCalcColor();
                    AnsiClearFontSize(__AnsiY + 1);
                    for (int i_ = __AnsiY + 1; i_ < AnsiMaxY; i_++)
                    {
                        for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiChar(ii_, i_, 32);
                        }
                    }
                    if (AnsiMaxY > __AnsiY)
                    {
                        if (AnsiGetFontSize(__AnsiY) > 0)
                        {
                            for (int ii_ = (__AnsiX << 1); ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiChar(ii_, __AnsiY, 32);
                            }
                        }
                        else
                        {
                            for (int ii_ = __AnsiX; ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiChar(ii_, __AnsiY, 32);
                            }
                        }
                    }
                    break;
                case "[1J":
                    AnsiCalcColor();
                    for (int i_ = 0; i_ < __AnsiY; i_++)
                    {
                        AnsiSetFontSize(i_, 0, true);
                        if (AnsiMaxY > __AnsiY)
                        {
                            for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiChar(ii_, i_, 32);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (AnsiGetFontSize(__AnsiY) > 0)
                    {
                        for (int ii_ = 0; ii_ <= (__AnsiX << 1); ii_++)
                        {
                            AnsiChar(ii_, __AnsiY, 32);
                        }
                    }
                    else
                    {
                        for (int ii_ = 0; ii_ <= __AnsiX; ii_++)
                        {
                            AnsiChar(ii_, __AnsiY, 32);
                        }
                    }
                    break;
                case "[2J":
                    AnsiCalcColor();
                    if (__AnsiTest == 2)
                    {
                        Console.Clear();
                    }
                    AnsiClearFontSize(-1);
                    for (int i_ = 0; i_ < AnsiMaxY; i_++)
                    {
                        for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiChar(ii_, i_, 32);
                        }
                    }
                    if (ANSIDOS)
                    {
                        __AnsiX = 0;
                        __AnsiY = 0;
                    }
                    break;
                case "[K":
                case "[0K":
                    if (AnsiMaxY > __AnsiY)
                    {
                        AnsiCalcColor();
                        if (AnsiGetFontSize(__AnsiY) > 0)
                        {
                            for (int ii_ = (__AnsiX << 1); ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiChar(ii_, __AnsiY, 32);
                            }
                        }
                        else
                        {
                            for (int ii_ = __AnsiX; ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiChar(ii_, __AnsiY, 32);
                            }
                        }
                    }
                    break;
                case "[1K":
                    if (AnsiMaxY > __AnsiY)
                    {
                        AnsiCalcColor();
                        if (AnsiGetFontSize(__AnsiY) > 0)
                        {
                            for (int ii_ = 0; ii_ <= (__AnsiX << 1); ii_++)
                            {
                                AnsiChar(ii_, __AnsiY, 32);
                            }
                        }
                        else
                        {
                            for (int ii_ = 0; ii_ <= __AnsiX; ii_++)
                            {
                                AnsiChar(ii_, __AnsiY, 32);
                            }
                        }
                    }
                    break;
                case "[2K":
                    if (AnsiMaxY > __AnsiY)
                    {
                        AnsiCalcColor();
                        for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiChar(ii_, __AnsiY, 32);
                        }
                    }
                    break;
                case "[c":
                case "[0c":
                    __AnsiResponse.Add("[0c");
                    break;
                default:
                    AnsiProcess_CSI(AnsiCmd_);
                    break;
            }
        }

        int AnsiProcess_Int(string Param, string AnsiCmd_)
        {
            try
            {
                return int.Parse(Param);
            }
            catch
            {
                throw new Exception("Integer error: " + AnsiCmd_);
            }
        }

        void AnsiProcess_CSI(string AnsiCmd_)
        {
            string[] AnsiParams = AnsiCmd_.Substring(1, AnsiCmd_.Length - 2).Split(';');
            switch (AnsiCmd_[AnsiCmd_.Length - 1])
            {
                case 'V':
                    if (AnsiParams.Length == 2)
                    {
                        AnsiParams = new string[] { AnsiParams[0], AnsiParams[1], "0" };
                    }
                    if (AnsiParams.Length == 1)
                    {
                        AnsiParams = new string[] { AnsiParams[0], "0", "0" };
                    }
                    if (AnsiParams[0] == "") { AnsiParams[0] = "0"; }
                    if (AnsiParams[1] == "") { AnsiParams[1] = "0"; }
                    if (AnsiParams[2] == "") { AnsiParams[2] = "0"; }
                    switch (AnsiParams[0])
                    {
                        case "0":
                            __AnsiFontSizeW = AnsiProcess_Int(AnsiParams[1], AnsiCmd_);
                            __AnsiFontSizeH = AnsiProcess_Int(AnsiParams[2], AnsiCmd_);
                            if (__AnsiFontSizeW > 9)
                            {
                                __AnsiFontSizeW = 0;
                            }
                            if (__AnsiFontSizeH > 9)
                            {
                                __AnsiFontSizeH = 0;
                            }
                            break;
                        case "1":
                            {
                                long TimeStamp = AnsiProcess_Int(AnsiParams[1], AnsiCmd_);
                                __AnsiProcessDelay = TimeStamp * __AnsiProcessDelayFactor;

                                long DelayDiff = __AnsiProcessDelay - __AnsiProcessStep;

                                if (__AnsiProcessDelayMin > __AnsiProcessDelayMax)
                                {
                                    __AnsiProcessDelayMin = DelayDiff;
                                    __AnsiProcessDelayMax = DelayDiff;
                                }
                                else
                                {
                                    __AnsiProcessDelayMin = Math.Min(__AnsiProcessDelayMin, DelayDiff);
                                    __AnsiProcessDelayMax = Math.Max(__AnsiProcessDelayMax, DelayDiff);
                                }

                            }
                            break;
                    }
                    break;
                case 'h':
                    switch (AnsiParams[0])
                    {
                        case "4":
                            __AnsiInsertMode = true;
                            break;
                        case "20":
                            __AnsiNewLineKey = true;
                            break;
                    }
                    break;
                case 'l':
                    switch (AnsiParams[0])
                    {
                        case "4":
                            __AnsiInsertMode = false;
                            break;
                        case "20":
                            {
                                __AnsiNewLineKey = false;
                            }
                            break;
                    }
                    break;
                case 'H':
                case 'f':
                    if (AnsiParams.Length == 1)
                    {
                        AnsiParams = new string[] { AnsiParams[0], "1" };
                    }
                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                    if (AnsiParams[1] == "") { AnsiParams[1] = "1"; }
                    __AnsiY = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                    __AnsiX = AnsiProcess_Int(AnsiParams[1], AnsiCmd_) - 1;
                    if (__AnsiY < 0)
                    {
                        __AnsiY = 0;
                    }
                    if (__AnsiX < 0)
                    {
                        __AnsiX = 0;
                    }

                    if (__AnsiOrigin)
                    {
                        __AnsiY += __AnsiScrollFirst;
                    }
                    if (__AnsiY >= AnsiMaxY)
                    {
                        __AnsiY = AnsiMaxY - 1;
                    }
                    break;
                case 'A':
                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                    __AnsiY -= Math.Max(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), 1);
                    if (__AnsiY < __AnsiScrollFirst)
                    {
                        __AnsiY = __AnsiScrollFirst;
                    }
                    break;
                case 'B':
                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                    __AnsiY += Math.Max(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), 1);
                    if (__AnsiY > __AnsiScrollLast)
                    {
                        __AnsiY = __AnsiScrollLast;
                    }
                    break;
                case 'C':
                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                    __AnsiX += Math.Max(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), 1);
                    if ((AnsiMaxX > 0) && (__AnsiX >= AnsiMaxX))
                    {
                        if (ANSIDOS)
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
                    __AnsiX -= Math.Max(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), 1);
                    if (__AnsiX < 0)
                    {
                        __AnsiX = 0;
                    }
                    break;

                case 'd':
                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                    __AnsiY = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                    break;
                case 'e':
                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                    __AnsiY += AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                    break;

                case 'E':
                    __AnsiX = 0;
                    __AnsiY += AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                    break;
                case 'F':
                    __AnsiX = 0;
                    __AnsiY -= AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                    break;
                case 'G':
                    __AnsiX = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                    break;
                case 'S':
                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                    AnsiScrollInit(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), AnsiScrollCommandDef.None);
                    break;
                case 'T':
                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                    AnsiScrollInit(0 - AnsiProcess_Int(AnsiParams[0], AnsiCmd_), AnsiScrollCommandDef.None);
                    break;
                case 'r':
                    if (AnsiParams.Length == 1)
                    {
                        AnsiParams = new string[] { AnsiParams[0], (AnsiMaxY + 1).ToString() };
                    }
                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                    if (AnsiParams[1] == "") { AnsiParams[1] = (AnsiMaxY + 1).ToString(); }
                    __AnsiScrollFirst = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                    __AnsiScrollLast = AnsiProcess_Int(AnsiParams[1], AnsiCmd_) - 1;
                    if (__AnsiScrollFirst < 0)
                    {
                        __AnsiScrollFirst = 0;
                    }
                    if (__AnsiScrollLast >= AnsiMaxY)
                    {
                        __AnsiScrollLast = AnsiMaxY - 1;
                    }
                    __AnsiX = 0;
                    if (__AnsiOrigin)
                    {
                        __AnsiY = __AnsiScrollFirst;
                    }
                    else
                    {
                        __AnsiY = 0;
                    }
                    break;
                case 'L':
                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                    if (ANSIDOS)
                    {

                    }
                    else
                    {
                        int T1 = __AnsiScrollFirst;
                        int T2 = __AnsiScrollLast;
                        //__AnsiX = 0;
                        __AnsiScrollFirst = __AnsiY;
                        AnsiScrollInit(0 - AnsiProcess_Int(AnsiParams[0], AnsiCmd_), AnsiScrollCommandDef.FirstLast, T1, T2, 0);
                    }
                    break;
                case 'M':
                    if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                    if (ANSIDOS)
                    {
                        __AnsiMusic = true;
                    }
                    else
                    {
                        int T1 = __AnsiScrollFirst;
                        int T2 = __AnsiScrollLast;
                        //__AnsiX = 0;
                        __AnsiScrollFirst = __AnsiY;
                        AnsiScrollInit(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), AnsiScrollCommandDef.FirstLast, T1, T2, 0);
                    }
                    break;
                case 'm':
                    {
                        AnsiProcess_CSI_m(AnsiParams);
                    }
                    break;
                case '@':
                    {
                        if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                        int InsCycle = AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                        if (__AnsiLineOccupy.Count > __AnsiY)
                        {
                            int FontSize = AnsiGetFontSize(__AnsiY);
                            while (InsCycle > 0)
                            {
                                int TempC = 0, TempB = 0, TempF = 0;
                                if (FontSize > 0)
                                {
                                    AnsiGetF(__AnsiX, __AnsiY, out TempC, out TempB, out TempF);
                                    if (__AnsiLineOccupy[__AnsiY].Count > (__AnsiX * 2 * __AnsiLineOccupyFactor))
                                    {
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * 2 * __AnsiLineOccupyFactor, FontSize - 1);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * 2 * __AnsiLineOccupyFactor, 2);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * 2 * __AnsiLineOccupyFactor, TempF);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * 2 * __AnsiLineOccupyFactor, TempB);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * 2 * __AnsiLineOccupyFactor, 32);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * 2 * __AnsiLineOccupyFactor, FontSize - 1);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * 2 * __AnsiLineOccupyFactor, 1);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * 2 * __AnsiLineOccupyFactor, TempF);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * 2 * __AnsiLineOccupyFactor, TempB);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * 2 * __AnsiLineOccupyFactor, 32);
                                    }
                                    if (__AnsiLineOccupy[__AnsiY].Count > (AnsiMaxX * __AnsiLineOccupyFactor))
                                    {
                                        __AnsiLineOccupy[__AnsiY].RemoveRange(AnsiMaxX * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor + __AnsiLineOccupyFactor);
                                    }
                                }
                                else
                                {
                                    AnsiGetF(__AnsiX, __AnsiY, out TempC, out TempB, out TempF);
                                    if (__AnsiLineOccupy[__AnsiY].Count > (__AnsiX * __AnsiLineOccupyFactor))
                                    {
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * __AnsiLineOccupyFactor, 0);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * __AnsiLineOccupyFactor, 0);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * __AnsiLineOccupyFactor, TempF);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * __AnsiLineOccupyFactor, TempB);
                                        __AnsiLineOccupy[__AnsiY].Insert(__AnsiX * __AnsiLineOccupyFactor, 32);
                                    }
                                    if (__AnsiLineOccupy[__AnsiY].Count > (AnsiMaxX * __AnsiLineOccupyFactor))
                                    {
                                        __AnsiLineOccupy[__AnsiY].RemoveRange(AnsiMaxX * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor);
                                    }
                                }
                                InsCycle--;
                            }
                            AnsiRepaintLine(__AnsiY);
                        }
                    }
                    break;
                case 'P':
                    {
                        if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                        int DelCycle = AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                        if (__AnsiLineOccupy.Count > __AnsiY)
                        {
                            while (DelCycle > 0)
                            {
                                int TempC = 0, TempB = 0, TempF = 0;
                                if (AnsiGetFontSize(__AnsiY) > 0)
                                {
                                    AnsiGetF((AnsiMaxX / 2) - 1, __AnsiY, out TempC, out TempB, out TempF);
                                    if (__AnsiLineOccupy[__AnsiY].Count > (__AnsiX * __AnsiLineOccupyFactor))
                                    {
                                        __AnsiLineOccupy[__AnsiY].RemoveRange((__AnsiX * __AnsiLineOccupyFactor), __AnsiLineOccupyFactor + __AnsiLineOccupyFactor);
                                    }
                                    if (TempC > 0)
                                    {
                                        AnsiCalcColor();
                                        AnsiCharF((AnsiMaxX / 2) - 1, __AnsiY, 32);
                                    }
                                }
                                else
                                {
                                    AnsiGetF(AnsiMaxX - 1, __AnsiY, out TempC, out TempB, out TempF);
                                    if (__AnsiLineOccupy[__AnsiY].Count > (__AnsiX * __AnsiLineOccupyFactor))
                                    {
                                        __AnsiLineOccupy[__AnsiY].RemoveRange((__AnsiX * __AnsiLineOccupyFactor), __AnsiLineOccupyFactor);
                                    }
                                    if (TempC > 0)
                                    {
                                        AnsiCalcColor();
                                        AnsiCharF(AnsiMaxX - 1, __AnsiY, 32);
                                    }
                                }
                                DelCycle--;
                            }
                            AnsiRepaintLine(__AnsiY);
                        }
                    }
                    break;
                case 'X':
                    {
                        if (AnsiParams[0] == "") { AnsiParams[0] = "1"; }
                        int DelCycle = AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                        AnsiCalcColor();
                        while (DelCycle > 0)
                        {
                            AnsiCharF(__AnsiX + DelCycle - 1, __AnsiY, 32);
                            DelCycle--;
                        }
                    }
                    break;

                case 'g':
                    {
                        switch (AnsiParams[0])
                        {
                            case "":
                            case "0":
                                if (__AnsiTabs.Contains(__AnsiX) && (__AnsiX >= 0))
                                {
                                    __AnsiTabs.Remove(__AnsiX);
                                }
                                break;
                            case "3":
                                __AnsiTabs.Clear();
                                __AnsiTabs.Add(-1);
                                break;
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

        void AnsiProcess_CSI_m(string[] AnsiParams)
        {
            for (int i_ = 0; i_ < AnsiParams.Length; i_++)
            {
                switch (AnsiParams[i_])
                {
                    case "0":
                    case "00":
                    case "":
                        __AnsiFore = -1;
                        __AnsiBack = -1;
                        __AnsiFontBold = false;
                        __AnsiFontInverse = false;
                        __AnsiFontBlink = false;
                        __AnsiFontInvisible = false;
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
                    case "5": __AnsiFontBlink = true; break;
                    case "25": __AnsiFontBlink = false; break;
                    case "7": __AnsiFontInverse = true; break;
                    case "27": __AnsiFontInverse = false; break;
                    case "8": __AnsiFontInvisible = true; break;
                    case "28": __AnsiFontInvisible = false; break;


                    case "38":
                        {
                            if (AnsiParams.Length > i_ + 2)
                            {
                                if (AnsiParams[i_ + 1] == "5")
                                {
                                    try
                                    {
                                        __AnsiFore = Color256[int.Parse(AnsiParams[i_ + 2])];
                                    }
                                    catch
                                    {
                                        __AnsiFore = -1;
                                    }
                                }
                                i_ += 2;
                            }
                        }
                        break;

                    case "48":
                        {
                            if (AnsiParams.Length > i_ + 2)
                            {
                                if (AnsiParams[i_ + 1] == "5")
                                {
                                    try
                                    {
                                        __AnsiBack = Color256[int.Parse(AnsiParams[i_ + 2])];
                                    }
                                    catch
                                    {
                                        __AnsiBack = -1;
                                    }
                                }
                                i_ += 2;
                            }
                        }
                        break;

                    default:
                        //Console.BackgroundColor = ConsoleColor.Black;
                        //Console.ForegroundColor = ConsoleColor.White;
                        //Console.WriteLine("{" + AnsiParams[i_] + "}");
                        break;
                }
            }
        }

        public void AnsiRepaintCursor()
        {
            if (AnsiGetFontSize(__AnsiY) > 0)
            {
                Screen_.SetCursorPosition(__AnsiX << 1, __AnsiY);
            }
            else
            {
                Screen_.SetCursorPosition(__AnsiX, __AnsiY);
            }
        }

        private void AnsiCharPrint(int TextFileLine_i)
        {
            if (TextFileLine_i == 127)
            {
                if (!ANSIDOS)
                {
                    return;
                }
            }

            if ((!__AnsiMusic) && (TextFileLine_i < 32) && (ANSIDOS))
            {
                switch (TextFileLine_i)
                {
                    case 13:
                    case 10:
                        break;
                    case 26:
                        if (__AnsiUseEOF)
                        {
                            __AnsiBeyondEOF = true;
                        }
                        break;
                    case 8:
                        if (ANSIPrintBackspace)
                        {
                            TextFileLine_i = DosControl[TextFileLine_i];
                        }
                        break;
                    case 9:
                    case 11:
                    case 14:
                    case 15:
                        if (ANSIDOS)
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
                if (__AnsiVT52)
                {
                    if (VT52_SemigraphDef)
                    {
                        if ((TextFileLine_i >= 95) && (TextFileLine_i <= 126))
                        {
                            TextFileLine_i = VT52_SemigraphChars[TextFileLine_i - 95];
                        }
                    }
                }
                else
                {
                    if (VT100_SemigraphDef[VT100_SemigraphNum])
                    {
                        if ((TextFileLine_i >= 95) && (TextFileLine_i <= 126))
                        {
                            TextFileLine_i = VT100_SemigraphChars[TextFileLine_i - 95];
                        }
                    }
                }

                if (!__AnsiMusic)
                {
                    if (ANSIDOS)
                    {
                        AnsiCharFI(__AnsiX, __AnsiY, TextFileLine_i, __AnsiBackWork, __AnsiForeWork);
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
                                if ((AnsiMaxY > 0) && (__AnsiY > __AnsiScrollLast))
                                {
                                    AnsiScrollInit(__AnsiY - __AnsiScrollLast, AnsiScrollCommandDef.None);
                                    __AnsiY = __AnsiScrollLast;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool CharNoScroll = true;
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

                                if ((AnsiMaxY > 0) && (__AnsiY > __AnsiScrollLast))
                                {
                                    AnsiScrollInit(__AnsiY - __AnsiScrollLast, AnsiScrollCommandDef.Char, TextFileLine_i, __AnsiBackWork, __AnsiForeWork);
                                    __AnsiY = __AnsiScrollLast;
                                    CharNoScroll = false;
                                }
                            }
                        }
                        if (CharNoScroll)
                        {
                            AnsiCharFI(__AnsiX, __AnsiY, TextFileLine_i, __AnsiBackWork, __AnsiForeWork);
                            __AnsiX++;
                        }
                    }
                }
            }
            else
            {
                if (__AnsiMusic)
                {
                    switch (TextFileLine_i)
                    {
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
                                if (!ANSIPrintBackspace)
                                {
                                    if (__AnsiX == AnsiMaxX)
                                    {
                                        __AnsiX--;
                                    }
                                    if (__AnsiX > 0)
                                    {
                                        __AnsiX--;
                                    }
                                }
                            }
                            break;
                        case 9:
                            {
                                if (!ANSIDOS)
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
                                        if (__AnsiX >= (AnsiMaxX / 2))
                                        {
                                            __AnsiX = (AnsiMaxX / 2) - 1;
                                        }
                                    }
                                    else
                                    {
                                        if (__AnsiX >= AnsiMaxX)
                                        {
                                            __AnsiX = AnsiMaxX - 1;
                                        }
                                    }
                                }
                            }
                            break;
                        case 13:
                            {
                                switch (ANSI_CR)
                                {
                                    case 0:
                                        __AnsiX = 0;
                                        break;
                                    case 1:
                                        __AnsiX = 0;
                                        if (__AnsiY == __AnsiScrollLast)
                                        {
                                            AnsiScrollInit(1, AnsiScrollCommandDef.None);
                                        }
                                        else
                                        {
                                            __AnsiY++;
                                        }
                                        break;
                                }
                            }
                            break;
                        case 10:
                            {
                                switch (ANSI_LF)
                                {
                                    case 0:
                                        if (__AnsiY == __AnsiScrollLast)
                                        {
                                            AnsiScrollInit(1, AnsiScrollCommandDef.None);
                                        }
                                        else
                                        {
                                            __AnsiY++;
                                        }
                                        break;
                                    case 1:
                                        __AnsiX = 0;
                                        if (__AnsiY == __AnsiScrollLast)
                                        {
                                            AnsiScrollInit(1, AnsiScrollCommandDef.None);
                                        }
                                        else
                                        {
                                            __AnsiY++;
                                        }
                                        break;
                                }
                            }
                            break;
                        case 11:
                            if (!ANSIDOS)
                            {
                                __AnsiY++;
                                if (AnsiMaxY > 0)
                                {
                                    if (__AnsiY > __AnsiScrollLast)
                                    {
                                        AnsiScrollInit(__AnsiY - __AnsiScrollLast, AnsiScrollCommandDef.None);
                                        __AnsiY = __AnsiScrollLast;
                                    }
                                }
                            }
                            break;
                        case 14:
                            if (!ANSIDOS)
                            {
                                VT100_SemigraphNum = 1;
                            }
                            break;
                        case 15:
                            if (!ANSIDOS)
                            {
                                VT100_SemigraphNum = 0;
                            }
                            break;
                    }
                }
            }
        }


    }
}
