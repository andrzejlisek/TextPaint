using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextPaint
{
    public class UniConnLoopback : UniConn
    {
        public UniConnLoopback(Encoding TerminalEncoding_) : base(TerminalEncoding_)
        {

        }

        bool Connected = false;

        public override void Send(byte[] Raw)
        {
            if (Connected)
            {
                LoopSend(Raw);
            }
        }

        public override void Receive(MemoryStream ms)
        {
            LoopReceive(ms);
        }

        public override void Open(string Addr, int Port, string TerminalName_, int TerminalW, int TerminalH)
        {
            Loop.Clear();
            Connected = true;
        }

        public override int IsConnected()
        {
            if (Connected || (Loop.Count > 0))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public override void Close()
        {
            Connected = false;
        }

    }
}
