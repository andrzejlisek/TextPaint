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
                        if ((__AnsiCmd[1] == '0') || (__AnsiCmd[1] == '2'))
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
                case 'n': // LS2
                    VT100_SemigraphNum = 2;
                    __AnsiCommand = false;
                    break;
                case 'o': // LS3
                    VT100_SemigraphNum = 3;
                    __AnsiCommand = false;
                    break;
                case '#':
                    if (__AnsiCmd.Count == 2)
                    {
                        switch (__AnsiCmd[1])
                        {
                            case '3': // DECDHL
                                {
                                    AnsiSetFontSize(__AnsiY, 2, true);
                                }
                                break;
                            case '4': // DECDHL
                                {
                                    AnsiSetFontSize(__AnsiY, 3, true);
                                }
                                break;
                            case '5': // DECSWL
                                {
                                    AnsiSetFontSize(__AnsiY, 0, true);
                                }
                                break;
                            case '6': // DECDWL
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
                case 'N': // SS2 - not implemented
                    __AnsiCommand = false;
                    break;
                case 'O': // SS3 - not implemented
                    __AnsiCommand = false;
                    break;
                case '7': // DECSC
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
                case '8': // DECRC
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
                case 'D': // IND
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
                case 'M': // RI
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
                case 'E': // NEL
                    __AnsiY += 1;
                    __AnsiX = AnsiProcessGetXMin(true);
                    if (__AnsiY >= AnsiMaxY)
                    {
                        AnsiScrollInit(1, AnsiScrollCommandDef.None);
                        __AnsiY--;
                    }
                    __AnsiCommand = false;
                    break;
                case 'H': // HTS
                    if (!__AnsiTabs.Contains(__AnsiX))
                    {
                        __AnsiTabs.Add(__AnsiX);
                        __AnsiTabs.Sort();
                    }
                    __AnsiCommand = false;
                    break;
                case 'P': // DCS
                    __AnsiDCS = "";
                    __AnsiDCS_ = true;
                    __AnsiCommand = false;
                    break;
                case '=': // DECKPAM
                case '>': // DECKPNM
                    break;
                case '\\': // ST
                    if (__AnsiDCS_)
                    {
                        __AnsiResponse.Add(__AnsiDCS);
                        __AnsiDCS = "";
                        __AnsiDCS_ = false;
                    }
                    __AnsiCommand = false;
                    break;
                case 'c': // RIS
                    AnsiTerminalReset();
                    break;
                case '6': // DECBI
                    if (__AnsiLineOccupy.Count > __AnsiY)
                    {
                        int FontSize = AnsiGetFontSize(__AnsiY);
                        int TempC = 0, TempB = 0, TempF = 0;
                        if (FontSize > 0)
                        {
                            AnsiGetF(__AnsiX, __AnsiY, out TempC, out TempB, out TempF);
                            if (__AnsiLineOccupy[__AnsiY].Count >= (AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor))
                            {
                                __AnsiLineOccupy[__AnsiY].RemoveRange((AnsiProcessGetXMax(false) - 1) * 2 * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor + __AnsiLineOccupyFactor);
                            }
                            if (__AnsiLineOccupy[__AnsiY].Count > (__AnsiX * 2 * __AnsiLineOccupyFactor))
                            {
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, FontSize - 1);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, 2);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, TempF);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, TempB);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, 32);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, FontSize - 1);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, 1);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, TempF);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, TempB);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, 32);
                            }
                        }
                        else
                        {
                            AnsiGetF(AnsiProcessGetXMin(false), __AnsiY, out TempC, out TempB, out TempF);
                            if (__AnsiLineOccupy[__AnsiY].Count >= (AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor))
                            {
                                __AnsiLineOccupy[__AnsiY].RemoveRange((AnsiProcessGetXMax(false) - 1) * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor);
                            }
                            if (__AnsiLineOccupy[__AnsiY].Count > (AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor))
                            {
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor, 0);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor, 0);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor, TempF);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor, TempB);
                                __AnsiLineOccupy[__AnsiY].Insert(AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor, 32);
                            }
                        }
                        AnsiRepaintLine(__AnsiY);
                    }
                    __AnsiCommand = false;
                    break;
                case '9': // DECFI
                    if (__AnsiLineOccupy.Count > __AnsiY)
                    {
                        int TempC = 0, TempB = 0, TempF = 0;
                        if (AnsiGetFontSize(__AnsiY) > 0)
                        {
                            AnsiGetF((AnsiProcessGetXMax(false) / 2) - 1, __AnsiY, out TempC, out TempB, out TempF);
                            if (__AnsiLineOccupy[__AnsiY].Count >= ((AnsiProcessGetXMax(false) / 2) * __AnsiLineOccupyFactor))
                            {
                                __AnsiLineOccupy[__AnsiY].Insert(((AnsiProcessGetXMax(false) / 2)) * __AnsiLineOccupyFactor, 0);
                                __AnsiLineOccupy[__AnsiY].Insert(((AnsiProcessGetXMax(false) / 2)) * __AnsiLineOccupyFactor, 0);
                                __AnsiLineOccupy[__AnsiY].Insert(((AnsiProcessGetXMax(false) / 2)) * __AnsiLineOccupyFactor, TempF);
                                __AnsiLineOccupy[__AnsiY].Insert(((AnsiProcessGetXMax(false) / 2)) * __AnsiLineOccupyFactor, TempB);
                                __AnsiLineOccupy[__AnsiY].Insert(((AnsiProcessGetXMax(false) / 2)) * __AnsiLineOccupyFactor, 32);
                            }
                            if (__AnsiLineOccupy[__AnsiY].Count > (AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor))
                            {
                                __AnsiLineOccupy[__AnsiY].RemoveRange((AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor), __AnsiLineOccupyFactor + __AnsiLineOccupyFactor);
                            }
                            if (TempC > 0)
                            {
                                AnsiCalcColor();
                                AnsiCharF((AnsiProcessGetXMax(false) / 2) - 1, __AnsiY, 32);
                            }
                        }
                        else
                        {
                            AnsiGetF(AnsiProcessGetXMax(false) - 1, __AnsiY, out TempC, out TempB, out TempF);
                            if (__AnsiLineOccupy[__AnsiY].Count >= (AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor))
                            {
                                __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempF);
                                __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempB);
                                __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 32);
                            }
                            if (__AnsiLineOccupy[__AnsiY].Count > (AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor))
                            {
                                __AnsiLineOccupy[__AnsiY].RemoveRange((AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor), __AnsiLineOccupyFactor);
                            }
                            if (TempC > 0)
                            {
                                AnsiCalcColor();
                                AnsiCharF(AnsiProcessGetXMax(false) - 1, __AnsiY, 32);
                            }
                        }
                        AnsiRepaintLine(__AnsiY);
                    }
                    break;
            }
            if (__AnsiTestCmd)
            {
                if (!__AnsiCommand)
                {
                    Console.WriteLine("ANSI command: " + TextWork.IntToStr(__AnsiCmd));
                }
            }
        }

        void AnsiProcess_CSI_Question(string AnsiCmd_)
        {
            string[] AnsiParams = AnsiCmd_.Substring(2, AnsiCmd_.Length - 3).Split(';');
            switch (AnsiCmd_[AnsiCmd_.Length - 1])
            {
                case 'h': // DECSET
                    {
                        for (int i = 0; i < AnsiParams.Length; i++)
                        {
                            switch (AnsiParams[i])
                            {
                                case "3": // DECSET / DECCOLM
                                    {
                                        AnsiClearFontSize(-1);
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
                                case "4": // DECSET / DECSCLM
                                    __AnsiSmoothScroll = true;
                                    break;
                                case "6": // DECSET / DECOM
                                    __AnsiOrigin = true;
                                    __AnsiX = 0;
                                    __AnsiY = __AnsiScrollFirst;
                                    break;
                                case "7": // DECSET / DECCOLM
                                    __AnsiNoWrap = false;
                                    break;
                                case "69": // DECSET / DECLRMM
                                    __AnsiMarginLeftRight = true;
                                    break;
                            }
                        }
                    }
                    break;
                case 'l': // DECRST
                    {
                        for (int i = 0; i < AnsiParams.Length; i++)
                        {
                            switch (AnsiParams[i])
                            {
                                case "2": // DECRST / DECANM
                                    __AnsiVT52 = true;
                                    __AnsiCommand = false;
                                    break;
                                case "3": // DECRST / DECCOLM
                                    {
                                        AnsiClearFontSize(-1);
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
                                case "4": // DECRST / DECSCLM
                                    __AnsiSmoothScroll = false;
                                    break;
                                case "6": // DECRST / DECOM
                                    __AnsiOrigin = false;
                                    __AnsiX = 0;
                                    __AnsiY = 0;
                                    break;
                                case "7": // DECRST / DECAWM
                                    __AnsiNoWrap = true;
                                    break;
                                case "69": // DECRST / DECLRMM
                                    __AnsiMarginLeftRight = false;
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
                case "[!p": // DECSTR
                    AnsiTerminalReset();
                    break;
                case "[5n": // DSR
                case "[6n": // DSR / CPR
                case "[>c": // Secondary DA
                case "[>0c": // Secondary DA
                case "[=c": // Tertiary DA
                case "[=0c": // Tertiary DA
                case "[0x": // DECREQTPARM
                case "[1x": // DECREQTPARM
                    __AnsiResponse.Add(AnsiCmd_);
                    break;
                case "[s": // SCOSC
                    if (!__AnsiMarginLeftRight)
                    {
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
                    }
                    else
                    {
                        __AnsiMarginLeft = 0;
                        __AnsiMarginRight = AnsiMaxX;
                    }
                    break;
                case "[u": // SCORC
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
                case "[J": // ED
                case "[0J": // ED
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
                case "[1J": // ED
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
                case "[2J": // ED
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
                case "[K": // EL
                case "[0K": // EL
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
                case "[1K": // EL
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
                case "[2K": // EL
                    if (AnsiMaxY > __AnsiY)
                    {
                        AnsiCalcColor();
                        for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiChar(ii_, __AnsiY, 32);
                        }
                    }
                    break;
                case "[c": // Primary DA
                case "[0c": // Primary DA
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
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "0"; }
                    if ("".Equals(AnsiParams[1])) { AnsiParams[1] = "0"; }
                    if ("".Equals(AnsiParams[2])) { AnsiParams[2] = "0"; }
                    switch (AnsiParams[0])
                    {
                        case "0":
                            __AnsiFontSizeW = AnsiProcess_Int(AnsiParams[1], AnsiCmd_);
                            __AnsiFontSizeH = AnsiProcess_Int(AnsiParams[2], AnsiCmd_);
                            if (__AnsiFontSizeW > FontMaxSizeCode)
                            {
                                __AnsiFontSizeW = 0;
                            }
                            if (__AnsiFontSizeH > FontMaxSizeCode)
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
                case 'h': // SM
                    switch (AnsiParams[0])
                    {
                        case "4": // SM / IRM
                            __AnsiInsertMode = true;
                            break;
                        case "20": // SM / LNM
                            __AnsiNewLineKey = true;
                            break;
                    }
                    break;
                case 'l': // RM
                    switch (AnsiParams[0])
                    {
                        case "4": // RM / IRM
                            __AnsiInsertMode = false;
                            break;
                        case "20": // RM / LNM
                            {
                                __AnsiNewLineKey = false;
                            }
                            break;
                    }
                    break;
                case 'H': // CUP
                case 'f': // HVP
                    if (AnsiParams.Length == 1)
                    {
                        AnsiParams = new string[] { AnsiParams[0], "1" };
                    }
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    if ("".Equals(AnsiParams[1])) { AnsiParams[1] = "1"; }
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
                    __AnsiX += AnsiProcessGetXMin(true);
                    if (__AnsiY >= AnsiMaxY)
                    {
                        __AnsiY = AnsiMaxY - 1;
                    }
                    if (__AnsiOrigin)
                    {
                        if (__AnsiMarginLeftRight && (__AnsiX >= __AnsiMarginRight))
                        {
                            __AnsiX = __AnsiMarginRight - 1;
                        }
                        if (__AnsiY > __AnsiScrollLast)
                        {
                            __AnsiY = __AnsiScrollLast;
                        }
                    }

                    break;
                case 'A': // CUU // SR
                    if (AnsiCmd_[AnsiCmd_.Length - 2] == ' ')
                    {
                        // SR
                        if (" ".Equals(AnsiParams[0])) { AnsiParams[0] = "1 "; }
                        if ("0 ".Equals(AnsiParams[0])) { AnsiParams[0] = "1 "; }
                        AnsiScrollColumns(0 - AnsiProcess_Int(AnsiParams[0].Substring(0, AnsiParams[0].Length - 1), AnsiCmd_));
                    }
                    else
                    {
                        // CUU
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        __AnsiY -= Math.Max(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), 1);
                        if (__AnsiY < __AnsiScrollFirst)
                        {
                            __AnsiY = __AnsiScrollFirst;
                        }
                    }
                    break;
                case 'B': // CUD
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    __AnsiY += Math.Max(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), 1);
                    if (__AnsiY > __AnsiScrollLast)
                    {
                        __AnsiY = __AnsiScrollLast;
                    }
                    break;
                case 'C': // CUF
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    __AnsiX += Math.Max(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), 1);
                    if ((AnsiMaxX > 0) && (__AnsiX >= AnsiProcessGetXMax(false)))
                    {
                        if (ANSIDOS)
                        {
                            __AnsiY++;
                            __AnsiX = __AnsiX - AnsiMaxX;
                        }
                        else
                        {
                            __AnsiX = AnsiProcessGetXMax(false) - 1;
                        }
                    }
                    break;
                case 'D': // CUB
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    if (AnsiMaxX > 0)
                    {
                        if (__AnsiX >= AnsiMaxX)
                        {
                            __AnsiX = AnsiMaxX - 1;
                        }
                    }
                    __AnsiX -= Math.Max(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), 1);
                    if (__AnsiX < AnsiProcessGetXMin(false))
                    {
                        __AnsiX = AnsiProcessGetXMin(false);
                    }
                    break;

                case 'd': // VPA
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    __AnsiY = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                    if (__AnsiOrigin)
                    {
                        __AnsiY += __AnsiScrollFirst;
                    }
                    break;
                case 'e': // VPR
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    __AnsiY += AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                    break;

                case '`': // HPA
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    __AnsiX = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1 + AnsiProcessGetXMin(true);
                    break;
                case 'a': // HPR
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    __AnsiX += AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                    break;

                case 'E': // CNL
                    __AnsiX = AnsiProcessGetXMin(false);
                    if ("0".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    __AnsiY += AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                    if (__AnsiY > __AnsiScrollLast)
                    {
                        __AnsiY = __AnsiScrollLast;
                    }
                    break;
                case 'F': // CPL
                    __AnsiX = AnsiProcessGetXMin(false);
                    if ("0".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    __AnsiY -= AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                    if (__AnsiY < __AnsiScrollFirst)
                    {
                        __AnsiY = __AnsiScrollFirst;
                    }
                    break;
                case 'G': // CHA
                    if ("0".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    __AnsiX = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                    if (__AnsiOrigin && __AnsiMarginLeftRight)
                    {
                        __AnsiX += __AnsiMarginLeft;
                    }
                    break;
                case 'S': // SU
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    AnsiScrollInit(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), AnsiScrollCommandDef.None);
                    break;
                case 'T': // SD
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    AnsiScrollInit(0 - AnsiProcess_Int(AnsiParams[0], AnsiCmd_), AnsiScrollCommandDef.None);
                    break;
                case 'r': // DECSTBM

                    // DECCARA - not implemented
                    if (AnsiCmd_.EndsWith("$r"))
                    {
                    }
                    else
                    {

                        if (AnsiParams.Length == 1)
                        {
                            AnsiParams = new string[] { AnsiParams[0], (AnsiMaxY + 1).ToString() };
                        }
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "0"; }
                        if ("".Equals(AnsiParams[1])) { AnsiParams[1] = "0"; }
                        __AnsiScrollFirst = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                        __AnsiScrollLast = AnsiProcess_Int(AnsiParams[1], AnsiCmd_) - 1;
                        if (__AnsiScrollFirst > __AnsiScrollLast)
                        {
                            __AnsiScrollFirst = 0;
                            __AnsiScrollLast = AnsiMaxY - 1;
                        }
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
                    }
                    break;
                case 's': // DECSLRM
                    if (AnsiParams.Length == 1)
                    {
                        AnsiParams = new string[] { AnsiParams[0], (AnsiMaxX + 1).ToString() };
                    }
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "0"; }
                    if ("".Equals(AnsiParams[1])) { AnsiParams[1] = "0"; }
                    if (__AnsiMarginLeftRight)
                    {
                        __AnsiMarginLeft = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                        __AnsiMarginRight = AnsiProcess_Int(AnsiParams[1], AnsiCmd_);
                        if ((__AnsiMarginLeft == -1) && (__AnsiMarginRight == 0))
                        {
                            __AnsiMarginLeft = 0;
                            __AnsiMarginRight = AnsiMaxX;
                        }
                        if (__AnsiMarginLeft >= __AnsiMarginRight)
                        {
                            __AnsiMarginLeft = 0;
                            __AnsiMarginRight = AnsiMaxX;
                        }
                        if (__AnsiMarginLeft < 0)
                        {
                            __AnsiMarginLeft = 0;
                        }
                        if (__AnsiMarginRight > AnsiMaxX)
                        {
                            __AnsiMarginRight = AnsiMaxX;
                        }
                    }
                    break;
                case 'L': // IL
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    if (ANSIDOS)
                    {

                    }
                    else
                    {
                        if ((__AnsiY >= __AnsiScrollFirst) && (__AnsiY <= __AnsiScrollLast))
                        {
                            int T1 = __AnsiScrollFirst;
                            int T2 = __AnsiScrollLast;
                            //__AnsiX = 0;
                            __AnsiScrollFirst = __AnsiY;
                            AnsiScrollInit(0 - AnsiProcess_Int(AnsiParams[0], AnsiCmd_), AnsiScrollCommandDef.FirstLast, T1, T2, 0);
                        }
                    }
                    break;
                case 'M': // DL
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    if (ANSIDOS)
                    {
                        __AnsiMusic = true;
                    }
                    else
                    {
                        if ((__AnsiY >= __AnsiScrollFirst) && (__AnsiY <= __AnsiScrollLast))
                        {
                            int T1 = __AnsiScrollFirst;
                            int T2 = __AnsiScrollLast;
                            //__AnsiX = 0;
                            __AnsiScrollFirst = __AnsiY;
                            AnsiScrollInit(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), AnsiScrollCommandDef.FirstLast, T1, T2, 0);
                        }
                    }
                    break;
                case 'm': // SGR
                    {
                        AnsiProcess_CSI_m(AnsiParams);
                    }
                    break;
                case '@': // ICH // SL
                    if (AnsiCmd_[AnsiCmd_.Length - 2] == ' ')
                    {
                        // SL
                        if (" ".Equals(AnsiParams[0])) { AnsiParams[0] = "1 "; }
                        if ("0 ".Equals(AnsiParams[0])) { AnsiParams[0] = "1 "; }
                        AnsiScrollColumns(AnsiProcess_Int(AnsiParams[0].Substring(0, AnsiParams[0].Length - 1), AnsiCmd_));
                    }
                    else
                    {
                        // ICH
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
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
                                    if (__AnsiLineOccupy[__AnsiY].Count > (AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor))
                                    {
                                        __AnsiLineOccupy[__AnsiY].RemoveRange(AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor + __AnsiLineOccupyFactor);
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
                                    if (__AnsiLineOccupy[__AnsiY].Count > (AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor))
                                    {
                                        __AnsiLineOccupy[__AnsiY].RemoveRange(AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor);
                                    }
                                }
                                InsCycle--;
                            }
                            AnsiRepaintLine(__AnsiY);
                        }
                    }
                    break;
                case 'P': // DCH
                    {
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        int DelCycle = AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                        if (__AnsiLineOccupy.Count > __AnsiY)
                        {
                            while (DelCycle > 0)
                            {
                                int TempC = 0, TempB = 0, TempF = 0;
                                if (AnsiGetFontSize(__AnsiY) > 0)
                                {
                                    AnsiGetF((AnsiProcessGetXMax(false)) - 1, __AnsiY, out TempC, out TempB, out TempF);
                                    if (__AnsiLineOccupy[__AnsiY].Count >= ((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor))
                                    {
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempF);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempB);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 32);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempF);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempB);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 32);
                                    }
                                    if (__AnsiLineOccupy[__AnsiY].Count > (__AnsiX * __AnsiLineOccupyFactor))
                                    {
                                        __AnsiLineOccupy[__AnsiY].RemoveRange((__AnsiX * __AnsiLineOccupyFactor), __AnsiLineOccupyFactor + __AnsiLineOccupyFactor);
                                    }
                                    if (TempC > 0)
                                    {
                                        AnsiCalcColor();
                                        AnsiCharF((AnsiProcessGetXMax(false) / 2) - 1, __AnsiY, 32);
                                    }
                                }
                                else
                                {
                                    AnsiGetF(AnsiProcessGetXMax(false) - 1, __AnsiY, out TempC, out TempB, out TempF);
                                    if (__AnsiLineOccupy[__AnsiY].Count >= ((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor))
                                    {
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempF);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempB);
                                        __AnsiLineOccupy[__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 32);
                                    }
                                    if (__AnsiLineOccupy[__AnsiY].Count > (__AnsiX * __AnsiLineOccupyFactor))
                                    {
                                        __AnsiLineOccupy[__AnsiY].RemoveRange((__AnsiX * __AnsiLineOccupyFactor), __AnsiLineOccupyFactor);
                                    }
                                    if (TempC > 0)
                                    {
                                        AnsiCalcColor();
                                        AnsiCharF(AnsiProcessGetXMax(false) - 1, __AnsiY, 32);
                                    }
                                }
                                DelCycle--;
                            }
                            AnsiRepaintLine(__AnsiY);
                        }
                    }
                    break;
                case 'X': // ECH
                    {
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        int DelCycle = AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                        AnsiCalcColor();
                        while (DelCycle > 0)
                        {
                            AnsiCharF(__AnsiX + DelCycle - 1, __AnsiY, 32);
                            DelCycle--;
                        }
                    }
                    break;

                case 'g': // TBC
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

                case 'I': // CHT
                    {
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        if ("0".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        AnsiDoTab(AnsiProcess_Int(AnsiParams[0], AnsiCmd_));
                    }
                    break;
                case 'Z': // CBT
                    {
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        if ("0".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        AnsiDoTab(0 - AnsiProcess_Int(AnsiParams[0], AnsiCmd_));
                    }
                    break;
                case 'b': // REP
                    {
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        if ("0".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        if (AnsiCharPrintLast >= 0)
                        {
                            AnsiCharPrintRepeater = AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
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
                                if ("5".Equals(AnsiParams[i_ + 1]))
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
                                if ("5".Equals(AnsiParams[i_ + 1]))
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

        int AnsiProcessGetXMin(bool Origin)
        {
            if (((!Origin) || __AnsiOrigin) && __AnsiMarginLeftRight)
            {
                return __AnsiMarginLeft;
            }
            else
            {
                return 0;
            }
        }

        int AnsiProcessGetXMax(bool Origin)
        {
            if (((!Origin) || __AnsiOrigin) && __AnsiMarginLeftRight)
            {
                return __AnsiMarginRight;
            }
            else
            {
                return AnsiMaxX;
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

        private int AnsiCharPrintLast = -1;
        private int AnsiCharPrintRepeater = 0;

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
                    case 26:
                        break;
                    case 8:
                        if (ANSIPrintBackspace)
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
                        AnsiCharPrintLast = TextFileLine_i;
                        AnsiCharFI(__AnsiX, __AnsiY, TextFileLine_i, __AnsiBackWork, __AnsiForeWork);
                        __AnsiX++;
                        if ((AnsiMaxX > 0) && (__AnsiX == AnsiProcessGetXMax(true)))
                        {
                            if (__AnsiNoWrap)
                            {
                                __AnsiX--;
                            }
                            else
                            {
                                __AnsiX = AnsiProcessGetXMin(true);
                                __AnsiY++;
                                if ((AnsiMaxY > 0) && (__AnsiY > __AnsiScrollLast))
                                {
                                    int L = __AnsiY - __AnsiScrollLast;
                                    __AnsiY = __AnsiScrollLast;
                                    AnsiScrollInit(L, AnsiScrollCommandDef.None);
                                }
                            }
                        }
                    }
                    else
                    {
                        bool CharNoScroll = true;
                        if ((AnsiMaxX > 0) && (__AnsiX == AnsiProcessGetXMax(true)))
                        {
                            if (__AnsiNoWrap)
                            {
                                __AnsiX--;
                            }
                            else
                            {
                                __AnsiX = AnsiProcessGetXMin(true);
                                __AnsiY++;

                                if ((AnsiMaxY > 0) && (__AnsiY > __AnsiScrollLast))
                                {
                                    int L = __AnsiY - __AnsiScrollLast;
                                    __AnsiY = __AnsiScrollLast;
                                    AnsiScrollInit(L, AnsiScrollCommandDef.Char, TextFileLine_i, __AnsiBackWork, __AnsiForeWork);
                                    CharNoScroll = false;
                                }
                            }
                        }
                        if (CharNoScroll)
                        {
                            AnsiCharPrintLast = TextFileLine_i;
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
                        case 7:
                            if (!__AnsiCommand)
                            {
                                Screen_.Bell();
                            }
                            break;
                        case 8:
                            {
                                if (!ANSIPrintBackspace)
                                {
                                    if (__AnsiX == AnsiMaxX)
                                    {
                                        __AnsiX--;
                                    }
                                    if (__AnsiX > AnsiProcessGetXMin(true))
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
                                    AnsiDoTab(1);
                                }
                            }
                            break;
                        case 13:
                            {
                                switch (ANSI_CR)
                                {
                                    case 0:
                                        __AnsiX = AnsiProcessGetXMin(false);
                                        break;
                                    case 1:
                                        __AnsiX = AnsiProcessGetXMin(false);
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
                        case 12:
                            if (!ANSIDOS)
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
                                        __AnsiX = AnsiProcessGetXMin(true);
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
                                        __AnsiX = AnsiProcessGetXMin(true);
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
                        case 26:
                            if (__AnsiUseEOF)
                            {
                                __AnsiBeyondEOF = true;
                            }
                            break;
                    }
                }
            }
        }


    }
}
