using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TextPaint
{
    public class TelnetServer
    {
        object StatusMutex = new object();
        object TelnetMutex = new object();

        enum WorkStateSDef { None, InfoScreen, WaitForKey, FileOpen, FileOpenWait, DisplayPlayFwd, DisplayPlayRev, DisplayPause, DisplayFwd, DisplayRev, DisplayInfo, DispConf };
        WorkStateSDef WorkStateS = WorkStateSDef.InfoScreen;

        Server Server_ = new Server();
        int ServerPort = 0;
        bool ServerTelnet = false;
        Encoding ServerEncoding;

        int FileDelayTime = 100;
        int FileDelayStep = 1;
        int FileDelayOffset = 0;

        Clipboard Clipboard_;
        List<int> FileCtX;

        string ANSIBrowseWildcard;

        string ScreenWelcomeBuf = "";

        void Screen_WriteText(string Text)
        {
            ScreenWelcomeBuf = ScreenWelcomeBuf + Text;
            //Screen_.WriteText(Text, Core_.TextNormalBack, Core_.TextNormalFore);
        }

        void Screen_WriteLine()
        {
            ScreenWelcomeBuf = ScreenWelcomeBuf + "\n";
            //Screen_.WriteLine();
        }

        void Screen_BackText()
        {
            ScreenWelcomeBuf = ScreenWelcomeBuf.Substring(0, ScreenWelcomeBuf.Length - 1);
        }

        void Screen_Clear()
        {
            ScreenWelcomeBuf = "";
        }

        void Screen_Refresh()
        {
            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
            Screen_.SetCursorPositionNoRefresh(0, 0);
            string[] BufL = ScreenWelcomeBuf.Split('\n');
            for (int i = 0; i < BufL.Length; i++)
            {
                if (i > 0) Screen_.WriteLine();
                Screen_.WriteText(BufL[i], Core_.TextNormalBack, Core_.TextNormalFore);
            }
            Screen_.Refresh();
        }

        public void CoreEvent_Server(string KeyName, char KeyChar, int KeyCharI, bool ModShift, bool ModCtrl, bool ModAlt)
        {
            switch (KeyName)
            {
                case "Resize":
                    {
                        Monitor.Enter(TelnetMutex);
                        Core_.CoreAnsi_.AnsiTerminalResize(Core_.Screen_.WinW, Core_.Screen_.WinH);
                        Monitor.Exit(TelnetMutex);
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.WaitForKey:
                            case WorkStateSDef.DisplayInfo:
                                Screen_Refresh();
                                break;
                            case WorkStateSDef.DispConf:
                                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                break;
                        }
                    }
                    break;
                case "Space":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.DisplayFwd:
                            case WorkStateSDef.DisplayRev:
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                            case WorkStateSDef.DisplayPause:
                                {
                                    ActionRequest = 5;
                                }
                                break;
                            case WorkStateSDef.DisplayInfo:
                                ExitInfo();
                                break;
                        }
                    }
                    break;
                case "Backspace":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.WaitForKey:
                                {
                                    if (NewFileName.Count > 0)
                                    {
                                        NewFileName.RemoveAt(NewFileName.Count - 1);
                                        Screen_BackText();
                                    }
                                    Screen_Refresh();
                                }
                                break;
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                            case WorkStateSDef.DisplayPause:
                                {
                                    DisplayPaused = !DisplayPaused;
                                    if (!DisplayPaused)
                                    {
                                        WorkSeekAdvance = FileDelayStep__;
                                        WorkStateS = WorkStateSDef.DisplayRev;
                                        FileTimerOffset = FileTimer.ElapsedMilliseconds;
                                    }
                                }
                                break;
                            case WorkStateSDef.DisplayInfo:
                                ExitInfo();
                                break;
                        }
                    }
                    break;
                case "Enter":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.WaitForKey:
                                {
                                    Screen_Clear();
                                    FileListPrepare();
                                    WorkStateS = WorkStateSDef.FileOpen;
                                }
                                break;
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                            case WorkStateSDef.DisplayPause:
                                {
                                    DisplayPaused = !DisplayPaused;
                                    if (!DisplayPaused)
                                    {
                                        WorkSeekAdvance = FileDelayStep__;
                                        WorkStateS = WorkStateSDef.DisplayFwd;
                                        FileTimerOffset = FileTimer.ElapsedMilliseconds;
                                    }
                                }
                                break;
                            case WorkStateSDef.DisplayInfo:
                                ExitInfo();
                                break;
                            case WorkStateSDef.DispConf:
                                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                break;
                        }
                    }
                    break;
                case "Tab":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.WaitForKey:
                                {
                                    Screen_.CloseApp(Core_.TextNormalBack, Core_.TextNormalFore);
                                    WorkStateS = WorkStateSDef.None;
                                }
                                break;
                            case WorkStateSDef.DisplayPause:
                            case WorkStateSDef.DisplayFwd:
                            case WorkStateSDef.DisplayRev:
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                                {
                                    if (Monitor.TryEnter(StatusMutex))
                                    {
                                        DisplayStatus++;
                                        if ((DisplayStatus % 3) == 0)
                                        {
                                            DisplayStatus -= 3;
                                        }
                                        ForceRepaint = true;
                                        Monitor.Exit(StatusMutex);
                                    }
                                }
                                break;
                            case WorkStateSDef.DispConf:
                                {
                                    DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                }
                                break;
                        }
                    }
                    break;
                case "UpArrow":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.DisplayPause:
                            case WorkStateSDef.DisplayFwd:
                            case WorkStateSDef.DisplayRev:
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                                ActionRequestParam = -1;
                                ActionRequest = 4;
                                break;
                            case WorkStateSDef.DisplayInfo:
                                InfoPosV--;
                                DisplayInfoRepaint = true;
                                break;
                            case WorkStateSDef.DispConf:
                                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                break;
                        }
                    }
                    break;
                case "DownArrow":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.DisplayPause:
                            case WorkStateSDef.DisplayFwd:
                            case WorkStateSDef.DisplayRev:
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                                ActionRequestParam = 1;
                                ActionRequest = 4;
                                break;
                            case WorkStateSDef.DisplayInfo:
                                InfoPosV++;
                                DisplayInfoRepaint = true;
                                break;
                            case WorkStateSDef.DispConf:
                                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                break;
                        }
                    }
                    break;
                case "LeftArrow":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.DisplayPause:
                            case WorkStateSDef.DisplayFwd:
                            case WorkStateSDef.DisplayRev:
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                                ActionRequestParam = -2;
                                ActionRequest = 4;
                                break;
                            case WorkStateSDef.DisplayInfo:
                                InfoPosH--;
                                DisplayInfoRepaint = true;
                                break;
                            case WorkStateSDef.DispConf:
                                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                break;
                        }
                    }
                    break;
                case "RightArrow":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.DisplayPause:
                            case WorkStateSDef.DisplayFwd:
                            case WorkStateSDef.DisplayRev:
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                                ActionRequestParam = 2;
                                ActionRequest = 4;
                                break;
                            case WorkStateSDef.DisplayInfo:
                                InfoPosH++;
                                DisplayInfoRepaint = true;
                                break;
                            case WorkStateSDef.DispConf:
                                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                break;
                        }
                    }
                    break;
                case "Home":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.DisplayPause:
                            case WorkStateSDef.DisplayFwd:
                            case WorkStateSDef.DisplayRev:
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                                ActionRequestParam = -3;
                                ActionRequest = 4;
                                break;
                            case WorkStateSDef.DispConf:
                                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                break;
                        }
                    }
                    break;
                case "End":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.DisplayPause:
                            case WorkStateSDef.DisplayFwd:
                            case WorkStateSDef.DisplayRev:
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                                ActionRequestParam = 3;
                                ActionRequest = 4;
                                break;
                            case WorkStateSDef.DispConf:
                                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                break;
                        }
                    }
                    break;
                case "WindowClose":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.WaitForKey:
                            case WorkStateSDef.DisplayFwd:
                            case WorkStateSDef.DisplayRev:
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                            case WorkStateSDef.DisplayPause:
                            case WorkStateSDef.DisplayInfo:
                                {
                                    Screen_.AppWorking = false;
                                    WorkStateS = WorkStateSDef.None;
                                }
                                break;
                            case WorkStateSDef.DispConf:
                                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                break;
                        }
                    }
                    break;
                case "Escape":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.DisplayInfo:
                                {
                                    ExitInfo();
                                }
                                break;
                            case WorkStateSDef.DisplayFwd:
                            case WorkStateSDef.DisplayRev:
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                            case WorkStateSDef.DisplayPause:
                                {
                                    ActionRequest = 2;
                                }
                                break;
                            case WorkStateSDef.DispConf:
                                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                break;
                        }
                    }
                    break;
                case "PageUp":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.DisplayFwd:
                            case WorkStateSDef.DisplayRev:
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                            case WorkStateSDef.DisplayPause:
                                {
                                    if (PlaylistFiles.Count > 1)
                                    {
                                        if (PlaylistFiles[PlaylistIdx].StartsWith(CoreStatic.GetFileListExtraInfo, StringComparison.Ordinal))
                                        {
                                            PlaylistFiles.RemoveAt(PlaylistIdx);
                                            if (PlaylistIdx > 0)
                                            {
                                                PlaylistIdx--;
                                            }
                                            else
                                            {
                                                PlaylistIdx = PlaylistFiles.Count - 1;
                                            }
                                        }

                                        if (PlaylistIdx > 0)
                                        {
                                            PlaylistIdx--;
                                        }
                                        else
                                        {
                                            PlaylistIdx = PlaylistFiles.Count - 1;
                                        }
                                    }
                                    Core_.CurrentFileName = PlaylistFiles[PlaylistIdx];
                                    ActionRequest = 1;
                                }
                                break;
                            case WorkStateSDef.DispConf:
                                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                break;
                        }
                    }
                    break;
                case "PageDown":
                    {
                        switch (WorkStateS)
                        {
                            case WorkStateSDef.DisplayFwd:
                            case WorkStateSDef.DisplayRev:
                            case WorkStateSDef.DisplayPlayFwd:
                            case WorkStateSDef.DisplayPlayRev:
                            case WorkStateSDef.DisplayPause:
                                {
                                    if (PlaylistFiles.Count > 1)
                                    {
                                        if (PlaylistFiles[PlaylistIdx].StartsWith(CoreStatic.GetFileListExtraInfo, StringComparison.Ordinal))
                                        {
                                            PlaylistFiles.RemoveAt(PlaylistIdx);
                                        }
                                        else
                                        {
                                            PlaylistIdx++;
                                            if (PlaylistIdx == PlaylistFiles.Count)
                                            {
                                                PlaylistIdx = 0;
                                            }
                                        }
                                    }
                                    Core_.CurrentFileName = PlaylistFiles[PlaylistIdx];
                                    ActionRequest = 1;
                                }
                                break;
                            case WorkStateSDef.DispConf:
                                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                                break;
                        }
                    }
                    break;
                default:
                    switch (WorkStateS)
                    {
                        case WorkStateSDef.WaitForKey:
                            if (KeyChar >= 32)
                            {
                                NewFileName.Add(KeyCharI);
                                Screen_WriteText(TextWork.IntToStr(KeyCharI));
                            }
                            Screen_Refresh();
                            break;
                        case WorkStateSDef.DisplayPause:
                        case WorkStateSDef.DisplayFwd:
                        case WorkStateSDef.DisplayRev:
                        case WorkStateSDef.DisplayPlayFwd:
                        case WorkStateSDef.DisplayPlayRev:
                            {
                                switch (KeyChar)
                                {
                                    case ']':
                                        FileDelayStepFactor++;
                                        if ((FileDelayStep__ * 2) < 1)
                                        {
                                            FileDelayStepFactor--;
                                        }
                                        ActionRequest = 3;
                                        break;
                                    case '[':
                                        if (FileDelayStep__ > 1)
                                        {
                                            FileDelayStepFactor--;
                                        }
                                        ActionRequest = 3;
                                        break;
                                    case '/':
                                        FileDelayStepFactor = 0;
                                        ActionRequest = 3;
                                        break;
                                    case '`':
                                        {
                                            if (Monitor.TryEnter(StatusMutex))
                                            {
                                                if ((DisplayStatus < 12))
                                                {
                                                    DisplayStatus += 3;
                                                }
                                                else
                                                {
                                                    DisplayStatus -= 12;
                                                }
                                                ForceRepaint = true;
                                                Monitor.Exit(StatusMutex);
                                            }
                                        }
                                        break;
                                    case '\\':
                                        {
                                            ActionRequest = 6;
                                        }
                                        break;
                                    case ',':
                                        {
                                            ActionRequest = 7;
                                        }
                                        break;
                                }
                            }
                            break;
                        case WorkStateSDef.DispConf:
                            DisplayConfig_.ProcessKey(KeyName, KeyChar);
                            break;
                    }
                    break;
            }
        }




        public bool ForceRepaint = false;
        long FileTimerOffset = 0;
        long FileDelayTimeL = 0;
        Stopwatch FileTimer = new Stopwatch();
        int ActionRequestParam = 0;
        int ActionRequest = 0;
        int WorkSeekAdvance = 1;
        bool WorkSeekOneChar = false;

        int MoviePos = 0;
        int MovieLength = 0;

        int FileDelayStep__ = 1;
        int FileDelayStepFactor = 0;

        List<string> PlaylistFiles = new List<string>();
        int PlaylistIdx = 0;

        void FileListPrepare()
        {
            DisplayStatus_ = -1;
            string NewFileNameS = CoreStatic.PrepareFileName(NewFileName);
            if (!("".Equals(NewFileNameS)))
            {
                Core_.CurrentFileName = NewFileNameS;
            }
            PlaylistFiles = CoreStatic.GetFileList(Core_.CurrentFileName, ANSIBrowseWildcard);
            PlaylistIdx = CoreStatic.GetFileListIdx;
        }

        AnsiSauce AnsiSauce_ = new AnsiSauce();

        void FileOpenCalc()
        {
            Monitor.Enter(TelnetMutex);
            Core_.CoreAnsi_.AnsiTerminalResize(Core_.Screen_.WinW, Core_.Screen_.WinH);

            try
            {
                Core_.CoreAnsi_.AnsiProcessReset(true, false, 1);
                Core_.CoreAnsi_.AnsiProcessSupply(FileCtX);
                MovieLength = Core_.CoreAnsi_.AnsiProcess(-1);
            }
            catch (Exception E)
            {
                FileCtX = TextWork.StrToInt(E.Message);
                Core_.CoreAnsi_.AnsiProcessReset(true, false, 1);
                Core_.CoreAnsi_.AnsiProcessSupply(FileCtX);
                MovieLength = Core_.CoreAnsi_.AnsiProcess(-1);
            }
            AnsiSauce_.NonSauceInfo("Index", (PlaylistIdx + 1).ToString() + "/" + PlaylistFiles.Count.ToString());
            AnsiSauce_.NonSauceInfo("File name", Path.GetFileName(Core_.CurrentFileName));
            AnsiSauce_.NonSauceInfo("Steps", MovieLength);
            AnsiSauce_.NonSauceInfo("Characters", Core_.CoreAnsi_.AnsiState_.AnsiBufferI);
            AnsiSauce_.NonSauceInfo("Overwrites/writes", Core_.CoreAnsi_.AnsiState_.PrintCharCounterOver + "/" + Core_.CoreAnsi_.AnsiState_.PrintCharCounter);
            AnsiSauce_.NonSauceInfo("Inserts and deletes", Core_.CoreAnsi_.AnsiState_.PrintCharInsDel);
            AnsiSauce_.NonSauceInfo("Scrolls", Core_.CoreAnsi_.AnsiState_.PrintCharScroll);

            AnsiSauce_.CreateInfo();
            MoviePos = 0;
            Core_.CoreAnsi_.AnsiProcessReset(true, true, 2);
            Core_.CoreAnsi_.AnsiProcessSupply(FileCtX);


            Monitor.Exit(TelnetMutex);
            DisplayStatusText(new List<int>());
            FileTimerOffset = FileTimer.ElapsedMilliseconds;

            if (Server_ != null)
            {
                Server_.Send(UniConn.HexToRaw(Server_.TerminalResetCommand));
            }

            if (FileDelayOffset < 0)
            {
                WorkSeekAdvance = MovieLength + FileDelayOffset;
                if (WorkSeekAdvance < 0)
                {
                    WorkSeekAdvance = 0;
                }
            }
            else
            {
                WorkSeekAdvance = FileDelayOffset;
                if (WorkSeekAdvance > MovieLength)
                {
                    WorkSeekAdvance = MovieLength;
                }
            }
            WorkStateS = WorkStateSDef.DisplayPlayFwd;
            DisplayPaused = true;
        }

        Screen Screen_;
        Core Core_;

        public TelnetServer(Core Core__, ConfigFile CF)
        {
            Core_ = Core__;
            Screen_ = Core__.Screen_;
            Core_.CoreAnsi_.__AnsiProcessDelayFactor = CF.ParamGetI("FileDelayFrame");

            ServerPort = CF.ParamGetI("ServerPort");
            ServerTelnet = CF.ParamGetB("ServerTelnet");
            ServerEncoding = TextWork.EncodingFromName(CF.ParamGetS("ServerEncoding"));

            ANSIBrowseWildcard = CF.ParamGetS("FileBrowseWildcard");
            if (ANSIBrowseWildcard == "")
            {
                ANSIBrowseWildcard = "*";
            }

            FileDelayTime = CF.ParamGetI("FileDelayTime");
            FileDelayStep = CF.ParamGetI("FileDelayStep");
            FileDelayOffset = CF.ParamGetI("FileDelayOffset");

            if (FileDelayTime < 0)
            {
                FileDelayTime = 0;
            }
            if (FileDelayStep <= 0)
            {
                FileDelayStep = 1;
            }
        }

        List<int> DispBuffer___;

        public void TelnetServerStart()
        {
            Clipboard_ = new Clipboard(Core_);
            Screen_.MultiThread = true;

            DisplayConfig_ = new DisplayConfig(Core_);
            DisplayConfig_.ConfigRepaint += DisplayConfigRepaint;
            DisplayConfig_.ConfigClose += DisplayConfigClose;

            Thread Thr = new Thread(ServerFile);
            Thr.Start();
        }

        public void ServerFile()
        {
            WorkStateS = WorkStateSDef.InfoScreen;
            List<int> DispBuffer = new List<int>();
            int DispBufferLen = 1000;
            int FilePos = 0;
            List<int> PX = new List<int>();
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
            byte[] DummyBuf = new byte[1024];
            FileTimer.Start();
            int FileDelayTime_ = FileDelayTime / 5;
            FileDelayTimeL = FileDelayTime;
            ActionRequest = 0;
            FileDelayStep__ = FileDelayStep;
            while (Screen_.AppWorking)
            {
                if (ActionRequest > 0)
                {
                    switch (ActionRequest)
                    {
                        case 1:
                            WorkStateS = WorkStateSDef.FileOpen;
                            break;
                        case 2:
                            WorkStateS = WorkStateSDef.InfoScreen;
                            break;
                        case 3:
                            {
                                FileDelayStep__ = FileDelayStep;
                                int T = FileDelayStepFactor;
                                while (T > 0)
                                {
                                    FileDelayStep__ = FileDelayStep__ * 2;
                                    T--;
                                }
                                while (T < 0)
                                {
                                    FileDelayStep__ = FileDelayStep__ / 2;
                                    T++;
                                }
                                if (FileDelayStep__ < 1)
                                {
                                    FileDelayStep__ = 1;
                                }
                                if (!WorkSeekOneChar)
                                {
                                    WorkSeekAdvance = FileDelayStep__;
                                }
                                ForceRepaint = true;
                            }
                            break;
                        case 4:
                            {
                                switch (WorkStateS)
                                {
                                    case WorkStateSDef.DisplayFwd:
                                    case WorkStateSDef.DisplayRev:
                                    case WorkStateSDef.DisplayPlayFwd:
                                    case WorkStateSDef.DisplayPlayRev:
                                    case WorkStateSDef.DisplayPause:
                                        {
                                            DisplayPaused = true;
                                            switch (ActionRequestParam)
                                            {
                                                case -1:
                                                case 1:
                                                    WorkSeekAdvance = 1;
                                                    WorkSeekOneChar = true;
                                                    break;
                                                case -2:
                                                case 2:
                                                    WorkSeekAdvance = FileDelayStep__;
                                                    break;
                                                case -3:
                                                    WorkSeekAdvance = MovieLength;
                                                    break;
                                                case 3:
                                                    WorkSeekAdvance = MovieLength - MoviePos;
                                                    break;
                                            }
                                            if (ActionRequestParam > 0)
                                            {
                                                WorkStateS = WorkStateSDef.DisplayFwd;
                                            }
                                            if (ActionRequestParam < 0)
                                            {
                                                WorkStateS = WorkStateSDef.DisplayRev;
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                        case 5:
                            {
                                DisplayPaused = true;
                                ForceRepaint = true;
                                InfoPosH = 0;
                                InfoPosV = 0;
                                WorkStateS = WorkStateSDef.DisplayInfo;
                                DisplayInfo(true);
                            }
                            break;
                        case 6:
                            {
                                DispBuffer___ = DispBuffer;
                                DisplayConfigOpen();
                            }
                            break;
                        case 7:
                            {
                                Clipboard_.TextClipboardCopy(Core_.CoreAnsi_.AnsiState_.GetScreen(0, 0, Screen_.WinW - 1, Screen_.WinH - 1));
                            }
                            break;
                    }
                    ActionRequest = 0;
                }

                if (ForceRepaint)
                {
                    switch (WorkStateS)
                    {
                        case WorkStateSDef.DisplayPause:
                        case WorkStateSDef.DisplayFwd:
                        case WorkStateSDef.DisplayRev:
                        case WorkStateSDef.DisplayPlayFwd:
                        case WorkStateSDef.DisplayPlayRev:
                            Core_.CoreAnsi_.AnsiRepaint(false);
                            DisplayStatusText(DispBuffer);
                            break;
                    }
                    ForceRepaint = false;
                }
                switch (WorkStateS)
                {
                    case WorkStateSDef.InfoScreen: // Information screen before file opening
                        {
                            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
                            Screen_.SetCursorPositionNoRefresh(0, 0);
                            Screen_Clear();
                            NewFileName.Clear();
                            Screen_WriteText("During displaying:");
                            Screen_WriteLine();
                            Screen_WriteText("Esc - Return to this screen");
                            Screen_WriteLine();
                            Screen_WriteText("Tab - Show/hide status");
                            Screen_WriteLine();
                            Screen_WriteText("` - Change status information");
                            Screen_WriteLine();
                            Screen_WriteText("Space - File information");
                            Screen_WriteLine();
                            Screen_WriteText("Enter - Play/stop forward");
                            Screen_WriteLine();
                            Screen_WriteText("Backspace - Play/stop backward");
                            Screen_WriteLine();
                            Screen_WriteText("Up/Down arrow - Move 1 step");
                            Screen_WriteLine();
                            Screen_WriteText("Left/Right arrow - Move " + FileDelayStep__.ToString() + " steps");
                            Screen_WriteLine();
                            Screen_WriteText("Home - Move to begin of animation");
                            Screen_WriteLine();
                            Screen_WriteText("End - Move to end of animation");
                            Screen_WriteLine();
                            Screen_WriteText("] - Increase playing speed");
                            Screen_WriteLine();
                            Screen_WriteText("[ - Decrease playing speed");
                            Screen_WriteLine();
                            Screen_WriteText("/ - Reset playing speed");
                            Screen_WriteLine();
                            Screen_WriteText("Page Up/Page Down - Previous/Next file");
                            Screen_WriteLine();
                            Screen_WriteText(", - Copy screen to clipboard");
                            Screen_WriteLine();
                            Screen_WriteText("\\ - Display configuration");
                            Screen_WriteLine();
                            Screen_WriteLine();
                            Screen_WriteLine();
                            Screen_WriteText("Press Enter to start displaying or Tab to quit application.");
                            Screen_WriteLine();
                            Screen_Refresh();
                            WorkStateS = WorkStateSDef.WaitForKey;
                        }
                        break;
                    case WorkStateSDef.WaitForKey: // Waiting for user key press before opening file
                        {
                            Thread.Sleep(100);
                        }
                        break;
                    case WorkStateSDef.FileOpen: // Opening file
                        {
                            WorkSeekOneChar = false;
                            Screen_Clear();
                            Screen_Refresh();
                            DispBuffer.Clear();

                            AnsiSauce_.Info.Clear();
                            try
                            {
                                FileStream FS;
                                if (PlaylistFiles[PlaylistIdx].StartsWith(CoreStatic.GetFileListExtraInfo, StringComparison.Ordinal))
                                {
                                    FS = new FileStream(PlaylistFiles[PlaylistIdx].Substring(CoreStatic.GetFileListExtraInfo.Length), FileMode.Open, FileAccess.Read);
                                }
                                else
                                {
                                    FS = new FileStream(PlaylistFiles[PlaylistIdx], FileMode.Open, FileAccess.Read);
                                }
                                StreamReader SR;
                                if ("".Equals(Core_.CoreFile_.FileREnc))
                                {
                                    SR = new StreamReader(FS);
                                }
                                else
                                {
                                    SR = new StreamReader(FS, TextWork.EncodingFromName(Core_.CoreFile_.FileREnc));
                                }
                                FileCtX = TextWork.StrToInt(SR.ReadToEnd());
                                SR.Close();
                                FS.Close();

                                if (PlaylistFiles[PlaylistIdx].StartsWith(CoreStatic.GetFileListExtraInfo, StringComparison.Ordinal))
                                {
                                    FS = new FileStream(PlaylistFiles[PlaylistIdx].Substring(CoreStatic.GetFileListExtraInfo.Length), FileMode.Open, FileAccess.Read);
                                }
                                else
                                {
                                    FS = new FileStream(PlaylistFiles[PlaylistIdx], FileMode.Open, FileAccess.Read);
                                }
                                BinaryReader BR = new BinaryReader(FS);
                                AnsiSauce_.LoadRaw(BR.ReadBytes((int)FS.Length));
                                BR.Close();
                                FS.Close();
                            }
                            catch (Exception E)
                            {
                                FileCtX = TextWork.StrToInt(E.Message);
                            }
                            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);

                            FilePos = 0;
                            WorkStateS = WorkStateSDef.FileOpenWait;
                            Thread Thr = new Thread(FileOpenCalc);
                            Thr.Start();
                        }
                        break;
                    case WorkStateSDef.FileOpenWait:
                        {
                            Screen_Clear();
                            Screen_WriteText(Core_.CoreAnsi_.AnsiState_.AnsiBufferI + "/" + FileCtX.Count);
                            Screen_Refresh();
                            Thread.Sleep(100);
                        }
                        break;
                    case WorkStateSDef.DisplayPlayFwd: // Play forward
                        {
                            if (WorkSeekAdvance > (MovieLength - MoviePos))
                            {
                                WorkSeekAdvance = (MovieLength - MoviePos);
                            }

                            bool NeedRepaint = false;
                            if (WorkSeekAdvance > 0)
                            {
                                Monitor.Enter(TelnetMutex);
                                int CharsToSend = Core_.CoreAnsi_.AnsiState_.AnsiBufferI;
                                NeedRepaint = Core_.CoreAnsi_.AnsiSeek(WorkSeekAdvance);
                                CharsToSend = Core_.CoreAnsi_.AnsiState_.AnsiBufferI - CharsToSend;
                                Monitor.Exit(TelnetMutex);

                                if (Server_ != null)
                                {
                                    Server_.Send(ServerEncoding.GetBytes(TextWork.IntToStr(FileCtX.GetRange(FilePos, CharsToSend))));
                                }
                                if (CharsToSend <= (DispBufferLen + DispBufferLen))
                                {
                                    while (CharsToSend > 0)
                                    {
                                        DispBuffer.Add(FileCtX[FilePos]);
                                        FilePos++;
                                        CharsToSend--;
                                    }
                                    while (DispBuffer.Count > (DispBufferLen + DispBufferLen))
                                    {
                                        DispBuffer.RemoveRange(0, DispBufferLen);
                                    }
                                }
                                else
                                {
                                    DispBuffer.Clear();
                                    int StartI = Core_.CoreAnsi_.AnsiState_.AnsiBufferI - DispBufferLen;
                                    if (StartI < 0)
                                    {
                                        StartI = 0;
                                    }
                                    for (int i = StartI; i < Core_.CoreAnsi_.AnsiState_.AnsiBufferI; i++)
                                    {
                                        DispBuffer.Add(FileCtX[i]);
                                    }
                                    FilePos = FilePos + CharsToSend;
                                }
                                MoviePos += WorkSeekAdvance;
                            }
                            else
                            {
                                DisplayPaused = true;
                            }

                            if (NeedRepaint)
                            {
                                Core_.CoreAnsi_.AnsiRepaint(false);
                            }
                            DisplayStatusText(DispBuffer);
                            if (FileDelayTime > 0)
                            {
                                FileTimerOffset += FileDelayTimeL;
                                while (FileTimer.ElapsedMilliseconds < FileTimerOffset)
                                {
                                    Thread.Sleep(FileDelayTime_);
                                    if (DisplayPaused)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (WorkStateS != WorkStateSDef.DispConf)
                            {
                                if (DisplayPaused)
                                {
                                    WorkStateS = WorkStateSDef.DisplayPause;
                                    Core_.CoreAnsi_.AnsiRepaint(false);
                                    DisplayStatusText(DispBuffer);
                                }
                                else
                                {
                                    WorkStateS = WorkStateSDef.DisplayFwd;
                                }
                            }
                            WorkSeekOneChar = false;
                        }
                        break;
                    case WorkStateSDef.DisplayPlayRev: // Play backward
                        {
                            if (WorkSeekAdvance > MoviePos)
                            {
                                WorkSeekAdvance = MoviePos;
                            }

                            bool NeedRepaint = false;
                            if (WorkSeekAdvance > 0)
                            {
                                Monitor.Enter(TelnetMutex);
                                int CharsToSend = Core_.CoreAnsi_.AnsiState_.AnsiBufferI;
                                int BufI = Core_.CoreAnsi_.AnsiState_.AnsiBufferI;
                                NeedRepaint = Core_.CoreAnsi_.AnsiSeek(0 - WorkSeekAdvance);
                                CharsToSend = CharsToSend - Core_.CoreAnsi_.AnsiState_.AnsiBufferI;
                                Monitor.Exit(TelnetMutex);

                                int BufL = WorkSeekAdvance + DispBufferLen; 
                                if (BufL >= BufI)
                                {
                                    BufL = BufI - 1;
                                }
                                BufI = BufI - DispBuffer.Count - 1;
                                if (CharsToSend <= (DispBufferLen + DispBufferLen))
                                {
                                    while (DispBuffer.Count <= BufL)
                                    {
                                        DispBuffer.Insert(0, FileCtX[BufI]);
                                        BufI--;
                                    }
                                    while (CharsToSend > 0)
                                    {
                                        DispBuffer.RemoveAt(DispBuffer.Count - 1);
                                        FilePos--;
                                        CharsToSend--;
                                    }
                                }
                                else
                                {
                                    DispBuffer.Clear();
                                    int StartI = Core_.CoreAnsi_.AnsiState_.AnsiBufferI - DispBufferLen;
                                    if (StartI < 0)
                                    {
                                        StartI = 0;
                                    }
                                    for (int i = StartI; i < Core_.CoreAnsi_.AnsiState_.AnsiBufferI; i++)
                                    {
                                        DispBuffer.Add(FileCtX[i]);
                                    }
                                    FilePos = FilePos - CharsToSend;
                                }
                                if (Server_ != null)
                                {
                                    Server_.Send(UniConn.HexToRaw(Server_.TerminalResetCommand));
                                    Server_.Send(ServerEncoding.GetBytes(TextWork.IntToStr(FileCtX.GetRange(0, FilePos))));
                                }
                                MoviePos -= WorkSeekAdvance;
                            }
                            else
                            {
                                DisplayPaused = true;
                            }

                            if (NeedRepaint)
                            {
                                Core_.CoreAnsi_.AnsiRepaint(false);
                            }
                            DisplayStatusText(DispBuffer);
                            if (FileDelayTime > 0)
                            {
                                FileTimerOffset += FileDelayTimeL;
                                while (FileTimer.ElapsedMilliseconds < FileTimerOffset)
                                {
                                    Thread.Sleep(FileDelayTime_);
                                    if (DisplayPaused)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (WorkStateS != WorkStateSDef.DispConf)
                            {
                                if (DisplayPaused)
                                {
                                    WorkStateS = WorkStateSDef.DisplayPause;
                                    Core_.CoreAnsi_.AnsiRepaint(false);
                                    DisplayStatusText(DispBuffer);
                                }
                                else
                                {
                                    WorkStateS = WorkStateSDef.DisplayRev;
                                }
                            }
                            WorkSeekOneChar = false;
                        }
                        break;
                    case WorkStateSDef.DisplayPause: // Display paused
                        {
                            Thread.Sleep(20);
                        }
                        break;
                    case WorkStateSDef.DisplayInfo: // Sauce info
                        {
                            DisplayInfo(false);
                            Thread.Sleep(20);
                        }
                        break;
                    case WorkStateSDef.DisplayFwd: // Advance forward
                        WorkStateS = WorkStateSDef.DisplayPlayFwd;
                        break;
                    case WorkStateSDef.DisplayRev: // Advance backward
                        WorkStateS = WorkStateSDef.DisplayPlayRev;
                        break;
                    case WorkStateSDef.DispConf:
                        break;
                }



            }
            if (Server_ != null)
            {
                Server_.Stop();
            }
            Screen_.CloseApp(Core_.TextNormalBack, Core_.TextNormalFore);
        }


        int InfoPosH = 0;
        int InfoPosV = 0;

        void ExitInfo()
        {
            DisplayPaused = true;
            WorkStateS = WorkStateSDef.DisplayPause;
            ForceRepaint = true;
        }

        int DisplayStatus_ = 0;
        bool DisplayInfoRepaint = false;

        void DisplayInfo(bool Forced)
        {
            if (Forced)
            {
                DisplayInfoRepaint = true;
            }

            if (DisplayInfoRepaint)
            {
                if (InfoPosH < 0) { InfoPosH = 0; }
                if (InfoPosH > 80) { InfoPosH = 80; }
                if (InfoPosV < 0) { InfoPosV = 0; }
                if (InfoPosV >= AnsiSauce_.Info.Count) { InfoPosV = AnsiSauce_.Info.Count - 1; }
                Screen_Clear();
                for (int i = InfoPosV; i < AnsiSauce_.Info.Count; i++)
                {
                    string T = AnsiSauce_.Info[i];
                    if (InfoPosH > 0)
                    {
                        if (T.Length >= InfoPosH)
                        {
                            T = T.Substring(InfoPosH);
                        }
                        else
                        {
                            T = "";
                        }
                    }
                    Screen_WriteText(T);
                    Screen_WriteLine();
                }
                Screen_Refresh();
                DisplayInfoRepaint = false;
            }
        }

        void DisplayStatusText(List<int> DispBuffer)
        {
            Monitor.Enter(StatusMutex);
            int DisplayStatus0 = DisplayStatus;
            Monitor.Exit(StatusMutex);
            string CharMsg = "  ";
            int DisplayStatusMod = (DisplayStatus0 % 3);
            int DisplayStatusDiv = (DisplayStatus0 / 3);
            if ((DisplayStatus_ != DisplayStatus0) || (DisplayStatusMod > 0))
            {
                Core_.CoreAnsi_.AnsiRepaint(false);
            }
            if (DisplayStatusMod != 0)
            {
                CharMsg = "";
                switch (DisplayStatusDiv)
                {
                    case 0:
                        {
                            string CharMsgIdx = MoviePos.ToString() + "/" + MovieLength.ToString();
                            CharMsgIdx = CharMsgIdx + " | " + Core_.CoreAnsi_.AnsiState_.PrintCharCounterOver + "/" + Core_.CoreAnsi_.AnsiState_.PrintCharCounter + " " + Core_.CoreAnsi_.AnsiState_.PrintCharInsDel + " " + Core_.CoreAnsi_.AnsiState_.PrintCharScroll;
                            CharMsgIdx = CharMsgIdx + " | " + (PlaylistIdx + 1) + "/" + PlaylistFiles.Count + (AnsiSauce_.Exists ? "= " : ": ");

                            CharMsg = PlaylistFiles[PlaylistIdx];
                            int MaxS = (Screen_.WinW - CharMsgIdx.Length);
                            if (CharMsg.Length > MaxS)
                            {
                                CharMsg = "..." + CharMsg.Substring(CharMsg.Length - MaxS + 3);
                            }
                            CharMsg = CharMsgIdx + CharMsg + " ";
                        }
                        break;
                    case 1:
                        {
                            CharMsg = MoviePos.ToString() + "/" + MovieLength.ToString();
                            CharMsg = CharMsg + " | " + Core_.CoreAnsi_.AnsiState_.PrintCharCounterOver + "/" + Core_.CoreAnsi_.AnsiState_.PrintCharCounter + " " + Core_.CoreAnsi_.AnsiState_.PrintCharInsDel + " " + Core_.CoreAnsi_.AnsiState_.PrintCharScroll;
                            CharMsg = CharMsg + " | " + TextWork.NumPlusMinus(FileDelayStepFactor);
                            CharMsg = CharMsg + " | " + FileDelayStep__;

                            if (Core_.CoreAnsi_.AnsiState_.__AnsiProcessDelayMin > Core_.CoreAnsi_.AnsiState_.__AnsiProcessDelayMax)
                            {
                                CharMsg = CharMsg + " | ? ?  ";
                            }
                            else
                            {
                                CharMsg = CharMsg + " | " + Core_.CoreAnsi_.AnsiState_.__AnsiProcessDelayMin + " " + Core_.CoreAnsi_.AnsiState_.__AnsiProcessDelayMax + "  ";
                            }
                        }
                        break;
                    case 2:
                    case 3:
                    case 4:
                        {
                            string CharCounter = MoviePos.ToString() + "/" + MovieLength.ToString() + " | " + Core_.CoreAnsi_.AnsiState_.AnsiBufferI.ToString() + "/" + FileCtX.Count.ToString() + ": ";
                            for (int iii = DispBuffer.Count - 1; iii >= 0; iii--)
                            {
                                string CharMsg_ = TextWork.CharCode(DispBuffer[iii], 0) + " ";
                                switch (DisplayStatusDiv)
                                {
                                    case 2:
                                        if (DispBuffer[iii] >= 33)
                                        {
                                            CharMsg_ = TextWork.IntToStr(DispBuffer[iii]) + " ";
                                        }
                                        break;
                                    case 3:
                                        if ((DispBuffer[iii] >= 33) && (DispBuffer[iii] <= 126))
                                        {
                                            CharMsg_ = TextWork.IntToStr(DispBuffer[iii]) + " ";
                                        }
                                        break;
                                }
                                if ((CharCounter.Length + CharMsg.Length + CharMsg_.Length - 1) < Screen_.WinW)
                                {
                                    CharMsg = CharMsg_ + CharMsg;
                                }
                            }
                            CharMsg = CharCounter + CharMsg.PadLeft(Screen_.WinW - CharCounter.Length, ' ');
                        }
                        break;
                }
                CharMsg = CharMsg.Substring(0, CharMsg.Length - 1).PadRight(Screen_.WinW, ' ');
                if (DisplayStatusMod == 1)
                {
                    Screen_.SetCursorPositionNoRefresh(0, 0);
                    Screen_.WriteText(CharMsg, Core_.StatusBack, Core_.StatusFore);
                }
                if (DisplayStatusMod == 2)
                {
                    Screen_.SetCursorPositionNoRefresh(0, Screen_.WinH - 1);
                    Screen_.WriteText(CharMsg, Core_.StatusBack, Core_.StatusFore);
                }
            }
            Core_.CoreAnsi_.AnsiRepaintCursor();
            DisplayStatus_ = DisplayStatus0;
        }

        List<int> NewFileName = new List<int>();

        int DisplayStatus = 0;
        bool DisplayPaused = false;

        bool DisplayConfigReload = false;

        bool DisplayConfig_RequestReapint = false;
        bool DisplayConfig_RequestMenu = false;
        bool DisplayConfig_RequestClose = false;

        DisplayConfig DisplayConfig_;

        void DisplayConfigRepaint(bool AnsiReload, bool Menu)
        {
            Core_.CoreAnsi_.AnsiRepaint(false);
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

        void DisplayConfigOpen()
        {
            DisplayPaused = true;
            ActionRequestParam = 0;
            WorkStateS = WorkStateSDef.DispConf;
            DisplayConfigReload = false;
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
            if (WindowClose)
            {
                Screen_.AppWorking = false;
                WorkStateS = WorkStateSDef.None;
            }
            else
            {
                WorkStateS = WorkStateSDef.DisplayPause;
            }
            if (DisplayConfigReload)
            {
                ActionRequest = 1;
            }
        }

    }
}
