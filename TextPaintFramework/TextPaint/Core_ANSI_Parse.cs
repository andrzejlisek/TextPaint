using System;
namespace TextPaint
{
    public partial class Core
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
                case '=':
                    __AnsiResponse.Add("NumpadKey_1");
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '>':
                    __AnsiResponse.Add("NumpadKey_0");
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
                case '<':
                    AnsiState_.__AnsiVT52 = false;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'A':
                    AnsiState_.__AnsiY -= 1;
                    if (AnsiState_.__AnsiY < 0)
                    {
                        AnsiState_.__AnsiY = 0;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'B':
                    AnsiState_.__AnsiY += 1;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'C':
                    AnsiState_.__AnsiX += 1;
                    if ((AnsiMaxX > 0) && (AnsiState_.__AnsiX >= AnsiMaxX))
                    {
                        if (ANSIDOS)
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
                case 'H':
                    AnsiState_.__AnsiX = 0;
                    AnsiState_.__AnsiY = 0;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'Y':
                    if (AnsiState_.__AnsiCmd.Count == 3)
                    {
                        AnsiState_.__AnsiY = AnsiState_.__AnsiCmd[1] - 32;
                        AnsiState_.__AnsiX = AnsiState_.__AnsiCmd[2] - 32;
                        AnsiState_.__AnsiCommand = false;
                    }
                    break;
                case 'd':
                    AnsiCalcColor();
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
                case 'E':
                case 'J':
                    if (AnsiState_.__AnsiCmd[0] == 'E')
                    {
                        AnsiState_.__AnsiX = 0;
                        AnsiState_.__AnsiY = 0;
                    }
                    AnsiCalcColor();
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
                case 'Z':
                    __AnsiResponse.Add("VT52:Z");
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'I':
                    AnsiState_.__AnsiY -= 1;
                    if (AnsiState_.__AnsiY < AnsiState_.__AnsiScrollFirst)
                    {
                        AnsiScrollInit(AnsiState_.__AnsiY - AnsiState_.__AnsiScrollFirst, AnsiState.AnsiScrollCommandDef.None);
                        AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'K':
                    for (int ii_ = AnsiState_.__AnsiX; ii_ < AnsiMaxX; ii_++)
                    {
                        AnsiChar(ii_, AnsiState_.__AnsiY, 32);
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'b':
                case 'c':
                    if (AnsiState_.__AnsiCmd.Count == 2)
                    {
                        AnsiState_.__AnsiCommand = false;
                    }
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
                                                    SetProcessDelay(AnsiProcess_Int(AnsiParams[1], AnsiCmd_));
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
                default:
                    AnsiState_.__AnsiCommand = false;
                    break;
            }
        }

        void AnsiProcess_Fixed(int TextFileLine_i)
        {
            switch (AnsiState_.__AnsiCmd[0])
            {
                case '(':
                case ')':
                case '*':
                case '+':
                    if (AnsiState_.__AnsiCmd.Count == 2)
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
                        if ((AnsiState_.__AnsiCmd[1] == '0') || (AnsiState_.__AnsiCmd[1] == '2'))
                        {
                            AnsiState_.VT100_SemigraphDef[CharNum] = true;
                        }
                        else
                        {
                            AnsiState_.VT100_SemigraphDef[CharNum] = false;
                        }
                        AnsiState_.__AnsiCommand = false;
                    }
                    break;
                case 'n': // LS2
                    AnsiState_.VT100_SemigraphNum = 2;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'o': // LS3
                    AnsiState_.VT100_SemigraphNum = 3;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '#':
                    if (AnsiState_.__AnsiCmd.Count == 2)
                    {
                        switch (AnsiState_.__AnsiCmd[1])
                        {
                            case '3': // DECDHL
                                {
                                    AnsiSetFontSize(AnsiState_.__AnsiY, 2, true);
                                }
                                break;
                            case '4': // DECDHL
                                {
                                    AnsiSetFontSize(AnsiState_.__AnsiY, 3, true);
                                }
                                break;
                            case '5': // DECSWL
                                {
                                    AnsiSetFontSize(AnsiState_.__AnsiY, 0, true);
                                }
                                break;
                            case '6': // DECDWL
                                {
                                    AnsiSetFontSize(AnsiState_.__AnsiY, 1, true);
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
                        AnsiState_.__AnsiCommand = false;
                    }
                    break;
                case ']':
                    if (TextFileLine_i == 0x07)
                    {
                        AnsiState_.__AnsiCommand = false;
                    }
                    break;
                case 'N': // SS2 - not implemented
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'O': // SS3 - not implemented
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '7': // DECSC
                    AnsiState_.__AnsiX_ = AnsiState_.__AnsiX;
                    AnsiState_.__AnsiY_ = AnsiState_.__AnsiY;
                    AnsiState_.__AnsiBack_ = AnsiState_.__AnsiBack;
                    AnsiState_.__AnsiFore_ = AnsiState_.__AnsiFore;
                    AnsiState_.__AnsiFontBold_ = AnsiState_.__AnsiFontBold;
                    AnsiState_.__AnsiFontInverse_ = AnsiState_.__AnsiFontInverse;
                    AnsiState_.__AnsiFontBlink_ = AnsiState_.__AnsiFontBlink;
                    AnsiState_.__AnsiFontInvisible_ = AnsiState_.__AnsiFontInvisible;
                    AnsiState_.VT100_SemigraphDef_[0] = AnsiState_.VT100_SemigraphDef[0];
                    AnsiState_.VT100_SemigraphDef_[1] = AnsiState_.VT100_SemigraphDef[1];
                    AnsiState_.VT100_SemigraphDef_[2] = AnsiState_.VT100_SemigraphDef[2];
                    AnsiState_.VT100_SemigraphDef_[3] = AnsiState_.VT100_SemigraphDef[3];
                    AnsiState_.VT100_SemigraphNum_ = AnsiState_.VT100_SemigraphNum;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '8': // DECRC
                    AnsiState_.__AnsiX = AnsiState_.__AnsiX_;
                    AnsiState_.__AnsiY = AnsiState_.__AnsiY_;
                    AnsiState_.__AnsiBack = AnsiState_.__AnsiBack_;
                    AnsiState_.__AnsiFore = AnsiState_.__AnsiFore_;
                    AnsiState_.__AnsiFontBold = AnsiState_.__AnsiFontBold_;
                    AnsiState_.__AnsiFontInverse = AnsiState_.__AnsiFontInverse_;
                    AnsiState_.__AnsiFontBlink = AnsiState_.__AnsiFontBlink_;
                    AnsiState_.__AnsiFontInvisible = AnsiState_.__AnsiFontInvisible_;
                    AnsiState_.VT100_SemigraphDef[0] = AnsiState_.VT100_SemigraphDef_[0];
                    AnsiState_.VT100_SemigraphDef[1] = AnsiState_.VT100_SemigraphDef_[1];
                    AnsiState_.VT100_SemigraphDef[2] = AnsiState_.VT100_SemigraphDef_[2];
                    AnsiState_.VT100_SemigraphDef[3] = AnsiState_.VT100_SemigraphDef_[3];
                    AnsiState_.VT100_SemigraphNum = AnsiState_.VT100_SemigraphNum_;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'D': // IND
                    if (ANSIDOS)
                    {

                    }
                    else
                    {
                        AnsiState_.__AnsiY += 1;
                        if (AnsiState_.__AnsiY > AnsiState_.__AnsiScrollLast)
                        {
                            AnsiScrollInit(AnsiState_.__AnsiY - AnsiState_.__AnsiScrollLast, AnsiState.AnsiScrollCommandDef.None);
                            AnsiState_.__AnsiY = AnsiState_.__AnsiScrollLast;
                        }
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'M': // RI
                    if (ANSIDOS)
                    {
                        AnsiState_.__AnsiMusic = true;
                    }
                    else
                    {
                        AnsiState_.__AnsiY -= 1;
                        if (AnsiState_.__AnsiY < AnsiState_.__AnsiScrollFirst)
                        {
                            AnsiScrollInit(AnsiState_.__AnsiY - AnsiState_.__AnsiScrollFirst, AnsiState.AnsiScrollCommandDef.None);
                            AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                        }
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'E': // NEL
                    AnsiState_.__AnsiY += 1;
                    AnsiState_.__AnsiX = AnsiProcessGetXMin(true);
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
                case 'P': // DCS
                    AnsiState_.__AnsiDCS = "";
                    AnsiState_.__AnsiDCS_ = true;
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
                case '\\': // ST
                    if (AnsiState_.__AnsiDCS_)
                    {
                        __AnsiResponse.Add(AnsiState_.__AnsiDCS);
                        AnsiState_.__AnsiDCS = "";
                        AnsiState_.__AnsiDCS_ = false;
                    }
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'c': // RIS
                    AnsiTerminalReset();
                    break;
                case '6': // DECBI
                    if (AnsiState_.__AnsiLineOccupy.Count > AnsiState_.__AnsiY)
                    {
                        int FontSize = AnsiGetFontSize(AnsiState_.__AnsiY);
                        int TempC = 0, TempB = 0, TempF = 0;
                        if (FontSize > 0)
                        {
                            AnsiGetF(AnsiState_.__AnsiX, AnsiState_.__AnsiY, out TempC, out TempB, out TempF);
                            if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count >= (AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor))
                            {
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].RemoveRange((AnsiProcessGetXMax(false) - 1) * 2 * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor + __AnsiLineOccupyFactor);
                            }
                            if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count > (AnsiState_.__AnsiX * 2 * __AnsiLineOccupyFactor))
                            {
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, FontSize - 1);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, 2);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, TempF);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, TempB);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, 32);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, FontSize - 1);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, 1);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, TempF);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, TempB);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * 2 * __AnsiLineOccupyFactor, 32);
                            }
                        }
                        else
                        {
                            AnsiGetF(AnsiProcessGetXMin(false), AnsiState_.__AnsiY, out TempC, out TempB, out TempF);
                            if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count >= (AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor))
                            {
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].RemoveRange((AnsiProcessGetXMax(false) - 1) * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor);
                            }
                            if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count > (AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor))
                            {
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor, 0);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor, 0);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor, TempF);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor, TempB);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor, 32);
                            }
                        }
                        AnsiRepaintLine(AnsiState_.__AnsiY);
                    }
                    AnsiState_.PrintCharInsDel++;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case '9': // DECFI
                    if (AnsiState_.__AnsiLineOccupy.Count > AnsiState_.__AnsiY)
                    {
                        int TempC = 0, TempB = 0, TempF = 0;
                        if (AnsiGetFontSize(AnsiState_.__AnsiY) > 0)
                        {
                            AnsiGetF((AnsiProcessGetXMax(false) / 2) - 1, AnsiState_.__AnsiY, out TempC, out TempB, out TempF);
                            if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count >= ((AnsiProcessGetXMax(false) / 2) * __AnsiLineOccupyFactor))
                            {
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(((AnsiProcessGetXMax(false) / 2)) * __AnsiLineOccupyFactor, 0);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(((AnsiProcessGetXMax(false) / 2)) * __AnsiLineOccupyFactor, 0);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(((AnsiProcessGetXMax(false) / 2)) * __AnsiLineOccupyFactor, TempF);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(((AnsiProcessGetXMax(false) / 2)) * __AnsiLineOccupyFactor, TempB);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(((AnsiProcessGetXMax(false) / 2)) * __AnsiLineOccupyFactor, 32);
                            }
                            if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count > (AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor))
                            {
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].RemoveRange((AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor), __AnsiLineOccupyFactor + __AnsiLineOccupyFactor);
                            }
                            if (TempC > 0)
                            {
                                AnsiCalcColor();
                                AnsiCharF((AnsiProcessGetXMax(false) / 2) - 1, AnsiState_.__AnsiY, 32);
                            }
                        }
                        else
                        {
                            AnsiGetF(AnsiProcessGetXMax(false) - 1, AnsiState_.__AnsiY, out TempC, out TempB, out TempF);
                            if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count >= (AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor))
                            {
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempF);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempB);
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 32);
                            }
                            if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count > (AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor))
                            {
                                AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].RemoveRange((AnsiProcessGetXMin(false) * __AnsiLineOccupyFactor), __AnsiLineOccupyFactor);
                            }
                            if (TempC > 0)
                            {
                                AnsiCalcColor();
                                AnsiCharF(AnsiProcessGetXMax(false) - 1, AnsiState_.__AnsiY, 32);
                            }
                        }
                        AnsiRepaintLine(AnsiState_.__AnsiY);
                    }
                    AnsiState_.PrintCharInsDel++;
                    AnsiState_.__AnsiCommand = false;
                    break;
                case 'V':
                    AnsiState_.CharProtection2Print = true;
                    break;
                case 'W':
                    AnsiState_.CharProtection2Print = false;
                    break;
            }
            if (__AnsiTestCmd)
            {
                if (!AnsiState_.__AnsiCommand)
                {
                    Console.WriteLine("ANSI command: " + TextWork.IntToStr(AnsiState_.__AnsiCmd));
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
                                case "1": // DECSET / DECCKM
                                    __AnsiResponse.Add("CursorKey_1");
                                    break;
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
                                case "6": // DECSET / DECOM
                                    AnsiState_.__AnsiOrigin = true;
                                    AnsiState_.__AnsiX = 0;
                                    AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                                    break;
                                case "7": // DECSET / DECCOLM
                                    AnsiState_.__AnsiNoWrap = false;
                                    break;
                                case "69": // DECSET / DECLRMM
                                    AnsiState_.__AnsiMarginLeftRight = true;
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
                                case "1": // DECRST / DECCKM
                                    __AnsiResponse.Add("CursorKey_0");
                                    break;
                                case "2": // DECRST / DECANM
                                    AnsiState_.__AnsiVT52 = true;
                                    AnsiState_.__AnsiCommand = false;
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
                                case "6": // DECRST / DECOM
                                    AnsiState_.__AnsiOrigin = false;
                                    AnsiState_.__AnsiX = 0;
                                    AnsiState_.__AnsiY = 0;
                                    break;
                                case "7": // DECRST / DECAWM
                                    AnsiState_.__AnsiNoWrap = true;
                                    break;
                                case "69": // DECRST / DECLRMM
                                    AnsiState_.__AnsiMarginLeftRight = false;
                                    break;
                            }
                        }
                    }
                    break;
            }
            switch (AnsiCmd_)
            {
                case "[?J": // DECSED
                case "[?0J": // DECSED
                    AnsiCalcColor();
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
                    AnsiCalcColor();
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
                            AnsiCharUnprotected1(ii_, i_, 32);
                        }
                    }
                    if (ANSIDOS)
                    {
                        AnsiState_.__AnsiX = 0;
                        AnsiState_.__AnsiY = 0;
                    }
                    break;
                case "[?K": // DECSEL
                case "[?0K": // DECSEL
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        AnsiCalcColor();
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
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        AnsiCalcColor();
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
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        AnsiCalcColor();
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
                    if (!AnsiState_.__AnsiMarginLeftRight)
                    {
                        AnsiState_.__AnsiX_ = AnsiState_.__AnsiX;
                        AnsiState_.__AnsiY_ = AnsiState_.__AnsiY;
                        AnsiState_.__AnsiBack_ = AnsiState_.__AnsiBack;
                        AnsiState_.__AnsiFore_ = AnsiState_.__AnsiFore;
                        AnsiState_.__AnsiFontBold_ = AnsiState_.__AnsiFontBold;
                        AnsiState_.__AnsiFontInverse_ = AnsiState_.__AnsiFontInverse;
                        AnsiState_.__AnsiFontBlink_ = AnsiState_.__AnsiFontBlink;
                        AnsiState_.__AnsiFontInvisible_ = AnsiState_.__AnsiFontInvisible;
                        AnsiState_.VT100_SemigraphDef_[0] = AnsiState_.VT100_SemigraphDef[0];
                        AnsiState_.VT100_SemigraphDef_[1] = AnsiState_.VT100_SemigraphDef[1];
                        AnsiState_.VT100_SemigraphDef_[2] = AnsiState_.VT100_SemigraphDef[2];
                        AnsiState_.VT100_SemigraphDef_[3] = AnsiState_.VT100_SemigraphDef[3];
                        AnsiState_.VT100_SemigraphNum_ = AnsiState_.VT100_SemigraphNum;
                    }
                    else
                    {
                        AnsiState_.__AnsiMarginLeft = 0;
                        AnsiState_.__AnsiMarginRight = AnsiMaxX;
                    }
                    break;
                case "[u": // SCORC
                    AnsiState_.__AnsiX = AnsiState_.__AnsiX_;
                    AnsiState_.__AnsiY = AnsiState_.__AnsiY_;
                    AnsiState_.__AnsiBack = AnsiState_.__AnsiBack_;
                    AnsiState_.__AnsiFore = AnsiState_.__AnsiFore_;
                    AnsiState_.__AnsiFontBold = AnsiState_.__AnsiFontBold_;
                    AnsiState_.__AnsiFontInverse = AnsiState_.__AnsiFontInverse_;
                    AnsiState_.__AnsiFontBlink = AnsiState_.__AnsiFontBlink_;
                    AnsiState_.__AnsiFontInvisible = AnsiState_.__AnsiFontInvisible_;
                    AnsiState_.VT100_SemigraphDef[0] = AnsiState_.VT100_SemigraphDef_[0];
                    AnsiState_.VT100_SemigraphDef[1] = AnsiState_.VT100_SemigraphDef_[1];
                    AnsiState_.VT100_SemigraphDef[2] = AnsiState_.VT100_SemigraphDef_[2];
                    AnsiState_.VT100_SemigraphDef[3] = AnsiState_.VT100_SemigraphDef_[3];
                    AnsiState_.VT100_SemigraphNum = AnsiState_.VT100_SemigraphNum_;
                    break;
                case "[J": // ED
                case "[0J": // ED
                    AnsiCalcColor();
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
                    AnsiCalcColor();
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
                            AnsiCharUnprotected2(ii_, i_, 32);
                        }
                    }
                    if (ANSIDOS)
                    {
                        AnsiState_.__AnsiX = 0;
                        AnsiState_.__AnsiY = 0;
                    }
                    break;
                case "[K": // EL
                case "[0K": // EL
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        AnsiCalcColor();
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
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        AnsiCalcColor();
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
                    if (AnsiMaxY > AnsiState_.__AnsiY)
                    {
                        AnsiCalcColor();
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
                            AnsiState_.__AnsiFontSizeW = AnsiProcess_Int(AnsiParams[1], AnsiCmd_);
                            AnsiState_.__AnsiFontSizeH = AnsiProcess_Int(AnsiParams[2], AnsiCmd_);
                            if (AnsiState_.__AnsiFontSizeW > FontMaxSizeCode)
                            {
                                AnsiState_.__AnsiFontSizeW = 0;
                            }
                            if (AnsiState_.__AnsiFontSizeH > FontMaxSizeCode)
                            {
                                AnsiState_.__AnsiFontSizeH = 0;
                            }
                            break;
                        case "1":
                            {
                                SetProcessDelay(AnsiProcess_Int(AnsiParams[1], AnsiCmd_));
                            }
                            break;
                    }
                    break;
                case 'h': // SM
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
                case 'H': // CUP
                case 'f': // HVP
                    if (!AnsiState_.StatusBar)
                    {
                        if (AnsiParams.Length == 1)
                        {
                            AnsiParams = new string[] { AnsiParams[0], "1" };
                        }
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        if ("".Equals(AnsiParams[1])) { AnsiParams[1] = "1"; }
                        AnsiState_.__AnsiY = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                        AnsiState_.__AnsiX = AnsiProcess_Int(AnsiParams[1], AnsiCmd_) - 1;
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
                        AnsiState_.__AnsiY -= Math.Max(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), 1);
                        if (AnsiState_.__AnsiY < AnsiState_.__AnsiScrollFirst)
                        {
                            AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                        }
                    }
                    break;
                case 'B': // CUD
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    AnsiState_.__AnsiY += Math.Max(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), 1);
                    if (AnsiState_.__AnsiY > AnsiState_.__AnsiScrollLast)
                    {
                        AnsiState_.__AnsiY = AnsiState_.__AnsiScrollLast;
                    }
                    break;
                case 'C': // CUF
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    AnsiState_.__AnsiX += Math.Max(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), 1);
                    if ((AnsiMaxX > 0) && (AnsiState_.__AnsiX >= AnsiProcessGetXMax(false)))
                    {
                        if (ANSIDOS)
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
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    if (AnsiMaxX > 0)
                    {
                        if (AnsiState_.__AnsiX >= AnsiMaxX)
                        {
                            AnsiState_.__AnsiX = AnsiMaxX - 1;
                        }
                    }
                    AnsiState_.__AnsiX -= Math.Max(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), 1);
                    if (AnsiState_.__AnsiX < AnsiProcessGetXMin(false))
                    {
                        AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                    }
                    break;

                case 'd': // VPA
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    AnsiState_.__AnsiY = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                    if (AnsiState_.__AnsiOrigin)
                    {
                        AnsiState_.__AnsiY += AnsiState_.__AnsiScrollFirst;
                    }
                    break;
                case 'e': // VPR
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    AnsiState_.__AnsiY += AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                    break;

                case '`': // HPA
                    if (!AnsiState_.StatusBar)
                    {
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        AnsiState_.__AnsiX = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1 + AnsiProcessGetXMin(true);
                    }
                    break;
                case 'a': // HPR
                    if (!AnsiState_.StatusBar)
                    {
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        AnsiState_.__AnsiX += AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                    }
                    break;

                case 'E': // CNL
                    AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                    if ("0".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    AnsiState_.__AnsiY += AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                    if (AnsiState_.__AnsiY > AnsiState_.__AnsiScrollLast)
                    {
                        AnsiState_.__AnsiY = AnsiState_.__AnsiScrollLast;
                    }
                    break;
                case 'F': // CPL
                    AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                    if ("0".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    AnsiState_.__AnsiY -= AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                    if (AnsiState_.__AnsiY < AnsiState_.__AnsiScrollFirst)
                    {
                        AnsiState_.__AnsiY = AnsiState_.__AnsiScrollFirst;
                    }
                    break;
                case 'G': // CHA
                    if ("0".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    AnsiState_.__AnsiX = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                    if (AnsiState_.__AnsiOrigin && AnsiState_.__AnsiMarginLeftRight)
                    {
                        AnsiState_.__AnsiX += AnsiState_.__AnsiMarginLeft;
                    }
                    break;
                case 'S': // SU
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    AnsiScrollInit(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), AnsiState.AnsiScrollCommandDef.None);
                    break;
                case 'T': // SD
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    AnsiScrollInit(0 - AnsiProcess_Int(AnsiParams[0], AnsiCmd_), AnsiState.AnsiScrollCommandDef.None);
                    break;
                case 'q':
                    if (AnsiCmd_.EndsWith("\"q"))
                    {
                        if (AnsiParams.Length >= 1)
                        {
                            AnsiParams[AnsiParams.Length - 1] = AnsiParams[AnsiParams.Length - 1].Substring(0, AnsiParams[AnsiParams.Length - 1].Length - 1);
                            if (AnsiProcess_Int(AnsiParams[0], AnsiCmd_) == 1)
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
                            AnsiAttriburesSave();
                            int RecY1 = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                            int RecX1 = AnsiProcess_Int(AnsiParams[1], AnsiCmd_) - 1;
                            int RecY2 = AnsiProcess_Int(AnsiParams[2], AnsiCmd_) - 1;
                            int RecX2 = AnsiProcess_Int(AnsiParams[3], AnsiCmd_) - 1;
                            int Attr = AnsiProcess_Int(AnsiParams[4], AnsiCmd_);
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
                                    int TempC, TempB, TempF;
                                    AnsiGetF(X, Y, out TempC, out TempB, out TempF);
                                    AnsiDetectAttributes(TempB, TempF);
                                    switch (Attr)
                                    {
                                        case 0:
                                            if (AnsiState_.__AnsiFontInverse && (ANSIReverseMode == 1))
                                            {
                                                int T = TempF;
                                                TempF = TempB;
                                                TempB = T;
                                            }
                                            if (AnsiState_.__AnsiFontBold)
                                            {
                                                TempF = AnsiCalcColorSwapLoHi(TempF);
                                            }
                                            if (AnsiState_.__AnsiFontBlink)
                                            {
                                                TempB = AnsiCalcColorSwapLoHi(TempB);
                                            }
                                            if (AnsiState_.__AnsiFontInverse && (ANSIReverseMode == 0))
                                            {
                                                int T = TempF;
                                                TempF = TempB;
                                                TempB = T;
                                            }
                                            AnsiState_.__AnsiFontBold = false;
                                            AnsiState_.__AnsiFontBlink = false;
                                            AnsiState_.__AnsiFontInverse = false;
                                            break;
                                        case 1: // Bold
                                            AnsiState_.__AnsiFontBold = true;
                                            break;
                                        case 4: // Underline
                                            break;
                                        case 5: // Blink
                                            AnsiState_.__AnsiFontBlink = true;
                                            break;
                                        case 7: // Inverse
                                            AnsiState_.__AnsiFontInverse = true;
                                            break;
                                    }
                                    AnsiCalcColor(TempB, TempF);
                                    AnsiCharF(X, Y, TempC);
                                }
                                AnsiRepaintLine(Y);
                            }
                            AnsiAttributesLoad();
                        }
                    }
                    else
                    {

                        if (AnsiParams.Length == 1)
                        {
                            AnsiParams = new string[] { AnsiParams[0], (AnsiMaxY + 1).ToString() };
                        }
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "0"; }
                        if ("".Equals(AnsiParams[1])) { AnsiParams[1] = "0"; }
                        AnsiState_.__AnsiScrollFirst = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                        AnsiState_.__AnsiScrollLast = AnsiProcess_Int(AnsiParams[1], AnsiCmd_) - 1;
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
                case 'v':
                    // DECCRA
                    if (AnsiCmd_.EndsWith("$v")) 
                    {
                        if (AnsiParams.Length >= 8)
                        {
                            AnsiParams[AnsiParams.Length - 1] = AnsiParams[AnsiParams.Length - 1].Substring(0, AnsiParams[AnsiParams.Length - 1].Length - 1);
                            AnsiAttriburesSave();
                            int RecY1 = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                            int RecX1 = AnsiProcess_Int(AnsiParams[1], AnsiCmd_) - 1;
                            int RecY2 = AnsiProcess_Int(AnsiParams[2], AnsiCmd_) - 1;
                            int RecX2 = AnsiProcess_Int(AnsiParams[3], AnsiCmd_) - 1;
                            int TargetY = AnsiProcess_Int(AnsiParams[5], AnsiCmd_) - 1;
                            int TargetX = AnsiProcess_Int(AnsiParams[6], AnsiCmd_) - 1;
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
                                            int TempC, TempB, TempF;
                                            AnsiGetF(X, Y, out TempC, out TempB, out TempF);
                                            AnsiCalcColor(TempB, TempF);
                                            AnsiCharF(X + TargetX, Y + TargetY, TempC);
                                        }
                                    }
                                    else
                                    {
                                        for (int X = RecX1; X <= RecX2; X++)
                                        {
                                            int TempC, TempB, TempF;
                                            AnsiGetF(X, Y, out TempC, out TempB, out TempF);
                                            AnsiCalcColor(TempB, TempF);
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
                                            int TempC, TempB, TempF;
                                            AnsiGetF(X, Y, out TempC, out TempB, out TempF);
                                            AnsiCalcColor(TempB, TempF);
                                            AnsiCharF(X + TargetX, Y + TargetY, TempC);
                                        }
                                    }
                                    else
                                    {
                                        for (int X = RecX1; X <= RecX2; X++)
                                        {
                                            int TempC, TempB, TempF;
                                            AnsiGetF(X, Y, out TempC, out TempB, out TempF);
                                            AnsiCalcColor(TempB, TempF);
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
                            AnsiAttriburesSave();
                            int FillC = AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                            int RecY1 = AnsiProcess_Int(AnsiParams[1], AnsiCmd_) - 1;
                            int RecX1 = AnsiProcess_Int(AnsiParams[2], AnsiCmd_) - 1;
                            int RecY2 = AnsiProcess_Int(AnsiParams[3], AnsiCmd_) - 1;
                            int RecX2 = AnsiProcess_Int(AnsiParams[4], AnsiCmd_) - 1;
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

                            if (AnsiState_.VT100_SemigraphDef[AnsiState_.VT100_SemigraphNum])
                            {
                                if ((FillC >= 95) && (FillC <= 126))
                                {
                                    FillC = VT100_SemigraphChars[FillC - 95];
                                }
                            }

                            for (int Y = RecY1; Y <= RecY2; Y++)
                            {
                                for (int X = RecX1; X <= RecX2; X++)
                                {
                                    AnsiCalcColor();
                                    AnsiCharF(X, Y, FillC);
                                }
                                AnsiRepaintLine(Y);
                            }
                            AnsiAttributesLoad();
                        }
                    }
                    break;
                case 'z':
                    // DECERA
                    if (AnsiCmd_.EndsWith("$z"))
                    {
                        if (AnsiParams.Length >= 4)
                        {
                            AnsiParams[AnsiParams.Length - 1] = AnsiParams[AnsiParams.Length - 1].Substring(0, AnsiParams[AnsiParams.Length - 1].Length - 1);
                            AnsiAttriburesSave();
                            int RecY1 = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                            int RecX1 = AnsiProcess_Int(AnsiParams[1], AnsiCmd_) - 1;
                            int RecY2 = AnsiProcess_Int(AnsiParams[2], AnsiCmd_) - 1;
                            int RecX2 = AnsiProcess_Int(AnsiParams[3], AnsiCmd_) - 1;
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
                                    int TempC, TempB, TempF;
                                    AnsiGetF(X, Y, out TempC, out TempB, out TempF);
                                    AnsiState_.__AnsiBack = TempB;
                                    AnsiState_.__AnsiFore = TempF;
                                    AnsiCalcColor();
                                    AnsiCharF(X, Y, ' ');
                                }
                                AnsiRepaintLine(Y);
                            }
                            AnsiAttributesLoad();
                        }
                    }
                    break;
                case 't':
                    // DECRARA - not implemented
                    if (AnsiCmd_.EndsWith("$t"))
                    {
                        if (AnsiParams.Length >= 5)
                        {
                            AnsiParams[AnsiParams.Length - 1] = AnsiParams[AnsiParams.Length - 1].Substring(0, AnsiParams[AnsiParams.Length - 1].Length - 1);
                            AnsiAttriburesSave();
                            int RecY1 = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                            int RecX1 = AnsiProcess_Int(AnsiParams[1], AnsiCmd_) - 1;
                            int RecY2 = AnsiProcess_Int(AnsiParams[2], AnsiCmd_) - 1;
                            int RecX2 = AnsiProcess_Int(AnsiParams[3], AnsiCmd_) - 1;
                            int Attr = AnsiProcess_Int(AnsiParams[4], AnsiCmd_);
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
                                    int TempC, TempB, TempF;
                                    AnsiGetF(X, Y, out TempC, out TempB, out TempF);
                                    AnsiDetectAttributes(TempB, TempF);
                                    switch (Attr)
                                    {
                                        case 1: // Bold
                                            if (AnsiState_.__AnsiFontBold)
                                            {
                                                TempF = AnsiCalcColorSwapLoHi(TempF);
                                            }
                                            AnsiState_.__AnsiFontBold = !AnsiState_.__AnsiFontBold;
                                            break;
                                        case 4: // Underline
                                            break;
                                        case 5: // Blink
                                            if (AnsiState_.__AnsiFontBlink)
                                            {
                                                TempB = AnsiCalcColorSwapLoHi(TempB);
                                            }
                                            AnsiState_.__AnsiFontBlink = !AnsiState_.__AnsiFontBlink;
                                            break;
                                        case 7: // Inverse
                                            if (AnsiState_.__AnsiFontInverse)
                                            {
                                                if (ANSIReverseMode == 1)
                                                {
                                                    if (AnsiState_.__AnsiFontBold)
                                                    {
                                                        TempF = AnsiCalcColorSwapLoHi(TempF);
                                                    }
                                                    if (AnsiState_.__AnsiFontBlink)
                                                    {
                                                        TempB = AnsiCalcColorSwapLoHi(TempB);
                                                    }
                                                }
                                                int T = TempF;
                                                TempF = TempB;
                                                TempB = T;
                                                if (ANSIReverseMode == 1)
                                                {
                                                    if (AnsiState_.__AnsiFontBold)
                                                    {
                                                        TempF = AnsiCalcColorSwapLoHi(TempF);
                                                    }
                                                    if (AnsiState_.__AnsiFontBlink)
                                                    {
                                                        TempB = AnsiCalcColorSwapLoHi(TempB);
                                                    }
                                                }
                                            }
                                            AnsiState_.__AnsiFontInverse = !AnsiState_.__AnsiFontInverse;
                                            break;
                                    }
                                    AnsiCalcColor(TempB, TempF);
                                    AnsiCharF(X, Y, TempC);
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
                            AnsiAttriburesSave();
                            int RecY1 = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                            int RecX1 = AnsiProcess_Int(AnsiParams[1], AnsiCmd_) - 1;
                            int RecY2 = AnsiProcess_Int(AnsiParams[2], AnsiCmd_) - 1;
                            int RecX2 = AnsiProcess_Int(AnsiParams[3], AnsiCmd_) - 1;
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
                                    int TempC, TempB, TempF;
                                    AnsiGetF(X, Y, out TempC, out TempB, out TempF);
                                    AnsiState_.__AnsiBack = TempB;
                                    AnsiState_.__AnsiFore = TempF;
                                    AnsiCalcColor();
                                    AnsiCharFUnprotected1(X, Y, ' ');
                                }
                                AnsiRepaintLine(Y);
                            }
                            AnsiAttributesLoad();
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
                    if (AnsiState_.__AnsiMarginLeftRight)
                    {
                        AnsiState_.__AnsiMarginLeft = AnsiProcess_Int(AnsiParams[0], AnsiCmd_) - 1;
                        AnsiState_.__AnsiMarginRight = AnsiProcess_Int(AnsiParams[1], AnsiCmd_);
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
                case 'L': // IL
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    if (ANSIDOS)
                    {

                    }
                    else
                    {
                        if ((AnsiState_.__AnsiY >= AnsiState_.__AnsiScrollFirst) && (AnsiState_.__AnsiY <= AnsiState_.__AnsiScrollLast))
                        {
                            int T1 = AnsiState_.__AnsiScrollFirst;
                            int T2 = AnsiState_.__AnsiScrollLast;
                            //__AnsiX = 0;
                            AnsiState_.__AnsiScrollFirst = AnsiState_.__AnsiY;
                            AnsiScrollInit(0 - AnsiProcess_Int(AnsiParams[0], AnsiCmd_), AnsiState.AnsiScrollCommandDef.FirstLast, T1, T2, 0);
                        }
                    }
                    break;
                case 'M': // DL
                    if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                    if (ANSIDOS)
                    {
                        AnsiState_.__AnsiMusic = true;
                    }
                    else
                    {
                        if ((AnsiState_.__AnsiY >= AnsiState_.__AnsiScrollFirst) && (AnsiState_.__AnsiY <= AnsiState_.__AnsiScrollLast))
                        {
                            int T1 = AnsiState_.__AnsiScrollFirst;
                            int T2 = AnsiState_.__AnsiScrollLast;
                            //__AnsiX = 0;
                            AnsiState_.__AnsiScrollFirst = AnsiState_.__AnsiY;
                            AnsiScrollInit(AnsiProcess_Int(AnsiParams[0], AnsiCmd_), AnsiState.AnsiScrollCommandDef.FirstLast, T1, T2, 0);
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
                        if (AnsiState_.__AnsiLineOccupy.Count > AnsiState_.__AnsiY)
                        {
                            int FontSize = AnsiGetFontSize(AnsiState_.__AnsiY);
                            while (InsCycle > 0)
                            {
                                int TempC = 0, TempB = 0, TempF = 0;
                                if (FontSize > 0)
                                {
                                    AnsiGetF(AnsiState_.__AnsiX, AnsiState_.__AnsiY, out TempC, out TempB, out TempF);
                                    if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count > (AnsiState_.__AnsiX * 2 * __AnsiLineOccupyFactor))
                                    {
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * 2 * __AnsiLineOccupyFactor, FontSize - 1);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * 2 * __AnsiLineOccupyFactor, 2);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * 2 * __AnsiLineOccupyFactor, TempF);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * 2 * __AnsiLineOccupyFactor, TempB);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * 2 * __AnsiLineOccupyFactor, 32);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * 2 * __AnsiLineOccupyFactor, FontSize - 1);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * 2 * __AnsiLineOccupyFactor, 1);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * 2 * __AnsiLineOccupyFactor, TempF);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * 2 * __AnsiLineOccupyFactor, TempB);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * 2 * __AnsiLineOccupyFactor, 32);
                                    }
                                    if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count > (AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor))
                                    {
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].RemoveRange(AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor + __AnsiLineOccupyFactor);
                                    }
                                }
                                else
                                {
                                    AnsiGetF(AnsiState_.__AnsiX, AnsiState_.__AnsiY, out TempC, out TempB, out TempF);
                                    if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count > (AnsiState_.__AnsiX * __AnsiLineOccupyFactor))
                                    {
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * __AnsiLineOccupyFactor, 0);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * __AnsiLineOccupyFactor, 0);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * __AnsiLineOccupyFactor, TempF);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * __AnsiLineOccupyFactor, TempB);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert(AnsiState_.__AnsiX * __AnsiLineOccupyFactor, 32);
                                    }
                                    if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count > (AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor))
                                    {
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].RemoveRange(AnsiProcessGetXMax(false) * __AnsiLineOccupyFactor, __AnsiLineOccupyFactor);
                                    }
                                }
                                InsCycle--;
                            }
                            AnsiRepaintLine(AnsiState_.__AnsiY);
                        }
                        AnsiState_.PrintCharInsDel++;
                    }
                    break;
                case 'P': // DCH
                    {
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        int DelCycle = AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                        if (AnsiState_.__AnsiLineOccupy.Count > AnsiState_.__AnsiY)
                        {
                            while (DelCycle > 0)
                            {
                                int TempC = 0, TempB = 0, TempF = 0;
                                if (AnsiGetFontSize(AnsiState_.__AnsiY) > 0)
                                {
                                    AnsiGetF((AnsiProcessGetXMax(false)) - 1, AnsiState_.__AnsiY, out TempC, out TempB, out TempF);
                                    if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count >= ((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor))
                                    {
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempF);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempB);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 32);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempF);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempB);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 32);
                                    }
                                    if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count > (AnsiState_.__AnsiX * __AnsiLineOccupyFactor))
                                    {
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].RemoveRange((AnsiState_.__AnsiX * __AnsiLineOccupyFactor), __AnsiLineOccupyFactor + __AnsiLineOccupyFactor);
                                    }
                                    if (TempC > 0)
                                    {
                                        AnsiCalcColor();
                                        AnsiCharF((AnsiProcessGetXMax(false) / 2) - 1, AnsiState_.__AnsiY, 32);
                                    }
                                }
                                else
                                {
                                    AnsiGetF(AnsiProcessGetXMax(false) - 1, AnsiState_.__AnsiY, out TempC, out TempB, out TempF);
                                    if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count >= ((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor))
                                    {
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 0);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempF);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, TempB);
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Insert((AnsiProcessGetXMax(false)) * __AnsiLineOccupyFactor, 32);
                                    }
                                    if (AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].Count > (AnsiState_.__AnsiX * __AnsiLineOccupyFactor))
                                    {
                                        AnsiState_.__AnsiLineOccupy[AnsiState_.__AnsiY].RemoveRange((AnsiState_.__AnsiX * __AnsiLineOccupyFactor), __AnsiLineOccupyFactor);
                                    }
                                    if (TempC > 0)
                                    {
                                        AnsiCalcColor();
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
                case 'X': // ECH
                    {
                        if ("".Equals(AnsiParams[0])) { AnsiParams[0] = "1"; }
                        int DelCycle = AnsiProcess_Int(AnsiParams[0], AnsiCmd_);
                        AnsiCalcColor();
                        while (DelCycle > 0)
                        {
                            AnsiCharFUnprotected2(AnsiState_.__AnsiX + DelCycle - 1, AnsiState_.__AnsiY, 32);
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
                        AnsiState_.__AnsiFontBold = false;
                        AnsiState_.__AnsiFontInverse = false;
                        AnsiState_.__AnsiFontBlink = false;
                        AnsiState_.__AnsiFontInvisible = false;
                        break;

                    case "39": AnsiState_.__AnsiFore = -1; break;
                    case "49": AnsiState_.__AnsiBack = -1; break;

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

                    case "1": AnsiState_.__AnsiFontBold = true; break;
                    case "22": AnsiState_.__AnsiFontBold = false; break;
                    case "5": AnsiState_.__AnsiFontBlink = true; break;
                    case "25": AnsiState_.__AnsiFontBlink = false; break;
                    case "7": AnsiState_.__AnsiFontInverse = true; break;
                    case "27": AnsiState_.__AnsiFontInverse = false; break;
                    case "8": AnsiState_.__AnsiFontInvisible = true; break;
                    case "28": AnsiState_.__AnsiFontInvisible = false; break;


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
                        //Console.BackgroundColor = ConsoleColor.Black;
                        //Console.ForegroundColor = ConsoleColor.White;
                        //Console.WriteLine("{" + AnsiParams[i_] + "}");
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
                Screen_.SetCursorPosition(AnsiState_.__AnsiX << 1, AnsiState_.__AnsiY);
            }
            else
            {
                Screen_.SetCursorPosition(AnsiState_.__AnsiX, AnsiState_.__AnsiY);
            }
        }

        private int AnsiCharPrintLast = -1;
        private int AnsiCharPrintRepeater = 0;

        private void AnsiCharPrint(int TextFileLine_i)
        {
            if (AnsiState_.StatusBar)
            {
                return;
            }

                if (TextFileLine_i == 127)
                {
                    if (!ANSIDOS)
                    {
                        return;
                    }
                }

                if ((!AnsiState_.__AnsiMusic) && (TextFileLine_i < 32) && (ANSIDOS))
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
                AnsiCalcColor();
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
                    if (AnsiState_.VT100_SemigraphDef[AnsiState_.VT100_SemigraphNum])
                    {
                        if ((TextFileLine_i >= 95) && (TextFileLine_i <= 126))
                        {
                            TextFileLine_i = VT100_SemigraphChars[TextFileLine_i - 95];
                        }
                    }
                }

                if (!AnsiState_.__AnsiMusic)
                {
                    if (ANSIDOS)
                    {
                        AnsiCharPrintLast = TextFileLine_i;
                        AnsiCharFI(AnsiState_.__AnsiX, AnsiState_.__AnsiY, TextFileLine_i, AnsiState_.__AnsiBackWork, AnsiState_.__AnsiForeWork);
                        AnsiState_.__AnsiX++;
                        if ((AnsiMaxX > 0) && (AnsiState_.__AnsiX == AnsiProcessGetXMax(true)))
                        {
                            if (AnsiState_.__AnsiNoWrap)
                            {
                                AnsiState_.__AnsiX--;
                            }
                            else
                            {
                                AnsiState_.__AnsiX = AnsiProcessGetXMin(true);
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
                        if ((AnsiMaxX > 0) && (AnsiState_.__AnsiX == AnsiProcessGetXMax(true)))
                        {
                            if (AnsiState_.__AnsiNoWrap)
                            {
                                AnsiState_.__AnsiX--;
                            }
                            else
                            {
                                AnsiState_.__AnsiX = AnsiProcessGetXMin(true);
                                AnsiState_.__AnsiY++;

                                if ((AnsiMaxY > 0) && (AnsiState_.__AnsiY > AnsiState_.__AnsiScrollLast))
                                {
                                    int L = AnsiState_.__AnsiY - AnsiState_.__AnsiScrollLast;
                                    AnsiState_.__AnsiY = AnsiState_.__AnsiScrollLast;
                                    AnsiScrollInit(L, AnsiState.AnsiScrollCommandDef.Char, TextFileLine_i, AnsiState_.__AnsiBackWork, AnsiState_.__AnsiForeWork);
                                    CharNoScroll = false;
                                }
                            }
                        }
                        if (CharNoScroll)
                        {
                            AnsiCharPrintLast = TextFileLine_i;
                            AnsiCharFI(AnsiState_.__AnsiX, AnsiState_.__AnsiY, TextFileLine_i, AnsiState_.__AnsiBackWork, AnsiState_.__AnsiForeWork);
                            AnsiState_.__AnsiX++;
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
                            if (!ANSIDOS)
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
                                    Screen_.Bell();
                                }
                            }
                            break;
                        case 8:
                            {
                                if (!ANSIPrintBackspace)
                                {
                                    if (AnsiState_.__AnsiX == AnsiMaxX)
                                    {
                                        AnsiState_.__AnsiX--;
                                    }
                                    if (AnsiState_.__AnsiX > AnsiProcessGetXMin(true))
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
                                        AnsiState_.__AnsiX = AnsiProcessGetXMin(false);
                                        break;
                                    case 1:
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
                            if (!ANSIDOS)
                            {
                                switch (ANSI_LF)
                                {
                                    case 0:
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
                                        AnsiState_.__AnsiX = AnsiProcessGetXMin(true);
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
                                        AnsiState_.__AnsiX = AnsiProcessGetXMin(true);
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
                            if (!ANSIDOS)
                            {
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
                        case 14:
                            if (!ANSIDOS)
                            {
                                AnsiState_.VT100_SemigraphNum = 1;
                            }
                            break;
                        case 15:
                            if (!ANSIDOS)
                            {
                                AnsiState_.VT100_SemigraphNum = 0;
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
