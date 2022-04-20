﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TextPaint
{
    public partial class Telnet
    {

        public void CoreEvent_Client(string KeyName, char KeyChar, int KeyCharI)
        {
            if (KeyName == "WindowClose")
            {
                TelnetClose(true);
            }
            else
            {

                switch (WorkState)
                {
                    case 0:
                        {
                            if (KeyName != "")
                            {
                                EscapeKey = KeyName;
                            }
                        }
                        break;
                    case 1:
                        {
                            if (KeyName == EscapeKey)
                            {
                                WorkState = 2;
                            }
                            else
                            {
                                if (Core_.__AnsiTestCmd)
                                {
                                    Console.WriteLine("####################");
                                }
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
                                else
                                {
                                    switch (KeyName)
                                    {
                                        case "UpArrow":
                                            Send(TerminalKeys["Up"]);
                                            break;
                                        case "DownArrow":
                                            Send(TerminalKeys["Down"]);
                                            break;
                                        case "LeftArrow":
                                            Send(TerminalKeys["Left"]);
                                            break;
                                        case "RightArrow":
                                            Send(TerminalKeys["Right"]);
                                            break;
                                        case "Home":
                                            Send(TerminalKeys["Home"]);
                                            break;
                                        case "End":
                                            Send(TerminalKeys["End"]);
                                            break;
                                        case "PageUp":
                                            Send(TerminalKeys["PageUp"]);
                                            break;
                                        case "PageDown":
                                            Send(TerminalKeys["PageDown"]);
                                            break;

                                        case "Escape": Send(TerminalKeys["Escape"]); break;
                                        case "Insert": Send(TerminalKeys["Insert"]); break;
                                        case "Delete": Send(TerminalKeys["Delete"]); break;

                                        case "Enter":
                                            Send(TerminalKeys["Enter"]);
                                            if (LocalEcho)
                                            {
                                                Monitor.Enter(LocalEchoBuf);
                                                LocalEchoBuf.Add(13);
                                                LocalEchoBuf.Add(10);
                                                Monitor.Exit(LocalEchoBuf);
                                            }
                                            break;


                                        case "F1": Send(TerminalKeys["F1"]); break;
                                        case "F2": Send(TerminalKeys["F2"]); break;
                                        case "F3": Send(TerminalKeys["F3"]); break;
                                        case "F4": Send(TerminalKeys["F4"]); break;
                                        case "F5": Send(TerminalKeys["F5"]); break;
                                        case "F6": Send(TerminalKeys["F6"]); break;
                                        case "F7": Send(TerminalKeys["F7"]); break;
                                        case "F8": Send(TerminalKeys["F8"]); break;
                                        case "F9": Send(TerminalKeys["F9"]); break;
                                        case "F10": Send(TerminalKeys["F10"]); break;
                                        case "F11": Send(TerminalKeys["F11"]); break;
                                        case "F12": Send(TerminalKeys["F12"]); break;
                                    }
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
                        EscapeKey = KeyName;
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

                    case "D1": WorkState = 1; Send(TerminalKeys["F1"]); break;
                    case "D2": WorkState = 1; Send(TerminalKeys["F2"]); break;
                    case "D3": WorkState = 1; Send(TerminalKeys["F3"]); break;
                    case "D4": WorkState = 1; Send(TerminalKeys["F4"]); break;
                    case "D5": WorkState = 1; Send(TerminalKeys["F5"]); break;
                    case "D6": WorkState = 1; Send(TerminalKeys["F6"]); break;
                    case "D7": WorkState = 1; Send(TerminalKeys["F7"]); break;
                    case "D8": WorkState = 1; Send(TerminalKeys["F8"]); break;
                    case "D9": WorkState = 1; Send(TerminalKeys["F9"]); break;
                    case "D0": WorkState = 1; Send(TerminalKeys["F10"]); break;
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
                            case '[':
                            case '{':
                                WorkState = 1;
                                Send(TerminalKeys["F11"]);
                                break;
                            case ']':
                            case '}':
                                WorkState = 1;
                                Send(TerminalKeys["F12"]);
                                break;
                            case '/':
                            case '?':
                                if (TcpConnected(TCPC))
                                {
                                    TelnetClose(false);
                                }
                                else
                                {
                                    TelnetOpen();
                                }
                                break;
                            case '\\':
                            case '|':
                                {
                                    WorkState = 1;
                                    string WinSizeMsg = "";
                                    if ((Core_.AnsiMaxX > 0) && (Core_.AnsiMaxY > 0))
                                    {
                                        WinSizeMsg = "FFFA1F" + Core_.AnsiMaxX.ToString("X").PadLeft(4, '0') + Core_.AnsiMaxY.ToString("X").PadLeft(4, '0') + "FFF0";
                                        Send(WinSizeMsg);
                                    }
                                }
                                break;
                        }
                        break;
                }

                Monitor.Enter(FuncKeyBuf);
            }
            Monitor.Exit(FuncKeyBuf);
        }

        public string CommandDesc(string Cmd)
        {
            string CmdDesc = "";

            if (Cmd.Contains("|"))
            {
                string[] Cmd_ = Cmd.Split('|');
                for (int i = 0; i < Cmd_.Length; i++)
                {
                    if (i > 0)
                    {
                        CmdDesc = CmdDesc + "|";
                    }
                    CmdDesc = CmdDesc + Cmd_[i];
                }
                return CmdDesc;
            }

            switch (Cmd.Substring(4, 2))
            {
                case "00": CmdDesc = "Binary Transmission"; break;
                case "01": CmdDesc = "Echo"; break;
                case "02": CmdDesc = "Reconnection"; break;
                case "03": CmdDesc = "Suppress Go Ahead"; break;
                case "04": CmdDesc = "Approx Message Size Negotiation"; break;
                case "05": CmdDesc = "Status"; break;
                case "06": CmdDesc = "Timing Mark"; break;
                case "07": CmdDesc = "Remote Controlled Trans and Echo"; break;
                case "08": CmdDesc = "Output Line Width"; break;
                case "09": CmdDesc = "Output Page Size"; break;
                case "0A": CmdDesc = "Output Carriage-Return Disposition"; break;
                case "0B": CmdDesc = "Output Horizontal Tab Stops"; break;
                case "0C": CmdDesc = "Output Horizontal Tab Disposition"; break;
                case "0D": CmdDesc = "Output Formfeed Disposition"; break;
                case "0E": CmdDesc = "Output Vertical Tabstops"; break;
                case "0F": CmdDesc = "Output Vertical Tab Disposition"; break;
                case "10": CmdDesc = "Output Linefeed Disposition"; break;
                case "11": CmdDesc = "Extended ASCII"; break;
                case "12": CmdDesc = "Logout"; break;
                case "13": CmdDesc = "Byte Macro"; break;
                case "14": CmdDesc = "Data Entry Terminal"; break;
                case "15": CmdDesc = "SUPDUP"; break;
                case "16": CmdDesc = "SUPDUP Output"; break;
                case "17": CmdDesc = "Send Location"; break;
                case "18": CmdDesc = "Terminal Type"; break;
                case "19": CmdDesc = "End of Record"; break;
                case "1A": CmdDesc = "TACACS User Identification"; break;
                case "1B": CmdDesc = "Output Marking"; break;
                case "1C": CmdDesc = "Terminal Location Number"; break;
                case "1D": CmdDesc = "Telnet 3270 Regime"; break;
                case "1E": CmdDesc = "X.3 PAD"; break;
                case "1F": CmdDesc = "Negotiate About Window Size"; break;
                case "20": CmdDesc = "Terminal Speed"; break;
                case "21": CmdDesc = "Remote Flow Control"; break;
                case "22": CmdDesc = "Linemode"; break;
                case "23": CmdDesc = "X Display Location"; break;
                case "24": CmdDesc = "Environment Variables"; break;
                case "27": CmdDesc = "Telnet Environment Option"; break;
                default:
                    {
                        CmdDesc = "UNKNOWN " + Cmd.Substring(4, 2);
                    }
                    break;
            }
            switch (Cmd.Substring(2, 2))
            {
                case "FB": CmdDesc = "WILL " + CmdDesc; break;
                case "FC": CmdDesc = "WON'T " + CmdDesc; break;
                case "FD": CmdDesc = "DO " + CmdDesc; break;
                case "FE": CmdDesc = "DON'T " + CmdDesc; break;
                case "FA": CmdDesc = "SUB " + CmdDesc + " " + Cmd.Substring(6, Cmd.Length - 10); break;
                default:
                    {
                        CmdDesc = "UNKNOWN " + Cmd.Substring(2, 2);
                    }
                    break;
            }
            return CmdDesc + " (" + Cmd + ")";
        }

        public byte[] Receive(bool Str)
        {
            Monitor.Enter(TCPCMon);
            if (NSX == null)
            {
                Monitor.Exit(TCPCMon);
                return new byte[0];
            }
            byte[] data = new byte[20];
            using (MemoryStream ms = new MemoryStream())
            {
                bool DataAvailable = false;
                try
                {
                    DataAvailable = NSX.DataAvailable;
                }
                catch (ObjectDisposedException ex)
                {
                    DataAvailable = false;
                }
                while (DataAvailable)
                {
                    int numBytesRead = 0;
                    try
                    {
                        numBytesRead = NSX.Read(data, 0, data.Length);
                        DataAvailable = NSX.DataAvailable;
                    }
                    catch (ObjectDisposedException ex)
                    {
                        DataAvailable = false;
                    }
                    ms.Write(data, 0, numBytesRead);
                }
                if (LocalEchoBuf.Count > 0)
                {
                    Monitor.Enter(LocalEchoBuf);
                    ms.Write(LocalEchoBuf.ToArray(), 0, LocalEchoBuf.ToArray().Length);
                    LocalEchoBuf.Clear();
                    Monitor.Exit(LocalEchoBuf);
                }
                byte[] SS = ms.ToArray();
                if (SS.Length > 0)
                {
                    if (ConsoleTestI) Console.Write("> ");
                }
                if (Str)
                {
                    for (int i = 0; i < SS.Length; i++)
                    {
                        TelnetBuf.Add(SS[i]);
                        if (SS[i] < 32) { SS[i] = (byte)'_'; }
                        if (SS[i] > 126) { SS[i] = (byte)'_'; }
                    }
                    if (ConsoleTestI) Console.Write(Encoding.UTF8.GetString(SS));
                }
                else
                {
                    for (int i = 0; i < SS.Length; i++)
                    {
                        TelnetBuf.Add(SS[i]);
                        if (ConsoleTestI) Console.Write(((int)SS[i]).ToString("X").PadLeft(2, '0'));
                    }
                }
                if (SS.Length > 0)
                {
                    if (ConsoleTestI) Console.WriteLine();
                }
                Monitor.Exit(TCPCMon);
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
                Buf_ += H(Buf[i]);
            }
            Send(Buf_);
        }

        public void Send(string STR)
        {
            if (!TcpConnected(TCPC))
            {
                return;
            }
            Monitor.Enter(TCPCMon);
            if (STR == "")
            {
                return;
            }
            if (STR[0] == '-')
            {
                if (ConsoleTestO) Console.WriteLine("< " + STR.Substring(1));
                byte[] Raw = Encoding.UTF8.GetBytes(STR.Substring(1));
                NSX.Write(Raw, 0, Raw.Length);
            }
            else
            {
                if (ConsoleTestO) Console.WriteLine("< " + STR);
                byte[] Raw = new byte[STR.Length / 2];
                for (int i = 0; i < Raw.Length; i++)
                {
                    Raw[i] = (byte)int.Parse(STR.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                }
                NSX.Write(Raw, 0, Raw.Length);
            }
            Monitor.Exit(TCPCMon);
        }

        // 0 - Normal
        // 1 - InsideCommand
        int ProcessState = 0;

        string TelnetCommand = "";

        public void ProcessBuf()
        {
            while (TelnetBuf.Count > 0)
            {
                byte Chr = TelnetBuf[0];
                TelnetBuf.RemoveAt(0);
                switch (ProcessState)
                {
                    case 0:
                        {
                            if (Chr == 255)
                            {
                                ProcessState = 1;
                                TelnetCommand = "FF";
                            }
                            else
                            {
                                ByteStr.Add(Chr);
                            }
                        }
                        break;
                    case 1:
                        {
                            TelnetCommand = TelnetCommand + H(Chr);

                            bool StdCmd = true;

                            if (TelnetCommand == "FFFF")
                            {
                                ByteStr.Add(255);
                                ProcessState = 0;
                                StdCmd = false;
                            }
                            if (TelnetCommand == "FFF9")
                            {
                                ProcessState = 0;
                                StdCmd = false;
                            }
                            if (StdCmd)
                            {
                                string CommandAnswer = "";
                                if (TelnetCommand.Length >= 6)
                                {
                                    for (int i = 0; i < NegoAnswers.Count; i++)
                                    {
                                        if (TelnetCommand.Length >= NegoAnswers[i][0].Length)
                                        {
                                            if (TelnetCommand.StartsWith(NegoAnswers[i][0]))
                                            {
                                                CommandAnswer = NegoAnswers[i][1];
                                                break;
                                            }
                                        }
                                    }

                                    if (CommandAnswer == "")
                                    {
                                        for (int i = 0; i < NegoAnswers.Count; i++)
                                        {
                                            if (TelnetCommand.Length >= (NegoAnswers[i][0].Replace("??", "").Length))
                                            {
                                                if (TelnetCommand.StartsWith(NegoAnswers[i][0].Replace("??", "")) && NegoAnswers[i][0].EndsWith("??"))
                                                {
                                                    CommandAnswer = NegoAnswers[i][1] + TelnetCommand.Substring(4);
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (CommandAnswer != "")
                                    {
                                        if (CommandTest) Console.WriteLine(CommandDesc(TelnetCommand) + " -> " + CommandDesc(CommandAnswer));
                                        Send(CommandAnswer.Replace("|", ""));
                                        ProcessState = 0;

                                        switch (TelnetCommand + "|" + CommandAnswer)
                                        {
                                            case "FFFB01|FFFD01":
                                                LocalEcho = false;
                                                if (CommandTest) { Console.WriteLine("LocalEcho = false"); }
                                                break;
                                            case "FFFC01|FFFE01":
                                                LocalEcho = true;
                                                if (CommandTest) { Console.WriteLine("LocalEcho = true"); }
                                                break;
                                        }

                                    }
                                    else
                                    {
                                        bool NeedAnswer = false;
                                        switch (TelnetCommand.Substring(2, 2))
                                        {
                                            case "FB":
                                            case "FC":
                                            case "FD":
                                            case "FE":
                                                if (TelnetCommand.Length == 6)
                                                {
                                                    NeedAnswer = true;
                                                    ProcessState = 0;
                                                }
                                                break;
                                            case "FF":
                                                ProcessState = 0;
                                                break;
                                            case "FA":
                                                if (TelnetCommand.EndsWith("FFF0"))
                                                {
                                                    NeedAnswer = true;
                                                    ProcessState = 0;
                                                }
                                                break;
                                        }
                                        if (NeedAnswer)
                                        {
                                            if (CommandTest) Console.WriteLine("Command without answer " + CommandDesc(TelnetCommand));
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }


            // Avoid cutted UTF-8 stream
            if (true)
            {
                //for (int i = 0; i < max; i++)
                {

                }
            }

            int ToSend = ByteStr.Count;

            FileCtX.AddRange(TextWork.StrToInt(TerminalEncoding.GetString(ByteStr.ToArray())));

            ByteStr.RemoveRange(0, ToSend);

        }

        string TCPCMon = "X";

        public bool TcpConnected(TcpClient tcpClient)
        {
            Monitor.Enter(TCPCMon);
            if (tcpClient == null)
            {
                Monitor.Exit(TCPCMon);
                return false;
            }
            try
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

                TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

                foreach (TcpConnectionInformation c in tcpConnections)
                {
                    TcpState stateOfConnection = c.State;

                    if (c.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint) && c.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint))
                    {
                        if (stateOfConnection == TcpState.Established)
                        {
                            Monitor.Exit(TCPCMon);
                            return true;
                        }
                        else
                        {
                            tcpClient = null;
                            Monitor.Exit(TCPCMon);
                            return false;
                        }
                    }
                }
            }
            catch (ObjectDisposedException e)
            {
                tcpClient = null;
            }

            Monitor.Exit(TCPCMon);
            return false;
        }

        string ConError = "";

        bool OpenCloseRepaint = false;

        public void TelnetOpen()
        {
            TCPC = new TcpClient();
            string[] AddrPort = Core_.CurrentFileName.Split(':');
            try
            {
                if (AddrPort.Length == 2)
                {
                    TCPC.Connect(AddrPort[0], int.Parse(AddrPort[1]));
                }
                else
                {
                    TCPC.Connect(AddrPort[0], 23);
                }
                NSX = TCPC.GetStream();
                ConError = "";
            }
            catch (Exception e)
            {
                ConError = e.Message;
                TCPC = null;
                NSX = null;
            }
            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
            Core_.AnsiProcessReset(false);
            OpenCloseRepaint = true;
        }

        public void TelnetClose(bool StopApp)
        {
            if (TcpConnected(TCPC))
            {
                TCPC.Close();
            }
            if (StopApp)
            {
                Screen_.CloseApp(Core_.TextNormalBack, Core_.TextNormalFore);
            }
            else
            {
                OpenCloseRepaint = true;
            }
        }

        public void TelnetClientWork()
        {
            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
            Screen_.WriteText("Press key, which will be used as escape key...", Core_.TextNormalBack, Core_.TextNormalFore);
            Screen_.Refresh();
            WorkState = 0;
            EscapeKey = "";
            while (Screen_.AppWorking && (EscapeKey == ""))
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
            bool ConnStatusX = TcpConnected(TCPC);
            while (Screen_.AppWorking)
            {
                Thread.Sleep(100);
                FileCtX.Clear();
                if (ConnStatusX != TcpConnected(TCPC))
                {
                    if (WorkState == 2)
                    {
                        WorkState_ = 20;
                    }
                    ConnStatusX = TcpConnected(TCPC);
                }
                Receive(false);
                ProcessBuf();
                if (Core_.AnsiProcess(FileCtX))
                {
                    Screen_.SetCursorPosition(Core_.__AnsiX, Core_.__AnsiY);
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
                if (((WorkState == 2) || (WorkState == 4)) && ((WorkState_ != WorkState) || (TelnetInfoPos_ != TelnetInfoPos)))
                {
                    bool NeedRepaint = (TelnetInfoPos_ != TelnetInfoPos);
                    NeedRepaint = NeedRepaint | (WorkState_ != 20);
                    NeedRepaint = NeedRepaint | OpenCloseRepaint;
                    OpenCloseRepaint = false;
                    TelnetDisplayInfo(NeedRepaint);
                    WorkState_ = WorkState;
                    TelnetInfoPos_ = TelnetInfoPos;
                }
                if ((WorkState == 1) && (WorkState_ != WorkState))
                {
                    Core_.AnsiRepaint();
                    Screen_.SetCursorPosition(Core_.__AnsiX, Core_.__AnsiY);
                    WorkState_ = WorkState;
                }
                FuncKeyProcess();
            }
            Screen_.CloseApp(Core_.TextNormalBack, Core_.TextNormalFore);
        }

        void TelnetDisplayInfo(bool NeedRepaint)
        {
            if (NeedRepaint)
            {
                Core_.AnsiRepaint();
            }
            int CB_ = Core_.PopupBack;
            int CF_ = Core_.PopupFore;

            List<string> InfoMsg = new List<string>();
            bool IsConn = TcpConnected(TCPC);
            if (WorkState == 2)
            {
                string StatusInfo = IsConn ? "Connected" : "Disconnected";
                InfoMsg.Add(" Status: " + StatusInfo + " ");
                InfoMsg.Add(" Screen size: " + Core_.AnsiMaxX + "x" + Core_.AnsiMaxY + " ");
                InfoMsg.Add(" Escape key: " + EscapeKey + " ");
                InfoMsg.Add(" Esc - Return to terminal ");
                InfoMsg.Add(" Enter - Change escape key ");
                InfoMsg.Add(" Tab - Move info ");
                InfoMsg.Add(" Backspace - Quit ");
                InfoMsg.Add(" 1-0 - Send F1-F10 ");
                InfoMsg.Add(" [, ] - Send F11, F12 ");
                InfoMsg.Add(" Letter - Send CTRL+letter ");
                InfoMsg.Add(" / - Connect/disconnect ");
                InfoMsg.Add(" \\ - Send screen size ");
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

        int TelnetInfoPos = 0;
    }
}
