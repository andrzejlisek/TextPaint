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
        List<string> TerminalKeys0 = new List<string>();
        List<string> TerminalKeys1 = new List<string>();
        List<string> TerminalKeys2 = new List<string>();
        List<string> TerminalKeys3 = new List<string>();

        int ServerPort = 0;

        bool LocalEcho = true;
        List<byte> LocalEchoBuf = new List<byte>();

        Encoding TerminalEncoding;
        Encoding ServerEncoding;
        int FileDelayTime = 100;
        int FileDelayStep = 1;
        int FileDelayOffset = 0;

        long TelnetTimerResolution = 1000;
        string TelnetFileName = "";
        bool TelnetFileUse = false;
        FileStream TelnetFileS = null;
        StreamWriter TelnetFileW = null;
        int TerminalStep = 0;

        bool TelnetClient = false;
        int TelnetAutoSendWindowSize = 200;
        string TelnetKeyboardConf;
        string TelnetKeyboardConfMax = "1112";
        int TelnetKeyboardConfI = 0;
        bool TelnetFuncKeyOther = false;

        void TelnetKeyboardConfMove()
        {
            TelnetKeyboardConfI++;
            if (TelnetKeyboardConfI == TelnetKeyboardConfMax.Length)
            {
                TelnetKeyboardConfI = 0;
            }
        }

        void TelnetKeyboardConfStep()
        {
            int Val = int.Parse(TelnetKeyboardConf[TelnetKeyboardConfI].ToString());

            if (TelnetKeyboardConf[TelnetKeyboardConfI] == TelnetKeyboardConfMax[TelnetKeyboardConfI])
            {
                Val = 0;
            }
            else
            {
                Val++;
            }
            string X = "";
            for (int i = 0; i < TelnetKeyboardConf.Length; i++)
            {
                if (i == TelnetKeyboardConfI)
                {
                    X = X + Val.ToString();
                }
                else
                {
                    X = X + TelnetKeyboardConf[i].ToString();
                }
            }
            TelnetKeyboardConf = X;
        }

        string GetTelnetKeyboardConf(int N)
        {
            if (Core_.__AnsiVT52)
            {
                return "9";
            }
            return TelnetKeyboardConf[N].ToString();
        }

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
            FileDelayStep = CF.ParamGetI("FileDelayStep");
            FileDelayOffset = CF.ParamGetI("FileDelayOffset");
            Core_.__AnsiProcessDelayFactor = CF.ParamGetI("FileDelayFrame");

            TelnetKeyboardConf = CF.ParamGetI("TerminalKeys").ToString();
            bool NotGood = false;
            if (TelnetKeyboardConf.Length != 4)
            {
                NotGood = true;
            }
            else
            {
                if (!("01".Contains(TelnetKeyboardConf[0])))
                {
                    NotGood = true;
                }
                if (!("01".Contains(TelnetKeyboardConf[1])))
                {
                    NotGood = true;
                }
                if (!("01".Contains(TelnetKeyboardConf[2])))
                {
                    NotGood = true;
                }
                if (!("012".Contains(TelnetKeyboardConf[3])))
                {
                    NotGood = true;
                }
            }
            if (NotGood)
            {
                TelnetKeyboardConf = "0000";
            }

            TelnetTimerResolution = CF.ParamGetL("TerminalTimeResolution");
            TelnetFileName = CF.ParamGetS("TerminalFile");

            TerminalStep = CF.ParamGetI("TerminalStep");
            if (TerminalStep == 0)
            {
                TerminalStep = int.MaxValue;
            }


            if (TelnetTimerResolution <= 0)
            {
                TelnetTimerResolution = 100;
            }

            if (FileDelayTime < 0)
            {
                FileDelayTime = 0;
            }
            if (FileDelayStep <= 0)
            {
                FileDelayStep = 1;
            }
            if (FileDelayOffset < 0)
            {
                FileDelayOffset = 0;
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


            TerminalKeys0.Add("Up");
            TerminalKeys0.Add("Down");
            TerminalKeys0.Add("Right");
            TerminalKeys0.Add("Left");
            TerminalKeys1.Add("F1");
            TerminalKeys1.Add("F2");
            TerminalKeys1.Add("F3");
            TerminalKeys1.Add("F4");
            TerminalKeys2.Add("F5");
            TerminalKeys2.Add("F6");
            TerminalKeys2.Add("F7");
            TerminalKeys2.Add("F8");
            TerminalKeys2.Add("F9");
            TerminalKeys2.Add("F10");
            TerminalKeys2.Add("F11");
            TerminalKeys2.Add("F12");
            TerminalKeys3.Add("Home");
            TerminalKeys3.Add("End");

            // Common keys
            TerminalKeys.Add("Escape", "1B");
            TerminalKeys.Add("Enter0", "0D");
            TerminalKeys.Add("Enter1", "0D0A");
            TerminalKeys.Add("Backspace", "7F");

            // Keys depending on configuration 0
            TerminalKeys.Add("Up_0", "1B_[_A");
            TerminalKeys.Add("Down_0", "1B_[_B");
            TerminalKeys.Add("Right_0", "1B_[_C");
            TerminalKeys.Add("Left_0", "1B_[_D");
            TerminalKeys.Add("Up_1", "1B_O_A");
            TerminalKeys.Add("Down_1", "1B_O_B");
            TerminalKeys.Add("Right_1", "1B_O_C");
            TerminalKeys.Add("Left_1", "1B_O_D");

            // Keys depending on configuration 1 and 2
            TerminalKeys.Add("F1_0", "1B_[_1_1_~");
            TerminalKeys.Add("F2_0", "1B_[_1_2_~");
            TerminalKeys.Add("F3_0", "1B_[_1_3_~");
            TerminalKeys.Add("F4_0", "1B_[_1_4_~");
            TerminalKeys.Add("F5_0", "1B_[_1_5_~");
            TerminalKeys.Add("F6_0", "1B_[_1_7_~");
            TerminalKeys.Add("F7_0", "1B_[_1_8_~");
            TerminalKeys.Add("F8_0", "1B_[_1_9_~");
            TerminalKeys.Add("F9_0", "1B_[_2_0_~");
            TerminalKeys.Add("F10_0", "1B_[_2_1_~");
            TerminalKeys.Add("F11_0", "1B_[_2_3_~");
            TerminalKeys.Add("F12_0", "1B_[_2_4_~");
            TerminalKeys.Add("F1_1", "1B_O_P");
            TerminalKeys.Add("F2_1", "1B_O_Q");
            TerminalKeys.Add("F3_1", "1B_O_R");
            TerminalKeys.Add("F4_1", "1B_O_S");
            TerminalKeys.Add("F5_1", "1B_O_T");
            TerminalKeys.Add("F6_1", "1B_O_U");
            TerminalKeys.Add("F7_1", "1B_O_V");
            TerminalKeys.Add("F8_1", "1B_O_W");
            TerminalKeys.Add("F9_1", "1B_O_X");
            TerminalKeys.Add("F10_1", "1B_O_Y");
            TerminalKeys.Add("F11_1", "1B_O_Z");
            TerminalKeys.Add("F12_1", "1B_O_[");

            // Keys depending on configuration 3
            TerminalKeys.Add("Insert_0", "1B_[_2_~");
            TerminalKeys.Add("Delete_0", "1B_[_3_~");
            TerminalKeys.Add("Home_0", "1B_[_1_~");
            TerminalKeys.Add("End_0", "1B_[_4_~");
            TerminalKeys.Add("PageUp_0", "1B_[_5_~");
            TerminalKeys.Add("PageDown_0", "1B_[_6_~");
            TerminalKeys.Add("Insert_1", "1B_[_2_~");
            TerminalKeys.Add("Delete_1", "1B_[_3_~");
            TerminalKeys.Add("Home_1", "1B_O_H");
            TerminalKeys.Add("End_1", "1B_O_F");
            TerminalKeys.Add("PageUp_1", "1B_[_5_~");
            TerminalKeys.Add("PageDown_1", "1B_[_6_~");
            TerminalKeys.Add("Insert_2", "1B_[_2_~");
            TerminalKeys.Add("Delete_2", "1B_[_3_~");
            TerminalKeys.Add("Home_2", "1B_[_H");
            TerminalKeys.Add("End_2", "1B_[_F");
            TerminalKeys.Add("PageUp_2", "1B_[_5_~");
            TerminalKeys.Add("PageDown_2", "1B_[_6_~");

            // Keys for VT52 mode
            TerminalKeys.Add("Up_9", "1B_A");
            TerminalKeys.Add("Down_9", "1B_B");
            TerminalKeys.Add("Right_9", "1B_C");
            TerminalKeys.Add("Left_9", "1B_D");
            TerminalKeys.Add("F1_9", "1B_P");
            TerminalKeys.Add("F2_9", "1B_Q");
            TerminalKeys.Add("F3_9", "1B_R");
            TerminalKeys.Add("F4_9", "1B_S");
            TerminalKeys.Add("F5_9", "1B_T");
            TerminalKeys.Add("F6_9", "1B_U");
            TerminalKeys.Add("F7_9", "1B_V");
            TerminalKeys.Add("F8_9", "1B_W");
            TerminalKeys.Add("F9_9", "1B_X");
            TerminalKeys.Add("F10_9", "1B_Y");
            TerminalKeys.Add("F11_9", "1B_Z");
            TerminalKeys.Add("F12_9", "1B_[");
            TerminalKeys.Add("Insert_9", "1B_L");
            TerminalKeys.Add("Delete_9", "1B_M");
            TerminalKeys.Add("Home_9", "1B_H");
            TerminalKeys.Add("End_9", "1B_E");
            TerminalKeys.Add("PageUp_9", "1B_I");
            TerminalKeys.Add("PageDown_9", "1B_G");
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
            Core_.AnsiRepaint(false);
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
