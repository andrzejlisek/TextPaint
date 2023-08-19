using System;
namespace TextPaint
{
    public class DisplayConfig
    {
        public delegate void ConfigRepaintEventHandler(bool AnsiReload, bool Menu);
        public event ConfigRepaintEventHandler ConfigRepaint;

        public delegate void ConfigCloseEventHandler(bool WindowClose, int NewW, int NewH);
        public event ConfigCloseEventHandler ConfigClose;

        public int MenuPos = 0;

        public bool IsOpen = false;

        public void DisplayMenu()
        {
            if (!AllowResize)
            {
                WinW = Core_.Screen_.WinW;
                WinH = Core_.Screen_.WinH;
            }
            int OffsetX = 0;
            int OffsetY = 0;
            int InfoW = 31;
            int InfoH = 20;

            string[] MenuInfo = new string[InfoH];
            MenuInfo[0] = " Screen size: " + WinW + "x" + WinH + (Core_.Screen_.WinAuto ? " auto" : "");
            MenuInfo[1] = "";
            MenuInfo[2] = " K. Process mode: " + (Core_.ANSIDOS ? "DOS" : "VTx");
            MenuInfo[3] = " L. 8-bit controls: " + (Core_.ANSI8bit ? "Yes" : "No");
            MenuInfo[4] = " U. Backspace: " + (Core_.ANSIPrintBackspace ? "Character" : "Movement");
            MenuInfo[5] = " I. Tab: " + (Core_.ANSIPrintTab ? "Character" : "Movement");
            MenuInfo[6] = " O. CR: ";
            switch (Core_.ANSI_CR)
            {
                case 0: MenuInfo[6] = MenuInfo[6] + "CR"; break;
                case 1: MenuInfo[6] = MenuInfo[6] + "CR+LF"; break;
                case 2: MenuInfo[6] = MenuInfo[6] + "Ignore"; break;
            }
            MenuInfo[7] = " P. LF: ";
            switch (Core_.ANSI_LF)
            {
                case 0: MenuInfo[7] = MenuInfo[7] + "LF"; break;
                case 1: MenuInfo[7] = MenuInfo[7] + "CR+LF"; break;
                case 2: MenuInfo[7] = MenuInfo[7] + "Ignore"; break;
            }
            MenuInfo[8] = "";
            MenuInfo[9] = " Q. Use colors: " + (Core_.Screen_.ANSIColors ? "Yes" : "No ");
            MenuInfo[10] = " W. Reverse mode: ";
            switch (Core_.Screen_.ANSIReverseMode)
            {
                case 0: MenuInfo[10] = MenuInfo[10] + "None  "; break;
                case 1: MenuInfo[10] = MenuInfo[10] + "Before"; break;
                case 2: MenuInfo[10] = MenuInfo[10] + "After "; break;
            }
            MenuInfo[11] = " E. Bold as color: " + (Core_.Screen_.ANSIColorBold ? "Yes" : "No ");
            MenuInfo[12] = " R. Blink as color: " + (Core_.Screen_.ANSIColorBlink ? "Yes" : "No ");
            MenuInfo[13] = " T. Ignore concealed: " + (Core_.Screen_.ANSIIgnoreConcealed ? "Yes" : "No ");
            MenuInfo[14] = " ";
            MenuInfo[15] = " S. Blink: ";
            switch (Core_.Screen_.FontModeBlink)
            {
                case 0: MenuInfo[15] = MenuInfo[15] + "None"; break;
                case 1: MenuInfo[15] = MenuInfo[15] + "VTx "; break;
                case 2: MenuInfo[15] = MenuInfo[15] + "DOS "; break;
            }
            MenuInfo[16] = " D. Bold: " + ((Core_.Screen_.FontModeBold == 1) ? "Yes" : "No ");
            MenuInfo[17] = " F. Italic: " + ((Core_.Screen_.FontModeItalic == 1) ? "Yes" : "No ");
            MenuInfo[18] = " G. Underline: " + ((Core_.Screen_.FontModeUnderline == 1) ? "Yes" : "No ");
            MenuInfo[19] = " H. Strikethrough: " + ((Core_.Screen_.FontModeStrike == 1) ? "Yes" : "No ");


            if ((MenuPos == 1) || (MenuPos == 3))
            {
                OffsetX = Core_.Screen_.WinW - InfoW;
            }
            if ((MenuPos == 2) || (MenuPos == 3))
            {
                OffsetY = Core_.Screen_.WinH - InfoH;
            }

            int CB_ = Core_.PopupBack;
            int CF_ = Core_.PopupFore;

            for (int Y = 0; Y < InfoH; Y++)
            {
                for (int X = 0; X < InfoW; X++)
                {
                    Core_.Screen_.PutChar(OffsetX + X, OffsetY + Y, ' ', CB_, CF_);
                    if (MenuInfo.Length > Y)
                    {
                        if (MenuInfo[Y].Length > X)
                        {
                            Core_.Screen_.PutChar(OffsetX + X, OffsetY + Y, MenuInfo[Y][X], CB_, CF_);
                        }
                    }
                }
            }

            for (int i = -1; i < InfoW + 1; i++)
            {
                Core_.Screen_.PutChar(OffsetX + i, OffsetY + InfoH, ' ', CF_, CB_);
                Core_.Screen_.PutChar(OffsetX + i, OffsetY - 1, ' ', CF_, CB_);
            }
            for (int i = 0; i < InfoH; i++)
            {
                Core_.Screen_.PutChar(OffsetX + InfoW, OffsetY + i, ' ', CF_, CB_);
                Core_.Screen_.PutChar(OffsetX - 1, OffsetY + i, ' ', CF_, CB_);
            }

            switch (MenuPos)
            {
                case 0:
                    Core_.Screen_.SetCursorPosition(InfoW, InfoH);
                    break;
                case 1:
                    Core_.Screen_.SetCursorPosition(OffsetX - 1, InfoH);
                    break;
                case 2:
                    Core_.Screen_.SetCursorPosition(InfoW, OffsetY - 1);
                    break;
                case 3:
                    Core_.Screen_.SetCursorPosition(OffsetX - 1, OffsetY - 1);
                    break;
            }
        }

        public void ProcessKey(string KeyName, char KeyChar)
        {
            switch (KeyName)
            {
                default:
                    {
                        bool AnsiReload = false;
                        switch (KeyChar)
                        {
                            case 'q':
                            case 'Q':
                                Core_.Screen_.ANSIColors = !Core_.Screen_.ANSIColors;
                                break;
                            case 'w':
                            case 'W':
                                Core_.Screen_.ANSIReverseMode = (Core_.Screen_.ANSIReverseMode + 1) % 3;
                                break;
                            case 'e':
                            case 'E':
                                Core_.Screen_.ANSIColorBold = !Core_.Screen_.ANSIColorBold;
                                break;
                            case 'r':
                            case 'R':
                                Core_.Screen_.ANSIColorBlink = !Core_.Screen_.ANSIColorBlink;
                                break;
                            case 't':
                            case 'T':
                                Core_.Screen_.ANSIIgnoreConcealed = !Core_.Screen_.ANSIIgnoreConcealed;
                                break;

                            case 'k':
                            case 'K':
                                Core_.ANSIDOS = !Core_.ANSIDOS;
                                AnsiReload = true;
                                break;
                            case 'l':
                            case 'L':
                                Core_.ANSI8bit = !Core_.ANSI8bit;
                                AnsiReload = true;
                                break;
                            case 'u':
                            case 'U':
                                Core_.ANSIPrintBackspace = !Core_.ANSIPrintBackspace;
                                AnsiReload = true;
                                break;
                            case 'i':
                            case 'I':
                                Core_.ANSIPrintTab = !Core_.ANSIPrintTab;
                                AnsiReload = true;
                                break;
                            case 'o':
                            case 'O':
                                Core_.ANSI_CR = ((Core_.ANSI_CR + 1) % 3);
                                AnsiReload = true;
                                break;
                            case 'p':
                            case 'P':
                                Core_.ANSI_LF = ((Core_.ANSI_LF + 1) % 3);
                                AnsiReload = true;
                                break;

                            case 's':
                            case 'S':
                                Core_.Screen_.FontModeBlink = ((Core_.Screen_.FontModeBlink + 1) % 3);
                                break;
                            case 'd':
                            case 'D':
                                Core_.Screen_.FontModeBold = 1 - Core_.Screen_.FontModeBold;
                                break;
                            case 'f':
                            case 'F':
                                Core_.Screen_.FontModeItalic = 1 - Core_.Screen_.FontModeItalic;
                                break;
                            case 'g':
                            case 'G':
                                Core_.Screen_.FontModeUnderline = 1 - Core_.Screen_.FontModeUnderline;
                                break;
                            case 'h':
                            case 'H':
                                Core_.Screen_.FontModeStrike = 1 - Core_.Screen_.FontModeStrike;
                                break;
                        }
                        ConfigRepaint(AnsiReload, true);
                    }
                    break;
                case "Enter":
                case "Escape":
                case "WindowClose":
                    IsOpen = false;
                    if (((WinW == WinW0) && (WinH == WinH0)) || (KeyName == "WindowClose") || (!AllowResize))
                    {
                        ConfigClose(KeyName == "WindowClose", -1, -1);
                    }
                    else
                    {
                        ConfigClose(KeyName == "WindowClose", WinW, WinH);
                    }
                    break;
                case "Tab":
                    MenuPos++;
                    if (MenuPos == 4)
                    {
                        MenuPos = 0;
                    }
                    ConfigRepaint(false, true);
                    break;
                case "Resize":
                    ConfigRepaint(false, true);
                    break;
                case "UpArrow":
                    if (AllowResize)
                    {
                        if (WinH > 1) WinH--;
                    }
                    DisplayMenu();
                    break;
                case "DownArrow":
                    if (AllowResize)
                    {
                        WinH++;
                    }
                    DisplayMenu();
                    break;
                case "LeftArrow":
                    if (AllowResize)
                    {
                        if (WinW > 1) WinW--;
                    }
                    DisplayMenu();
                    break;
                case "RightArrow":
                    if (AllowResize)
                    {
                        WinW++;
                    }
                    DisplayMenu();
                    break;
                case "PageUp":
                    if (AllowResize)
                    {
                        WinH -= 10;
                        if (WinH < 1) WinH = 1;
                    }
                    DisplayMenu();
                    break;
                case "PageDown":
                    if (AllowResize)
                    {
                        WinH += 10;
                    }
                    DisplayMenu();
                    break;
                case "Home":
                    if (AllowResize)
                    {
                        WinW -= 10;
                        if (WinW < 1) WinW = 1;
                    }
                    DisplayMenu();
                    break;
                case "End":
                    if (AllowResize)
                    {
                        WinW += 10;
                    }
                    DisplayMenu();
                    break;
                case "Space":
                    if (AllowResize && (Core_.Screen_.WinAutoAllowed))
                    {
                        Core_.Screen_.WinAuto = !Core_.Screen_.WinAuto;
                    }
                    DisplayMenu();
                    break;
            }
        }

        Core Core_;
        int WinW = 0;
        int WinH = 0;
        int WinW0 = 0;
        int WinH0 = 0;
        bool AllowResize = false;

        public DisplayConfig(Core Core__)
        {
            Core_ = Core__;
        }

        public void Open()
        {
            AllowResize = (Core_.Screen_.WinFixed > 0);
            IsOpen = true;
            WinW = Core_.Screen_.WinW;
            WinW0 = Core_.Screen_.WinW;
            WinH = Core_.Screen_.WinH;
            WinH0 = Core_.Screen_.WinH;
        }
    }
}
