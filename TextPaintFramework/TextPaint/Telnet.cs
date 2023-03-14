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

        Dictionary<string, string> TerminalKeys = new Dictionary<string, string>();
        List<string> TerminalKeys0 = new List<string>();
        List<string> TerminalKeys1 = new List<string>();
        List<string> TerminalKeys2 = new List<string>();
        List<string> TerminalKeys3 = new List<string>();

        int ServerPort = 0;
        bool ServerTelnet = false;
        Server Server_ = new Server();

        string TerminalResetCommand = "";

        bool LocalEcho = false;
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
        BinaryWriter TelnetFileW = null;
        int TerminalStep = 0;

        bool TelnetClient = false;
        int TelnetAutoSendWindowSize = 200;
        string TelnetKeyboardConf;
        string TelnetKeyboardConfMax = "1112211";
        int TelnetKeyboardConfI = 0;
        int TelnetFuncKeyOther = 0;

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
            if (Core_.AnsiState_.__AnsiVT52 && (N <= 3))
            {
                return "9";
            }
            return TelnetKeyboardConf[N].ToString();
        }

        void SetTelnetKeyboardConf(int N, int V)
        {
            string S1 = "";
            string S2 = "";
            if (N > 0)
            {
                S1 = TelnetKeyboardConf.Substring(0, N);
            }
            if (N < (TelnetKeyboardConfMax.Length - 1))
            {
                S2 = TelnetKeyboardConf.Substring(N + 1);
            }
            TelnetKeyboardConf = (S1 + V.ToString() + S2);
        }

        string EscapeKey = "";

        bool TerminalEncodingUTF = false;
        bool TerminalEncodingUTF8 = false;
        bool TerminalEncodingUTF16LE = false;
        bool TerminalEncodingUTF16BE = false;
        bool TerminalEncodingUTF32 = false;

        string ANSIBrowseWildcard;

        public Telnet(Core Core__, ConfigFile CF_)
        {
            CF = CF_;
            Core_ = Core__;
            Screen_ = Core__.Screen_;

            ANSIBrowseWildcard = CF.ParamGetS("FileBrowseWildcard");
            if (ANSIBrowseWildcard == "")
            {
                ANSIBrowseWildcard = "*";
            }
            ServerPort = CF.ParamGetI("ServerPort");
            ServerTelnet = CF.ParamGetB("ServerTelnet");
            TerminalEncoding = TextWork.EncodingFromName(CF.ParamGetS("TerminalEncoding"));
            if (TerminalEncoding == Encoding.UTF8)
            {
                TerminalEncodingUTF = true;
                TerminalEncodingUTF8 = true;
            }
            if (TerminalEncoding == Encoding.GetEncoding("UTF-16LE"))
            {
                TerminalEncodingUTF = true;
                TerminalEncodingUTF16LE = true;
            }
            if (TerminalEncoding == Encoding.GetEncoding("UTF-16BE"))
            {
                TerminalEncodingUTF = true;
                TerminalEncodingUTF16BE = true;
            }
            if (TerminalEncoding == Encoding.UTF32)
            {
                TerminalEncodingUTF = true;
                TerminalEncodingUTF32 = true;
            }
            ServerEncoding = TextWork.EncodingFromName(CF.ParamGetS("ServerEncoding"));
            FileDelayTime = CF.ParamGetI("FileDelayTime");
            FileDelayStep = CF.ParamGetI("FileDelayStep");
            FileDelayOffset = CF.ParamGetI("FileDelayOffset");
            Core_.__AnsiProcessDelayFactor = CF.ParamGetI("FileDelayFrame");

            string TelnetKeyboardConf_ = CF.ParamGetI("TerminalKeys").ToString();
            TelnetKeyboardConf = "";
            while (TelnetKeyboardConf_.Length < TelnetKeyboardConfMax.Length)
            {
                TelnetKeyboardConf_ = TelnetKeyboardConf_ + "0";
            }
            for (int i = 0; i < TelnetKeyboardConfMax.Length; i++)
            {
                string Allowed = "0";
                if (TelnetKeyboardConfMax[i] == 1) { Allowed = Allowed + "1"; }
                if (TelnetKeyboardConfMax[i] == 2) { Allowed = Allowed + "2"; }
                if (TelnetKeyboardConfMax[i] == 3) { Allowed = Allowed + "3"; }
                if (Allowed.Contains(TelnetKeyboardConf_[i]))
                {
                    TelnetKeyboardConf = TelnetKeyboardConf + TelnetKeyboardConf_[i].ToString();
                }
                else
                {
                    TelnetKeyboardConf = TelnetKeyboardConf + "0";
                }
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

            TerminalConnection = CF.ParamGetS("TerminalConnection");

            TerminalName = CF.ParamGetS("TerminalName");
            TerminalType = CF.ParamGetI("TerminalType");
            if ((TerminalType < 0) || (TerminalType > 5))
            {
                TerminalType = 1;
            }
            string TerminalAnswerBack0 = CF.ParamGetS("TerminalAnswerBack");
            TerminalAnswerBack = "";
            for (int i = 0; i < TerminalAnswerBack0.Length; i++)
            {
                TerminalAnswerBack = TerminalAnswerBack + "_" + TerminalAnswerBack0[i].ToString();
            }




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
            TerminalKeys.Add("Enter_0", "0D");
            TerminalKeys.Add("Enter_1", "0D0A");
            TerminalKeys.Add("Enter_2", "0A");
            TerminalKeys.Add("Backspace_0", "7F");
            TerminalKeys.Add("Backspace_1", "08");

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

            TerminalKeys.Add("F13_0", "1B_[_2_5_~");
            TerminalKeys.Add("F14_0", "1B_[_2_6_~");
            TerminalKeys.Add("F15_0", "1B_[_2_8_~");
            TerminalKeys.Add("F16_0", "1B_[_2_9_~");
            TerminalKeys.Add("F17_0", "1B_[_3_1_~");
            TerminalKeys.Add("F18_0", "1B_[_3_2_~");
            TerminalKeys.Add("F19_0", "1B_[_3_3_~");
            TerminalKeys.Add("F20_0", "1B_[_3_4_~");

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

            TerminalKeys.Add("F13_1", "1B_O_\\");
            TerminalKeys.Add("F14_1", "1B_O_]");
            TerminalKeys.Add("F15_1", "1B_O_^");
            TerminalKeys.Add("F16_1", "1B_O__");
            TerminalKeys.Add("F17_1", "");
            TerminalKeys.Add("F18_1", "");
            TerminalKeys.Add("F19_1", "");
            TerminalKeys.Add("F20_1", "");

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

            TerminalKeys.Add("F13_9", "1B_\\");
            TerminalKeys.Add("F14_9", "1B_]");
            TerminalKeys.Add("F15_9", "1B_^");
            TerminalKeys.Add("F16_9", "1B__");
            TerminalKeys.Add("F17_9", "");
            TerminalKeys.Add("F18_9", "");
            TerminalKeys.Add("F19_9", "");
            TerminalKeys.Add("F20_9", "");

            TerminalKeys.Add("Insert_9", "1B_L");
            TerminalKeys.Add("Delete_9", "1B_M");
            TerminalKeys.Add("Home_9", "1B_H");
            TerminalKeys.Add("End_9", "1B_E");
            TerminalKeys.Add("PageUp_9", "1B_I");
            TerminalKeys.Add("PageDown_9", "1B_G");

            // Alternate numeric keys
            TerminalKeys.Add("NumPad_48_0", "1B_O_p");
            TerminalKeys.Add("NumPad_49_0", "1B_O_q");
            TerminalKeys.Add("NumPad_50_0", "1B_O_r");
            TerminalKeys.Add("NumPad_51_0", "1B_O_s");
            TerminalKeys.Add("NumPad_52_0", "1B_O_t");
            TerminalKeys.Add("NumPad_53_0", "1B_O_u");
            TerminalKeys.Add("NumPad_54_0", "1B_O_v");
            TerminalKeys.Add("NumPad_55_0", "1B_O_w");
            TerminalKeys.Add("NumPad_56_0", "1B_O_x");
            TerminalKeys.Add("NumPad_57_0", "1B_O_y");
            TerminalKeys.Add("NumPad_46_0", "1B_O_n");
            TerminalKeys.Add("NumPad_44_0", "1B_O_l");
            TerminalKeys.Add("NumPad_45_0", "1B_O_m");
            TerminalKeys.Add("NumPad_43_0", "1B_O_M");

            TerminalKeys.Add("NumPad_48_1", "1B_?_p");
            TerminalKeys.Add("NumPad_49_1", "1B_?_q");
            TerminalKeys.Add("NumPad_50_1", "1B_?_r");
            TerminalKeys.Add("NumPad_51_1", "1B_?_s");
            TerminalKeys.Add("NumPad_52_1", "1B_?_t");
            TerminalKeys.Add("NumPad_53_1", "1B_?_u");
            TerminalKeys.Add("NumPad_54_1", "1B_?_v");
            TerminalKeys.Add("NumPad_55_1", "1B_?_w");
            TerminalKeys.Add("NumPad_56_1", "1B_?_x");
            TerminalKeys.Add("NumPad_57_1", "1B_?_y");
            TerminalKeys.Add("NumPad_46_1", "1B_?_n");
            TerminalKeys.Add("NumPad_44_1", "1B_?_l");
            TerminalKeys.Add("NumPad_45_1", "1B_?_m");
            TerminalKeys.Add("NumPad_43_1", "1B_?_M");
        }

        public Core Core_;
        public Screen Screen_;







        UniConn Conn;

        List<byte> ByteStr = new List<byte>();


        enum WorkStateSDef { None, InfoScreen, WaitForKey, FileOpen, FileOpenWait, DisplayPlayFwd, DisplayPlayRev, DisplayPause, DisplayFwd, DisplayRev, DisplayInfo };
        WorkStateSDef WorkStateS = WorkStateSDef.InfoScreen;

        enum WorkStateCDef { InfoScreen, Session, Toolbox, Toolbox_, EscapeKey }
        WorkStateCDef WorkStateC = WorkStateCDef.InfoScreen;



        public void Open(bool TelnetClient_)
        {
            // Reset command
            TerminalResetCommand = "";

            // 80 columns
            TerminalResetCommand += "1B_[_?_3_l";

            // Jump scroll
            TerminalResetCommand += "1B_[_?_4_l";

            // Normal (non-inverted) display
            TerminalResetCommand += "1B_[_?_5_l";

            // Disable origin
            TerminalResetCommand += "1B_[_?_6_l";

            // Enable auto wrap
            TerminalResetCommand += "1B_[_?_7_h";

            // Reset character set
            TerminalResetCommand += "1B_(_B1B_)_B1B_*_B1B_+_B15";

            // Reset top and bottom margin
            TerminalResetCommand += "1B_[_r";

            // Reset left and right margin
            TerminalResetCommand += "1B_[_?_6_9_h1B_[_s1B_[_?_6_9_l";

            // Reset text attributes and clear screen
            TerminalResetCommand += "1B_[_0_m1B_[_2_J1B_[_0_;_0_H";


            LocalEcho = false;
            LocalEchoBuf.Clear();
            TelnetClient = TelnetClient_;
            if (TelnetClient)
            {
                ByteStr = new List<byte>();
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


        public object TelnetMutex = new object();

        public void CoreEvent(string KeyName, char KeyChar, bool ModShift, bool ModCtrl, bool ModAlt)
        {
            int KeyCharI = ((int)KeyChar);

            Core_.WindowResize();

            if (TelnetClient)
            {
                CoreEvent_Client(KeyName, KeyChar, KeyCharI, ModShift, ModCtrl, ModAlt);
            }
            else
            {
                CoreEvent_Server(KeyName, KeyChar, KeyCharI, ModShift, ModCtrl, ModAlt);
            }
        }
    }
}
