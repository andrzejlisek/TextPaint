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
        Clipboard Clipboard_;
        DisplayConfig DisplayConfig_;

        Dictionary<string, string> TerminalKeys = new Dictionary<string, string>();
        List<string> TerminalKeys0 = new List<string>();
        List<string> TerminalKeys1 = new List<string>();
        List<string> TerminalKeys2 = new List<string>();
        List<string> TerminalKeys3 = new List<string>();
        List<string> TerminalKeys4 = new List<string>();

        int ServerPort = 0;
        bool ServerTelnet = false;
        Server Server_ = new Server();

        string TerminalResetCommand = "";

        bool LocalEcho = false;
        bool Command_8bit = false;
        bool UseCtrlKeys = false;
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
        string TelnetKeyboardMods = "000";
        int TelnetKeyboardConfI = 0;
        int TelnetKeyboardModsI = 0;
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

        void TelnetKeyboardModsMove()
        {
            TelnetKeyboardModsI++;
            if (TelnetKeyboardModsI == TelnetKeyboardMods.Length)
            {
                TelnetKeyboardModsI = 0;
            }
        }

        void TelnetKeyboardModsStep()
        {
            string Val = TelnetKeyboardMods[TelnetKeyboardModsI].ToString();

            if (TelnetKeyboardMods[TelnetKeyboardModsI] == '1')
            {
                Val = "0";
            }
            else
            {
                Val = "1";
            }
            string X = "";
            for (int i = 0; i < TelnetKeyboardMods.Length; i++)
            {
                if (i == TelnetKeyboardModsI)
                {
                    X = X + Val;
                }
                else
                {
                    X = X + TelnetKeyboardMods[i].ToString();
                }
            }
            TelnetKeyboardMods = X;
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

        List<int> DispBuffer___;

        bool DisplayConfigReload = false;

        bool DisplayConfig_RequestReapint = false;
        bool DisplayConfig_RequestMenu = false;
        bool DisplayConfig_RequestClose = false;

        void DisplayConfigRepaint(bool AnsiReload, bool Menu)
        {
            if (TelnetClient)
            {
                DisplayConfig_RequestReapint = true;
                if (Menu)
                {
                    DisplayConfig_RequestMenu = true;
                }
            }
            else
            {
                Core_.AnsiRepaint(false);
                DisplayStatusText(DispBuffer___);
                if (AnsiReload)
                {
                    DisplayConfigReload = true;
                }
                if (Menu)
                {
                    DisplayConfig_.DisplayMenu();
                }
            }
        }

        void DisplayConfigOpen()
        {
            if (TelnetClient)
            {
                DisplayConfig_.MenuPos = TelnetInfoPos;
                WorkStateC = WorkStateCDef.DispConf;
            }
            else
            {
                DisplayPaused = true;
                ActionRequestParam = 0;
                WorkStateS = WorkStateSDef.DispConf;
                DisplayConfigReload = false;
            }
            DisplayConfig_.Open();
            DisplayConfigRepaint(false, true);
        }

        void DisplayConfigClose(bool WindowClose, int NewW, int NewH)
        {
            if ((NewW > 0) && (NewH >= 0))
            {
                Core_.Screen_.AppResize(NewW, NewH);
                DisplayConfigRepaint(true, false);
            }
            else
            {
                DisplayConfigRepaint(false, false);
            }
            if (TelnetClient)
            {
                DisplayConfig_RequestClose = true;
            }
            else
            {
                if (WindowClose)
                {
                    Screen_.AppWorking = false;
                    WorkStateS = WorkStateSDef.None;
                }
                else
                {
                    WorkStateS = WorkStateSDef.DisplayPause;
                }
            }
            if (!TelnetClient)
            {
                if (DisplayConfigReload)
                {
                    ActionRequest = 1;
                }
            }
        }

        public Telnet(Core Core__, ConfigFile CF_)
        {
            Clipboard_ = new Clipboard(Core__);
            Clipboard_.TextClipboardPasteEvent += TextPasteWork;

            DisplayConfig_ = new DisplayConfig(Core__);

            DisplayConfig_.ConfigRepaint += DisplayConfigRepaint;
            DisplayConfig_.ConfigClose += DisplayConfigClose;

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
            TerminalKeys4.Add("Insert");
            TerminalKeys4.Add("Delete");
            TerminalKeys4.Add("PageUp");
            TerminalKeys4.Add("PageDown");
            TerminalKeys4.Add("F13");
            TerminalKeys4.Add("F14");
            TerminalKeys4.Add("F15");
            TerminalKeys4.Add("F16");
            TerminalKeys4.Add("F17");
            TerminalKeys4.Add("F18");
            TerminalKeys4.Add("F19");
            TerminalKeys4.Add("F20");

            // Common keys
            TerminalKeys.Add("Escape", "1B");
            TerminalKeys.Add("Enter_0", "0D");
            TerminalKeys.Add("Enter_1", "0D0A");
            TerminalKeys.Add("Enter_2", "0A");
            TerminalKeys.Add("Backspace_0", "7F");
            TerminalKeys.Add("Backspace_1", "08");

            // Keys depending on configuration 0
            TerminalKeys.Add("Up_000_0", "##_[_A");
            TerminalKeys.Add("Down_000_0", "##_[_B");
            TerminalKeys.Add("Right_000_0", "##_[_C");
            TerminalKeys.Add("Left_000_0", "##_[_D");
            TerminalKeys.Add("Up_000_1", "##_O_A");
            TerminalKeys.Add("Down_000_1", "##_O_B");
            TerminalKeys.Add("Right_000_1", "##_O_C");
            TerminalKeys.Add("Left_000_1", "##_O_D");

            TerminalKeys.Add("Up_CAS_0", "##_[_1_;_@_A");
            TerminalKeys.Add("Down_CAS_0", "##_[_1_;_@_B");
            TerminalKeys.Add("Right_CAS_0", "##_[_1_;_@_C");
            TerminalKeys.Add("Left_CAS_0", "##_[_1_;_@_D");
            TerminalKeys.Add("Up_CAS_1", "##_[_1_;_@_A");
            TerminalKeys.Add("Down_CAS_1", "##_[_1_;_@_B");
            TerminalKeys.Add("Right_CAS_1", "##_[_1_;_@_C");
            TerminalKeys.Add("Left_CAS_1", "##_[_1_;_@_D");

            // Keys depending on configuration 1 and 2
            TerminalKeys.Add("F1_000_0", "##_[_1_1_~");
            TerminalKeys.Add("F2_000_0", "##_[_1_2_~");
            TerminalKeys.Add("F3_000_0", "##_[_1_3_~");
            TerminalKeys.Add("F4_000_0", "##_[_1_4_~");
            TerminalKeys.Add("F5_000_0", "##_[_1_5_~");
            TerminalKeys.Add("F6_000_0", "##_[_1_7_~");
            TerminalKeys.Add("F7_000_0", "##_[_1_8_~");
            TerminalKeys.Add("F8_000_0", "##_[_1_9_~");
            TerminalKeys.Add("F9_000_0", "##_[_2_0_~");
            TerminalKeys.Add("F10_000_0", "##_[_2_1_~");
            TerminalKeys.Add("F11_000_0", "##_[_2_3_~");
            TerminalKeys.Add("F12_000_0", "##_[_2_4_~");

            TerminalKeys.Add("F13_000_0", "##_[_2_5_~");
            TerminalKeys.Add("F14_000_0", "##_[_2_6_~");
            TerminalKeys.Add("F15_000_0", "##_[_2_8_~");
            TerminalKeys.Add("F16_000_0", "##_[_2_9_~");
            TerminalKeys.Add("F17_000_0", "##_[_3_1_~");
            TerminalKeys.Add("F18_000_0", "##_[_3_2_~");
            TerminalKeys.Add("F19_000_0", "##_[_3_3_~");
            TerminalKeys.Add("F20_000_0", "##_[_3_4_~");

            TerminalKeys.Add("F1_CAS_0", "##_[_1_1_;_@_~");
            TerminalKeys.Add("F2_CAS_0", "##_[_1_2_;_@_~");
            TerminalKeys.Add("F3_CAS_0", "##_[_1_3_;_@_~");
            TerminalKeys.Add("F4_CAS_0", "##_[_1_4_;_@_~");
            TerminalKeys.Add("F5_CAS_0", "##_[_1_5_;_@_~");
            TerminalKeys.Add("F6_CAS_0", "##_[_1_7_;_@_~");
            TerminalKeys.Add("F7_CAS_0", "##_[_1_8_;_@_~");
            TerminalKeys.Add("F8_CAS_0", "##_[_1_9_;_@_~");
            TerminalKeys.Add("F9_CAS_0", "##_[_2_0_;_@_~");
            TerminalKeys.Add("F10_CAS_0", "##_[_2_1_;_@_~");
            TerminalKeys.Add("F11_CAS_0", "##_[_2_3_;_@_~");
            TerminalKeys.Add("F12_CAS_0", "##_[_2_4_;_@_~");

            TerminalKeys.Add("F13_CAS_0", "##_[_2_5_;_@_~");
            TerminalKeys.Add("F14_CAS_0", "##_[_2_6_;_@_~");
            TerminalKeys.Add("F15_CAS_0", "##_[_2_8_;_@_~");
            TerminalKeys.Add("F16_CAS_0", "##_[_2_9_;_@_~");
            TerminalKeys.Add("F17_CAS_0", "##_[_3_1_;_@_~");
            TerminalKeys.Add("F18_CAS_0", "##_[_3_2_;_@_~");
            TerminalKeys.Add("F19_CAS_0", "##_[_3_3_;_@_~");
            TerminalKeys.Add("F20_CAS_0", "##_[_3_4_;_@_~");

            TerminalKeys.Add("F1_000_1", "##_O_P");
            TerminalKeys.Add("F2_000_1", "##_O_Q");
            TerminalKeys.Add("F3_000_1", "##_O_R");
            TerminalKeys.Add("F4_000_1", "##_O_S");
            TerminalKeys.Add("F5_000_1", "##_O_T");
            TerminalKeys.Add("F6_000_1", "##_O_U");
            TerminalKeys.Add("F7_000_1", "##_O_V");
            TerminalKeys.Add("F8_000_1", "##_O_W");
            TerminalKeys.Add("F9_000_1", "##_O_X");
            TerminalKeys.Add("F10_000_1", "##_O_Y");
            TerminalKeys.Add("F11_000_1", "##_O_Z");
            TerminalKeys.Add("F12_000_1", "##_O_[");

            TerminalKeys.Add("F13_000_1", "##_O_\\");
            TerminalKeys.Add("F14_000_1", "##_O_]");
            TerminalKeys.Add("F15_000_1", "##_O_^");
            TerminalKeys.Add("F16_000_1", "##_O__");
            TerminalKeys.Add("F17_000_1", "");
            TerminalKeys.Add("F18_000_1", "");
            TerminalKeys.Add("F19_000_1", "");
            TerminalKeys.Add("F20_000_1", "");

            TerminalKeys.Add("F1_CAS_1", "##_[_1_;_@_P");
            TerminalKeys.Add("F2_CAS_1", "##_[_1_;_@_Q");
            TerminalKeys.Add("F3_CAS_1", "##_[_1_;_@_R");
            TerminalKeys.Add("F4_CAS_1", "##_[_1_;_@_S");
            TerminalKeys.Add("F5_CAS_1", "##_[_1_;_@_T");
            TerminalKeys.Add("F6_CAS_1", "##_[_1_;_@_U");
            TerminalKeys.Add("F7_CAS_1", "##_[_1_;_@_V");
            TerminalKeys.Add("F8_CAS_1", "##_[_1_;_@_W");
            TerminalKeys.Add("F9_CAS_1", "##_[_1_;_@_X");
            TerminalKeys.Add("F10_CAS_1", "##_[_1_;_@_Y");
            TerminalKeys.Add("F11_CAS_1", "##_[_1_;_@_Z");
            TerminalKeys.Add("F12_CAS_1", "##_[_1_;_@_[");

            TerminalKeys.Add("F13_CAS_1", "##_[_1_;_@_\\");
            TerminalKeys.Add("F14_CAS_1", "##_[_1_;_@_]");
            TerminalKeys.Add("F15_CAS_1", "##_[_1_;_@_^");
            TerminalKeys.Add("F16_CAS_1", "##_[_1_;_@__");
            TerminalKeys.Add("F17_CAS_1", "");
            TerminalKeys.Add("F18_CAS_1", "");
            TerminalKeys.Add("F19_CAS_1", "");
            TerminalKeys.Add("F20_CAS_1", "");

            // Keys depending on configuration 3
            TerminalKeys.Add("Insert_000_0", "##_[_2_~");
            TerminalKeys.Add("Delete_000_0", "##_[_3_~");
            TerminalKeys.Add("Home_000_0", "##_[_1_~");
            TerminalKeys.Add("End_000_0", "##_[_4_~");
            TerminalKeys.Add("PageUp_000_0", "##_[_5_~");
            TerminalKeys.Add("PageDown_000_0", "##_[_6_~");
            TerminalKeys.Add("Insert_000_1", "##_[_2_~");
            TerminalKeys.Add("Delete_000_1", "##_[_3_~");
            TerminalKeys.Add("Home_000_1", "##_O_H");
            TerminalKeys.Add("End_000_1", "##_O_F");
            TerminalKeys.Add("PageUp_000_1", "##_[_5_~");
            TerminalKeys.Add("PageDown_000_1", "##_[_6_~");
            TerminalKeys.Add("Insert_000_2", "##_[_2_~");
            TerminalKeys.Add("Delete_000_2", "##_[_3_~");
            TerminalKeys.Add("Home_000_2", "##_[_H");
            TerminalKeys.Add("End_000_2", "##_[_F");
            TerminalKeys.Add("PageUp_000_2", "##_[_5_~");
            TerminalKeys.Add("PageDown_000_2", "##_[_6_~");

            TerminalKeys.Add("Insert_CAS_0", "##_[_2_;_@_~");
            TerminalKeys.Add("Delete_CAS_0", "##_[_3_;_@_~");
            TerminalKeys.Add("Home_CAS_0", "##_[_1_;_@_~");
            TerminalKeys.Add("End_CAS_0", "##_[_4_;_@_~");
            TerminalKeys.Add("PageUp_CAS_0", "##_[_5_;_@_~");
            TerminalKeys.Add("PageDown_CAS_0", "##_[_6_;_@_~");
            TerminalKeys.Add("Insert_CAS_1", "##_[_2_;_@_~");
            TerminalKeys.Add("Delete_CAS_1", "##_[_3_;_@_~");
            TerminalKeys.Add("Home_CAS_1", "##_[_1_;_@_H");
            TerminalKeys.Add("End_CAS_1", "##_[_1_;_@_F");
            TerminalKeys.Add("PageUp_CAS_1", "##_[_5_;_@_~");
            TerminalKeys.Add("PageDown_CAS_1", "##_[_6_;_@_~");
            TerminalKeys.Add("Insert_CAS_2", "##_[_2_;_@_~");
            TerminalKeys.Add("Delete_CAS_2", "##_[_3_;_@_~");
            TerminalKeys.Add("Home_CAS_2", "##_[_H");
            TerminalKeys.Add("End_CAS_2", "##_[_F");
            TerminalKeys.Add("PageUp_CAS_2", "##_[_5_;_@_~");
            TerminalKeys.Add("PageDown_CAS_2", "##_[_6_;_@_~");

            // Keys for VT52 mode
            TerminalKeys.Add("Up_000_9", "##_A");
            TerminalKeys.Add("Down_000_9", "##_B");
            TerminalKeys.Add("Right_000_9", "##_C");
            TerminalKeys.Add("Left_000_9", "##_D");
            TerminalKeys.Add("F1_000_9", "##_P");
            TerminalKeys.Add("F2_000_9", "##_Q");
            TerminalKeys.Add("F3_000_9", "##_R");
            TerminalKeys.Add("F4_000_9", "##_S");
            TerminalKeys.Add("F5_000_9", "##_T");
            TerminalKeys.Add("F6_000_9", "##_U");
            TerminalKeys.Add("F7_000_9", "##_V");
            TerminalKeys.Add("F8_000_9", "##_W");
            TerminalKeys.Add("F9_000_9", "##_X");
            TerminalKeys.Add("F10_000_9", "##_Y");
            TerminalKeys.Add("F11_000_9", "##_Z");
            TerminalKeys.Add("F12_000_9", "##_[");

            TerminalKeys.Add("F13_000_9", "##_\\");
            TerminalKeys.Add("F14_000_9", "##_]");
            TerminalKeys.Add("F15_000_9", "##_^");
            TerminalKeys.Add("F16_000_9", "##__");
            TerminalKeys.Add("F17_000_9", "");
            TerminalKeys.Add("F18_000_9", "");
            TerminalKeys.Add("F19_000_9", "");
            TerminalKeys.Add("F20_000_9", "");

            TerminalKeys.Add("Insert_000_9", "##_L");
            TerminalKeys.Add("Delete_000_9", "##_M");
            TerminalKeys.Add("Home_000_9", "##_H");
            TerminalKeys.Add("End_000_9", "##_E");
            TerminalKeys.Add("PageUp_000_9", "##_I");
            TerminalKeys.Add("PageDown_000_9", "##_G");

            // Alternate numeric keys
            TerminalKeys.Add("NumPad_48_0", "##_O_p");
            TerminalKeys.Add("NumPad_49_0", "##_O_q");
            TerminalKeys.Add("NumPad_50_0", "##_O_r");
            TerminalKeys.Add("NumPad_51_0", "##_O_s");
            TerminalKeys.Add("NumPad_52_0", "##_O_t");
            TerminalKeys.Add("NumPad_53_0", "##_O_u");
            TerminalKeys.Add("NumPad_54_0", "##_O_v");
            TerminalKeys.Add("NumPad_55_0", "##_O_w");
            TerminalKeys.Add("NumPad_56_0", "##_O_x");
            TerminalKeys.Add("NumPad_57_0", "##_O_y");
            TerminalKeys.Add("NumPad_46_0", "##_O_n");
            TerminalKeys.Add("NumPad_44_0", "##_O_l");
            TerminalKeys.Add("NumPad_45_0", "##_O_m");
            TerminalKeys.Add("NumPad_43_0", "##_O_M");

            TerminalKeys.Add("NumPad_48_1", "##_?_p");
            TerminalKeys.Add("NumPad_49_1", "##_?_q");
            TerminalKeys.Add("NumPad_50_1", "##_?_r");
            TerminalKeys.Add("NumPad_51_1", "##_?_s");
            TerminalKeys.Add("NumPad_52_1", "##_?_t");
            TerminalKeys.Add("NumPad_53_1", "##_?_u");
            TerminalKeys.Add("NumPad_54_1", "##_?_v");
            TerminalKeys.Add("NumPad_55_1", "##_?_w");
            TerminalKeys.Add("NumPad_56_1", "##_?_x");
            TerminalKeys.Add("NumPad_57_1", "##_?_y");
            TerminalKeys.Add("NumPad_46_1", "##_?_n");
            TerminalKeys.Add("NumPad_44_1", "##_?_l");
            TerminalKeys.Add("NumPad_45_1", "##_?_m");
            TerminalKeys.Add("NumPad_43_1", "##_?_M");

            // Generating codes for CAS modifiers
            List<string> TerminalKeysCAS = new List<string>();
            TerminalKeysCAS.AddRange(TerminalKeys0);
            TerminalKeysCAS.AddRange(TerminalKeys1);
            TerminalKeysCAS.AddRange(TerminalKeys2);
            TerminalKeysCAS.AddRange(TerminalKeys3);
            TerminalKeysCAS.AddRange(TerminalKeys4);
            for (int i = 0; i < TerminalKeysCAS.Count; i++)
            {
                string N = TerminalKeysCAS[i];
                for (int ii = 0; ii < 3; ii++)
                {
                    string N_ = ii.ToString();
                    if (TerminalKeys.ContainsKey(N + "_000_" + N_))
                    {
                        string NN = N + "_CAS_" + N_;
                        TerminalKeys.Add(N + "_001_" + N_, TerminalKeys[NN].Replace("_@", "_2"));
                        TerminalKeys.Add(N + "_010_" + N_, TerminalKeys[NN].Replace("_@", "_3"));
                        TerminalKeys.Add(N + "_011_" + N_, TerminalKeys[NN].Replace("_@", "_4"));
                        TerminalKeys.Add(N + "_100_" + N_, TerminalKeys[NN].Replace("_@", "_5"));
                        TerminalKeys.Add(N + "_101_" + N_, TerminalKeys[NN].Replace("_@", "_6"));
                        TerminalKeys.Add(N + "_110_" + N_, TerminalKeys[NN].Replace("_@", "_7"));
                        TerminalKeys.Add(N + "_111_" + N_, TerminalKeys[NN].Replace("_@", "_8"));
                    }
                }
                if (TerminalKeys.ContainsKey(N + "_000_9"))
                {
                    TerminalKeys.Add(N + "_001_9", TerminalKeys[N + "_000_9"]);
                    TerminalKeys.Add(N + "_010_9", TerminalKeys[N + "_000_9"]);
                    TerminalKeys.Add(N + "_011_9", TerminalKeys[N + "_000_9"]);
                    TerminalKeys.Add(N + "_100_9", TerminalKeys[N + "_000_9"]);
                    TerminalKeys.Add(N + "_101_9", TerminalKeys[N + "_000_9"]);
                    TerminalKeys.Add(N + "_110_9", TerminalKeys[N + "_000_9"]);
                    TerminalKeys.Add(N + "_111_9", TerminalKeys[N + "_000_9"]);
                }
            }
        }

        public Core Core_;
        public Screen Screen_;







        UniConn Conn;
        string WindowTitle = "";
        string WindowIcon = "";

        List<byte> ByteStr = new List<byte>();


        enum WorkStateSDef { None, InfoScreen, WaitForKey, FileOpen, FileOpenWait, DisplayPlayFwd, DisplayPlayRev, DisplayPause, DisplayFwd, DisplayRev, DisplayInfo, DispConf };
        WorkStateSDef WorkStateS = WorkStateSDef.InfoScreen;

        enum WorkStateCDef { InfoScreen, Session, Toolbox, Toolbox_, EscapeKey, DispConf }
        WorkStateCDef WorkStateC = WorkStateCDef.InfoScreen;



        public void Open(bool TelnetClient_)
        {
            // Reset command
            TerminalResetCommand = "";

            // Main screen
            TerminalResetCommand += "1B_[_?_4_7_l";
            TerminalResetCommand += "1B_[_?_1_0_4_7_l";

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
            Command_8bit = false;
            UseCtrlKeys = false;
            WindowTitle = "";
            WindowIcon = "";
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
        public object TelnetMutex2 = new object();

        public void CoreEvent(string KeyName, char KeyChar, bool ModShift, bool ModCtrl, bool ModAlt)
        {
            if (TelnetClient)
            {
                Monitor.Enter(TelnetMutex2);
            }
            int KeyCharI = ((int)KeyChar);
            Core_.WindowResize();
            if (TelnetClient)
            {
                CoreEvent_Client(KeyName, KeyChar, KeyCharI, ModShift, ModCtrl, ModAlt);
                Monitor.Exit(TelnetMutex2);
            }
            else
            {
                CoreEvent_Server(KeyName, KeyChar, KeyCharI, ModShift, ModCtrl, ModAlt);
            }
        }
    }
}
