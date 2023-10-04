using System;
namespace TextPaint
{
    public partial class CoreAnsi
    {
        void SetProcessDelay(long TimeStamp)
        {
            AnsiState_.__AnsiProcessDelay = TimeStamp * __AnsiProcessDelayFactor;

            long DelayDiff = AnsiState_.__AnsiProcessDelay - AnsiState_.__AnsiProcessStep;

            if (AnsiState_.__AnsiProcessDelayMin > AnsiState_.__AnsiProcessDelayMax)
            {
                AnsiState_.__AnsiProcessDelayMin = DelayDiff;
                AnsiState_.__AnsiProcessDelayMax = DelayDiff;
            }
            else
            {
                AnsiState_.__AnsiProcessDelayMin = Math.Min(AnsiState_.__AnsiProcessDelayMin, DelayDiff);
                AnsiState_.__AnsiProcessDelayMax = Math.Max(AnsiState_.__AnsiProcessDelayMax, DelayDiff);
            }
        }

        void AnsiProcess_VT52()
        {
            switch (AnsiState_.__AnsiCmd[0])
            {
                case '<':
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiVT52 = false;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '=':
                    __AnsiResponse.Add("NumpadKey_1");
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '>':
                    __AnsiResponse.Add("NumpadKey_0");
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'A':
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiY -= 1;
                    if (AnsiState_.__AnsiY < 0)
                    {
                        AnsiState_.__AnsiY = 0;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'B':
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiY += 1;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'C':
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiX += 1;
                    if ((AnsiMaxX > 0) && (AnsiState_.__AnsiX >= AnsiMaxX))
                    {
                        if (ANSIDOS_)
                        {
                            AnsiState_.__AnsiY++;
                            AnsiState_.__AnsiX = AnsiState_.__AnsiX - AnsiMaxX;
                        }
                        else
                        {
                            AnsiState_.__AnsiX = AnsiMaxX - 1;
                        }
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'D':
                    AnsiState_.__AnsiWrapFlag = false;
                    if (AnsiMaxX > 0)
                    {
                        if (AnsiState_.__AnsiX >= AnsiMaxX)
                        {
                            AnsiState_.__AnsiX = AnsiMaxX - 1;
                        }
                    }
                    AnsiState_.__AnsiX -= 1;
                    if (AnsiState_.__AnsiX < 0)
                    {
                        AnsiState_.__AnsiX = 0;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'E':
                case 'J':
                    AnsiState_.__AnsiWrapFlag = false;
                    if (AnsiState_.__AnsiCmd[0] == 'E')
                    {
                        AnsiState_.__AnsiX = 0;
                        AnsiState_.__AnsiY = 0;
                    }
                    for (int i_ = AnsiState_.__AnsiY + 1; i_ < AnsiMaxY; i_++)
                    {
                        for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiChar(ii_, i_, 32);
                        }
                    }
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        for (int ii_ = AnsiState_.__AnsiX; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiChar(ii_, AnsiState_.__AnsiY, 32);
                        }
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'F':
                    AnsiState_.VT52_SemigraphDef = true;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'G':
                    AnsiState_.VT52_SemigraphDef = false;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'H':
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiX = 0;
                    AnsiState_.__AnsiY = 0;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'I':
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiY -= 1;
                    if (AnsiState_.__AnsiY < AnsiState_.__AnsiScrollFirst)
                    {
                        AnsiScrollInit(AnsiState_.__AnsiY - AnsiState_.__AnsiScrollFirst, AnsiState.AnsiScrollCommandDef.None);
                        AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'K':
                    AnsiState_.__AnsiWrapFlag = false;
                    for (int ii_ = AnsiState_.__AnsiX; ii_ < AnsiMaxX; ii_++)
                    {
                        AnsiChar(ii_, AnsiState_.__AnsiY, 32);
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'Y':
                    if (AnsiState_.__AnsiCmd.Count == 3)
                    {
                        AnsiState_.__AnsiWrapFlag = false;
                        AnsiState_.__AnsiY = AnsiState_.__AnsiCmd[1] - 32;
                        AnsiState_.__AnsiX = AnsiState_.__AnsiCmd[2] - 32;
                        AnsiState_.__AnsiCommand = false;
                    }
                    break;
                case 'Z':
                    __AnsiResponse.Add("VT52:Z");
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '[':
                    {
                        switch (AnsiState_.__AnsiCmd[AnsiState_.__AnsiCmd.Count - 1])
                        {
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                            case ';':
                            case '[':
                                break;
                            case 'V':
                                {
                                    string AnsiCmd_ = TextWork.IntToStr(AnsiState_.__AnsiCmd);
                                    string[] AnsiParams = AnsiCmd_.Substring(1, AnsiCmd_.Length - 2).Split(';');
                                    if (AnsiCmd_.Length >= 2)
                                    {
                                        switch (AnsiParams[0])
                                        {
                                            case "1":
                                                {
                                                    SetProcessDelay(AnsiProcess_Int0(AnsiParams[1], AnsiCmd_));
                                                }
                                                break;
                                        }
                                    }
                                }
                                AnsiState_.__AnsiCommand = false;
                                break;
                            default:
                                AnsiState_.__AnsiCommand = false;
                                break;
                        }
                    }
                    break;
                case 'b':
                case 'c':
                    if (AnsiState_.__AnsiCmd.Count == 2)
                    {
                        AnsiState_.__AnsiCommand = false;
                    }
                    break;
                case 'd':
                    AnsiState_.__AnsiWrapFlag = false;
                    for (int i_ = 0; i_ < AnsiState_.__AnsiY; i_++)
                    {
                        if (AnsiMaxY > AnsiState_.__AnsiY)
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
                    for (int ii_ = 0; ii_ <= AnsiState_.__AnsiX; ii_++)
                    {
                        AnsiChar(ii_, AnsiState_.__AnsiY, 32);
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                default:
                    AnsiState_.__AnsiCommand = false;
                    break;
            }
        }

        void AnsiProcess_Fixed(int TextFileLine_i)
        {
            if (AnsiState_.__AnsiCmd[0] == '[')
            {
                return;
            }
            switch (AnsiState_.__AnsiCmd[0])
            {
                case ' ':
                    if (AnsiState_.__AnsiCmd.Count == 2)
                    {
                        switch (AnsiState_.__AnsiCmd[1])
                        {
                            case 'F': // S7C1T
                                __AnsiResponse.Add("Control8bit_0");
                                break;
                            case 'G': // S8C1T
                                __AnsiResponse.Add("Control8bit_1");
                                break;
                        }
                        AnsiState_.__AnsiCommand = false;
                    }
                    break;
                case '#':
                    if (AnsiState_.__AnsiCmd.Count == 2)
                    {
                        switch (AnsiState_.__AnsiCmd[1])
                        {
                            case '3': // DECDHL
                                {
                                    AnsiState_.__AnsiWrapFlag = false;
                                    AnsiSetFontSize(AnsiState_.__AnsiY, 2, true);
                                }
                                break;
                            case '4': // DECDHL
                                {
                                    AnsiState_.__AnsiWrapFlag = false;
                                    AnsiSetFontSize(AnsiState_.__AnsiY, 3, true);
                                }
                                break;
                            case '5': // DECSWL
                                {
                                    AnsiState_.__AnsiWrapFlag = false;
                                    AnsiSetFontSize(AnsiState_.__AnsiY, 0, true);
                                }
                                break;
                            case '6': // DECDWL
                                {
                                    AnsiState_.__AnsiWrapFlag = false;
                                    AnsiSetFontSize(AnsiState_.__AnsiY, 1, true);
                                }
                                break;
                            case '8':
                                {
                                    for (int YY = 0; YY < AnsiMaxY; YY++)
                                    {
                                        for (int XX = 0; XX < AnsiMaxX; XX++)
                                        {
                                            AnsiChar(XX, YY, 'E', Core_.TextNormalBack, Core_.TextNormalFore, 0, 0, 0);
                                        }
                                    }
                                }
                                break;
                        }
                        AnsiState_.__AnsiCommand = false;
                    }
                    break;
                case '(':
                case ')':
                case '*':
                case '+':
                    if (AnsiState_.__AnsiCmd.Count >= 2)
                    {
                        int CharNum = 0;
                        switch (AnsiState_.__AnsiCmd[0])
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

                        string CharMap = ((char)AnsiState_.__AnsiCmd[1]).ToString();

                        if ("\"%&".IndexOf(CharMap) >= 0)
                        {
                            if (AnsiState_.__AnsiCmd.Count == 3)
                            {
                                CharMap = CharMap + ((char)AnsiState_.__AnsiCmd[2]).ToString();
                            }
                            else
                            {
                                CharMap = "";
                            }
                        }

                        if (!"".Equals(CharMap))
                        {
                            if (!ANSIDOS_)
                            {
                                AnsiState_.SetCharMap(CharNum, "1" + CharMap);
                            }
                            AnsiState_.__AnsiCommand = false;
                        }
                    }
                    break;
                case '-':
                case '.':
                case '/':
                    if (AnsiState_.__AnsiCmd.Count == 2)
                    {
                        int CharNum = 0;
                        switch (AnsiState_.__AnsiCmd[0])
                        {
                            case '-':
                                CharNum = 1;
                                break;
                            case '.':
                                CharNum = 2;
                                break;
                            case '/':
                                CharNum = 3;
                                break;
                        }

                        string CharMap = ((char)AnsiState_.__AnsiCmd[1]).ToString();

                        if (!"".Equals(CharMap))
                        {
                            if (!ANSIDOS_)
                            {
                                AnsiState_.SetCharMap(CharNum, "2" + CharMap);
                            }
                            AnsiState_.__AnsiCommand = false;
                        }
                    }
                    break;
                case '6': // DECBI
                    AnsiState_.__AnsiWrapFlag = false;
                    for (int __AnsiY = AnsiState_.__AnsiScrollFirst; __AnsiY <= AnsiState_.__AnsiScrollLast; __AnsiY++)
                    {
                        if (AnsiState_.__AnsiLineOccupy__.CountLines() > __AnsiY)
                        {
                            int FontSize = AnsiGetFontSize(__AnsiY);
                            int TempC = 0, TempB = 0, TempF = 0, TempA = 0;
                            if (FontSize > 0)
                            {
                                AnsiGetF(AnsiState_.__AnsiX, __AnsiY, out TempC, out TempB, out TempF, out TempA);
                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) >= (AnsiProcessGetXMax(false)))
                                {
                                    AnsiState_.__AnsiLineOccupy__.Delete(__AnsiY, (AnsiProcessGetXMax(false) - 1) * 2);
                                    AnsiState_.__AnsiLineOccupy__.Delete(__AnsiY, (AnsiProcessGetXMax(false) - 1) * 2);
                                }
                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) > (AnsiState_.__AnsiX * 2))
                                {
                                    AnsiState_.__AnsiLineOccupy__.BlankChar(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);
                                    AnsiState_.__AnsiLineOccupy__.Item_FontW = 2;
                                    AnsiState_.__AnsiLineOccupy__.Item_FontH = FontSize - 1;
                                    AnsiState_.__AnsiLineOccupy__.Insert(__AnsiY, AnsiProcessGetXMin(false) * 2);
                                    AnsiState_.__AnsiLineOccupy__.Item_FontW = 1;
                                    AnsiState_.__AnsiLineOccupy__.Insert(__AnsiY, AnsiProcessGetXMin(false) * 2);
                                }
                            }
                            else
                            {
                                AnsiGetF(AnsiProcessGetXMin(false), __AnsiY, out TempC, out TempB, out TempF, out TempA);
                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) >= (AnsiProcessGetXMax(false)))
                                {
                                    AnsiState_.__AnsiLineOccupy__.Delete(__AnsiY, (AnsiProcessGetXMax(false) - 1));
                                }
                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) > (AnsiProcessGetXMin(false)))
                                {
                                    AnsiState_.__AnsiLineOccupy__.BlankChar(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);

                                    AnsiState_.__AnsiLineOccupy__.Insert(__AnsiY, AnsiProcessGetXMin(false));
                                }
                            }
                            AnsiRepaintLine(__AnsiY);
                        }
                        AnsiState_.PrintCharInsDel++;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '7': // DECSC
                    AnsiState_.CursorSave();
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '8': // DECRC
                    AnsiState_.CursorLoad();
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '9': // DECFI
                    AnsiState_.__AnsiWrapFlag = false;
                    for (int __AnsiY = AnsiState_.__AnsiScrollFirst; __AnsiY <= AnsiState_.__AnsiScrollLast; __AnsiY++)
                    {
                        if (AnsiState_.__AnsiLineOccupy__.CountLines() > __AnsiY)
                        {
                            int TempC = 0, TempB = 0, TempF = 0, TempA = 0;
                            if (AnsiGetFontSize(__AnsiY) > 0)
                            {
                                AnsiGetF((AnsiProcessGetXMax(false) / 2) - 1, __AnsiY, out TempC, out TempB, out TempF, out TempA);
                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) >= ((AnsiProcessGetXMax(false) / 2)))
                                {
                                    AnsiState_.__AnsiLineOccupy__.BlankChar(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);
                                    AnsiState_.__AnsiLineOccupy__.Insert(__AnsiY, ((AnsiProcessGetXMax(false) / 2)));
                                }
                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) > (AnsiProcessGetXMin(false)))
                                {
                                    AnsiState_.__AnsiLineOccupy__.Delete(__AnsiY, AnsiProcessGetXMin(false));
                                    AnsiState_.__AnsiLineOccupy__.Delete(__AnsiY, AnsiProcessGetXMin(false));
                                }
                                if (TempC > 0)
                                {
                                    AnsiCharF((AnsiProcessGetXMax(false) / 2) - 1, __AnsiY, 32);
                                }
                            }
                            else
                            {
                                AnsiGetF(AnsiProcessGetXMax(false) - 1, __AnsiY, out TempC, out TempB, out TempF, out TempA);
                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) >= (AnsiProcessGetXMax(false)))
                                {
                                    AnsiState_.__AnsiLineOccupy__.BlankChar(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);
                                    AnsiState_.__AnsiLineOccupy__.Insert(__AnsiY, AnsiProcessGetXMax(false));
                                }
                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) > (AnsiProcessGetXMin(false)))
                                {
                                    AnsiState_.__AnsiLineOccupy__.Delete(__AnsiY, AnsiProcessGetXMin(false));
                                }
                                if (TempC > 0)
                                {
                                    AnsiCharF(AnsiProcessGetXMax(false) - 1, __AnsiY, 32);
                                }
                            }
                            AnsiRepaintLine(__AnsiY);
                        }
                        AnsiState_.PrintCharInsDel++;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '=': // DECKPAM
                    __AnsiResponse.Add("NumpadKey_1");
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '>': // DECKPNM
                    __AnsiResponse.Add("NumpadKey_0");
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'D': // IND
                    if (ANSIDOS_)
                    {

                    }
                    else
                    {
                        AnsiState_.__AnsiWrapFlag = false;
                        AnsiState_.__AnsiY += 1;
                        if (AnsiState_.__AnsiY > AnsiState_.__AnsiScrollLast)
                        {
                            AnsiScrollInit(AnsiState_.__AnsiY - AnsiState_.__AnsiScrollLast, AnsiState.AnsiScrollCommandDef.None);
                            AnsiState_.__AnsiY = AnsiState_.__AnsiScrollLast;
                        }
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'E': // NEL
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiY += 1;
                    AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                    if (AnsiState_.__AnsiY >= AnsiMaxY)
                    {
                        AnsiScrollInit(1, AnsiState.AnsiScrollCommandDef.None);
                        AnsiState_.__AnsiY--;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'H': // HTS
                    if (!AnsiState_.__AnsiTabs.Contains(AnsiState_.__AnsiX))
                    {
                        AnsiState_.__AnsiTabs.Add(AnsiState_.__AnsiX);
                        AnsiState_.__AnsiTabs.Sort();
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'M': // RI
                    if (ANSIDOS_)
                    {
                        AnsiState_.__AnsiMusic = true;
                    }
                    else
                    {
                        AnsiState_.__AnsiWrapFlag = false;
                        AnsiState_.__AnsiY -= 1;
                        if (AnsiState_.__AnsiY < AnsiState_.__AnsiScrollFirst)
                        {
                            AnsiScrollInit(AnsiState_.__AnsiY - AnsiState_.__AnsiScrollFirst, AnsiState.AnsiScrollCommandDef.None);
                            AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                        }
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'N': // SS2
                    if (!ANSIDOS_)
                    {
                        AnsiState_.CharMapNumGL = AnsiState_.CharMapNumGL % 10;
                        AnsiState_.CharMapNumGL += 20;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'O': // SS3
                    if (!ANSIDOS_)
                    {
                        AnsiState_.CharMapNumGL = AnsiState_.CharMapNumGL % 10;
                        AnsiState_.CharMapNumGL += 30;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'P': // DCS
                    AnsiState_.__AnsiDCS = "";
                    AnsiState_.__AnsiDCS_ = true;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'V':
                    AnsiState_.CharProtection2Print = true;
                    break;
                case 'W':
                    AnsiState_.CharProtection2Print = false;
                    break;
                case '\\': // ST
                    if (AnsiState_.__AnsiDCS_)
                    {
                        __AnsiResponse.Add(AnsiState_.__AnsiDCS);
                        AnsiState_.__AnsiDCS = "";
                        AnsiState_.__AnsiDCS_ = false;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case ']': // OSC
                    if (TextFileLine_i == 0x07)
                    {
                        string AnsiCmd_ = TextWork.IntToStr(AnsiState_.__AnsiCmd);
                        int Sep = AnsiCmd_.IndexOf(';');
                        if (Sep > 0)
                        {
                            string Opt = AnsiCmd_.Substring(0, Sep);
                            if (Opt == "]0")
                            {
                                __AnsiResponse.Add("WindowIcon" + AnsiCmd_.Substring(Sep + 1));
                                __AnsiResponse.Add("WindowTitle" + AnsiCmd_.Substring(Sep + 1));
                            }
                            if (Opt == "]1")
                            {
                                __AnsiResponse.Add("WindowIcon" + AnsiCmd_.Substring(Sep + 1));
                            }
                            if (Opt == "]2")
                            {
                                __AnsiResponse.Add("WindowTitle" + AnsiCmd_.Substring(Sep + 1));
                            }
                        }
                        AnsiState_.__AnsiCommand = false;
                    }
                    break;
                case 'c': // RIS
                    AnsiTerminalReset();
                    break;
                case 'n': // LS2
                    if (!ANSIDOS_)
                    {
                        AnsiState_.CharMapNumGL = 2;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'o': // LS3
                    if (!ANSIDOS_)
                    {
                        AnsiState_.CharMapNumGL = 3;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '~': // LS1R
                    if (!ANSIDOS_)
                    {
                        AnsiState_.CharMapNumGR = 1;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '}': // LS2R
                    if (!ANSIDOS_)
                    {
                        AnsiState_.CharMapNumGR = 2;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '|': // LS3R
                    if (!ANSIDOS_)
                    {
                        AnsiState_.CharMapNumGR = 3;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                default:
                    {
                        switch (AnsiState_.__AnsiCmd[0])
                        {
                            case '%':
                            case '-':
                            case '.':
                            case '/':
                                if (AnsiState_.__AnsiCmd.Count == 2)
                                {
                                    AnsiState_.__AnsiCommand = false;
                                }
                                break;
                            default:
                                AnsiState_.__AnsiCommand = false;
                                break;
                        }
                    }
                    break;
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
                            try
                            {
                                AnsiState_.DecParamSet(AnsiProcess_Int0(AnsiParams[i], ""), 1);
                            }
                            catch
                            {

                            }
                            switch (AnsiParams[i])
                            {
                                case "1": // DECSET / DECCKM
                                    __AnsiResponse.Add("CursorKey_1");
                                    break;
                                case "3": // DECSET / DECCOLM
                                    {
                                        AnsiState_.__AnsiWrapFlag = false;
                                        AnsiResize(132, -1);
                                        AnsiClearFontSize(-1);
                                        if (!AnsiState_.DECCOLMPreserve)
                                        {
                                            for (int i_ = 0; i_ < AnsiMaxY; i_++)
                                            {
                                                for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                                                {
                                                    AnsiChar(ii_, i_, 32);
                                                }
                                            }
                                        }
                                        AnsiState_.__AnsiX = 0;
                                        if (AnsiMaxY > 0)
                                        {
                                            AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                                        }
                                        else
                                        {
                                            AnsiState_.__AnsiY = 0;
                                        }
                                    }
                                    break;
                                case "4": // DECSET / DECSCLM
                                    AnsiState_.__AnsiSmoothScroll = true;
                                    break;
                                case "5": // DECSET / DECSCNM
                                    AnsiScreenNegative(true);
                                    break;
                                case "6": // DECSET / DECOM
                                    AnsiState_.__AnsiWrapFlag = false;
                                    AnsiState_.__AnsiOrigin = true;
                                    AnsiState_.__AnsiX = 0;
                                    AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                                    break;
                                case "7": // DECSET / DECAWM
                                    AnsiState_.__AnsiWrapFlag = false;
                                    AnsiState_.__AnsiNoWrap = false;
                                    break;
                                case "9":
                                    __AnsiResponse.Add("Mouse;1;" + AnsiParams[i]);
                                    break;
                                case "42": // DECSET / DECNRCM
                                    AnsiState_.CharMapNRCS = true;
                                    AnsiState_.RefreshCharMaps();
                                    break;
                                case "47":
                                    AnsiState_.ScreenAlte();
                                    for (int Y = 0; Y < AnsiMaxY; Y++)
                                    {
                                        AnsiRepaintLine(Y);
                                    }
                                    break;
                                case "66": // DECSET / DECNKM
                                    __AnsiResponse.Add("NumpadKey_1");
                                    break;
                                case "67": // DECSET / DECBKM
                                    __AnsiResponse.Add("BackspaceKey_1");
                                    break;
                                case "69": // DECSET / DECLRMM
                                    AnsiState_.__AnsiMarginLeftRight = true;
                                    break;
                                case "95": // DECSET / DECNCSM
                                    AnsiState_.DECCOLMPreserve = true;
                                    break;
                                case "1047":
                                    AnsiState_.ScreenAlte();
                                    for (int Y = 0; Y < AnsiMaxY; Y++)
                                    {
                                        AnsiRepaintLine(Y);
                                    }
                                    break;
                                case "1048":
                                    AnsiState_.CursorSave();
                                    break;
                                case "1049":
                                    AnsiState_.CursorSave();
                                    AnsiState_.ScreenAlte();
                                    for (int Y = 0; Y < AnsiMaxY; Y++)
                                    {
                                        AnsiRepaintLine(Y);
                                    }
                                    break;
                                case "1000":
                                case "1001":
                                case "1002":
                                case "1003":
                                case "1004":
                                case "1005":
                                case "1006":
                                case "1015":
                                case "1016":
                                    __AnsiResponse.Add("Mouse;1;" + AnsiParams[i]);
                                    break;
                            }
                        }
                    }
                    break;
                case 'l': // DECRST
                    {
                        for (int i = 0; i < AnsiParams.Length; i++)
                        {
                            try
                            {
                                AnsiState_.DecParamSet(AnsiProcess_Int0(AnsiParams[i], ""), 2);
                            }
                            catch
                            {

                            }
                            switch (AnsiParams[i])
                            {
                                case "1": // DECRST / DECCKM
                                    __AnsiResponse.Add("CursorKey_0");
                                    break;
                                case "2": // DECRST / DECANM
                                    AnsiState_.__AnsiVT52 = true;
                                    AnsiState_.__AnsiCommand = false;
                                    break;
                                case "3": // DECRST / DECCOLM
                                    {
                                        AnsiState_.__AnsiWrapFlag = false;
                                        AnsiResize(80, -1);
                                        AnsiClearFontSize(-1);
                                        if (!AnsiState_.DECCOLMPreserve)
                                        {
                                            for (int i_ = 0; i_ < AnsiMaxY; i_++)
                                            {
                                                for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                                                {
                                                    AnsiChar(ii_, i_, 32);
                                                }
                                            }
                                        }
                                        AnsiState_.__AnsiX = 0;
                                        if (AnsiMaxY > 0)
                                        {
                                            AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                                        }
                                        else
                                        {
                                            AnsiState_.__AnsiY = 0;
                                        }
                                    }
                                    break;
                                case "4": // DECRST / DECSCLM
                                    AnsiState_.__AnsiSmoothScroll = false;
                                    break;
                                case "5": // DECRST / DECSCNM
                                    AnsiScreenNegative(false);
                                    break;
                                case "6": // DECRST / DECOM
                                    AnsiState_.__AnsiWrapFlag = false;
                                    AnsiState_.__AnsiOrigin = false;
                                    AnsiState_.__AnsiX = 0;
                                    AnsiState_.__AnsiY = 0;
                                    break;
                                case "7": // DECRST / DECAWM
                                    AnsiState_.__AnsiWrapFlag = false;
                                    AnsiState_.__AnsiNoWrap = true;
                                    break;
                                case "9":
                                    __AnsiResponse.Add("Mouse;0;" + AnsiParams[i]);
                                    break;
                                case "42": // DECSET / DECNRCM
                                    AnsiState_.CharMapNRCS = false;
                                    AnsiState_.RefreshCharMaps();
                                    break;
                                case "47":
                                    AnsiState_.ScreenMain();
                                    for (int Y = 0; Y < AnsiMaxY; Y++)
                                    {
                                        AnsiRepaintLine(Y);
                                    }
                                    break;
                                case "66": // DECRST / DECNKM
                                    __AnsiResponse.Add("NumpadKey_0");
                                    break;
                                case "67": // DECRST / DECBKM
                                    __AnsiResponse.Add("BackspaceKey_0");
                                    break;
                                case "69": // DECRST / DECLRMM
                                    AnsiState_.__AnsiMarginLeftRight = false;
                                    break;
                                case "95": // DECRST / DECNCSM
                                    AnsiState_.DECCOLMPreserve = false;
                                    break;
                                case "1047":
                                    if (AnsiState_.IsScreenAlternate)
                                    {
                                        for (int i_ = 0; i_ < AnsiMaxY; i_++)
                                        {
                                            for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                                            {
                                                AnsiChar(ii_, i_, 32);
                                            }
                                        }
                                    }
                                    AnsiState_.ScreenMain();
                                    for (int Y = 0; Y < AnsiMaxY; Y++)
                                    {
                                        AnsiRepaintLine(Y);
                                    }
                                    break;
                                case "1048":
                                    AnsiState_.CursorLoad();
                                    break;
                                case "1049":
                                    if (AnsiState_.IsScreenAlternate)
                                    {
                                        for (int i_ = 0; i_ < AnsiMaxY; i_++)
                                        {
                                            for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                                            {
                                                AnsiChar(ii_, i_, 32);
                                            }
                                        }
                                    }
                                    AnsiState_.ScreenMain();
                                    AnsiState_.CursorLoad();
                                    for (int Y = 0; Y < AnsiMaxY; Y++)
                                    {
                                        AnsiRepaintLine(Y);
                                    }
                                    break;
                                case "1000":
                                case "1001":
                                case "1002":
                                case "1003":
                                case "1004":
                                case "1005":
                                case "1006":
                                case "1015":
                                case "1016":
                                    __AnsiResponse.Add("Mouse;0;" + AnsiParams[i]);
                                    break;
                            }
                        }
                    }
                    break;
                case 'n': // DSR
                    {
                        __AnsiResponse.Add(AnsiCmd_);
                    }
                    break;
                case 'p':
                    {
                        // DECRQM
                        if (AnsiCmd_.EndsWith("$p"))
                        {
                            __AnsiResponse.Add(AnsiCmd_);
                        }
                    }
                    break;
            }
            switch (AnsiCmd_)
            {
                case "[?J": // DECSED
                case "[?0J": // DECSED
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiClearFontSize(AnsiState_.__AnsiY + 1);
                    for (int i_ = AnsiState_.__AnsiY + 1; i_ < AnsiMaxY; i_++)
                    {
                        for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiCharUnprotected1(ii_, i_, 32);
                        }
                    }
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        if (AnsiGetFontSize(AnsiState_.__AnsiY) > 0)
                        {
                            for (int ii_ = (AnsiState_.__AnsiX << 1); ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiCharUnprotected1(ii_, AnsiState_.__AnsiY, 32);
                            }
                        }
                        else
                        {
                            for (int ii_ = AnsiState_.__AnsiX; ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiCharUnprotected1(ii_, AnsiState_.__AnsiY, 32);
                            }
                        }
                    }
                    break;
                case "[?1J": // DECSED
                    AnsiState_.__AnsiWrapFlag = false;
                    for (int i_ = 0; i_ < AnsiState_.__AnsiY; i_++)
                    {
                        AnsiSetFontSize(i_, 0, true);
                        if (AnsiMaxY > AnsiState_.__AnsiY)
                        {
                            for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiCharUnprotected1(ii_, i_, 32);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (AnsiGetFontSize(AnsiState_.__AnsiY) > 0)
                    {
                        for (int ii_ = 0; ii_ <= (AnsiState_.__AnsiX << 1); ii_++)
                        {
                            AnsiCharUnprotected1(ii_, AnsiState_.__AnsiY, 32);
                        }
                    }
                    else
                    {
                        for (int ii_ = 0; ii_ <= AnsiState_.__AnsiX; ii_++)
                        {
                            AnsiCharUnprotected1(ii_, AnsiState_.__AnsiY, 32);
                        }
                    }
                    break;
                case "[?2J": // DECSED
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiClearFontSize(-1);
                    for (int i_ = 0; i_ < AnsiMaxY; i_++)
                    {
                        for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiCharUnprotected1(ii_, i_, 32);
                        }
                    }
                    if (ANSIDOS_)
                    {
                        AnsiState_.__AnsiX = 0;
                        AnsiState_.__AnsiY = 0;
                    }
                    break;
                case "[?K": // DECSEL
                case "[?0K": // DECSEL
                    AnsiState_.__AnsiWrapFlag = false;
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        if (AnsiGetFontSize(AnsiState_.__AnsiY) > 0)
                        {
                            for (int ii_ = (AnsiState_.__AnsiX << 1); ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiCharUnprotected1(ii_, AnsiState_.__AnsiY, 32);
                            }
                        }
                        else
                        {
                            for (int ii_ = AnsiState_.__AnsiX; ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiCharUnprotected1(ii_, AnsiState_.__AnsiY, 32);
                            }
                        }
                    }
                    break;
                case "[?1K": // DECSEL
                    AnsiState_.__AnsiWrapFlag = false;
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        if (AnsiGetFontSize(AnsiState_.__AnsiY) > 0)
                        {
                            for (int ii_ = 0; ii_ <= (AnsiState_.__AnsiX << 1); ii_++)
                            {
                                AnsiCharUnprotected1(ii_, AnsiState_.__AnsiY, 32);
                            }
                        }
                        else
                        {
                            for (int ii_ = 0; ii_ <= AnsiState_.__AnsiX; ii_++)
                            {
                                AnsiCharUnprotected1(ii_, AnsiState_.__AnsiY, 32);
                            }
                        }
                    }
                    break;
                case "[?2K": // DECSEL
                    AnsiState_.__AnsiWrapFlag = false;
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiCharUnprotected1(ii_, AnsiState_.__AnsiY, 32);
                        }
                    }
                    break;
            }
        }

        void AnsiProcess_CSI_Fixed(string AnsiCmd_)
        {
            switch (AnsiCmd_)
            {
                case "[J": // ED
                case "[0J": // ED
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiClearFontSize(AnsiState_.__AnsiY + 1);
                    for (int i_ = AnsiState_.__AnsiY + 1; i_ < AnsiMaxY; i_++)
                    {
                        for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiCharUnprotected2(ii_, i_, 32);
                        }
                    }
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        if (AnsiGetFontSize(AnsiState_.__AnsiY) > 0)
                        {
                            for (int ii_ = (AnsiState_.__AnsiX << 1); ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiCharUnprotected2(ii_, AnsiState_.__AnsiY, 32);
                            }
                        }
                        else
                        {
                            for (int ii_ = AnsiState_.__AnsiX; ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiCharUnprotected2(ii_, AnsiState_.__AnsiY, 32);
                            }
                        }
                    }
                    break;
                case "[1J": // ED
                    AnsiState_.__AnsiWrapFlag = false;
                    for (int i_ = 0; i_ < AnsiState_.__AnsiY; i_++)
                    {
                        AnsiSetFontSize(i_, 0, true);
                        if (AnsiMaxY > AnsiState_.__AnsiY)
                        {
                            for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiCharUnprotected2(ii_, i_, 32);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (AnsiGetFontSize(AnsiState_.__AnsiY) > 0)
                    {
                        for (int ii_ = 0; ii_ <= (AnsiState_.__AnsiX << 1); ii_++)
                        {
                            AnsiCharUnprotected2(ii_, AnsiState_.__AnsiY, 32);
                        }
                    }
                    else
                    {
                        for (int ii_ = 0; ii_ <= AnsiState_.__AnsiX; ii_++)
                        {
                            AnsiCharUnprotected2(ii_, AnsiState_.__AnsiY, 32);
                        }
                    }
                    break;
                case "[2J": // ED
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiClearFontSize(-1);
                    for (int i_ = 0; i_ < AnsiMaxY; i_++)
                    {
                        for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiCharUnprotected2(ii_, i_, 32);
                        }
                    }
                    if (ANSIDOS_)
                    {
                        AnsiState_.__AnsiX = 0;
                        AnsiState_.__AnsiY = 0;
                    }
                    break;
                case "[K": // EL
                case "[0K": // EL
                    AnsiState_.__AnsiWrapFlag = false;
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        if (AnsiGetFontSize(AnsiState_.__AnsiY) > 0)
                        {
                            for (int ii_ = (AnsiState_.__AnsiX << 1); ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiCharUnprotected2(ii_, AnsiState_.__AnsiY, 32);
                            }
                        }
                        else
                        {
                            for (int ii_ = AnsiState_.__AnsiX; ii_ < AnsiMaxX; ii_++)
                            {
                                AnsiCharUnprotected2(ii_, AnsiState_.__AnsiY, 32);
                            }
                        }
                    }
                    break;
                case "[1K": // EL
                    AnsiState_.__AnsiWrapFlag = false;
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        if (AnsiGetFontSize(AnsiState_.__AnsiY) > 0)
                        {
                            for (int ii_ = 0; ii_ <= (AnsiState_.__AnsiX << 1); ii_++)
                            {
                                AnsiCharUnprotected2(ii_, AnsiState_.__AnsiY, 32);
                            }
                        }
                        else
                        {
                            for (int ii_ = 0; ii_ <= AnsiState_.__AnsiX; ii_++)
                            {
                                AnsiCharUnprotected2(ii_, AnsiState_.__AnsiY, 32);
                            }
                        }
                    }
                    break;
                case "[2K": // EL
                    AnsiState_.__AnsiWrapFlag = false;
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        for (int ii_ = 0; ii_ < AnsiMaxX; ii_++)
                        {
                            AnsiCharUnprotected2(ii_, AnsiState_.__AnsiY, 32);
                        }
                    }
                    break;
                case "[c": // Primary DA
                case "[0c": // Primary DA
                    __AnsiResponse.Add("[0c");
                    break;
                case "[>c": // Secondary DA
                case "[>0c": // Secondary DA
                case "[=c": // Tertiary DA
                case "[=0c": // Tertiary DA
                case "[5n": // DSR
                case "[6n": // DSR / CPR
                case "[0x": // DECREQTPARM
                case "[1x": // DECREQTPARM
                    __AnsiResponse.Add(AnsiCmd_);
                    break;
                case "[!p": // DECSTR
                    AnsiTerminalReset();
                    break;
                case "[s": // SCOSC
                    if (!AnsiState_.__AnsiMarginLeftRight)
                    {
                        AnsiState_.CursorSave();
                    }
                    else
                    {
                        AnsiState_.__AnsiMarginLeft = 0;
                        AnsiState_.__AnsiMarginRight = AnsiMaxX;
                    }
                    break;
                case "[u": // SCORC
                    AnsiState_.CursorLoad();
                    break;
                case "[$}": // DECSASD
                case "[0$}": // DECSASD
                    AnsiState_.StatusBar = false;
                    break;
                case "[1$}": // DECSASD
                    AnsiState_.StatusBar = true;
                    break;
                default:
                    AnsiProcess_CSI(AnsiCmd_);
                    break;
            }
        }

        int AnsiProcess_Int0(string Param, string AnsiCmd_)
        {
            try
            {
                return int.Parse(Param);
            }
            catch
            {
                if (Param == "") return 0;
                throw new Exception("Integer error: " + AnsiCmd_);
            }
        }

        int AnsiProcess_Int1(string Param, string AnsiCmd_)
        {
            try
            {
                return int.Parse(Param);
            }
            catch
            {
                if (Param == "") return 1;
                throw new Exception("Integer error: " + AnsiCmd_);
            }
        }

        int AnsiProcess_Int11(string Param, string AnsiCmd_)
        {
            if (Param == "0") return 1;
            try
            {
                return int.Parse(Param);
            }
            catch
            {
                if (Param == "") return 1;
                throw new Exception("Integer error: " + AnsiCmd_);
            }
        }


        void AnsiProcess_CSI(string AnsiCmd_)
        {
            string[] AnsiParams = AnsiCmd_.Substring(1, AnsiCmd_.Length - 2).Split(';');
            switch (AnsiCmd_[AnsiCmd_.Length - 1])
            {
                case '@': // ICH // SL
                    if (AnsiCmd_[AnsiCmd_.Length - 2] == ' ')
                    {
                        // SL
                        AnsiScrollColumns(AnsiProcess_Int11(AnsiParams[0].Substring(0, AnsiParams[0].Length - 1), AnsiCmd_));
                    }
                    else
                    {
                        // ICH
                        AnsiState_.__AnsiWrapFlag = false;
                        int InsCycle = AnsiProcess_Int1(AnsiParams[0], AnsiCmd_);
                        if (AnsiState_.__AnsiLineOccupy__.CountLines() > AnsiState_.__AnsiY)
                        {
                            int FontSize = AnsiGetFontSize(AnsiState_.__AnsiY);
                            while (InsCycle > 0)
                            {
                                int TempC = 0, TempB = 0, TempF = 0, TempA = 0;
                                if (FontSize > 0)
                                {
                                    AnsiGetF(AnsiState_.__AnsiX, AnsiState_.__AnsiY, out TempC, out TempB, out TempF, out TempA);
                                    if (AnsiState_.__AnsiLineOccupy__.CountItems(AnsiState_.__AnsiY) > (AnsiState_.__AnsiX * 2))
                                    {
                                        AnsiState_.__AnsiLineOccupy__.BlankChar(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);
                                        AnsiState_.__AnsiLineOccupy__.Item_FontW = 2;
                                        AnsiState_.__AnsiLineOccupy__.Item_FontH = FontSize - 1;
                                        AnsiState_.__AnsiLineOccupy__.Insert(AnsiState_.__AnsiY, AnsiState_.__AnsiX * 2);
                                        AnsiState_.__AnsiLineOccupy__.Item_FontW = 1;
                                        AnsiState_.__AnsiLineOccupy__.Insert(AnsiState_.__AnsiY, AnsiState_.__AnsiX * 2);
                                    }
                                    if (AnsiState_.__AnsiLineOccupy__.CountItems(AnsiState_.__AnsiY) > (AnsiProcessGetXMax(false)))
                                    {
                                        AnsiState_.__AnsiLineOccupy__.Delete(AnsiState_.__AnsiY, AnsiProcessGetXMax(false));
                                        AnsiState_.__AnsiLineOccupy__.Delete(AnsiState_.__AnsiY, AnsiProcessGetXMax(false));
                                    }
                                }
                                else
                                {
                                    AnsiGetF(AnsiState_.__AnsiX, AnsiState_.__AnsiY, out TempC, out TempB, out TempF, out TempA);
                                    if (AnsiState_.__AnsiLineOccupy__.CountItems(AnsiState_.__AnsiY) > (AnsiState_.__AnsiX))
                                    {
                                        AnsiState_.__AnsiLineOccupy__.BlankChar(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);

                                        AnsiState_.__AnsiLineOccupy__.Insert(AnsiState_.__AnsiY, AnsiState_.__AnsiX);
                                    }
                                    if (AnsiState_.__AnsiLineOccupy__.CountItems(AnsiState_.__AnsiY) > (AnsiProcessGetXMax(false)))
                                    {
                                        AnsiState_.__AnsiLineOccupy__.Delete(AnsiState_.__AnsiY, AnsiProcessGetXMax(false));
                                    }
                                }
                                InsCycle--;
                            }
                            AnsiRepaintLine(AnsiState_.__AnsiY);
                        }
                        AnsiState_.PrintCharInsDel++;
                    }
                    break;
                case 'A': // CUU // SR
                    AnsiState_.__AnsiWrapFlag = false;
                    if (AnsiCmd_[AnsiCmd_.Length - 2] == ' ')
                    {
                        // SR
                        AnsiScrollColumns(0 - AnsiProcess_Int11(AnsiParams[0].Substring(0, AnsiParams[0].Length - 1), AnsiCmd_));
                    }
                    else
                    {
                        // CUU
                        AnsiState_.__AnsiY -= Math.Max(AnsiProcess_Int1(AnsiParams[0], AnsiCmd_), 1);
                        if (AnsiState_.__AnsiY < AnsiState_.__AnsiScrollFirst)
                        {
                            AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                        }
                    }
                    break;
                case 'B': // CUD
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiY += Math.Max(AnsiProcess_Int1(AnsiParams[0], AnsiCmd_), 1);
                    if (AnsiState_.__AnsiY > AnsiState_.__AnsiScrollLast)
                    {
                        AnsiState_.__AnsiY = AnsiState_.__AnsiScrollLast;
                    }
                    break;
                case 'C': // CUF
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiX += Math.Max(AnsiProcess_Int1(AnsiParams[0], AnsiCmd_), 1);
                    if ((AnsiMaxX > 0) && (AnsiState_.__AnsiX >= AnsiProcessGetXMax(false)))
                    {
                        if (ANSIDOS_)
                        {
                            AnsiState_.__AnsiY++;
                            AnsiState_.__AnsiX = AnsiState_.__AnsiX - AnsiMaxX;
                        }
                        else
                        {
                            AnsiState_.__AnsiX = AnsiProcessGetXMax(false) - 1;
                        }
                    }
                    break;
                case 'D': // CUB
                    AnsiState_.__AnsiWrapFlag = false;
                    if (AnsiMaxX > 0)
                    {
                        if (AnsiState_.__AnsiX >= AnsiMaxX)
                        {
                            AnsiState_.__AnsiX = AnsiMaxX - 1;
                        }
                    }
                    AnsiState_.__AnsiX -= Math.Max(AnsiProcess_Int1(AnsiParams[0], AnsiCmd_), 1);
                    if (AnsiState_.__AnsiX < AnsiProcessGetXMin(false))
                    {
                        AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                    }
                    break;
                case 'E': // CNL
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                    AnsiState_.__AnsiY += AnsiProcess_Int11(AnsiParams[0], AnsiCmd_);
                    if (AnsiState_.__AnsiY > AnsiState_.__AnsiScrollLast)
                    {
                        AnsiState_.__AnsiY = AnsiState_.__AnsiScrollLast;
                    }
                    break;
                case 'F': // CPL
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                    AnsiState_.__AnsiY -= AnsiProcess_Int11(AnsiParams[0], AnsiCmd_);
                    if (AnsiState_.__AnsiY < AnsiState_.__AnsiScrollFirst)
                    {
                        AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                    }
                    break;
                case 'G': // CHA
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiX = AnsiProcess_Int11(AnsiParams[0], AnsiCmd_) - 1;
                    if (AnsiState_.__AnsiOrigin && AnsiState_.__AnsiMarginLeftRight)
                    {
                        AnsiState_.__AnsiX += AnsiState_.__AnsiMarginLeft;
                    }
                    break;
                case 'H': // CUP
                case 'f': // HVP
                    if (!AnsiState_.StatusBar)
                    {
                        AnsiState_.__AnsiWrapFlag = false;
                        if (AnsiParams.Length == 1)
                        {
                            AnsiParams = new string[] { AnsiParams[0], "1" };
                        }
                        AnsiState_.__AnsiY = AnsiProcess_Int1(AnsiParams[0], AnsiCmd_) - 1;
                        AnsiState_.__AnsiX = AnsiProcess_Int1(AnsiParams[1], AnsiCmd_) - 1;
                        if (AnsiState_.__AnsiY < 0)
                        {
                            AnsiState_.__AnsiY = 0;
                        }
                        if (AnsiState_.__AnsiX < 0)
                        {
                            AnsiState_.__AnsiX = 0;
                        }
                        if (AnsiState_.__AnsiOrigin)
                        {
                            AnsiState_.__AnsiY += AnsiState_.__AnsiScrollFirst;
                        }
                        AnsiState_.__AnsiX += AnsiProcessGetXMin(true);
                        if (AnsiState_.__AnsiX >= AnsiMaxX)
                        {
                            AnsiState_.__AnsiX = AnsiMaxX - 1;
                        }
                        if (AnsiState_.__AnsiY >= AnsiMaxY)
                        {
                            AnsiState_.__AnsiY = AnsiMaxY - 1;
                        }
                        if (AnsiState_.__AnsiOrigin)
                        {
                            if (AnsiState_.__AnsiMarginLeftRight && (AnsiState_.__AnsiX >= AnsiState_.__AnsiMarginRight))
                            {
                                AnsiState_.__AnsiX = AnsiState_.__AnsiMarginRight - 1;
                            }
                            if (AnsiState_.__AnsiY > AnsiState_.__AnsiScrollLast)
                            {
                                AnsiState_.__AnsiY = AnsiState_.__AnsiScrollLast;
                            }
                        }
                    }
                    break;
                case 'I': // CHT
                    {
                        AnsiDoTab(AnsiProcess_Int11(AnsiParams[0], AnsiCmd_));
                    }
                    break;
                case 'L': // IL
                    if (ANSIDOS_)
                    {

                    }
                    else
                    {
                        if ((AnsiState_.__AnsiY >= AnsiState_.__AnsiScrollFirst) && (AnsiState_.__AnsiY <= AnsiState_.__AnsiScrollLast))
                        {
                            int T1 = AnsiState_.__AnsiScrollFirst;
                            int T2 = AnsiState_.__AnsiScrollLast;
                            AnsiState_.__AnsiScrollFirst = AnsiState_.__AnsiY;
                            AnsiScrollInit(0 - AnsiProcess_Int1(AnsiParams[0], AnsiCmd_), AnsiState.AnsiScrollCommandDef.FirstLast, T1, T2, 0, 0);
                        }
                    }
                    break;
                case 'M': // DL
                    if (ANSIDOS_)
                    {
                        AnsiState_.__AnsiMusic = true;
                    }
                    else
                    {
                        if ((AnsiState_.__AnsiY >= AnsiState_.__AnsiScrollFirst) && (AnsiState_.__AnsiY <= AnsiState_.__AnsiScrollLast))
                        {
                            int T1 = AnsiState_.__AnsiScrollFirst;
                            int T2 = AnsiState_.__AnsiScrollLast;
                            AnsiState_.__AnsiScrollFirst = AnsiState_.__AnsiY;
                            AnsiScrollInit(AnsiProcess_Int1(AnsiParams[0], AnsiCmd_), AnsiState.AnsiScrollCommandDef.FirstLast, T1, T2, 0, 0);
                        }
                    }
                    break;
                case 'P': // DCH
                    {
                        AnsiState_.__AnsiWrapFlag = false;
                        int DelCycle = AnsiProcess_Int1(AnsiParams[0], AnsiCmd_);
                        if (AnsiState_.__AnsiLineOccupy__.CountLines() > AnsiState_.__AnsiY)
                        {
                            while (DelCycle > 0)
                            {
                                int TempC = 0, TempB = 0, TempF = 0, TempA = 0;
                                if (AnsiGetFontSize(AnsiState_.__AnsiY) > 0)
                                {
                                    AnsiGetF((AnsiProcessGetXMax(false)) - 1, AnsiState_.__AnsiY, out TempC, out TempB, out TempF, out TempA);
                                    if (AnsiState_.__AnsiLineOccupy__.CountItems(AnsiState_.__AnsiY) >= ((AnsiProcessGetXMax(false))))
                                    {
                                        AnsiState_.__AnsiLineOccupy__.BlankChar(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);

                                        AnsiState_.__AnsiLineOccupy__.Insert(AnsiState_.__AnsiY, (AnsiProcessGetXMax(false)));
                                        AnsiState_.__AnsiLineOccupy__.Insert(AnsiState_.__AnsiY, (AnsiProcessGetXMax(false)));
                                    }
                                    if (AnsiState_.__AnsiLineOccupy__.CountItems(AnsiState_.__AnsiY) > (AnsiState_.__AnsiX))
                                    {
                                        AnsiState_.__AnsiLineOccupy__.Delete(AnsiState_.__AnsiY, AnsiState_.__AnsiX);
                                        AnsiState_.__AnsiLineOccupy__.Delete(AnsiState_.__AnsiY, AnsiState_.__AnsiX);
                                    }
                                    if (TempC > 0)
                                    {
                                        AnsiCharF((AnsiProcessGetXMax(false) / 2) - 1, AnsiState_.__AnsiY, 32);
                                    }
                                }
                                else
                                {
                                    AnsiGetF(AnsiProcessGetXMax(false) - 1, AnsiState_.__AnsiY, out TempC, out TempB, out TempF, out TempA);
                                    if (AnsiState_.__AnsiLineOccupy__.CountItems(AnsiState_.__AnsiY) >= ((AnsiProcessGetXMax(false))))
                                    {
                                        AnsiState_.__AnsiLineOccupy__.BlankChar(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);

                                        AnsiState_.__AnsiLineOccupy__.Insert(AnsiState_.__AnsiY, (AnsiProcessGetXMax(false)));
                                    }
                                    if (AnsiState_.__AnsiLineOccupy__.CountItems(AnsiState_.__AnsiY) > (AnsiState_.__AnsiX))
                                    {
                                        AnsiState_.__AnsiLineOccupy__.Delete(AnsiState_.__AnsiY, AnsiState_.__AnsiX);
                                    }
                                    if (TempC > 0)
                                    {
                                        AnsiCharF(AnsiProcessGetXMax(false) - 1, AnsiState_.__AnsiY, 32);
                                    }
                                }
                                DelCycle--;
                            }
                            AnsiRepaintLine(AnsiState_.__AnsiY);
                        }
                        AnsiState_.PrintCharInsDel++;
                    }
                    break;
                case 'S': // SU
                    if (AnsiParams.Length == 1)
                    {
                        AnsiScrollInit(AnsiProcess_Int11(AnsiParams[0], AnsiCmd_), AnsiState.AnsiScrollCommandDef.None);
                    }
                    break;
                case 'T': // SD, XTHIMOUSE
                    if (AnsiParams.Length == 1)
                    {
                        AnsiScrollInit(0 - AnsiProcess_Int11(AnsiParams[0], AnsiCmd_), AnsiState.AnsiScrollCommandDef.None);
                    }
                    if (AnsiParams.Length >= 5)
                    {
                        __AnsiResponse.Add("Mouse;2;" + AnsiCmd_.Substring(1, AnsiCmd_.Length - 2));
                    }
                    break;
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
                            AnsiState_.__AnsiFontSizeW = AnsiProcess_Int0(AnsiParams[1], AnsiCmd_);
                            AnsiState_.__AnsiFontSizeH = AnsiProcess_Int0(AnsiParams[2], AnsiCmd_);
                            if (AnsiState_.__AnsiFontSizeW > Core_.FontMaxSizeCode)
                            {
                                AnsiState_.__AnsiFontSizeW = 0;
                            }
                            if (AnsiState_.__AnsiFontSizeH > Core_.FontMaxSizeCode)
                            {
                                AnsiState_.__AnsiFontSizeH = 0;
                            }
                            break;
                        case "1":
                            {
                                SetProcessDelay(AnsiProcess_Int0(AnsiParams[1], AnsiCmd_));
                            }
                            break;
                    }
                    break;
                case 'X': // ECH
                    {
                        AnsiState_.__AnsiWrapFlag = false;
                        int DelCycle = AnsiProcess_Int1(AnsiParams[0], AnsiCmd_);
                        while (DelCycle > 0)
                        {
                            AnsiCharFUnprotected2(AnsiState_.__AnsiX + DelCycle - 1, AnsiState_.__AnsiY, 32);
                            DelCycle--;
                        }
                    }
                    break;
                case 'Z': // CBT
                    {
                        AnsiDoTab(0 - AnsiProcess_Int11(AnsiParams[0], AnsiCmd_));
                    }
                    break;
                case '`': // HPA
                    if (!AnsiState_.StatusBar)
                    {
                        AnsiState_.__AnsiWrapFlag = false;
                        AnsiState_.__AnsiX = AnsiProcess_Int1(AnsiParams[0], AnsiCmd_) - 1 + AnsiProcessGetXMin(true);
                    }
                    break;
                case 'a': // HPR
                    if (!AnsiState_.StatusBar)
                    {
                        AnsiState_.__AnsiWrapFlag = false;
                        AnsiState_.__AnsiX += AnsiProcess_Int1(AnsiParams[0], AnsiCmd_);
                    }
                    break;
                case 'b': // REP
                    {
                        if (AnsiCharPrintLast >= 0)
                        {
                            AnsiCharPrintRepeater = AnsiProcess_Int11(AnsiParams[0], AnsiCmd_);
                        }
                    }
                    break;
                case 'd': // VPA
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiY = AnsiProcess_Int1(AnsiParams[0], AnsiCmd_) - 1;
                    if (AnsiState_.__AnsiOrigin)
                    {
                        AnsiState_.__AnsiY += AnsiState_.__AnsiScrollFirst;
                    }
                    break;
                case 'e': // VPR
                    AnsiState_.__AnsiWrapFlag = false;
                    AnsiState_.__AnsiY += AnsiProcess_Int1(AnsiParams[0], AnsiCmd_);
                    break;
                case 'g': // TBC
                    {
                        switch (AnsiParams[0])
                        {
                            case "":
                            case "0":
                                if (AnsiState_.__AnsiTabs.Contains(AnsiState_.__AnsiX) && (AnsiState_.__AnsiX >= 0))
                                {
                                    AnsiState_.__AnsiTabs.Remove(AnsiState_.__AnsiX);
                                }
                                break;
                            case "3":
                                AnsiState_.__AnsiTabs.Clear();
                                AnsiState_.__AnsiTabs.Add(-1);
                                break;
                        }
                    }
                    break;
                case 'h': // SM
                    try
                    {
                        AnsiState_.AnsiParamSet(AnsiProcess_Int0(AnsiParams[0], ""), 1);
                    }
                    catch
                    {

                    }
                    switch (AnsiParams[0])
                    {
                        case "4": // SM / IRM
                            AnsiState_.__AnsiInsertMode = true;
                            break;
                        case "6": // SM / ???
                            AnsiState_.CharProtection2Ommit = true;
                            break;
                        case "12": // SM / SRM
                            __AnsiResponse.Add("LocalEcho_0");
                            break;
                        case "20": // SM / LNM
                            __AnsiResponse.Add("EnterKey_1");
                            break;
                    }
                    break;
                case 'l': // RM
                    try
                    {
                        AnsiState_.AnsiParamSet(AnsiProcess_Int0(AnsiParams[0], ""), 2);
                    }
                    catch
                    {

                    }
                    switch (AnsiParams[0])
                    {
                        case "4": // RM / IRM
                            AnsiState_.__AnsiInsertMode = false;
                            break;
                        case "6": // RM / ???
                            AnsiState_.CharProtection2Ommit = false;
                            break;
                        case "12": // RM / SRM
                            __AnsiResponse.Add("LocalEcho_1");
                            break;
                        case "20": // RM / LNM
                            __AnsiResponse.Add("EnterKey_0");
                            break;
                    }
                    break;
                case 'm': // SGR
                    {
                        AnsiProcess_CSI_m(AnsiParams);
                    }
                    break;
                case 'p':
                    {
                        // DECRQM
                        if (AnsiCmd_.EndsWith("$p"))
                        {
                            __AnsiResponse.Add(AnsiCmd_);
                        }

                        // DECSCL
                        if (AnsiCmd_.EndsWith("\"p"))
                        {
                            if (AnsiParams.Length >= 2)
                            {
                                if (AnsiProcess_Int0(AnsiParams[1].Substring(0, AnsiParams[1].Length - 1), AnsiCmd_) == 1)
                                {
                                    __AnsiResponse.Add("Control8bit_0");
                                }
                                else
                                {
                                    __AnsiResponse.Add("Control8bit_1");
                                }
                            }
                        }
                    }
                    break;
                case 'q':
                    // DECSCA
                    if (AnsiCmd_.EndsWith("\"q"))
                    {
                        if (AnsiParams.Length >= 1)
                        {
                            AnsiParams[AnsiParams.Length - 1] = AnsiParams[AnsiParams.Length - 1].Substring(0, AnsiParams[AnsiParams.Length - 1].Length - 1);
                            if (AnsiProcess_Int0(AnsiParams[0], AnsiCmd_) == 1)
                            {
                                AnsiState_.CharProtection1Print = true;
                            }
                            else
                            {
                                AnsiState_.CharProtection1Print = false;
                            }
                        }
                    }
                    break;
                case 'r': // DECSTBM
                    // DECCARA
                    if (AnsiCmd_.EndsWith("$r"))
                    {
                        if (AnsiParams.Length >= 5)
                        {
                            AnsiParams[AnsiParams.Length - 1] = AnsiParams[AnsiParams.Length - 1].Substring(0, AnsiParams[AnsiParams.Length - 1].Length - 1);
                            AnsiAttributesSave();
                            int RecY1 = AnsiProcess_Int0(AnsiParams[0], AnsiCmd_) - 1;
                            int RecX1 = AnsiProcess_Int0(AnsiParams[1], AnsiCmd_) - 1;
                            int RecY2 = AnsiProcess_Int0(AnsiParams[2], AnsiCmd_) - 1;
                            int RecX2 = AnsiProcess_Int0(AnsiParams[3], AnsiCmd_) - 1;
                            int Attr = AnsiProcess_Int0(AnsiParams[4], AnsiCmd_);
                            if (AnsiState_.__AnsiOrigin)
                            {
                                RecY1 += AnsiState_.__AnsiScrollFirst;
                                RecY2 += AnsiState_.__AnsiScrollFirst;
                                if (AnsiState_.__AnsiMarginLeftRight)
                                {
                                    RecX1 += AnsiState_.__AnsiMarginLeft;
                                    RecX2 += AnsiState_.__AnsiMarginLeft;
                                }
                            }

                            for (int Y = RecY1; Y <= RecY2; Y++)
                            {
                                for (int X = RecX1; X <= RecX2; X++)
                                {
                                    int TempC, TempB, TempF, TempA;
                                    AnsiGetF(X, Y, out TempC, out TempB, out TempF, out TempA);
                                    switch (Attr)
                                    {
                                        case 0:
                                            TempA = TempA & 0x20;
                                            break;
                                        case 1: // Bold
                                            TempA = TempA | 0x01;
                                            break;
                                        case 3: // Italic
                                            TempA = TempA | 0x02;
                                            break;
                                        case 4: // Underline
                                            TempA = TempA | 0x04;
                                            break;
                                        case 5: // Blink
                                            TempA = TempA | 0x08;
                                            break;
                                        case 7: // Inverse
                                            TempA = TempA | 0x10;
                                            break;
                                        case 8: // Invisible
                                            TempA = TempA | 0x20;
                                            break;
                                        case 9: // Strikethrough
                                            TempA = TempA | 0x40;
                                            break;
                                    }
                                    AnsiState_.__AnsiBack = TempB;
                                    AnsiState_.__AnsiFore = TempF;
                                    AnsiState_.__AnsiAttr = TempA;
                                    AnsiCharF(X, Y, TempC);
                                }
                                AnsiRepaintLine(Y);
                            }
                            AnsiAttributesLoad();
                        }
                    }
                    else
                    {
                        AnsiState_.__AnsiWrapFlag = false;
                        if (AnsiParams.Length == 1)
                        {
                            AnsiParams = new string[] { AnsiParams[0], (AnsiMaxY + 1).ToString() };
                        }
                        AnsiState_.__AnsiScrollFirst = AnsiProcess_Int0(AnsiParams[0], AnsiCmd_) - 1;
                        AnsiState_.__AnsiScrollLast = AnsiProcess_Int0(AnsiParams[1], AnsiCmd_) - 1;
                        if (AnsiState_.__AnsiScrollFirst > AnsiState_.__AnsiScrollLast)
                        {
                            AnsiState_.__AnsiScrollFirst = 0;
                            AnsiState_.__AnsiScrollLast = AnsiMaxY - 1;
                        }
                        if (AnsiState_.__AnsiScrollFirst < 0)
                        {
                            AnsiState_.__AnsiScrollFirst = 0;
                        }
                        if (AnsiState_.__AnsiScrollLast >= AnsiMaxY)
                        {
                            AnsiState_.__AnsiScrollLast = AnsiMaxY - 1;
                        }
                        AnsiState_.__AnsiX = 0;
                        if (AnsiState_.__AnsiOrigin)
                        {
                            AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                        }
                        else
                        {
                            AnsiState_.__AnsiY = 0;
                        }
                    }
                    break;
                case 's': // DECSLRM
                    if (AnsiParams.Length == 1)
                    {
                        AnsiParams = new string[] { AnsiParams[0], (AnsiMaxX + 1).ToString() };
                    }
                    if (AnsiState_.__AnsiMarginLeftRight)
                    {
                        AnsiState_.__AnsiMarginLeft = AnsiProcess_Int0(AnsiParams[0], AnsiCmd_) - 1;
                        AnsiState_.__AnsiMarginRight = AnsiProcess_Int0(AnsiParams[1], AnsiCmd_);
                        if ((AnsiState_.__AnsiMarginLeft == -1) && (AnsiState_.__AnsiMarginRight == 0))
                        {
                            AnsiState_.__AnsiMarginLeft = 0;
                            AnsiState_.__AnsiMarginRight = AnsiMaxX;
                        }
                        if (AnsiState_.__AnsiMarginLeft >= AnsiState_.__AnsiMarginRight)
                        {
                            AnsiState_.__AnsiMarginLeft = 0;
                            AnsiState_.__AnsiMarginRight = AnsiMaxX;
                        }
                        if (AnsiState_.__AnsiMarginLeft < 0)
                        {
                            AnsiState_.__AnsiMarginLeft = 0;
                        }
                        if (AnsiState_.__AnsiMarginRight > AnsiMaxX)
                        {
                            AnsiState_.__AnsiMarginRight = AnsiMaxX;
                        }
                    }
                    break;
                case 't': // XTWINOPS
                    // DECRARA
                    if (AnsiCmd_.EndsWith("$t"))
                    {
                        if (AnsiParams.Length >= 5)
                        {
                            AnsiParams[AnsiParams.Length - 1] = AnsiParams[AnsiParams.Length - 1].Substring(0, AnsiParams[AnsiParams.Length - 1].Length - 1);
                            AnsiAttributesSave();
                            int RecY1 = AnsiProcess_Int0(AnsiParams[0], AnsiCmd_) - 1;
                            int RecX1 = AnsiProcess_Int0(AnsiParams[1], AnsiCmd_) - 1;
                            int RecY2 = AnsiProcess_Int0(AnsiParams[2], AnsiCmd_) - 1;
                            int RecX2 = AnsiProcess_Int0(AnsiParams[3], AnsiCmd_) - 1;
                            int Attr = AnsiProcess_Int0(AnsiParams[4], AnsiCmd_);
                            if (AnsiState_.__AnsiOrigin)
                            {
                                RecY1 += AnsiState_.__AnsiScrollFirst;
                                RecY2 += AnsiState_.__AnsiScrollFirst;
                                if (AnsiState_.__AnsiMarginLeftRight)
                                {
                                    RecX1 += AnsiState_.__AnsiMarginLeft;
                                    RecX2 += AnsiState_.__AnsiMarginLeft;
                                }
                            }

                            for (int Y = RecY1; Y <= RecY2; Y++)
                            {
                                for (int X = RecX1; X <= RecX2; X++)
                                {
                                    int TempC, TempB, TempF, TempA;
                                    AnsiGetF(X, Y, out TempC, out TempB, out TempF, out TempA);
                                    switch (Attr)
                                    {
                                        case 1: // Bold
                                            if ((TempA & 1) > 0)
                                            {
                                                TempA = TempA & 0xFE;
                                            }
                                            else
                                            {
                                                TempA = TempA | 0x01;
                                            }
                                            break;
                                        case 3: // Italic
                                            if ((TempA & 2) > 0)
                                            {
                                                TempA = TempA & 0xFD;
                                            }
                                            else
                                            {
                                                TempA = TempA | 0x02;
                                            }
                                            break;
                                        case 4: // Underline
                                            if ((TempA & 4) > 0)
                                            {
                                                TempA = TempA & 0xFB;
                                            }
                                            else
                                            {
                                                TempA = TempA | 0x04;
                                            }
                                            break;
                                        case 5: // Blink
                                            if ((TempA & 8) > 0)
                                            {
                                                TempA = TempA & 0xF7;
                                            }
                                            else
                                            {
                                                TempA = TempA | 0x08;
                                            }
                                            break;
                                        case 7: // Inverse
                                            if ((TempA & 16) > 0)
                                            {
                                                TempA = TempA & 0xEF;
                                            }
                                            else
                                            {
                                                TempA = TempA | 0x10;
                                            }
                                            break;
                                        case 8: // Invisible
                                            if ((TempA & 32) > 0)
                                            {
                                                TempA = TempA & 0xDF;
                                            }
                                            else
                                            {
                                                TempA = TempA | 0x20;
                                            }
                                            break;
                                        case 9: // Strikethrough
                                            if ((TempA & 64) > 0)
                                            {
                                                TempA = TempA & 0xBF;
                                            }
                                            else
                                            {
                                                TempA = TempA | 0x40;
                                            }
                                            break;
                                    }
                                    AnsiState_.__AnsiBack = TempB;
                                    AnsiState_.__AnsiFore = TempF;
                                    AnsiState_.__AnsiAttr = TempA;
                                    AnsiCharF(X, Y, TempC);
                                }
                                AnsiRepaintLine(Y);
                            }
                            AnsiAttributesLoad();
                        }
                    }
                    else
                    {
                        {
                            switch (AnsiProcess_Int0(AnsiParams[0], AnsiCmd_))
                            {
                                case 11:
                                case 13:
                                case 14:
                                case 15:
                                case 16:
                                case 18:
                                case 19:
                                case 20:
                                case 21:
                                case 22:
                                case 23:
                                    __AnsiResponse.Add(AnsiCmd_);
                                    break;
                                default:
                                    {
                                        int S = AnsiProcess_Int0(AnsiParams[0], AnsiCmd_);
                                        if (S >= 24)
                                        {
                                            AnsiResize(-1, S);
                                        }
                                    }
                                    break;
                            }
                        }
                        if (AnsiParams.Length == 3)
                        {
                            switch (AnsiProcess_Int0(AnsiParams[0], AnsiCmd_))
                            {
                                case 4:
                                    {
                                        int CmdW = AnsiProcess_Int0(AnsiParams[2], AnsiCmd_) / Screen.TerminalCellW;
                                        int CmdH = AnsiProcess_Int0(AnsiParams[1], AnsiCmd_) / Screen.TerminalCellH;
                                        if ((CmdW > 0) && (CmdH > 0))
                                        {
                                            AnsiResize(CmdW, CmdH);
                                        }
                                        else
                                        {
                                            if (CmdW > 0)
                                            {
                                                AnsiResize(CmdW, -1);
                                            }
                                            if (CmdH > 0)
                                            {
                                                AnsiResize(-1, CmdH);
                                            }
                                        }
                                    }
                                    break;
                                case 8:
                                    {
                                        int CmdW = AnsiProcess_Int0(AnsiParams[2], AnsiCmd_);
                                        int CmdH = AnsiProcess_Int0(AnsiParams[1], AnsiCmd_);
                                        if ((CmdW > 0) && (CmdH > 0))
                                        {
                                            AnsiResize(CmdW, CmdH);
                                        }
                                        else
                                        {
                                            if (CmdW > 0)
                                            {
                                                AnsiResize(CmdW, -1);
                                            }
                                            if (CmdH > 0)
                                            {
                                                AnsiResize(-1, CmdH);
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                case 'v':
                    // DECCRA
                    if (AnsiCmd_.EndsWith("$v"))
                    {
                        if (AnsiParams.Length >= 8)
                        {
                            AnsiParams[AnsiParams.Length - 1] = AnsiParams[AnsiParams.Length - 1].Substring(0, AnsiParams[AnsiParams.Length - 1].Length - 1);
                            AnsiAttributesSave();
                            int RecY1 = AnsiProcess_Int0(AnsiParams[0], AnsiCmd_) - 1;
                            int RecX1 = AnsiProcess_Int0(AnsiParams[1], AnsiCmd_) - 1;
                            int RecY2 = AnsiProcess_Int0(AnsiParams[2], AnsiCmd_) - 1;
                            int RecX2 = AnsiProcess_Int0(AnsiParams[3], AnsiCmd_) - 1;
                            int TargetY = AnsiProcess_Int0(AnsiParams[5], AnsiCmd_) - 1;
                            int TargetX = AnsiProcess_Int0(AnsiParams[6], AnsiCmd_) - 1;
                            TargetX = TargetX - RecX1;
                            TargetY = TargetY - RecY1;
                            if (AnsiState_.__AnsiOrigin)
                            {
                                RecY1 += AnsiState_.__AnsiScrollFirst;
                                RecY2 += AnsiState_.__AnsiScrollFirst;
                                if (AnsiState_.__AnsiMarginLeftRight)
                                {
                                    RecX1 += AnsiState_.__AnsiMarginLeft;
                                    RecX2 += AnsiState_.__AnsiMarginLeft;
                                }
                            }

                            if (TargetY >= 0)
                            {
                                for (int Y = RecY2; Y >= RecY1; Y--)
                                {
                                    if (TargetX >= 0)
                                    {
                                        for (int X = RecX2; X >= RecX1; X--)
                                        {
                                            int TempC, TempB, TempF, TempA;
                                            AnsiGetF(X, Y, out TempC, out TempB, out TempF, out TempA);
                                            AnsiState_.__AnsiBack = TempB;
                                            AnsiState_.__AnsiFore = TempF;
                                            AnsiState_.__AnsiAttr = TempA;
                                            AnsiCharF(X + TargetX, Y + TargetY, TempC);
                                        }
                                    }
                                    else
                                    {
                                        for (int X = RecX1; X <= RecX2; X++)
                                        {
                                            int TempC, TempB, TempF, TempA;
                                            AnsiGetF(X, Y, out TempC, out TempB, out TempF, out TempA);
                                            AnsiState_.__AnsiBack = TempB;
                                            AnsiState_.__AnsiFore = TempF;
                                            AnsiState_.__AnsiAttr = TempA;
                                            AnsiCharF(X + TargetX, Y + TargetY, TempC);
                                        }
                                    }
                                    AnsiRepaintLine(Y + TargetY);
                                }
                            }
                            else
                            {
                                for (int Y = RecY1; Y <= RecY2; Y++)
                                {
                                    if (TargetX >= 0)
                                    {
                                        for (int X = RecX2; X >= RecX1; X--)
                                        {
                                            int TempC, TempB, TempF, TempA;
                                            AnsiGetF(X, Y, out TempC, out TempB, out TempF, out TempA);
                                            AnsiState_.__AnsiBack = TempB;
                                            AnsiState_.__AnsiFore = TempF;
                                            AnsiState_.__AnsiAttr = TempA;
                                            AnsiCharF(X + TargetX, Y + TargetY, TempC);
                                        }
                                    }
                                    else
                                    {
                                        for (int X = RecX1; X <= RecX2; X++)
                                        {
                                            int TempC, TempB, TempF, TempA;
                                            AnsiGetF(X, Y, out TempC, out TempB, out TempF, out TempA);
                                            AnsiState_.__AnsiBack = TempB;
                                            AnsiState_.__AnsiFore = TempF;
                                            AnsiState_.__AnsiAttr = TempA;
                                            AnsiCharF(X + TargetX, Y + TargetY, TempC);
                                        }
                                    }
                                    AnsiRepaintLine(Y + TargetY);
                                }
                            }
                            AnsiAttributesLoad();
                        }
                    }
                    break;
                case 'x':
                    // DECFRA
                    if (AnsiCmd_.EndsWith("$x"))
                    {
                        if (AnsiParams.Length >= 5)
                        {
                            AnsiParams[AnsiParams.Length - 1] = AnsiParams[AnsiParams.Length - 1].Substring(0, AnsiParams[AnsiParams.Length - 1].Length - 1);
                            AnsiAttributesSave();
                            int FillC = AnsiProcess_Int0(AnsiParams[0], AnsiCmd_);
                            int RecY1 = AnsiProcess_Int0(AnsiParams[1], AnsiCmd_) - 1;
                            int RecX1 = AnsiProcess_Int0(AnsiParams[2], AnsiCmd_) - 1;
                            int RecY2 = AnsiProcess_Int0(AnsiParams[3], AnsiCmd_) - 1;
                            int RecX2 = AnsiProcess_Int0(AnsiParams[4], AnsiCmd_) - 1;
                            if (AnsiState_.__AnsiOrigin)
                            {
                                RecY1 += AnsiState_.__AnsiScrollFirst;
                                RecY2 += AnsiState_.__AnsiScrollFirst;
                                if (AnsiState_.__AnsiMarginLeftRight)
                                {
                                    RecX1 += AnsiState_.__AnsiMarginLeft;
                                    RecX2 += AnsiState_.__AnsiMarginLeft;
                                }
                            }

                            FillC = AnsiState_.GetChar(FillC);

                            int FillCDbl = Core_.Screen_.CharDouble(FillC);
                            if (FillCDbl != 0)
                            {
                                for (int Y = RecY1; Y <= RecY2; Y++)
                                {
                                    for (int X = RecX1; X < RecX2; X += 2)
                                    {
                                        AnsiCharF(X + 0, Y, FillC);
                                        AnsiCharF(X + 1, Y, FillCDbl);
                                    }
                                    AnsiRepaintLine(Y);
                                }
                            }
                            else
                            {
                                for (int Y = RecY1; Y <= RecY2; Y++)
                                {
                                    for (int X = RecX1; X <= RecX2; X++)
                                    {
                                        AnsiCharF(X, Y, FillC);
                                    }
                                    AnsiRepaintLine(Y);
                                }
                            }
                            AnsiAttributesLoad();
                        }
                    }
                    break;
                case 'y':
                    // DECRQCRA
                    if (AnsiCmd_.EndsWith("*y"))
                    {
                        __AnsiResponse.Add(AnsiCmd_);
                    }
                    break;
                case 'z':
                    // DECERA
                    if (AnsiCmd_.EndsWith("$z"))
                    {
                        if (AnsiParams.Length >= 4)
                        {
                            AnsiParams[AnsiParams.Length - 1] = AnsiParams[AnsiParams.Length - 1].Substring(0, AnsiParams[AnsiParams.Length - 1].Length - 1);
                            AnsiAttributesSave();
                            int RecY1 = AnsiProcess_Int0(AnsiParams[0], AnsiCmd_) - 1;
                            int RecX1 = AnsiProcess_Int0(AnsiParams[1], AnsiCmd_) - 1;
                            int RecY2 = AnsiProcess_Int0(AnsiParams[2], AnsiCmd_) - 1;
                            int RecX2 = AnsiProcess_Int0(AnsiParams[3], AnsiCmd_) - 1;
                            if (AnsiState_.__AnsiOrigin)
                            {
                                RecY1 += AnsiState_.__AnsiScrollFirst;
                                RecY2 += AnsiState_.__AnsiScrollFirst;
                                if (AnsiState_.__AnsiMarginLeftRight)
                                {
                                    RecX1 += AnsiState_.__AnsiMarginLeft;
                                    RecX2 += AnsiState_.__AnsiMarginLeft;
                                }
                            }

                            for (int Y = RecY1; Y <= RecY2; Y++)
                            {
                                for (int X = RecX1; X <= RecX2; X++)
                                {
                                    int TempC, TempB, TempF, TempA;
                                    AnsiGetF(X, Y, out TempC, out TempB, out TempF, out TempA);
                                    AnsiState_.__AnsiBack = TempB;
                                    AnsiState_.__AnsiFore = TempF;
                                    AnsiState_.__AnsiAttr = TempA;
                                    AnsiCharF(X, Y, ' ');
                                }
                                AnsiRepaintLine(Y);
                            }
                            AnsiAttributesLoad();
                        }
                    }
                    break;
                case '{':
                    // DECSERA
                    if (AnsiCmd_.EndsWith("${"))
                    {
                        if (AnsiParams.Length >= 4)
                        {
                            AnsiParams[AnsiParams.Length - 1] = AnsiParams[AnsiParams.Length - 1].Substring(0, AnsiParams[AnsiParams.Length - 1].Length - 1);
                            AnsiAttributesSave();
                            int RecY1 = AnsiProcess_Int0(AnsiParams[0], AnsiCmd_) - 1;
                            int RecX1 = AnsiProcess_Int0(AnsiParams[1], AnsiCmd_) - 1;
                            int RecY2 = AnsiProcess_Int0(AnsiParams[2], AnsiCmd_) - 1;
                            int RecX2 = AnsiProcess_Int0(AnsiParams[3], AnsiCmd_) - 1;
                            if (AnsiState_.__AnsiOrigin)
                            {
                                RecY1 += AnsiState_.__AnsiScrollFirst;
                                RecY2 += AnsiState_.__AnsiScrollFirst;
                                if (AnsiState_.__AnsiMarginLeftRight)
                                {
                                    RecX1 += AnsiState_.__AnsiMarginLeft;
                                    RecX2 += AnsiState_.__AnsiMarginLeft;
                                }
                            }

                            for (int Y = RecY1; Y <= RecY2; Y++)
                            {
                                for (int X = RecX1; X <= RecX2; X++)
                                {
                                    int TempC, TempB, TempF, TempA;
                                    AnsiGetF(X, Y, out TempC, out TempB, out TempF, out TempA);
                                    AnsiState_.__AnsiBack = TempB;
                                    AnsiState_.__AnsiFore = TempF;
                                    AnsiState_.__AnsiAttr = TempA;
                                    AnsiCharFUnprotected1(X, Y, ' ');
                                }
                                AnsiRepaintLine(Y);
                            }
                            AnsiAttributesLoad();
                        }
                    }
                    break;
                case '|':
                    {
                        // DECSNLS
                        if ((AnsiCmd_.Length > 2) && (AnsiCmd_.EndsWith("*|")))
                        {
                            AnsiResize(-1, AnsiProcess_Int1(AnsiParams[0].Substring(0, AnsiParams[0].Length - 1), AnsiCmd_));
                        }
                    }
                    break;
                case '}':
                    // DECIC
                    if (AnsiCmd_.EndsWith("'}"))
                    {
                        AnsiState_.__AnsiWrapFlag = false;
                        if (AnsiParams[0].Length > 1)
                        {
                            AnsiParams[0] = AnsiParams[0].Substring(0, AnsiParams[0].Length - 1);
                        }
                        else
                        {
                            AnsiParams[0] = "1";
                        }
                        int __AnsiX = AnsiState_.__AnsiX;
                        if ((!AnsiState_.__AnsiMarginLeftRight) || ((__AnsiX >= AnsiState_.__AnsiMarginLeft) && (__AnsiX < AnsiState_.__AnsiMarginRight)))
                        {
                            if (AnsiState_.__AnsiOrigin || ((AnsiState_.__AnsiY >= AnsiState_.__AnsiScrollFirst) && (AnsiState_.__AnsiY <= AnsiState_.__AnsiScrollLast)))
                            {
                                for (int __AnsiY = AnsiState_.__AnsiScrollFirst; __AnsiY <= AnsiState_.__AnsiScrollLast; __AnsiY++)
                                {
                                    int InsCycle = AnsiProcess_Int1(AnsiParams[0], AnsiCmd_);
                                    if (AnsiState_.__AnsiLineOccupy__.CountLines() > __AnsiY)
                                    {
                                        int FontSize = AnsiGetFontSize(__AnsiY);
                                        while (InsCycle > 0)
                                        {
                                            int TempC = 0, TempB = 0, TempF = 0, TempA = 0;
                                            if (FontSize > 0)
                                            {
                                                AnsiGetF(__AnsiX, __AnsiY, out TempC, out TempB, out TempF, out TempA);
                                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) > (__AnsiX * 2))
                                                {
                                                    AnsiState_.__AnsiLineOccupy__.BlankChar(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);
                                                    AnsiState_.__AnsiLineOccupy__.Item_FontW = 2;
                                                    AnsiState_.__AnsiLineOccupy__.Item_FontH = FontSize - 1;
                                                    AnsiState_.__AnsiLineOccupy__.Insert(__AnsiY, __AnsiX * 2);
                                                    AnsiState_.__AnsiLineOccupy__.Item_FontW = 1;
                                                    AnsiState_.__AnsiLineOccupy__.Insert(__AnsiY, __AnsiX * 2);
                                                }
                                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) > (AnsiProcessGetXMax(false)))
                                                {
                                                    AnsiState_.__AnsiLineOccupy__.Delete(__AnsiY, AnsiProcessGetXMax(false));
                                                    AnsiState_.__AnsiLineOccupy__.Delete(__AnsiY, AnsiProcessGetXMax(false));
                                                }
                                            }
                                            else
                                            {
                                                AnsiGetF(__AnsiX, __AnsiY, out TempC, out TempB, out TempF, out TempA);
                                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) > (__AnsiX))
                                                {
                                                    AnsiState_.__AnsiLineOccupy__.BlankChar(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);

                                                    AnsiState_.__AnsiLineOccupy__.Insert(__AnsiY, __AnsiX);
                                                }
                                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) > (AnsiProcessGetXMax(false)))
                                                {
                                                    AnsiState_.__AnsiLineOccupy__.Delete(__AnsiY, AnsiProcessGetXMax(false));
                                                }
                                            }
                                            InsCycle--;
                                        }
                                        AnsiRepaintLine(__AnsiY);
                                    }
                                    AnsiState_.PrintCharInsDel++;
                                }
                            }
                        }
                    }
                    break;
                case '~':
                    // DECDC
                    if (AnsiCmd_.EndsWith("'~"))
                    {
                        AnsiState_.__AnsiWrapFlag = false;
                        if (AnsiParams[0].Length > 1)
                        {
                            AnsiParams[0] = AnsiParams[0].Substring(0, AnsiParams[0].Length - 1);
                        }
                        else
                        {
                            AnsiParams[0] = "1";
                        }
                        int __AnsiX = AnsiState_.__AnsiX;
                        if ((!AnsiState_.__AnsiMarginLeftRight) || ((__AnsiX >= AnsiState_.__AnsiMarginLeft) && (__AnsiX < AnsiState_.__AnsiMarginRight)))
                        {
                            if (AnsiState_.__AnsiOrigin || ((AnsiState_.__AnsiY >= AnsiState_.__AnsiScrollFirst) && (AnsiState_.__AnsiY <= AnsiState_.__AnsiScrollLast)))
                            {
                                for (int __AnsiY = AnsiState_.__AnsiScrollFirst; __AnsiY <= AnsiState_.__AnsiScrollLast; __AnsiY++)
                                {
                                    int DelCycle = AnsiProcess_Int1(AnsiParams[0], AnsiCmd_);
                                    if (AnsiState_.__AnsiLineOccupy__.CountLines() > __AnsiY)
                                    {
                                        while (DelCycle > 0)
                                        {
                                            int TempC = 0, TempB = 0, TempF = 0, TempA = 0;
                                            if (AnsiGetFontSize(__AnsiY) > 0)
                                            {
                                                AnsiGetF((AnsiProcessGetXMax(false)) - 1, __AnsiY, out TempC, out TempB, out TempF, out TempA);
                                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) >= ((AnsiProcessGetXMax(false))))
                                                {
                                                    AnsiState_.__AnsiLineOccupy__.BlankChar(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);

                                                    AnsiState_.__AnsiLineOccupy__.Insert(__AnsiY, (AnsiProcessGetXMax(false)));
                                                    AnsiState_.__AnsiLineOccupy__.Insert(__AnsiY, (AnsiProcessGetXMax(false)));
                                                }
                                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) > (__AnsiX))
                                                {
                                                    AnsiState_.__AnsiLineOccupy__.Delete(__AnsiY, __AnsiX);
                                                    AnsiState_.__AnsiLineOccupy__.Delete(__AnsiY, __AnsiX);
                                                }
                                                if (TempC > 0)
                                                {
                                                    AnsiCharF((AnsiProcessGetXMax(false) / 2) - 1, __AnsiY, 32);
                                                }
                                            }
                                            else
                                            {
                                                AnsiGetF(AnsiProcessGetXMax(false) - 1, __AnsiY, out TempC, out TempB, out TempF, out TempA);
                                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) >= ((AnsiProcessGetXMax(false))))
                                                {
                                                    AnsiState_.__AnsiLineOccupy__.BlankChar(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);

                                                    AnsiState_.__AnsiLineOccupy__.Insert(__AnsiY, (AnsiProcessGetXMax(false)));
                                                }
                                                if (AnsiState_.__AnsiLineOccupy__.CountItems(__AnsiY) > (__AnsiX))
                                                {
                                                    AnsiState_.__AnsiLineOccupy__.Delete(__AnsiY, __AnsiX);
                                                }
                                                if (TempC > 0)
                                                {
                                                    AnsiCharF(AnsiProcessGetXMax(false) - 1, __AnsiY, 32);
                                                }
                                            }
                                            DelCycle--;
                                        }
                                        AnsiRepaintLine(__AnsiY);
                                    }
                                    AnsiState_.PrintCharInsDel++;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        void AnsiProcess_CSI_m(string[] AnsiParams)
        {
            if (AnsiState_.StatusBar)
            {
                return;
            }

            for (int i_ = 0; i_ < AnsiParams.Length; i_++)
            {
                switch (AnsiParams[i_])
                {
                    case "0":
                    case "00":
                    case "":
                        AnsiState_.__AnsiFore = -1;
                        AnsiState_.__AnsiBack = -1;
                        AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr & 0x80;
                        break;

                    // Bold
                    case "1": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr | 0x01; break;
                    case "22": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr & 0xFE; break;

                    // Italic
                    case "3": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr | 0x02; break;
                    case "23": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr & 0xFD; break;

                    // Underline
                    case "4": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr | 0x04; break;
                    case "24": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr & 0xFB; break;

                    // Blink
                    case "5": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr | 0x08; break;
                    case "25": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr & 0xF7; break;

                    // Reverse
                    case "7": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr | 0x10; break;
                    case "27": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr & 0xEF; break;

                    // Conceale
                    case "8": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr | 0x20; break;
                    case "28": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr & 0xDF; break;

                    // Strikethrough
                    case "9": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr | 0x40; break;
                    case "29": AnsiState_.__AnsiAttr = AnsiState_.__AnsiAttr & 0xBF; break;

                    // Default color
                    case "39": AnsiState_.__AnsiFore = -1; break;
                    case "49": AnsiState_.__AnsiBack = -1; break;

                    // Foreground color
                    case "30": AnsiState_.__AnsiFore = 0; break;
                    case "31": AnsiState_.__AnsiFore = 1; break;
                    case "32": AnsiState_.__AnsiFore = 2; break;
                    case "33": AnsiState_.__AnsiFore = 3; break;
                    case "34": AnsiState_.__AnsiFore = 4; break;
                    case "35": AnsiState_.__AnsiFore = 5; break;
                    case "36": AnsiState_.__AnsiFore = 6; break;
                    case "37": AnsiState_.__AnsiFore = 7; break;
                    case "90": AnsiState_.__AnsiFore = 8; break;
                    case "91": AnsiState_.__AnsiFore = 9; break;
                    case "92": AnsiState_.__AnsiFore = 10; break;
                    case "93": AnsiState_.__AnsiFore = 11; break;
                    case "94": AnsiState_.__AnsiFore = 12; break;
                    case "95": AnsiState_.__AnsiFore = 13; break;
                    case "96": AnsiState_.__AnsiFore = 14; break;
                    case "97": AnsiState_.__AnsiFore = 15; break;

                    // Background color
                    case "40": AnsiState_.__AnsiBack = 0; break;
                    case "41": AnsiState_.__AnsiBack = 1; break;
                    case "42": AnsiState_.__AnsiBack = 2; break;
                    case "43": AnsiState_.__AnsiBack = 3; break;
                    case "44": AnsiState_.__AnsiBack = 4; break;
                    case "45": AnsiState_.__AnsiBack = 5; break;
                    case "46": AnsiState_.__AnsiBack = 6; break;
                    case "47": AnsiState_.__AnsiBack = 7; break;
                    case "100": AnsiState_.__AnsiBack = 8; break;
                    case "101": AnsiState_.__AnsiBack = 9; break;
                    case "102": AnsiState_.__AnsiBack = 10; break;
                    case "103": AnsiState_.__AnsiBack = 11; break;
                    case "104": AnsiState_.__AnsiBack = 12; break;
                    case "105": AnsiState_.__AnsiBack = 13; break;
                    case "106": AnsiState_.__AnsiBack = 14; break;
                    case "107": AnsiState_.__AnsiBack = 15; break;

                    case "38":
                        {
                            if (AnsiParams.Length > i_ + 2)
                            {
                                if ("5".Equals(AnsiParams[i_ + 1]))
                                {
                                    try
                                    {
                                        AnsiState_.__AnsiFore = Color256[int.Parse(AnsiParams[i_ + 2])];
                                    }
                                    catch
                                    {
                                        AnsiState_.__AnsiFore = -1;
                                    }
                                    i_ += 2;
                                }
                            }
                            if (AnsiParams.Length > i_ + 4)
                            {
                                if ("2".Equals(AnsiParams[i_ + 1]))
                                {
                                    try
                                    {
                                        int TempR = int.Parse(AnsiParams[i_ + 2]);
                                        int TempG = int.Parse(AnsiParams[i_ + 3]);
                                        int TempB = int.Parse(AnsiParams[i_ + 4]);
                                        AnsiState_.__AnsiFore = AnsiColor16(TempR, TempG, TempB);
                                    }
                                    catch
                                    {
                                        AnsiState_.__AnsiFore = -1;
                                    }
                                    i_ += 4;
                                }
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
                                        AnsiState_.__AnsiBack = Color256[int.Parse(AnsiParams[i_ + 2])];
                                    }
                                    catch
                                    {
                                        AnsiState_.__AnsiBack = -1;
                                    }
                                    i_ += 2;
                                }
                            }
                            if (AnsiParams.Length > i_ + 4)
                            {
                                if ("2".Equals(AnsiParams[i_ + 1]))
                                {
                                    try
                                    {
                                        int TempR = int.Parse(AnsiParams[i_ + 2]);
                                        int TempG = int.Parse(AnsiParams[i_ + 3]);
                                        int TempB = int.Parse(AnsiParams[i_ + 4]);
                                        AnsiState_.__AnsiBack = AnsiColor16(TempR, TempG, TempB);
                                    }
                                    catch
                                    {
                                        AnsiState_.__AnsiBack = -1;
                                    }
                                    i_ += 4;
                                }
                            }
                        }
                        break;

                    default:
                        if (AnsiParams[i_].StartsWith("38:2:", StringComparison.Ordinal))
                        {
                            string[] AnsiParamsX = AnsiParams[i_].Split(':');
                            if (AnsiParamsX.Length >= 5)
                            {
                                try
                                {
                                    int TempR = int.Parse(AnsiParamsX[2]);
                                    int TempG = int.Parse(AnsiParamsX[3]);
                                    int TempB = int.Parse(AnsiParamsX[4]);
                                    AnsiState_.__AnsiFore = AnsiColor16(TempR, TempG, TempB);
                                }
                                catch
                                {
                                    AnsiState_.__AnsiFore = -1;
                                }
                            }
                        }
                        if (AnsiParams[i_].StartsWith("48:2:", StringComparison.Ordinal))
                        {
                            string[] AnsiParamsX = AnsiParams[i_].Split(':');
                            if (AnsiParamsX.Length >= 5)
                            {
                                try
                                {
                                    int TempR = int.Parse(AnsiParamsX[2]);
                                    int TempG = int.Parse(AnsiParamsX[3]);
                                    int TempB = int.Parse(AnsiParamsX[4]);
                                    AnsiState_.__AnsiBack = AnsiColor16(TempR, TempG, TempB);
                                }
                                catch
                                {
                                    AnsiState_.__AnsiBack = -1;
                                }
                            }
                        }
                        if (AnsiParams[i_].StartsWith("38:5:", StringComparison.Ordinal))
                        {
                            string[] AnsiParamsX = AnsiParams[i_].Split(':');
                            if (AnsiParamsX.Length >= 3)
                            {
                                try
                                {
                                    AnsiState_.__AnsiFore = Color256[int.Parse(AnsiParamsX[2])];
                                }
                                catch
                                {
                                    AnsiState_.__AnsiFore = -1;
                                }
                            }
                        }
                        if (AnsiParams[i_].StartsWith("48:5:", StringComparison.Ordinal))
                        {
                            string[] AnsiParamsX = AnsiParams[i_].Split(':');
                            if (AnsiParamsX.Length >= 3)
                            {
                                try
                                {
                                    AnsiState_.__AnsiBack = Color256[int.Parse(AnsiParamsX[2])];
                                }
                                catch
                                {
                                    AnsiState_.__AnsiBack = -1;
                                }
                            }
                        }
                        break;
                }
            }
        }

        int AnsiProcessGetXMin(bool Origin)
        {
            if (((!Origin) || AnsiState_.__AnsiOrigin) && AnsiState_.__AnsiMarginLeftRight)
            {
                return AnsiState_.__AnsiMarginLeft;
            }
            else
            {
                return 0;
            }
        }

        int AnsiProcessGetXMax(bool Origin)
        {
            if (((!Origin) || AnsiState_.__AnsiOrigin) && AnsiState_.__AnsiMarginLeftRight)
            {
                return AnsiState_.__AnsiMarginRight;
            }
            else
            {
                return AnsiMaxX;
            }
        }

        public void AnsiRepaintCursor()
        {
            if (AnsiGetFontSize(AnsiState_.__AnsiY) > 0)
            {
                Core_.Screen_.SetCursorPosition(AnsiState_.__AnsiX << 1, AnsiState_.__AnsiY);
            }
            else
            {
                Core_.Screen_.SetCursorPosition(AnsiState_.__AnsiX, AnsiState_.__AnsiY);
            }
        }

        private bool AnsiCharNotCmd(int CharCode)
        {
            if (ANSI8bit)
            {
                if ((CharCode >= 0x80) && (CharCode <= 0x9F))
                {
                    AnsiState_.__AnsiCmd.Clear();
                    AnsiState_.__AnsiCmd.Add(CharCode - 0x40);
                    AnsiState_.__AnsiCommand = true;
                    return false;
                }
            }
            return true;
        }

        private int AnsiCharPrintLast = -1;
        private int AnsiCharPrintRepeater = 0;

        private void AnsiCharPrint(int TextFileLine_i)
        {
            if (AnsiState_.StatusBar)
            {
                return;
            }

            int TextFileLine_i_GetChar = TextFileLine_i;
            if (!ANSIDOS_)
            {
                if (!AnsiState_.__AnsiVT52)
                {
                    TextFileLine_i_GetChar = AnsiState_.GetChar(TextFileLine_i);
                }

                if ((!ANSIDOS_) && (TextFileLine_i_GetChar == 127))
                {
                    return;
                }
            }

            if ((!AnsiState_.__AnsiMusic) && (TextFileLine_i < 32) && (ANSIDOS_))
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
                    case 9:
                        if (ANSIPrintTab)
                        {
                            TextFileLine_i = DosControl[TextFileLine_i];
                        }
                        break;
                    default:
                        TextFileLine_i = DosControl[TextFileLine_i];
                        break;
                }
            }
            if ((TextFileLine_i >= 32))
            {
                if (AnsiState_.__AnsiVT52)
                {
                    if (AnsiState_.VT52_SemigraphDef)
                    {
                        if ((TextFileLine_i >= 95) && (TextFileLine_i <= 126))
                        {
                            TextFileLine_i = VT52_SemigraphChars[TextFileLine_i - 95];
                        }
                    }
                }
                else
                {
                    TextFileLine_i = TextFileLine_i_GetChar;
                }

                if (!AnsiState_.__AnsiMusic)
                {
                    int TextFileLine_i_dbl = Core_.Screen_.CharDouble(TextFileLine_i);
                    if (ANSIDOS_)
                    {
                        AnsiCharPrintLast = TextFileLine_i;
                        AnsiCharFI(AnsiState_.__AnsiX, AnsiState_.__AnsiY, TextFileLine_i, AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);
                        AnsiState_.__AnsiX++;
                        if ((TextFileLine_i_dbl != 0) && (AnsiState_.__AnsiX < AnsiProcessGetXMax(false)))
                        {
                            AnsiCharFI(AnsiState_.__AnsiX, AnsiState_.__AnsiY, TextFileLine_i_dbl, AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);
                            AnsiState_.__AnsiX++;
                        }
                        if ((AnsiMaxX > 0) && (AnsiState_.__AnsiX >= AnsiProcessGetXMax(false)))
                        {
                            if (AnsiState_.__AnsiNoWrap)
                            {
                                AnsiState_.__AnsiX--;
                            }
                            else
                            {
                                AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                                AnsiState_.__AnsiY++;
                                if ((AnsiMaxY > 0) && (AnsiState_.__AnsiY > AnsiState_.__AnsiScrollLast))
                                {
                                    int L = AnsiState_.__AnsiY - AnsiState_.__AnsiScrollLast;
                                    AnsiState_.__AnsiY = AnsiState_.__AnsiScrollLast;
                                    AnsiScrollInit(L, AnsiState.AnsiScrollCommandDef.None);
                                }
                            }
                        }
                    }
                    else
                    {
                        bool CharNoScroll = true;
                        if (AnsiState_.__AnsiWrapFlag)
                        {
                            if (!AnsiState_.__AnsiNoWrap)
                            {
                                AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                                AnsiState_.__AnsiY++;

                                if ((AnsiMaxY > 0) && (AnsiState_.__AnsiY > AnsiState_.__AnsiScrollLast))
                                {
                                    int L = AnsiState_.__AnsiY - AnsiState_.__AnsiScrollLast;
                                    AnsiState_.__AnsiY = AnsiState_.__AnsiScrollLast;
                                    AnsiScrollInit(L, AnsiState.AnsiScrollCommandDef.Char, TextFileLine_i, AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);
                                    CharNoScroll = false;
                                }
                            }
                            AnsiState_.__AnsiWrapFlag = false;
                        }
                        if (CharNoScroll)
                        {
                            AnsiCharPrintLast = TextFileLine_i;
                            AnsiCharFI(AnsiState_.__AnsiX, AnsiState_.__AnsiY, TextFileLine_i, AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);
                            AnsiState_.__AnsiX++;
                            if ((TextFileLine_i_dbl != 0) && (AnsiState_.__AnsiX < AnsiProcessGetXMax(false)))
                            {
                                AnsiCharFI(AnsiState_.__AnsiX, AnsiState_.__AnsiY, TextFileLine_i_dbl, AnsiState_.__AnsiBack, AnsiState_.__AnsiFore, AnsiState_.__AnsiAttr);
                                AnsiState_.__AnsiX++;
                            }
                            if ((AnsiMaxX > 0) && (AnsiState_.__AnsiX >= AnsiProcessGetXMax(false)))
                            {
                                if (!AnsiState_.__AnsiNoWrap)
                                {
                                    AnsiState_.__AnsiWrapFlag = true;
                                }
                                AnsiState_.__AnsiX--;
                            }
                        }
                    }
                }
            }
            else
            {
                if (AnsiState_.__AnsiMusic)
                {
                    switch (TextFileLine_i)
                    {
                        case 14:
                            {
                                if (AnsiState_.__AnsiMusic)
                                {
                                    AnsiState_.__AnsiMusic = false;
                                }
                            }
                            break;
                    }
                }
                else
                {
                    switch (TextFileLine_i)
                    {
                        case 5:
                            if (!ANSIDOS_)
                            {
                                __AnsiResponse.Add("AnswerBack");
                            }
                            break;
                        case 7:
                            if (!AnsiState_.__AnsiCommand)
                            {
                                AnsiState_.AnsiRingBellCount++;
                                if (AnsiRingBell)
                                {
                                    Core_.Screen_.Bell();
                                }
                            }
                            break;
                        case 8:
                            {
                                if (!ANSIPrintBackspace)
                                {
                                    AnsiState_.__AnsiWrapFlag = false;
                                    if (AnsiState_.__AnsiX >= AnsiProcessGetXMax(false))
                                    {
                                        AnsiState_.__AnsiX = AnsiProcessGetXMax(false) - 1;
                                    }
                                    if (AnsiState_.__AnsiX > AnsiProcessGetXMin(false))
                                    {
                                        AnsiState_.__AnsiX--;
                                    }
                                }
                            }
                            break;
                        case 9:
                            {
                                if (!ANSIPrintTab)
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
                                        AnsiState_.__AnsiWrapFlag = false;
                                        AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                                        break;
                                    case 1:
                                        AnsiState_.__AnsiWrapFlag = false;
                                        AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                                        if (AnsiState_.__AnsiY == AnsiState_.__AnsiScrollLast)
                                        {
                                            AnsiScrollInit(1, AnsiState.AnsiScrollCommandDef.None);
                                        }
                                        else
                                        {
                                            AnsiState_.__AnsiY++;
                                        }
                                        break;
                                }
                            }
                            break;
                        case 12:
                            if (!ANSIDOS_)
                            {
                                switch (ANSI_LF)
                                {
                                    case 0:
                                        AnsiState_.__AnsiWrapFlag = false;
                                        if (AnsiState_.__AnsiY == AnsiState_.__AnsiScrollLast)
                                        {
                                            AnsiScrollInit(1, AnsiState.AnsiScrollCommandDef.None);
                                        }
                                        else
                                        {
                                            AnsiState_.__AnsiY++;
                                        }
                                        break;
                                    case 1:
                                        AnsiState_.__AnsiWrapFlag = false;
                                        AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                                        if (AnsiState_.__AnsiY == AnsiState_.__AnsiScrollLast)
                                        {
                                            AnsiScrollInit(1, AnsiState.AnsiScrollCommandDef.None);
                                        }
                                        else
                                        {
                                            AnsiState_.__AnsiY++;
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
                                        AnsiState_.__AnsiWrapFlag = false;
                                        if (AnsiState_.__AnsiY == AnsiState_.__AnsiScrollLast)
                                        {
                                            AnsiScrollInit(1, AnsiState.AnsiScrollCommandDef.None);
                                        }
                                        else
                                        {
                                            AnsiState_.__AnsiY++;
                                        }
                                        break;
                                    case 1:
                                        AnsiState_.__AnsiWrapFlag = false;
                                        AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                                        if (AnsiState_.__AnsiY == AnsiState_.__AnsiScrollLast)
                                        {
                                            AnsiScrollInit(1, AnsiState.AnsiScrollCommandDef.None);
                                        }
                                        else
                                        {
                                            AnsiState_.__AnsiY++;
                                        }
                                        break;
                                }
                            }
                            break;
                        case 11:
                            if (!ANSIDOS_)
                            {
                                AnsiState_.__AnsiWrapFlag = false;
                                AnsiState_.__AnsiY++;
                                if (AnsiMaxY > 0)
                                {
                                    if (AnsiState_.__AnsiY > AnsiState_.__AnsiScrollLast)
                                    {
                                        AnsiScrollInit(AnsiState_.__AnsiY - AnsiState_.__AnsiScrollLast, AnsiState.AnsiScrollCommandDef.None);
                                        AnsiState_.__AnsiY = AnsiState_.__AnsiScrollLast;
                                    }
                                }
                            }
                            break;
                        case 14: // SO, LS1
                            if (!ANSIDOS_)
                            {
                                AnsiState_.CharMapNumGL = 1;
                            }
                            break;
                        case 15: // SI, LS0
                            if (!ANSIDOS_)
                            {
                                AnsiState_.CharMapNumGL = 0;
                            }
                            break;
                        case 26:
                            if (AnsiState_.__AnsiUseEOF)
                            {
                                AnsiState_.__AnsiBeyondEOF = true;
                            }
                            break;
                    }
                }
            }
        }
    }
}
