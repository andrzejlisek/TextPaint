using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TextPaint
{
    public class TelnetClient
    {
        bool ConsoleTestI = false;
        bool ConsoleTestO = false;
        bool CommandTest = false;

        object TelnetMutex = new object();

        enum WorkStateCDef { InfoScreen, Session, Toolbox, Toolbox_, EscapeKey, DispConf }
        WorkStateCDef WorkStateC = WorkStateCDef.InfoScreen;

        Server Server_ = new Server();
        int ServerPort = 0;
        bool ServerTelnet = false;
        Encoding ServerEncoding;

        Encoding TerminalEncoding;

        UniConn Conn;
        string WindowTitle = "";
        string WindowIcon = "";
        Stack<string> WindowTitleSt = new Stack<string>();
        Stack<string> WindowIconSt = new Stack<string>();

        string EscapeKey = "";

        bool TelnetMouseWork = false;
        int TelnetMouseX = -1;
        int TelnetMouseY = -1;
        bool TelnetMouse1 = false;
        bool TelnetMouse2 = false;
        bool TelnetMouse3 = false;

        bool MouseHighlight = false;
        int MouseHighlightX = -1;
        int MouseHighlightY = -1;
        int MouseHighlightFirst = -1;
        int MouseHighlightLast = -1;

        bool TerminalEncodingUTF = false;
        bool TerminalEncodingUTF8 = false;
        bool TerminalEncodingUTF16LE = false;
        bool TerminalEncodingUTF16BE = false;
        bool TerminalEncodingUTF32 = false;

        int TelnetAutoSendWindowSize = 200;
        string TelnetKeyboardConf;
        string TelnetKeyboardConfMax = "1112211";
        string TelnetKeyboardMods = "000";
        int TelnetKeyboardConfI = 0;
        int TelnetKeyboardModsI = 0;
        int TelnetFuncKeyOther = 0;

        Dictionary<string, string> TerminalKeys = new Dictionary<string, string>();
        List<string> TerminalKeys0 = new List<string>();
        List<string> TerminalKeys1 = new List<string>();
        List<string> TerminalKeys2 = new List<string>();
        List<string> TerminalKeys3 = new List<string>();
        List<string> TerminalKeys4 = new List<string>();

        Clipboard Clipboard_;

        bool LocalEcho = false;
        bool Command_8bit = false;
        bool UseCtrlKeys = false;
        List<byte> LocalEchoBuf = new List<byte>();

        List<byte> ByteStr = new List<byte>();

        object TelnetFileUseMtx = new object();
        bool TelnetFileUse = false;
        FileStream TelnetFileS = null;
        BinaryWriter TelnetFileW = null;

        List<int> FileCtX;

        long TelnetTimerResolution = 1000;
        string TelnetFileName = "";
        int TerminalStep = 0;

        void TelnetMouseReset()
        {
            MouseHighlight = false;
            TelnetMouseX = -1;
            TelnetMouseY = -1;
            TelnetMouse1 = false;
            TelnetMouse2 = false;
            TelnetMouse3 = false;
        }

        void EscapeKeyMessage()
        {
            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
            Screen_.SetCursorPositionNoRefresh(0, 0);
            Screen_.WriteText("Press key, which will be used as escape key...", Core_.TextNormalBack, Core_.TextNormalFore);
            Screen_.Refresh();
        }

        string EscapeKeyId(string KeyName, char KeyChar)
        {
            return KeyName + "_" + TextWork.CharCode(KeyChar, 0);
        }

        public object TelnetMutex2 = new object();

        public void Repaint()
        {
            if (WorkStateC == WorkStateCDef.InfoScreen)
            {
                EscapeKeyMessage();
            }
            if ((WorkStateC == WorkStateCDef.Toolbox) || (WorkStateC == WorkStateCDef.EscapeKey))
            {
                TelnetDisplayInfo(true);
            }
        }

        private string MouseCoordCalc(int Val, bool UTF8)
        {
            if (UTF8)
            {
                if (Val <= 94)
                {
                    return TextWork.CharCode(Val + 33, 0);
                }
                else
                {
                    return TextWork.CharCode(((Val + 33) / 64) + 192, 0) + TextWork.CharCode(((Val + 33) % 64) + 128, 0);
                }
            }
            else
            {
                return TextWork.CharCode(Val + 33, 0);
            }
        }

        private void SendMouseEvent(int X, int Y, int Evt)
        {
            int EvtCode = 0;
            int EvtCode_ = 0;
            int EvtType = 0;
            switch (Evt % 100)
            {
                case 1: EvtCode = 0x20; EvtCode_ = 0x20; EvtType = 0; break;
                case 2: EvtCode = 0x21; EvtCode_ = 0x21; EvtType = 0; break;
                case 3: EvtCode = 0x22; EvtCode_ = 0x22; EvtType = 0; break;
                case 4: EvtCode = 0x60; EvtCode_ = 0x60; EvtType = 0; break;
                case 5: EvtCode = 0x61; EvtCode_ = 0x61; EvtType = 0; break;
                case 11: EvtCode = 0x23; EvtCode_ = 0x20; EvtType = 1; break;
                case 12: EvtCode = 0x23; EvtCode_ = 0x21; EvtType = 1; break;
                case 13: EvtCode = 0x23; EvtCode_ = 0x22; EvtType = 1; break;
                case 14: EvtCode = 0x23; EvtCode_ = 0x60; EvtType = 1; break;
                case 15: EvtCode = 0x23; EvtCode_ = 0x61; EvtType = 1; break;
                case 21: EvtCode = 0x40; EvtCode_ = 0x40; EvtType = 2; break;
                case 22: EvtCode = 0x41; EvtCode_ = 0x41; EvtType = 2; break;
                case 23: EvtCode = 0x42; EvtCode_ = 0x42; EvtType = 2; break;
                case 20: EvtCode = 0x43; EvtCode_ = 0x43; EvtType = 2; break;
            }
            switch (Evt / 100)
            {
                case 0: EvtCode += 0x00; EvtCode_ += 0x00; break;
                case 4: EvtCode += 0x04; EvtCode_ += 0x04; break;
                case 2: EvtCode += 0x08; EvtCode_ += 0x08; break;
                case 6: EvtCode += 0x0C; EvtCode_ += 0x0C; break;
                case 1: EvtCode += 0x10; EvtCode_ += 0x10; break;
                case 5: EvtCode += 0x14; EvtCode_ += 0x14; break;
                case 3: EvtCode += 0x18; EvtCode_ += 0x18; break;
                case 7: EvtCode += 0x1c; EvtCode_ += 0x1c; break;
            }
            if ((X >= 0) && (Y >= 0))
            {
                bool Std = true;

                // SGR-Pixel
                if (Screen_.MouseGet(1016))
                {
                    Std = false;
                    if (EvtType == 1)
                    {
                        Send("##_[_<" + TelnetReportNumToStr(EvtCode_ - 32) + "_;" + TelnetReportNumToStr(X * Screen.TerminalCellW + Screen.TerminalCellW2) + "_;" + TelnetReportNumToStr(Y * Screen.TerminalCellH + Screen.TerminalCellH2) + "_m");
                    }
                    else
                    {
                        Send("##_[_<" + TelnetReportNumToStr(EvtCode_ - 32) + "_;" + TelnetReportNumToStr(X * Screen.TerminalCellW + Screen.TerminalCellW2) + "_;" + TelnetReportNumToStr(Y * Screen.TerminalCellH + Screen.TerminalCellH2) + "_M");
                    }
                }
                else
                {
                    // SGR
                    if (Screen_.MouseGet(1006))
                    {
                        Std = false;
                        if (EvtType == 1)
                        {
                            Send("##_[_<" + TelnetReportNumToStr(EvtCode_ - 32) + "_;" + TelnetReportNumToStr(X + 1) + "_;" + TelnetReportNumToStr(Y + 1) + "_m");
                        }
                        else
                        {
                            Send("##_[_<" + TelnetReportNumToStr(EvtCode_ - 32) + "_;" + TelnetReportNumToStr(X + 1) + "_;" + TelnetReportNumToStr(Y + 1) + "_M");
                        }
                    }
                    else
                    {
                        // URXVT
                        if (Screen_.MouseGet(1015))
                        {
                            Std = false;
                            Send("##_[" + TelnetReportNumToStr(EvtCode) + "_;" + TelnetReportNumToStr(X + 1) + "_;" + TelnetReportNumToStr(Y + 1) + "_M");
                        }
                        else
                        {
                            // UTF-8
                            if (Screen_.MouseGet(1005) && (X <= 2014) && (Y <= 2014))
                            {
                                Std = false;
                                Send("##_[_M" + TextWork.CharCode(EvtCode, 0) + MouseCoordCalc(X, true) + MouseCoordCalc(Y, true));
                            }
                            else
                            {
                                if (Std && (X <= 222) && (Y <= 222))
                                {
                                    Send("##_[_M" + TextWork.CharCode(EvtCode, 0) + MouseCoordCalc(X, false) + MouseCoordCalc(Y, false));
                                }
                            }
                        }
                    }
                }

            }
        }

        public void CoreEvent_Client(string KeyName, char KeyChar, int KeyCharI, bool ModShift, bool ModCtrl, bool ModAlt)
        {
            Monitor.Enter(TelnetMutex2);
            switch (KeyName)
            {
                case "Resize":
                    Repaint();
                    Monitor.Exit(TelnetMutex2);
                    return;
                case "MouseMove":
                    TelnetMouseX = Core_.EventMouseX;
                    TelnetMouseY = Core_.EventMouseY;
                    if (Screen_.MouseIsActiveX)
                    {
                        int Btn = 20;
                        if (TelnetMouse3) { Btn = 23; }
                        if (TelnetMouse2) { Btn = 22; }
                        if (TelnetMouse1) { Btn = 21; }
                        if ((Screen_.MouseGet(1002) && (Btn > 20)) || Screen_.MouseGet(1003))
                        {
                            int KeyMods = 0;
                            if (ModCtrl || (TelnetKeyboardMods[0] != '0')) { KeyMods += 100; }
                            if (ModAlt || (TelnetKeyboardMods[1] != '0')) { KeyMods += 200; }
                            if (ModShift || (TelnetKeyboardMods[2] != '0')) { KeyMods += 400; }
                            SendMouseEvent(TelnetMouseX, TelnetMouseY, Btn + KeyMods);
                        }
                    }
                    Monitor.Exit(TelnetMutex2);
                    return;
                case "MouseBtn":
                    if (Screen_.MouseIsActiveX)
                    {
                        bool SendBtn = false;
                        switch (KeyCharI)
                        {
                            case 1: TelnetMouse1 = true; TelnetMouse2 = false; TelnetMouse3 = false; break;
                            case 2: TelnetMouse2 = true; TelnetMouse3 = false; TelnetMouse1 = false; break;
                            case 3: TelnetMouse3 = true; TelnetMouse1 = false; TelnetMouse2 = false; break;
                            case 11: TelnetMouse1 = false; TelnetMouse2 = false; TelnetMouse3 = false; break;
                            case 12: TelnetMouse2 = false; TelnetMouse3 = false; TelnetMouse1 = false; break;
                            case 13: TelnetMouse3 = false; TelnetMouse1 = false; TelnetMouse2 = false; break;
                        }
                        if (Screen_.MouseGet(9) && (KeyCharI <= 3))
                        {
                            SendBtn = true;
                        }
                        int KeyMods = 0;
                        if (Screen_.MouseGet(1000) || Screen_.MouseGet(1001) || Screen_.MouseGet(1002) || Screen_.MouseGet(1003))
                        {
                            SendBtn = true;
                            if (ModCtrl || (TelnetKeyboardMods[0] != '0')) { KeyMods += 100; }
                            if (ModAlt || (TelnetKeyboardMods[1] != '0')) { KeyMods += 200; }
                            if (ModShift || (TelnetKeyboardMods[2] != '0')) { KeyMods += 400; }
                        }
                        if (SendBtn)
                        {
                            if ((KeyCharI >= 10) && (KeyCharI < 20) && (MouseHighlight))
                            {
                                int SelectX1 = (MouseHighlightX);
                                int SelectY1 = (MouseHighlightY);
                                int SelectX2 = (TelnetMouseX + 1);
                                int SelectY2 = (TelnetMouseY + 1);

                                if (Screen_.MouseGet(1016))
                                {
                                    SelectX2 = SelectX2 * Screen.TerminalCellW;
                                    SelectY2 = SelectY2 * Screen.TerminalCellH;
                                    if (SelectY1 < (MouseHighlightFirst * Screen.TerminalCellH)) { SelectY1 = (MouseHighlightFirst) * Screen.TerminalCellH; }
                                    if (SelectY1 > ((MouseHighlightLast - 1) * Screen.TerminalCellH)) { SelectY1 = (MouseHighlightLast - 1) * Screen.TerminalCellH; }
                                    if (SelectY2 < (MouseHighlightFirst * Screen.TerminalCellH)) { SelectY2 = (MouseHighlightFirst) * Screen.TerminalCellH; }
                                    if (SelectY2 > ((MouseHighlightLast - 1) * Screen.TerminalCellH)) { SelectY2 = (MouseHighlightLast - 1) * Screen.TerminalCellH; }
                                }
                                else
                                {
                                    if (SelectY1 < MouseHighlightFirst) { SelectY1 = MouseHighlightFirst; }
                                    if (SelectY1 > (MouseHighlightLast - 1)) { SelectY1 = MouseHighlightLast - 1; }
                                    if (SelectY2 < MouseHighlightFirst) { SelectY2 = MouseHighlightFirst; }
                                    if (SelectY2 > (MouseHighlightLast - 1)) { SelectY2 = MouseHighlightLast - 1; }
                                }

                                bool Std = true;

                                // SGR-Pixel
                                if (Screen_.MouseGet(1016))
                                {
                                    Std = false;
                                    if ((SelectX1 == SelectX2) && (SelectY1 == SelectY2))
                                    {
                                        Send("##_[_<" + TelnetReportNumToStr(SelectX1) + "_;" + TelnetReportNumToStr(SelectY1) + "_t");
                                    }
                                    else
                                    {
                                        Send("##_[_<" + TelnetReportNumToStr(SelectX1) + "_;" + TelnetReportNumToStr(SelectY1) + "_;" + TelnetReportNumToStr(SelectX2) + "_;" + TelnetReportNumToStr(SelectY2) + "_;" + TelnetReportNumToStr((TelnetMouseX + 1) * Screen.TerminalCellW) + "_;" + TelnetReportNumToStr((TelnetMouseY + 1) * Screen.TerminalCellH) + "_T");
                                    }
                                }
                                else
                                {
                                    // SGR
                                    if (Screen_.MouseGet(1006))
                                    {
                                        Std = false;
                                        if ((SelectX1 == SelectX2) && (SelectY1 == SelectY2))
                                        {
                                            Send("##_[_<" + TelnetReportNumToStr(SelectX1) + "_;" + TelnetReportNumToStr(SelectY1) + "_t");
                                        }
                                        else
                                        {
                                            Send("##_[_<" + TelnetReportNumToStr(SelectX1) + "_;" + TelnetReportNumToStr(SelectY1) + "_;" + TelnetReportNumToStr(SelectX2) + "_;" + TelnetReportNumToStr(SelectY2) + "_;" + TelnetReportNumToStr(TelnetMouseX + 1) + "_;" + TelnetReportNumToStr(TelnetMouseY + 1) + "_T");
                                        }
                                    }
                                    else
                                    {
                                        // URXVT
                                        if (Screen_.MouseGet(1015))
                                        {
                                            Std = false;
                                            if ((SelectX1 == SelectX2) && (SelectY1 == SelectY2))
                                            {
                                                Send("##_[" + TelnetReportNumToStr(SelectX1) + "_;" + TelnetReportNumToStr(SelectY1) + "_t");
                                            }
                                            else
                                            {
                                                Send("##_[" + TelnetReportNumToStr(SelectX1) + "_;" + TelnetReportNumToStr(SelectY1) + "_;" + TelnetReportNumToStr(SelectX2) + "_;" + TelnetReportNumToStr(SelectY2) + "_;" + TelnetReportNumToStr(TelnetMouseX + 1) + "_;" + TelnetReportNumToStr(TelnetMouseY + 1) + "_T");
                                            }
                                        }
                                        else
                                        {
                                            // UTF-8
                                            if (Screen_.MouseGet(1005) && (TelnetMouseX <= 2014) && (TelnetMouseY <= 2014))
                                            {
                                                Std = false;
                                                if ((SelectX1 == SelectX2) && (SelectY1 == SelectY2))
                                                {
                                                    Send("##_[_t" + MouseCoordCalc(SelectX1, true) + MouseCoordCalc(SelectY1, true));
                                                }
                                                else
                                                {
                                                    Send("##_[_T" + MouseCoordCalc(SelectX1, true) + MouseCoordCalc(SelectY1, true) + MouseCoordCalc(SelectX2, true) + MouseCoordCalc(SelectY2, true) + MouseCoordCalc(TelnetMouseX + 1, true) + MouseCoordCalc(TelnetMouseY + 1, true));
                                                }
                                            }
                                            else
                                            {
                                                if (Std && (TelnetMouseX <= 222) && (TelnetMouseY <= 222))
                                                {
                                                    if ((SelectX1 == SelectX2) && (SelectY1 == SelectY2))
                                                    {
                                                        Send("##_[_t" + MouseCoordCalc(SelectX1, false) + MouseCoordCalc(SelectY1, false));
                                                    }
                                                    else
                                                    {
                                                        Send("##_[_T" + MouseCoordCalc(SelectX1, false) + MouseCoordCalc(SelectY1, false) + MouseCoordCalc(SelectX2, false) + MouseCoordCalc(SelectY2, false) + MouseCoordCalc(TelnetMouseX + 1, false) + MouseCoordCalc(TelnetMouseY + 1, false));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                MouseHighlight = false;
                            }
                            else
                            {
                                SendMouseEvent(TelnetMouseX, TelnetMouseY, KeyCharI + KeyMods);
                            }
                        }
                    }
                    Monitor.Exit(TelnetMutex2);
                    return;
                case "WindowClose":
                    TelnetClose(true);
                    Monitor.Exit(TelnetMutex2);
                    return;
                case "WindowFocus1":
                    if (Screen_.MouseGet(1004))
                    {
                        Send("##_[_I");
                    }
                    Monitor.Exit(TelnetMutex2);
                    return;
                case "WindowFocus0":
                    if (Screen_.MouseGet(1004))
                    {
                        Send("##_[_O");
                    }
                    Monitor.Exit(TelnetMutex2);
                    return;
            }

            {
                Monitor.Enter(TelnetMutex);
                if (Core_.CoreAnsi_.AnsiTerminalResize(Core_.Screen_.WinW, Core_.Screen_.WinH))
                {
                    Monitor.Exit(TelnetMutex);
                    if (WorkStateC == WorkStateCDef.InfoScreen)
                    {
                        EscapeKeyMessage();
                    }
                    if ((WorkStateC == WorkStateCDef.Session) || (WorkStateC == WorkStateCDef.Toolbox))
                    {
                        if (Conn.IsConnected() == 1)
                        {
                            //TelnetSendWindowSize();
                        }
                    }
                }
                else
                {
                    Monitor.Exit(TelnetMutex);
                }

                switch (WorkStateC)
                {
                    case WorkStateCDef.InfoScreen:
                        {
                            if (!("".Equals(KeyName)))
                            {
                                EscapeKey = EscapeKeyId(KeyName, KeyChar);
                            }
                        }
                        break;
                    case WorkStateCDef.Session:
                        {
                            if (EscapeKeyId(KeyName, KeyChar).Equals(EscapeKey))
                            {
                                TelnetFileRestart();
                                WorkStateC = WorkStateCDef.Toolbox;
                            }
                            else
                            {
                                string CAS = "";
                                if (ModCtrl || (TelnetKeyboardMods[0] != '0')) { CAS = CAS + "1"; } else { CAS = CAS + "0"; }
                                if (ModAlt || (TelnetKeyboardMods[1] != '0')) { CAS = CAS + "1"; } else { CAS = CAS + "0"; }
                                if (ModShift || (TelnetKeyboardMods[2] != '0')) { CAS = CAS + "1"; } else { CAS = CAS + "0"; }
                                bool FuncShift = (TelnetFuncKeyOther == 1);
                                switch (KeyName)
                                {
                                    case "UpArrow":
                                        Send(TerminalKeys["Up_" + CAS + "_" + GetTelnetKeyboardConf(0)]);
                                        break;
                                    case "DownArrow":
                                        Send(TerminalKeys["Down_" + CAS + "_" + GetTelnetKeyboardConf(0)]);
                                        break;
                                    case "LeftArrow":
                                        Send(TerminalKeys["Left_" + CAS + "_" + GetTelnetKeyboardConf(0)]);
                                        break;
                                    case "RightArrow":
                                        Send(TerminalKeys["Right_" + CAS + "_" + GetTelnetKeyboardConf(0)]);
                                        break;
                                    case "Insert":
                                        Send(TerminalKeys["Insert_" + CAS + "_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "Delete":
                                        Send(TerminalKeys["Delete_" + CAS + "_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "Home":
                                        Send(TerminalKeys["Home_" + CAS + "_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "End":
                                        Send(TerminalKeys["End_" + CAS + "_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "PageUp":
                                        Send(TerminalKeys["PageUp_" + CAS + "_" + GetTelnetKeyboardConf(3)]);
                                        break;
                                    case "PageDown":
                                        Send(TerminalKeys["PageDown_" + CAS + "_" + GetTelnetKeyboardConf(3)]);
                                        break;

                                    case "Escape": Send(TerminalKeys["Escape"]); break;

                                    case "Enter":
                                        Send(TerminalKeys["Enter_" + GetTelnetKeyboardConf(4)]);
                                        if (LocalEcho)
                                        {
                                            Monitor.Enter(LocalEchoBuf);
                                            LocalEchoBuf.Add(13);
                                            LocalEchoBuf.Add(10);
                                            Monitor.Exit(LocalEchoBuf);
                                        }
                                        break;


                                    case "F1":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F11_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F1_" + CAS + "_" + GetTelnetKeyboardConf(1)]); }
                                        }
                                        else
                                        {
                                            Send("00");
                                        }
                                        break;
                                    case "F2":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F12_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F2_" + CAS + "_" + GetTelnetKeyboardConf(1)]); }
                                        }
                                        else
                                        {
                                            Send("1B");
                                        }
                                        break;
                                    case "F3":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F13_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F3_" + CAS + "_" + GetTelnetKeyboardConf(1)]); }
                                        }
                                        else
                                        {
                                            Send("1C");
                                        }
                                        break;
                                    case "F4":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F14_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F4_" + CAS + "_" + GetTelnetKeyboardConf(1)]); }
                                        }
                                        else
                                        {
                                            Send("1D");
                                        }
                                        break;
                                    case "F5":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F15_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F5_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        else
                                        {
                                            Send("1E");
                                        }
                                        break;
                                    case "F6":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F16_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F6_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        else
                                        {
                                            Send("1F");
                                        }
                                        break;
                                    case "F7":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F17_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F7_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        break;
                                    case "F8":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F18_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F8_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        break;
                                    case "F9":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F19_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F9_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        break;
                                    case "F10":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { Send(TerminalKeys["F20_" + CAS + "_" + GetTelnetKeyboardConf(2)]); } else { Send(TerminalKeys["F10_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        break;
                                    case "F11":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { } else { Send(TerminalKeys["F11_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        else
                                        {
                                            Send(TerminalAnswerBack);
                                        }
                                        break;
                                    case "F12":
                                        if (TelnetFuncKeyOther < 2)
                                        {
                                            if (FuncShift) { } else { Send(TerminalKeys["F12_" + CAS + "_" + GetTelnetKeyboardConf(2)]); }
                                        }
                                        else
                                        {
                                            Send(TerminalAnswerBack);
                                        }
                                        break;

                                    case "F13": Send(TerminalKeys["F11_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F14": Send(TerminalKeys["F12_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F15": Send(TerminalKeys["F13_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F16": Send(TerminalKeys["F14_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F17": Send(TerminalKeys["F15_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F18": Send(TerminalKeys["F16_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F19": Send(TerminalKeys["F17_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F20": Send(TerminalKeys["F18_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F21": Send(TerminalKeys["F19_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F22": Send(TerminalKeys["F20_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                    case "F23": break;
                                    case "F24": break;


                                    case "Backspace":
                                        Send(TerminalKeys["Backspace_" + GetTelnetKeyboardConf(5)]);
                                        if (LocalEcho)
                                        {
                                            Monitor.Enter(LocalEchoBuf);
                                            LocalEchoBuf.Add(8);
                                            Monitor.Exit(LocalEchoBuf);
                                        }
                                        break;

                                    default:
                                        {
                                            if (ModCtrl)
                                            {
                                                switch (KeyCharI)
                                                {
                                                    case '2':
                                                    case ' ':
                                                    case '@':
                                                        Send("00");
                                                        break;
                                                    case 'a':
                                                    case 'A':
                                                        Send("01");
                                                        break;
                                                    case 'b':
                                                    case 'B':
                                                        Send("02");
                                                        break;
                                                    case 'c':
                                                    case 'C':
                                                        Send("03");
                                                        break;
                                                    case 'd':
                                                    case 'D':
                                                        Send("04");
                                                        break;
                                                    case 'e':
                                                    case 'E':
                                                        Send("05");
                                                        break;
                                                    case 'f':
                                                    case 'F':
                                                        Send("06");
                                                        break;
                                                    case 'g':
                                                    case 'G':
                                                        Send("07");
                                                        break;
                                                    case 'h':
                                                    case 'H':
                                                        Send("08");
                                                        break;
                                                    case 'i':
                                                    case 'I':
                                                        Send("09");
                                                        break;
                                                    case 'j':
                                                    case 'J':
                                                        Send("0A");
                                                        break;
                                                    case 'k':
                                                    case 'K':
                                                        Send("0B");
                                                        break;
                                                    case 'l':
                                                    case 'L':
                                                        Send("0C");
                                                        break;
                                                    case 'm':
                                                    case 'M':
                                                        Send("0D");
                                                        break;
                                                    case 'n':
                                                    case 'N':
                                                        Send("0E");
                                                        break;
                                                    case 'o':
                                                    case 'O':
                                                        Send("0F");
                                                        break;
                                                    case 'p':
                                                    case 'P':
                                                        Send("10");
                                                        break;
                                                    case 'q':
                                                    case 'Q':
                                                        Send("11");
                                                        break;
                                                    case 'r':
                                                    case 'R':
                                                        Send("12");
                                                        break;
                                                    case 's':
                                                    case 'S':
                                                        Send("13");
                                                        break;
                                                    case 't':
                                                    case 'T':
                                                        Send("14");
                                                        break;
                                                    case 'u':
                                                    case 'U':
                                                        Send("15");
                                                        break;
                                                    case 'v':
                                                    case 'V':
                                                        Send("16");
                                                        break;
                                                    case 'w':
                                                    case 'W':
                                                        Send("17");
                                                        break;
                                                    case 'x':
                                                    case 'X':
                                                        Send("18");
                                                        break;
                                                    case 'y':
                                                    case 'Y':
                                                        Send("19");
                                                        break;
                                                    case 'z':
                                                    case 'Z':
                                                        Send("1A");
                                                        break;
                                                    case '3':
                                                    case '[':
                                                    case '{':
                                                        Send("1B");
                                                        break;
                                                    case '4':
                                                    case '\\':
                                                    case '|':
                                                        Send("1C");
                                                        break;
                                                    case '5':
                                                    case ']':
                                                    case '}':
                                                        Send("1D");
                                                        break;
                                                    case '6':
                                                    case '~':
                                                    case '^':
                                                        Send("1E");
                                                        break;
                                                    case '7':
                                                    case '/':
                                                    case '_':
                                                        Send("1F");
                                                        break;
                                                    case '8':
                                                    case '?':
                                                        Send("7F");
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                bool StdKey = true;
                                                if (GetTelnetKeyboardConf(6) == "1")
                                                {
                                                    switch (KeyCharI)
                                                    {
                                                        case '0':
                                                        case '1':
                                                        case '2':
                                                        case '3':
                                                        case '4':
                                                        case '5':
                                                        case '6':
                                                        case '7':
                                                        case '8':
                                                        case '9':
                                                        case '.':
                                                        case ',':
                                                        case '-':
                                                        case '+':
                                                            StdKey = false;
                                                            if (Core_.CoreAnsi_.AnsiState_.__AnsiVT52)
                                                            {
                                                                Send(TerminalKeys["NumPad_" + KeyCharI + "_1"]);
                                                            }
                                                            else
                                                            {
                                                                Send(TerminalKeys["NumPad_" + KeyCharI + "_0"]);
                                                            }
                                                            break;
                                                    }
                                                }
                                                if (StdKey)
                                                {
                                                    if ((KeyCharI >= 32) || ((KeyCharI >= 1) && (KeyCharI <= 26) && (KeyCharI != 13)))
                                                    {
                                                        List<int> KeyCharI_ = new List<int>();
                                                        KeyCharI_.Add(KeyCharI);
                                                        if (LocalEcho)
                                                        {
                                                            Monitor.Enter(LocalEchoBuf);
                                                            byte[] KeyBytes = StrToBytes(KeyCharI_);
                                                            for (int i = 0; i < KeyBytes.Length; i++)
                                                            {
                                                                LocalEchoBuf.Add(KeyBytes[i]);
                                                            }
                                                            Monitor.Exit(LocalEchoBuf);
                                                        }
                                                        Send(KeyCharI_);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case WorkStateCDef.Toolbox:
                        {
                            switch (KeyName)
                            {
                                case "Backspace":
                                    TelnetClose(true);
                                    break;
                                default:
                                    string FuncKeyItem = KeyName + "|" + KeyChar.ToString();
                                    Monitor.Enter(FuncKeyBuf);
                                    FuncKeyBuf.Enqueue(FuncKeyItem);
                                    Monitor.Exit(FuncKeyBuf);
                                    break;
                            }
                        }
                        break;
                    case WorkStateCDef.EscapeKey:
                        EscapeKey = EscapeKeyId(KeyName, KeyChar);
                        WorkStateC = WorkStateCDef.Toolbox;
                        break;
                    case WorkStateCDef.DispConf:
                        {
                            DisplayConfig_.ProcessKey(KeyName, KeyChar);
                        }
                        break;
                }
                if (WorkStateC == WorkStateCDef.Session)
                {
                    Screen_.MouseActive(TelnetMouseWork);
                }
                else
                {
                    Screen_.MouseActive(false);
                }
                if (!Screen_.MouseIsActiveX)
                {
                    TelnetMouseReset();
                }
            }
            Monitor.Exit(TelnetMutex2);
        }


        Queue<string> FuncKeyBuf = new Queue<string>();

        void FuncKeyProcess()
        {
            Monitor.Enter(FuncKeyBuf);
            while (FuncKeyBuf.Count > 0)
            {
                string FuncKeyItem = FuncKeyBuf.Dequeue();
                Monitor.Exit(FuncKeyBuf);

                int P = FuncKeyItem.IndexOf('|');
                string KeyName = FuncKeyItem.Substring(0, P);
                char KeyChar = FuncKeyItem[P + 1];

                string CAS = "";
                if ((TelnetKeyboardMods[0] != '0')) { CAS = CAS + "1"; } else { CAS = CAS + "0"; }
                if ((TelnetKeyboardMods[1] != '0')) { CAS = CAS + "1"; } else { CAS = CAS + "0"; }
                if ((TelnetKeyboardMods[2] != '0')) { CAS = CAS + "1"; } else { CAS = CAS + "0"; }

                switch (KeyName)
                {
                    case "Tab":
                        TelnetInfoPos++;
                        if (TelnetInfoPos == 4)
                        {
                            TelnetInfoPos = 0;
                        }
                        break;
                    case "Enter":
                        {
                            WorkStateC = WorkStateCDef.EscapeKey;
                        }
                        break;
                    case "Escape":
                        {
                            WorkStateC = WorkStateCDef.Session;
                        }
                        break;

                    case "A": WorkStateC = WorkStateCDef.Session; Send("01"); break;
                    case "B": WorkStateC = WorkStateCDef.Session; Send("02"); break;
                    case "C": WorkStateC = WorkStateCDef.Session; Send("03"); break;
                    case "D": WorkStateC = WorkStateCDef.Session; Send("04"); break;
                    case "E": WorkStateC = WorkStateCDef.Session; Send("05"); break;
                    case "F": WorkStateC = WorkStateCDef.Session; Send("06"); break;
                    case "G": WorkStateC = WorkStateCDef.Session; Send("07"); break;
                    case "H": WorkStateC = WorkStateCDef.Session; Send("08"); break;
                    case "I": WorkStateC = WorkStateCDef.Session; Send("09"); break;
                    case "J": WorkStateC = WorkStateCDef.Session; Send("0A"); break;
                    case "K": WorkStateC = WorkStateCDef.Session; Send("0B"); break;
                    case "L": WorkStateC = WorkStateCDef.Session; Send("0C"); break;
                    case "M": WorkStateC = WorkStateCDef.Session; Send("0D"); break;
                    case "N": WorkStateC = WorkStateCDef.Session; Send("0E"); break;
                    case "O": WorkStateC = WorkStateCDef.Session; Send("0F"); break;
                    case "P": WorkStateC = WorkStateCDef.Session; Send("10"); break;
                    case "Q": WorkStateC = WorkStateCDef.Session; Send("11"); break;
                    case "R": WorkStateC = WorkStateCDef.Session; Send("12"); break;
                    case "S": WorkStateC = WorkStateCDef.Session; Send("13"); break;
                    case "T": WorkStateC = WorkStateCDef.Session; Send("14"); break;
                    case "U": WorkStateC = WorkStateCDef.Session; Send("15"); break;
                    case "V": WorkStateC = WorkStateCDef.Session; Send("16"); break;
                    case "W": WorkStateC = WorkStateCDef.Session; Send("17"); break;
                    case "X": WorkStateC = WorkStateCDef.Session; Send("18"); break;
                    case "Y": WorkStateC = WorkStateCDef.Session; Send("19"); break;
                    case "Z": WorkStateC = WorkStateCDef.Session; Send("1A"); break;

                    default:
                        {
                            switch (KeyChar)
                            {
                                case '=':
                                case ';':
                                case ':':
                                    TelnetFuncKeyOther++;
                                    if (TelnetFuncKeyOther == 3)
                                    {
                                        UseCtrlKeys = !UseCtrlKeys;
                                        TelnetFuncKeyOther = 0;
                                    }
                                    break;
                                case '1':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F1_" + CAS + "_" + GetTelnetKeyboardConf(1)]); break;
                                            case 1: Send(TerminalKeys["F11_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                        }
                                    }
                                    break;
                                case '2':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F2_" + CAS + "_" + GetTelnetKeyboardConf(1)]); break;
                                            case 1: Send(TerminalKeys["F12_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("00"); break;
                                        }
                                    }
                                    break;
                                case '3':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F3_" + CAS + "_" + GetTelnetKeyboardConf(1)]); break;
                                            case 1: Send(TerminalKeys["F13_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("1B"); break;
                                        }
                                    }
                                    break;
                                case '4':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F4_" + CAS + "_" + GetTelnetKeyboardConf(1)]); break;
                                            case 1: Send(TerminalKeys["F14_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("1C"); break;
                                        }
                                    }
                                    break;
                                case '5':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F5_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 1: Send(TerminalKeys["F15_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("1D"); break;
                                        }
                                    }
                                    break;
                                case '6':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F6_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 1: Send(TerminalKeys["F16_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("1E"); break;
                                        }
                                    }
                                    break;
                                case '7':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F7_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 1: Send(TerminalKeys["F17_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("1F"); break;
                                        }
                                    }
                                    break;
                                case '8':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F8_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 1: Send(TerminalKeys["F18_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 2: Send("7F"); break;
                                        }
                                    }
                                    break;
                                case '9':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F9_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 1: Send(TerminalKeys["F19_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                        }
                                    }
                                    break;
                                case '0':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        switch (TelnetFuncKeyOther)
                                        {
                                            case 0: Send(TerminalKeys["F10_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                            case 1: Send(TerminalKeys["F20_" + CAS + "_" + GetTelnetKeyboardConf(2)]); break;
                                        }
                                    }
                                    break;
                                case ',':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        Clipboard_.TextClipboardCopy(Core_.CoreAnsi_.AnsiState_.GetScreen(0, 0, Screen_.WinW - 1, Screen_.WinH - 1));
                                    }
                                    break;
                                case '.':
                                    {
                                        WorkStateC = WorkStateCDef.Session;
                                        Clipboard_.TextClipboardPaste();
                                    }
                                    break;
                                default:
                                    if (UseCtrlKeys)
                                    {
                                        switch (KeyChar)
                                        {
                                            case ' ':
                                            case '@':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("00");
                                                break;
                                            case '[':
                                            case '{':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("1B");
                                                break;
                                            case '\\':
                                            case '|':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("1C");
                                                break;
                                            case ']':
                                            case '}':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("1D");
                                                break;
                                            case '~':
                                            case '^':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("1E");
                                                break;
                                            case '/':
                                            case '_':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("1F");
                                                break;
                                            case '?':
                                                WorkStateC = WorkStateCDef.Session;
                                                Send("7F");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (KeyChar)
                                        {
                                            case '`':
                                                LocalEcho = !LocalEcho;
                                                break;
                                            case '|':
                                                Command_8bit = !Command_8bit;
                                                break;
                                            case '{':
                                                TelnetKeyboardConfMove();
                                                break;
                                            case '}':
                                                TelnetKeyboardConfStep();
                                                break;
                                            case '[':
                                                TelnetKeyboardModsMove();
                                                break;
                                            case ']':
                                                TelnetKeyboardModsStep();
                                                break;
                                            case '<':
                                                WorkStateC = WorkStateCDef.Session;
                                                if (TelnetFuncKeyOther < 2)
                                                {
                                                    Send(TerminalKeys["F11_" + CAS + "_" + GetTelnetKeyboardConf(2)]);
                                                }
                                                else
                                                {
                                                    Send(TerminalAnswerBack);
                                                }
                                                break;
                                            case '>':
                                                WorkStateC = WorkStateCDef.Session;
                                                if (TelnetFuncKeyOther < 2)
                                                {
                                                    Send(TerminalKeys["F12_" + CAS + "_" + GetTelnetKeyboardConf(2)]);
                                                }
                                                else
                                                {
                                                    Send(TerminalAnswerBack);
                                                }
                                                break;
                                            case '/':
                                                if (Conn.IsConnected() == 0)
                                                {
                                                    TelnetOpen();
                                                }
                                                else
                                                {
                                                    if (Conn.IsConnected() == 1)
                                                    {
                                                        TelnetClose(false);
                                                    }
                                                }
                                                break;
                                            case '?':
                                                {
                                                    WorkStateC = WorkStateCDef.Session;
                                                    TelnetSendWindowSize();
                                                }
                                                break;
                                            case '\\':
                                                {
                                                    DisplayConfigOpen();
                                                }
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }

                Monitor.Enter(FuncKeyBuf);
                if (WorkStateC == WorkStateCDef.Session)
                {
                    Screen_.MouseActive(TelnetMouseWork);
                }
                else
                {
                    Screen_.MouseActive(false);
                }
                if (!Screen_.MouseIsActiveX)
                {
                    TelnetMouseReset();
                }
            }
            Monitor.Exit(FuncKeyBuf);
        }

        bool TelnetSendWindowSize()
        {
            if ((Core_.CoreAnsi_.AnsiMaxX > 0) && (Core_.CoreAnsi_.AnsiMaxY > 0))
            {
                Conn.Resize(Core_.CoreAnsi_.AnsiMaxX, Core_.CoreAnsi_.AnsiMaxY);
                return true;
            }
            else
            {
                return false;
            }
        }

        public byte[] Receive(bool Str)
        {
            if (Conn == null)
            {
                return new byte[0];
            }

            Conn.MonitorEnter();
            using (MemoryStream ms = new MemoryStream())
            {
                if (LocalEchoBuf.Count > 0)
                {
                    Monitor.Enter(LocalEchoBuf);
                    ms.Write(LocalEchoBuf.ToArray(), 0, LocalEchoBuf.ToArray().Length);
                    LocalEchoBuf.Clear();
                    Monitor.Exit(LocalEchoBuf);
                }
                Conn.Receive(ms);
                byte[] SS = ms.ToArray();
                if (SS.Length > 0)
                {
                    if (ConsoleTestI) Console.Write("> ");
                }
                if (Str)
                {
                    for (int i = 0; i < SS.Length; i++)
                    {
                        ByteStr.Add(SS[i]);
                        if (SS[i] < 32) { SS[i] = (byte)'_'; }
                        if (SS[i] > 126) { SS[i] = (byte)'_'; }
                    }
                    if (ConsoleTestI) Console.Write(Encoding.UTF8.GetString(SS));
                }
                else
                {
                    for (int i = 0; i < SS.Length; i++)
                    {
                        ByteStr.Add(SS[i]);
                        if (ConsoleTestI) Console.Write(((int)SS[i]).ToString("X").PadLeft(2, '0'));
                    }
                }
                if (SS.Length > 0)
                {
                    if (ConsoleTestI) Console.WriteLine();
                }
                Conn.MonitorExit();
                return SS;
            }
        }

        public byte[] StrToBytes(List<int> STR)
        {
            return TerminalEncoding.GetBytes(TextWork.IntToStr(STR));
        }

        public void Send(List<int> STR)
        {
            byte[] Buf = StrToBytes(STR);
            string Buf_ = "";
            for (int i = 0; i < Buf.Length; i++)
            {
                Buf_ += UniConn.H(Buf[i]);
            }
            Send(Buf_);
        }

        public void Send(string STR)
        {
            if (Command_8bit)
            {
                for (int i = 0x40; i < 0x60; i++)
                {
                    STR = STR.Replace("##_" + ((char)i), (i + 0x40).ToString("X"));
                    STR = STR.Replace("##" + (i.ToString("X")), (i + 0x40).ToString("X"));
                }
            }
            else
            {
                STR = STR.Replace("##", "1B");
            }
            Conn.SendHex(STR);
        }

        public void ProcessBuf(Stopwatch TelnetTimer)
        {
            int ToSend = ByteStr.Count;

            // Avoid cutted UTF stream
            if (TerminalEncodingUTF)
            {
                if (TerminalEncodingUTF8)
                {
                    ToSend = TextWork.FullUTF8(ByteStr, ToSend);
                }
                else
                {
                    if (TerminalEncodingUTF16LE)
                    {
                        ToSend = TextWork.FullUTF16LE(ByteStr, ToSend);
                    }
                    else
                    {
                        if (TerminalEncodingUTF16BE)
                        {
                            ToSend = TextWork.FullUTF16BE(ByteStr, ToSend);
                        }
                        else
                        {
                            if (TerminalEncodingUTF32)
                            {
                                ToSend = TextWork.FullUTF32(ByteStr, ToSend);
                            }
                        }
                    }
                }
            }

            if (ToSend > 0)
            {
                string TempStr = TerminalEncoding.GetString(ByteStr.ToArray(), 0, ToSend);
                if (TelnetFileUse)
                {
                    Monitor.Enter(TelnetFileUseMtx);
                    TelnetFileW.Write(TextWork.TelnetTimerBegin);
                    TelnetFileW.Write(Encoding.UTF8.GetBytes((TelnetTimer.ElapsedMilliseconds / TelnetTimerResolution).ToString()));
                    TelnetFileW.Write(TextWork.TelnetTimerEnd);
                    TelnetFileW.Write(ByteStr.ToArray());
                    Monitor.Exit(TelnetFileUseMtx);
                }
                FileCtX.AddRange(TextWork.StrToInt(TempStr));

                ByteStr.RemoveRange(0, ToSend);
            }
        }

        bool OpenCloseRepaint = false;
        bool TerminalAutoSendWindowFlag = false;
        Stopwatch TerminalAutoSendWindowSW = new Stopwatch();

        public void TelnetOpen()
        {
            TerminalAutoSendWindowFlag = false;
            TerminalAutoSendWindowSW.Stop();
            TerminalAutoSendWindowSW.Reset();
            LocalEcho = false;
            Command_8bit = false;
            UseCtrlKeys = false;
            WindowTitle = "";
            WindowIcon = "";
            WindowTitleSt.Clear();
            WindowIconSt.Clear();

            int DefaultPort = 0;
            string[] AddrPort = Core_.CurrentFileName.Split(':');
            switch (TerminalConnection.ToUpperInvariant())
            {
                default:
                    Conn = new UniConnLoopback(TerminalEncoding);
                    break;
                case "RAW":
                    DefaultPort = 23;
                    Conn = new UniConnRaw(TerminalEncoding);
                    break;
                case "TELNET":
                    DefaultPort = 23;
                    Conn = new UniConnTelnet(TerminalEncoding);
                    break;
                case "SSH":
                    DefaultPort = 22;
                    Conn = new UniConnSSH(TerminalEncoding);
                    break;
                case "APPLICATION":
                    AddrPort = new string[2];
                    AddrPort[0] = Core_.CurrentFileName;
                    AddrPort[1] = "0";
                    Conn = new UniConnApp(TerminalEncoding);
                    break;
                case "SERIAL":
                    AddrPort = new string[2];
                    AddrPort[0] = Core_.CurrentFileName;
                    AddrPort[1] = "0";
                    Conn = new UniConnSerial(TerminalEncoding);
                    break;
            }
            try
            {
                if (AddrPort.Length >= 2)
                {
                    Conn.Open(AddrPort[0], int.Parse(AddrPort[1]), TerminalName, Core_.Screen_.WinW, Core_.Screen_.WinH);
                }
                else
                {
                    Conn.Open(AddrPort[0], DefaultPort, TerminalName, Core_.Screen_.WinW, Core_.Screen_.WinH);
                }
                if (TelnetAutoSendWindowSize > 0)
                {
                    TerminalAutoSendWindowFlag = true;
                    TerminalAutoSendWindowSW.Start();
                }
                TelnetMouseWork = true;
            }
            catch (Exception e)
            {
                TelnetMouseWork = false;
                Screen_.MouseReset();
                Conn = new UniConnLoopback(TerminalEncoding);
                Conn.Open("", 0, "", Core_.Screen_.WinW, Core_.Screen_.WinH);
                Conn.Send(TerminalEncoding.GetBytes(e.Message));
                Conn.Close();
            }
            if (Server_ != null)
            {
                Server_.Receive();
                Server_.Send(UniConn.HexToRaw(Server_.TerminalResetCommand));
            }
            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
            Monitor.Enter(TelnetMutex);
            Core_.CoreAnsi_.AnsiProcessReset(false, true, 0);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(1, 4);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(2, 2);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(3, 2);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(4, 2);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(5, 4);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(7, 4);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(10, 4);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(11, 4);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(12, 1);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(13, 4);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(14, 4);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(15, 4);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(16, 4);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(17, 4);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(18, 4);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(19, 4);
            Core_.CoreAnsi_.AnsiState_.AnsiParamSet(20, 2);
            Core_.CoreAnsi_.AnsiState_.DecParamSet(9, 2);
            Core_.CoreAnsi_.AnsiState_.DecParamSet(1000, 2);
            Core_.CoreAnsi_.AnsiState_.DecParamSet(1001, 2);
            Core_.CoreAnsi_.AnsiState_.DecParamSet(1002, 2);
            Core_.CoreAnsi_.AnsiState_.DecParamSet(1003, 2);
            Core_.CoreAnsi_.AnsiState_.DecParamSet(1004, 2);
            Core_.CoreAnsi_.AnsiState_.DecParamSet(1005, 2);
            Core_.CoreAnsi_.AnsiState_.DecParamSet(1006, 2);
            Core_.CoreAnsi_.AnsiState_.DecParamSet(1015, 2);
            Core_.CoreAnsi_.AnsiState_.DecParamSet(1016, 2);
            Monitor.Exit(TelnetMutex);
            OpenCloseRepaint = true;
        }

        void TelnetFileRestart()
        {
            if (TelnetFileUse)
            {
                Monitor.Enter(TelnetFileUseMtx);
                try
                {
                    TelnetFileW.Close();
                }
                catch
                {
                }
                try
                {
                    TelnetFileS.Close();
                }
                catch
                {
                }
                try
                {
                    TelnetFileS = new FileStream(TelnetFileName, FileMode.Append, FileAccess.Write);
                    try
                    {
                        TelnetFileW = new BinaryWriter(TelnetFileS);
                    }
                    catch
                    {
                    }
                }
                catch
                {
                }
                Monitor.Exit(TelnetFileUseMtx);
            }
        }

        void TelnetFileClose()
        {
            if (TelnetFileUse)
            {
                Monitor.Enter(TelnetFileUseMtx);
                try
                {
                    TelnetFileW.Close();
                }
                catch
                {

                }
                try
                {
                    TelnetFileS.Close();
                }
                catch
                {

                }
                Monitor.Exit(TelnetFileUseMtx);
            }
        }

        public void TelnetClose(bool StopApp)
        {
            if (!StopApp)
            {
                TelnetMouseWork = false;
                Screen_.MouseReset();
                Screen_.MouseActive(false);
            }
            if (!Screen_.MouseIsActiveX)
            {
                TelnetMouseReset();
            }
            if (Conn != null)
            {
                if (Conn.IsConnected() != 0)
                {
                    Conn.Close();
                }
            }
            if (StopApp)
            {
                TelnetFileClose();
                Screen_.CloseApp(Core_.TextNormalBack, Core_.TextNormalFore);
            }
            else
            {
                OpenCloseRepaint = true;
            }
        }

        Screen Screen_;
        Core Core_;

        public TelnetClient(Core Core__, ConfigFile CF)
        {
            Core_ = Core__;
            Screen_ = Core__.Screen_;
            Core_.CoreAnsi_.__AnsiProcessDelayFactor = CF.ParamGetI("FileDelayFrame");

            ServerPort = CF.ParamGetI("ServerPort");
            ServerTelnet = CF.ParamGetB("ServerTelnet");
            ServerEncoding = TextWork.EncodingFromName(CF.ParamGetS("ServerEncoding"));

            ByteStr = new List<byte>();

            LocalEcho = false;
            Command_8bit = false;
            UseCtrlKeys = false;
            LocalEchoBuf.Clear();

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


            WindowTitle = "";
            WindowIcon = "";
            WindowTitleSt.Clear();
            WindowIconSt.Clear();

            TerminalConnection = CF.ParamGetS("TerminalConnection");

            TerminalName = CF.ParamGetS("TerminalName");
            TerminalType = CF.ParamGetI("TerminalType");
            if (TerminalType < 0)
            {
                TerminalType = 1;
            }
            string TerminalAnswerBack0 = CF.ParamGetS("TerminalAnswerBack");
            TerminalAnswerBack = "";
            for (int i = 0; i < TerminalAnswerBack0.Length; i++)
            {
                TerminalAnswerBack = TerminalAnswerBack + "_" + TerminalAnswerBack0[i].ToString();
            }

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
                if (Allowed.Contains(TelnetKeyboardConf_[i].ToString()))
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
        }

        public void TelnetClientStart()
        {
            Screen_.MultiThread = true;

            DisplayConfig_ = new DisplayConfig(Core_);
            DisplayConfig_.ConfigRepaint += DisplayConfigRepaint;
            DisplayConfig_.ConfigClose += DisplayConfigClose;

            Clipboard_ = new Clipboard(Core_);
            Clipboard_.TextClipboardPasteEvent += TextPasteWork;

            Thread Thr = new Thread(TelnetClientWork);
            Thr.Start();
        }

        public void TelnetClientWork()
        {
            EscapeKeyMessage();

            if (ServerPort > 0)
            {
                Server_ = new Server();
                if (!Server_.Start(ServerPort, ServerTelnet))
                {
                    Server_ = null;
                }
            }
            else
            {
                Server_ = null;
            }

            WorkStateC = WorkStateCDef.InfoScreen;
            EscapeKey = "";
            while (Screen_.AppWorking && ("".Equals(EscapeKey)))
            {
                Thread.Sleep(100);
            }
            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
            Screen_.SetCursorPosition(0, 0);
            WorkStateC = WorkStateCDef.Session;

            FileCtX = new List<int>();
            TelnetOpen();
            WorkStateCDef WorkState_ = WorkStateCDef.Session;
            string ToRepaintState = "";
            int ConnStatusX = Conn.IsConnected();

            TelnetFileUse = false;
            if (!("".Equals(TelnetFileName)))
            {
                Monitor.Enter(TelnetFileUseMtx);
                try
                {
                    TelnetFileS = new FileStream(TelnetFileName, FileMode.Create, FileAccess.Write);
                    TelnetFileW = new BinaryWriter(TelnetFileS);
                    TelnetFileUse = true;
                }
                catch
                {
                    TelnetFileUse = false;
                }
                Monitor.Exit(TelnetFileUseMtx);
            }

            Stopwatch TelnetTimer = new Stopwatch();
            FileCtX.Clear();
            TelnetTimer.Start();
            long TelnetTimerNext = TelnetTimerResolution;
            int TelnetTimerWait = (int)(TelnetTimerResolution / 10L);
            if (TelnetTimerWait < 1)
            {
                TelnetTimerWait = 1;
            }
            try
            {
                while (Screen_.AppWorking)
                {
                    if (TerminalAutoSendWindowFlag)
                    {
                        if (TerminalAutoSendWindowSW.ElapsedMilliseconds > TelnetAutoSendWindowSize)
                        {
                            if (TelnetSendWindowSize())
                            {
                                TerminalAutoSendWindowSW.Stop();
                                TerminalAutoSendWindowFlag = false;
                            }
                        }
                    }
                    while (TelnetTimer.ElapsedMilliseconds > TelnetTimerNext)
                    {
                        TelnetTimerNext += TelnetTimerResolution;
                    }
                    //TelnetTimerNext = ((TelnetTimer.ElapsedMilliseconds / TelnetTimerResolution) * TelnetTimerResolution) + TelnetTimerResolution;
                    while (TelnetTimer.ElapsedMilliseconds <= TelnetTimerNext)
                    {
                        Thread.Sleep(TelnetTimerWait);
                    }
                    if (ConnStatusX != Conn.IsConnected())
                    {
                        if (WorkStateC == WorkStateCDef.Toolbox)
                        {
                            WorkState_ = WorkStateCDef.Toolbox_;
                        }
                        ConnStatusX = Conn.IsConnected();
                        if (Conn.IsConnected() == 1)
                        {
                            Screen_.MouseActive(TelnetMouseWork);
                        }
                        else
                        {
                            TelnetMouseWork = false;
                            Screen_.MouseReset();
                            Screen_.MouseActive(false);
                        }
                        if (!Screen_.MouseIsActiveX)
                        {
                            TelnetMouseReset();
                        }
                    }
                    Receive(false);
                    ProcessBuf(TelnetTimer);
                    bool WasAnsiProcess = false;

                    Core_.CoreAnsi_.AnsiProcessSupply(FileCtX);
                    FileCtX.Clear();

                    Monitor.Enter(TelnetMutex);
                    int AnsiBufferI1 = Core_.CoreAnsi_.AnsiState_.AnsiBufferI;
                    WasAnsiProcess = (Core_.CoreAnsi_.AnsiProcess(TerminalStep) != 0);
                    int AnsiBufferI2 = Core_.CoreAnsi_.AnsiState_.AnsiBufferI;
                    Monitor.Exit(TelnetMutex);
                    if (Server_ != null)
                    {
                        byte[] ServerBuf = Server_.Receive();
                        if ((ServerBuf.Length > 0) && (Conn.IsConnected() == 1))
                        {
                            /*for (int I = 0; I < ServerBuf.Length; I++)
                            {
                                Console.Write(ServerBuf[I] + "_");
                            }
                            Console.WriteLine();*/
                            string DataStr = ServerEncoding.GetString(ServerBuf);
                            Conn.Send(TerminalEncoding.GetBytes(DataStr));
                        }
                    }

                    if (WasAnsiProcess)
                    {
                        if (Screen_.MouseIsActive != TelnetMouseWork)
                        {
                            if (WorkStateC == WorkStateCDef.Session)
                            {
                                Screen_.MouseActive(TelnetMouseWork);
                            }
                            else
                            {
                                Screen_.MouseActive(false);
                            }
                            if (!Screen_.MouseIsActiveX)
                            {
                                TelnetMouseReset();
                            }
                        }


                        for (int i = 0; i < Core_.CoreAnsi_.__AnsiResponse.Count; i++)
                        {
                            TelnetReport(Core_.CoreAnsi_.__AnsiResponse[i]);
                        }
                        Core_.CoreAnsi_.__AnsiResponse.Clear();

                        if ((Server_ != null) && (AnsiBufferI2 > AnsiBufferI1))
                        {
                            Server_.Send(ServerEncoding.GetBytes(TextWork.IntToStr(Core_.CoreAnsi_.AnsiBuffer.GetRange(AnsiBufferI1, AnsiBufferI2 - AnsiBufferI1))));
                        }


                        if (Core_.CoreAnsi_.AnsiGetFontSize(Core_.CoreAnsi_.AnsiState_.__AnsiY) > 0)
                        {
                            Screen_.SetCursorPosition(Core_.CoreAnsi_.AnsiState_.__AnsiX * 2, Core_.CoreAnsi_.AnsiState_.__AnsiY);
                        }
                        else
                        {
                            Screen_.SetCursorPosition(Core_.CoreAnsi_.AnsiState_.__AnsiX, Core_.CoreAnsi_.AnsiState_.__AnsiY);
                        }
                        if (WorkStateC == WorkStateCDef.Toolbox)
                        {
                            WorkState_ = WorkStateCDef.Toolbox_;
                        }
                        if (WorkStateC == WorkStateCDef.DispConf)
                        {
                            DisplayConfig_.DisplayMenu();
                        }
                    }

                    if (WorkStateC == WorkStateCDef.DispConf)
                    {
                        if (DisplayConfig_RequestReapint)
                        {
                            Core_.CoreAnsi_.AnsiRepaint(false);
                            DisplayConfig_RequestReapint = false;
                        }
                        if (DisplayConfig_RequestMenu)
                        {
                            DisplayConfig_.DisplayMenu();
                            DisplayConfig_RequestMenu = false;
                        }
                        if (DisplayConfig_RequestClose)
                        {
                            TelnetInfoPos = DisplayConfig_.MenuPos;
                            WorkStateC = WorkStateCDef.Toolbox;
                            TelnetDisplayInfo(true);
                            DisplayConfig_RequestClose = false;
                        }
                    }
                    if (OpenCloseRepaint)
                    {
                        if (WorkStateC == WorkStateCDef.Toolbox)
                        {
                            WorkState_ = WorkStateCDef.Toolbox_;
                        }
                    }
                    if ((WorkStateC == WorkStateCDef.Toolbox) || (WorkStateC == WorkStateCDef.EscapeKey))
                    {
                        string ToRepaintStateX = TelnetInfoPos.ToString() + "_" + TelnetKeyboardConfI.ToString() + "_" + TelnetKeyboardConf;
                        ToRepaintStateX = ToRepaintStateX + "_" + TelnetKeyboardModsI + "_" + TelnetKeyboardMods + "_" + TelnetFuncKeyOther;
                        ToRepaintStateX = ToRepaintStateX + "_" + LocalEcho + "_" + Command_8bit + "_" + UseCtrlKeys;
                        ToRepaintStateX = ToRepaintStateX + "_|" + WindowTitle + "|_";
                        ToRepaintStateX = ToRepaintStateX + "_|" + WindowIcon + "|_";

                        if ((WorkState_ != WorkStateC) || (ToRepaintState != ToRepaintStateX))
                        {
                            bool NeedRepaint = (ToRepaintState != ToRepaintStateX);
                            NeedRepaint = NeedRepaint | (WorkState_ != WorkStateCDef.Toolbox_);
                            NeedRepaint = NeedRepaint | OpenCloseRepaint;
                            OpenCloseRepaint = false;
                            TelnetDisplayInfo(NeedRepaint);
                            WorkState_ = WorkStateC;
                            ToRepaintState = ToRepaintStateX;
                        }
                    }
                    if ((WorkStateC == WorkStateCDef.Session) && (WorkState_ != WorkStateC))
                    {
                        Core_.CoreAnsi_.AnsiRepaint(false);
                        if (Core_.CoreAnsi_.AnsiGetFontSize(Core_.CoreAnsi_.AnsiState_.__AnsiY) > 0)
                        {
                            Screen_.SetCursorPosition(Core_.CoreAnsi_.AnsiState_.__AnsiX * 2, Core_.CoreAnsi_.AnsiState_.__AnsiY);
                        }
                        else
                        {
                            Screen_.SetCursorPosition(Core_.CoreAnsi_.AnsiState_.__AnsiX, Core_.CoreAnsi_.AnsiState_.__AnsiY);
                        }
                        WorkState_ = WorkStateC;
                    }
                    FuncKeyProcess();
                }
                TelnetMouseWork = false;
                Screen_.MouseReset();
                Screen_.MouseActive(false);
                TelnetFileClose();
            }
            finally
            {
                TelnetMouseWork = false;
                Screen_.MouseReset();
                Screen_.MouseActive(false);
                TelnetFileClose();
            }
            if (!Screen_.MouseIsActiveX)
            {
                TelnetMouseReset();
            }

            if (Server_ != null)
            {
                Server_.Stop();
            }
            Screen_.CloseApp(Core_.TextNormalBack, Core_.TextNormalFore);
        }

        void TelnetDisplayInfo(bool NeedRepaint)
        {
            if (NeedRepaint)
            {
                Core_.CoreAnsi_.AnsiRepaint(false);
            }
            int CB_ = Core_.PopupBack;
            int CF_ = Core_.PopupFore;

            List<string> InfoMsg = new List<string>();
            int IsConn = Conn.IsConnected();
            if (WorkStateC == WorkStateCDef.Toolbox)
            {
                string StatusInfo = "Unknown";
                switch (IsConn)
                {
                    case 0: StatusInfo = "Disconnected"; break;
                    case 1: StatusInfo = "Connected"; break;
                    case 2: StatusInfo = "Connecting"; break;
                    case 3: StatusInfo = "Disconnecting"; break;
                }
                InfoMsg.Add(" Status: " + StatusInfo + " ");
                if ((WindowTitle != "") || (WindowIcon != ""))
                {
                    if (WindowTitle.Equals(WindowIcon))
                    {
                        InfoMsg.Add(" Title: " + WindowTitle + " ");
                    }
                    else
                    {
                        if (WindowTitle.Equals("") || WindowIcon.Equals(""))
                        {
                            InfoMsg.Add(" Title: " + (WindowIcon + WindowTitle) + " ");
                        }
                        else
                        {
                            InfoMsg.Add(" Title: " + (WindowIcon + " " + WindowTitle) + " ");
                        }
                    }
                }
                InfoMsg.Add(" Screen size: " + Core_.CoreAnsi_.AnsiMaxX + "x" + Core_.CoreAnsi_.AnsiMaxY + " ");
                InfoMsg.Add(" Escape key: " + EscapeKey + " ");
                InfoMsg.Add(" Esc - Return to terminal ");
                InfoMsg.Add(" Enter - Change escape key ");
                InfoMsg.Add(" Tab - Move info ");
                InfoMsg.Add(" Backspace - Quit ");
                switch (TelnetFuncKeyOther)
                {
                    case 0:
                        InfoMsg.Add(" 1-0 - Send F1-F10                  ");
                        InfoMsg.Add(" < > - Send F11, F12                ");
                        break;
                    case 1:
                        InfoMsg.Add(" 1-0 - Send F11-F20                 ");
                        InfoMsg.Add(" < > - Send F11, F12                ");
                        break;
                    case 2:
                        InfoMsg.Add(" 2-8 - Send NUL,ESC,FS,GS,RS,US,DEL ");
                        InfoMsg.Add(" < > - Send AnswerBack              ");
                        break;
                }
                InfoMsg.Add(" = ; : - Change control code ");
                InfoMsg.Add(" Letter - Send CTRL+letter ");
                string TelnetKeyboardConf_ = "";
                switch (TelnetKeyboardConfI.ToString() + TelnetKeyboardConf[TelnetKeyboardConfI].ToString())
                {
                    default: TelnetKeyboardConf_ = (TelnetKeyboardConfI.ToString() + TelnetKeyboardConf[TelnetKeyboardConfI].ToString()); break;
                    case "00": TelnetKeyboardConf_ = "Cursor - Normal"; break;
                    case "01": TelnetKeyboardConf_ = "Cursor - Appli"; break;
                    case "10": TelnetKeyboardConf_ = "F1-F4 - Normal"; break;
                    case "11": TelnetKeyboardConf_ = "F1-F4 - Alter"; break;
                    case "20": TelnetKeyboardConf_ = "F5-F12 - Normal"; break;
                    case "21": TelnetKeyboardConf_ = "F5-F12 - Alter"; break;
                    case "30": TelnetKeyboardConf_ = "Editing - DEC"; break;
                    case "31": TelnetKeyboardConf_ = "Editing - IBM 1"; break;
                    case "32": TelnetKeyboardConf_ = "Editing - IBM 2"; break;
                    case "40": TelnetKeyboardConf_ = "Enter - CR"; break;
                    case "41": TelnetKeyboardConf_ = "Enter - CR+LF"; break;
                    case "42": TelnetKeyboardConf_ = "Enter - LF"; break;
                    case "50": TelnetKeyboardConf_ = "Backspace - DEL"; break;
                    case "51": TelnetKeyboardConf_ = "Backspace - BS"; break;
                    case "60": TelnetKeyboardConf_ = "Numpad - Normal"; break;
                    case "61": TelnetKeyboardConf_ = "Numpad - Appli"; break;
                }


                string TelnetKeyboardMods_ = "";
                switch (TelnetKeyboardModsI.ToString() + TelnetKeyboardMods[TelnetKeyboardModsI].ToString())
                {
                    default: TelnetKeyboardMods_ = (TelnetKeyboardModsI.ToString() + TelnetKeyboardMods[TelnetKeyboardModsI].ToString()); break;
                    case "00": TelnetKeyboardMods_ = "Ctrl - None"; break;
                    case "01": TelnetKeyboardMods_ = "Ctrl - Force"; break;
                    case "10": TelnetKeyboardMods_ = "Alt - None"; break;
                    case "11": TelnetKeyboardMods_ = "Alt - Force"; break;
                    case "20": TelnetKeyboardMods_ = "Shift - None"; break;
                    case "21": TelnetKeyboardMods_ = "Shift - Force"; break;
                }

                if (UseCtrlKeys)
                {
                    InfoMsg.Add(" @ - Send NUL");
                    InfoMsg.Add(" [ { - Send ESC");
                    InfoMsg.Add(" \\ | - Send FS");
                    InfoMsg.Add(" ] } - Send GS");
                    InfoMsg.Add(" ~ ^ - Send RS");
                    InfoMsg.Add(" / _ - Send US");
                    InfoMsg.Add(" ? - Send DEL");
                }
                else
                {
                    InfoMsg.Add(" { } - Key codes: " + TelnetKeyboardConf_ + " ");
                    InfoMsg.Add(" [ ] - Modifiers: " + TelnetKeyboardMods_ + " ");
                    InfoMsg.Add(" / - Connect/disconnect ");
                    InfoMsg.Add(" ? - Send screen size ");
                    InfoMsg.Add(" ` - Local echo: " + (LocalEcho ? "on" : "off"));
                    InfoMsg.Add(" | - Input commands: " + (Command_8bit ? "8-bit" : "7-bit"));
                    InfoMsg.Add(" \\ - Display configuration");
                }
                InfoMsg.Add(" , - Copy screen as text");
                InfoMsg.Add(" . - Paste text as keystrokes");
            }
            if (WorkStateC == WorkStateCDef.EscapeKey)
            {
                InfoMsg.Add(" Press key, which will be ");
                InfoMsg.Add(" used as new escape key   ");
            }

            int InfoW = 0;
            int InfoH = InfoMsg.Count;

            List<int>[] InfoMsg_ = new List<int>[InfoH];
            for (int i = 0; i < InfoH; i++)
            {
                InfoMsg_[i] = TextWork.StrToInt(InfoMsg[i]);
                InfoW = Math.Max(InfoW, InfoMsg_[i].Count);
            }
            for (int i = 0; i < InfoH; i++)
            {
                while (InfoMsg_[i].Count < InfoW)
                {
                    InfoMsg_[i].Add(32);
                }
            }



            int OffsetX = 0;
            int OffsetY = 0;
            int InfoCX = InfoW;
            int InfoCY = InfoH;
            if ((TelnetInfoPos == 1) || (TelnetInfoPos == 3))
            {
                OffsetX = Screen_.WinW - InfoW;
                InfoCX = OffsetX - 1;
            }
            if ((TelnetInfoPos == 2) || (TelnetInfoPos == 3))
            {
                OffsetY = Screen_.WinH - InfoH;
                InfoCY = OffsetY - 1;
            }


            for (int i = -1; i < InfoW + 1; i++)
            {
                Screen_.PutChar(OffsetX + i, OffsetY + InfoH, ' ', CF_, CB_);
                Screen_.PutChar(OffsetX + i, OffsetY - 1, ' ', CF_, CB_);
            }
            for (int i = 0; i < InfoH; i++)
            {
                Screen_.PutChar(OffsetX + InfoW, OffsetY + i, ' ', CF_, CB_);
                Screen_.PutChar(OffsetX - 1, OffsetY + i, ' ', CF_, CB_);
            }

            for (int I = 0; I < InfoH; I++)
            {
                for (int II = 0; II < InfoW; II++)
                {
                    Screen_.PutChar(OffsetX + II, OffsetY + I, InfoMsg_[I][II], CB_, CF_);
                }
            }
            Screen_.SetCursorPosition(InfoCX, InfoCY);
        }


        void TextPasteWork(string Raw)
        {
            Conn.Send(TerminalEncoding.GetBytes(Raw));
        }


        int TelnetInfoPos = 0;

        string TerminalConnection = "";


        string TerminalName = "";

        string TerminalAnswerBack = "";

        // 0 - VT100
        // 1 - VT102
        // 2 - VT220
        // 3 - VT320
        // 4 - VT420
        // 5 - VT520
        int TerminalType = 1;

        string TelnetReportStrToStr(string N)
        {
            List<int> N_ = TextWork.StrToInt(N);
            string S = "";
            for (int i = 0; i < N_.Count; i++)
            {
                string Chr = TextWork.CharCode(N_[i], 0);
                if (Chr.EndsWith("??"))
                {
                    Chr = "_ ";
                }
                S = S + Chr;
            }
            return S;
        }

        string TelnetReportNumToStr(int N)
        {
            string N_ = N.ToString();
            string S = "";
            for (int i = 0; i < N_.Length; i++)
            {
                S = S + "_" + N_[i];
            }
            return S;
        }

        void TelnetReport(string ReportRequest)
        {
            bool SendAnswer = (TerminalType < 10);
            switch (ReportRequest)
            {
                default:
                    {
                        if (ReportRequest.StartsWith("Mouse;"))
                        {
                            string[] ParamStr = ReportRequest.Split(';');

                            switch (ParamStr[1])
                            {
                                case "0":
                                    Screen_.MouseSet(int.Parse(ParamStr[2]), false);
                                    break;
                                case "1":
                                    Screen_.MouseSet(int.Parse(ParamStr[2]), true);
                                    break;
                                case "2":
                                    if (int.Parse(ParamStr[2]) > 0)
                                    {
                                        MouseHighlight = true;
                                        MouseHighlightX = int.Parse(ParamStr[3]);
                                        MouseHighlightY = int.Parse(ParamStr[4]);
                                        MouseHighlightFirst = int.Parse(ParamStr[5]);
                                        MouseHighlightLast = int.Parse(ParamStr[6]);
                                    }
                                    else
                                    {
                                        MouseHighlight = false;
                                    }
                                    break;
                            }

                        }
                        if (ReportRequest.StartsWith("WindowTitle"))
                        {
                            WindowTitle = ReportRequest.Substring(11);
                        }
                        if (ReportRequest.StartsWith("WindowIcon"))
                        {
                            WindowIcon = ReportRequest.Substring(10);
                        }
                        if (ReportRequest.StartsWith("[") && ReportRequest.EndsWith("*y"))
                        {
                            string[] ParamStr = ReportRequest.Substring(1, ReportRequest.Length - 3).Split(';');
                            int N = 0;
                            string Resp = "##_P" + TelnetReportStrToStr(ParamStr[0]) + "_!_~";
                            Resp = Resp + TelnetReportStrToStr(TextWork.CharCode(N, 1));
                            Resp = Resp + "##_\\";
                            if (SendAnswer)
                            {
                                Send(Resp);
                            }
                        }
                        if (ReportRequest.StartsWith("$q"))
                        {
                            if (SendAnswer)
                            {
                                Send("##_P_0_$_r##_\\");
                            }
                        }
                        if (ReportRequest.StartsWith("[?") && ReportRequest.EndsWith("$p"))
                        {
                            int N = int.Parse(ReportRequest.Substring(2, ReportRequest.Length - 4));
                            int V = Core_.CoreAnsi_.AnsiState_.DecParamGet(N);
                            switch (N)
                            {
                                case 1:
                                    V = (GetTelnetKeyboardConf(0) == "1") ? 1 : 2;
                                    break;
                                case 3:
                                    {
                                        V = 0;
                                        if (Core_.Screen_.WinW == 80) V = 2;
                                        if (Core_.Screen_.WinW == 132) V = 1;
                                    }
                                    break;
                                case 4:
                                    V = Core_.CoreAnsi_.AnsiState_.__AnsiSmoothScroll ? 1 : 2;
                                    break;
                                case 5:
                                    {
                                        V = 2;
                                        if (Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__.CountLines() > 0)
                                        {
                                            if (Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__.CountItems(0) > 0)
                                            {
                                                Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__.Get(0, 0);
                                                if (Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__.Item_ColorA >= 128)
                                                {
                                                    V = 1;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case 6:
                                    V = Core_.CoreAnsi_.AnsiState_.__AnsiOrigin ? 1 : 2;
                                    break;
                                case 7:
                                    V = Core_.CoreAnsi_.AnsiState_.__AnsiNoWrap ? 2 : 1;
                                    break;
                                case 47:
                                case 1047:
                                case 1049:
                                    V = Core_.CoreAnsi_.AnsiState_.IsScreenAlternate ? 1 : 2;
                                    break;
                                case 66:
                                    V = (GetTelnetKeyboardConf(6) == "1") ? 1 : 2;
                                    break;
                                case 67:
                                    V = (GetTelnetKeyboardConf(5) == "1") ? 1 : 2;
                                    break;
                                case 69:
                                    V = Core_.CoreAnsi_.AnsiState_.__AnsiMarginLeftRight ? 1 : 2;
                                    break;
                                case 95:
                                    V = Core_.CoreAnsi_.AnsiState_.DECCOLMPreserve ? 1 : 2;
                                    break;
                            }
                            if (SendAnswer)
                            {
                                Send("##_[_?" + TelnetReportNumToStr(N) + "_;_" + V.ToString() + "_$_y");
                            }
                        }
                        else
                        {
                            if (ReportRequest.StartsWith("[") && ReportRequest.EndsWith("$p"))
                            {
                                int N = int.Parse(ReportRequest.Substring(1, ReportRequest.Length - 3));
                                int V = Core_.CoreAnsi_.AnsiState_.AnsiParamGet(N);
                                switch (N)
                                {
                                    case 12:
                                        V = LocalEcho ? 2 : 1;
                                        break;
                                    case 20:
                                        V = (GetTelnetKeyboardConf(4) == "1") ? 1 : 2;
                                        break;
                                }
                                if (SendAnswer)
                                {
                                    Send("##_[" + TelnetReportNumToStr(N) + "_;_" + V.ToString() + "_$_y");
                                }
                            }
                        }
                    }
                    break;
                case "[11t":
                    if (SendAnswer)
                    {
                        Send("##_[_1_t");
                    }
                    break;
                case "[13t":
                case "[13;2t":
                    if (SendAnswer)
                    {
                        Send("##_[_3_;_1_;_1_t");
                    }
                    break;
                case "[14t":
                case "[14;2t":
                    if (SendAnswer)
                    {
                        Send("##_[_4_;" + TelnetReportNumToStr(Core_.Screen_.WinH * Screen.TerminalCellH) + "_;" + TelnetReportNumToStr(Core_.Screen_.WinW * Screen.TerminalCellW) + "_t");
                    }
                    break;
                case "[15t":
                    if (SendAnswer)
                    {
                        Send("##_[_5_;" + TelnetReportNumToStr(Core_.Screen_.WinH * Screen.TerminalCellH) + "_;" + TelnetReportNumToStr(Core_.Screen_.WinW * Screen.TerminalCellW) + "_t");
                    }
                    break;
                case "[16t":
                    if (SendAnswer)
                    {
                        Send("##_[_6_;" + TelnetReportNumToStr(Screen.TerminalCellH) + "_;" + TelnetReportNumToStr(Screen.TerminalCellW) + "_t");
                    }
                    break;
                case "[18t":
                    if (SendAnswer)
                    {
                        Send("##_[_8_;" + TelnetReportNumToStr(Core_.Screen_.WinH) + "_;" + TelnetReportNumToStr(Core_.Screen_.WinW) + "_t");
                    }
                    break;
                case "[19t":
                    if (SendAnswer)
                    {
                        Send("##_[_9_;" + TelnetReportNumToStr(Core_.Screen_.WinH) + "_;" + TelnetReportNumToStr(Core_.Screen_.WinW) + "_t");
                    }
                    break;
                case "[20t":
                    if (SendAnswer)
                    {
                        Send("##_[_L" + TelnetReportStrToStr(WindowIcon) + "##_\\");
                    }
                    break;
                case "[21t":
                    if (SendAnswer)
                    {
                        Send("##_[_l" + TelnetReportStrToStr(WindowTitle) + "##_\\");
                    }
                    break;
                case "[22;0t":
                    {
                        WindowTitleSt.Push(WindowTitle);
                        WindowIconSt.Push(WindowIcon);
                    }
                    break;
                case "[22;1t":
                    {
                        WindowIconSt.Push(WindowIcon);
                    }
                    break;
                case "[22;2t":
                    {
                        WindowTitleSt.Push(WindowTitle);
                    }
                    break;
                case "[23;0t":
                    {
                        if (WindowTitleSt.Count > 0)
                        {
                            WindowTitle = WindowTitleSt.Pop();
                        }
                        else
                        {
                            WindowTitle = "";
                        }
                        if (WindowIconSt.Count > 0)
                        {
                            WindowIcon = WindowIconSt.Pop();
                        }
                        else
                        {
                            WindowIcon = "";
                        }
                    }
                    break;
                case "[23;1t":
                    {
                        if (WindowIconSt.Count > 0)
                        {
                            WindowIcon = WindowIconSt.Pop();
                        }
                        else
                        {
                            WindowIcon = "";
                        }
                    }
                    break;
                case "[23;2t":
                    {
                        if (WindowTitleSt.Count > 0)
                        {
                            WindowTitle = WindowTitleSt.Pop();
                        }
                        else
                        {
                            WindowTitle = "";
                        }
                    }
                    break;
                case "[?6n": // DECXCPR
                    if (SendAnswer)
                    {
                        Send("##_[_?" + TelnetReportNumToStr(Core_.CoreAnsi_.ReportCursorY()) + "_;" + TelnetReportNumToStr(Core_.CoreAnsi_.ReportCursorX()) + "_;_1_R");
                    }
                    break;
                case "[?15n": // Printer
                    if (SendAnswer)
                    {
                        Send("##_[_?_1_3_n");
                    }
                    break;
                case "[?25n": // UDK
                    if (SendAnswer)
                    {
                        Send("##_[_?_2_0_n");
                    }
                    break;
                case "[?26n": // Keyboard
                    if (SendAnswer)
                    {
                        Send("##_[_?_2_7_;_1_;_0_;_0_n");
                    }
                    break;
                case "[?53n": // Locator
                    if (SendAnswer)
                    {
                        Send("##_[_?_5_3_n");
                    }
                    break;
                case "[?62n": // DECMSR
                    if (SendAnswer)
                    {
                        Send("##_[_0_0_0_0_*_{");
                    }
                    break;
                case "[?63;1n": // DECCKSR
                    if (SendAnswer)
                    {
                        Send("##_P_1_!_~_0_0_0_0##_\\");
                    }
                    break;
                case "[?75n": // Data integrity
                    if (SendAnswer)
                    {
                        Send("##_[_?_7_0_n");
                    }
                    break;
                case "[?85n": // Multi-session
                    if (SendAnswer)
                    {
                        Send("##_[_?_8_3_n");
                    }
                    break;
                case "$q\"p": // DCS / DECSCL
                    {
                        string Bit = Command_8bit ? "_0" : "_1";
                        switch (TerminalType)
                        {
                            case 0:
                                Send("##_P_1_$_r_6_1_;" + Bit + "_\"_p##_\\");
                                break;
                            case 1:
                                Send("##_P_1_$_r_6_1_;" + Bit + "_\"_p##_\\");
                                break;
                            case 2:
                                Send("##_P_1_$_r_6_2_;" + Bit + "_\"_p##_\\");
                                break;
                            case 3:
                                Send("##_P_1_$_r_6_3_;" + Bit + "_\"_p##_\\");
                                break;
                            case 4:
                                Send("##_P_1_$_r_6_4_;" + Bit + "_\"_p##_\\");
                                break;
                            case 5:
                                Send("##_P_1_$_r_6_5_;" + Bit + "_\"_p##_\\");
                                break;
                        }
                    }
                    break;
                case "$q*|": // DCS / DECSNLS
                    if (SendAnswer)
                    {
                        Send("##_P_1_$_r" + TelnetReportNumToStr(Core_.CoreAnsi_.AnsiMaxY) + "_*_|##_\\");
                    }
                    break;
                case "$qr": // DCS / DECSTBM
                    if (SendAnswer)
                    {
                        Send("##_P_1_$_r" + TelnetReportNumToStr(Core_.CoreAnsi_.AnsiState_.__AnsiScrollFirst + 1) + "_;" + TelnetReportNumToStr(Core_.CoreAnsi_.AnsiState_.__AnsiScrollLast + 1) + "_r##_\\");
                    }
                    break;
                case "$qs": // DCS / DECSLRM
                    if (SendAnswer)
                    {
                        Send("##_P_1_$_r" + TelnetReportNumToStr(Core_.CoreAnsi_.AnsiState_.__AnsiMarginLeft + 1) + "_;" + TelnetReportNumToStr(Core_.CoreAnsi_.AnsiState_.__AnsiMarginRight) + "_s##_\\");
                    }
                    break;
                case "[0c": // Primary DA
                    {
                        switch (TerminalType)
                        {
                            case 0:
                                Send("##_[_?_1_;_2_c");
                                break;
                            case 1:
                                Send("##_[_?_6_c");
                                break;
                            case 2:
                                Send("##_[_?_6_2_;_1_;_2_;_6_;_7_;_8_;_9_c");
                                break;
                            case 3:
                                Send("##_[_?_6_3_;_1_;_2_;_6_;_7_;_8_;_9_c");
                                break;
                            case 4:
                                Send("##_[_?_6_4_;_1_;_2_;_6_;_7_;_8_;_9_;_1_5_;_1_8_;_2_1_c");
                                break;
                            case 5:
                                Send("##_[_?_6_5_;_1_;_2_;_6_;_7_;_8_;_9_;_1_5_;_1_8_;_2_1_c");
                                break;
                        }
                    }
                    break;
                case "[>c": // Secondary DA
                case "[>0c": // Secondary DA
                    {
                        switch (TerminalType)
                        {
                            case 0:
                                Send("##_[_>_0_;_1_0_;_0_c");
                                break;
                            case 1:
                                Send("##_[_>_0_;_1_0_;_0_c");
                                break;
                            case 2:
                                Send("##_[_>_1_;_1_0_;_0_c");
                                break;
                            case 3:
                                Send("##_[_>_2_4_;_1_0_;_0_c");
                                break;
                            case 4:
                                Send("##_[_>_4_1_;_1_0_;_0_c");
                                break;
                            case 5:
                                Send("##_[_>_6_4_;_1_0_;_0_c");
                                break;
                        }
                    }
                    break;
                case "[=c": // Tertiary DA
                case "[=0c": // Tertiary DA
                    switch (TerminalType)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            Send("##_P_!_|_0_0_0_0_0_0_0_0##_\\");
                            break;
                    }
                    break;
                case "[6n": // DSR / CPR
                    if (SendAnswer)
                    {
                        Send("##_[" + TelnetReportNumToStr(Core_.CoreAnsi_.ReportCursorY()) + "_;" + TelnetReportNumToStr(Core_.CoreAnsi_.ReportCursorX()) + "_R");
                    }
                    break;
                case "VT52:Z":
                    if (SendAnswer)
                    {
                        Send("1B_/_Z");
                    }
                    break;
                case "[5n": // DSR
                    if (SendAnswer)
                    {
                        Send("##_[_0_n");
                    }
                    break;
                case "[0x": // DECREQTPARM
                    if (SendAnswer)
                    {
                        Send("##_[_2_;_1_;_1_;_1_1_2_;_1_1_2_;_1_;_0_x");
                    }
                    break;
                case "[1x": // DECREQTPARM
                    if (SendAnswer)
                    {
                        Send("##_[_3_;_1_;_1_;_1_1_2_;_1_1_2_;_1_;_0_x");
                    }
                    break;

                case "AnswerBack":
                    if (SendAnswer)
                    {
                        Send(TerminalAnswerBack);
                    }
                    break;

                case "Control8bit_1":
                    Command_8bit = true;
                    break;
                case "Control8bit_0":
                    Command_8bit = false;
                    break;
                case "LocalEcho_1":
                    LocalEcho = true;
                    break;
                case "LocalEcho_0":
                    LocalEcho = false;
                    break;
                case "CursorKey_1":
                    SetTelnetKeyboardConf(0, 1);
                    break;
                case "CursorKey_0":
                    SetTelnetKeyboardConf(0, 0);
                    break;
                case "EnterKey_1":
                    SetTelnetKeyboardConf(4, 1);
                    break;
                case "EnterKey_0":
                    SetTelnetKeyboardConf(4, 0);
                    break;
                case "NumpadKey_1":
                    SetTelnetKeyboardConf(6, 1);
                    break;
                case "NumpadKey_0":
                    SetTelnetKeyboardConf(6, 0);
                    break;
                case "BackspaceKey_1":
                    SetTelnetKeyboardConf(5, 1);
                    break;
                case "BackspaceKey_0":
                    SetTelnetKeyboardConf(5, 0);
                    break;
            }
        }

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
            if (Core_.CoreAnsi_.AnsiState_.__AnsiVT52 && (N <= 3))
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


        bool DisplayConfigReload = false;

        bool DisplayConfig_RequestReapint = false;
        bool DisplayConfig_RequestMenu = false;
        bool DisplayConfig_RequestClose = false;

        DisplayConfig DisplayConfig_;

        void DisplayConfigRepaint(bool AnsiReload, bool Menu)
        {
            DisplayConfig_RequestReapint = true;
            if (Menu)
            {
                DisplayConfig_RequestMenu = true;
            }
        }

        void DisplayConfigOpen()
        {
            DisplayConfig_.MenuPos = TelnetInfoPos;
            WorkStateC = WorkStateCDef.DispConf;
            DisplayConfig_.Open();
            DisplayConfigRepaint(false, true);
        }

        void DisplayConfigClose(bool WindowClose, int NewW, int NewH)
        {
            if ((NewW > 0) && (NewH >= 0))
            {
                Core_.AppResize(NewW, NewH, false);
                DisplayConfigRepaint(true, false);
            }
            else
            {
                DisplayConfigRepaint(false, false);
            }
            DisplayConfig_RequestClose = true;
        }
    }
}
