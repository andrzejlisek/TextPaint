using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TextPaint
{
    public abstract class UniConn
    {
        protected string TCPCMon = "X";
        protected byte[] data = new byte[20];
        protected string TerminalName = "";

        public static string H(byte B)
        {
            int X = B;
            string XX = X.ToString("X");
            return XX.PadLeft(2, '0');
        }

        public static byte[] HexToRaw(string STR)
        {
            byte[] Raw = new byte[STR.Length / 2];
            for (int i = 0; i < Raw.Length; i++)
            {
                string STR_Byte = STR.Substring(i * 2, 2);
                if (STR_Byte[0] == '_')
                {
                    Raw[i] = (byte)Encoding.UTF8.GetBytes(STR_Byte)[1];
                }
                else
                {
                    Raw[i] = (byte)int.Parse(STR_Byte, System.Globalization.NumberStyles.HexNumber);
                }
            }
            return Raw;
        }

        public void SendHex(string STR)
        {
            MonitorEnter();
            if ("".Equals(STR))
            {
                MonitorExit();
                return;
            }

            if (STR[0] == '-')
            {
                //Console.WriteLine("< " + STR.Substring(1));
                Send(Encoding.UTF8.GetBytes(STR.Substring(1)));
            }
            else
            {
                //Console.WriteLine("< " + STR);
                Send(HexToRaw(STR));
            }


            MonitorExit();
        }

        public virtual void Send(byte[] Raw)
        {
        }

        public virtual void Receive(MemoryStream ms)
        {
        }

        public virtual void Open(string Addr, int Port, string TerminalName_, int TerminalW, int TerminalH)
        {
        }

        public virtual int IsConnected()
        {
            return 0;
        }

        public virtual void Close()
        {
        }


        public virtual void Resize(int NewW, int NewH)
        {

        }


        public void MonitorEnter()
        {
            Monitor.Enter(TCPCMon);
        }

        public void MonitorExit()
        {
            Monitor.Exit(TCPCMon);
        }

        protected Encoding TerminalEncoding;

        public UniConn(Encoding TerminalEncoding_)
        {
            TerminalEncoding = TerminalEncoding_;
        }

        protected Queue<byte> Loop = new Queue<byte>();

        protected void LoopSend(string Msg)
        {
            LoopSend(TerminalEncoding.GetBytes(Msg));
        }

        protected void LoopSend(byte[] Msg)
        {
            for (int i = 0; i < Msg.Length; i++)
            {
                Loop.Enqueue(Msg[i]);
            }
        }

        protected void LoopSend(byte Msg)
        {
            Loop.Enqueue(Msg);
        }

        protected void LoopReceive(MemoryStream ms)
        {
            while (Loop.Count > 0)
            {
                ms.WriteByte(Loop.Dequeue());
            }
        }

        protected void ScreenNewLine()
        {
            Loop.Enqueue(13);
            Loop.Enqueue(10);
        }

        protected void ScreenClear()
        {
            Loop.Enqueue(0x1B);
            Loop.Enqueue((byte)'[');
            Loop.Enqueue((byte)'0');
            Loop.Enqueue((byte)'m');
            Loop.Enqueue(0x1B);
            Loop.Enqueue((byte)'[');
            Loop.Enqueue((byte)'1');
            Loop.Enqueue((byte)';');
            Loop.Enqueue((byte)'1');
            Loop.Enqueue((byte)'H');
            Loop.Enqueue(0x1B);
            Loop.Enqueue((byte)'[');
            Loop.Enqueue((byte)'2');
            Loop.Enqueue((byte)'J');
        }

    }
}
