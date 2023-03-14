using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace TextPaint
{
    public class UniConnSerial : UniConn
    {
        public UniConnSerial(Encoding TerminalEncoding_) : base(TerminalEncoding_)
        {
        }

        SerialPort SP = null;
        Thread SendThr = null;

        public override void Send(byte[] Raw)
        {
            if (IsDisconn)
            {
                return;
            }
            if (SP != null)
            {
                if (SP.IsOpen)
                {
                    SendData.Enqueue(Raw);
                    if (SendThr == null)
                    {
                        SendThr = new Thread(ProcSend);
                        SendThr.Start();
                    }
                    else
                    {
                        if (SendThr.ThreadState != ThreadState.Running)
                        {
                            SendThr = new Thread(ProcSend);
                            SendThr.Start();
                        }
                    }
                }
            }
        }

        Queue<byte[]> SendData = new Queue<byte[]>();

        void ProcSend()
        {
            while (SendData.Count > 0)
            {
                byte[] _ = SendData.Peek();
                try
                {
                    SP.Write(_, 0, _.Length);
                    SendData.Dequeue();
                }
                catch
                {

                }
            }
            SendThr = null;
        }

        public override void Receive(MemoryStream ms)
        {
            LoopReceive(ms);
        }

        public override void Open(string Addr, int Port, string TerminalName_, int TerminalW, int TerminalH)
        {
            if (IsDisconn)
            {
                return;
            }

            string[] SerialParam = Addr.Split(':');
            Loop.Clear();
            if (SerialParam.Length == 6)
            {
                SP = new SerialPort();
                SP.PortName = SerialParam[0];
                SP.BaudRate = int.Parse(SerialParam[1]);
                SP.DataBits = int.Parse(SerialParam[2]);
                switch (SerialParam[3])
                {
                    default:
                        SP.Parity = Parity.None;
                        break;
                    case "1":
                        SP.Parity = Parity.Odd;
                        break;
                    case "2":
                        SP.Parity = Parity.Even;
                        break;
                    case "3":
                        SP.Parity = Parity.Mark;
                        break;
                    case "4":
                        SP.Parity = Parity.Space;
                        break;
                }
                switch (SerialParam[4])
                {
                    default:
                        SP.StopBits = StopBits.None;
                        break;
                    case "1":
                        SP.StopBits = StopBits.One;
                        break;
                    case "2":
                        SP.StopBits = StopBits.Two;
                        break;
                    case "3":
                        SP.StopBits = StopBits.OnePointFive;
                        break;
                }
                switch (SerialParam[5])
                {
                    default:
                        SP.Handshake = Handshake.None;
                        break;
                    case "1":
                        SP.Handshake = Handshake.XOnXOff;
                        break;
                    case "2":
                        SP.Handshake = Handshake.RequestToSend;
                        break;
                    case "3":
                        SP.Handshake = Handshake.RequestToSendXOnXOff;
                        break;
                }
                SP.ReadTimeout = 3000;
                SP.WriteTimeout = 3000;
                SP.Open();
                Thread Thr = new Thread(RecvLoop);
                Thr.Start();
            }
            else
            {
                throw new Exception("Serial port address must have 6 elements. For example: COM1:9600:8:0:1:0");
            }
        }

        byte[] SerialBuf = new byte[1000];

        void RecvLoop()
        {
            while (true)
            {
                try
                {
                    int BufL = 0;
                    try
                    {
                        BufL = SP.Read(SerialBuf, 0, SerialBuf.Length);
                    }
                    catch (TimeoutException TE)
                    {

                    }
                    for (int i = 0; i < BufL; i++)
                    {
                        LoopSend(SerialBuf[i]);
                    }
                    if ((IsConnected() != 1) && (BufL == 0))
                    {
                        break;
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        bool IsDisconn = false;

        public override int IsConnected()
        {
            if (IsDisconn)
            {
                return 3;
            }
            if (SP != null)
            {
                if (SP.IsOpen)
                {
                    return 1;
                }
            }
            if (Loop.Count > 0)
            {
                return 1;
            }
            return 0;
        }

        public override void Close()
        {
            if (SP != null)
            {
                IsDisconn = true;
                Thread Thr = new Thread(CloseWork);
                Thr.Start();
            }
        }

        void CloseWork()
        {
            try
            {
                SP.Close();
            }
            catch
            {

            }
            SP = null;
            IsDisconn = false;
        }
    }
}
