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

        public void CoreEvent_Client(string KeyName, char KeyChar, int KeyCharI)
        {
            if ("Resize".Equals(KeyName))
            {
                if (WorkState == 0)
                {
                    EscapeKeyMessage();
                }
                if ((WorkState == 2) || (WorkState == 4))
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
                    if (WorkState == 0)
                    {
                        EscapeKeyMessage();
                    }
                    if ((WorkState == 1) || (WorkState == 2))
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

                switch (WorkState)
                {
                    case 0:
                        {
                            if (!("".Equals(KeyName)))
                            {
                                EscapeKey = EscapeKeyId(KeyName, KeyChar);
                            }
                        }
                        break;
                    case 1:
                        {
                            if (EscapeKeyId(KeyName, KeyChar).Equals(EscapeKey))
                            {
                                WorkState = 2;
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
                                        if (Core_.__AnsiNewLineKey)
                                        {
                                            Send(TerminalKeys["Enter1"]);
                                        }
                                        else
                                        {
                                            Send(TerminalKeys["Enter0"]);
                                        }
                                        if (LocalEcho)
                                        {
                                            Monitor.Enter(LocalEchoBuf);
                                            LocalEchoBuf.Add(13);
                                            LocalEchoBuf.Add(10);
                                            Monitor.Exit(LocalEchoBuf);
                                        }
                                        break;


                                    case "F1": Send(TerminalKeys["F1_" + GetTelnetKeyboardConf(1)]); break;
                                    case "F2": Send(TerminalKeys["F2_" + GetTelnetKeyboardConf(1)]); break;
                                    case "F3": Send(TerminalKeys["F3_" + GetTelnetKeyboardConf(1)]); break;
                                    case "F4": Send(TerminalKeys["F4_" + GetTelnetKeyboardConf(1)]); break;
                                    case "F5": Send(TerminalKeys["F5_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F6": Send(TerminalKeys["F6_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F7": Send(TerminalKeys["F7_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F8": Send(TerminalKeys["F8_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F9": Send(TerminalKeys["F9_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F10": Send(TerminalKeys["F10_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F11": Send(TerminalKeys["F11_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F12": Send(TerminalKeys["F12_" + GetTelnetKeyboardConf(2)]); break;

                                    case "Backspace":
                                        Send(TerminalKeys["Backspace"]);
                                        if (LocalEcho)
                                        {
                                            Monitor.Enter(LocalEchoBuf);
                                            LocalEchoBuf.Add(8);
                                            Monitor.Exit(LocalEchoBuf);
                                        }
                                        break;

                                    default:
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
                                        break;
                                }
                            }
                        }
                        break;
                    case 2:
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
                    case 4:
                        EscapeKey = EscapeKeyId(KeyName, KeyChar);
                        WorkState = 2;
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
                            WorkState = 4;
                        }
                        break;
                    case "Escape":
                        {
                            WorkState = 1;
                        }
                        break;

                    case "D1": WorkState = 1; if (!TelnetFuncKeyOther) { Send(TerminalKeys["F1_" + GetTelnetKeyboardConf(1)]); } else { Send("00"); } break;
                    case "D2": WorkState = 1; if (!TelnetFuncKeyOther) { Send(TerminalKeys["F2_" + GetTelnetKeyboardConf(1)]); } else { Send("1B"); } break;
                    case "D3": WorkState = 1; if (!TelnetFuncKeyOther) { Send(TerminalKeys["F3_" + GetTelnetKeyboardConf(1)]); } else { Send("1C"); } break;
                    case "D4": WorkState = 1; if (!TelnetFuncKeyOther) { Send(TerminalKeys["F4_" + GetTelnetKeyboardConf(1)]); } else { Send("1D"); } break;
                    case "D5": WorkState = 1; if (!TelnetFuncKeyOther) { Send(TerminalKeys["F5_" + GetTelnetKeyboardConf(2)]); } else { Send("1E"); } break;
                    case "D6": WorkState = 1; if (!TelnetFuncKeyOther) { Send(TerminalKeys["F6_" + GetTelnetKeyboardConf(2)]); } else { Send("1F"); } break;
                    case "D7": WorkState = 1; if (!TelnetFuncKeyOther) { Send(TerminalKeys["F7_" + GetTelnetKeyboardConf(2)]); } else { } break;
                    case "D8": WorkState = 1; if (!TelnetFuncKeyOther) { Send(TerminalKeys["F8_" + GetTelnetKeyboardConf(2)]); } else { } break;
                    case "D9": WorkState = 1; if (!TelnetFuncKeyOther) { Send(TerminalKeys["F9_" + GetTelnetKeyboardConf(2)]); } else { } break;
                    case "D0": WorkState = 1; if (!TelnetFuncKeyOther) { Send(TerminalKeys["F10_" + GetTelnetKeyboardConf(2)]); } else { } break;

                    case "Space": TelnetFuncKeyOther = !TelnetFuncKeyOther; break;
                    case "A": WorkState = 1; Send("01"); break;
                    case "B": WorkState = 1; Send("02"); break;
                    case "C": WorkState = 1; Send("03"); break;
                    case "D": WorkState = 1; Send("04"); break;
                    case "E": WorkState = 1; Send("05"); break;
                    case "F": WorkState = 1; Send("06"); break;
                    case "G": WorkState = 1; Send("07"); break;
                    case "H": WorkState = 1; Send("08"); break;
                    case "I": WorkState = 1; Send("09"); break;
                    case "J": WorkState = 1; Send("0A"); break;
                    case "K": WorkState = 1; Send("0B"); break;
                    case "L": WorkState = 1; Send("0C"); break;
                    case "M": WorkState = 1; Send("0D"); break;
                    case "N": WorkState = 1; Send("0E"); break;
                    case "O": WorkState = 1; Send("0F"); break;
                    case "P": WorkState = 1; Send("10"); break;
                    case "Q": WorkState = 1; Send("11"); break;
                    case "R": WorkState = 1; Send("12"); break;
                    case "S": WorkState = 1; Send("13"); break;
                    case "T": WorkState = 1; Send("14"); break;
                    case "U": WorkState = 1; Send("15"); break;
                    case "V": WorkState = 1; Send("16"); break;
                    case "W": WorkState = 1; Send("17"); break;
                    case "X": WorkState = 1; Send("18"); break;
                    case "Y": WorkState = 1; Send("19"); break;
                    case "Z": WorkState = 1; Send("1A"); break;

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
                                WorkState = 1;
                                Send(TerminalKeys["F11_" + GetTelnetKeyboardConf(2)]);
                                break;
                            case ']':
                            case '}':
                                WorkState = 1;
                                Send(TerminalKeys["F12_" + GetTelnetKeyboardConf(2)]);
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
                                    WorkState = 1;
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
                Buf_ += Conn.H(Buf[i]);
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
                case "TELNET":
                    DefaultPort = 23;
                    Conn = new UniConnTelnet(TerminalEncoding);
                    break;
                case "SSH":
                    DefaultPort = 22;
                    Conn = new UniConnSSH(TerminalEncoding);
                    break;
                case "APPLICATION0":
                case "APPLICATION1":
                case "APPLICATION2":
                case "APPLICATION3":
                    AddrPort = new string[2];
                    AddrPort[0] = Core_.CurrentFileName;
                    AddrPort[1] = TerminalConnection.Substring(11);
                    Conn = new UniConnApp(TerminalEncoding);
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
            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
            Monitor.Enter(TelnetMutex);
            Core_.AnsiProcessReset(false);
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
            WorkState = 0;
            EscapeKey = "";
            while (Screen_.AppWorking && ("".Equals(EscapeKey)))
            {
                Thread.Sleep(100);
            }
            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
            Screen_.SetCursorPosition(0, 0);
            WorkState = 1;







            FileCtX = new List<int>();
            TelnetOpen();
            int WorkState_ = 1;
            int TelnetInfoPos_ = -1;
            string TelnetKeyboardConf_ = "";
            int TelnetKeyboardConfI_ = -1;
            bool TelnetFuncKeyOther_ = false;
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
                        if (WorkState == 2)
                        {
                            WorkState_ = 20;
                        }
                        ConnStatusX = Conn.IsConnected();
                    }
                    Receive(false);
                    ProcessBuf(TelnetTimer);
                    bool WasAnsiProcess = false;

                    Core_.AnsiProcessSupply(FileCtX);
                    FileCtX.Clear();

                    Monitor.Enter(TelnetMutex);
                    WasAnsiProcess = (Core_.AnsiProcess(TerminalStep) != 0);
                    Monitor.Exit(TelnetMutex);

                    if (WasAnsiProcess)
                    {
                        for (int i = 0; i < Core_.__AnsiResponse.Count; i++)
                        {
                            TelnetReport(Core_.__AnsiResponse[i]);
                        }
                        Core_.__AnsiResponse.Clear();
                        if (Core_.AnsiGetFontSize(Core_.__AnsiY) > 0)
                        {
                            Screen_.SetCursorPosition(Core_.__AnsiX * 2, Core_.__AnsiY);
                        }
                        else
                        {
                            Screen_.SetCursorPosition(Core_.__AnsiX, Core_.__AnsiY);
                        }
                        if (WorkState == 2)
                        {
                            WorkState_ = 20;
                        }
                    }
                    if (OpenCloseRepaint)
                    {
                        if (WorkState == 2)
                        {
                            WorkState_ = 20;
                        }
                    }
                    if (((WorkState == 2) || (WorkState == 4)) && ((WorkState_ != WorkState) || (TelnetInfoPos_ != TelnetInfoPos) || (TelnetKeyboardConfI_ != TelnetKeyboardConfI) || (TelnetKeyboardConf_ != TelnetKeyboardConf) || (TelnetFuncKeyOther_ != TelnetFuncKeyOther) || (LocalEcho_ != LocalEcho)))
                    {
                        bool NeedRepaint = (TelnetInfoPos_ != TelnetInfoPos);
                        NeedRepaint = NeedRepaint | (TelnetKeyboardConfI_ != TelnetKeyboardConfI);
                        NeedRepaint = NeedRepaint | (TelnetKeyboardConf_ != TelnetKeyboardConf);
                        NeedRepaint = NeedRepaint | (LocalEcho_ != LocalEcho);
                        NeedRepaint = NeedRepaint | (WorkState_ != 20);
                        NeedRepaint = NeedRepaint | OpenCloseRepaint;
                        OpenCloseRepaint = false;
                        TelnetDisplayInfo(NeedRepaint);
                        WorkState_ = WorkState;
                        TelnetInfoPos_ = TelnetInfoPos;
                        TelnetKeyboardConfI_ = TelnetKeyboardConfI;
                        TelnetKeyboardConf_ = TelnetKeyboardConf;
                        TelnetFuncKeyOther_ = TelnetFuncKeyOther;
                        LocalEcho_ = LocalEcho;
                    }
                    if ((WorkState == 1) && (WorkState_ != WorkState))
                    {
                        Core_.AnsiRepaint(false);
                        if (Core_.AnsiGetFontSize(Core_.__AnsiY) > 0)
                        {
                            Screen_.SetCursorPosition(Core_.__AnsiX * 2, Core_.__AnsiY);
                        }
                        else
                        {
                            Screen_.SetCursorPosition(Core_.__AnsiX, Core_.__AnsiY);
                        }
                        WorkState_ = WorkState;
                    }
                    FuncKeyProcess();
                }

                TelnetCloseFile();
            }
            finally
            {
                TelnetCloseFile();
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
            if (WorkState == 2)
            {
                string StatusInfo = "Unknown";
                switch (IsConn)
                {
                    case 0: StatusInfo = "Disconnected"; break;
                    case 1: StatusInfo = "Connected"; break;
                    case 2: StatusInfo = "Connecting"; break;
                }
                InfoMsg.Add(" Status: " + StatusInfo + " ");
                InfoMsg.Add(" Screen size: " + Core_.AnsiMaxX + "x" + Core_.AnsiMaxY + " ");
                InfoMsg.Add(" Escape key: " + EscapeKey + " ");
                InfoMsg.Add(" Esc - Return to terminal ");
                InfoMsg.Add(" Enter - Change escape key ");
                InfoMsg.Add(" Tab - Move info ");
                InfoMsg.Add(" Backspace - Quit ");
                if (TelnetFuncKeyOther)
                {
                    InfoMsg.Add(" 1-6 - Send NUL,ESC,FS,GS,RS,US ");
                }
                else
                {
                    InfoMsg.Add(" 1-0 - Send F1-F10 ");
                }
                InfoMsg.Add(" Space - Send other control code ");
                InfoMsg.Add(" [, ] - Send F11, F12 ");
                InfoMsg.Add(" Letter - Send CTRL+letter ");
                InfoMsg.Add(" / - Connect/disconnect ");
                InfoMsg.Add(" \\ - Send screen size ");
                string TelnetKeyboardConf_ = "";
                switch (TelnetKeyboardConfI)
                {
                    case 0: TelnetKeyboardConf_ = "[" + TelnetKeyboardConf[0] + "]" + TelnetKeyboardConf[1] + " " + TelnetKeyboardConf[2] + " " + TelnetKeyboardConf[3] + " "; break;
                    case 1: TelnetKeyboardConf_ = " " + TelnetKeyboardConf[0] + "[" + TelnetKeyboardConf[1] + "]" + TelnetKeyboardConf[2] + " " + TelnetKeyboardConf[3] + " "; break;
                    case 2: TelnetKeyboardConf_ = " " + TelnetKeyboardConf[0] + " " + TelnetKeyboardConf[1] + "[" + TelnetKeyboardConf[2] + "]" + TelnetKeyboardConf[3] + " "; break;
                    case 3: TelnetKeyboardConf_ = " " + TelnetKeyboardConf[0] + " " + TelnetKeyboardConf[1] + " " + TelnetKeyboardConf[2] + "[" + TelnetKeyboardConf[3] + "]"; break;
                }
                InfoMsg.Add(" +=, -* - Key codes: " + TelnetKeyboardConf_ + " ");
                InfoMsg.Add(" `~ - Local echo: " + (LocalEcho ? "on" : "off"));
            }
            if (WorkState == 4)
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
            }
        }
    }
}
