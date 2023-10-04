/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-07
 * Time: 21:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TextPaint
{
    /// <summary>
    /// Description of Semigraphics.
    /// </summary>
    public class Semigraphics
    {
        public int AnsiMaxX_ = 0;
        public int AnsiMaxY_ = 0;

        public int ANSI_CR_ = 0;
        public int ANSI_LF_ = 0;

        int MaxCharCode = 0x110000;
        int SelectorState = 1;

        public Core Core_;
        
        public int[] FrameChar = new int[22];
        public int[] FavChar = new int[256];
        
        public List<int[]> Frame1C = new List<int[]>();
        public List<string> Frame1N = new List<string>();
        public List<int[]> Frame2C = new List<int[]>();
        public List<string> Frame2N = new List<string>();


        DisplayConfig DisplayConfig_;

        void DisplayConfigRepaint(bool AnsiReload, bool Menu)
        {
            Core_.ScreenRefresh(true);
            if (Menu)
            {
                DisplayConfig_.DisplayMenu();
            }
        }

        void DisplayConfigOpen()
        {
            DisplayConfig_.Open();
            DisplayConfig_.MenuPos = CharPosMode;
            SelectCharClose(2);
            Core_.ScreenRefresh(true);
            DisplayConfig_.DisplayMenu();
        }

        void DisplayConfigClose(bool WindowClose, int NewW, int NewH)
        {
            CharPosMode = DisplayConfig_.MenuPos;
            if ((NewW > 0) && (NewH >= 0))
            {
                Core_.AppResize(NewW, NewH, false);
            }
            DisplayConfigRepaint(false, false);
            SelectCharInit();

            if (WindowClose)
            {
                Core_.CoreEvent("WindowClose", '\0', false, false, false);
            }
        }


        List<int> WinBitmapPage = new List<int>();
        bool WinBitmapEnabled = true;

        int WinBitmapNearest(int CharCode, bool Backward)
        {
            if (WinBitmapEnabled)
            {
                int CharPage = CharCode >> 8;
                if (WinBitmapPage.Count > 0)
                {
                    if (CharPage < WinBitmapPage[0])
                    {
                        if (Backward)
                        {
                            CharPage = WinBitmapPage[WinBitmapPage.Count - 1];
                        }
                        else
                        {
                            CharPage = WinBitmapPage[0];
                        }
                    }
                    else
                    {
                        if (CharPage > WinBitmapPage[WinBitmapPage.Count - 1])
                        {
                            if (Backward)
                            {
                                CharPage = WinBitmapPage[WinBitmapPage.Count - 1];
                            }
                            else
                            {
                                CharPage = WinBitmapPage[0];
                            }
                        }
                        else
                        {
                            if (Backward)
                            {
                                int I = WinBitmapPage.Count - 1;
                                while (CharPage < WinBitmapPage[I])
                                {
                                    I--;
                                }
                                CharPage = WinBitmapPage[I];
                            }
                            else
                            {
                                int I = 0;
                                while (CharPage > WinBitmapPage[I])
                                {
                                    I++;
                                }
                                CharPage = WinBitmapPage[I];
                            }
                        }
                    }
                }
                else
                {
                    if ((CharPage >= 0xD8) && (CharPage <= 0xDF))
                    {
                        if (Backward)
                        {
                            CharPage = 0xD7;
                        }
                        else
                        {
                            CharPage = 0xE0;
                        }
                    }
                }
                CharCode = (CharPage << 8) + (CharCode & 255);
            }
            return CharCode;
        }

        public Semigraphics(Core Core__, string FontName)
        {
            Core_ = Core__;
            DisplayConfig_ = new DisplayConfig(Core_);
            DisplayConfig_.ConfigRepaint += DisplayConfigRepaint;
            DisplayConfig_.ConfigClose += DisplayConfigClose;
            WinBitmapPage.Clear();
        }

        public void CreateFontList(ScreenFont ScreenFont_)
        {
            WinBitmapPage.Clear();
            foreach (var item in ScreenFont_.WinBitmapPage)
            {
                int Plane = item.Key >> 8;
                bool PlaneSecondExists = false;
                for (int i = 0; i < ScreenFont_.PlaneSecond.Length; i++)
                {
                    if ((ScreenFont_.PlaneSecond[i] == Plane) && (ScreenFont_.PlaneSecond[i] >= 0))
                    {
                        PlaneSecondExists = true;
                    }
                }
                if (!PlaneSecondExists)
                {
                    WinBitmapPage.Add(item.Key);
                }
            }
            WinBitmapPage.Sort();
        }

        ConfigFile FavCharFile;

        void FavCharSet(int Pos, int Char)
        {
            FavChar[Pos] = Char;
            if ((Pos >= 0) && (Pos <= 255))
            {
                FavCharFile.ParamSet("Fav" + Pos.ToString("X").PadLeft(2, '0'), Char.ToString("X").PadLeft(4, '0'));
            }
            FavCharFile.FileSave(Core.AppDir() + "Fav.txt");
        }

        public void Init(ConfigFile CF)
        {
            /*
                0 -
                1 |
                
                2---3---4
                |   |   |
                5---6---7
                |   |   |
                8---9---10
                
                11 \
                12 /
                
                      13
                     /  \
                   16    14
                  /  \  /  \
                19    17    15
                  \  /  \  /
                   20    18
                     \  /
                      21
             */
            
            Frame1N.Add("Char");
            Frame2N.Add("Char");
            Frame1C.Add(new int[11]);
            Frame2C.Add(new int[11]);
            for (int i = 0; i < 11; i++)
            {
                Frame1C[Frame1C.Count - 1][i] = 32;
                Frame2C[Frame2C.Count - 1][i] = 32;
            }
            
            for (int i = 0; i >= 0; i++)
            {
                string[] Buf = CF.ParamGetS("Frame1_" + i).Split(',');
                if (Buf.Length == 12)
                {
                    Frame1N.Add(Buf[0]);
                    int[] Temp = new int[11];
                    for (int ii = 0; ii < 11; ii++)
                    {
                        Temp[ii] = TextWork.CodeChar(Buf[ii + 1]);
                    }
                    Frame1C.Add(Temp);
                }
                else
                {
                    i = -2;
                }
            }
            for (int i = 0; i >= 0; i++)
            {
                string[] Buf = CF.ParamGetS("Frame2_" + i).Split(',');
                if (Buf.Length == 12)
                {
                    Frame2N.Add(Buf[0]);
                    int[] Temp = new int[11];
                    for (int ii = 0; ii < 11; ii++)
                    {
                        Temp[ii] = TextWork.CodeChar(Buf[ii + 1]);
                    }
                    Frame2C.Add(Temp);
                }
                else
                {
                    i = -2;
                }
            }

            FavCharFile = new ConfigFile();
            FavCharFile.FileLoad(Core.AppDir() + "Fav.txt");
            for (int i = 0; i < 256; i++)
            {
                FavChar[i] = ' ';
                try
                {
                    int Temp = TextWork.CodeChar(FavCharFile.ParamGetS("Fav" + i.ToString("X").PadLeft(2, '0')));
                    FavChar[i] = Temp;
                }
                catch
                {

                }
                FavCharFile.ParamSet("Fav" + i.ToString("X").PadLeft(2, '0'), FavChar[i].ToString("X").PadLeft(4, '0'));
            }

            DrawCharI = TextWork.SpaceChar0;
            DrawCharIdbl = 0;
            DrawColoBI = -1;
            DrawColoFI = -1;

            SetFrame(0, 0);
        }
        
        public int Frame1I;
        public int Frame2I;

        public string GetFrameName(int F)
        {
            if (F == 1)
            {
                if (Frame1I == 0)
                {
                    return TextWork.CharCode(DrawCharI, 1) + " " + TextWork.CharToStr(DrawCharI);
                }
                else
                {
                    return Frame1N[Frame1I];
                }
            }
            if (F == 2)
            {
                if (Frame2I == 0)
                {
                    return TextWork.CharCode(DrawCharI, 1) + " " + TextWork.CharToStr(DrawCharI);
                }
                else
                {
                    return Frame2N[Frame2I];
                }
            }
            return "";
        }

        public void SetFrame(int F1, int F2)
        {
            Frame1I = F1;
            Frame2I = F2;
            for (int i = 0; i < 11; i++)
            {
                FrameChar[i] = Frame1C[F1][i];
                FrameChar[i + 11] = Frame2C[F2][i];
            }
        }
        
        public void SetFrameNext(int N)
        {
            if (N == 1)
            {
                Frame1I++;
                if (Frame1I >= Frame1C.Count)
                {
                    Frame1I = 0;
                }
            }
            if (N == 2)
            {
                Frame2I++;
                if (Frame2I >= Frame2C.Count)
                {
                    Frame2I = 0;
                }
            }
            SetFrame(Frame1I, Frame2I);
        }
        
        public bool SelectCharState = false;
        public int SelectChar___ = 0;
        public int SelectColoB___ = -1;
        public int SelectColoF___ = -1;
        public int SelectColoA___ = 0;
        public int SelectToFav = -1;

        public int SelectChar___X()
        {
            return (Math.Abs(SelectChar___) % 16);
        }

        public int SelectChar___Y()
        {
            return (Math.Abs(SelectChar___) % 256) / 16;
        }

        public int SelectChar___Page()
        {
            if (SelectChar___ < 0)
            {
                return -1;
            }
            else
            {
                return (Math.Abs(SelectChar___) / 256);
            }
        }

        public void SelectChar___FavSet(int N)
        {
            SelectChar___ = 0 - (N + 256);
        }

        public int SelectChar___FavGet()
        {
            return (0 - SelectChar___) - 256;
        }

        public int SelectChar___FavGetX()
        {
            return (0 - (SelectChar___ - 1)) - 256;
        }


        public void SelectCharInit()
        {
            switch (CharPosMode)
            {
                case 0:
                    CharPosX = -1;
                    CharPosY = -1;
                    break;
                case 1:
                    CharPosX = Core_.Screen_.WinW - 34;
                    CharPosY = -1;
                    break;
                case 2:
                    CharPosX = -1;
                    CharPosY = Core_.Screen_.WinH - 18;
                    break;
                case 3:
                    CharPosX = Core_.Screen_.WinW - 34;
                    CharPosY = Core_.Screen_.WinH - 18;
                    break;
            }

            SelectChar___ = DrawCharI;
            SelectColoB___ = DrawColoBI;
            SelectColoF___ = DrawColoFI;
            SelectColoA___ = DrawColoAI;
            if ((TabChar >= 0) || (TabChar <= (-256)))
            {
                SelectChar___ = TabChar;
                TabChar = -1;
            }
            if ((TabColoB >= -1) && (TabColoF >= -1))
            {
                SelectColoB___ = TabColoB;
                SelectColoF___ = TabColoF;
                SelectColoA___ = TabColoA;
                TabColoB = -2;
                TabColoF = -2;
                TabColoA = -2;
            }
            SelectCharState = true;
            SelectCharRepaintBack();
            SelectCharRepaint();
        }
        
        public void SelectCharCode()
        {
            if (SelectorState == 1)
            {
                int CX = SelectCharGetCodeX();
                int CXdbl = (CX >= 0) ? Core_.Screen_.CharDouble(SelectCharGetCodeX()) : 0;
                int C = SelectCharGetCode();
                string C_ = TextWork.CharCode(C, 2);
                Core_.Screen_.PutChar(CharPosX + 1, CharPosY + 1, C_[0], Core_.PopupBack, Core_.PopupFore);
                Core_.Screen_.PutChar(CharPosX + 2, CharPosY + 1, C_[1], Core_.PopupBack, Core_.PopupFore);
                Core_.Screen_.PutChar(CharPosX + 3, CharPosY + 1, C_[2], Core_.PopupBack, Core_.PopupFore);
                Core_.Screen_.PutChar(CharPosX + 4, CharPosY + 1, C_[3], Core_.PopupBack, Core_.PopupFore);
                Core_.Screen_.PutChar(CharPosX + 5, CharPosY + 1, C_[4], Core_.PopupBack, Core_.PopupFore);

                Core_.Screen_.PutChar(CharPosX + 6, CharPosY + 1, ' ', Core_.PopupBack, Core_.PopupFore);
                Core_.Screen_.PutChar(CharPosX + 7, CharPosY + 1, ' ', Core_.PopupBack, Core_.PopupFore);
                Core_.Screen_.PutChar(CharPosX + 8, CharPosY + 1, ' ', Core_.PopupBack, Core_.PopupFore);
                Core_.Screen_.PutChar(CharPosX + 9, CharPosY + 1, ' ', Core_.PopupBack, Core_.PopupFore);
                Core_.Screen_.PutChar(CharPosX + 7, CharPosY + 1, C, Core_.PopupBack, Core_.PopupFore);
                int Cdbl = Core_.Screen_.CharDouble(C);
                if (CXdbl != 0)
                {
                    Core_.Screen_.PutChar(CharPosX + SelectChar___X() * 2 + 0, CharPosY + SelectChar___Y() + 2, ' ', Core_.PopupBack, Core_.PopupFore);
                }
                if (Cdbl != 0)
                {
                    Core_.Screen_.PutChar(CharPosX + 8, CharPosY + 1, Cdbl, Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + SelectChar___X() * 2 + 1, CharPosY + SelectChar___Y() + 2, '>', Core_.PopupBack, Core_.PopupFore);
                }
                else
                {
                    Core_.Screen_.PutChar(CharPosX + SelectChar___X() * 2 + 1, CharPosY + SelectChar___Y() + 2, '[', Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + SelectChar___X() * 2 + 3, CharPosY + SelectChar___Y() + 2, ']', Core_.PopupBack, Core_.PopupFore);
                }

                if (SelectChar___ < 0)
                {
                    int TempCharCode = SelectChar___Y() * 16 + SelectChar___X();
                    if ((TempCharCode >= 32) && (TempCharCode <= 126))
                    {
                        Core_.Screen_.PutChar(CharPosX + 10, CharPosY + 1, '[', Core_.PopupBack, Core_.PopupFore);
                        Core_.Screen_.PutChar(CharPosX + 11, CharPosY + 1, TempCharCode, Core_.PopupBack, Core_.PopupFore);
                        Core_.Screen_.PutChar(CharPosX + 12, CharPosY + 1, ']', Core_.PopupBack, Core_.PopupFore);
                    }
                    else
                    {
                        Core_.Screen_.PutChar(CharPosX + 10, CharPosY + 1, 'F', Core_.PopupBack, Core_.PopupFore);
                        Core_.Screen_.PutChar(CharPosX + 11, CharPosY + 1, 'A', Core_.PopupBack, Core_.PopupFore);
                        Core_.Screen_.PutChar(CharPosX + 12, CharPosY + 1, 'V', Core_.PopupBack, Core_.PopupFore);
                    }
                }
                else
                {
                    Core_.Screen_.PutChar(CharPosX + 10, CharPosY + 1, ' ', Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + 11, CharPosY + 1, ' ', Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + 12, CharPosY + 1, ' ', Core_.PopupBack, Core_.PopupFore);
                }
                if (!WinBitmapEnabled)
                {
                    Core_.Screen_.PutChar(CharPosX + 30, CharPosY + 1, 'A', Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + 31, CharPosY + 1, 'L', Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + 32, CharPosY + 1, 'L', Core_.PopupBack, Core_.PopupFore);
                }
                else
                {
                    Core_.Screen_.PutChar(CharPosX + 30, CharPosY + 1, ' ', Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + 31, CharPosY + 1, ' ', Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + 32, CharPosY + 1, ' ', Core_.PopupBack, Core_.PopupFore);
                }
                Core_.Screen_.SetCursorPosition(CharPosX + SelectChar___X() * 2 + 2, CharPosY + SelectChar___Y() + 2);
            }
            if (SelectorState == 2)
            {
                int TempB = SelectColoB___;
                int TempF = SelectColoF___;
                int TempA = SelectColoA___;
                if ((TempB >= 0) && (TempF >= 0))
                {
                    Core_.Screen_.PutChar(CharPosX + TempF * 2 + 1, CharPosY + TempB + 2, '[', Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + TempF * 2 + 3, CharPosY + TempB + 2, ']', Core_.PopupBack, Core_.PopupFore);
                }
                else
                {
                    if (TempB >= 0)
                    {
                        Core_.Screen_.PutChar(CharPosX + 0 * 2 + 1, CharPosY + TempB + 2, '[', Core_.PopupBack, Core_.PopupFore);
                        Core_.Screen_.PutChar(CharPosX + 15 * 2 + 3, CharPosY + TempB + 2, ']', Core_.PopupBack, Core_.PopupFore);
                    }
                    if (TempF >= 0)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            Core_.Screen_.PutChar(CharPosX + TempF * 2 + 1, CharPosY + i + 2, '[', Core_.PopupBack, Core_.PopupFore);
                            Core_.Screen_.PutChar(CharPosX + TempF * 2 + 3, CharPosY + i + 2, ']', Core_.PopupBack, Core_.PopupFore);
                        }
                    }
                }
                if (TempB < 0)
                {
                    TempB = 0;
                    Core_.Screen_.PutChar(CharPosX + 2, CharPosY + 1, '-', Core_.PopupBack, Core_.PopupFore);
                }
                else
                {
                    Core_.Screen_.PutChar(CharPosX + 2, CharPosY + 1, TempB.ToString("X")[0], Core_.PopupBack, Core_.PopupFore);
                }
                if (TempF < 0)
                {
                    TempF = 0;
                    Core_.Screen_.PutChar(CharPosX + 3, CharPosY + 1, '-', Core_.PopupBack, Core_.PopupFore);
                }
                else
                {
                    Core_.Screen_.PutChar(CharPosX + 3, CharPosY + 1, TempF.ToString("X")[0], Core_.PopupBack, Core_.PopupFore);
                }

                string StateMsg = " Text+color ";
                if (!Core_.ToggleDrawText)
                {
                    StateMsg = " Color      ";
                }
                if (!Core_.ToggleDrawColo)
                {
                    StateMsg = " Text       ";
                }

                string EOL1 = ANSI_CR_.ToString();
                string EOL2 = ANSI_LF_.ToString();

                StateMsg = CoreStatic.GetAttribText(TempA) + StateMsg + EOL1 + EOL2 + (AnsiMaxX_ + "x" + AnsiMaxY_).PadLeft(7);

                for (int i = 0; i < StateMsg.Length; i++)
                {
                    Core_.Screen_.PutChar(CharPosX + 5 + i, CharPosY + 1, StateMsg[i], Core_.PopupBack, Core_.PopupFore);
                }


                Core_.Screen_.SetCursorPosition(CharPosX + TempF * 2 + 2, CharPosY + TempB + 2);
            }
        }

        public void SelectCharRepaintBack()
        {
            int CharW = 35;
            int CharH = 19;
            for (int Y = 0; Y < CharH; Y++)
            {
                for (int X = 0; X < CharW; X++)
                {
                    Core_.Screen_.PutChar(CharPosX + X, CharPosY + Y, ' ', Core_.PopupBack, Core_.PopupFore);
                }
            }
            for (int i = 0; i < CharW; i++)
            {
                Core_.Screen_.PutChar(CharPosX + i, CharPosY + 0, ' ', Core_.PopupFore, Core_.PopupBack);
                Core_.Screen_.PutChar(CharPosX + i, CharPosY + CharH - 1, ' ', Core_.PopupFore, Core_.PopupBack);
            }
            for (int i = 0; i < CharH; i++)
            {
                Core_.Screen_.PutChar(CharPosX + 0, CharPosY + i, ' ', Core_.PopupFore, Core_.PopupBack);
                Core_.Screen_.PutChar(CharPosX + CharW - 1, CharPosY + i, ' ', Core_.PopupFore, Core_.PopupBack);
            }
            if (SelectorState == 1)
            {
                if (SelectToFav >= 0)
                {
                    int FavPos = 26;
                    string C_ = TextWork.CharCode(SelectToFav, 2);
                    Core_.Screen_.PutChar(CharPosX + FavPos + 0, CharPosY + 1, C_[0], Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + FavPos + 1, CharPosY + 1, C_[1], Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + FavPos + 2, CharPosY + 1, C_[2], Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + FavPos + 3, CharPosY + 1, C_[3], Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + FavPos + 4, CharPosY + 1, C_[4], Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + FavPos + 6, CharPosY + 1, SelectToFav, Core_.PopupBack, Core_.PopupFore);
                }
            }
        }
        
        public void SelectCharRepaint()
        {
            if (DisplayConfig_.IsOpen)
            {
                return;
            }

            SelectCharRepaintBack();
            if (SelectorState == 1)
            {
                if (SelectChar___ >= 0)
                {
                    int C = SelectChar___Page() * 256;
                    int Cdbl = 0;
                    for (int Y = 0; Y < 16; Y++)
                    {
                        for (int X = 0; X < 16; X++)
                        {
                            Core_.Screen_.PutChar(CharPosX + X * 2 + 2, CharPosY + Y + 2, C, Core_.PopupBack, Core_.PopupFore);
                            Cdbl = Core_.Screen_.CharDouble(C);
                            if (Cdbl != 0)
                            {
                                Core_.Screen_.PutChar(CharPosX + X * 2 + 3, CharPosY + Y + 2, Cdbl, Core_.PopupBack, Core_.PopupFore);
                            }
                            C++;
                        }
                    }
                }
                else
                {
                    int C = 0;
                    int Cdbl = 0;
                    for (int Y = 0; Y < 16; Y++)
                    {
                        for (int X = 0; X < 16; X++)
                        {
                            Core_.Screen_.PutChar(CharPosX + X * 2 + 2, CharPosY + Y + 2, FavChar[C], Core_.PopupBack, Core_.PopupFore);
                            Cdbl = Core_.Screen_.CharDouble(FavChar[C]);
                            if (Cdbl != 0)
                            {
                                Core_.Screen_.PutChar(CharPosX + X * 2 + 3, CharPosY + Y + 2, Cdbl, Core_.PopupBack, Core_.PopupFore);
                            }
                            C++;
                        }
                    }
                }
            }
            if (SelectorState == 2)
            {
                int C = 0;
                for (int Y = 0; Y < 16; Y++)
                {
                    for (int X = 0; X < 16; X++)
                    {
                        Core_.Screen_.PutChar(CharPosX + X * 2 + 2, CharPosY + Y + 2, '#', Y, X);
                        C++;
                    }
                }
            }
            SelectCharCode();
        }
        
        int SelectCharGetCode()
        {
            if (SelectChar___ >= 0)
            {
                return SelectChar___;
            }
            else
            {
                return FavChar[SelectChar___FavGet()];
            }
        }

        int SelectCharGetCodeX()
        {
            if (SelectChar___X() == 0)
            {
                return -1;
            }
            if ((SelectChar___ - 1) >= 0)
            {
                return (SelectChar___ - 1);
            }
            else
            {
                return FavChar[SelectChar___FavGetX()];
            }
        }

        void SelectCharChange(int T)
        {
            if (SelectorState == 1)
            {
                Core_.Screen_.PutChar(CharPosX + SelectChar___X() * 2 + 1, CharPosY + SelectChar___Y() + 2, ' ', Core_.PopupBack, Core_.PopupFore);
                if (SelectChar___X() > 0)
                {
                    int CX = SelectCharGetCodeX();
                    int CXdbl = Core_.Screen_.CharDouble(CX);
                    if (CXdbl != 0)
                    {
                        Core_.Screen_.PutChar(CharPosX + SelectChar___X() * 2 + 0, CharPosY + SelectChar___Y() + 2, CX, Core_.PopupBack, Core_.PopupFore);
                        Core_.Screen_.PutChar(CharPosX + SelectChar___X() * 2 + 1, CharPosY + SelectChar___Y() + 2, CXdbl, Core_.PopupBack, Core_.PopupFore);
                    }
                }
                if (Core_.Screen_.CharDouble(SelectCharGetCode()) == 0)
                {
                    Core_.Screen_.PutChar(CharPosX + SelectChar___X() * 2 + 3, CharPosY + SelectChar___Y() + 2, ' ', Core_.PopupBack, Core_.PopupFore);
                }
                bool FavPage = (SelectChar___ < 0);
                int PageDisplayed = FavPage ? 0 : (SelectChar___ >> 8);

                int SelectChar___0 = SelectChar___;
                switch (T)
                {
                    case -1:
                        if (FavPage)
                        {
                            SelectChar___ += 1;
                        }
                        else
                        {
                            SelectChar___ -= 1;
                        }
                        break;
                    case 1:
                        if (FavPage)
                        {
                            SelectChar___ -= 1;
                        }
                        else
                        {
                            SelectChar___ += 1;
                        }
                        break;
                    case -2:
                        if (FavPage)
                        {
                            SelectChar___ += 16;
                        }
                        else
                        {
                            SelectChar___ -= 16;
                        }
                        break;
                    case 2:
                        if (FavPage)
                        {
                            SelectChar___ -= 16;
                        }
                        else
                        {
                            SelectChar___ += 16;
                        }
                        break;
                    case -3:
                        if (!FavPage)
                        {
                            SelectChar___ -= 256;
                        }
                        break;
                    case 3:
                        if (!FavPage)
                        {
                            SelectChar___ += 256;
                        }
                        break;
                    case -4:
                        if (!FavPage)
                        {
                            SelectChar___ -= 4096;
                        }
                        break;
                    case 4:
                        if (!FavPage)
                        {
                            SelectChar___ += 4096;
                        }
                        break;
                    case -5:
                        if (!FavPage)
                        {
                            SelectChar___ -= 65536;
                        }
                        break;
                    case 5:
                        if (!FavPage)
                        {
                            SelectChar___ += 65536;
                        }
                        break;
                }
                if (FavPage)
                {
                    while (SelectChar___ < (-511))
                    {
                        SelectChar___ += 256;
                    }
                    while (SelectChar___ > (-256))
                    {
                        SelectChar___ -= 256;
                    }
                }
                else
                {
                    while (SelectChar___ < 0)
                    {
                        SelectChar___ += MaxCharCode;
                    }
                    while (SelectChar___ >= MaxCharCode)
                    {
                        SelectChar___ -= MaxCharCode;
                    }
                    if (T != 0)
                    {
                        if ((SelectChar___0 >> 8) != (SelectChar___ >> 8))
                        {
                            SelectChar___ = WinBitmapNearest(SelectChar___, (T < 0));
                        }
                    }
                }
                if ((SelectChar___ >= 0) && (PageDisplayed != (SelectChar___ >> 8)))
                {
                    SelectCharRepaint();
                }
                else
                {
                    SelectCharCode();
                }
            }
            if (SelectorState == 2)
            {
                int TempB = SelectColoB___;
                int TempF = SelectColoF___;
                int TempA = SelectColoA___;
                if ((TempB >= 0) && (TempF >= 0))
                {
                    Core_.Screen_.PutChar(CharPosX + TempF * 2 + 1, CharPosY + TempB + 2, ' ', Core_.PopupBack, Core_.PopupFore);
                    Core_.Screen_.PutChar(CharPosX + TempF * 2 + 3, CharPosY + TempB + 2, ' ', Core_.PopupBack, Core_.PopupFore);
                }
                else
                {
                    if (TempB >= 0)
                    {
                        Core_.Screen_.PutChar(CharPosX + 0 * 2 + 1, CharPosY + TempB + 2, ' ', Core_.PopupBack, Core_.PopupFore);
                        Core_.Screen_.PutChar(CharPosX + 15 * 2 + 3, CharPosY + TempB + 2, ' ', Core_.PopupBack, Core_.PopupFore);
                    }
                    if (TempF >= 0)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            Core_.Screen_.PutChar(CharPosX + TempF * 2 + 1, CharPosY + i + 2, ' ', Core_.PopupBack, Core_.PopupFore);
                            Core_.Screen_.PutChar(CharPosX + TempF * 2 + 3, CharPosY + i + 2, ' ', Core_.PopupBack, Core_.PopupFore);
                        }
                    }
                }

                switch (T)
                {
                    case -1:
                        TempF--;
                        if (TempF == -2) { TempF = 15; }
                        break;
                    case 1:
                        TempF++;
                        if (TempF == 16) { TempF = -1; }
                        break;
                    case -2:
                        TempB--;
                        if (TempB == -2) { TempB = 15; }
                        break;
                    case 2:
                        TempB++;
                        if (TempB == 16) { TempB = -1; }
                        break;
                    case 3:
                        AnsiMaxX_++;
                        break;
                    case -3:
                        if (AnsiMaxX_ > 0) { AnsiMaxX_--; }
                        break;
                    case 4:
                        AnsiMaxY_++;
                        break;
                    case -4:
                        if (AnsiMaxY_ > 0) { AnsiMaxY_--; }
                        break;
                    case 5:
                        ANSI_LF_++;
                        if (ANSI_LF_ == 3)
                        {
                            ANSI_LF_ = 0;
                        }
                        break;
                    case -5:
                        ANSI_CR_++;
                        if (ANSI_CR_ == 3)
                        {
                            ANSI_CR_ = 0;
                        }
                        break;
                }

                SelectColoB___ = TempB;
                SelectColoF___ = TempF;
                SelectColoA___ = TempA;

                SelectCharCode();
            }

        }

        public int TabChar = -1;
        public int TabColoB = -2;
        public int TabColoF = -2;
        public int TabColoA = -2;
        public int CharPosMode = 0;
        public int CharPosX = 0;
        public int CharPosY = 0;

        public void SelectCharEvent(string KeyName, char KeyChar)
        {
            if (DisplayConfig_.IsOpen)
            {
                DisplayConfig_.ProcessKey(KeyName, KeyChar);
                return;
            }

            switch (KeyName)
            {
                default:
                    if (SelectorState == 1)
                    {
                        if (((int)KeyChar >= 32) && ((int)KeyChar <= 255))
                        {
                            int C = SelectCharGetCode();
                            if (SelectChar___ >= 0)
                            {
                                FavCharSet(KeyChar, C);
                            }
                            else
                            {
                                SelectChar___FavSet(KeyChar);
                            }
                            SelectCharRepaint();
                        }
                    }
                    if (SelectorState == 2)
                    {
                        switch (KeyName)
                        {
                            case "D1":
                                SelectColoA___ = CoreStatic.SetAttribBit(SelectColoA___, 0, !CoreStatic.GetAttribBit(SelectColoA___, 0));
                                SelectCharRepaint();
                                return;
                            case "D2":
                                SelectColoA___ = CoreStatic.SetAttribBit(SelectColoA___, 1, !CoreStatic.GetAttribBit(SelectColoA___, 1));
                                SelectCharRepaint();
                                return;
                            case "D3":
                                SelectColoA___ = CoreStatic.SetAttribBit(SelectColoA___, 2, !CoreStatic.GetAttribBit(SelectColoA___, 2));
                                SelectCharRepaint();
                                return;
                            case "D4":
                                SelectColoA___ = CoreStatic.SetAttribBit(SelectColoA___, 6, !CoreStatic.GetAttribBit(SelectColoA___, 6));
                                SelectCharRepaint();
                                return;
                            case "D5":
                                SelectColoA___ = CoreStatic.SetAttribBit(SelectColoA___, 3, !CoreStatic.GetAttribBit(SelectColoA___, 3));
                                SelectCharRepaint();
                                return;
                            case "D6":
                                SelectColoA___ = CoreStatic.SetAttribBit(SelectColoA___, 4, !CoreStatic.GetAttribBit(SelectColoA___, 4));
                                SelectCharRepaint();
                                return;
                            case "D7":
                                SelectColoA___ = CoreStatic.SetAttribBit(SelectColoA___, 5, !CoreStatic.GetAttribBit(SelectColoA___, 5));
                                SelectCharRepaint();
                                return;
                        }
                    }
                    break;
                case "Resize":
                    SelectCharClose(2);
                    Core_.ScreenRefresh(false);
                    SelectCharInit();
                    break;
                case "UpArrow":
                    SelectCharChange(-2);
                    break;
                case "DownArrow":
                    SelectCharChange(2);
                    break;
                case "LeftArrow":
                    SelectCharChange(-1);
                    break;
                case "RightArrow":
                    SelectCharChange(1);
                    break;
                case "PageUp":
                    SelectCharChange(-3);
                    break;
                case "PageDown":
                    SelectCharChange(3);
                    break;
                case "Home":
                    SelectCharChange(-4);
                    break;
                case "End":
                    SelectCharChange(4);
                    break;
                case "F1":
                    SelectCharChange(-5);
                    break;
                case "F2":
                    SelectCharChange(5);
                    break;
                case "F3":
                    if (SelectorState == 1)
                    {
                        SelectorState = 2;
                    }
                    else
                    {
                        SelectorState = 1;
                    }
                    SelectCharRepaint();
                    break;
                case "F4":
                    if (SelectorState == 1)
                    {
                        WinBitmapEnabled = !WinBitmapEnabled;
                    }
                    if (SelectorState == 2)
                    {
                        DisplayConfigOpen();
                    }
                    SelectCharRepaint();
                    break;
                case "Backspace":
                    if ((SelectChar___ >= 0) || (SelectToFav < 0))
                    {
                        SelectToFav = SelectCharGetCode();
                    }
                    else
                    {
                        if ((SelectChar___ < 0) && (SelectToFav >= 0))
                        {
                            FavCharSet( SelectChar___FavGet(), SelectToFav);
                            SelectToFav = -1;
                        }
                    }
                    SelectCharRepaint();
                    return;
                case "Escape":
                    SelectCharClose(0);
                    Core_.PixelCharGet();
                    Core_.ScreenRefresh(true);
                    return;
                case "Enter":
                    SelectCharClose(1);
                    Core_.PixelCharGet();
                    Core_.ScreenRefresh(true);
                    return;
                case "WindowClose":
                    SelectCharClose(3);
                    Core_.PixelCharGet();
                    Core_.ScreenRefresh(true);
                    return;
                case "Tab":
                    SelectCharClose(2);
                    Core_.ScreenRefresh(true);
                    CharPosMode++;
                    if (CharPosMode == 4)
                    {
                        CharPosMode = 0;
                    }
                    SelectCharInit();
                    return;
                case "Insert":
                    {
                        if (SelectorState == 1)
                        {
                            int C = SelectCharGetCode();
                            if (SelectChar___ >= 0)
                            {
                                SelectChar___FavSet(0);
                                for (int i = 0; i < FavChar.Length; i++)
                                {
                                    if (FavChar[i] == C)
                                    {
                                        SelectChar___FavSet(i);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                SelectChar___ = C;
                            }
                        }
                        if (SelectorState == 2)
                        {
                            if (Core_.ToggleDrawText && Core_.ToggleDrawColo)
                            {
                                Core_.ToggleDrawColo = false;
                            }
                            else
                            {
                                if (Core_.ToggleDrawText)
                                {
                                    Core_.ToggleDrawText = false;
                                    Core_.ToggleDrawColo = true;
                                }
                                else
                                {
                                    Core_.ToggleDrawText = true;
                                    Core_.ToggleDrawColo = true;
                                }
                            }
                        }
                        SelectCharRepaint();
                    }
                    return;
                case "Delete":
                    {
                        if (SelectorState == 1)
                        {
                            if ((CursorChar >= 0) && (CursorChar < MaxCharCode))
                            {
                                SelectChar___ = CursorChar;
                            }
                        }
                        if (SelectorState == 2)
                        {
                            SelectColoB___ = CursorColoB;
                            SelectColoF___ = CursorColoF;
                            SelectColoA___ = CursorColoA;
                        }
                        SelectCharRepaint();
                    }
                    return;
            }
        }
        
        public void SelectCharClose(int Set)
        {
            TabChar = -1;
            TabColoB = -2;
            TabColoF = -2;
            if ((Set == 1) || (Set == 3))
            {
                DrawCharI = SelectCharGetCode();
                DrawCharIdbl = Core_.Screen_.CharDouble(DrawCharI);
                DrawColoBI = SelectColoB___;
                DrawColoFI = SelectColoF___;
                DrawColoAI = SelectColoA___;
                Core_.CoreAnsi_.AnsiMaxX = AnsiMaxX_;
                Core_.CoreAnsi_.AnsiMaxY = AnsiMaxY_;
                Core_.CoreAnsi_.ANSI_CR = ANSI_CR_;
                Core_.CoreAnsi_.ANSI_LF = ANSI_LF_;
                for (int i = 0; i < 11; i++)
                {
                    Frame1C[0][i] = DrawCharI;
                    Frame2C[0][i] = DrawCharI;
                }
                SetFrame(Frame1I, Frame2I);
            }
            if (Set == 2)
            {
                TabChar = SelectChar___;
                TabColoB = SelectColoB___;
                TabColoF = SelectColoF___;
                TabColoA = SelectColoA___;
            }
            if (!DisplayConfig_.IsOpen)
            {
                SelectCharState = false;
                Core_.CoreEvent("", '\0', false, false, false);
                Core_.Screen_.PutChar(Core_.Screen_.WinW - 1, Core_.Screen_.WinH - 1, ' ', Core_.PopupFore, Core_.PopupBack);
                if (Set == 3)
                {
                    Core_.CoreEvent("WindowClose", '\0', false, false, false);
                }
            }
        }

        public int DrawCharI;
        public int DrawCharIdbl;
        public int DrawColoBI;
        public int DrawColoFI;
        public int DrawColoAI;
        public int DrawCharI_;
        public int CursorChar = 32;
        public int CursorColoB = -1;
        public int CursorColoF = -1;
        public int CursorColoA = 0;

        public void DrawCharCustomSet(int C)
        {
            DrawCharI_ = DrawCharI;
            DrawCharI = C;
            DrawCharIdbl = Core_.Screen_.CharDouble(DrawCharI);
        }

        public int DrawCharCustomGet()
        {
            int C = DrawCharI;
            DrawCharI = DrawCharI_;
            DrawCharIdbl = Core_.Screen_.CharDouble(DrawCharI);
            return C;
        }



        public void FrameCharPut1(int X, int Y, bool ForceT, bool ForceB, bool ForceL, bool ForceR, int FontW, int FontH, bool DoubleChar)
        {
            int CharSide = 0;
            List<int> CharT  = new List<int>(new int[] { FrameChar[1],  FrameChar[2],  FrameChar[3],  FrameChar[4],  FrameChar[5],  FrameChar[6],  FrameChar[7]  });
            List<int> CharB  = new List<int>(new int[] { FrameChar[1],  FrameChar[5],  FrameChar[6],  FrameChar[7],  FrameChar[8],  FrameChar[9],  FrameChar[10] });
            List<int> CharL  = new List<int>(new int[] { FrameChar[0],  FrameChar[2],  FrameChar[3],  FrameChar[5],  FrameChar[6],  FrameChar[8],  FrameChar[9]  });
            List<int> CharR  = new List<int>(new int[] { FrameChar[0],  FrameChar[3],  FrameChar[4],  FrameChar[6],  FrameChar[7],  FrameChar[9],  FrameChar[10] });
            CharSide += ((ForceT || CharT.Contains(CharGet(X + 0, Y - FontH))) ? 1 : 0);
            CharSide += ((ForceB || CharB.Contains(CharGet(X + 0, Y + FontH))) ? 2 : 0);
            if (DoubleChar)
            {
                CharSide += ((ForceL || CharL.Contains(CharGet(X - FontW - FontW, Y + 0))) ? 4 : 0);
                CharSide += ((ForceR || CharR.Contains(CharGet(X + FontW + FontW, Y + 0))) ? 8 : 0);
            }
            else
            {
                CharSide += ((ForceL || CharL.Contains(CharGet(X - FontW, Y + 0))) ? 4 : 0);
                CharSide += ((ForceR || CharR.Contains(CharGet(X + FontW, Y + 0))) ? 8 : 0);
            }
            switch (CharSide)
            {
                case 0:  CharPutDbl(X, Y, FrameChar[6], FontW); break;
                case 1:  CharPutDbl(X, Y, FrameChar[1], FontW); break;
                case 2:  CharPutDbl(X, Y, FrameChar[1], FontW); break;
                case 3:  CharPutDbl(X, Y, FrameChar[1], FontW); break;
                case 4:  CharPutDbl(X, Y, FrameChar[0], FontW); break;
                case 5:  CharPutDbl(X, Y, FrameChar[10], FontW); break;
                case 6:  CharPutDbl(X, Y, FrameChar[4], FontW); break;
                case 7:  CharPutDbl(X, Y, FrameChar[7], FontW); break;
                case 8:  CharPutDbl(X, Y, FrameChar[0], FontW); break;
                case 9:  CharPutDbl(X, Y, FrameChar[8], FontW); break;
                case 10: CharPutDbl(X, Y, FrameChar[2], FontW); break;
                case 11: CharPutDbl(X, Y, FrameChar[5], FontW); break;
                case 12: CharPutDbl(X, Y, FrameChar[0], FontW); break;
                case 13: CharPutDbl(X, Y, FrameChar[9], FontW); break;
                case 14: CharPutDbl(X, Y, FrameChar[3], FontW); break;
                case 15: CharPutDbl(X, Y, FrameChar[6], FontW); break;
            }
        }

        public void FrameCharPut2(int X, int Y, bool ForceTR, bool ForceBL, bool ForceTL, bool ForceBR, int FontW, int FontH, bool DoubleChar)
        {
            int CharSide = 0;
            List<int> CharTR = new List<int>(new int[] { FrameChar[12], FrameChar[13], FrameChar[14], FrameChar[15], FrameChar[16], FrameChar[17], FrameChar[18] });
            List<int> CharBL = new List<int>(new int[] { FrameChar[12], FrameChar[16], FrameChar[17], FrameChar[18], FrameChar[19], FrameChar[20], FrameChar[21] });
            List<int> CharTL = new List<int>(new int[] { FrameChar[11], FrameChar[13], FrameChar[14], FrameChar[16], FrameChar[17], FrameChar[19], FrameChar[20] });
            List<int> CharBR = new List<int>(new int[] { FrameChar[11], FrameChar[14], FrameChar[15], FrameChar[17], FrameChar[18], FrameChar[20], FrameChar[21] });
            if (DoubleChar)
            {
                CharSide += ((ForceTR || CharTR.Contains(CharGet(X + FontW + FontW, Y - FontH))) ? 1 : 0);
                CharSide += ((ForceBL || CharBL.Contains(CharGet(X - FontW - FontW, Y + FontH))) ? 2 : 0);
                CharSide += ((ForceTL || CharTL.Contains(CharGet(X - FontW - FontW, Y - FontH))) ? 4 : 0);
                CharSide += ((ForceBR || CharBR.Contains(CharGet(X + FontW + FontW, Y + FontH))) ? 8 : 0);
            }
            else
            {
                CharSide += ((ForceTR || CharTR.Contains(CharGet(X + FontW, Y - FontH))) ? 1 : 0);
                CharSide += ((ForceBL || CharBL.Contains(CharGet(X - FontW, Y + FontH))) ? 2 : 0);
                CharSide += ((ForceTL || CharTL.Contains(CharGet(X - FontW, Y - FontH))) ? 4 : 0);
                CharSide += ((ForceBR || CharBR.Contains(CharGet(X + FontW, Y + FontH))) ? 8 : 0);
            }
            switch (CharSide)
            {
                case 0:  CharPutDbl(X, Y, FrameChar[17], FontW); break;
                case 1:  CharPutDbl(X, Y, FrameChar[12], FontW); break;
                case 2:  CharPutDbl(X, Y, FrameChar[12], FontW); break;
                case 3:  CharPutDbl(X, Y, FrameChar[12], FontW); break;
                case 4:  CharPutDbl(X, Y, FrameChar[11], FontW); break;
                case 5:  CharPutDbl(X, Y, FrameChar[21], FontW); break;
                case 6:  CharPutDbl(X, Y, FrameChar[15], FontW); break;
                case 7:  CharPutDbl(X, Y, FrameChar[18], FontW); break;
                case 8:  CharPutDbl(X, Y, FrameChar[11], FontW); break;
                case 9:  CharPutDbl(X, Y, FrameChar[19], FontW); break;
                case 10: CharPutDbl(X, Y, FrameChar[13], FontW); break;
                case 11: CharPutDbl(X, Y, FrameChar[16], FontW); break;
                case 12: CharPutDbl(X, Y, FrameChar[11], FontW); break;
                case 13: CharPutDbl(X, Y, FrameChar[20], FontW); break;
                case 14: CharPutDbl(X, Y, FrameChar[14], FontW); break;
                case 15: CharPutDbl(X, Y, FrameChar[17], FontW); break;
            }
        }


        public void FrameCharPut(int Dir, int FontW, int FontH, bool CharDbl)
        {
            int FontW_ = CharDbl ? (FontW + FontW) : FontW;
            int CharSide = 0;
            
            List<int> CharT  = new List<int>(new int[] { FrameChar[1],  FrameChar[2],  FrameChar[3],  FrameChar[4],  FrameChar[5],  FrameChar[6],  FrameChar[7]  });
            List<int> CharB  = new List<int>(new int[] { FrameChar[1],  FrameChar[5],  FrameChar[6],  FrameChar[7],  FrameChar[8],  FrameChar[9],  FrameChar[10] });
            List<int> CharL  = new List<int>(new int[] { FrameChar[0],  FrameChar[2],  FrameChar[3],  FrameChar[5],  FrameChar[6],  FrameChar[8],  FrameChar[9]  });
            List<int> CharR  = new List<int>(new int[] { FrameChar[0],  FrameChar[3],  FrameChar[4],  FrameChar[6],  FrameChar[7],  FrameChar[9],  FrameChar[10] });

            List<int> CharTR = new List<int>(new int[] { FrameChar[12], FrameChar[13], FrameChar[14], FrameChar[15], FrameChar[16], FrameChar[17], FrameChar[18] });
            List<int> CharBL = new List<int>(new int[] { FrameChar[12], FrameChar[16], FrameChar[17], FrameChar[18], FrameChar[19], FrameChar[20], FrameChar[21] });
            List<int> CharTL = new List<int>(new int[] { FrameChar[11], FrameChar[13], FrameChar[14], FrameChar[16], FrameChar[17], FrameChar[19], FrameChar[20] });
            List<int> CharBR = new List<int>(new int[] { FrameChar[11], FrameChar[14], FrameChar[15], FrameChar[17], FrameChar[18], FrameChar[20], FrameChar[21] });

            switch (Dir)
            {
                case 0:
                    {
                        CharSide += (CharB.Contains(CharGet( 0, FontH)) ? 1 : 0);
                        CharSide += (CharL.Contains(CharGet(-FontW_,  0)) ? 2 : 0);
                        CharSide += (CharR.Contains(CharGet(FontW_,  0)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPutDbl(0, 0, FrameChar[1], FontW);  break;
                            case 1: CharPutDbl(0, 0, FrameChar[1], FontW);  break;
                            case 2: CharPutDbl(0, 0, FrameChar[10], FontW); break;
                            case 3: CharPutDbl(0, 0, FrameChar[7], FontW);  break;
                            case 4: CharPutDbl(0, 0, FrameChar[8], FontW);  break;
                            case 5: CharPutDbl(0, 0, FrameChar[5], FontW);  break;
                            case 6: CharPutDbl(0, 0, FrameChar[9], FontW);  break;
                            case 7: CharPutDbl(0, 0, FrameChar[6], FontW);  break;
                        }
                    }
                    break;
                case 1:
                    {
                        CharSide += (CharT.Contains(CharGet( 0, -FontH)) ? 1 : 0);
                        CharSide += (CharR.Contains(CharGet(FontW_,  0)) ? 2 : 0);
                        CharSide += (CharL.Contains(CharGet(-FontW_,  0)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPutDbl(0, 0, FrameChar[1], FontW);  break;
                            case 1: CharPutDbl(0, 0, FrameChar[1], FontW);  break;
                            case 2: CharPutDbl(0, 0, FrameChar[2], FontW);  break;
                            case 3: CharPutDbl(0, 0, FrameChar[5], FontW);  break;
                            case 4: CharPutDbl(0, 0, FrameChar[4], FontW);  break;
                            case 5: CharPutDbl(0, 0, FrameChar[7], FontW);  break;
                            case 6: CharPutDbl(0, 0, FrameChar[3], FontW);  break;
                            case 7: CharPutDbl(0, 0, FrameChar[6], FontW);  break;
                        }
                    }
                    break;
                case 2:
                    {
                        CharSide += (CharR.Contains(CharGet(FontW_,  0)) ? 1 : 0);
                        CharSide += (CharB.Contains(CharGet( 0, FontH)) ? 2 : 0);
                        CharSide += (CharT.Contains(CharGet( 0, -FontH)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPutDbl(0, 0, FrameChar[0], FontW);  break;
                            case 1: CharPutDbl(0, 0, FrameChar[0], FontW);  break;
                            case 2: CharPutDbl(0, 0, FrameChar[4], FontW);  break;
                            case 3: CharPutDbl(0, 0, FrameChar[3], FontW);  break;
                            case 4: CharPutDbl(0, 0, FrameChar[10], FontW); break;
                            case 5: CharPutDbl(0, 0, FrameChar[9], FontW);  break;
                            case 6: CharPutDbl(0, 0, FrameChar[7], FontW);  break;
                            case 7: CharPutDbl(0, 0, FrameChar[6], FontW);  break;
                        }
                    }
                    break;
                case 3:
                    {
                        CharSide += (CharL.Contains(CharGet(-FontW_,  0)) ? 1 : 0);
                        CharSide += (CharT.Contains(CharGet( 0, -FontH)) ? 2 : 0);
                        CharSide += (CharB.Contains(CharGet( 0, FontH)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPutDbl(0, 0, FrameChar[0], FontW);  break;
                            case 1: CharPutDbl(0, 0, FrameChar[0], FontW);  break;
                            case 2: CharPutDbl(0, 0, FrameChar[8], FontW);  break;
                            case 3: CharPutDbl(0, 0, FrameChar[9], FontW);  break;
                            case 4: CharPutDbl(0, 0, FrameChar[2], FontW);  break;
                            case 5: CharPutDbl(0, 0, FrameChar[3], FontW);  break;
                            case 6: CharPutDbl(0, 0, FrameChar[5], FontW);  break;
                            case 7: CharPutDbl(0, 0, FrameChar[6], FontW);  break;
                        }
                    }
                    break;
                case 4:
                    {
                        CharSide += (CharBL.Contains(CharGet(-FontW_, FontH)) ? 1 : 0);
                        CharSide += (CharTL.Contains(CharGet(-FontW_, -FontH)) ? 2 : 0);
                        CharSide += (CharBR.Contains(CharGet(FontW_, FontH)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPutDbl(0, 0, FrameChar[12], FontW); break;
                            case 1: CharPutDbl(0, 0, FrameChar[12], FontW); break;
                            case 2: CharPutDbl(0, 0, FrameChar[21], FontW); break;
                            case 3: CharPutDbl(0, 0, FrameChar[18], FontW); break;
                            case 4: CharPutDbl(0, 0, FrameChar[19], FontW); break;
                            case 5: CharPutDbl(0, 0, FrameChar[16], FontW); break;
                            case 6: CharPutDbl(0, 0, FrameChar[20], FontW); break;
                            case 7: CharPutDbl(0, 0, FrameChar[17], FontW); break;
                        }
                    }
                    break;
                case 5:
                    {
                        CharSide += (CharTR.Contains(CharGet(FontW_, -FontH)) ? 1 : 0);
                        CharSide += (CharBR.Contains(CharGet(FontW_, FontH)) ? 2 : 0);
                        CharSide += (CharTL.Contains(CharGet(-FontW_, -FontH)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPutDbl(0, 0, FrameChar[12], FontW); break;
                            case 1: CharPutDbl(0, 0, FrameChar[12], FontW); break;
                            case 2: CharPutDbl(0, 0, FrameChar[13], FontW); break;
                            case 3: CharPutDbl(0, 0, FrameChar[16], FontW); break;
                            case 4: CharPutDbl(0, 0, FrameChar[15], FontW); break;
                            case 5: CharPutDbl(0, 0, FrameChar[18], FontW); break;
                            case 6: CharPutDbl(0, 0, FrameChar[14], FontW); break;
                            case 7: CharPutDbl(0, 0, FrameChar[17], FontW); break;
                        }
                    }
                    break;
                case 6:
                    {
                        CharSide += (CharTL.Contains(CharGet(FontW_, FontH)) ? 1 : 0);
                        CharSide += (CharTR.Contains(CharGet(-FontW_, FontH)) ? 2 : 0);
                        CharSide += (CharBL.Contains(CharGet(FontW_, -FontH)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPutDbl(0, 0, FrameChar[11], FontW); break;
                            case 1: CharPutDbl(0, 0, FrameChar[11], FontW); break;
                            case 2: CharPutDbl(0, 0, FrameChar[15], FontW); break;
                            case 3: CharPutDbl(0, 0, FrameChar[14], FontW); break;
                            case 4: CharPutDbl(0, 0, FrameChar[21], FontW); break;
                            case 5: CharPutDbl(0, 0, FrameChar[20], FontW); break;
                            case 6: CharPutDbl(0, 0, FrameChar[18], FontW); break;
                            case 7: CharPutDbl(0, 0, FrameChar[17], FontW); break;
                        }
                    }
                    break;
                case 7:
                    {
                        CharSide += (CharTL.Contains(CharGet(-FontW_, -FontH)) ? 1 : 0);
                        CharSide += (CharTR.Contains(CharGet(FontW_, -FontH)) ? 2 : 0);
                        CharSide += (CharBL.Contains(CharGet(-FontW_, FontH)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPutDbl(0, 0, FrameChar[11], FontW); break;
                            case 1: CharPutDbl(0, 0, FrameChar[11], FontW); break;
                            case 2: CharPutDbl(0, 0, FrameChar[19], FontW); break;
                            case 3: CharPutDbl(0, 0, FrameChar[20], FontW); break;
                            case 4: CharPutDbl(0, 0, FrameChar[13], FontW); break;
                            case 5: CharPutDbl(0, 0, FrameChar[14], FontW); break;
                            case 6: CharPutDbl(0, 0, FrameChar[16], FontW); break;
                            case 7: CharPutDbl(0, 0, FrameChar[17], FontW); break;
                        }
                    }
                    break;
            }
        }
        
        public bool DrawCharEraseFrame = true;
        
        public void FrameCharErase()
        {
            CharPut(0, 0, DrawCharI);
            if (DrawCharEraseFrame)
            {
                int FrameCharN_T = -1;
                int FrameCharN_B = -1;
                int FrameCharN_L = -1;
                int FrameCharN_R = -1;
                int FrameCharN_TR = -1;
                int FrameCharN_BL = -1;
                int FrameCharN_TL = -1;
                int FrameCharN_BR = -1;
                for (int i = 0; i < FrameChar.Length; i++)
                {
                    if (CharGet( 0, -1) == FrameChar[i]) { FrameCharN_T  = i; }
                    if (CharGet( 0,  1) == FrameChar[i]) { FrameCharN_B  = i; }
                    if (CharGet(-1,  0) == FrameChar[i]) { FrameCharN_L  = i; }
                    if (CharGet( 1,  0) == FrameChar[i]) { FrameCharN_R  = i; }
    
                    if (CharGet( 1, -1) == FrameChar[i]) { FrameCharN_TR = i; }
                    if (CharGet(-1,  1) == FrameChar[i]) { FrameCharN_BL = i; }
                    if (CharGet(-1, -1) == FrameChar[i]) { FrameCharN_TL = i; }
                    if (CharGet( 1,  1) == FrameChar[i]) { FrameCharN_BR = i; }
                }
                switch (FrameCharN_T)
                {
                    case 2:  CharPut( 0, -1, FrameChar[0]);  break;
                    case 5:  CharPut( 0, -1, FrameChar[8]);  break;
                    case 3:  CharPut( 0, -1, FrameChar[0]);  break;
                    case 6:  CharPut( 0, -1, FrameChar[9]);  break;
                    case 4:  CharPut( 0, -1, FrameChar[0]);  break;
                    case 7:  CharPut( 0, -1, FrameChar[10]); break;
                }
                switch (FrameCharN_B)
                {
                    case 8:  CharPut( 0,  1, FrameChar[0]);  break;
                    case 5:  CharPut( 0,  1, FrameChar[2]);  break;
                    case 9:  CharPut( 0,  1, FrameChar[0]);  break;
                    case 6:  CharPut( 0,  1, FrameChar[3]);  break;
                    case 10: CharPut( 0,  1, FrameChar[0]);  break;
                    case 7:  CharPut( 0,  1, FrameChar[4]);  break;
                }
                switch (FrameCharN_L)
                {
                    case 2:  CharPut(-1,  0, FrameChar[1]);  break;
                    case 3:  CharPut(-1,  0, FrameChar[4]);  break;
                    case 5:  CharPut(-1,  0, FrameChar[1]);  break;
                    case 6:  CharPut(-1,  0, FrameChar[7]);  break;
                    case 8:  CharPut(-1,  0, FrameChar[1]);  break;
                    case 9:  CharPut(-1,  0, FrameChar[10]); break;
                }
                switch (FrameCharN_R)
                {
                    case 4:  CharPut( 1,  0, FrameChar[1]);  break;
                    case 3:  CharPut( 1,  0, FrameChar[2]);  break;
                    case 7:  CharPut( 1,  0, FrameChar[1]);  break;
                    case 6:  CharPut( 1,  0, FrameChar[5]);  break;
                    case 10: CharPut( 1,  0, FrameChar[1]);  break;
                    case 9:  CharPut( 1,  0, FrameChar[8]);  break;
                }
                switch (FrameCharN_TR)
                {
                    case 13: CharPut( 1, -1, FrameChar[11]); break;
                    case 16: CharPut( 1, -1, FrameChar[19]); break;
                    case 14: CharPut( 1, -1, FrameChar[11]); break;
                    case 17: CharPut( 1, -1, FrameChar[20]); break;
                    case 15: CharPut( 1, -1, FrameChar[11]); break;
                    case 18: CharPut( 1, -1, FrameChar[21]); break;
                }
                switch (FrameCharN_BL)
                {
                    case 19: CharPut(-1,  1, FrameChar[11]); break;
                    case 16: CharPut(-1,  1, FrameChar[13]); break;
                    case 20: CharPut(-1,  1, FrameChar[11]); break;
                    case 17: CharPut(-1,  1, FrameChar[14]); break;
                    case 21: CharPut(-1,  1, FrameChar[11]); break;
                    case 18: CharPut(-1,  1, FrameChar[15]); break;
                }
                switch (FrameCharN_TL)
                {
                    case 13: CharPut(-1, -1, FrameChar[12]); break;
                    case 14: CharPut(-1, -1, FrameChar[15]); break;
                    case 16: CharPut(-1, -1, FrameChar[12]); break;
                    case 17: CharPut(-1, -1, FrameChar[18]); break;
                    case 19: CharPut(-1, -1, FrameChar[12]); break;
                    case 20: CharPut(-1, -1, FrameChar[21]); break;
                }
                switch (FrameCharN_BR)
                {
                    case 15: CharPut( 1,  1, FrameChar[12]); break;
                    case 14: CharPut( 1,  1, FrameChar[13]); break;
                    case 18: CharPut( 1,  1, FrameChar[12]); break;
                    case 17: CharPut( 1,  1, FrameChar[16]); break;
                    case 21: CharPut( 1,  1, FrameChar[12]); break;
                    case 20: CharPut( 1,  1, FrameChar[19]); break;
                }
            }
        }

        public void CharPutDbl(int X, int Y, int C, int Offset)
        {
            AnsiLineOccupyItem Item = new AnsiLineOccupyItem();
            Item.BlankChar();
            Item.Item_Char = C;
            Item.Item_ColorB = DrawColoBI;
            Item.Item_ColorF = DrawColoFI;
            Core_.CharPut(Core_.CursorX + X, Core_.CursorY + Y, Item, false);

            int C_ = Core_.Screen_.CharDouble(C);
            if (C_ != 0)
            {
                AnsiLineOccupyItem Item_ = new AnsiLineOccupyItem();
                Item_.BlankChar();
                Item_.Item_Char = C_;
                Item_.Item_ColorB = DrawColoBI;
                Item_.Item_ColorF = DrawColoFI;
                Core_.CharPut(Core_.CursorX + X + Offset, Core_.CursorY + Y, Item_, false);
            }
        }

        public void CharPut(int X, int Y, int C)
        {
            AnsiLineOccupyItem Item = new AnsiLineOccupyItem();
            Item.BlankChar();
            Item.Item_Char = C;
            Item.Item_ColorB = DrawColoBI;
            Item.Item_ColorF = DrawColoFI;
            Core_.CharPut(Core_.CursorX + X, Core_.CursorY + Y, Item, false);
        }

        public int CharGet(int X, int Y)
        {
            return Core_.CharGet(Core_.CursorX + X, Core_.CursorY + Y, true, false);
        }

        public bool DoubleDrawMode()
        {
            if (Frame1I == 0)
            {
                if (DrawCharIdbl != 0)
                {
                    return true;
                }
            }
            else
            {
                if (DiamondType == 0)
                {
                    for (int i = 0; i <= 10; i++)
                    {
                        if (Core_.Screen_.CharDouble(FrameChar[i]) != 0)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    for (int i = 11; i <= 21; i++)
                    {
                        if (Core_.Screen_.CharDouble(FrameChar[i]) != 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void RectangleDraw(int X, int Y, int W__, int H__, int T, int FontW, int FontH)
        {
            int W = W__ * FontW;
            int H = H__ * FontH;
            if (W < 0)
            {
                RectangleDraw(X + W, Y, 0 - W__, H__, T, FontW, FontH);
                return;
            }
            if (H < 0)
            {
                RectangleDraw(X, Y + H, W__, 0 - H__, T, FontW, FontH);
                return;
            }

            bool DoubleCharMode = DoubleDrawMode();
            if (DoubleCharMode)
            {
                if ((W % (FontW << 1)) == 0)
                {
                    W -= FontW;
                }
            }
            if (W <= 0)
            {
                return;
            }

            if (T == 1)
            {
                if (Frame1I == 0)
                {
                    if (DoubleCharMode)
                    {
                        for (int i = 0; i < W; i += (FontW << 1))
                        {
                            CharPut(X + i, Y + 0, DrawCharI);
                            CharPut(X + i + FontW, Y + 0, DrawCharIdbl);
                            CharPut(X + i, Y + H, DrawCharI);
                            CharPut(X + i + FontW, Y + H, DrawCharIdbl);
                        }
                        for (int i = 0; i <= H; i += FontH)
                        {
                            CharPut(X + 0, Y + i, DrawCharI);
                            CharPut(X + FontW, Y + i, DrawCharIdbl);
                            CharPut(X + W - FontW, Y + i, DrawCharI);
                            CharPut(X + W - 0, Y + i, DrawCharIdbl);
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= W; i += FontW)
                        {
                            CharPut(X + i, Y + 0, DrawCharI);
                            CharPut(X + i, Y + H, DrawCharI);
                        }
                        for (int i = 0; i <= H; i += FontH)
                        {
                            CharPut(X + 0, Y + i, DrawCharI);
                            CharPut(X + W, Y + i, DrawCharI);
                        }
                    }
                }
                else
                {
                    if (DoubleCharMode)
                    {
                        for (int i = (FontW << 1); i < (W - FontW); i += (FontW << 1))
                        {
                            FrameCharPut1(X + i, Y + 0, false, false, true, true, FontW, FontH, true);
                            FrameCharPut1(X + i, Y + H, false, false, true, true, FontW, FontH, true);
                        }
                        for (int i = FontH; i < H; i += FontH)
                        {
                            FrameCharPut1(X + 0, Y + i, true, true, false, false, FontW, FontH, true);
                            FrameCharPut1(X + W - FontW, Y + i, true, true, false, false, FontW, FontH, true);
                        }
                        if ((W != 1) && (H != 0))
                        {
                            FrameCharPut1(X + 0, Y + 0, false, true, false, true, FontW, FontH, true);
                            FrameCharPut1(X + W - FontW, Y + 0, false, true, true, false, FontW, FontH, true);
                            FrameCharPut1(X + 0, Y + H, true, false, false, true, FontW, FontH, true);
                            FrameCharPut1(X + W - FontW, Y + H, true, false, true, false, FontW, FontH, true);
                        }
                        if ((W == 1) && (H != 0))
                        {
                            FrameCharPut1(X + 0, Y + 0, false, true, false, false, FontW, FontH, true);
                            FrameCharPut1(X + 0, Y + H, true, false, false, false, FontW, FontH, true);
                        }
                        if ((W != 1) && (H == 0))
                        {
                            FrameCharPut1(X + 0, Y + 0, false, false, false, true, FontW, FontH, true);
                            FrameCharPut1(X + W - FontW, Y + 0, false, false, true, false, FontW, FontH, true);
                        }
                        if ((W == 1) && (H == 0))
                        {
                            FrameCharPut1(X + 0, Y + 0, false, false, false, false, FontW, FontH, true);
                        }
                    }
                    else
                    {
                        for (int i = FontW; i < W; i += FontW)
                        {
                            FrameCharPut1(X + i, Y + 0, false, false, true, true, FontW, FontH, false);
                            FrameCharPut1(X + i, Y + H, false, false, true, true, FontW, FontH, false);
                        }
                        for (int i = FontH; i < H; i += FontH)
                        {
                            FrameCharPut1(X + 0, Y + i, true, true, false, false, FontW, FontH, false);
                            FrameCharPut1(X + W, Y + i, true, true, false, false, FontW, FontH, false);
                        }
                        if ((W != 0) && (H != 0))
                        {
                            FrameCharPut1(X + 0, Y + 0, false, true, false, true, FontW, FontH, false);
                            FrameCharPut1(X + W, Y + 0, false, true, true, false, FontW, FontH, false);
                            FrameCharPut1(X + 0, Y + H, true, false, false, true, FontW, FontH, false);
                            FrameCharPut1(X + W, Y + H, true, false, true, false, FontW, FontH, false);
                        }
                        if ((W == 0) && (H != 0))
                        {
                            FrameCharPut1(X + 0, Y + 0, false, true, false, false, FontW, FontH, false);
                            FrameCharPut1(X + 0, Y + H, true, false, false, false, FontW, FontH, false);
                        }
                        if ((W != 0) && (H == 0))
                        {
                            FrameCharPut1(X + 0, Y + 0, false, false, false, true, FontW, FontH, false);
                            FrameCharPut1(X + W, Y + 0, false, false, true, false, FontW, FontH, false);
                        }
                        if ((W == 0) && (H == 0))
                        {
                            FrameCharPut1(X + 0, Y + 0, false, false, false, false, FontW, FontH, false);
                        }
                    }
                }
            }
            if (T == 2)
            {
                if (Frame1I == 0)
                {
                    if (DoubleCharMode)
                    {
                        for (int i = 0; i <= H; i += FontH)
                        {
                            for (int ii = 0; ii <= W; ii += (FontW << 1))
                            {
                                CharPut(X + ii, Y + i, DrawCharI);
                                CharPut(X + ii + FontW, Y + i, DrawCharIdbl);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= H; i += FontH)
                        {
                            for (int ii = 0; ii <= W; ii += FontW)
                            {
                                CharPut(X + ii, Y + i, DrawCharI);
                            }
                        }
                    }
                }
                else
                {
                    if (DoubleCharMode)
                    {
                        if ((W != 0) && (H != 0))
                        {
                            for (int i = (FontW << 1); i < W; i += (FontW << 1))
                            {
                                FrameCharPut1(X + i, Y + 0, false, true, true, true, FontW, FontH, true);
                                FrameCharPut1(X + i, Y + H, true, false, true, true, FontW, FontH, true);
                            }
                            for (int i = FontH; i < H; i += FontH)
                            {
                                FrameCharPut1(X + 0, Y + i, true, true, false, true, FontW, FontH, true);
                                FrameCharPut1(X + W - FontW, Y + i, true, true, true, false, FontW, FontH, true);
                            }
                            for (int i_Y = FontH; i_Y < H; i_Y += FontH)
                            {
                                for (int i_X = (FontW << 1); i_X < W; i_X += (FontW << 1))
                                {
                                    FrameCharPut1(X + i_X, Y + i_Y, true, true, true, true, FontW, FontH, true);
                                }
                            }
                            FrameCharPut1(X + 0, Y + 0, false, true, false, true, FontW, FontH, true);
                            FrameCharPut1(X + W - FontW, Y + 0, false, true, true, false, FontW, FontH, true);
                            FrameCharPut1(X + 0, Y + H, true, false, false, true, FontW, FontH, true);
                            FrameCharPut1(X + W - FontW, Y + H, true, false, true, false, FontW, FontH, true);
                        }
                        if ((W == 1) && (H != 0))
                        {
                            for (int i = FontH; i < H; i += FontH)
                            {
                                FrameCharPut1(X + 0, Y + i, true, true, false, false, FontW, FontH, true);
                                FrameCharPut1(X + W - FontW, Y + i, true, true, false, false, FontW, FontH, true);
                            }
                            FrameCharPut1(X + 0, Y + 0, false, true, false, false, FontW, FontH, true);
                            FrameCharPut1(X + 0, Y + H, true, false, false, false, FontW, FontH, true);
                        }
                        if ((W != 1) && (H == 0))
                        {
                            for (int i = (FontW << 1); i < W; i += (FontW << 1))
                            {
                                FrameCharPut1(X + i, Y + 0, false, false, true, true, FontW, FontH, true);
                                FrameCharPut1(X + i, Y + H, false, false, true, true, FontW, FontH, true);
                            }
                            FrameCharPut1(X + 0, Y + 0, false, false, false, true, FontW, FontH, true);
                            FrameCharPut1(X + W, Y + 0, false, false, true, false, FontW, FontH, true);
                        }
                        if ((W == 1) && (H == 0))
                        {
                            FrameCharPut1(X + 0, Y + 0, false, false, false, false, FontW, FontH, true);
                        }
                    }
                    else
                    {
                        if ((W != 0) && (H != 0))
                        {
                            for (int i = FontW; i < W; i += FontW)
                            {
                                FrameCharPut1(X + i, Y + 0, false, true, true, true, FontW, FontH, false);
                                FrameCharPut1(X + i, Y + H, true, false, true, true, FontW, FontH, false);
                            }
                            for (int i = FontH; i < H; i += FontH)
                            {
                                FrameCharPut1(X + 0, Y + i, true, true, false, true, FontW, FontH, false);
                                FrameCharPut1(X + W, Y + i, true, true, true, false, FontW, FontH, false);
                            }
                            for (int i_Y = FontH; i_Y < H; i_Y += FontH)
                            {
                                for (int i_X = FontW; i_X < W; i_X += FontW)
                                {
                                    FrameCharPut1(X + i_X, Y + i_Y, true, true, true, true, FontW, FontH, false);
                                }
                            }
                            FrameCharPut1(X + 0, Y + 0, false, true, false, true, FontW, FontH, false);
                            FrameCharPut1(X + W, Y + 0, false, true, true, false, FontW, FontH, false);
                            FrameCharPut1(X + 0, Y + H, true, false, false, true, FontW, FontH, false);
                            FrameCharPut1(X + W, Y + H, true, false, true, false, FontW, FontH, false);
                        }
                        if ((W == 0) && (H != 0))
                        {
                            for (int i = FontH; i < H; i += FontH)
                            {
                                FrameCharPut1(X + 0, Y + i, true, true, false, false, FontW, FontH, false);
                                FrameCharPut1(X + W, Y + i, true, true, false, false, FontW, FontH, false);
                            }
                            FrameCharPut1(X + 0, Y + 0, false, true, false, false, FontW, FontH, false);
                            FrameCharPut1(X + 0, Y + H, true, false, false, false, FontW, FontH, false);
                        }
                        if ((W != 0) && (H == 0))
                        {
                            for (int i = FontW; i < W; i += FontW)
                            {
                                FrameCharPut1(X + i, Y + 0, false, false, true, true, FontW, FontH, false);
                                FrameCharPut1(X + i, Y + H, false, false, true, true, FontW, FontH, false);
                            }
                            FrameCharPut1(X + 0, Y + 0, false, false, false, true, FontW, FontH, false);
                            FrameCharPut1(X + W, Y + 0, false, false, true, false, FontW, FontH, false);
                        }
                        if ((W == 0) && (H == 0))
                        {
                            FrameCharPut1(X + 0, Y + 0, false, false, false, false, FontW, FontH, false);
                        }
                    }
                }
            }
        }

        public int DiamondType = 0;

        public void DiamondDraw(int X, int Y, int W__, int H__, int T, int TT, int FontW, int FontH)
        {
            int W = W__ * FontW;
            int H = H__ * FontH;
            int W_ = W__ * FontH;
            int H_ = H__ * FontW;
            if (W < 0)
            {
                DiamondDraw(X + W, Y + W_, 0 - W__, H__, T, TT, FontW, FontH);
                return;
            }
            if (H < 0)
            {
                DiamondDraw(X - H_, Y + H, W__, 0 - H__, T, TT, FontW, FontH);
                return;
            }

            if (TT < 0)
            {
                switch (DiamondType)
                {
                    case 1: DiamondDraw(X +     0, Y +     0, W__, H__, T, 0, FontW, FontH); return;
                    case 2: DiamondDraw(X +     0, Y +     0, W__, H__, T, 1, FontW, FontH); return;
                    case 3: DiamondDraw(X +     0, Y +     0, W__, H__, T, 2, FontW, FontH); return;
                    case 4: DiamondDraw(X - FontW, Y +     0, W__, H__, T, 1, FontW, FontH); return;
                    case 5: DiamondDraw(X +     0, Y - FontH, W__, H__, T, 2, FontW, FontH); return;
                    case 6: DiamondDraw(X +     0, Y +     0, W__, H__, T, 3, FontW, FontH); return;
                    case 7: DiamondDraw(X +     0, Y + FontH, W__, H__, T, 3, FontW, FontH); return;
                    case 8: DiamondDraw(X - FontW, Y + FontH, W__, H__, T, 3, FontW, FontH); return;
                    case 9: DiamondDraw(X - FontW, Y +     0, W__, H__, T, 3, FontW, FontH); return;
                }
            }

            bool DoubleCharMode = DoubleDrawMode();

            if (T == 1)
            {
                if (Frame2I == 0)
                {
                    if (DoubleCharMode)
                    {
                        if ((TT == 1) || (TT == 3))
                        {
                            int TX = 0;
                            int TY = 0;
                            if (TT == 3)
                            {
                                TY = FontH;
                            }
                            int ii = 0;
                            for (int i = 0; i <= W; i += FontW)
                            {
                                CharPut(X + i + TX, Y + ii - TY, DrawCharI);
                                CharPut(X + i + TX + FontW, Y + ii - TY, DrawCharIdbl);
                                CharPut(X + i - H_, Y + ii + H, DrawCharI);
                                CharPut(X + i - H_ + FontW, Y + ii + H, DrawCharIdbl);
                                ii += FontH;
                            }
                            ii = 0;
                            for (int i = 0; i <= H; i += FontH)
                            {
                                CharPut(X - ii, Y + i - TY, DrawCharI);
                                CharPut(X - ii + FontW, Y + i - TY, DrawCharIdbl);
                                CharPut(X + W - ii + TX, Y + i + W_, DrawCharI);
                                CharPut(X + W - ii + TX + FontW, Y + i + W_, DrawCharIdbl);
                                ii += FontW;
                            }
                        }
                    }
                    else
                    {
                        if (TT == 3)
                        {
                            int ii = 0;
                            for (int i = 0; i <= W; i += FontW)
                            {
                                CharPut(X + i + FontW, Y + ii - FontH, DrawCharI);
                                CharPut(X + i - H_, Y + ii + H, DrawCharI);
                                ii += FontH;
                            }
                            ii = 0;
                            for (int i = 0; i <= H; i += FontH)
                            {
                                CharPut(X - ii, Y + i - FontH, DrawCharI);
                                CharPut(X + W - ii + FontW, Y + i + W_, DrawCharI);
                                ii += FontW;
                            }
                        }
                        else
                        {
                            int TX = 0;
                            int TY = 0;
                            if (TT == 1)
                            {
                                TX = FontW;
                            }
                            if (TT == 2)
                            {
                                TY = FontH;
                            }
                            int ii = 0;
                            for (int i = 0; i <= W; i += FontW)
                            {
                                CharPut(X + i + TX, Y + ii, DrawCharI);
                                CharPut(X + i - H_, Y + ii + H + TY, DrawCharI);
                                ii += FontH;
                            }
                            ii = 0;
                            for (int i = 0; i <= H; i += FontH)
                            {
                                CharPut(X - ii, Y + i, DrawCharI);
                                CharPut(X + W - ii + TX, Y + i + W_ + TY, DrawCharI);
                                ii += FontW;
                            }
                        }
                    }
                }
                else
                {
                    if (DoubleCharMode)
                    {
                        if ((TT == 1) || (TT == 3))
                        {
                            int TX = 0;
                            int TY = 0;
                            bool StdCfg = true;

                            if (TT == 1)
                            {
                                if ((W == 0) && (H == 0))
                                {
                                    FrameCharPut2(X, Y, false, false, false, false, FontW, FontH, true);
                                    StdCfg = false;
                                }
                                else
                                {
                                    if ((W > 0) && (H == 0))
                                    {
                                        int ii = 0;
                                        for (int i = 0; i <= W; i += FontW)
                                        {
                                            FrameCharPut2(X + i + TX, Y + ii, false, false, true, true, FontW, FontH, true);
                                            ii += FontH;
                                        }
                                        StdCfg = false;
                                    }
                                    if ((W == 0) && (H > 0))
                                    {
                                        int ii = 0;
                                        for (int i = 0; i <= H; i += FontH)
                                        {
                                            FrameCharPut2(X - ii, Y + i, true, true, false, false, FontW, FontH, true);
                                            ii += FontW;
                                        }
                                        StdCfg = false;
                                    }
                                }
                            }

                            if (StdCfg)
                            {
                                if (TT == 3)
                                {
                                    TY = FontH;
                                }

                                int ii = FontH;
                                for (int i = FontW; i < W; i += FontW)
                                {
                                    FrameCharPut2(X + i + TX, Y + ii - TY, false, false, true, true, FontW, FontH, true);
                                    FrameCharPut2(X + i - H_, Y + ii + H, false, false, true, true, FontW, FontH, true);
                                    ii += FontH;
                                }
                                if (TT == 3)
                                {
                                    FrameCharPut2(X - H_, Y + H, false, false, true, true, FontW, FontH, true);
                                    FrameCharPut2(X + W + TX, Y + W_ - TY, false, false, true, true, FontW, FontH, true);
                                }

                                ii = FontW;
                                for (int i = FontH; i < H; i += FontH)
                                {
                                    FrameCharPut2(X - ii, Y + i - TY, true, true, false, false, FontW, FontH, true);
                                    FrameCharPut2(X + W - ii + TX, Y + i + W_, true, true, false, false, FontW, FontH, true);
                                    ii += FontW;
                                }
                                if (TT == 3)
                                {
                                    FrameCharPut2(X + W + TX, Y + W_, true, true, false, false, FontW, FontH, true);
                                    FrameCharPut2(X - H_, Y + H - TY, true, true, false, false, FontW, FontH, true);
                                }

                                FrameCharPut2(X, Y - TY, false, true, false, true, FontW, FontH, true);
                                FrameCharPut2(X + W - H_, Y + W_ + H, true, false, true, false, FontW, FontH, true);
                                if (TT == 1)
                                {
                                    FrameCharPut2(X + W + TX, Y + W_, false, true, true, false, FontW, FontH, true);
                                    FrameCharPut2(X - H_, Y + H, true, false, false, true, FontW, FontH, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (TT == 3)
                        {
                            int ii = 0;
                            for (int i = 0; i <= W; i += FontW)
                            {
                                FrameCharPut2(X + i + FontW, Y + ii - FontH, false, false, true, true, FontW, FontH, false);
                                FrameCharPut2(X + i - H_, Y + ii + H, false, false, true, true, FontW, FontH, false);
                                ii += FontH;
                            }
                            ii = 0;
                            for (int i = 0; i <= H; i += FontH)
                            {
                                FrameCharPut2(X - ii, Y + i - FontH, true, true, false, false, FontW, FontH, false);
                                FrameCharPut2(X + W - ii + FontW, Y + i + W_, true, true, false, false, FontW, FontH, false);
                                ii += FontW;
                            }
                        }
                        else
                        {
                            int TX = 0;
                            int TY = 0;
                            bool StdCfg = true;

                            if (TT == 0)
                            {
                                if (StdCfg)
                                {

                                    if (TT == 1)
                                    {
                                        TX = FontW;
                                    }
                                    if (TT == 2)
                                    {
                                        TY = FontH;
                                    }

                                    int ii = FontH;
                                    for (int i = FontW; i < W; i += FontW)
                                    {
                                        FrameCharPut2(X + i + TX, Y + ii, false, false, true, true, FontW, FontH, false);
                                        FrameCharPut2(X + i - H_, Y + ii + H + TY, false, false, true, true, FontW, FontH, false);
                                        ii += FontH;
                                    }
                                    if (TT == 1)
                                    {
                                        FrameCharPut2(X + TX, Y, false, false, true, true, FontW, FontH, false);
                                        FrameCharPut2(X + W - H_, Y + W_ + H + TY, false, false, true, true, FontW, FontH, false);
                                    }
                                    if (TT == 2)
                                    {
                                        FrameCharPut2(X - H_, Y + H + TY, false, false, true, true, FontW, FontH, false);
                                        FrameCharPut2(X + W + TX, Y + W_, false, false, true, true, FontW, FontH, false);
                                    }

                                    ii = FontW;
                                    for (int i = FontH; i < H; i += FontH)
                                    {
                                        FrameCharPut2(X - ii, Y + i, true, true, false, false, FontW, FontH, false);
                                        FrameCharPut2(X + W - ii + TX, Y + i + W_ + TY, true, true, false, false, FontW, FontH, false);
                                        ii += FontW;
                                    }
                                    if (TT == 1)
                                    {
                                        FrameCharPut2(X, Y, true, true, false, false, FontW, FontH, false);
                                        FrameCharPut2(X + W - H_ + TX, Y + H + W_ + TY, true, true, false, false, FontW, FontH, false);
                                    }
                                    if (TT == 2)
                                    {
                                        FrameCharPut2(X + W + TX, Y + W_ + TY, true, true, false, false, FontW, FontH, false);
                                        FrameCharPut2(X - H_, Y + H, true, true, false, false, FontW, FontH, false);
                                    }

                                    if (TT != 1)
                                    {
                                        FrameCharPut2(X, Y, false, true, false, true, FontW, FontH, false);
                                        FrameCharPut2(X + W - H_, Y + W_ + H + TY, true, false, true, false, FontW, FontH, false);
                                    }
                                    if (TT != 2)
                                    {
                                        FrameCharPut2(X + W + TX, Y + W_, false, true, true, false, FontW, FontH, false);
                                        FrameCharPut2(X - H_, Y + H, true, false, false, true, FontW, FontH, false);
                                    }
                                }
                                if ((W == 0) && (H == 0))
                                {
                                    FrameCharPut2(X, Y, false, false, false, false, FontW, FontH, false);
                                    StdCfg = false;
                                }
                                else
                                {
                                    if ((W > 0) && (H == 0))
                                    {
                                        int ii = 0;
                                        for (int i = 0; i <= W; i += FontW)
                                        {
                                            FrameCharPut2(X + i + TX, Y + ii, false, false, true, true, FontW, FontH, false);
                                            ii += FontH;
                                        }
                                        StdCfg = false;
                                    }
                                    if ((W == 0) && (H > 0))
                                    {
                                        int ii = 0;
                                        for (int i = 0; i <= H; i += FontH)
                                        {
                                            FrameCharPut2(X - ii, Y + i, true, true, false, false, FontW, FontH, false);
                                            ii += FontW;
                                        }
                                        StdCfg = false;
                                    }
                                }
                            }

                            if (StdCfg)
                            {

                                if (TT == 1)
                                {
                                    TX = FontW;
                                }
                                if (TT == 2)
                                {
                                    TY = FontH;
                                }

                                int ii = FontH;
                                for (int i = FontW; i < W; i += FontW)
                                {
                                    FrameCharPut2(X + i + TX, Y + ii, false, false, true, true, FontW, FontH, false);
                                    FrameCharPut2(X + i - H_, Y + ii + H + TY, false, false, true, true, FontW, FontH, false);
                                    ii += FontH;
                                }
                                if (TT == 1)
                                {
                                    FrameCharPut2(X + TX, Y, false, false, true, true, FontW, FontH, false);
                                    FrameCharPut2(X + W - H_, Y + W_ + H + TY, false, false, true, true, FontW, FontH, false);
                                }
                                if (TT == 2)
                                {
                                    FrameCharPut2(X - H_, Y + H + TY, false, false, true, true, FontW, FontH, false);
                                    FrameCharPut2(X + W + TX, Y + W_, false, false, true, true, FontW, FontH, false);
                                }

                                ii = FontW;
                                for (int i = FontH; i < H; i += FontH)
                                {
                                    FrameCharPut2(X - ii, Y + i, true, true, false, false, FontW, FontH, false);
                                    FrameCharPut2(X + W - ii + TX, Y + i + W_ + TY, true, true, false, false, FontW, FontH, false);
                                    ii += FontW;
                                }
                                if (TT == 1)
                                {
                                    FrameCharPut2(X, Y, true, true, false, false, FontW, FontH, false);
                                    FrameCharPut2(X + W - H_ + TX, Y + H + W_ + TY, true, true, false, false, FontW, FontH, false);
                                }
                                if (TT == 2)
                                {
                                    FrameCharPut2(X + W + TX, Y + W_ + TY, true, true, false, false, FontW, FontH, false);
                                    FrameCharPut2(X - H_, Y + H, true, true, false, false, FontW, FontH, false);
                                }

                                if (TT != 1)
                                {
                                    FrameCharPut2(X, Y, false, true, false, true, FontW, FontH, false);
                                    FrameCharPut2(X + W - H_, Y + W_ + H + TY, true, false, true, false, FontW, FontH, false);
                                }
                                if (TT != 2)
                                {
                                    FrameCharPut2(X + W + TX, Y + W_, false, true, true, false, FontW, FontH, false);
                                    FrameCharPut2(X - H_, Y + H, true, false, false, true, FontW, FontH, false);
                                }
                            }
                        }
                    }
                }
            }
            if (T == 2)
            {
                int i_X_, i_Y_;
                if (Frame2I == 0)
                {
                    if (DoubleCharMode)
                    {
                        if (TT == 1)
                        {
                            i_Y_ = 0;
                            for (int i_Y = 0; i_Y <= H; i_Y += FontH)
                            {
                                i_X_ = 0;
                                for (int i_X = 0; i_X <= W; i_X += FontW)
                                {
                                    CharPut(X + i_X - i_Y_, Y + i_X_ + i_Y, DrawCharI);
                                    CharPut(X + i_X - i_Y_ + FontW, Y + i_X_ + i_Y, DrawCharIdbl);
                                    i_X_ += FontH;
                                }
                                i_Y_ += FontW;
                            }
                        }
                    }
                    else
                    {
                        if (TT == 3)
                        {
                            i_Y_ = 0 - FontW;
                            for (int i_Y = 0 - FontH; i_Y <= H; i_Y += FontH)
                            {
                                i_X_ = 0;
                                for (int i_X = 0; i_X <= W; i_X += FontW)
                                {
                                    CharPut(X + i_X - i_Y_, Y + i_X_ + i_Y, DrawCharI);
                                    i_X_ += FontH;
                                }
                                i_Y_ += FontW;
                            }
                            i_Y_ = 0 - FontW;
                            for (int i_Y = 0 - FontH; i_Y < H; i_Y += FontH)
                            {
                                i_X_ = 0 - FontH;
                                for (int i_X = 0 - FontW; i_X <= W; i_X += FontW)
                                {
                                    CharPut(X + i_X - i_Y_, Y + i_X_ + i_Y + FontH, DrawCharI);
                                    i_X_ += FontH;
                                }
                                i_Y_ += FontW;
                            }
                        }
                        else
                        {
                            i_Y_ = 0;
                            for (int i_Y = 0; i_Y <= H; i_Y += FontH)
                            {
                                i_X_ = 0;
                                for (int i_X = 0; i_X <= W; i_X += FontW)
                                {
                                    CharPut(X + i_X - i_Y_, Y + i_X_ + i_Y, DrawCharI);
                                    i_X_ += FontH;
                                }
                                i_Y_ += FontW;
                            }
                            if (TT == 0)
                            {
                                i_Y_ = FontW;
                                for (int i_Y = FontH; i_Y <= H; i_Y += FontH)
                                {
                                    i_X_ = 0;
                                    for (int i_X = 0; i_X < W; i_X += FontW)
                                    {
                                        CharPut(X + i_X - i_Y_ + FontW, Y + i_X_ + i_Y, DrawCharI);
                                        i_X_ += FontH;
                                    }
                                    i_Y_ += FontW;
                                }
                            }
                            if (TT == 1)
                            {
                                i_Y_ = 0;
                                for (int i_Y = 0; i_Y <= H; i_Y += FontH)
                                {
                                    i_X_ = 0;
                                    for (int i_X = 0; i_X <= W; i_X += FontW)
                                    {
                                        CharPut(X + i_X - i_Y_ + FontW, Y + i_X_ + i_Y, DrawCharI);
                                        i_X_ += FontH;
                                    }
                                    i_Y_ += FontW;
                                }
                            }
                            if (TT == 2)
                            {
                                i_Y_ = 0;
                                for (int i_Y = 0; i_Y <= H; i_Y += FontH)
                                {
                                    i_X_ = 0;
                                    for (int i_X = 0; i_X <= W; i_X += FontW)
                                    {
                                        CharPut(X + i_X - i_Y_, Y + i_X_ + i_Y + FontH, DrawCharI);
                                        i_X_ += FontH;
                                    }
                                    i_Y_ += FontW;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (DoubleCharMode)
                    {
                        if (TT == 1)
                        {
                            int TX = 0;
                            int TY = 0;
                            bool StdCfg = true;

                            if ((W == 0) && (H == 0))
                            {
                                StdCfg = false;
                                FrameCharPut2(X, Y, false, false, false, false, FontW, FontH, true);
                            }
                            else
                            {
                                if ((W > 0) && (H == 0))
                                {
                                    int ii = 0;
                                    for (int i = 0; i <= W; i += FontW)
                                    {
                                        FrameCharPut2(X + i + TX, Y + ii, false, false, true, true, FontW, FontH, true);
                                        ii += FontH;
                                    }
                                    StdCfg = false;
                                }
                                if ((W == 0) && (H > 0))
                                {
                                    int ii = 0;
                                    for (int i = 0; i <= H; i += FontH)
                                    {
                                        FrameCharPut2(X - ii, Y + i, true, true, false, false, FontW, FontH, true);
                                        ii += FontW;
                                    }
                                    StdCfg = false;
                                }
                            }

                            if (StdCfg)
                            {
                                i_Y_ = (FontW - TX);
                                for (int i_Y = FontH; i_Y < (H + TY); i_Y += FontH)
                                {
                                    i_X_ = FontH;
                                    for (int i_X = FontW; i_X < (W + TX + TY); i_X += FontW)
                                    {
                                        FrameCharPut2(X + i_X - i_Y_, Y + i_X_ + i_Y, true, true, true, true, FontW, FontH, true);
                                        i_X_ += FontH;
                                    }
                                    i_Y_ += FontW;
                                }

                                i_X_ = FontH;
                                for (int i_X = FontW; i_X < W; i_X += FontW)
                                {
                                    FrameCharPut2(X + i_X + TX, Y + i_X_, false, true, true, true, FontW, FontH, true);
                                    FrameCharPut2(X + i_X - H_, Y + i_X_ + H + TY, true, false, true, true, FontW, FontH, true);
                                    i_X_ += FontH;
                                }

                                i_Y_ = FontW;
                                for (int i_Y = FontH; i_Y < H; i_Y += FontH)
                                {
                                    FrameCharPut2(X - i_Y_, Y + i_Y, true, true, false, true, FontW, FontH, true);
                                    FrameCharPut2(X + W - i_Y_ + TX, Y + i_Y + W_ + TY, true, true, true, false, FontW, FontH, true);
                                    i_Y_ += FontW;
                                }

                                FrameCharPut2(X, Y, false, true, false, true, FontW, FontH, true);
                                FrameCharPut2(X + W - H_, Y + W_ + H + TY, true, false, true, false, FontW, FontH, true);
                                FrameCharPut2(X + W + TX, Y + W_, false, true, true, false, FontW, FontH, true);
                                FrameCharPut2(X - H_, Y + H, true, false, false, true, FontW, FontH, true);
                            }
                        }
                    }
                    else
                    {
                        if (TT == 3)
                        {
                            i_Y_ = 0;
                            for (int i_Y = 0; i_Y < H; i_Y += FontH)
                            {
                                i_X_ = 0;
                                for (int i_X = 0; i_X <= W; i_X += FontW)
                                {
                                    FrameCharPut2(X + i_X - i_Y_, Y + i_X_ + i_Y, true, true, true, true, FontW, FontH, false);
                                    i_X_ += FontH;
                                }
                                i_Y_ += FontW;
                            }
                            i_Y_ = 0 - FontW;
                            for (int i_Y = 0 - FontH; i_Y < H; i_Y += FontH)
                            {
                                i_X_ = 0;
                                for (int i_X = 0; i_X < W; i_X += FontW)
                                {
                                    FrameCharPut2(X + i_X - i_Y_, Y + i_X_ + i_Y + FontH, true, true, true, true, FontW, FontH, false);
                                    i_X_ += FontH;
                                }
                                i_Y_ += FontW;
                            }

                            i_X_ = 0;
                            for (int i_X = 0; i_X <= W; i_X += FontW)
                            {
                                FrameCharPut2(X + i_X + FontW, Y + i_X_ - FontH, false, true, true, true, FontW, FontH, false);
                                FrameCharPut2(X + i_X - H_, Y + i_X_ + H, true, false, true, true, FontW, FontH, false);
                                i_X_ += FontH;
                            }

                            i_Y_ = 0;
                            for (int i_Y = 0; i_Y <= H; i_Y += FontH)
                            {
                                FrameCharPut2(X - i_Y_, Y + i_Y - FontH, true, true, false, true, FontW, FontH, false);
                                FrameCharPut2(X + W - i_Y_ + FontW, Y + i_Y + W_, true, true, true, false, FontW, FontH, false);
                                i_Y_ += FontW;
                            }
                        }
                        else
                        {
                            int TX = 0;
                            int TY = 0;
                            bool StdCfg = true;

                            if (TT == 0)
                            {
                                if ((W == 0) && (H == 0))
                                {
                                    StdCfg = false;
                                    FrameCharPut2(X, Y, false, false, false, false, FontW, FontH, false);
                                }
                                else
                                {
                                    if ((W > 0) && (H == 0))
                                    {
                                        int ii = 0;
                                        for (int i = 0; i <= W; i += FontW)
                                        {
                                            FrameCharPut2(X + i + TX, Y + ii, false, false, true, true, FontW, FontH, false);
                                            ii += FontH;
                                        }
                                        StdCfg = false;
                                    }
                                    if ((W == 0) && (H > 0))
                                    {
                                        int ii = 0;
                                        for (int i = 0; i <= H; i += FontH)
                                        {
                                            FrameCharPut2(X - ii, Y + i, true, true, false, false, FontW, FontH, false);
                                            ii += FontW;
                                        }
                                        StdCfg = false;
                                    }
                                }
                            }

                            if (StdCfg)
                            {
                                if (TT == 1)
                                {
                                    TX = FontW;
                                }
                                if (TT == 2)
                                {
                                    TY = FontH;
                                }

                                i_Y_ = (FontW - TX);
                                for (int i_Y = (FontH - (TX > 0 ? FontH : 0)); i_Y < (H + TY); i_Y += FontH)
                                {
                                    i_X_ = FontH;
                                    for (int i_X = FontW; i_X < (W + TX + TY); i_X += FontW)
                                    {
                                        FrameCharPut2(X + i_X - i_Y_, Y + i_X_ + i_Y, true, true, true, true, FontW, FontH, false);
                                        i_X_ += FontH;
                                    }
                                    i_Y_ += FontW;
                                }
                                i_Y_ = 0;
                                for (int i_Y = 0; i_Y < H; i_Y += FontH)
                                {
                                    i_X_ = 0;
                                    for (int i_X = 0; i_X < W; i_X += FontW)
                                    {
                                        FrameCharPut2(X + i_X - i_Y_, Y + i_X_ + i_Y + FontH, true, true, true, true, FontW, FontH, false);
                                        i_X_ += FontH;
                                    }
                                    i_Y_ += FontW;
                                }

                                i_X_ = FontH;
                                for (int i_X = FontW; i_X < W; i_X += FontW)
                                {
                                    FrameCharPut2(X + i_X + TX, Y + i_X_, false, true, true, true, FontW, FontH, false);
                                    FrameCharPut2(X + i_X - H_, Y + i_X_ + H + TY, true, false, true, true, FontW, FontH, false);
                                    i_X_ += FontH;
                                }
                                if (TT == 1)
                                {
                                    FrameCharPut2(X + TX, Y, false, true, true, true, FontW, FontH, false);
                                    FrameCharPut2(X + W - H_, Y + W_ + H + TY, true, false, true, true, FontW, FontH, false);
                                }
                                if (TT == 2)
                                {
                                    FrameCharPut2(X - H_, Y + H + TY, true, false, true, true, FontW, FontH, false);
                                    FrameCharPut2(X + W + TX, Y + W_, false, true, true, true, FontW, FontH, false);
                                }

                                i_Y_ = FontW;
                                for (int i_Y = FontH; i_Y < H; i_Y += FontH)
                                {
                                    FrameCharPut2(X - i_Y_, Y + i_Y, true, true, false, true, FontW, FontH, false);
                                    FrameCharPut2(X + W - i_Y_ + TX, Y + i_Y + W_ + TY, true, true, true, false, FontW, FontH, false);
                                    i_Y_ += FontW;
                                }
                                if (TT == 1)
                                {
                                    FrameCharPut2(X, Y, true, true, false, true, FontW, FontH, false);
                                    FrameCharPut2(X + W - H_ + TX, Y + H + W_ + TY, true, true, true, false, FontW, FontH, false);
                                }
                                if (TT == 2)
                                {
                                    FrameCharPut2(X + W + TX, Y + W_ + TY, true, true, true, false, FontW, FontH, false);
                                    FrameCharPut2(X - H_, Y + H, true, true, false, true, FontW, FontH, false);
                                }

                                if (TT != 1)
                                {
                                    FrameCharPut2(X, Y, false, true, false, true, FontW, FontH, false);
                                    FrameCharPut2(X + W - H_, Y + W_ + H + TY, true, false, true, false, FontW, FontH, false);
                                }
                                if (TT != 2)
                                {
                                    FrameCharPut2(X + W + TX, Y + W_, false, true, true, false, FontW, FontH, false);
                                    FrameCharPut2(X - H_, Y + H, true, false, false, true, FontW, FontH, false);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
