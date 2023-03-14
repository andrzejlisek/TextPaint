using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TextPaint
{
    public class UniConnRaw : UniConn
    {
        public UniConnRaw(Encoding TerminalEncoding_) : base(TerminalEncoding_)
        {
        }

        TcpClient TCPC;
        NetworkStream NSX;

        public override void Open(string Addr, int Port, string TerminalName_, int TerminalW, int TerminalH)
        {
            if (IsConnOpening)
            {
                return;
            }

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
                    ms.Write(data, 0, numBytesRead);
                }
                catch
                {
                    DataAvailable = false;
                }
            }
        }
    }
}
