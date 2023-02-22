using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Renci.SshNet;

namespace TextPaint
{
    public class UniConnSSH : UniConn
    {
        SshClient Serv;
        ShellStream SSX;
        int SizeW = -1;
        int SizeH = -1;
        int ConnLogin = 0;
        List<byte> LoginBuf = new List<byte>();
        string LoginUser = "";
        string LoginPass = "";
        string ServerAddr = "";
        int ServerPort = 0;

        public UniConnSSH(Encoding TerminalEncoding_) : base(TerminalEncoding_)
        {

        }

        public override void Send(byte[] Raw)
        {
            if (IsConnected() == 0)
            {
                return;
            }

            if ((ConnLogin > 0) && (ConnLogin < 3))
            {
                for (int i = 0; i < Raw.Length; i++)
                {
                    if (Raw[i] == 13)
                    {
                        switch (ConnLogin)
                        {
                            case 1:
                                LoginUser = TerminalEncoding.GetString(LoginBuf.ToArray());
                                LoginBuf.Clear();
                                ConnLogin = 2;

                                ScreenNewLine();
                                LoopSend("password: ");

                                break;
                            case 2:
                                LoginPass = TerminalEncoding.GetString(LoginBuf.ToArray());
                                LoginBuf.Clear();
                                ConnLogin = 3;
                                ScreenClear();
                                Thread Thr = new Thread(ServerLogin);
                                Thr.Start();
                                break;
                        }
                    }
                    else
                    {
                        if ((Raw[i] == 8) || (Raw[i] == 127))
                        {
                            //if (LoginBuf.Count > 0)
                            {
                                //DummyShell.Enqueue(8);
                            }
                        }
                        if (Raw[i] >= 32)
                        {
                            LoginBuf.Add(Raw[i]);
                            if (ConnLogin == 1)
                            {
                                LoopSend(Raw[i]);
                            }
                            if (ConnLogin == 2)
                            {
                                LoopSend((byte)'*');
                            }
                        }
                    }
                }
                switch (ConnLogin)
                {
                    case 1:
                        break;
                    case 2:
                        break;
                }
                return;
            }

            if (ConnLogin == 4)
            {
                try
                {
                    SSX.Write(TerminalEncoding.GetString(Raw));
                }
                catch
                {
                    Close();
                }
            }
        }

        public override void Receive(MemoryStream ms)
        {
            LoopReceive(ms);
            if (SSX == null)
            {
                return;
            }
            bool DataAvailable = false;
            try
            {
                DataAvailable = SSX.DataAvailable;
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
                    numBytesRead = SSX.Read(data, 0, data.Length);
                    DataAvailable = SSX.DataAvailable;
                }
                catch
                {
                    DataAvailable = false;
                }
                ms.Write(data, 0, numBytesRead);
            }
        }

        public override void Open(string Addr, int Port, string TerminalName_, int TerminalW, int TerminalH)
        {
            ServerAddr = Addr;
            ServerPort = Port;
            TerminalName = TerminalName_;

            ConnLogin = 0;
            LoginBuf.Clear();
            SSX = null;

            SizeW = TerminalW;
            SizeH = TerminalH;

            ScreenClear();

            LoopSend("login as: ");
            ConnLogin = 1;
        }

        private void ServerLogin()
        {
            try
            {
                Serv = new SshClient(ServerAddr, ServerPort, LoginUser, LoginPass);
                Serv.Connect();
                SSX = Serv.CreateShellStream(TerminalName, (uint)SizeW, (uint)SizeH, (uint)SizeW * 8, (uint)SizeH * 16, SizeH);
                ConnLogin = 4;
            }
            catch (Exception X)
            {
                LoopSend(X.Message);
                ConnLogin = 0;
            }
        }

        public override int IsConnected()
        {
            if (ConnLogin == 0)
            {
                return 0;
            }
            if ((ConnLogin == 1) || (ConnLogin == 2) || (ConnLogin == 3))
            {
                return 2;
            }
            if ((Serv != null) && (Serv.IsConnected) && (SSX != null))
            {
                return 1;
            }
            return 0;
        }

        public override void Close()
        {
            try
            {
                SSX.Close();
            }
            catch
            {

            }
            try
            {
                Serv.Disconnect();
            }
            catch
            {

            }
            ConnLogin = 0;
        }

        public override void Resize(int NewW, int NewH)
        {
            if (ConnLogin < 3)
            {
                SizeW = NewW;
                SizeH = NewH;
                return;
            }

            if (SSX != null)
            {
                SSX.Flush();
                SSX.Close();
            }
            ScreenClear();
            SSX = Serv.CreateShellStream(TerminalName, (uint)NewW, (uint)NewH, (uint)NewW * 8, (uint)NewH * 16, NewH);
            SizeW = NewW;
            SizeH = NewH;
        }
    }
}
