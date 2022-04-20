using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TextPaint
{
    public partial class Telnet
    {
        object StatusMutex = new object();


        bool ConsoleTestI = false;
        bool ConsoleTestO = false;
        bool CommandTest = false;

        ConfigFile CF;

        List<string[]> NegoAnswers = new List<string[]>();

        Dictionary<string, string> TerminalKeys = new Dictionary<string, string>();

        int ServerPort = 0;

        bool LocalEcho = true;
        List<byte> LocalEchoBuf = new List<byte>();

        Encoding TerminalEncoding;
        Encoding ServerEncoding;
        int FileDelayTime = 100;
        int FileDelayChars = 1;

        bool TelnetClient = false;

        string EscapeKey = "";

        public Telnet(Core Core__, ConfigFile CF_)
        {
            CF = CF_;
            Core_ = Core__;
            Screen_ = Core__.Screen_;

            ServerPort = CF.ParamGetI("ServerPort");
            TerminalEncoding = TextWork.EncodingFromName(CF.ParamGetS("TerminalEncoding"));
            ServerEncoding = TextWork.EncodingFromName(CF.ParamGetS("ServerEncoding"));
            FileDelayTime = CF.ParamGetI("FileDelayTime");
            FileDelayChars = CF.ParamGetI("FileDelayChars");

            bool VT100func = CF.ParamGetB("TerminalVTFuncKeys");
            bool VT100arrows = CF.ParamGetB("TerminalVTArrowKeys");


            if (FileDelayTime < 0)
            {
                FileDelayTime = 0;
            }
            if (FileDelayChars <= 0)
            {
                FileDelayChars = 1;
            }

            string TermName_ = CF.ParamGetS("TerminalName");
            string TermName = "";
            List<int> TermNameI = TextWork.StrToInt(TermName_);
            for (int i = 0; i < TermNameI.Count; i++)
            {
                if ((TermNameI[i] >= 32) && (TermNameI[i] <= 126))
                {
                    TermName = TermName + H((byte)TermNameI[i]);
                }
            }


            // FA SUB BEGIN
            // F0 SUB SE
            // FB WILL
            // FC WON'T
            // FD DO
            // FE DON'T
            NegoAnswers.Add(new string[] { "fffd18", "fffb18" });
            NegoAnswers.Add(new string[] { "fffd03", "fffb03" });
            NegoAnswers.Add(new string[] { "fffa 1801 fff0", "fffa 1800" + TermName + " fff0" });
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


            TerminalKeys.Add("Escape", "1B");
            TerminalKeys.Add("Enter", "0D0A");
            TerminalKeys.Add("Insert", "1B5B327E");
            TerminalKeys.Add("Delete", "1B5B337E"); //7F
            TerminalKeys.Add("Home", "1B5B317E");
            TerminalKeys.Add("End", "1B5B347E");
            TerminalKeys.Add("PageUp", "1B5B357E");
            TerminalKeys.Add("PageDown", "1B5B367E");
            if (VT100arrows)
            {
                TerminalKeys.Add("Up", "1B4F41");
                TerminalKeys.Add("Down", "1B4F42");
                TerminalKeys.Add("Left", "1B4F44");
                TerminalKeys.Add("Right", "1B4F43");
            }
            else
            {
                TerminalKeys.Add("Up", "1B5b41");
                TerminalKeys.Add("Down", "1B5b42");
                TerminalKeys.Add("Left", "1B5b44");
                TerminalKeys.Add("Right", "1B5b43");
            }
            if (VT100func)
            {
                TerminalKeys.Add("F1", "1B4F50");
                TerminalKeys.Add("F2", "1B4F51");
                TerminalKeys.Add("F3", "1B4F52");
                TerminalKeys.Add("F4", "1B4F53");
                TerminalKeys.Add("F5", "1B4F54");
                TerminalKeys.Add("F6", "1B4F55");
                TerminalKeys.Add("F7", "1B4F56");
                TerminalKeys.Add("F8", "1B4F57");
                TerminalKeys.Add("F9", "1B4F58");
                TerminalKeys.Add("F10", "1B4F59");
                TerminalKeys.Add("F11", "1B4F5A");
                TerminalKeys.Add("F12", "1B4F5B");
            }
            else
            {
                TerminalKeys.Add("F1", "1B5B31317E");
                TerminalKeys.Add("F2", "1B5B31327E");
                TerminalKeys.Add("F3", "1B5B31337E");
                TerminalKeys.Add("F4", "1B5B31347E");
                TerminalKeys.Add("F5", "1B5B31357E");
                TerminalKeys.Add("F6", "1B5B31377E");
                TerminalKeys.Add("F7", "1B5B31387E");
                TerminalKeys.Add("F8", "1B5B31397E");
                TerminalKeys.Add("F9", "1B5B32307E");
                TerminalKeys.Add("F10", "1B5B32317E");
                TerminalKeys.Add("F11", "1B5B32337E");
                TerminalKeys.Add("F12", "1B5B32347E");
            }

        }

        public Core Core_;
        public Screen Screen_;








        TcpClient TCPC;
        NetworkStream NSX;


        List<byte> TelnetBuf = new List<byte>();


        public string H(byte B)
        {
            int X = B;
            string XX = X.ToString("X");
            return XX.PadLeft(2, '0');
        }


        List<byte> ByteStr = new List<byte>();


        int WorkState = 0;







        public void Open(bool TelnetClient_)
        {
            LocalEcho = true;
            LocalEchoBuf.Clear();
            TelnetClient = TelnetClient_;
            if (TelnetClient)
            {
                for (int i = 0; i < NegoAnswers.Count; i++)
                {
                    NegoAnswers[i][0] = NegoAnswers[i][0].ToUpperInvariant().Replace(" ", "");
                    NegoAnswers[i][1] = NegoAnswers[i][1].ToUpperInvariant().Replace(" ", "");
                }

                TelnetBuf = new List<byte>();
                Thread Thr = new Thread(TelnetClientWork);
                Screen_.MultiThread = true;
                Thr.Start();
            }
            else
            {
                Thread Thr = new Thread(ServerFile);
                Screen_.MultiThread = true;
                Thr.Start();
            }
        }





        public void TelnetRepaint()
        {
            Core_.AnsiRepaint();
        }




        public void CoreEvent(string KeyName, char KeyChar)
        {
            int KeyCharI = ((int)KeyChar);

            Core_.WindowResize();

            if (TelnetClient)
            {
                CoreEvent_Client(KeyName, KeyChar, KeyCharI);
            }
            else
            {
                CoreEvent_Server(KeyName, KeyChar, KeyCharI);
            }
        }
    }
}
