using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TextPaint
{
    public partial class Telnet
    {
        void EscapeKeyMessage()
        {
            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
            Screen_.SetCursorPositionNoRefresh(0, 0);
            Screen_.WriteText("Press key, which will be used as escape key...", Core_.TextNormalBack, Core_.TextNormalFore);
            Screen_.Refresh();
        }

        string EscapeKeyId(string KeyName, char KeyChar)
        {
            return KeyName + "_" + TextWork.CharCode(KeyChar, 0);
        }

        public void CoreEvent_Client(string KeyName, char KeyChar, int KeyCharI, bool ModShift, bool ModCtrl, bool ModAlt)
        {
            if ("Resize".Equals(KeyName))
            {
                if (WorkStateC == WorkStateCDef.InfoScreen)
                {
                    EscapeKeyMessage();
                }
                if ((WorkStateC == WorkStateCDef.Toolbox) || (WorkStateC == WorkStateCDef.EscapeKey))
                {
                    TelnetDisplayInfo(true);
                }
                return;
            }

            if ("WindowClose".Equals(KeyName))
            {
                TelnetClose(true);
            }
            else
            {
                Monitor.Enter(TelnetMutex);
                if (Core_.AnsiTerminalResize(Core_.Screen_.WinW, Core_.Screen_.WinH))
                {
                    Monitor.Exit(TelnetMutex);
                    if (WorkStateC == WorkStateCDef.InfoScreen)
                    {
                        EscapeKeyMessage();
                    }
                    if ((WorkStateC == WorkStateCDef.Session) || (WorkStateC == WorkStateCDef.Toolbox))
                    {
                        if (Conn.IsConnected() == 1)
                        {
                            //TelnetSendWindowSize();
                        }
                    }
                }
                else
                {
                    Monitor.Exit(TelnetMutex);
                }

                switch (WorkStateC)
                {
                    case WorkStateCDef.InfoScreen:
                        {
                            if (!("".Equals(KeyName)))
                            {
                                EscapeKey = EscapeKeyId(KeyName, KeyChar);
                            }
                        }
                        break;
                    case WorkStateCDef.Session:
                        {
                            if (EscapeKeyId(KeyName, KeyChar).Equals(EscapeKey))
                            {
                                WorkStateC = WorkStateCDef.Toolbox;
                            }
                            else
                            {
                                if (Core_.__AnsiTestCmd)
                                {
                                    Console.WriteLine("####################");
                                }
                                string CAS = "";
                                if (ModCtrl || (TelnetKeyboardMods[0] != '0')) { CAS = CAS + "1"; } else { CAS = CAS + "0"; }
                                if (ModAlt || (TelnetKeyboardMods[1] != '0')) { CAS = CAS + "1"; } else { CAS = CAS + "0"; }
                                if (ModShift || (TelnetKeyboardMods[2] != '0')) { CAS = CAS + "1"; } else { CAS = CAS + "0"; }
                                bool FuncShift = (TelnetFuncKeyOther == 1);
                                switch (KeyName)
                                {
                                    case "UpArrow":
                                        Send(TerminalKeys["Up_" + CAS + "_" + GetTelnetKeyboardConf(0)]);
                                        break;
                                    case "DownArrow":
                                        Send(TerminalKeys["Down_" + CAS + "_" + GetTelnetKeyboardConf(0)]);
                                        break;
                                    case "LeftArrow":
                                        Send(TerminalKeys["Left_" + CAS + "_" + GetTelnetKeyboardConf(0)]);
                                        break;
                                    case "RightArrow":
                                        Send(TerminalKeys["Right_" + CAS + "_" + GetTelnetKeyboardConf(0)]);
                                        break;
                                    case "Insert":
                                        Send(TerminalKeys["Insert_" + CAS + "_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "Delete":
                                        Send(TerminalKeys["Delete_" + CAS + "_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "Home":
                                        Send(TerminalKeys["Home_" + CAS + "_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "End":
                                        Send(TerminalKeys["End_" + CAS + "_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "PageUp":
                                        Send(TerminalKeys["PageUp_" + CAS + "_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "PageDown":
                                        Send(TerminalKeys["PageDown_" + CAS + "_" + GetTelnetKeyboardConf(3)]);
                                        break;

                                    case "Escape": Send(TerminalKeys["Escape"]); break;

                                    case "Enter":
                                        Send(TerminalKeys["Enter_" + GetTelnetKeyboardConf(4)]);
                                        if (LocalEcho)
                                        {
                                            Monitor.Enter(LocalEchoBuf);
                                            LocalEchoBuf.Add(13);
                                            LocalEchoBuf.Add(10);
                                            Monitor.Exit(LocalEchoBuf);
                                        }
                                        break;


                                    case "F1":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F11_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F1_" + CAS + "_" + GetTelnetKeyboardConf(1)]); }
                                        }
                                        else
                                        {
                                            Send("00");
                                        }
                                        break;
                                    case "F2":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F12_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F2_" + CAS + "_" + GetTelnetKeyboardConf(1)]); }
                                        }
                                        else
                                        {
                                            Send("1B");
                                        }
                                        break;
                                    case "F3":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F13_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F3_" + CAS + "_" + GetTelnetKeyboardConf(1)]); }
                                        }
                                        else
                                        {
                                            Send("1C");
                                        }
                                        break;
                                    case "F4":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F14_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F4_" + CAS + "_" + GetTelnetKeyboardConf(1)]); }
                                        }
                                        else
                                        {
                                            Send("1D");
                                        }
                                        break;
                                    case "F5":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F15_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F5_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        else
                                        {
                                            Send("1E");
                                        }
                                        break;
                                    case "F6":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F16_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F6_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        else
                                        {
                                            Send("1F");
                                        }
                                        break;
                                    case "F7":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F17_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F7_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        break;
                                    case "F8":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F18_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F8_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        break;
                                    case "F9":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F19_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F9_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        break;
                                    case "F10":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F20_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F10_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        break;
                                    case "F11":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { } else { Send(TerminalKeys["F11_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        else
                                        {
                                            Send(TerminalAnswerBack);
                                        }
                                        break;
                                    case "F12":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { } else { Send(TerminalKeys["F12_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        else
                                        {
                                            Send(TerminalAnswerBack);
                                        }
                                        break;

                                    case "F13": Send(TerminalKeys["F11_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F14": Send(TerminalKeys["F12_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F15": Send(TerminalKeys["F13_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F16": Send(TerminalKeys["F14_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F17": Send(TerminalKeys["F15_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F18": Send(TerminalKeys["F16_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F19": Send(TerminalKeys["F17_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F20": Send(TerminalKeys["F18_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F21": Send(TerminalKeys["F19_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F22": Send(TerminalKeys["F20_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F23": break;
                                    case "F24": break;


                                    case "Backspace":
                                        Send(TerminalKeys["Backspace_" + GetTelnetKeyboardConf(5)]);
                                        if (LocalEcho)
                                        {
                                            Monitor.Enter(LocalEchoBuf);
                                            LocalEchoBuf.Add(8);
                                            Monitor.Exit(LocalEchoBuf);
                                        }
                                        break;

                                    default:
                                        {
                                            if (ModCtrl)
                                            {
                                                switch (KeyCharI)
                                                {
                                                    case '2':
                                                    case ' ':
                                                    case '@':
                                                        Send("00");
                                                        break;
                                                    case 'a':
                                                    case 'A':
                                                        Send("01");
                                                        break;
                                                    case 'b':
                                                    case 'B':
                                                        Send("02");
                                                        break;
                                                    case 'c':
                                                    case 'C':
                                                        Send("03");
                                                        break;
                                                    case 'd':
                                                    case 'D':
                                                        Send("04");
                                                        break;
                                                    case 'e':
                                                    case 'E':
                                                        Send("05");
                                                        break;
                                                    case 'f':
                                                    case 'F':
                                                        Send("06");
                                                        break;
                                                    case 'g':
                                                    case 'G':
                                                        Send("07");
                                                        break;
                                                    case 'h':
                                                    case 'H':
                                                        Send("08");
                                                        break;
                                                    case 'i':
                                                    case 'I':
                                                        Send("09");
                                                        break;
                                                    case 'j':
                                                    case 'J':
                                                        Send("0A");
                                                        break;
                                                    case 'k':
                                                    case 'K':
                                                        Send("0B");
                                                        break;
                                                    case 'l':
                                                    case 'L':
                                                        Send("0C");
                                                        break;
                                                    case 'm':
                                                    case 'M':
                                                        Send("0D");
                                                        break;
                                                    case 'n':
                                                    case 'N':
                                                        Send("0E");
                                                        break;
                                                    case 'o':
                                                    case 'O':
                                                        Send("0F");
                                                        break;
                                                    case 'p':
                                                    case 'P':
                                                        Send("10");
                                                        break;
                                                    case 'q':
                                                    case 'Q':
                                                        Send("11");
                                                        break;
                                                    case 'r':
                                                    case 'R':
                                                        Send("12");
                                                        break;
                                                    case 's':
                                                    case 'S':
                                                        Send("13");
                                                        break;
                                                    case 't':
                                                    case 'T':
                                                        Send("14");
                                                        break;
                                                    case 'u':
                                                    case 'U':
                                                        Send("15");
                                                        break;
                                                    case 'v':
                                                    case 'V':
                                                        Send("16");
                                                        break;
                                                    case 'w':
                                                    case 'W':
                                                        Send("17");
                                                        break;
                                                    case 'x':
                                                    case 'X':
                                                        Send("18");
                                                        break;
                                                    case 'y':
                                                    case 'Y':
                                                        Send("19");
                                                        break;
                                                    case 'z':
                                                    case 'Z':
                                                        Send("1A");
                                                        break;
                                                    case '3':
                                                    case '[':
                                                    case '{':
                                                        Send("1B");
                                                        break;
                                                    case '4':
                                                    case '\\':
                                                    case '|':
                                                        Send("1C");
                                                        break;
                                                    case '5':
                                                    case ']':
                                                    case '}':
                                                        Send("1D");
                                                        break;
                                                    case '6':
                                                    case '~':
                                                    case '^':
                                                        Send("1E");
                                                        break;
                                                    case '7':
                                                    case '/':
                                                    case '_':
                                                        Send("1F");
                                                        break;
                                                    case '8':
                                                    case '?':
                                                        Send("7F");
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                bool StdKey = true;
                                                if (GetTelnetKeyboardConf(6) == "1")
                                                {
                                                    switch (KeyCharI)
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
                                                        case '.':
                                                        case ',':
                                                        case '-':
                                                        case '+':
                                                            StdKey = false;
                                                            if (Core_.AnsiState_.__AnsiVT52)
                                                            {
                                                                Send(TerminalKeys["NumPad_" + KeyCharI + "_1"]);
                                                            }
                                                            else
                                                            {
                                                                Send(TerminalKeys["NumPad_" + KeyCharI + "_0"]);
                                                            }
                                                            break;
                                                    }
                                                }
                                                if (StdKey)
                                                {
                                                    if ((KeyCharI >= 32) || ((KeyCharI >= 1) && (KeyCharI <= 26) && (KeyCharI != 13)))
                                                    {
                                                        List<int> KeyCharI_ = new List<int>();
                                                        KeyCharI_.Add(KeyCharI);
                                                        if (LocalEcho)
                                                        {
                                                            Monitor.Enter(LocalEchoBuf);
                                                            byte[] KeyBytes = StrToBytes(KeyCharI_);
                                                            for (int i = 0; i < KeyBytes.Length; i++)
                                                            {
                                                                LocalEchoBuf.Add(KeyBytes[i]);
                                                            }
                                                            Monitor.Exit(LocalEchoBuf);
                                                        }
                                                        Send(KeyCharI_);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case WorkStateCDef.Toolbox:
                        {
                            switch (KeyName)
                            {
                                case "Backspace":
                                    TelnetClose(true);
                                    break;
                                default:
                                    string FuncKeyItem = KeyName + "|" + KeyChar.ToString();
                                    Monitor.Enter(FuncKeyBuf);
                                    FuncKeyBuf.Enqueue(FuncKeyItem);
                                    Monitor.Exit(FuncKeyBuf);
                                    break;
                            }
                        }
                        break;
                    case WorkStateCDef.EscapeKey:
                        EscapeKey = EscapeKeyId(KeyName, KeyChar);
                        WorkStateC = WorkStateCDef.Toolbox;
                        break;
                    case WorkStateCDef.DispConf:
                        {
                            DisplayConfig_.ProcessKey(KeyName, KeyChar);
                        }
                        break;
                }
            }
        }


        Queue<string> FuncKeyBuf = new Queue<string>();

        void FuncKeyProcess()
        {
            Monitor.Enter(FuncKeyBuf);
            while (FuncKeyBuf.Count > 0)
            {
                string FuncKeyItem = FuncKeyBuf.Dequeue();
                Monitor.Exit(FuncKeyBuf);

                int P = FuncKeyItem.IndexOf('|');
                string KeyName = FuncKeyItem.Substring(0, P);
                char KeyChar = FuncKeyItem[P + 1];

                string CAS = "";
                if ((TelnetKeyboardMods[0] != '0')) { CAS = CAS + "1"; } else { CAS = CAS + "0"; }
                if ((TelnetKeyboardMods[1] != '0')) { CAS = CAS + "1"; } else { CAS = CAS + "0"; }
                if ((TelnetKeyboardMods[2] != '0')) { CAS = CAS + "1"; } else { CAS = CAS + "0"; }

                switch (KeyName)
                {
                    case "Tab":
                        TelnetInfoPos++;
                        if (TelnetInfoPos == 4)
                        {
                            TelnetInfoPos = 0;
                        }
                        break;
                    case "Enter":
                        {
                            WorkStateC = WorkStateCDef.EscapeKey;
                        }
                        break;
                    case "Escape":
                        {
                            WorkStateC = WorkStateCDef.Session;
                        }
                        break;

                    case "A": WorkStateC = WorkStateCDef.Session; Send("01"); break;
                    case "B": WorkStateC = WorkStateCDef.Session; Send("02"); break;
                    case "C": WorkStateC = WorkStateCDef.Session; Send("03"); break;
                    case "D": WorkStateC = WorkStateCDef.Session; Send("04"); break;
                    case "E": WorkStateC = WorkStateCDef.Session; Send("05"); break;
                    case "F": WorkStateC = WorkStateCDef.Session; Send("06"); break;
                    case "G": WorkStateC = WorkStateCDef.Session; Send("07"); break;
                    case "H": WorkStateC = WorkStateCDef.Session; Send("08"); break;
                    case "I": WorkStateC = WorkStateCDef.Session; Send("09"); break;
                    case "J": WorkStateC = WorkStateCDef.Session; Send("0A"); break;
                    case "K": WorkStateC = WorkStateCDef.Session; Send("0B"); break;
                    case "L": WorkStateC = WorkStateCDef.Session; Send("0C"); break;
                    case "M": WorkStateC = WorkStateCDef.Session; Send("0D"); break;
                    case "N": WorkStateC = WorkStateCDef.Session; Send("0E"); break;
                    case "O": WorkStateC = WorkStateCDef.Session; Send("0F"); break;
                    case "P": WorkStateC = WorkStateCDef.Session; Send("10"); break;
                    case "Q": WorkStateC = WorkStateCDef.Session; Send("11"); break;
                    case "R": WorkStateC = WorkStateCDef.Session; Send("12"); break;
                    case "S": WorkStateC = WorkStateCDef.Session; Send("13"); break;
                    case "T": WorkStateC = WorkStateCDef.Session; Send("14"); break;
                    case "U": WorkStateC = WorkStateCDef.Session; Send("15"); break;
                    case "V": WorkStateC = WorkStateCDef.Session; Send("16"); break;
                    case "W": WorkStateC = WorkStateCDef.Session; Send("17"); break;
                    case "X": WorkStateC = WorkStateCDef.Session; Send("18"); break;
                    case "Y": WorkStateC = WorkStateCDef.Session; Send("19"); break;
                    case "Z": WorkStateC = WorkStateCDef.Session; Send("1A"); break;

                    default:
                        {
                            switch (KeyChar)
                            {
                                case '=':
                                case ';':
                                case ':':
                                    TelnetFuncKeyOther++;
                                    if (TelnetFuncKeyOther == 3)
                                    {
                                        UseCtrlKeys = !UseCtrlKeys;
                                        TelnetFuncKeyOther = 0;
                                    }
                                    break;
                                case '1':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F1_" + CAS + "_" + GetTelnetKeyboardConf(1)]); break;
                                            case 1: Send(TerminalKeys["F11_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                        }
                                    }
                                    break;
                                case '2':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F2_" + CAS + "_" + GetTelnetKeyboardConf(1)]); break;
                                            case 1: Send(TerminalKeys["F12_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("00"); break;
                                        }
                                    }
                                    break;
                                case '3':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F3_" + CAS + "_" + GetTelnetKeyboardConf(1)]); break;
                                            case 1: Send(TerminalKeys["F13_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("1B"); break;
                                        }
                                    }
                                    break;
                                case '4':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F4_" + CAS + "_" + GetTelnetKeyboardConf(1)]); break;
                                            case 1: Send(TerminalKeys["F14_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("1C"); break;
                                        }
                                    }
                                    break;
                                case '5':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F5_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 1: Send(TerminalKeys["F15_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("1D"); break;
                                        }
                                    }
                                    break;
                                case '6':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F6_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 1: Send(TerminalKeys["F16_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("1E"); break;
                                        }
                                    }
                                    break;
                                case '7':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F7_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 1: Send(TerminalKeys["F17_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("1F"); break;
                                        }
                                    }
                                    break;
                                case '8':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F8_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 1: Send(TerminalKeys["F18_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("7F"); break;
                                        }
                                    }
                                    break;
                                case '9':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F9_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 1: Send(TerminalKeys["F19_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                        }
                                    }
                                    break;
                                case '0':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F10_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 1: Send(TerminalKeys["F20_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                        }
                                    }
                                    break;
                                case ',':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        Clipboard_.TextClipboardCopy(Core_.AnsiState_.GetScreen(0, 0, Screen_.WinW, Screen_.WinH));
                                    }
                                    break;
                                case '.':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        Clipboard_.TextClipboardPaste();
                                    }
                                    break;
                                default:
                                    if (UseCtrlKeys)
                                    {
                                        switch (KeyChar)
                                        {
                                            case ' ':
                                            case '@':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("00");
                                                break;
                                            case '[':
                                            case '{':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("1B");
                                                break;
                                            case '\\':
                                            case '|':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("1C");
                                                break;
                                            case ']':
                                            case '}':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("1D");
                                                break;
                                            case '~':
                                            case '^':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("1E");
                                                break;
                                            case '/':
                                            case '_':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("1F");
                                                break;
                                            case '?':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("7F");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (KeyChar)
                                        {
                                            case '`':
                                                LocalEcho = !LocalEcho;
                                                break;
                                            case '|':
                                                Command_8bit = !Command_8bit;
                                                break;
                                            case '{':
                                                TelnetKeyboardConfMove();
                                                break;
                                            case '}':
                                                TelnetKeyboardConfStep();
                                                break;
                                            case '[':
                                                TelnetKeyboardModsMove();
                                                break;
                                            case ']':
                                                TelnetKeyboardModsStep();
                                                break;
                                            case '<':
                                                WorkStateC = WorkStateCDef.Session;
                                                if (TelnetFuncKeyOther < 2)
                                                {
                                                    Send(TerminalKeys["F11_" + CAS + "_" + GetTelnetKeyboardConf(2)]);
                                                }
                                                else
                                                {
                                                    Send(TerminalAnswerBack);
                                                }
                                                break;
                                            case '>':
                                                WorkStateC = WorkStateCDef.Session;
                                                if (TelnetFuncKeyOther < 2)
                                                {
                                                    Send(TerminalKeys["F12_" + CAS + "_" + GetTelnetKeyboardConf(2)]);
                                                }
                                                else
                                                {
                                                    Send(TerminalAnswerBack);
                                                }
                                                break;
                                            case '/':
                                                if (Conn.IsConnected() == 0)
                                                {
                                                    TelnetOpen();
                                                }
                                                else
                                                {
                                                    if (Conn.IsConnected() == 1)
                                                    {
                                                        TelnetClose(false);
                                                    }
                                                }
                                                break;
                                            case '?':
                                                {
                                                    WorkStateC = WorkStateCDef.Session;
                                                    TelnetSendWindowSize();
                                                }
                                                break;
                                            case '\\':
                                                {
                                                    DisplayConfigOpen();
                                                }
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }

                Monitor.Enter(FuncKeyBuf);
            }
            Monitor.Exit(FuncKeyBuf);
        }

        bool TelnetSendWindowSize()
        {
            if ((Core_.AnsiMaxX > 0) && (Core_.AnsiMaxY > 0))
            {
                Conn.Resize(Core_.AnsiMaxX, Core_.AnsiMaxY);
                return true;
            }
            else
            {
                return false;
            }
        }

        public byte[] Receive(bool Str)
        {
            if (Conn == null)
            {
                return new byte[0];
            }

            Conn.MonitorEnter();
            using (MemoryStream ms = new MemoryStream())
            {
                if (LocalEchoBuf.Count > 0)
                {
                    Monitor.Enter(LocalEchoBuf);
                    ms.Write(LocalEchoBuf.ToArray(), 0, LocalEchoBuf.ToArray().Length);
                    LocalEchoBuf.Clear();
                    Monitor.Exit(LocalEchoBuf);
                }
                Conn.Receive(ms);
                byte[] SS = ms.ToArray();
                if (SS.Length > 0)
                {
                    if (ConsoleTestI) Console.Write("> ");
                }
                if (Str)
                {
                    for (int i = 0; i < SS.Length; i++)
                    {
                        ByteStr.Add(SS[i]);
                        if (SS[i] < 32) { SS[i] = (byte)'_'; }
                        if (SS[i] > 126) { SS[i] = (byte)'_'; }
                    }
                    if (ConsoleTestI) Console.Write(Encoding.UTF8.GetString(SS));
                }
                else
                {
                    for (int i = 0; i < SS.Length; i++)
                    {
                        ByteStr.Add(SS[i]);
                        if (ConsoleTestI) Console.Write(((int)SS[i]).ToString("X").PadLeft(2, '0'));
                    }
                }
                if (SS.Length > 0)
                {
                    if (ConsoleTestI) Console.WriteLine();
                }
                Conn.MonitorExit();
                return SS;
            }
        }

        public byte[] StrToBytes(List<int> STR)
        {
            return TerminalEncoding.GetBytes(TextWork.IntToStr(STR));
        }

        public void Send(List<int> STR)
        {
            byte[] Buf = StrToBytes(STR);
            string Buf_ = "";
            for (int i = 0; i < Buf.Length; i++)
            {
                Buf_ += UniConn.H(Buf[i]);
            }
            Send(Buf_);
        }

        public void Send(string STR)
        {
            if (Command_8bit)
            {
                for (int i = 0x40; i < 0x60; i++)
                {
                    STR = STR.Replace("##_" + ((char)i), (i + 0x40).ToString("X"));
                    STR = STR.Replace("##" + (i.ToString("X")), (i + 0x40).ToString("X"));
                }
            }
            else
            {
                STR = STR.Replace("##", "1B");
            }
            Conn.SendHex(STR);
        }

        public void ProcessBuf(Stopwatch TelnetTimer)
        {
            int ToSend = ByteStr.Count;

            // Avoid cutted UTF stream
            if (TerminalEncodingUTF)
            {
                if (TerminalEncodingUTF8)
                {
                    ToSend = TextWork.FullUTF8(ByteStr, ToSend);
                }
                else
                {
                    if (TerminalEncodingUTF16LE)
                    {
                        ToSend = TextWork.FullUTF16LE(ByteStr, ToSend);
                    }
                    else
                    {
                        if (TerminalEncodingUTF16BE)
                        {
                            ToSend = TextWork.FullUTF16BE(ByteStr, ToSend);
                        }
                        else
                        {
                            if (TerminalEncodingUTF32)
                            {
                                ToSend = TextWork.FullUTF32(ByteStr, ToSend);
                            }
                        }
                    }
                }
            }

            if (ToSend > 0)
            {
                string TempStr = TerminalEncoding.GetString(ByteStr.ToArray(), 0, ToSend);
                if (TelnetFileUse)
                {
                    TelnetFileW.Write(TextWork.TelnetTimerBegin);
                    TelnetFileW.Write(Encoding.UTF8.GetBytes((TelnetTimer.ElapsedMilliseconds / TelnetTimerResolution).ToString()));
                    TelnetFileW.Write(TextWork.TelnetTimerEnd);
                    TelnetFileW.Write(ByteStr.ToArray());
                }
                FileCtX.AddRange(TextWork.StrToInt(TempStr));

                ByteStr.RemoveRange(0, ToSend);
            }
        }

        bool OpenCloseRepaint = false;
        bool TerminalAutoSendWindowFlag = false;
        Stopwatch TerminalAutoSendWindowSW = new Stopwatch();

        public void TelnetOpen()
        {
            TerminalAutoSendWindowFlag = false;
            TerminalAutoSendWindowSW.Stop();
            TerminalAutoSendWindowSW.Reset();
            LocalEcho = false;
            Command_8bit = false;
            UseCtrlKeys = false;
            WindowTitle = "";
            WindowIcon = "";

            int DefaultPort = 0;
            string[] AddrPort = Core_.CurrentFileName.Split(':');
            switch (TerminalConnection.ToUpperInvariant())
            {
                default:
                    Conn = new UniConnLoopback(TerminalEncoding);
                    break;
                case "RAW":
                    DefaultPort = 23;
                    Conn = new UniConnRaw(TerminalEncoding);
                    break;
                case "TELNET":
                    DefaultPort = 23;
                    Conn = new UniConnTelnet(TerminalEncoding);
                    break;
                case "SSH":
                    DefaultPort = 22;
                    Conn = new UniConnSSH(TerminalEncoding);
                    break;
                case "APPLICATION":
                    AddrPort = new string[2];
                    AddrPort[0] = Core_.CurrentFileName;
                    AddrPort[1] = "0";
                    Conn = new UniConnApp(TerminalEncoding);
                    break;
                case "SERIAL":
                    AddrPort = new string[2];
                    AddrPort[0] = Core_.CurrentFileName;
                    AddrPort[1] = "0";
                    Conn = new UniConnSerial(TerminalEncoding);
                    break;
            }
            try
            {
                if (AddrPort.Length >= 2)
                {
                    Conn.Open(AddrPort[0], int.Parse(AddrPort[1]), TerminalName, Core_.Screen_.WinW, Core_.Screen_.WinH);
                }
                else
                {
                    Conn.Open(AddrPort[0], DefaultPort, TerminalName, Core_.Screen_.WinW, Core_.Screen_.WinH);
                }
                if (TelnetAutoSendWindowSize > 0)
                {
                    TerminalAutoSendWindowFlag = true;
                    TerminalAutoSendWindowSW.Start();
                }
            }
            catch (Exception e)
            {
                Conn = new UniConnLoopback(TerminalEncoding);
                Conn.Open("", 0, "", Core_.Screen_.WinW, Core_.Screen_.WinH);
                Conn.Send(TerminalEncoding.GetBytes(e.Message));
                Conn.Close();
            }
            if (Server_ != null)
            {
                Server_.Receive();
                Server_.Send(UniConn.HexToRaw(TerminalResetCommand));
            }
            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
            Monitor.Enter(TelnetMutex);
            Core_.AnsiProcessReset(false, true, 0);
            Core_.AnsiState_.AnsiParamSet(1, 4);
            Core_.AnsiState_.AnsiParamSet(2, 2);
            Core_.AnsiState_.AnsiParamSet(3, 2);
            Core_.AnsiState_.AnsiParamSet(4, 2);
            Core_.AnsiState_.AnsiParamSet(5, 4);
            Core_.AnsiState_.AnsiParamSet(7, 4);
            Core_.AnsiState_.AnsiParamSet(10, 4);
            Core_.AnsiState_.AnsiParamSet(11, 4);
            Core_.AnsiState_.AnsiParamSet(12, 1);
            Core_.AnsiState_.AnsiParamSet(13, 4);
            Core_.AnsiState_.AnsiParamSet(14, 4);
            Core_.AnsiState_.AnsiParamSet(15, 4);
            Core_.AnsiState_.AnsiParamSet(16, 4);
            Core_.AnsiState_.AnsiParamSet(17, 4);
            Core_.AnsiState_.AnsiParamSet(18, 4);
            Core_.AnsiState_.AnsiParamSet(19, 4);
            Core_.AnsiState_.AnsiParamSet(20, 2);
            Monitor.Exit(TelnetMutex);
            OpenCloseRepaint = true;
        }

        void TelnetCloseFile()
        {
            if (TelnetFileUse)
            {
                try
                {
                    TelnetFileW.Close();
                }
                catch
                {

                }
                try
                {
                    TelnetFileS.Close();
                }
                catch
                {

                }
            }
        }

        public void TelnetClose(bool StopApp)
        {
            if (Conn != null)
            {
                if (Conn.IsConnected() != 0)
                {
                    Conn.Close();
                }
            }
            if (StopApp)
            {
                TelnetCloseFile();
                Screen_.CloseApp(Core_.TextNormalBack, Core_.TextNormalFore);
            }
            else
            {
                OpenCloseRepaint = true;
            }
        }

        public void TelnetClientWork()
        {
            EscapeKeyMessage();

            if (ServerPort > 0)
            {
                Server_ = new Server();
                if (!Server_.Start(ServerPort, ServerTelnet))
                {
                    Server_ = null;
                }
            }
            else
            {
                Server_ = null;
            }

            WorkStateC = WorkStateCDef.InfoScreen;
            EscapeKey = "";
            while (Screen_.AppWorking && ("".Equals(EscapeKey)))
            {
                Thread.Sleep(100);
            }
            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
            Screen_.SetCursorPosition(0, 0);
            WorkStateC = WorkStateCDef.Session;







            FileCtX = new List<int>();
            TelnetOpen();
            WorkStateCDef WorkState_ = WorkStateCDef.Session;
            string ToRepaintState = "";
            int ConnStatusX = Conn.IsConnected();

            TelnetFileUse = false;
            if (!("".Equals(TelnetFileName)))
            {
                try
                {
                    TelnetFileS = new FileStream(TelnetFileName, FileMode.Create, FileAccess.Write);
                    TelnetFileW = new BinaryWriter(TelnetFileS);
                    TelnetFileUse = true;
                }
                catch
                {

                }
            }

            Stopwatch TelnetTimer = new Stopwatch();
            FileCtX.Clear();
            TelnetTimer.Start();
            long TelnetTimerNext = TelnetTimerResolution;
            int TelnetTimerWait = (int)(TelnetTimerResolution / 10L);
            if (TelnetTimerWait < 1)
            {
                TelnetTimerWait = 1;
            }
            try
            {
                while (Screen_.AppWorking)
                {
                    if (TerminalAutoSendWindowFlag)
                    {
                        if (TerminalAutoSendWindowSW.ElapsedMilliseconds > TelnetAutoSendWindowSize)
                        {
                            if (TelnetSendWindowSize())
                            {
                                TerminalAutoSendWindowSW.Stop();
                                TerminalAutoSendWindowFlag = false;
                            }
                        }
                    }
                    while (TelnetTimer.ElapsedMilliseconds > TelnetTimerNext)
                    {
                        TelnetTimerNext += TelnetTimerResolution;
                    }
                    //TelnetTimerNext = ((TelnetTimer.ElapsedMilliseconds / TelnetTimerResolution) * TelnetTimerResolution) + TelnetTimerResolution;
                    while (TelnetTimer.ElapsedMilliseconds <= TelnetTimerNext)
                    {
                        Thread.Sleep(TelnetTimerWait);
                    }
                    if (ConnStatusX != Conn.IsConnected())
                    {
                        if (WorkStateC == WorkStateCDef.Toolbox)
                        {
                            WorkState_ = WorkStateCDef.Toolbox_;
                        }
                        ConnStatusX = Conn.IsConnected();
                    }
                    Receive(false);
                    ProcessBuf(TelnetTimer);
                    bool WasAnsiProcess = false;

                    Core_.AnsiProcessSupply(FileCtX);
                    FileCtX.Clear();

                    Monitor.Enter(TelnetMutex);
                    int AnsiBufferI1 = Core_.AnsiState_.AnsiBufferI;
                    WasAnsiProcess = (Core_.AnsiProcess(TerminalStep) != 0);
                    int AnsiBufferI2 = Core_.AnsiState_.AnsiBufferI;
                    Monitor.Exit(TelnetMutex);
                    if (Server_ != null)
                    {
                        byte[] ServerBuf = Server_.Receive();
                        if ((ServerBuf.Length > 0) && (Conn.IsConnected() == 1))
                        {
                            /*for (int I = 0; I < ServerBuf.Length; I++)
                            {
                                Console.Write(ServerBuf[I] + "_");
                            }
                            Console.WriteLine();*/
                            string DataStr = ServerEncoding.GetString(ServerBuf);
                            Conn.Send(TerminalEncoding.GetBytes(DataStr));
                        }
                    }

                    if (WasAnsiProcess)
                    {
                        for (int i = 0; i < Core_.__AnsiResponse.Count; i++)
                        {
                            TelnetReport(Core_.__AnsiResponse[i]);
                        }
                        Core_.__AnsiResponse.Clear();

                        if ((Server_ != null) && (AnsiBufferI2 > AnsiBufferI1))
                        {
                            Server_.Send(ServerEncoding.GetBytes(TextWork.IntToStr(Core_.AnsiBuffer.GetRange(AnsiBufferI1, AnsiBufferI2 - AnsiBufferI1))));
                        }


                        if (Core_.AnsiGetFontSize(Core_.AnsiState_.__AnsiY) > 0)
                        {
                            Screen_.SetCursorPosition(Core_.AnsiState_.__AnsiX * 2, Core_.AnsiState_.__AnsiY);
                        }
                        else
                        {
                            Screen_.SetCursorPosition(Core_.AnsiState_.__AnsiX, Core_.AnsiState_.__AnsiY);
                        }
                        if (WorkStateC == WorkStateCDef.Toolbox)
                        {
                            WorkState_ = WorkStateCDef.Toolbox_;
                        }
                        if (WorkStateC == WorkStateCDef.DispConf)
                        {
                            DisplayConfig_.DisplayMenu();
                        }
                    }
                    if (WorkStateC == WorkStateCDef.DispConf)
                    {
                        if (DisplayConfig_RequestReapint)
                        {
                            Core_.AnsiRepaint(false);
                            DisplayConfig_RequestReapint = false;
                        }
                        if (DisplayConfig_RequestMenu)
                        {
                            DisplayConfig_.DisplayMenu();
                            DisplayConfig_RequestMenu = false;
                        }
                        if (DisplayConfig_RequestClose)
                        {
                            TelnetInfoPos = DisplayConfig_.MenuPos;
                            WorkStateC = WorkStateCDef.Toolbox;
                            TelnetDisplayInfo(true);
                            DisplayConfig_RequestClose = false;
                        }
                    }
                    if (OpenCloseRepaint)
                    {
                        if (WorkStateC == WorkStateCDef.Toolbox)
                        {
                            WorkState_ = WorkStateCDef.Toolbox_;
                        }
                    }
                    if ((WorkStateC == WorkStateCDef.Toolbox) || (WorkStateC == WorkStateCDef.EscapeKey))
                    {
                        string ToRepaintStateX = TelnetInfoPos.ToString() + "_" + TelnetKeyboardConfI.ToString() + "_" + TelnetKeyboardConf;
                        ToRepaintStateX = ToRepaintStateX + "_" + TelnetKeyboardModsI + "_" + TelnetKeyboardMods + "_" + TelnetFuncKeyOther;
                        ToRepaintStateX = ToRepaintStateX + "_" + LocalEcho + "_" + Command_8bit + "_" + UseCtrlKeys;
                        ToRepaintStateX = ToRepaintStateX + "_|" + WindowTitle + "|_";
                        ToRepaintStateX = ToRepaintStateX + "_|" + WindowIcon + "|_";

                        if ((WorkState_ != WorkStateC) || (ToRepaintState != ToRepaintStateX))
                        {
                            bool NeedRepaint = (ToRepaintState != ToRepaintStateX);
                            NeedRepaint = NeedRepaint | (WorkState_ != WorkStateCDef.Toolbox_);
                            NeedRepaint = NeedRepaint | OpenCloseRepaint;
                            OpenCloseRepaint = false;
                            TelnetDisplayInfo(NeedRepaint);
                            WorkState_ = WorkStateC;
                            ToRepaintState = ToRepaintStateX;
                        }
                    }
                    if ((WorkStateC == WorkStateCDef.Session) && (WorkState_ != WorkStateC))
                    {
                        Core_.AnsiRepaint(false);
                        if (Core_.AnsiGetFontSize(Core_.AnsiState_.__AnsiY) > 0)
                        {
                            Screen_.SetCursorPosition(Core_.AnsiState_.__AnsiX * 2, Core_.AnsiState_.__AnsiY);
                        }
                        else
                        {
                            Screen_.SetCursorPosition(Core_.AnsiState_.__AnsiX, Core_.AnsiState_.__AnsiY);
                        }
                        WorkState_ = WorkStateC;
                    }
                    FuncKeyProcess();
                }

                TelnetCloseFile();
            }
            finally
            {
                TelnetCloseFile();
            }

            if (Server_ != null)
            {
                Server_.Stop();
            }
            Screen_.CloseApp(Core_.TextNormalBack, Core_.TextNormalFore);
        }

        void TelnetDisplayInfo(bool NeedRepaint)
        {
            if (NeedRepaint)
            {
                Core_.AnsiRepaint(false);
            }
            int CB_ = Core_.PopupBack;
            int CF_ = Core_.PopupFore;

            List<string> InfoMsg = new List<string>();
            int IsConn = Conn.IsConnected();
            if (WorkStateC == WorkStateCDef.Toolbox)
            {
                string StatusInfo = "Unknown";
                switch (IsConn)
                {
                    case 0: StatusInfo = "Disconnected"; break;
                    case 1: StatusInfo = "Connected"; break;
                    case 2: StatusInfo = "Connecting"; break;
                    case 3: StatusInfo = "Disconnecting"; break;
                }
                InfoMsg.Add(" Status: " + StatusInfo + " ");
                if ((WindowTitle != "") || (WindowIcon != ""))
                {
                    if (WindowTitle.Equals(WindowIcon))
                    {
                        InfoMsg.Add(" Title: " + WindowTitle + " ");
                    }
                    else
                    {
                        if (WindowTitle.Equals("") || WindowIcon.Equals(""))
                        {
                            InfoMsg.Add(" Title: " + (WindowIcon + WindowTitle) + " ");
                        }
                        else
                        {
                            InfoMsg.Add(" Title: " + (WindowIcon + " " + WindowTitle) + " ");
                        }
                    }
                }
                InfoMsg.Add(" Screen size: " + Core_.AnsiMaxX + "x" + Core_.AnsiMaxY + " ");
                InfoMsg.Add(" Escape key: " + EscapeKey + " ");
                InfoMsg.Add(" Esc - Return to terminal ");
                InfoMsg.Add(" Enter - Change escape key ");
                InfoMsg.Add(" Tab - Move info ");
                InfoMsg.Add(" Backspace - Quit ");
                switch (TelnetFuncKeyOther)
                {
                    case 0:
                        InfoMsg.Add(" 1-0 - Send F1-F10                  ");
                        InfoMsg.Add(" < > - Send F11, F12                ");
                        break;
                    case 1:
                        InfoMsg.Add(" 1-0 - Send F11-F20                 ");
                        InfoMsg.Add(" < > - Send F11, F12                ");
                        break;
                    case 2:
                        InfoMsg.Add(" 2-8 - Send NUL,ESC,FS,GS,RS,US,DEL ");
                        InfoMsg.Add(" < > - Send AnswerBack              ");
                        break;
                }
                InfoMsg.Add(" = ; : - Change control code ");
                InfoMsg.Add(" Letter - Send CTRL+letter ");
                string TelnetKeyboardConf_ = "";
                for (int i = 0; i < TelnetKeyboardConfMax.Length; i++)
                {
                    if (i == TelnetKeyboardConfI)
                    {
                        TelnetKeyboardConf_ = TelnetKeyboardConf_ + "[";
                    }
                    else
                    {
                        if (i == (TelnetKeyboardConfI + 1))
                        {
                            TelnetKeyboardConf_ = TelnetKeyboardConf_ + "]";
                        }
                        else
                        {
                            TelnetKeyboardConf_ = TelnetKeyboardConf_ + " ";
                        }
                    }
                    TelnetKeyboardConf_ = TelnetKeyboardConf_ + TelnetKeyboardConf[i];
                }
                if (TelnetKeyboardConfMax.Length == (TelnetKeyboardConfI + 1))
                {
                    TelnetKeyboardConf_ = TelnetKeyboardConf_ + "]";
                }
                else
                {
                    TelnetKeyboardConf_ = TelnetKeyboardConf_ + " ";
                }


                string TelnetKeyboardMods_ = "";
                for (int i = 0; i < TelnetKeyboardMods.Length; i++)
                {
                    if (i == TelnetKeyboardModsI)
                    {
                        TelnetKeyboardMods_ = TelnetKeyboardMods_ + "[";
                    }
                    else
                    {
                        if (i == (TelnetKeyboardModsI + 1))
                        {
                            TelnetKeyboardMods_ = TelnetKeyboardMods_ + "]";
                        }
                        else
                        {
                            TelnetKeyboardMods_ = TelnetKeyboardMods_ + " ";
                        }
                    }
                    TelnetKeyboardMods_ = TelnetKeyboardMods_ + TelnetKeyboardMods[i];
                }
                if (TelnetKeyboardMods.Length == (TelnetKeyboardModsI + 1))
                {
                    TelnetKeyboardMods_ = TelnetKeyboardMods_ + "]";
                }
                else
                {
                    TelnetKeyboardMods_ = TelnetKeyboardMods_ + " ";
                }

                if (UseCtrlKeys)
                {
                    InfoMsg.Add(" @ - Send NUL");
                    InfoMsg.Add(" [ { - Send ESC");
                    InfoMsg.Add(" \\ | - Send FS");
                    InfoMsg.Add(" ] } - Send GS");
                    InfoMsg.Add(" ~ ^ - Send RS");
                    InfoMsg.Add(" / _ - Send US");
                    InfoMsg.Add(" ? - Send DEL");
                }
                else
                {
                    InfoMsg.Add(" { } - Key codes: " + TelnetKeyboardConf_ + " ");
                    InfoMsg.Add(" [ ] - Ctrl,Alt,Shift: " + TelnetKeyboardMods_ + " ");
                    InfoMsg.Add(" / - Connect/disconnect ");
                    InfoMsg.Add(" ? - Send screen size ");
                    InfoMsg.Add(" ` - Local echo: " + (LocalEcho ? "on" : "off"));
                    InfoMsg.Add(" | - Input commands: " + (Command_8bit ? "8-bit" : "7-bit"));
                    InfoMsg.Add(" \\ - Display configuration");
                }
                InfoMsg.Add(" , - Copy screen as text");
                InfoMsg.Add(" . - Paste text as keystrokes");
            }
            if (WorkStateC == WorkStateCDef.EscapeKey)
            {
                InfoMsg.Add(" Press key, which will be ");
                InfoMsg.Add(" used as new escape key   ");
            }

            int InfoW = 0;
            int InfoH = InfoMsg.Count;

            List<int>[] InfoMsg_ = new List<int>[InfoH];
            for (int i = 0; i < InfoH; i++)
            {
                InfoMsg_[i] = TextWork.StrToInt(InfoMsg[i]);
                InfoW = Math.Max(InfoW, InfoMsg_[i].Count);
            }
            for (int i = 0; i < InfoH; i++)
            {
                while (InfoMsg_[i].Count < InfoW)
                {
                    InfoMsg_[i].Add(32);
                }
            }



            int OffsetX = 0;
            int OffsetY = 0;
            int InfoCX = InfoW;
            int InfoCY = InfoH;
            if ((TelnetInfoPos == 1) || (TelnetInfoPos == 3))
            {
                OffsetX = Screen_.WinW - InfoW;
                InfoCX = OffsetX - 1;
            }
            if ((TelnetInfoPos == 2) || (TelnetInfoPos == 3))
            {
                OffsetY = Screen_.WinH - InfoH;
                InfoCY = OffsetY - 1;
            }


            for (int i = -1; i < InfoW + 1; i++)
            {
                Screen_.PutChar(OffsetX + i, OffsetY + InfoH, ' ', CF_, CB_);
                Screen_.PutChar(OffsetX + i, OffsetY - 1, ' ', CF_, CB_);
            }
            for (int i = 0; i < InfoH; i++)
            {
                Screen_.PutChar(OffsetX + InfoW, OffsetY + i, ' ', CF_, CB_);
                Screen_.PutChar(OffsetX - 1, OffsetY + i, ' ', CF_, CB_);
            }

            for (int I = 0; I < InfoH; I++)
            {
                for (int II = 0; II < InfoW; II++)
                {
                    Screen_.PutChar(OffsetX + II, OffsetY + I, InfoMsg_[I][II], CB_, CF_);
                }
            }
            Screen_.SetCursorPosition(InfoCX, InfoCY);
        }


        void TextPasteWork(string Raw)
        {
            Conn.Send(TerminalEncoding.GetBytes(Raw));
        }


        int TelnetInfoPos = 0;

        string TerminalConnection = "";


        string TerminalName = "";

        string TerminalAnswerBack = "";

        // 0 - VT100
        // 1 - VT102
        // 2 - VT220
        // 3 - VT320
        // 4 - VT420
        // 5 - VT520
        int TerminalType = 1;

        string TelnetReportStrToStr(string N)
        {
            List<int> N_ = TextWork.StrToInt(N);
            string S = "";
            for (int i = 0; i < N_.Count; i++)
            {
                string Chr = TextWork.CharCode(N_[i], 0);
                if (Chr.EndsWith("??"))
                {
                    Chr = "_ ";
                }
                S = S + Chr;
            }
            return S;
        }

        string TelnetReportNumToStr(int N)
        {
            string N_ = N.ToString();
            string S = "";
            for (int i = 0; i < N_.Length; i++)
            {
                S = S + "_" + N_[i];
            }
            return S;
        }

        void TelnetReport(string ReportRequest)
        {
            switch (ReportRequest)
            {
                default:
                    {
                        if (ReportRequest.StartsWith("WindowTitle"))
                        {
                            WindowTitle = ReportRequest.Substring(11);
                        }
                        if (ReportRequest.StartsWith("WindowIcon"))
                        {
                            WindowIcon = ReportRequest.Substring(10);
                        }
                        if (ReportRequest.StartsWith("[") && ReportRequest.EndsWith("*y"))
                        {
                            string[] ParamStr = ReportRequest.Substring(1, ReportRequest.Length - 3).Split(';');
                            int N = 0;
                            string Resp = "##_P" + TelnetReportStrToStr(ParamStr[0]) + "_!_~";
                            Resp = Resp + TelnetReportStrToStr(TextWork.CharCode(N, 1));
                            Resp = Resp + "##_\\";
                            Send(Resp);
                        }
                        if (ReportRequest.StartsWith("$q"))
                        {
                            Send("##_P_0_$_r##_\\");
                        }
                        if (ReportRequest.StartsWith("[?") && ReportRequest.EndsWith("$p"))
                        {
                            int N = int.Parse(ReportRequest.Substring(2, ReportRequest.Length - 4));
                            int V = Core_.AnsiState_.DecParamGet(N);
                            switch (N)
                            {
                                case 1:
                                    V = (GetTelnetKeyboardConf(0) == "1") ? 1 : 2;
                                    break;
                                case 3:
                                    {
                                        V = 0;
                                        if (Core_.Screen_.WinW == 80) V = 2;
                                        if (Core_.Screen_.WinW == 132) V = 1;
                                    }
                                    break;
                                case 4:
                                    V = Core_.AnsiState_.__AnsiSmoothScroll ? 1 : 2;
                                    break;
                                case 5:
                                    {
                                        V = 2;
                                        if (Core_.AnsiState_.__AnsiLineOccupy__.CountLines() > 0)
                                        {
                                            if (Core_.AnsiState_.__AnsiLineOccupy__.CountItems(0) > 0)
                                            {
                                                Core_.AnsiState_.__AnsiLineOccupy__.Get(0, 0);
                                                if (Core_.AnsiState_.__AnsiLineOccupy__.Item_ColorA >= 128)
                                                {
                                                    V = 1;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case 6:
                                    V = Core_.AnsiState_.__AnsiOrigin ? 1 : 2;
                                    break;
                                case 7:
                                    V = Core_.AnsiState_.__AnsiNoWrap ? 2 : 1;
                                    break;
                                case 47:
                                case 1047:
                                case 1049:
                                    V = Core_.AnsiState_.IsScreenAlternate ? 1 : 2;
                                    break;
                                case 66:
                                    V = (GetTelnetKeyboardConf(6) == "1") ? 1 : 2;
                                    break;
                                case 67:
                                    V = (GetTelnetKeyboardConf(5) == "1") ? 1 : 2;
                                    break;
                                case 69:
                                    V = Core_.AnsiState_.__AnsiMarginLeftRight ? 1 : 2;
                                    break;
                                case 95:
                                    V = Core_.AnsiState_.DECCOLMPreserve ? 1 : 2;
                                    break;
                            }
                            Send("##_[_?" + TelnetReportNumToStr(N) + "_;_" + V.ToString() + "_$_y");
                        }
                        else
                        {
                            if (ReportRequest.StartsWith("[") && ReportRequest.EndsWith("$p"))
                            {
                                int N = int.Parse(ReportRequest.Substring(1, ReportRequest.Length - 3));
                                int V = Core_.AnsiState_.AnsiParamGet(N);
                                switch (N)
                                {
                                    case 12:
                                        V = LocalEcho ? 2 : 1;
                                        break;
                                    case 20:
                                        V = (GetTelnetKeyboardConf(4) == "1") ? 1 : 2;
                                        break;
                                }
                                Send("##_[" + TelnetReportNumToStr(N) + "_;_" + V.ToString() + "_$_y");
                            }
                        }
                    }
                    break;
                case "[20t":
                    {
                        Send("##_[_L" + TelnetReportStrToStr(WindowIcon) + "##_\\");
                    }
                    break;
                case "[21t":
                    {
                        Send("##_[_l" + TelnetReportStrToStr(WindowTitle) + "##_\\");
                    }
                    break;
                case "[19t":
                    {
                        Send("##_[_9_;" + TelnetReportNumToStr(Core_.Screen_.WinH) + "_;" + TelnetReportNumToStr(Core_.Screen_.WinW) + "_t");
                    }
                    break;
                case "[18t":
                    {
                        Send("##_[_8_;" + TelnetReportNumToStr(Core_.Screen_.WinH) + "_;" + TelnetReportNumToStr(Core_.Screen_.WinW) + "_t");
                    }
                    break;
                case "[14t":
                case "[14;2t":
                    {
                        Send("##_[_4_;" + TelnetReportNumToStr(Core_.Screen_.WinH * Screen.TerminalCellH) + "_;" + TelnetReportNumToStr(Core_.Screen_.WinW * Screen.TerminalCellW) + "_t");
                    }
                    break;
                case "[13t":
                case "[13;2t":
                    {
                        Send("##_[_3_;_0_;_0_t");
                    }
                    break;
                case "[11t":
                    {
                        Send("##_[_1_t");
                    }
                    break;
                case "[?15n": // Printer
                    {
                        Send("##_[_?_1_3_n");
                    }
                    break;
                case "[?25n": // UDK
                    {
                        Send("##_[_?_2_0_n");
                    }
                    break;
                case "[?26n": // Keyboard
                    {
                        Send("##_[_?_2_7_;_1_;_0_;_0_n");
                    }
                    break;
                case "[?53n": // Locator
                    {
                        Send("##_[_?_5_3_n");
                    }
                    break;
                case "[?62n": // DECMSR
                    {
                        Send("##_[_0_0_0_0_*_{");
                    }
                    break;
                case "[?63;1n": // DECCKSR
                    {
                        Send("##_P_1_!_~_0_0_0_0##_\\");
                    }
                    break;
                case "[?75n": // Data integrity
                    {
                        Send("##_[_?_7_0_n");
                    }
                    break;
                case "[?85n": // Multi-session
                    {
                        Send("##_[_?_8_3_n");
                    }
                    break;
                case "[?6n": // DECXCPR
                    {
                        Send("##_[_?" + TelnetReportNumToStr(Core_.ReportCursorY()) + "_;" + TelnetReportNumToStr(Core_.ReportCursorX()) + "_;_1_R");
                    }
                    break;
                case "$q\"p": // DCS / DECSCL
                    {
                        string Bit = Command_8bit ? "_0" : "_1";
                        switch (TerminalType)
                        {
                            case 0:
                                Send("##_P_1_$_r_6_1_;" + Bit + "_\"_p##_\\");
                                break;
                            case 1:
                                Send("##_P_1_$_r_6_1_;" + Bit + "_\"_p##_\\");
                                break;
                            case 2:
                                Send("##_P_1_$_r_6_2_;" + Bit + "_\"_p##_\\");
                                break;
                            case 3:
                                Send("##_P_1_$_r_6_3_;" + Bit + "_\"_p##_\\");
                                break;
                            case 4:
                                Send("##_P_1_$_r_6_4_;" + Bit + "_\"_p##_\\");
                                break;
                            case 5:
                                Send("##_P_1_$_r_6_5_;" + Bit + "_\"_p##_\\");
                                break;
                        }
                    }
                    break;
                case "$q*|": // DCS / DECSNLS
                    {
                        Send("##_P_1_$_r" + TelnetReportNumToStr(Core_.AnsiMaxY) + "_*_|##_\\");
                    }
                    break;
                case "$qr": // DCS / DECSTBM
                    {
                        Send("##_P_1_$_r" + TelnetReportNumToStr(Core_.AnsiState_.__AnsiScrollFirst + 1) + "_;" + TelnetReportNumToStr(Core_.AnsiState_.__AnsiScrollLast + 1) + "_r##_\\");
                    }
                    break;
                case "$qs": // DCS / DECSLRM
                    {
                        Send("##_P_1_$_r" + TelnetReportNumToStr(Core_.AnsiState_.__AnsiMarginLeft + 1) + "_;" + TelnetReportNumToStr(Core_.AnsiState_.__AnsiMarginRight) + "_s##_\\");
                    }
                    break;
                case "[0c": // Primary DA
                    {
                        switch (TerminalType)
                        {
                            case 0:
                                Send("##_[_?_1_;_2_c");
                                break;
                            case 1:
                                Send("##_[_?_6_c");
                                break;
                            case 2:
                                Send("##_[_?_6_2_;_1_;_2_;_6_;_7_;_8_;_9_c");
                                break;
                            case 3:
                                Send("##_[_?_6_3_;_1_;_2_;_6_;_7_;_8_;_9_c");
                                break;
                            case 4:
                                Send("##_[_?_6_4_;_1_;_2_;_6_;_7_;_8_;_9_;_1_5_;_1_8_;_2_1_c");
                                break;
                            case 5:
                                Send("##_[_?_6_5_;_1_;_2_;_6_;_7_;_8_;_9_;_1_5_;_1_8_;_2_1_c");
                                break;
                        }
                    }
                    break;
                case "[>c": // Secondary DA
                case "[>0c": // Secondary DA
                    {
                        switch (TerminalType)
                        {
                            case 0:
                                Send("##_[_>_0_;_1_0_;_0_c");
                                break;
                            case 1:
                                Send("##_[_>_0_;_1_0_;_0_c");
                                break;
                            case 2:
                                Send("##_[_>_1_;_1_0_;_0_c");
                                break;
                            case 3:
                                Send("##_[_>_2_4_;_1_0_;_0_c");
                                break;
                            case 4:
                                Send("##_[_>_4_1_;_1_0_;_0_c");
                                break;
                            case 5:
                                Send("##_[_>_6_4_;_1_0_;_0_c");
                                break;
                        }
                    }
                    break;
                case "[=c": // Tertiary DA
                case "[=0c": // Tertiary DA
                    Send("##_P_!_|_0_0_0_0_0_0_0_0##_\\");
                    break;
                case "[6n": // DSR / CPR
                    {
                        Send("##_[" + TelnetReportNumToStr(Core_.ReportCursorY()) + "_;" + TelnetReportNumToStr(Core_.ReportCursorX()) + "_R");
                    }
                    break;
                case "VT52:Z":
                    Send("1B_/_Z");
                    break;
                case "[5n": // DSR
                    Send("##_[_0_n");
                    break;
                case "[0x": // DECREQTPARM
                    Send("##_[_2_;_1_;_1_;_1_1_2_;_1_1_2_;_1_;_0_x");
                    break;
                case "[1x": // DECREQTPARM
                    Send("##_[_3_;_1_;_1_;_1_1_2_;_1_1_2_;_1_;_0_x");
                    break;

                case "AnswerBack":
                    Send(TerminalAnswerBack);
                    break;

                case "Control8bit_1":
                    Command_8bit = true;
                    break;
                case "Control8bit_0":
                    Command_8bit = false;
                    break;
                case "LocalEcho_1":
                    LocalEcho = true;
                    break;
                case "LocalEcho_0":
                    LocalEcho = false;
                    break;
                case "CursorKey_1":
                    SetTelnetKeyboardConf(0, 1);
                    break;
                case "CursorKey_0":
                    SetTelnetKeyboardConf(0, 0);
                    break;
                case "EnterKey_1":
                    SetTelnetKeyboardConf(4, 1);
                    break;
                case "EnterKey_0":
                    SetTelnetKeyboardConf(4, 0);
                    break;
                case "NumpadKey_1":
                    SetTelnetKeyboardConf(6, 1);
                    break;
                case "NumpadKey_0":
                    SetTelnetKeyboardConf(6, 0);
                    break;
                case "BackspaceKey_1":
                    SetTelnetKeyboardConf(5, 1);
                    break;
                case "BackspaceKey_0":
                    SetTelnetKeyboardConf(5, 0);
                    break;
            }
        }
    }
}
