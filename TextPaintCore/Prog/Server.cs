using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace TextPaint
{
    public class Server
    {
        bool ServerWorks = false;
        int ListenPort = 0;

        TcpListener TcpListener_ = null;
        List<Socket> Socket_ = new List<Socket>();
        object Mutex = new object();

        bool TelnetMode = false;
        List<int> TelnetProcessState = new List<int>();
        List<string> TelnetCommand = new List<string>();

        void NewConn()
        {
            while (ServerWorks)
            {
                try
                {
                    Socket_.Add(TcpListener_.AcceptSocket());
                    TelnetProcessState.Add(0);
                    TelnetCommand.Add("");
                }
                catch
                {
                    break;
                }
            }
        }

        public bool Start(int ListenPort_, bool TelnetMode_)
        {
            Monitor.Enter(Mutex);
            TelnetMode = TelnetMode_;
            if (ServerWorks)
            {
                Monitor.Exit(Mutex);
                return true;
            }

            ListenPort = ListenPort_;
            ServerWorks = true;

            TcpListener_ = new TcpListener(ListenPort);
            try
            {
                TcpListener_.Start();

                Thread Thr = new Thread(NewConn);
                Thr.Start();
                Monitor.Exit(Mutex);
                return true;
            }
            catch
            {
                ServerWorks = false;
                Monitor.Exit(Mutex);
                return false;
            }
        }

        public void Stop()
        {
            Monitor.Enter(Mutex);
            if (!ServerWorks)
            {
                Monitor.Exit(Mutex);
                return;
            }

            TcpListener_.Stop();
            for (int i = 0; i < Socket_.Count; i++)
            {
                if (Socket_[i] != null)
                {
                    try
                    {
                        Socket_[i].Close();
                    }
                    catch
                    {

                    }
                }
            }

            ServerWorks = false;
            Monitor.Exit(Mutex);
        }

        public void Send(byte[] Msg)
        {
            Monitor.Enter(Mutex);
            if (TelnetMode)
            {
                List<byte> MsgL = new List<byte>();
                MsgL.AddRange(Msg);
                for (int i = (MsgL.Count - 1); i >= 0; i--)
                {
                    if (MsgL[i] == 255)
                    {
                        MsgL.Insert(i, 255);
                    }
                }
                Msg = MsgL.ToArray();
            }
            if (ServerWorks)
            {
                for (int i = 0; i < Socket_.Count; i++)
                {
                    if (Socket_[i] != null)
                    {
                        try
                        {
                            Socket_[i].Send(Msg);
                        }
                        catch
                        {
                            try
                            {
                                Socket_[i].Close();
                            }
                            catch
                            {

                            }
                            Socket_[i] = null;
                        }
                    }
                }
            }
            Monitor.Exit(Mutex);
        }

        public byte[] Receive()
        {
            Monitor.Enter(Mutex);
            List<byte> Raw = new List<byte>();
            byte[] RawBuf = new byte[1024];
            if (ServerWorks)
            {
                for (int i = 0; i < Socket_.Count; i++)
                {
                    if (Socket_[i] != null)
                    {
                        try
                        {
                            while (Socket_[i].Available > 0)
                            {
                                int N = Socket_[i].Receive(RawBuf);
                                if (TelnetMode)
                                {
                                    byte[] RawBufT = TelnetReceive(i, RawBuf, N);
                                    N = RawBufT.Length;
                                    for (int ii = 0; ii < N; ii++)
                                    {
                                        Raw.Add(RawBufT[ii]);
                                    }
                                }
                                else
                                {
                                    for (int ii = 0; ii < N; ii++)
                                    {
                                        Raw.Add(RawBuf[ii]);
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            Monitor.Exit(Mutex);
            return Raw.ToArray();
        }

        byte[] TelnetReceive(int Idx, byte[] data, int RawN)
        {
            List<byte> ProcessedData = new List<byte>();

            for (int DataI = 0; DataI < RawN; DataI++)
            {
                byte Chr = data[DataI];
                switch (TelnetProcessState[Idx])
                {
                    case 0:
                        {
                            if (Chr == 255)
                            {
                                TelnetProcessState[Idx] = 1;
                                TelnetCommand[Idx] = "FF";
                            }
                            else
                            {
                                ProcessedData.Add(Chr);
                            }
                        }
                        break;
                    case 1:
                        {
                            TelnetCommand[Idx] = TelnetCommand[Idx] + UniConn.H(Chr);

                            bool StdCmd = true;

                            if ("FFFF".Equals(TelnetCommand[Idx]))
                            {
                                ProcessedData.Add(255);
                                TelnetProcessState[Idx] = 0;
                                StdCmd = false;
                            }
                            if ("FFF9".Equals(TelnetCommand[Idx]))
                            {
                                TelnetProcessState[Idx] = 0;
                                StdCmd = false;
                            }
                            if (StdCmd)
                            {
                                string CommandAnswer = "";
                                if (TelnetCommand[Idx].Length >= 6)
                                {
                                    if (!("".Equals(CommandAnswer)))
                                    {
                                        TelnetProcessState[Idx] = 0;
                                    }
                                    else
                                    {
                                        bool NeedAnswer = false;
                                        switch (TelnetCommand[Idx].Substring(2, 2))
                                        {
                                            case "FB":
                                            case "FC":
                                            case "FD":
                                            case "FE":
                                                if (TelnetCommand[Idx].Length == 6)
                                                {
                                                    NeedAnswer = true;
                                                    TelnetProcessState[Idx] = 0;
                                                }
                                                break;
                                            case "FF":
                                                TelnetProcessState[Idx] = 0;
                                                break;
                                            case "FA":
                                                if (TelnetCommand[Idx].EndsWith("FFF0"))
                                                {
                                                    NeedAnswer = true;
                                                    TelnetProcessState[Idx] = 0;
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            return ProcessedData.ToArray();
        }
    }
}
