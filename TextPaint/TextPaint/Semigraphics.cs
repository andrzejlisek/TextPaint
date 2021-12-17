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
        int MaxCharCode = 0x110000;

        public Core Core_;
        
        public int[] FrameChar = new int[22];
        public int[] FavChar = new int[256];
        
        public List<int[]> Frame1C = new List<int[]>();
        public List<string> Frame1N = new List<string>();
        public List<int[]> Frame2C = new List<int[]>();
        public List<string> Frame2N = new List<string>();

        public Semigraphics(Core Core__)
        {
            Core_ = Core__;
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
                    return TextWork.CharCode(DrawCharI, false) + " " + TextWork.CharToStr(DrawCharI);
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
                    return TextWork.CharCode(DrawCharI, false) + " " + TextWork.CharToStr(DrawCharI);
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

            if ((TabChar >= 0) || (TabChar <= (-256)))
            {
                SelectChar___ = TabChar;
                TabChar = -1;
            }
            else
            {
                SelectChar___ = DrawCharI;
            }
            SelectCharState = true;
            SelectCharRepaintBack();
            SelectCharRepaint();
        }
        
        public void SelectCharCode()
        {
            int C = SelectCharGetCode();
            string C_ = TextWork.CharCode(C, true);
            Core_.Screen_.PutChar(CharPosX + 1, CharPosY + 1, C_[0], 0, 3);
            Core_.Screen_.PutChar(CharPosX + 2, CharPosY + 1, C_[1], 0, 3);
            Core_.Screen_.PutChar(CharPosX + 3, CharPosY + 1, C_[2], 0, 3);
            Core_.Screen_.PutChar(CharPosX + 4, CharPosY + 1, C_[3], 0, 3);
            Core_.Screen_.PutChar(CharPosX + 5, CharPosY + 1, C_[4], 0, 3);
            Core_.Screen_.PutChar(CharPosX + 7, CharPosY + 1, C, 0, 3);
            Core_.Screen_.PutChar(CharPosX + SelectChar___X() * 2 + 2, CharPosY + SelectChar___Y() + 2, C, 1, 3);

            if (SelectChar___ < 0)
            {
                int TempCharCode = SelectChar___Y() * 16 + SelectChar___X();
                if ((TempCharCode >= 32) && (TempCharCode <= 126))
                {
                    Core_.Screen_.PutChar(CharPosX + 9, CharPosY + 1, '[', 0, 3);
                    Core_.Screen_.PutChar(CharPosX + 10, CharPosY + 1, TempCharCode, 0, 3);
                    Core_.Screen_.PutChar(CharPosX + 11, CharPosY + 1, ']', 0, 3);
                }
                else
                {
                    Core_.Screen_.PutChar(CharPosX + 9, CharPosY + 1, 'F', 0, 3);
                    Core_.Screen_.PutChar(CharPosX + 10, CharPosY + 1, 'A', 0, 3);
                    Core_.Screen_.PutChar(CharPosX + 11, CharPosY + 1, 'V', 0, 3);
                }
            }
            else
            {
                Core_.Screen_.PutChar(CharPosX + 9, CharPosY + 1, ' ', 0, 3);
                Core_.Screen_.PutChar(CharPosX + 10, CharPosY + 1, ' ', 0, 3);
                Core_.Screen_.PutChar(CharPosX + 11, CharPosY + 1, ' ', 0, 3);
            }

            Core_.Screen_.SetCursorPosition(CharPosX + SelectChar___X() * 2 + 2, CharPosY + SelectChar___Y() + 2);
        }

        public void SelectCharRepaintBack()
        {
            int CharW = 35;
            int CharH = 19;
            for (int Y = 0; Y < CharH; Y++)
            {
                for (int X = 0; X < CharW; X++)
                {
                    Core_.Screen_.PutChar(CharPosX + X, CharPosY + Y, ' ', 0, 3);
                }
            }
            for (int i = 0; i < CharW; i++)
            {
                Core_.Screen_.PutChar(CharPosX + i, CharPosY + 0, ' ', 3, 0);
                Core_.Screen_.PutChar(CharPosX + i, CharPosY + CharH - 1, ' ', 3, 0);
            }
            for (int i = 0; i < CharH; i++)
            {
                Core_.Screen_.PutChar(CharPosX + 0, CharPosY + i, ' ', 3, 0);
                Core_.Screen_.PutChar(CharPosX + CharW - 1, CharPosY + i, ' ', 3, 0);
            }
            if (SelectToFav >= 0)
            {
                int FavPos = 26;
                string C_ = TextWork.CharCode(SelectToFav, true);
                Core_.Screen_.PutChar(CharPosX + FavPos + 0, CharPosY + 1, C_[0], 0, 3);
                Core_.Screen_.PutChar(CharPosX + FavPos + 1, CharPosY + 1, C_[1], 0, 3);
                Core_.Screen_.PutChar(CharPosX + FavPos + 2, CharPosY + 1, C_[2], 0, 3);
                Core_.Screen_.PutChar(CharPosX + FavPos + 3, CharPosY + 1, C_[3], 0, 3);
                Core_.Screen_.PutChar(CharPosX + FavPos + 4, CharPosY + 1, C_[4], 0, 3);
                Core_.Screen_.PutChar(CharPosX + FavPos + 6, CharPosY + 1, SelectToFav, 0, 3);
            }
        }
        
        public void SelectCharRepaint()
        {
            SelectCharRepaintBack();
            if (SelectChar___ >= 0)
            {
                int C = SelectChar___Page() * 256;
                for (int Y = 0; Y < 16; Y++)
                {
                    for (int X = 0; X < 16; X++)
                    {
                        Core_.Screen_.PutChar(CharPosX + X * 2 + 2, CharPosY + Y + 2, C, 0, 3);
                        C++;
                    }
                }
            }
            else
            {
                int C = 0;
                for (int Y = 0; Y < 16; Y++)
                {
                    for (int X = 0; X < 16; X++)
                    {
                        Core_.Screen_.PutChar(CharPosX + X * 2 + 2, CharPosY + Y + 2, FavChar[C], 0, 3);
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
        
        void SelectCharChange(int T)
        {
            Core_.Screen_.PutChar(CharPosX + SelectChar___X() * 2 + 2, CharPosY + SelectChar___Y() + 2, SelectCharGetCode(), 0, 3);
            bool FavPage = (SelectChar___ < 0);
            int PageDisplayed = FavPage ? 0 : (SelectChar___ >> 8);

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

        public int TabChar = -1;
        public int CharPosMode = 0;
        public int CharPosX = 0;
        public int CharPosY = 0;

        public void SelectCharEvent(string KeyName, char KeyChar)
        {
            switch (KeyName)
            {
                default:
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
                    break;
                case "UpArrow":
                case "Up":
                    SelectCharChange(-2);
                    break;
                case "DownArrow":
                case "Down":
                    SelectCharChange(2);
                    break;
                case "LeftArrow":
                case "Left":
                    SelectCharChange(-1);
                    break;
                case "RightArrow":
                case "Right":
                    SelectCharChange(1);
                    break;
                case "PageUp":
                case "Prior":
                    SelectCharChange(-3);
                    break;
                case "PageDown":
                case "Next":
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
                case "Backspace":
                case "Back":
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
                case "Return":
                    SelectCharClose(1);
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
                        SelectCharRepaint();
                    }
                    return;
                case "Delete":
                    {
                        if ((CursorChar >= 0) && (CursorChar < MaxCharCode))
                        {
                            SelectChar___ = CursorChar;
                        }
                        SelectCharRepaint();
                    }
                    return;
            }
        }
        
        public void SelectCharClose(int Set)
        {
            TabChar = -1;
            if (Set == 1)
            {
                DrawCharI = SelectCharGetCode();
            }
            if (Set == 2)
            {
                TabChar = SelectChar___;
            }
            SelectCharState = false;
            Core_.CoreEvent("", '\0');
            Core_.Screen_.PutChar(Core_.Screen_.WinW - 1, Core_.Screen_.WinH - 1, ' ', 3, 0);
        }

        public int DrawCharI;
        public int DrawCharI_;
        public int CursorChar = 20;

        public void DrawCharCustomSet(int C)
        {
            DrawCharI_ = DrawCharI;
            DrawCharI = C;

        }

        public int DrawCharCustomGet()
        {
            int C = DrawCharI;
            DrawCharI = DrawCharI_;
            return C;
        }



        public void FrameCharPut1(int X, int Y, bool ForceT, bool ForceB, bool ForceL, bool ForceR)
        {
            int CharSide = 0;
            List<int> CharT  = new List<int>(new int[] { FrameChar[1],  FrameChar[2],  FrameChar[3],  FrameChar[4],  FrameChar[5],  FrameChar[6],  FrameChar[7]  });
            List<int> CharB  = new List<int>(new int[] { FrameChar[1],  FrameChar[5],  FrameChar[6],  FrameChar[7],  FrameChar[8],  FrameChar[9],  FrameChar[10] });
            List<int> CharL  = new List<int>(new int[] { FrameChar[0],  FrameChar[2],  FrameChar[3],  FrameChar[5],  FrameChar[6],  FrameChar[8],  FrameChar[9]  });
            List<int> CharR  = new List<int>(new int[] { FrameChar[0],  FrameChar[3],  FrameChar[4],  FrameChar[6],  FrameChar[7],  FrameChar[9],  FrameChar[10] });
            CharSide += ((ForceT || CharT.Contains(CharGet(X + 0, Y - 1))) ? 1 : 0);
            CharSide += ((ForceB || CharB.Contains(CharGet(X + 0, Y + 1))) ? 2 : 0);
            CharSide += ((ForceL || CharL.Contains(CharGet(X - 1, Y + 0))) ? 4 : 0);
            CharSide += ((ForceR || CharR.Contains(CharGet(X + 1, Y + 0))) ? 8 : 0);
            switch (CharSide)
            {
                case 0:  CharPut(X, Y, FrameChar[6]);  break;
                case 1:  CharPut(X, Y, FrameChar[1]);  break;
                case 2:  CharPut(X, Y, FrameChar[1]);  break;
                case 3:  CharPut(X, Y, FrameChar[1]);  break;
                case 4:  CharPut(X, Y, FrameChar[0]);  break;
                case 5:  CharPut(X, Y, FrameChar[10]); break;
                case 6:  CharPut(X, Y, FrameChar[4]);  break;
                case 7:  CharPut(X, Y, FrameChar[7]);  break;
                case 8:  CharPut(X, Y, FrameChar[0]);  break;
                case 9:  CharPut(X, Y, FrameChar[8]);  break;
                case 10: CharPut(X, Y, FrameChar[2]);  break;
                case 11: CharPut(X, Y, FrameChar[5]);  break;
                case 12: CharPut(X, Y, FrameChar[0]);  break;
                case 13: CharPut(X, Y, FrameChar[9]);  break;
                case 14: CharPut(X, Y, FrameChar[3]);  break;
                case 15: CharPut(X, Y, FrameChar[6]);  break;
            }
        }

        public void FrameCharPut2(int X, int Y, bool ForceTR, bool ForceBL, bool ForceTL, bool ForceBR)
        {
            int CharSide = 0;
            List<int> CharTR = new List<int>(new int[] { FrameChar[12], FrameChar[13], FrameChar[14], FrameChar[15], FrameChar[16], FrameChar[17], FrameChar[18] });
            List<int> CharBL = new List<int>(new int[] { FrameChar[12], FrameChar[16], FrameChar[17], FrameChar[18], FrameChar[19], FrameChar[20], FrameChar[21] });
            List<int> CharTL = new List<int>(new int[] { FrameChar[11], FrameChar[13], FrameChar[14], FrameChar[16], FrameChar[17], FrameChar[19], FrameChar[20] });
            List<int> CharBR = new List<int>(new int[] { FrameChar[11], FrameChar[14], FrameChar[15], FrameChar[17], FrameChar[18], FrameChar[20], FrameChar[21] });
            CharSide += ((ForceTR || CharTR.Contains(CharGet(X + 1, Y - 1))) ? 1 : 0);
            CharSide += ((ForceBL || CharBL.Contains(CharGet(X - 1, Y + 1))) ? 2 : 0);
            CharSide += ((ForceTL || CharTL.Contains(CharGet(X - 1, Y - 1))) ? 4 : 0);
            CharSide += ((ForceBR || CharBR.Contains(CharGet(X + 1, Y + 1))) ? 8 : 0);
            switch (CharSide)
            {
                case 0:  CharPut(X, Y, FrameChar[17]); break;
                case 1:  CharPut(X, Y, FrameChar[12]); break;
                case 2:  CharPut(X, Y, FrameChar[12]); break;
                case 3:  CharPut(X, Y, FrameChar[12]); break;
                case 4:  CharPut(X, Y, FrameChar[11]); break;
                case 5:  CharPut(X, Y, FrameChar[21]); break;
                case 6:  CharPut(X, Y, FrameChar[15]); break;
                case 7:  CharPut(X, Y, FrameChar[18]); break;
                case 8:  CharPut(X, Y, FrameChar[11]); break;
                case 9:  CharPut(X, Y, FrameChar[19]); break;
                case 10: CharPut(X, Y, FrameChar[13]); break;
                case 11: CharPut(X, Y, FrameChar[16]); break;
                case 12: CharPut(X, Y, FrameChar[11]); break;
                case 13: CharPut(X, Y, FrameChar[20]); break;
                case 14: CharPut(X, Y, FrameChar[14]); break;
                case 15: CharPut(X, Y, FrameChar[17]); break;
            }
        }


        public void FrameCharPut(int Dir)
        {
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
                        CharSide += (CharB.Contains(CharGet( 0,  1)) ? 1 : 0);
                        CharSide += (CharL.Contains(CharGet(-1,  0)) ? 2 : 0);
                        CharSide += (CharR.Contains(CharGet( 1,  0)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPut(0, 0, FrameChar[1]);  break;
                            case 1: CharPut(0, 0, FrameChar[1]);  break;
                            case 2: CharPut(0, 0, FrameChar[10]); break;
                            case 3: CharPut(0, 0, FrameChar[7]);  break;
                            case 4: CharPut(0, 0, FrameChar[8]);  break;
                            case 5: CharPut(0, 0, FrameChar[5]);  break;
                            case 6: CharPut(0, 0, FrameChar[9]);  break;
                            case 7: CharPut(0, 0, FrameChar[6]);  break;
                        }
                    }
                    break;
                case 1:
                    {
                        CharSide += (CharT.Contains(CharGet( 0, -1)) ? 1 : 0);
                        CharSide += (CharR.Contains(CharGet( 1,  0)) ? 2 : 0);
                        CharSide += (CharL.Contains(CharGet(-1,  0)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPut(0, 0, FrameChar[1]);  break;
                            case 1: CharPut(0, 0, FrameChar[1]);  break;
                            case 2: CharPut(0, 0, FrameChar[2]);  break;
                            case 3: CharPut(0, 0, FrameChar[5]);  break;
                            case 4: CharPut(0, 0, FrameChar[4]);  break;
                            case 5: CharPut(0, 0, FrameChar[7]);  break;
                            case 6: CharPut(0, 0, FrameChar[3]);  break;
                            case 7: CharPut(0, 0, FrameChar[6]);  break;
                        }
                    }
                    break;
                case 2:
                    {
                        CharSide += (CharR.Contains(CharGet( 1,  0)) ? 1 : 0);
                        CharSide += (CharB.Contains(CharGet( 0,  1)) ? 2 : 0);
                        CharSide += (CharT.Contains(CharGet( 0, -1)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPut(0, 0, FrameChar[0]);  break;
                            case 1: CharPut(0, 0, FrameChar[0]);  break;
                            case 2: CharPut(0, 0, FrameChar[4]);  break;
                            case 3: CharPut(0, 0, FrameChar[3]);  break;
                            case 4: CharPut(0, 0, FrameChar[10]); break;
                            case 5: CharPut(0, 0, FrameChar[9]);  break;
                            case 6: CharPut(0, 0, FrameChar[7]);  break;
                            case 7: CharPut(0, 0, FrameChar[6]);  break;
                        }
                    }
                    break;
                case 3:
                    {
                        CharSide += (CharL.Contains(CharGet(-1,  0)) ? 1 : 0);
                        CharSide += (CharT.Contains(CharGet( 0, -1)) ? 2 : 0);
                        CharSide += (CharB.Contains(CharGet( 0,  1)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPut(0, 0, FrameChar[0]);  break;
                            case 1: CharPut(0, 0, FrameChar[0]);  break;
                            case 2: CharPut(0, 0, FrameChar[8]);  break;
                            case 3: CharPut(0, 0, FrameChar[9]);  break;
                            case 4: CharPut(0, 0, FrameChar[2]);  break;
                            case 5: CharPut(0, 0, FrameChar[3]);  break;
                            case 6: CharPut(0, 0, FrameChar[5]);  break;
                            case 7: CharPut(0, 0, FrameChar[6]);  break;
                        }
                    }
                    break;
                case 4:
                    {
                        CharSide += (CharBL.Contains(CharGet(-1,  1)) ? 1 : 0);
                        CharSide += (CharTL.Contains(CharGet(-1, -1)) ? 2 : 0);
                        CharSide += (CharBR.Contains(CharGet( 1,  1)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPut(0, 0, FrameChar[12]); break;
                            case 1: CharPut(0, 0, FrameChar[12]); break;
                            case 2: CharPut(0, 0, FrameChar[21]); break;
                            case 3: CharPut(0, 0, FrameChar[18]); break;
                            case 4: CharPut(0, 0, FrameChar[19]); break;
                            case 5: CharPut(0, 0, FrameChar[16]); break;
                            case 6: CharPut(0, 0, FrameChar[20]); break;
                            case 7: CharPut(0, 0, FrameChar[17]); break;
                        }
                    }
                    break;
                case 5:
                    {
                        CharSide += (CharTR.Contains(CharGet( 1, -1)) ? 1 : 0);
                        CharSide += (CharBR.Contains(CharGet( 1,  1)) ? 2 : 0);
                        CharSide += (CharTL.Contains(CharGet(-1, -1)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPut(0, 0, FrameChar[12]); break;
                            case 1: CharPut(0, 0, FrameChar[12]); break;
                            case 2: CharPut(0, 0, FrameChar[13]); break;
                            case 3: CharPut(0, 0, FrameChar[16]); break;
                            case 4: CharPut(0, 0, FrameChar[15]); break;
                            case 5: CharPut(0, 0, FrameChar[18]); break;
                            case 6: CharPut(0, 0, FrameChar[14]); break;
                            case 7: CharPut(0, 0, FrameChar[17]); break;
                        }
                    }
                    break;
                case 6:
                    {
                        CharSide += (CharTL.Contains(CharGet( 1,  1)) ? 1 : 0);
                        CharSide += (CharTR.Contains(CharGet(-1,  1)) ? 2 : 0);
                        CharSide += (CharBL.Contains(CharGet( 1, -1)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPut(0, 0, FrameChar[11]); break;
                            case 1: CharPut(0, 0, FrameChar[11]); break;
                            case 2: CharPut(0, 0, FrameChar[15]); break;
                            case 3: CharPut(0, 0, FrameChar[14]); break;
                            case 4: CharPut(0, 0, FrameChar[21]); break;
                            case 5: CharPut(0, 0, FrameChar[20]); break;
                            case 6: CharPut(0, 0, FrameChar[18]); break;
                            case 7: CharPut(0, 0, FrameChar[17]); break;
                        }
                    }
                    break;
                case 7:
                    {
                        CharSide += (CharTL.Contains(CharGet(-1, -1)) ? 1 : 0);
                        CharSide += (CharTR.Contains(CharGet( 1, -1)) ? 2 : 0);
                        CharSide += (CharBL.Contains(CharGet(-1,  1)) ? 4 : 0);
                        switch (CharSide)
                        {
                            case 0: CharPut(0, 0, FrameChar[11]); break;
                            case 1: CharPut(0, 0, FrameChar[11]); break;
                            case 2: CharPut(0, 0, FrameChar[19]); break;
                            case 3: CharPut(0, 0, FrameChar[20]); break;
                            case 4: CharPut(0, 0, FrameChar[13]); break;
                            case 5: CharPut(0, 0, FrameChar[14]); break;
                            case 6: CharPut(0, 0, FrameChar[16]); break;
                            case 7: CharPut(0, 0, FrameChar[17]); break;
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
        
        public void CharPut(int X, int Y, int C)
        {
            Core_.CharPut(Core_.CursorX + X, Core_.CursorY + Y, C);
        }

        public int CharGet(int X, int Y)
        {
            return Core_.CharGet(Core_.CursorX + X, Core_.CursorY + Y, true);
        }
        
        public void RectangleDraw(int X, int Y, int W, int H, int T)
        {
            if (W < 0)
            {
                RectangleDraw(X + W, Y, 0 - W, H, T);
                return;
            }
            if (H < 0)
            {
                RectangleDraw(X, Y + H, W, 0 - H, T);
                return;
            }
            
            if (T == 1)
            {
                if (Frame1I == 0)
                {
                    for (int i = 0; i <= W; i++)
                    {
                        CharPut(X + i, Y + 0, DrawCharI);
                        CharPut(X + i, Y + H, DrawCharI);
                    }
                    for (int i = 0; i <= H; i++)
                    {
                        CharPut(X + 0, Y + i, DrawCharI);
                        CharPut(X + W, Y + i, DrawCharI);
                    }
                }
                else
                {
                    for (int i = 1; i < W; i++)
                    {
                        FrameCharPut1(X + i, Y + 0, false, false, true, true);
                        FrameCharPut1(X + i, Y + H, false, false, true, true);
                    }
                    for (int i = 1; i < H; i++)
                    {
                        FrameCharPut1(X + 0, Y + i, true, true, false, false);
                        FrameCharPut1(X + W, Y + i, true, true, false, false);
                    }
                    if ((W != 0) && (H != 0))
                    {
                        FrameCharPut1(X + 0, Y + 0, false, true, false, true);
                        FrameCharPut1(X + W, Y + 0, false, true, true, false);
                        FrameCharPut1(X + 0, Y + H, true, false, false, true);
                        FrameCharPut1(X + W, Y + H, true, false, true, false);
                    }
                    if ((W == 0) && (H != 0))
                    {
                        FrameCharPut1(X + 0, Y + 0, false, true, false, false);
                        FrameCharPut1(X + 0, Y + H, true, false, false, false);
                    }
                    if ((W != 0) && (H == 0))
                    {
                        FrameCharPut1(X + 0, Y + 0, false, false, false, true);
                        FrameCharPut1(X + W, Y + 0, false, false, true, false);
                    }
                    if ((W == 0) && (H == 0))
                    {
                        FrameCharPut1(X + 0, Y + 0, false, false, false, false);
                    }
                }
            }
            if (T == 2)
            {
                if (Frame1I == 0)
                {
                    for (int i = 0; i <= H; i++)
                    {
                        for (int ii = 0; ii <= W; ii++)
                        {
                            CharPut(X + ii, Y + i, DrawCharI);
                        }
                    }
                }
                else
                {
                    if ((W != 0) && (H != 0))
                    {
                        for (int i = 1; i < W; i++)
                        {
                            FrameCharPut1(X + i, Y + 0, false, true, true, true);
                            FrameCharPut1(X + i, Y + H, true, false, true, true);
                        }
                        for (int i = 1; i < H; i++)
                        {
                            FrameCharPut1(X + 0, Y + i, true, true, false, true);
                            FrameCharPut1(X + W, Y + i, true, true, true, false);
                        }
                        for (int i_Y = 1; i_Y < H; i_Y++)
                        {
                            for (int i_X = 1; i_X < W; i_X++)
                            {
                                FrameCharPut1(X + i_X, Y + i_Y, true, true, true, true);
                            }
                        }
                        FrameCharPut1(X + 0, Y + 0, false, true, false, true);
                        FrameCharPut1(X + W, Y + 0, false, true, true, false);
                        FrameCharPut1(X + 0, Y + H, true, false, false, true);
                        FrameCharPut1(X + W, Y + H, true, false, true, false);
                    }
                    if ((W == 0) && (H != 0))
                    {
                        for (int i = 1; i < H; i++)
                        {
                            FrameCharPut1(X + 0, Y + i, true, true, false, false);
                            FrameCharPut1(X + W, Y + i, true, true, false, false);
                        }
                        FrameCharPut1(X + 0, Y + 0, false, true, false, false);
                        FrameCharPut1(X + 0, Y + H, true, false, false, false);
                    }
                    if ((W != 0) && (H == 0))
                    {
                        for (int i = 1; i < W; i++)
                        {
                            FrameCharPut1(X + i, Y + 0, false, false, true, true);
                            FrameCharPut1(X + i, Y + H, false, false, true, true);
                        }
                        FrameCharPut1(X + 0, Y + 0, false, false, false, true);
                        FrameCharPut1(X + W, Y + 0, false, false, true, false);
                    }
                    if ((W == 0) && (H == 0))
                    {
                        FrameCharPut1(X + 0, Y + 0, false, false, false, false);
                    }
                }
            }
        }

        public int DiamondType = 0;

        public void DiamondDraw(int X, int Y, int W, int H, int T, int TT)
        {
            if (W < 0)
            {
                DiamondDraw(X + W, Y + W, 0 - W, H, T, TT);
                return;
            }
            if (H < 0)
            {
                DiamondDraw(X - H, Y + H, W, 0 - H, T, TT);
                return;
            }
            
            if (TT < 0)
            {
                switch (DiamondType)
                {
                    case 1: DiamondDraw(X + 0, Y + 0, W, H, T, 0); return;
                    case 2: DiamondDraw(X + 0, Y + 0, W, H, T, 1); return;
                    case 3: DiamondDraw(X + 0, Y + 0, W, H, T, 2); return;
                    case 4: DiamondDraw(X - 1, Y + 0, W, H, T, 1); return;
                    case 5: DiamondDraw(X + 0, Y - 1, W, H, T, 2); return;
                    case 6: DiamondDraw(X + 0, Y + 0, W, H, T, 3); return;
                    case 7: DiamondDraw(X + 0, Y + 1, W, H, T, 3); return;
                    case 8: DiamondDraw(X - 1, Y + 1, W, H, T, 3); return;
                    case 9: DiamondDraw(X - 1, Y + 0, W, H, T, 3); return;
                }
            }

            if (T == 1)
            {
                if (Frame2I == 0)
                {
                    if (TT == 3)
                    {
                        for (int i = 0; i <= W; i++)
                        {
                            CharPut(X + i + 1, Y + i - 1, DrawCharI);
                            CharPut(X + i - H, Y + i + H, DrawCharI);
                        }
                        for (int i = 0; i <= H; i++)
                        {
                            CharPut(X - i, Y + i - 1, DrawCharI);
                            CharPut(X + W - i + 1, Y + i + W + 0, DrawCharI);
                        }
                    }
                    else
                    {
                        int TX = 0;
                        int TY = 0;
                        if (TT == 1)
                        {
                            TX = 1;
                        }
                        if (TT == 2)
                        {
                            TY = 1;
                        }
                        for (int i = 0; i <= W; i++)
                        {
                            CharPut(X + i + TX, Y + i, DrawCharI);
                            CharPut(X + i - H, Y + i + H + TY, DrawCharI);
                        }
                        for (int i = 0; i <= H; i++)
                        {
                            CharPut(X - i, Y + i, DrawCharI);
                            CharPut(X + W - i + TX, Y + i + W + TY, DrawCharI);
                        }
                    }
                }
                else
                {
                    if (TT == 3)
                    {
                        for (int i = 0; i <= W; i++)
                        {
                            FrameCharPut2(X + i + 1, Y + i - 1, false, false, true, true);
                            FrameCharPut2(X + i - H, Y + i + H, false, false, true, true);
                        }

                        for (int i = 0; i <= H; i++)
                        {
                            FrameCharPut2(X + 0 - i + 0, Y + i - 1, true, true, false, false);
                            FrameCharPut2(X + W - i + 1, Y + i + W, true, true, false, false);
                        }
                    }
                    else
                    {
                        int TX = 0;
                        int TY = 0;
                        if (TT == 1)
                        {
                            TX = 1;
                        }
                        if (TT == 2)
                        {
                            TY = 1;
                        }

                        for (int i = 1; i < W; i++)
                        {
                            FrameCharPut2(X + i + TX, Y + i + 0 + 0, false, false, true, true);
                            FrameCharPut2(X + i - H, Y + i + H + TY, false, false, true, true);
                        }
                        if (TT == 1)
                        {
                            FrameCharPut2(X + 0 + TX, Y + 0 + 0 + 0, false, false, true, true);
                            FrameCharPut2(X + W - H, Y + W + H + TY, false, false, true, true);
                        }
                        if (TT == 2)
                        {
                            FrameCharPut2(X + 0 - H, Y + 0 + H + TY, false, false, true, true);
                            FrameCharPut2(X + W + TX, Y + W + 0 + 0, false, false, true, true);
                        }

                        for (int i = 1; i < H; i++)
                        {
                            FrameCharPut2(X + 0 - i + 0, Y + i + 0 + 0, true, true, false, false);
                            FrameCharPut2(X + W - i + TX, Y + i + W + TY, true, true, false, false);
                        }
                        if (TT == 1)
                        {
                            FrameCharPut2(X + 0 - 0 + 0, Y + 0 + 0 + 0, true, true, false, false);
                            FrameCharPut2(X + W - H + TX, Y + H + W + TY, true, true, false, false);
                        }
                        if (TT == 2)
                        {
                            FrameCharPut2(X + W - 0 + TX, Y + 0 + W + TY, true, true, false, false);
                            FrameCharPut2(X + 0 - H + 0, Y + H + 0 + 0, true, true, false, false);
                        }

                        if (TT != 1)
                        {
                            FrameCharPut2(X, Y, false, true, false, true);
                            FrameCharPut2(X + W - H, Y + W + H + TY, true, false, true, false);
                        }
                        if (TT != 2)
                        {
                            FrameCharPut2(X + W + TX, Y + W, false, true, true, false);
                            FrameCharPut2(X - H, Y + H, true, false, false, true);
                        }

                        if (TT == 0)
                        {
                            if ((W == 0) && (H == 0))
                            {
                                FrameCharPut2(X, Y, false, false, false, false);
                            }
                        }
                    }
                }
            }
            if (T == 2)
            {
                if (Frame2I == 0)
                {
                    if (TT == 3)
                    {
                        for (int i_Y = -1; i_Y <= H; i_Y++)
                        {
                            for (int i_X = 0; i_X <= W; i_X++)
                            {
                                CharPut(X + i_X - i_Y, Y + i_X + i_Y, DrawCharI);
                            }
                        }
                        for (int i_Y = -1; i_Y < H; i_Y++)
                        {
                            for (int i_X = -1; i_X <= W; i_X++)
                            {
                                CharPut(X + i_X - i_Y, Y + i_X + i_Y + 1, DrawCharI);
                            }
                        }
                    }
                    else
                    {
                        for (int i_Y = 0; i_Y <= H; i_Y++)
                        {
                            for (int i_X = 0; i_X <= W; i_X++)
                            {
                                CharPut(X + i_X - i_Y, Y + i_X + i_Y, DrawCharI);
                            }
                        }
                        if (TT == 0)
                        {
                            for (int i_Y = 1; i_Y <= H; i_Y++)
                            {
                                for (int i_X = 0; i_X < W; i_X++)
                                {
                                    CharPut(X + i_X - i_Y + 1, Y + i_X + i_Y, DrawCharI);
                                }
                            }
                        }
                        if (TT == 1)
                        {
                            for (int i_Y = 0; i_Y <= H; i_Y++)
                            {
                                for (int i_X = 0; i_X <= W; i_X++)
                                {
                                    CharPut(X + i_X - i_Y + 1, Y + i_X + i_Y, DrawCharI);
                                }
                            }
                        }
                        if (TT == 2)
                        {
                            for (int i_Y = 0; i_Y <= H; i_Y++)
                            {
                                for (int i_X = 0; i_X <= W; i_X++)
                                {
                                    CharPut(X + i_X - i_Y, Y + i_X + i_Y + 1, DrawCharI);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (TT == 3)
                    {
                        for (int i_Y = 0; i_Y < H; i_Y++)
                        {
                            for (int i_X = 0; i_X <= W; i_X++)
                            {
                                FrameCharPut2(X + i_X - i_Y, Y + i_X + i_Y, true, true, true, true);
                            }
                        }
                        for (int i_Y = -1; i_Y < H; i_Y++)
                        {
                            for (int i_X = 0; i_X < W; i_X++)
                            {
                                FrameCharPut2(X + i_X - i_Y, Y + i_X + i_Y + 1, true, true, true, true);
                            }
                        }
                        for (int i = 0; i <= W; i++)
                        {
                            FrameCharPut2(X + i + 1, Y + i - 1, false, true, true, true);
                            FrameCharPut2(X + i - H, Y + i + H, true, false, true, true);
                        }

                        for (int i = 0; i <= H; i++)
                        {
                            FrameCharPut2(X + 0 - i + 0, Y + i - 1, true, true, false, true);
                            FrameCharPut2(X + W - i + 1, Y + i + W, true, true, true, false);
                        }
                    }
                    else
                    {
                        int TX = 0;
                        int TY = 0;
                        if (TT == 1)
                        {
                            TX = 1;
                        }
                        if (TT == 2)
                        {
                            TY = 1;
                        }

                        for (int i_Y = (1 - TX); i_Y < (H + TY); i_Y++)
                        {
                            for (int i_X = 1; i_X < (W + TX + TY); i_X++)
                            {
                                FrameCharPut2(X + i_X - i_Y, Y + i_X + i_Y, true, true, true, true);
                            }
                        }
                        for (int i_Y = 0; i_Y < H; i_Y++)
                        {
                            for (int i_X = 0; i_X < W; i_X++)
                            {
                                FrameCharPut2(X + i_X - i_Y, Y + i_X + i_Y + 1, true, true, true, true);
                            }
                        }

                        for (int i = 1; i < W; i++)
                        {
                            FrameCharPut2(X + i + TX, Y + i + 0 + 0, false, true, true, true);
                            FrameCharPut2(X + i - H, Y + i + H + TY, true, false, true, true);
                        }
                        if (TT == 1)
                        {
                            FrameCharPut2(X + 0 + TX, Y + 0 + 0 + 0, false, true, true, true);
                            FrameCharPut2(X + W - H, Y + W + H + TY, true, false, true, true);
                        }
                        if (TT == 2)
                        {
                            FrameCharPut2(X + 0 - H, Y + 0 + H + TY, true, false, true, true);
                            FrameCharPut2(X + W + TX, Y + W + 0 + 0, false, true, true, true);
                        }

                        for (int i = 1; i < H; i++)
                        {
                            FrameCharPut2(X + 0 - i + 0, Y + i + 0 + 0, true, true, false, true);
                            FrameCharPut2(X + W - i + TX, Y + i + W + TY, true, true, true, false);
                        }
                        if (TT == 1)
                        {
                            FrameCharPut2(X + 0 - 0 + 0, Y + 0 + 0 + 0, true, true, false, true);
                            FrameCharPut2(X + W - H + TX, Y + H + W + TY, true, true, true, false);
                        }
                        if (TT == 2)
                        {
                            FrameCharPut2(X + W - 0 + TX, Y + 0 + W + TY, true, true, true, false);
                            FrameCharPut2(X + 0 - H + 0, Y + H + 0 + 0, true, true, false, true);
                        }

                        if (TT != 1)
                        {
                            FrameCharPut2(X, Y, false, true, false, true);
                            FrameCharPut2(X + W - H, Y + W + H + TY, true, false, true, false);
                        }
                        if (TT != 2)
                        {
                            FrameCharPut2(X + W + TX, Y + W, false, true, true, false);
                            FrameCharPut2(X - H, Y + H, true, false, false, true);
                        }

                        if (TT == 0)
                        {
                            if ((W == 0) && (H == 0))
                            {
                                FrameCharPut2(X, Y, false, false, false, false);
                            }
                        }
                    }
                }
            }
        }
    }
}
