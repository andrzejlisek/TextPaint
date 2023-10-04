using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TextPaint
{
    public class UniConnTelnet : UniConn
    {
        public UniConnTelnet(Encoding TerminalEncoding_) : base(TerminalEncoding_)
        {

        }

        TcpClient TCPC;
        NetworkStream NSX;

        List<string[]> NegoAnswers = new List<string[]>();

        bool TestCommand = false;

        // 0 - Normal
        // 1 - InsideCommand
        int ProcessState = 0;

        string TelnetCommand = "";


        public override void Open(string Addr, int Port, string TerminalName_, int TerminalW, int TerminalH)
        {
            if (IsConnOpening)
            {
                return;
            }

            TerminalName = TerminalName_;

            string TerminalNameHex = "";
            for (int i = 0; i < TerminalName.Length; i++)
            {
                if ((((int)TerminalName[i]) >= 32) && (((int)TerminalName[i]) <= 126))
                {
                    TerminalNameHex = TerminalNameHex + UniConn.H((byte)TerminalName[i]);
                }
            }


            // FA SUB BEGIN
            // F0 SUB SE
            // FB WILL
            // FC WON'T
            // FD DO
            // FE DON'T
            NegoAnswers.Clear();
            NegoAnswers.Add(new string[] { "fffd18", "fffb18" });
            NegoAnswers.Add(new string[] { "fffd03", "fffb03" });
            NegoAnswers.Add(new string[] { "fffa 1801 fff0", "fffa 1800" + TerminalNameHex + " fff0" });
            NegoAnswers.Add(new string[] { "fffa 2701 fff0", "fffa 2700 fff0" });
            NegoAnswers.Add(new string[] { "fffa 2001 fff0", "fffa 2000 33383430302c3338343030 fff0" });
            NegoAnswers.Add(new string[] { "fffb03", "fffd03" });
            NegoAnswers.Add(new string[] { "fffd01", "fffb01" });
            NegoAnswers.Add(new string[] { "fffd1f", "fffb1f" });
            NegoAnswers.Add(new string[] { "fffb01", "fffd01" });
            NegoAnswers.Add(new string[] { "fffe03", "fffc03" });

            NegoAnswers.Add(new string[] { "fffb??", "fffe" });
            NegoAnswers.Add(new string[] { "fffc??", "fffe" });
            NegoAnswers.Add(new string[] { "fffd??", "fffc" });
            NegoAnswers.Add(new string[] { "fffe??", "fffc" });

            for (int i = 0; i < NegoAnswers.Count; i++)
            {
                NegoAnswers[i][0] = NegoAnswers[i][0].ToUpperInvariant().Replace(" ", "");
                NegoAnswers[i][1] = NegoAnswers[i][1].ToUpperInvariant().Replace(" ", "");
            }


            ProcessState = 0;
            TelnetCommand = "";
            TerminalName = TerminalName_;

            IsConnOpening = true;
            OpenWorkAddr = Addr;
            OpenWorkPort = Port;

            Thread Thr = new Thread(OpenWork);
            Thr.Start();
        }

        bool IsConnOpening = false;

        string OpenWorkAddr;
        int OpenWorkPort;
        private void OpenWork()
        {
            try
            {
                TCPC = new TcpClient();
                TCPC.Connect(OpenWorkAddr, OpenWorkPort);
                NSX = TCPC.GetStream();
            }
            catch (Exception E)
            {
                LoopSend(E.Message);
            }
            IsConnOpening = false;
        }

        public override int IsConnected()
        {
            try
            {
                Monitor.Enter(TCPCMon);
                if (IsConnOpening)
                {
                    Monitor.Exit(TCPCMon);
                    return 2;
                }
                if (NSX == null)
                {
                    Monitor.Exit(TCPCMon);
                    return 0;
                }
                if (TCPC == null)
                {
                    Monitor.Exit(TCPCMon);
                    return 0;
                }
                if (TCPC.Client == null)
                {
                    Monitor.Exit(TCPCMon);
                    return 0;
                }
                try
                {
                    IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

                    TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

                    foreach (TcpConnectionInformation c in tcpConnections)
                    {
                        TcpState stateOfConnection = c.State;

                        if (c.LocalEndPoint.Equals(TCPC.Client.LocalEndPoint) && c.RemoteEndPoint.Equals(TCPC.Client.RemoteEndPoint))
                        {
                            if (stateOfConnection == TcpState.Established)
                            {
                                Monitor.Exit(TCPCMon);
                                return 1;
                            }
                            else
                            {
                                TCPC = null;
                                Monitor.Exit(TCPCMon);
                                return 0;
                            }
                        }
                    }
                }
                catch (ObjectDisposedException e)
                {
                    TCPC = null;
                }

                Monitor.Exit(TCPCMon);
            }
            catch
            {

            }
            return 0;
        }

        public override void Close()
        {
            if (!IsConnOpening)
            {
                TCPC.Close();
            }
        }

        public override void Send(byte[] Raw)
        {
            if (!IsConnOpening)
            {
                try
                {
                    NSX.Write(Raw, 0, Raw.Length);
                }
                catch
                {

                }
            }
        }

        public override void Receive(MemoryStream ms)
        {
            LoopReceive(ms);
            if (IsConnOpening)
            {
                return;
            }
            bool DataAvailable = false;
            try
            {
                DataAvailable = NSX.DataAvailable;
            }
            catch
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
                catch
                {
                    DataAvailable = false;
                }


                for (int DataI = 0; DataI < numBytesRead; DataI++)
                {
                    byte Chr = data[DataI];
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
                                    ms.WriteByte(Chr);
                                }
                            }
                            break;
                        case 1:
                            {
                                TelnetCommand = TelnetCommand + H(Chr);

                                bool StdCmd = true;

                                if ("FFFF".Equals(TelnetCommand))
                                {
                                    ms.WriteByte(255);
                                    ProcessState = 0;
                                    StdCmd = false;
                                }
                                if ("FFF9".Equals(TelnetCommand))
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

                                        if ("".Equals(CommandAnswer))
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

                                        if (!("".Equals(CommandAnswer)))
                                        {
                                            if (TestCommand) Console.WriteLine(CommandDesc(TelnetCommand) + " -> " + CommandDesc(CommandAnswer));
                                            SendHex(CommandAnswer.Replace("|", ""));
                                            ProcessState = 0;

                                            //switch (TelnetCommand + "|" + CommandAnswer)
                                            //{
                                            //    case "FFFB01|FFFD01":
                                            //        LocalEcho = false;
                                            //        break;
                                            //    case "FFFC01|FFFE01":
                                            //        LocalEcho = true;
                                            //        break;
                                            //}

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
                                                if (TestCommand) Console.WriteLine("Command without answer " + CommandDesc(TelnetCommand));
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }


        public override void Resize(int NewW, int NewH)
        {
            SendHex("FFFA1F" + NewW.ToString("X").PadLeft(4, '0') + NewH.ToString("X").PadLeft(4, '0') + "FFF0");
        }


        private string CommandDesc(string Cmd)
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
    }
}
