using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace TextPaint
{
    public class UniConnApp : UniConn
    {
        public UniConnApp(Encoding TerminalEncoding_) : base(TerminalEncoding_)
        {

        }

        Process App = null;

        public override void Open(string Addr, int Port, string TerminalName_, int TerminalW, int TerminalH)
        {
            App = new Process();
            int ParamPos = Addr.IndexOf(' ');
            if (ParamPos > 0)
            {
                App.StartInfo.FileName = Addr.Substring(0, ParamPos);
                App.StartInfo.Arguments = Addr.Substring(ParamPos + 1);
            }
            else
            {
                App.StartInfo.FileName = Addr;
                App.StartInfo.Arguments = "";
            }
            App.StartInfo.RedirectStandardInput = true;
            App.StartInfo.RedirectStandardOutput = true;
            App.StartInfo.RedirectStandardError = true;

            App.StartInfo.UseShellExecute = false;
            if (App.Start())
            {
                Thread Thr1 = new Thread(ReadO);
                Thr1.Start();

                Thread Thr2 = new Thread(ReadE);
                Thr2.Start();
            }
            else
            {
                App = null;
            }
        }

        public override int IsConnected()
        {
            if (App == null)
            {
                return 0;
            }
            if (App.HasExited)
            {
                return 0;
            }
            return 1;
        }

        public override void Close()
        {
            try
            {
                App.Kill();
            }
            catch
            {
            }
            App = null;
        }

        public override void Send(byte[] Raw)
        {
            try
            {
                App.StandardInput.Write(TerminalEncoding.GetString(Raw));
            }
            catch
            {
            }
        }

        byte[] StreamBufO = new byte[1000000];
        byte[] StreamBufE = new byte[1000000];

        public override void Receive(MemoryStream ms)
        {
            LoopReceive(ms);
        }

        private void ReadO()
        {
            while (true)
            {
                try
                {
                    int BufL = App.StandardOutput.BaseStream.Read(StreamBufO, 0, StreamBufO.Length);
                    for (int i = 0; i < BufL; i++)
                    {
                        LoopSend(StreamBufO[i]);
                    }
                    if ((IsConnected() == 0) && (BufL == 0))
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

        private void ReadE()
        {
            while (true)
            {
                try
                {
                    int BufL = App.StandardError.BaseStream.Read(StreamBufE, 0, StreamBufE.Length);
                    for (int i = 0; i < BufL; i++)
                    {
                        LoopSend(StreamBufE[i]);
                    }
                    if ((IsConnected() == 0) && (BufL == 0))
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

    }
}