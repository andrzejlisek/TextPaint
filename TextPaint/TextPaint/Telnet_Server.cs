﻿using System;
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
                case "Oemtilde":
                    {
                        switch (WorkState)
                        {
                            case 1:
                                break;
                            default:
                                {
                                    if (Monitor.TryEnter(StatusMutex))
                                    {
                                        if ((DisplayStatus < 9))
                                        {
                                            DisplayStatus += 3;
                                        }
                                        else
                                        {
                                            DisplayStatus -= 9;
                                        }
                                        Monitor.Exit(StatusMutex);
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
                                        if ((DisplayStatus % 3) == 0)
                                        {
                                            DisplayStatus -= 3;
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
            int DispBufferLen = 1000;
            int FilePos = 0;
            long FileDisp = 0;
            long FileDispPause = 0;
            List<int> PX = new List<int>();
            Socket SPX = null;
            int FileDelayStep__ = 0;
            while (Screen_.AppWorking)
            {
                switch (WorkState)
                {
                    case 0: // Information screen before file opening
                        {
                            FileDelayStep__ = FileDelayOffset;
                            if (FileDelayStep__ == 0)
                            {
                                FileDelayStep__ = FileDelayStep;
                            }
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
                            Screen_.WriteText("~` - Change status information", MsgB, MsgF);
                            Screen_.WriteLine();
                            Screen_.WriteText("Enter - Start/stop autodisplay", MsgB, MsgF);
                            Screen_.WriteLine();
                            Screen_.WriteText("Space - Process 1 step", MsgB, MsgF);
                            Screen_.WriteLine();
                            Screen_.WriteText("Backspace - Process " + FileDelayStep.ToString() + " steps", MsgB, MsgF);
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
                            Screen_.WriteLine();
                            Screen_.WriteText("Please wait...", Core_.TextNormalBack, Core_.TextNormalFore);
                            Screen_.WriteLine();
                            Screen_.Refresh();
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
                            Core_.AnsiProcessSupply(FileCtX);
                            FilePos = 0;
                            FileDisp = 0;
                            FileDispPause = 0;
                            DisplayStatusText(DispBuffer, FilePos);
                            Core_.AnsiRepaintCursor();
                        }
                        break;
                    case 3: // Display running
                        {
                            if (FileDisp >= FileDispPause)
                            {
                                DisplayStatusText(DispBuffer, FilePos);
                                Core_.AnsiRepaintCursor();
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
                                int CharsToSend = Core_.AnsiProcess(1);
                                FileDisp++;

                                while (CharsToSend > 0)
                                {
                                    while (DispBuffer.Count > (DispBufferLen + DispBufferLen))
                                    {
                                        DispBuffer.RemoveRange(0, DispBufferLen);
                                    }
                                    DispBuffer.Add(FileCtX[FilePos]);
                                    if (SPX != null)
                                    {
                                        SPX.Send(ServerEncoding.GetBytes(TextWork.IntToStr(FileCtX[FilePos])));
                                    }
                                    FilePos++;
                                    CharsToSend--;
                                }
                            }
                        }
                        break;
                    case 4: // Display paused or finished
                        {
                            DisplayStatusText(DispBuffer, FilePos);
                            Thread.Sleep(20);
                            if (FileDispPause > FileDisp)
                            {
                                WorkState = 3;
                            }
                        }
                        break;
                    case 5: // Advance X characters
                        FileDispPause = FileDisp + FileDelayStep__;
                        FileDelayStep__ = FileDelayStep;
                        WorkState = 4;
                        break;
                    case 6: // Advance 1 character
                        FileDispPause = FileDisp + 1;
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
                Core_.AnsiRepaint(false);
            }
            int DisplayStatusMod = (DisplayStatus0 % 3);
            int DisplayStatusDiv = (DisplayStatus0 / 3);
            if (DisplayStatusMod != 0)
            {
                CharMsg = "";
                if (DisplayStatusDiv <= 2)
                {
                    string CharCounter = (i).ToString() + "/" + FileCtX.Count.ToString() + ": ";
                    for (int iii = DispBuffer.Count - 1; iii >= 0; iii--)
                    {
                        string CharMsg_ = TextWork.CharCode(DispBuffer[iii], 0) + " ";
                        switch (DisplayStatusDiv)
                        {
                            case 0:
                                if (DispBuffer[iii] >= 32)
                                {
                                    if (DispBuffer[iii] == 32)
                                    {
                                        CharMsg_ = TextWork.IntToStr(0xFFFD) + " ";
                                    }
                                    else
                                    {
                                        CharMsg_ = TextWork.IntToStr(DispBuffer[iii]) + " ";
                                    }
                                }
                                break;
                            case 1:
                                if ((DispBuffer[iii] >= 32) && (DispBuffer[iii] <= 126))
                                {
                                    if (DispBuffer[iii] == 32)
                                    {
                                        CharMsg_ = TextWork.IntToStr(0xFFFD) + " ";
                                    }
                                    else
                                    {
                                        CharMsg_ = TextWork.IntToStr(DispBuffer[iii]) + " ";
                                    }
                                }
                                break;
                        }
                        if ((CharCounter.Length + CharMsg.Length + CharMsg_.Length - 1) < Screen_.WinW)
                        {
                            CharMsg = CharMsg_ + CharMsg;
                        }
                    }
                    CharMsg = CharCounter + CharMsg;
                }
                else
                {
                    if (Core_.__AnsiProcessDelayMin > Core_.__AnsiProcessDelayMax)
                    {
                        CharMsg = "Step: " + Core_.__AnsiProcessStep + "  Min: ?  Max: ?  ";
                    }
                    else
                    {
                        CharMsg = "Step: " + Core_.__AnsiProcessStep + "  Min: " + Core_.__AnsiProcessDelayMin + "  Max: " + Core_.__AnsiProcessDelayMax + "  ";
                    }
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
            Core_.AnsiRepaintCursor();
            DisplayStatus_ = DisplayStatus0;
        }

        List<int> NewFileName = new List<int>();

        int DisplayStatus = 0;
        bool DisplayPaused = false;
    }
}
