using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace TextPaint
{
    public partial class Telnet
    {
        List<int> FileCtX;

        public void CoreEvent_Server(string KeyName, char KeyChar, int KeyCharI)
        {
            switch (KeyName)
            {
                case "Enter":
                    {
                        switch (WorkState)
                        {
                            case 1:
                                {
                                    WorkState = 2;
                                }
                                break;
                            case 3:
                            case 4:
                                {
                                    DisplayPaused = !DisplayPaused;
                                    if (!DisplayPaused)
                                    {
                                        WorkState = 5;
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case "Tab":
                    {
                        switch (WorkState)
                        {
                            case 1:
                                {
                                    Screen_.CloseApp(Core_.TextNormalBack, Core_.TextNormalFore);
                                    WorkState = 99;
                                }
                                break;
                            default:
                                {
                                    if (Monitor.TryEnter(StatusMutex))
                                    {
                                        DisplayStatus++;
                                        if (DisplayStatus == 3)
                                        {
                                            DisplayStatus = 0;
                                        }
                                        Monitor.Exit(StatusMutex);
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case "Space":
                    {
                        switch (WorkState)
                        {
                            case 1:
                                {
                                    NewFileName.Add(KeyCharI);
                                    Screen_.WriteText(TextWork.IntToStr(KeyCharI), Core_.TextNormalBack, Core_.TextNormalFore);
                                    Screen_.Refresh();
                                }
                                break;
                            case 4:
                                {
                                    WorkState = 6;
                                }
                                break;
                        }
                    }
                    break;
                case "WindowClose":
                    {
                        switch (WorkState)
                        {
                            case 1:
                            case 3:
                            case 4:
                                {
                                    Screen_.AppWorking = false;
                                    WorkState = 99;
                                }
                                break;
                        }
                    }
                    break;
                case "Escape":
                    {
                        switch (WorkState)
                        {
                            case 3:
                            case 4:
                                {
                                    WorkState = 0;
                                }
                                break;
                        }
                    }
                    break;
                case "Backspace":
                    {
                        switch (WorkState)
                        {
                            case 1:
                                {
                                    if (NewFileName.Count > 0)
                                    {
                                        NewFileName.RemoveAt(NewFileName.Count - 1);
                                        Screen_.BackText(Core_.TextNormalBack, Core_.TextNormalFore);
                                        Screen_.Refresh();
                                    }
                                }
                                break;
                            case 4:
                                {
                                    WorkState = 5;
                                }
                                break;
                        }
                    }
                    break;
                default:
                    switch (WorkState)
                    {
                        case 1:
                            if (KeyChar >= 32)
                            {
                                NewFileName.Add(KeyCharI);
                                Screen_.WriteText(TextWork.IntToStr(KeyCharI), Core_.TextNormalBack, Core_.TextNormalFore);
                                Screen_.Refresh();
                            }
                            break;
                    }
                    break;
            }
        }

        public void ServerFile()
        {
            WorkState = 0;
            List<int> DispBuffer = new List<int>();
            int DispBufferLen = 50;
            int FilePos = 0;
            int FilePosPause = 0;
            List<int> PX = new List<int>();
            Socket SPX = null;
            while (Screen_.AppWorking)
            {
                switch (WorkState)
                {
                    case 0: // Information screen before file opening
                        {
                            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
                            Screen_.SetCursorPositionNoRefresh(0, 0);
                            if (ServerPort > 0)
                            {
                                if (SPX != null)
                                {
                                    try
                                    {
                                        SPX.Close();
                                    }
                                    catch
                                    {

                                    }
                                }
                                SPX = null;
                                try
                                {
                                    Screen_.WriteText("Waiting for connection on " + ServerPort.ToString() + "...", Core_.TextNormalBack, Core_.TextNormalFore);
                                    Screen_.WriteLine();
                                    Screen_.Refresh();
                                    TcpListener TXL = new TcpListener(ServerPort);
                                    TXL.Start();
                                    SPX = TXL.AcceptSocket();
                                    TXL.Stop();
                                    Screen_.WriteText("Connected", Core_.TextNormalBack, Core_.TextNormalFore);
                                    Screen_.WriteLine();
                                    Screen_.Refresh();
                                }
                                catch
                                {
                                    Screen_.WriteText("Port listening error", Core_.TextNormalBack, Core_.TextNormalFore);
                                    Screen_.WriteLine();
                                    SPX = null;
                                }
                            }
                            NewFileName.Clear();
                            int MsgB = Core_.TextNormalBack;
                            int MsgF = Core_.TextNormalFore;
                            Screen_.WriteText("During displaying:", MsgB, MsgF);
                            Screen_.WriteLine();
                            Screen_.WriteText("Esc - Return to this screen", MsgB, MsgF);
                            Screen_.WriteLine();
                            Screen_.WriteText("Tab - Show/hide status", MsgB, MsgF);
                            Screen_.WriteLine();
                            Screen_.WriteText("Enter - Start/stop autodisplay", MsgB, MsgF);
                            Screen_.WriteLine();
                            Screen_.WriteText("Space - Process 1 character", MsgB, MsgF);
                            Screen_.WriteLine();
                            Screen_.WriteText("Backspace - Process " + FileDelayChars.ToString() + " characters", MsgB, MsgF);
                            Screen_.WriteLine();
                            Screen_.WriteLine();
                            Screen_.WriteLine();
                            Screen_.WriteText("If you want to display another file, write file name with path now", MsgB, MsgF);
                            Screen_.WriteLine();
                            Screen_.WriteText("Press Enter to start displaying or Tab to quit application", MsgB, MsgF);
                            Screen_.WriteLine();
                            Screen_.Refresh();
                            WorkState = 1;
                        }
                        break;
                    case 1: // Waiting for user key press before opening file
                        {
                            Thread.Sleep(100);
                        }
                        break;
                    case 2: // Opening file
                        {
                            DispBuffer.Clear();
                            DisplayStatus_ = -1;
                            string NewFileNameS = Core_.PrepareFileName(NewFileName);
                            if (NewFileNameS != "")
                            {
                                Core_.CurrentFileName = NewFileNameS;
                            }
                            FileStream FS = new FileStream(Core_.CurrentFileName, FileMode.Open, FileAccess.Read);
                            StreamReader SR;
                            if (Core_.FileREnc != "")
                            {
                                SR = new StreamReader(FS, TextWork.EncodingFromName(Core_.FileREnc));
                            }
                            else
                            {
                                SR = new StreamReader(FS);
                            }
                            FileCtX = TextWork.StrToInt(SR.ReadToEnd());
                            SR.Close();
                            FS.Close();
                            Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
                            WorkState = 3;
                            DisplayPaused = true;
                            Core_.AnsiProcessReset(true);
                            FilePos = 0;
                            FilePosPause = 0;
                            DisplayStatusText(DispBuffer, FilePos);
                            Screen_.SetCursorPosition(Core_.__AnsiX, Core_.__AnsiY);
                        }
                        break;
                    case 3: // Display running
                        if (FilePos >= FileCtX.Count)
                        {
                            DisplayStatusText(DispBuffer, FilePos);
                            Screen_.SetCursorPosition(Core_.__AnsiX, Core_.__AnsiY);
                            WorkState = 4;
                        }
                        else
                        {
                            if (FilePos >= FilePosPause)
                            {
                                DisplayStatusText(DispBuffer, FilePos);
                                Screen_.SetCursorPosition(Core_.__AnsiX, Core_.__AnsiY);
                                if (FileDelayTime > 0)
                                {
                                    Thread.Sleep(FileDelayTime);
                                }
                                if (WorkState != 0)
                                {
                                    if (DisplayPaused)
                                    {
                                        WorkState = 4;
                                    }
                                    else
                                    {
                                        WorkState = 5;
                                    }
                                }
                            }
                            else
                            {
                                PX.Clear();
                                Core_.FileAdd(FileCtX[FilePos], ref PX);
                                while (DispBuffer.Count > DispBufferLen)
                                {
                                    DispBuffer.RemoveAt(0);
                                }
                                DispBuffer.Add(PX[0]);
                                if (SPX != null)
                                {
                                    SPX.Send(ServerEncoding.GetBytes(TextWork.IntToStr(PX)));
                                }
                                Core_.AnsiProcess(PX);
                                //DisplayStatus_ = -1;
                                FilePos++;
                            }
                        }
                        break;
                    case 4: // Display paused or finished
                        {
                            DisplayStatusText(DispBuffer, FilePos);
                            Thread.Sleep(20);
                            if (FilePosPause > FilePos)
                            {
                                WorkState = 3;
                            }
                        }
                        break;
                    case 5: // Advance X characters
                        FilePosPause = FilePos + FileDelayChars;
                        if ((FilePosPause >= FileCtX.Count) || (FilePosPause < 0))
                        {
                            FilePosPause = FileCtX.Count;
                        }
                        WorkState = 4;
                        break;
                    case 6: // Advance 1 character
                        FilePosPause = FilePos + 1;
                        if ((FilePosPause >= FileCtX.Count) || (FilePosPause < 0))
                        {
                            FilePosPause = FileCtX.Count;
                        }
                        WorkState = 4;
                        break;
                }



            }
            Screen_.CloseApp(Core_.TextNormalBack, Core_.TextNormalFore);
        }




        int DisplayStatus_ = 0;

        void DisplayStatusText(List<int> DispBuffer, int i)
        {
            Monitor.Enter(StatusMutex);
            int DisplayStatus0 = DisplayStatus;
            Monitor.Exit(StatusMutex);
            string CharMsg = "  ";
            if (DisplayStatus_ != DisplayStatus0)
            {
                Core_.AnsiRepaint();
            }
            if (DisplayStatus0 > 0)
            {
                CharMsg = "";
                string CharCounter = (i).ToString() + "/" + FileCtX.Count.ToString() + ": ";
                for (int iii = DispBuffer.Count - 1; iii >= 0; iii--)
                {
                    string CharMsg_ = TextWork.CharCode(DispBuffer[iii], 0) + " ";
                    if ((CharCounter.Length + CharMsg.Length + CharMsg_.Length - 1) < Screen_.WinW)
                    {
                        CharMsg = CharMsg_ + CharMsg;
                    }
                }
                CharMsg = CharCounter + CharMsg;
                CharMsg = CharMsg.Substring(0, CharMsg.Length - 1).PadRight(Screen_.WinW, ' ');
                if (DisplayStatus0 == 1)
                {
                    Screen_.SetCursorPositionNoRefresh(0, 0);
                    Screen_.WriteText(CharMsg, Core_.StatusBack, Core_.StatusFore);
                }
                if (DisplayStatus0 == 2)
                {
                    Screen_.SetCursorPositionNoRefresh(0, Screen_.WinH - 1);
                    Screen_.WriteText(CharMsg, Core_.StatusBack, Core_.StatusFore);
                }
            }
            Screen_.SetCursorPosition(Core_.__AnsiX, Core_.__AnsiY);
            DisplayStatus_ = DisplayStatus0;
        }

        List<int> NewFileName = new List<int>();

        int DisplayStatus = 0;
        bool DisplayPaused = false;
    }
}
