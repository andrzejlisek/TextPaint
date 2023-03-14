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
                                switch (KeyName)
                                {
                                    case "UpArrow":
                                        Send(TerminalKeys["Up_" + GetTelnetKeyboardConf(0)]);
                                        break;
                                    case "DownArrow":
                                        Send(TerminalKeys["Down_" + GetTelnetKeyboardConf(0)]);
                                        break;
                                    case "LeftArrow":
                                        Send(TerminalKeys["Left_" + GetTelnetKeyboardConf(0)]);
                                        break;
                                    case "RightArrow":
                                        Send(TerminalKeys["Right_" + GetTelnetKeyboardConf(0)]);
                                        break;
                                    case "Insert":
                                        Send(TerminalKeys["Insert_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "Delete":
                                        Send(TerminalKeys["Delete_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "Home":
                                        Send(TerminalKeys["Home_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "End":
                                        Send(TerminalKeys["End_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "PageUp":
                                        Send(TerminalKeys["PageUp_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "PageDown":
                                        Send(TerminalKeys["PageDown_" + GetTelnetKeyboardConf(3)]);
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


                                    case "F1": if (ModShift) { Send(TerminalKeys["F11_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F1_" + GetTelnetKeyboardConf(1)]); } break;
                                    case "F2": if (ModShift) { Send(TerminalKeys["F12_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F2_" + GetTelnetKeyboardConf(1)]); } break;
                                    case "F3": if (ModShift) { Send(TerminalKeys["F13_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F3_" + GetTelnetKeyboardConf(1)]); } break;
                                    case "F4": if (ModShift) { Send(TerminalKeys["F14_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F4_" + GetTelnetKeyboardConf(1)]); } break;
                                    case "F5": if (ModShift) { Send(TerminalKeys["F15_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F5_" + GetTelnetKeyboardConf(2)]); } break;
                                    case "F6": if (ModShift) { Send(TerminalKeys["F16_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F6_" + GetTelnetKeyboardConf(2)]); } break;
                                    case "F7": if (ModShift) { Send(TerminalKeys["F17_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F7_" + GetTelnetKeyboardConf(2)]); } break;
                                    case "F8": if (ModShift) { Send(TerminalKeys["F18_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F8_" + GetTelnetKeyboardConf(2)]); } break;
                                    case "F9": if (ModShift) { Send(TerminalKeys["F19_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F9_" + GetTelnetKeyboardConf(2)]); } break;
                                    case "F10": if (ModShift) { Send(TerminalKeys["F20_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F10_" + GetTelnetKeyboardConf(2)]); } break;
                                    case "F11": if (ModShift) { } else { Send(TerminalKeys["F11_" + GetTelnetKeyboardConf(2)]); } break;
                                    case "F12": if (ModShift) { } else { Send(TerminalKeys["F12_" + GetTelnetKeyboardConf(2)]); } break;

                                    case "F13": Send(TerminalKeys["F11_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F14": Send(TerminalKeys["F12_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F15": Send(TerminalKeys["F13_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F16": Send(TerminalKeys["F14_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F17": Send(TerminalKeys["F15_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F18": Send(TerminalKeys["F16_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F19": Send(TerminalKeys["F17_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F20": Send(TerminalKeys["F18_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F21": Send(TerminalKeys["F19_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F22": Send(TerminalKeys["F20_" + GetTelnetKeyboardConf(2)]); break;
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

                    case "D1":
                        {
                            WorkStateC = WorkStateCDef.Session;
                            switch (TelnetFuncKeyOther)
                            {
                                case 0: Send(TerminalKeys["F1_" + GetTelnetKeyboardConf(1)]); break;
                                case 1: Send(TerminalKeys["F11_" + GetTelnetKeyboardConf(2)]); break;
                                case 2: Send("00"); break;
                            }
                        }
                        break;
                    case "D2":
                        {
                            WorkStateC = WorkStateCDef.Session;
                            switch (TelnetFuncKeyOther)
                            {
                                case 0: Send(TerminalKeys["F2_" + GetTelnetKeyboardConf(1)]); break;
                                case 1: Send(TerminalKeys["F12_" + GetTelnetKeyboardConf(2)]); break;
                                case 2: Send("1B"); break;
                            }
                        }
                        break;
                    case "D3":
                        {
                            WorkStateC = WorkStateCDef.Session;
                            switch (TelnetFuncKeyOther)
                            {
                                case 0: Send(TerminalKeys["F3_" + GetTelnetKeyboardConf(1)]); break;
                                case 1: Send(TerminalKeys["F13_" + GetTelnetKeyboardConf(2)]); break;
                                case 2: Send("1C"); break;
                            }
                        }
                        break;
                    case "D4":
                        {
                            WorkStateC = WorkStateCDef.Session;
                            switch (TelnetFuncKeyOther)
                            {
                                case 0: Send(TerminalKeys["F4_" + GetTelnetKeyboardConf(1)]); break;
                                case 1: Send(TerminalKeys["F14_" + GetTelnetKeyboardConf(2)]); break;
                                case 2: Send("1D"); break;
                            }
                        }
                        break;
                    case "D5":
                        {
                            WorkStateC = WorkStateCDef.Session;
                            switch (TelnetFuncKeyOther)
                            {
                                case 0: Send(TerminalKeys["F5_" + GetTelnetKeyboardConf(2)]); break;
                                case 1: Send(TerminalKeys["F15_" + GetTelnetKeyboardConf(2)]); break;
                                case 2: Send("1E"); break;
                            }
                        }
                        break;
                    case "D6":
                        {
                            WorkStateC = WorkStateCDef.Session;
                            switch (TelnetFuncKeyOther)
                            {
                                case 0: Send(TerminalKeys["F6_" + GetTelnetKeyboardConf(2)]); break;
                                case 1: Send(TerminalKeys["F16_" + GetTelnetKeyboardConf(2)]); break;
                                case 2: Send("1F"); break;
                            }
                        }
                        break;
                    case "D7":
                        {
                            WorkStateC = WorkStateCDef.Session;
                            switch (TelnetFuncKeyOther)
                            {
                                case 0: Send(TerminalKeys["F7_" + GetTelnetKeyboardConf(2)]); break;
                                case 1: Send(TerminalKeys["F17_" + GetTelnetKeyboardConf(2)]); break;
                            }
                        }
                        break;
                    case "D8":
                        {
                            WorkStateC = WorkStateCDef.Session;
                            switch (TelnetFuncKeyOther)
                            {
                                case 0: Send(TerminalKeys["F8_" + GetTelnetKeyboardConf(2)]); break;
                                case 1: Send(TerminalKeys["F18_" + GetTelnetKeyboardConf(2)]); break;
                            }
                        }
                        break;
                    case "D9":
                        {
                            WorkStateC = WorkStateCDef.Session;
                            switch (TelnetFuncKeyOther)
                            {
                                case 0: Send(TerminalKeys["F9_" + GetTelnetKeyboardConf(2)]); break;
                                case 1: Send(TerminalKeys["F19_" + GetTelnetKeyboardConf(2)]); break;
                            }
                        }
                        break;
                    case "D0":
                        {
                            WorkStateC = WorkStateCDef.Session;
                            switch (TelnetFuncKeyOther)
                            {
                                case 0: Send(TerminalKeys["F10_" + GetTelnetKeyboardConf(2)]); break;
                                case 1: Send(TerminalKeys["F20_" + GetTelnetKeyboardConf(2)]); break;
                            }
                        }
                        break;

                    case "Space":
                        TelnetFuncKeyOther++;
                        if (TelnetFuncKeyOther == 3)
                        {
                            TelnetFuncKeyOther = 0;
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
                        switch (KeyChar)
                        {
                            case '`':
                            case '~':
                                LocalEcho = !LocalEcho;
                                break;
                            case '=':
                            case '+':
                                TelnetKeyboardConfMove();
                                break;
                            case '*':
                            case '-':
                                TelnetKeyboardConfStep();
                                break;
                            case '[':
                            case '{':
                                WorkStateC = WorkStateCDef.Session;
                                if (TelnetFuncKeyOther < 2)
                                {
                                    Send(TerminalKeys["F11_" + GetTelnetKeyboardConf(2)]);
                                }
                                else
                                {
                                    Send(TerminalAnswerBack);
                                }
                                break;
                            case ']':
                            case '}':
                                WorkStateC = WorkStateCDef.Session;
                                if (TelnetFuncKeyOther < 2)
                                {
                                    Send(TerminalKeys["F12_" + GetTelnetKeyboardConf(2)]);
                                }
                                else
                                {
                                    Send(TerminalAnswerBack);
                                }
                                break;
                            case '/':
                            case '?':
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
                            case '\\':
                            case '|':
                                {
                                    WorkStateC = WorkStateCDef.Session;
                                    TelnetSendWindowSize();
                                }
                                break;
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
            int TelnetInfoPos_ = -1;
            string TelnetKeyboardConf_ = "";
            int TelnetKeyboardConfI_ = -1;
            int TelnetFuncKeyOther_ = 0;
            bool LocalEcho_ = false;
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
                    }
                    if (OpenCloseRepaint)
                    {
                        if (WorkStateC == WorkStateCDef.Toolbox)
                        {
                            WorkState_ = WorkStateCDef.Toolbox_;
                        }
                    }
                    if (((WorkStateC == WorkStateCDef.Toolbox) || (WorkStateC == WorkStateCDef.EscapeKey)) && ((WorkState_ != WorkStateC) || (TelnetInfoPos_ != TelnetInfoPos) || (TelnetKeyboardConfI_ != TelnetKeyboardConfI) || (TelnetKeyboardConf_ != TelnetKeyboardConf) || (TelnetFuncKeyOther_ != TelnetFuncKeyOther) || (LocalEcho_ != LocalEcho)))
                    {
                        bool NeedRepaint = (TelnetInfoPos_ != TelnetInfoPos);
                        NeedRepaint = NeedRepaint | (TelnetKeyboardConfI_ != TelnetKeyboardConfI);
                        NeedRepaint = NeedRepaint | (TelnetKeyboardConf_ != TelnetKeyboardConf);
                        NeedRepaint = NeedRepaint | (LocalEcho_ != LocalEcho);
                        NeedRepaint = NeedRepaint | (WorkState_ != WorkStateCDef.Toolbox_);
                        NeedRepaint = NeedRepaint | OpenCloseRepaint;
                        OpenCloseRepaint = false;
                        TelnetDisplayInfo(NeedRepaint);
                        WorkState_ = WorkStateC;
                        TelnetInfoPos_ = TelnetInfoPos;
                        TelnetKeyboardConfI_ = TelnetKeyboardConfI;
                        TelnetKeyboardConf_ = TelnetKeyboardConf;
                        TelnetFuncKeyOther_ = TelnetFuncKeyOther;
                        LocalEcho_ = LocalEcho;
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
                InfoMsg.Add(" Screen size: " + Core_.AnsiMaxX + "x" + Core_.AnsiMaxY + " ");
                InfoMsg.Add(" Escape key: " + EscapeKey + " ");
                InfoMsg.Add(" Esc - Return to terminal ");
                InfoMsg.Add(" Enter - Change escape key ");
                InfoMsg.Add(" Tab - Move info ");
                InfoMsg.Add(" Backspace - Quit ");
                switch (TelnetFuncKeyOther)
                {
                    case 0:
                        InfoMsg.Add(" 1-0 - Send F1-F10 ");
                        InfoMsg.Add(" [, ] - Send F11, F12 ");
                        break;
                    case 1:
                        InfoMsg.Add(" 1-0 - Send F11-F20 ");
                        InfoMsg.Add(" [, ] - Send F11, F12 ");
                        break;
                    case 2:
                        InfoMsg.Add(" 1-6 - Send NUL,ESC,FS,GS,RS,US ");
                        InfoMsg.Add(" [, ] - Send AnswerBack ");
                        break;
                }
                InfoMsg.Add(" Space - Send other control code ");
                InfoMsg.Add(" Letter - Send CTRL+letter ");
                InfoMsg.Add(" / - Connect/disconnect ");
                InfoMsg.Add(" \\ - Send screen size ");
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
                InfoMsg.Add(" +=, -* - Key codes: " + TelnetKeyboardConf_ + " ");
                InfoMsg.Add(" `~ - Local echo: " + (LocalEcho ? "on" : "off"));
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
                Screen_.PutChar(OffsetX + i, OffsetY + InfoH, ' ', CF_, CB_, 0, 0);
                Screen_.PutChar(OffsetX + i, OffsetY - 1, ' ', CF_, CB_, 0, 0);
            }
            for (int i = 0; i < InfoH; i++)
            {
                Screen_.PutChar(OffsetX + InfoW, OffsetY + i, ' ', CF_, CB_, 0, 0);
                Screen_.PutChar(OffsetX - 1, OffsetY + i, ' ', CF_, CB_, 0, 0);
            }

            for (int I = 0; I < InfoH; I++)
            {
                for (int II = 0; II < InfoW; II++)
                {
                    Screen_.PutChar(OffsetX + II, OffsetY + I, InfoMsg_[I][II], CB_, CF_, 0, 0);
                }
            }
            Screen_.SetCursorPosition(InfoCX, InfoCY);
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

        void TelnetReport(string ReportRequest)
        {
            switch (ReportRequest)
            {
                case "$q\"p": // DCS / DECSCL
                    {
                        switch (TerminalType)
                        {
                            case 0:
                                Send("1B_P_1_$_r_6_1_;_1_\"_p1B_\\");
                                break;
                            case 1:
                                Send("1B_P_1_$_r_6_1_;_1_\"_p1B_\\");
                                break;
                            case 2:
                                Send("1B_P_1_$_r_6_2_;_1_\"_p1B_\\");
                                break;
                            case 3:
                                Send("1B_P_1_$_r_6_3_;_1_\"_p1B_\\");
                                break;
                            case 4:
                                Send("1B_P_1_$_r_6_4_;_1_\"_p1B_\\");
                                break;
                            case 5:
                                Send("1B_P_1_$_r_6_5_;_1_\"_p1B_\\");
                                break;
                        }
                    }
                    break;
                case "[0c": // Primary DA
                    {
                        switch (TerminalType)
                        {
                            case 0:
                                Send("1B_[_?_1_;_2_c");
                                break;
                            case 1:
                                Send("1B_[_?_6_c");
                                break;
                            case 2:
                                Send("1B_[_?_6_2_;_1_;_2_;_6_;_7_;_8_;_9_c");
                                break;
                            case 3:
                                Send("1B_[_?_6_3_;_1_;_2_;_6_;_7_;_8_;_9_c");
                                break;
                            case 4:
                                Send("1B_[_?_6_4_;_1_;_2_;_6_;_7_;_8_;_9_;_1_5_;_1_8_;_2_1_c");
                                break;
                            case 5:
                                Send("1B_[_?_6_5_;_1_;_2_;_6_;_7_;_8_;_9_;_1_5_;_1_8_;_2_1_c");
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
                                Send("1B_[_>_0_;_1_0_;_0_c");
                                break;
                            case 1:
                                Send("1B_[_>_0_;_1_0_;_0_c");
                                break;
                            case 2:
                                Send("1B_[_>_1_;_1_0_;_0_c");
                                break;
                            case 3:
                                Send("1B_[_>_2_4_;_1_0_;_0_c");
                                break;
                            case 4:
                                Send("1B_[_>_4_1_;_1_0_;_0_c");
                                break;
                            case 5:
                                Send("1B_[_>_6_4_;_1_0_;_0_c");
                                break;
                        }
                    }
                    break;
                case "[=c": // Tertiary DA
                case "[=0c": // Tertiary DA
                    Send("1B_P_!_|_0_0_0_0_0_0_0_01B_\\");
                    break;
                case "[6n": // DSR / CPR
                    {
                        string PosX = Core_.ReportCursorX().ToString();
                        string PosY = Core_.ReportCursorY().ToString();
                        string PosMsg = "1B_[";
                        for (int i = 0; i < PosY.Length; i++)
                        {
                            PosMsg = PosMsg + "_" + PosY[i].ToString();
                        }
                        PosMsg = PosMsg + "_;";
                        for (int i = 0; i < PosX.Length; i++)
                        {
                            PosMsg = PosMsg + "_" + PosX[i].ToString();
                        }
                        PosMsg = PosMsg + "_R";
                        Send(PosMsg);
                    }
                    break;
                case "VT52:Z":
                    Send("1B_/_Z");
                    break;
                case "[5n": // DSR
                    Send("1B_[_0_n");
                    break;
                case "[0x": // DECREQTPARM
                    Send("1B_[_2_;_1_;_1_;_1_1_2_;_1_1_2_;_1_;_0_x");
                    break;
                case "[1x": // DECREQTPARM
                    Send("1B_[_3_;_1_;_1_;_1_1_2_;_1_1_2_;_1_;_0_x");
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

                case "AnswerBack":
                    Send(TerminalAnswerBack);
                    break;

                case "LocalEcho_1":
                    LocalEcho = true;
                    break;
                case "LocalEcho_0":
                    LocalEcho = false;
                    break;
            }
        }
    }
}
