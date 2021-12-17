/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-07
 * Time: 23:08
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextPaint
{
    /// <summary>
    /// Description of Core.
    /// </summary>
    public class Core
    {
        public Screen Screen_;

        public bool UseWindow = false;

        public TextCipher TextCipher_;

        public InfoScreen InfoScreen_ = new InfoScreen();

        public PixelPaint PixelPaint_ = new PixelPaint();

        public static string FullPath(string FileName)
        {
            if (FileName.StartsWith("\\\\"))
            {
                return FileName;
            }
            if (FileName.StartsWith("/"))
            {
                return FileName;
            }
            if (FileName.Length > 3)
            {
                if (FileName[1] == ':')
                {
                    if (FileName[2] == '\\')
                    {
                        return FileName;
                    }
                }
            }
            return AppDir() + FileName;
        }

        public static string AppDir()
        {
            string Dir = AppDomain.CurrentDomain.BaseDirectory;
            if (!Dir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                Dir = Dir + Path.DirectorySeparatorChar.ToString();
            }
            return Dir;
        }

        public Core()
        {
        }

        const int MaxlineSize = 10000;

        List<List<int>> TextBuffer = new List<List<int>>();
        List<List<int>> ScrCharType;
        List<List<int>> ScrCharStr;

        List<List<int>> ScrCharTypeDisp;
        List<List<int>> ScrCharStrDisp;

        public int CursorXSize = 0;
        public int CursorYSize = 0;

        public int CursorX = 0;
        public int CursorY = 0;
        int DisplayX = 0;
        int DisplayY = 0;
        int WinW;
        int WinH;
        int WinTxtW;
        int WinTxtH;

        enum WorkStateDef { WriteText, WriteChar, DrawChar, DrawPixel };
        WorkStateDef WorkState = WorkStateDef.WriteText;

        int CursorType = 0;
        bool CursorDisplay = true;

        bool FramePencil = false;
        int FramePencilLastCross = 0;
        int TextMoveDir = 0;

        int TextInsDelMode = 0;

        string FileREnc = "";
        string FileWEnc = "";

        public Encoding StrToEnc(string Val)
        {
            if (Val == "")
            {
                return Encoding.Default;
            }
            bool DigitOnly = true;
            for (int i = 0; i < Val.Length; i++)
            {
                if ((Val[i] < '0') || (Val[i] > '9'))
                {
                    DigitOnly = false;
                }
            }
            try
            {
                if (DigitOnly)
                {
                    return Encoding.GetEncoding(int.Parse(Val));
                }
                else
                {
                    return Encoding.GetEncoding(Val);
                }
            }
            catch
            {
                return Encoding.Default;
            }
        }


        public List<int> BlankDispLine()
        {
            List<int> T = new List<int>();
            for (int i = 0; i < WinTxtW; i++)
            {
                T.Add('\t');
            }
            return T;
        }

        public void FileLoad(string FileName)
        {
            TextBuffer.Clear();
            if (FileName == "")
            {
                return;
            }
            try
            {
                TextCipher_.Reset();
                FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                StreamReader SR;
                if (FileREnc != "")
                {
                    SR = new StreamReader(FS, StrToEnc(FileREnc));
                }
                else
                {
                    SR = new StreamReader(FS);
                }
                string Buf = SR.ReadLine();
                while (Buf != null)
                {
                    TextBuffer.Add(TextCipher_.Crypt(TextWork.StrToInt(Buf.TrimEnd()), true));
                    Buf = SR.ReadLine();
                }
                SR.Close();
                FS.Close();
                TextBufferTrim();
                UndoBufferClear();
            }
            catch
            {

            }
        }

        public void FileSave(string FileName)
        {
            if (FileName == "")
            {
                return;
            }
            try
            {
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
                TextCipher_.Reset();
                FileStream FS = new FileStream(FileName, FileMode.Create, FileAccess.Write);
                StreamWriter SW;
                if (FileWEnc != "")
                {
                    SW = new StreamWriter(FS, StrToEnc(FileWEnc));
                }
                else
                {
                    SW = new StreamWriter(FS);
                }
                for (int i = 0; i < TextBuffer.Count; i++)
                {
                    SW.WriteLine(TextWork.IntToStr(TextCipher_.Crypt(TextBuffer[i], false)));
                }
                SW.Close();
                FS.Close();
            }
            catch
            {

            }
        }

        public void TextDisplay(int Mode)
        {
            if (InfoScreen_.Shown)
            {
                for (int i = 0; i < WinTxtH - 0; i++)
                {
                    if (i < (InfoScreen_.InfoText.Count - InfoScreen_.InfoY))
                    {
                        string InfoTemp = InfoScreen_.InfoText[i + InfoScreen_.InfoY];
                        if (InfoTemp.Length > InfoScreen_.InfoX)
                        {
                            InfoTemp = InfoTemp.Substring(InfoScreen_.InfoX);
                            if (InfoTemp.Length > WinTxtW)
                            {
                                InfoTemp = InfoTemp.Substring(0, WinTxtW);
                            }
                        }
                        else
                        {
                            InfoTemp = "";
                        }
                        ScrCharStr[i] = TextWork.StrToInt(InfoTemp);
                        ScrCharStr[i].AddRange(TextWork.Spaces(WinTxtW - InfoTemp.Length));
                    }
                    else
                    {
                        ScrCharStr[i] = TextWork.Spaces(WinTxtW);
                    }

                    ScrCharType[i] = TextWork.Pad(WinTxtW, 0);
                }
            }
            else
            {
                int I1 = 0;
                int I2 = (WinTxtH - 1);
                if (Mode == 1)
                {
                    I2 = 0;
                }
                if (Mode == 2)
                {
                    I1 = (WinTxtH - 1);
                }
                if (Mode >= MaxlineSize)
                {
                    I1 = Mode - MaxlineSize;
                    I2 = Mode - MaxlineSize;
                }

                if ((Mode < 3) || (Mode >= MaxlineSize))
                {
                    for (int i = I1; i <= I2; i++)
                    {
                        if ((i + DisplayY) < TextBuffer.Count)
                        {
                            List<int> S = TextBuffer[i + DisplayY];
                            if (DisplayX > 0)
                            {
                                if (S.Count > DisplayX)
                                {
                                    S = S.GetRange(DisplayX, S.Count - DisplayX);
                                }
                                else
                                {
                                    S = new List<int>();
                                }
                            }
                            if (S.Count < WinTxtW)
                            {
                                ScrCharStr[i] = TextWork.Concat(S, TextWork.Spaces(WinTxtW - S.Count));
                                ScrCharType[i] = TextWork.Concat(TextWork.Pad(S.Count, 0), TextWork.Pad(WinTxtW - S.Count, 1));
                            }
                            else
                            {
                                ScrCharStr[i] = S.GetRange(0, WinTxtW);
                                ScrCharType[i] = TextWork.Pad(WinTxtW, 0);
                            }
                        }
                        else
                        {
                            ScrCharStr[i] = TextWork.Spaces(WinTxtW);
                            ScrCharType[i] = TextWork.Pad(WinTxtW, 2);
                        }
                    }
                }
                else
                {
                    int CurOffset = (Mode == 3) ? 0 : WinTxtW - 1;
                    for (int i = I1; i <= I2; i++)
                    {
                        int ChType = 0;
                        int ChStr = TextWork.SpaceChar0;
                        if ((i + DisplayY) < TextBuffer.Count)
                        {
                            if (TextBuffer[i + DisplayY].Count > (DisplayX + CurOffset))
                            {
                                ChStr = TextBuffer[i + DisplayY][DisplayX + CurOffset];
                            }
                            else
                            {
                                ChType = 1;
                            }
                        }
                        else
                        {
                            ChType = 2;
                        }
                        if (Mode == 3)
                        {
                            ScrCharType[i][0] = ChType;
                            ScrCharStr[i][0] = ChStr;
                        }
                        else
                        {
                            ScrCharType[i][WinTxtW - 1] = ChType;
                            ScrCharStr[i][WinTxtW - 1] = ChStr;
                        }
                    }
                }
            }
        }

        public void TextDisplayLine(int Y)
        {
            if (((Y - DisplayY) >= 0) && ((Y - DisplayY) < WinTxtH))
            {
                TextDisplay(MaxlineSize + (Y - DisplayY));
            }
        }

        public int CharGet(int X, int Y, bool Space)
        {
            if ((TextBuffer.Count > Y) && (Y >= 0))
            {
                if ((TextBuffer[Y].Count > X) && (X >= 0))
                {
                    return TextBuffer[Y][X];
                }
            }
            if (Space)
            {
                return TextWork.SpaceChar0;
            }
            else
            {
                return -1;
            }
        }

        public void CharPut(int X, int Y, int C)
        {
            if (X < 0)
            {
                return;
            }
            if (Y < 0)
            {
                return;
            }
            if (UndoBufferEnabled)
            {
                bool UndoBufNew = true;
                for (int i = 0; i < UndoBufferItem_.X.Count; i++)
                {
                    if (UndoBufferItem_.X[i] == X)
                    {
                        if (UndoBufferItem_.Y[i] == Y)
                        {
                            UndoBufferItem_.CharNew[i] = C;
                            UndoBufNew = false;
                            break;
                        }
                    }
                }
                if (UndoBufNew)
                {
                    UndoBufferItem_.X.Add(X);
                    UndoBufferItem_.Y.Add(Y);
                    UndoBufferItem_.CharOld.Add(CharGet(X, Y, true));
                    UndoBufferItem_.CharNew.Add(C);
                    UndoBufferItem_.OpParams.Add(null);
                }
            }
            while (TextBuffer.Count <= Y)
            {
                TextBuffer.Add(new List<int>());
                TextDisplayLine(TextBuffer.Count - 1);
            }
            if (TextBuffer[Y].Count > X)
            {
                TextBuffer[Y][X] = C;
                if (TextWork.SpaceChars.Contains(C))
                {
                    TextBufferTrimLine(Y);
                }
            }
            else
            {
                if (!TextWork.SpaceChars.Contains(C))
                {
                    if (TextBuffer[Y].Count < X)
                    {
                        TextBuffer[Y].AddRange(TextWork.Spaces(X - TextBuffer[Y].Count));
                    }
                    TextBuffer[Y].Add(C);
                }
            }
            while ((TextBuffer.Count > 0) && (TextBuffer[TextBuffer.Count - 1].Count == 0))
            {
                TextBuffer.RemoveAt(TextBuffer.Count - 1);
                TextDisplayLine(TextBuffer.Count);
            }
            TextDisplayLine(Y);
        }






        public void CursorChar_(int XX, int YY, int X, int Y, bool Show)
        {
            if ((X != XX) || (Y != YY))
            {
                if ((X >= 0) && (Y >= 0) && (X < WinTxtW) && (Y < WinTxtH))
                {
                    CursorChar(X, Y, Show);
                }
            }
        }


        public void CursorCharWin(int X0, int Y0, int X, int Y, bool Show)
        {
            if (((X != X0) || (Y != Y0)) && (X >= 0) && (Y >= 0))
            {
                if ((X < WinTxtW) && (Y < WinTxtH))
                {
                    CursorChar(X, Y, Show);
                }
            }
        }

        public void CursorChar(int X, int Y, bool Show)
        {
            if (InfoScreen_.Shown)
            {
                return;
            }
            if (Show)
            {
                if (ScrCharType[Y][X] < 3)
                {
                    ScrCharType[Y][X] += 3;
                }
            }
            else
            {
                if (ScrCharType[Y][X] > 2)
                {
                    ScrCharType[Y][X] -= 3;
                }
            }
        }

        public void CursorLine(bool Show)
        {
            int XX = CursorX - DisplayX;
            int YY = CursorY - DisplayY;
            if (WorkState == WorkStateDef.DrawChar)
            {
                if (Semigraphics_.DiamondType == 0)
                {
                    int X1 = Math.Min(XX, XX + CursorXSize);
                    int X2 = Math.Max(XX, XX + CursorXSize);
                    int Y1 = Math.Min(YY, YY + CursorYSize);
                    int Y2 = Math.Max(YY, YY + CursorYSize);

                    X1 = Math.Max(Math.Min(X1, WinTxtW - 1), 0);
                    X2 = Math.Max(Math.Min(X2, WinTxtW - 1), 0);
                    Y1 = Math.Max(Math.Min(Y1, WinTxtH - 1), 0);
                    Y2 = Math.Max(Math.Min(Y2, WinTxtH - 1), 0);

                    for (int Y = Y1; Y <= Y2; Y++)
                    {
                        for (int X = X1; X <= X2; X++)
                        {
                            if ((X != XX) || (Y != YY))
                            {
                                CursorChar(X, Y, Show);
                            }
                        }
                    }
                }
                else
                {
                    int X1 = Math.Min(XX, XX + CursorXSize);
                    int X2 = Math.Max(XX, XX + CursorXSize);
                    int Y1 = Math.Min(YY, YY + CursorYSize);
                    int Y2 = Math.Max(YY, YY + CursorYSize);


                    for (int X_ = X1; X_ <= X2; X_++)
                    {
                        for (int Y_ = Y1; Y_ <= Y2; Y_++)
                        {
                            int X__ = X_ - Y_ + YY;
                            int Y__ = Y_ + X_ - XX;
                            CursorChar_(XX, YY, X__, Y__, Show);

                            switch (Semigraphics_.DiamondType)
                            {
                                case 1:
                                    if ((X_ < X2) && (Y_ > Y1))
                                    {
                                        CursorChar_(XX, YY, X__ + 1, Y__, Show);
                                    }
                                    break;
                                case 2:
                                    CursorChar_(XX, YY, X__ + 1, Y__, Show);
                                    break;
                                case 3:
                                    CursorChar_(XX, YY, X__, Y__ + 1, Show);
                                    break;
                                case 4:
                                    CursorChar_(XX, YY, X__ - 1, Y__, Show);
                                    break;
                                case 5:
                                    CursorChar_(XX, YY, X__, Y__ - 1, Show);
                                    break;
                                case 6:
                                    CursorChar_(XX, YY, X__ + 1, Y__, Show);
                                    CursorChar_(XX, YY, X__, Y__ - 1, Show);
                                    CursorChar_(XX, YY, X__ + 1, Y__ - 1, Show);
                                    break;
                                case 7:
                                    CursorChar_(XX, YY, X__ + 1, Y__, Show);
                                    CursorChar_(XX, YY, X__, Y__ + 1, Show);
                                    CursorChar_(XX, YY, X__ + 1, Y__ + 1, Show);
                                    break;
                                case 8:
                                    CursorChar_(XX, YY, X__ - 1, Y__, Show);
                                    CursorChar_(XX, YY, X__, Y__ + 1, Show);
                                    CursorChar_(XX, YY, X__ - 1, Y__ + 1, Show);
                                    break;
                                case 9:
                                    CursorChar_(XX, YY, X__ - 1, Y__, Show);
                                    CursorChar_(XX, YY, X__, Y__ - 1, Show);
                                    CursorChar_(XX, YY, X__ - 1, Y__ - 1, Show);
                                    break;
                            }
                        }
                    }
                }
            }
            if (WorkState == WorkStateDef.DrawPixel)
            {
                int XX2 = CursorX * PixelPaint_.CharW + PixelPaint_.CharX + PixelPaint_.SizeX;
                int YY2 = CursorY * PixelPaint_.CharH + PixelPaint_.CharY + PixelPaint_.SizeY;
                XX2 = (XX2 / PixelPaint_.CharW) - DisplayX;
                YY2 = (YY2 / PixelPaint_.CharH) - DisplayY;
                if ((XX != XX2) || (YY != YY2))
                {
                    int XX3 = Math.Max(Math.Min(XX2, WinTxtW - 1), 0);
                    int YY3 = Math.Max(Math.Min(YY2, WinTxtH - 1), 0);
                    CursorCharWin(XX, YY, XX3, YY3, Show);
                    if ((XX2 < 0) || (XX2 >= WinTxtW))
                    {
                        CursorCharWin(XX, YY, XX3, YY3 - 1, Show);
                        CursorCharWin(XX, YY, XX3, YY3 + 1, Show);
                    }
                    if ((YY2 < 0) || (YY2 >= WinTxtH))
                    {
                        CursorCharWin(XX, YY, XX3 - 1, YY3, Show);
                        CursorCharWin(XX, YY, XX3 + 1, YY3, Show);
                    }
                }
            }

            if (CursorDisplay)
            {
                CursorChar(XX, YY, Show);
            }
            if ((CursorType == 1) || (CursorType == 3))
            {
                for (int X = 0; X < XX; X++)
                {
                    CursorChar(X, YY, Show);
                }
                for (int X = XX + 1; X < WinTxtW; X++)
                {
                    CursorChar(X, YY, Show);
                }
                for (int Y = 0; Y < YY; Y++)
                {
                    CursorChar(XX, Y, Show);
                }
                for (int Y = YY + 1; Y < WinTxtH; Y++)
                {
                    CursorChar(XX, Y, Show);
                }
            }
            if ((CursorType == 2) || (CursorType == 3))
            {
                int I1 = 0 - Math.Min(XX, YY);
                int I2 = 0 + Math.Min(WinTxtW - XX - 1, WinTxtH - YY - 1);
                for (int i = I1; i < 0; i++)
                {
                    CursorChar(XX + i, YY + i, Show);
                }
                for (int i = 1; i <= I2; i++)
                {
                    CursorChar(XX + i, YY + i, Show);
                }
                I1 = 0 - Math.Min(WinTxtW - XX - 1, YY);
                I2 = 0 + Math.Min(XX, WinTxtH - YY - 1);
                for (int i = I1; i < 0; i++)
                {
                    CursorChar(XX - i, YY + i, Show);
                }
                for (int i = 1; i <= I2; i++)
                {
                    CursorChar(XX - i, YY + i, Show);
                }
            }
        }

        
        public void CursorEquivPos(int Dir)
        {
            if (WorkState == WorkStateDef.DrawChar)
            {
                if (Semigraphics_.DiamondType == 0)
                {
                    if (((CursorXSize < 0) & (CursorYSize < 0)) || ((CursorXSize > 0) & (CursorYSize > 0)))
                    {
                        if (Dir > 0) { CursorX += CursorXSize; CursorXSize = 0 - CursorXSize; }
                        if (Dir < 0) { CursorY += CursorYSize; CursorYSize = 0 - CursorYSize; }
                        CursorLimit();
                        return;
                    }
                    if (((CursorXSize > 0) & (CursorYSize < 0)) || ((CursorXSize < 0) & (CursorYSize > 0)))
                    {
                        if (Dir > 0) { CursorY += CursorYSize; CursorYSize = 0 - CursorYSize; }
                        if (Dir < 0) { CursorX += CursorXSize; CursorXSize = 0 - CursorXSize; }
                        CursorLimit();
                        return;
                    }
                    if ((CursorXSize == 0) || (CursorYSize == 0))
                    {
                        CursorX += CursorXSize; CursorXSize = 0 - CursorXSize;
                        CursorY += CursorYSize; CursorYSize = 0 - CursorYSize;
                        CursorLimit();
                        return;
                    }
                }
                else
                {
                    if (((CursorXSize < 0) & (CursorYSize < 0)) || ((CursorXSize > 0) & (CursorYSize > 0)))
                    {
                        if (Dir > 0) { CursorX += CursorXSize; CursorY += CursorXSize; CursorXSize = 0 - CursorXSize; }
                        if (Dir < 0) { CursorX -= CursorYSize; CursorY += CursorYSize; CursorYSize = 0 - CursorYSize; }
                        CursorLimit();
                        return;
                    }
                    if (((CursorXSize > 0) & (CursorYSize < 0)) || ((CursorXSize < 0) & (CursorYSize > 0)))
                    {
                        if (Dir > 0) { CursorX -= CursorYSize; CursorY += CursorYSize; CursorYSize = 0 - CursorYSize; }
                        if (Dir < 0) { CursorX += CursorXSize; CursorY += CursorXSize; CursorXSize = 0 - CursorXSize; }
                        CursorLimit();
                        return;
                    }
                    if (((CursorXSize == 0) & (CursorYSize != 0)) || ((CursorXSize != 0) & (CursorYSize == 0)))
                    {
                        CursorX += (CursorXSize - CursorYSize);
                        CursorY += (CursorXSize + CursorYSize);
                        CursorXSize = 0 - CursorXSize;
                        CursorYSize = 0 - CursorYSize;
                        CursorLimit();
                        return;
                    }
                }
            }
        }

        
        public void CursorLimit()
        {
            if (CursorX < 0)
            {
                CursorX = 0;
            }
            if (CursorY < 0)
            {
                CursorY = 0;
            }
            MoveCursor(-1);
        }


        public void MoveCursor(int Direction)
        {
            switch (Direction)
            {
                case 0:
                    if (CursorY > 0)
                    {
                        CursorY--;
                    }
                    break;
                case 1:
                    {
                        CursorY++;
                    }
                    break;
                case 2:
                    if (CursorX > 0)
                    {
                        CursorX--;
                    }
                    break;
                case 3:
                    {
                        CursorX++;
                    }
                    break;
                case 4:
                    MoveCursor(0);
                    MoveCursor(3);
                    break;
                case 5:
                    MoveCursor(1);
                    MoveCursor(2);
                    break;
                case 6:
                    MoveCursor(0);
                    MoveCursor(2);
                    break;
                case 7:
                    MoveCursor(1);
                    MoveCursor(3);
                    break;
            }
            while (DisplayY > CursorY)
            {
                DisplayY--;
                Screen_.Move(0, 0, 0, 1, WinTxtW, WinTxtH - 1);
                ScrCharType.Insert(0, new List<int>());
                ScrCharType.RemoveAt(WinTxtH);
                ScrCharStr.Insert(0, new List<int>());
                ScrCharStr.RemoveAt(WinTxtH);
                ScrCharTypeDisp.Insert(0, BlankDispLine());
                ScrCharTypeDisp.RemoveAt(WinTxtH);
                ScrCharStrDisp.Insert(0, BlankDispLine());
                ScrCharStrDisp.RemoveAt(WinTxtH);
                TextDisplay(1);
            }
            while (DisplayY < (CursorY - WinTxtH + 1))
            {
                DisplayY++;
                Screen_.Move(0, 1, 0, 0, WinTxtW, WinTxtH - 1);
                ScrCharType.RemoveAt(0);
                ScrCharType.Add(new List<int>());
                ScrCharStr.RemoveAt(0);
                ScrCharStr.Add(new List<int>());
                ScrCharTypeDisp.RemoveAt(0);
                ScrCharTypeDisp.Add(BlankDispLine());
                ScrCharStrDisp.RemoveAt(0);
                ScrCharStrDisp.Add(BlankDispLine());
                TextDisplay(2);
            }
            while (DisplayX > CursorX)
            {
                DisplayX--;
                Screen_.Move(0, 0, 1, 0, WinTxtW - 1, WinTxtH);
                for (int i = 0; i < WinTxtH; i++)
                {
                    ScrCharType[i].Insert(0, 0);
                    ScrCharType[i].RemoveAt(WinTxtW);
                    ScrCharStr[i].Insert(0, TextWork.SpaceChar0);
                    ScrCharStr[i].RemoveAt(WinTxtW);
                    ScrCharTypeDisp[i].Insert(0, TextWork.SpaceChar0);
                    ScrCharTypeDisp[i].RemoveAt(WinTxtW);
                    ScrCharStrDisp[i].Insert(0, TextWork.SpaceChar0);
                    ScrCharStrDisp[i].RemoveAt(WinTxtW);
                }
                TextDisplay(3);
            }
            while (DisplayX < (CursorX - WinTxtW + 1))
            {
                DisplayX++;
                Screen_.Move(1, 0, 0, 0, WinTxtW - 1, WinTxtH);
                for (int i = 0; i < WinTxtH; i++)
                {
                    ScrCharType[i].RemoveAt(0);
                    ScrCharType[i].Add(0);
                    ScrCharStr[i].RemoveAt(0);
                    ScrCharStr[i].Add(TextWork.SpaceChar0);
                    ScrCharTypeDisp[i].Add(TextWork.SpaceChar0);
                    ScrCharTypeDisp[i].RemoveAt(0);
                    ScrCharStrDisp[i].Add(TextWork.SpaceChar0);
                    ScrCharStrDisp[i].RemoveAt(0);
                }
                TextDisplay(4);
            }
        }



        Semigraphics Semigraphics_;
        Clipboard Clipboard_;
        
        string CurrentFileName = "";
        
        public void Init(string CurrentFileName_, string[] CmdArgs)
        {
            Semigraphics_ = new Semigraphics(this);
            Clipboard_ = new Clipboard(this);
            
            ConfigFile CF = new ConfigFile();
            Console.WriteLine(AppDir());
            CF.FileLoad(AppDir() + "Config.txt");

            for (int i = 1; i < CmdArgs.Length; i++)
            {
                int CmdN = CmdArgs[i].IndexOf('=');
                if (CmdN > 0)
                {
                    string ConfKey = CmdArgs[i].Substring(0, CmdN);
                    string ConfVal = CmdArgs[i].Substring(CmdN + 1);
                    if (ConfKey.Length > 0)
                    {
                        CF.ParamSet(ConfKey, ConfVal);
                    }
                }
            }

            string[] Space = CF.ParamGetS("Space").Split(',');
            TextWork.SpaceChars = new List<int>();
            for (int i = 0; i < Space.Length; i++)
            {
                TextWork.SpaceChars.Add(TextWork.CodeChar(Space[i]));
            }
            TextWork.SpaceChar0 = TextWork.SpaceChars[0];

            Semigraphics_.Init(CF);
            PixelPaint_.Init(CF, this);

            TextCipher_ = new TextCipher(CF, this);

            CurrentFileName = CurrentFileName_;
            
            WinW = -1;
            WinH = -1;
            ScrCharType = new List<List<int>>();
            ScrCharStr = new List<List<int>>();
            TextBuffer = new List<List<int>>();
            ScrCharTypeDisp = new List<List<int>>();
            ScrCharStrDisp = new List<List<int>>();
            TextBuffer.Clear();
            FileREnc = CF.ParamGetS("FileReadEncoding");
            FileWEnc = CF.ParamGetS("FileWriteEncoding");
            UseWindow = (CF.ParamGetI("WinUse") > 0);
            if (!UseWindow)
            {
                CursorDisplay = CF.ParamGetB("ConCursorDisplay");
            }

            if (UseWindow)
            {
                int WinW__ = 80;
                int WinH__ = 25;
                CF.ParamGet("WinW", ref WinW__);
                CF.ParamGet("WinH", ref WinH__);
                if (WinW__ < 1)
                {
                    WinW__ = Console.WindowWidth;
                    if (WinW__ < 1) { WinW__ = 80; }
                }
                if (WinH__ < 1)
                {
                    WinH__ = Console.WindowHeight;
                    if (WinH__ < 1) { WinH__ = 25; }
                }
                Screen_ = new ScreenWindow(this, CF, WinW__, WinH__);
                Screen_.UseMemo = 0;
            }
            else
            {
                Screen_ = new ScreenConsole(this, CF);
                Screen_.UseMemo = CF.ParamGetI("ConUseMemo");
            }
            Screen_.StartApp();
        }

        public void StartUp()
        {
            FileLoad0();
        }

        public void TextRepaint(bool Force)
        {
            for (int Y = 0; Y < WinTxtH; Y++)
            {
                for (int X = 0; X < WinTxtW; X++)
                {
                    if (Force || (ScrCharStrDisp[Y][X] != ScrCharStr[Y][X]) || (ScrCharTypeDisp[Y][X] != ScrCharType[Y][X]))
                    {
                        int ColorB = 0;
                        int ColorF = 0;
                        switch (ScrCharType[Y][X])
                        {
                            case 0:
                                ColorB = 0;
                                ColorF = 3;
                                break;
                            case 1:
                                ColorB = 1;
                                ColorF = 3;
                                break;
                            case 2:
                                ColorB = 2;
                                ColorF = 3;
                                break;
                            case 3:
                            case 4:
                            case 5:
                                ColorB = 3;
                                ColorF = 0;
                                break;
                        }
                        ScrCharTypeDisp[Y][X] = ScrCharType[Y][X];
                        ScrCharStrDisp[Y][X] = ScrCharStr[Y][X];
                        Screen_.PutChar(X, Y, ScrCharStr[Y][X], ColorB, ColorF);
                    }
                }
            }
        }

        bool WindowResizeInfo = false;
        public void WindowResize()
        {
            if (Screen_.WindowResize() || (WindowResizeInfo != InfoScreen_.Shown) || InfoScreen_.Shown)
            {
                WindowResizeInfo = InfoScreen_.Shown;
                WinW = Screen_.WinW;
                WinH = Screen_.WinH;
                WinTxtW = WinW;
                WinTxtH = WinH - 1;
                
                ScrCharType.Clear();
                ScrCharStr.Clear();
                ScrCharTypeDisp.Clear();
                ScrCharStrDisp.Clear();
                for (int i = 0; i < WinTxtH; i++)
                {
                    ScrCharType.Add(new List<int>());
                    ScrCharStr.Add(new List<int>());
                    ScrCharTypeDisp.Add(BlankDispLine());
                    ScrCharStrDisp.Add(BlankDispLine());
                }
                TextDisplay(0);
                CursorLimit();
                Screen_.PutChar(Screen_.WinW - 1, Screen_.WinH - 1, ' ', 3, 0);
            }
        }

        int StatusCursorChar = 32;
        public void ScreenRefresh(bool Force)
        {
            WindowResize();
            CursorLine(true);
            TextRepaint(Force);
            StringBuilder StatusText = new StringBuilder();
            Semigraphics_.CursorChar = ScrCharStr[CursorY - DisplayY][CursorX - DisplayX];
            if (WorkState == WorkStateDef.DrawPixel)
            {
                StatusText.Append(((CursorX * PixelPaint_.CharW) + PixelPaint_.CharX).ToString());
                if (PixelPaint_.SizeX >= 0)
                {
                    StatusText.Append("+");
                }
                else
                {
                    StatusText.Append("-");
                }
                StatusText.Append(Math.Abs(PixelPaint_.SizeX));
                StatusText.Append(":");
                StatusText.Append(((CursorY * PixelPaint_.CharH) + PixelPaint_.CharY).ToString());
                if (PixelPaint_.SizeY >= 0)
                {
                    StatusText.Append("+");
                }
                else
                {
                    StatusText.Append("-");
                }
                StatusText.Append(Math.Abs(PixelPaint_.SizeY));
            }
            else
            {
                StatusText.Append(CursorX.ToString());
                StatusText.Append(":");
                StatusText.Append(CursorY.ToString());
                if (!InfoScreen_.Shown)
                {
                    StatusCursorChar = Semigraphics_.CursorChar;
                }
                StatusText.Append(" " + TextWork.CharCode(StatusCursorChar, false) + " " + TextWork.CharToStr(StatusCursorChar) + " ");
            }

            switch (WorkState)
            {
                case WorkStateDef.WriteText:
                    if (TextMoveDir == 0) { StatusText.Append("Text-R"); }
                    if (TextMoveDir == 1) { StatusText.Append("Text-RD"); }
                    if (TextMoveDir == 2) { StatusText.Append("Text-D"); }
                    if (TextMoveDir == 3) { StatusText.Append("Text-DL"); }
                    if (TextMoveDir == 4) { StatusText.Append("Text-L"); }
                    if (TextMoveDir == 5) { StatusText.Append("Text-LU"); }
                    if (TextMoveDir == 6) { StatusText.Append("Text-U"); }
                    if (TextMoveDir == 7) { StatusText.Append("Text-UR"); }
                    break;
                case WorkStateDef.WriteChar:
                    if (TextMoveDir == 0) { StatusText.Append("Char-R"); }
                    if (TextMoveDir == 1) { StatusText.Append("Char-RD"); }
                    if (TextMoveDir == 2) { StatusText.Append("Char-D"); }
                    if (TextMoveDir == 3) { StatusText.Append("Char-DL"); }
                    if (TextMoveDir == 4) { StatusText.Append("Char-L"); }
                    if (TextMoveDir == 5) { StatusText.Append("Char-LU"); }
                    if (TextMoveDir == 6) { StatusText.Append("Char-U"); }
                    if (TextMoveDir == 7) { StatusText.Append("Char-UR"); }
                    break;
                case WorkStateDef.DrawChar:
                    if (Semigraphics_.DiamondType == 0)
                    {
                        StatusText.Append("Rect " + (Math.Abs(CursorXSize) + 1) + "x" + (Math.Abs(CursorYSize) + 1) + "  " + Semigraphics_.GetFrameName(1));
                    }
                    else
                    {
                        StatusText.Append("Dia " + (Math.Abs(CursorXSize) + 1) + "x" + (Math.Abs(CursorYSize) + 1) + "  " + Semigraphics_.GetFrameName(2));
                    }
                    break;
                case WorkStateDef.DrawPixel:
                    StatusText.Append(PixelPaint_.GetStatusInfo());
                    break;
            }
            if (WorkState != WorkStateDef.DrawPixel)
            {
                switch (TextInsDelMode)
                {
                    case 0: StatusText.Append(" H-block"); break;
                    case 1: StatusText.Append(" V-block"); break;
                    case 2: StatusText.Append(" H-line"); break;
                    case 3: StatusText.Append(" V-line"); break;
                }
            }

            Screen_.SetStatusText(StatusText.ToString());
            if (InfoScreen_.Shown)
            {
                Screen_.SetCursorPosition(WinW - 1, WinH - 1);
            }
            else
            {
                Screen_.SetCursorPosition(CursorX - DisplayX, CursorY - DisplayY);
            }
        }

        public void PixelCharSet()
        {
            if (WorkState == WorkStateDef.DrawPixel)
            {
                Semigraphics_.DrawCharCustomSet(PixelPaint_.CustomCharGet());
            }
        }

        public void PixelCharGet()
        {
            if (WorkState == WorkStateDef.DrawPixel)
            {
                PixelPaint_.CustomCharSet(Semigraphics_.DrawCharCustomGet());
            }
        }

        public void CoreEvent(string KeyName, char KeyChar)
        {
            if (InfoScreen_.ScreenKey(KeyName, KeyChar))
            {
                if (InfoScreen_.RequestHide)
                {
                    InfoScreen_.ScreenHide();
                    ScreenRefresh(true);
                }
                else
                {
                    if (InfoScreen_.ScreenNeedRepaint > 0)
                    {
                        switch (InfoScreen_.ScreenNeedRepaint)
                        {
                            case 2:
                                Screen_.Move(0, 0, 0, 1, WinTxtW, WinTxtH - 1);
                                ScrCharType.Insert(0, new List<int>());
                                ScrCharType.RemoveAt(WinTxtH);
                                ScrCharStr.Insert(0, new List<int>());
                                ScrCharStr.RemoveAt(WinTxtH);
                                ScrCharTypeDisp.Insert(0, BlankDispLine());
                                ScrCharTypeDisp.RemoveAt(WinTxtH);
                                ScrCharStrDisp.Insert(0, BlankDispLine());
                                ScrCharStrDisp.RemoveAt(WinTxtH);
                                TextDisplay(1);
                                break;
                            case 3:
                                Screen_.Move(0, 1, 0, 0, WinTxtW, WinTxtH - 1);
                                ScrCharType.RemoveAt(0);
                                ScrCharType.Add(new List<int>());
                                ScrCharStr.RemoveAt(0);
                                ScrCharStr.Add(new List<int>());
                                ScrCharTypeDisp.RemoveAt(0);
                                ScrCharTypeDisp.Add(BlankDispLine());
                                ScrCharStrDisp.RemoveAt(0);
                                ScrCharStrDisp.Add(BlankDispLine());
                                TextDisplay(2);
                                break;
                            case 4:
                                Screen_.Move(0, 0, 1, 0, WinTxtW - 1, WinTxtH);
                                for (int i = 0; i < WinTxtH; i++)
                                {
                                    ScrCharType[i].Insert(0, 0);
                                    ScrCharType[i].RemoveAt(WinTxtW);
                                    ScrCharStr[i].Insert(0, TextWork.SpaceChar0);
                                    ScrCharStr[i].RemoveAt(WinTxtW);
                                    ScrCharTypeDisp[i].Insert(0, TextWork.SpaceChar0);
                                    ScrCharTypeDisp[i].RemoveAt(WinTxtW);
                                    ScrCharStrDisp[i].Insert(0, TextWork.SpaceChar0);
                                    ScrCharStrDisp[i].RemoveAt(WinTxtW);
                                }
                                TextDisplay(3);
                                break;
                            case 5:
                                Screen_.Move(1, 0, 0, 0, WinTxtW - 1, WinTxtH);
                                for (int i = 0; i < WinTxtH; i++)
                                {
                                    ScrCharType[i].RemoveAt(0);
                                    ScrCharType[i].Add(0);
                                    ScrCharStr[i].RemoveAt(0);
                                    ScrCharStr[i].Add(TextWork.SpaceChar0);
                                    ScrCharTypeDisp[i].Add(TextWork.SpaceChar0);
                                    ScrCharTypeDisp[i].RemoveAt(0);
                                    ScrCharStrDisp[i].Add(TextWork.SpaceChar0);
                                    ScrCharStrDisp[i].RemoveAt(0);
                                }
                                TextDisplay(4);
                                break;
                        }
                        TextRepaint(false);
                        Screen_.SetCursorPosition(WinW - 1, WinH - 1);
                    }
                }
                return;
            }

            if (TextCipher_.PasswordState != 0)
            {
                TextCipher_.PasswordEvent(KeyName, KeyChar);
                return;
            }

            if (Semigraphics_.SelectCharState)
            {
                Semigraphics_.SelectCharEvent(KeyName, KeyChar);
                return;
            }
            
            CursorLine(false);
            switch (KeyName)
            {
                case "F9":
                    PixelCharSet();
                    Semigraphics_.SelectCharInit();
                    return;
                case "Tab":
                    CursorType++;
                    if (CursorType == 4)
                    {
                        CursorType = 0;
                    }
                    break;

                case "F1":
                    if (WorkState == WorkStateDef.WriteText)
                    {
                        InfoScreen_.ScreenShow(1);
                    }
                    else
                    {
                        WorkState = WorkStateDef.WriteText;
                    }
                    break;
                case "F2":
                    if (WorkState == WorkStateDef.WriteChar)
                    {
                        InfoScreen_.ScreenShow(2);
                    }
                    else
                    {
                        WorkState = WorkStateDef.WriteChar;
                    }
                    break;
                case "F3":
                    if (WorkState == WorkStateDef.DrawChar)
                    {
                        InfoScreen_.ScreenShow(3);
                    }
                    else
                    {
                        WorkState = WorkStateDef.DrawChar;
                    }
                    break;
                case "F4":
                    if (WorkState == WorkStateDef.DrawPixel)
                    {
                        InfoScreen_.ScreenShow(4);
                    }
                    else
                    {
                        WorkState = WorkStateDef.DrawPixel;
                        PixelPaint_.PaintStart();
                    }
                    break;

                case "F7":
                    FileSave0();
                    return;
                case "F8":
                    FileLoad0();
                    return;

                case "F12":
                    Screen_.AppWorking = false;
                    return;
            }

            if ((WorkState != WorkStateDef.WriteText) && (WorkState != WorkStateDef.WriteChar))
            {
                switch (KeyName)
                {
                    case "Z":
                        UndoBufferUndo();
                        break;
                    case "X":
                        UndoBufferRedo();
                        break;
                }
            }

            switch (WorkState)
            {
                case WorkStateDef.WriteText: // Edit text
                case WorkStateDef.WriteChar: // Edit text using characters
                    {
                        switch (KeyName)
                        {
                            case "UpArrow":
                            case "Up":
                                MoveCursor(0);
                                break;
                            case "DownArrow":
                            case "Down":
                                MoveCursor(1);
                                break;
                            case "LeftArrow":
                            case "Left":
                                MoveCursor(2);
                                break;
                            case "RightArrow":
                            case "Right":
                                MoveCursor(3);
                                break;
                            case "PageUp":
                            case "Prior":
                                MoveCursor(4);
                                break;
                            case "End":
                                MoveCursor(5);
                                break;
                            case "Home":
                                MoveCursor(6);
                                break;
                            case "PageDown":
                            case "Next":
                                MoveCursor(7);
                                break;

                            case "Escape":
                                {
                                    TextMoveDir++;
                                    if (TextMoveDir == 8)
                                    {
                                        TextMoveDir = 0;
                                    }
                                }
                                break;
                            case "Enter":
                            case "Return":
                                {
                                    TextInsDelMode++;
                                    if (TextInsDelMode == 4)
                                    {
                                        TextInsDelMode = 0;
                                    }
                                }
                                break;
                            case "Insert":
                                UndoBufferStart();
                                TextInsert(CursorX, CursorY, 0, 0, TextInsDelMode);
                                UndoBufferStop();
                                break;
                            case "Delete":
                                UndoBufferStart();
                                TextDelete(CursorX, CursorY, 0, 0, TextInsDelMode);
                                UndoBufferStop();
                                break;

                            case "Backspace":
                            case "Back":
                                {
                                    if (TextMoveDir == 0) { MoveCursor(2); }
                                    if (TextMoveDir == 2) { MoveCursor(0); }
                                    if (TextMoveDir == 4) { MoveCursor(3); }
                                    if (TextMoveDir == 6) { MoveCursor(1); }
                                    if (TextMoveDir == 1) { MoveCursor(6); }
                                    if (TextMoveDir == 3) { MoveCursor(4); }
                                    if (TextMoveDir == 5) { MoveCursor(7); }
                                    if (TextMoveDir == 7) { MoveCursor(5); }
                                }
                                break;
                                
                            default:
                                if (KeyChar >= 32)
                                {
                                    UndoBufferStart();
                                    if (WorkState == WorkStateDef.WriteChar)
                                    {
                                        if (KeyChar <= 255)
                                        {
                                            CharPut(CursorX, CursorY, Semigraphics_.FavChar[KeyChar]);
                                        }
                                    }
                                    else
                                    {
                                        CharPut(CursorX, CursorY, KeyChar);
                                    }
                                    if (TextMoveDir == 0) { MoveCursor(3); }
                                    if (TextMoveDir == 2) { MoveCursor(1); }
                                    if (TextMoveDir == 4) { MoveCursor(2); }
                                    if (TextMoveDir == 6) { MoveCursor(0); }
                                    if (TextMoveDir == 1) { MoveCursor(7); }
                                    if (TextMoveDir == 3) { MoveCursor(5); }
                                    if (TextMoveDir == 5) { MoveCursor(6); }
                                    if (TextMoveDir == 7) { MoveCursor(4); }
                                    UndoBufferStop();
                                }
                                break;
                        }
                    }
                    break;
                case WorkStateDef.DrawChar: // Paint rentangle or diamond with cursor and size
                    {
                        switch (KeyName)
                        {
                            case "UpArrow":
                            case "Up":
                                if (FramePencil)
                                {
                                    UndoBufferStart();
                                    switch (FramePencilLastCross)
                                    {
                                        case 0:
                                            {
                                                Semigraphics_.FrameCharPut(0);
                                            }
                                            break;
                                        case 3:
                                            {
                                                Semigraphics_.FrameCharPut(6);
                                            }
                                            break;
                                        case 1:
                                            {
                                                Semigraphics_.FrameCharPut(4);
                                            }
                                            break;
                                    }
                                }
                                MoveCursor(0);
                                if (FramePencil)
                                {
                                    FramePencilLastCross = 0;
                                    UndoBufferStop();
                                }
                                break;
                            case "DownArrow":
                            case "Down":
                                if (FramePencil)
                                {
                                    UndoBufferStart();
                                    switch (FramePencilLastCross)
                                    {
                                        case 0:
                                            {
                                                Semigraphics_.FrameCharPut(1);
                                            }
                                            break;
                                        case 2:
                                            {
                                                Semigraphics_.FrameCharPut(5);
                                            }
                                            break;
                                        case 4:
                                            {
                                                Semigraphics_.FrameCharPut(7);
                                            }
                                            break;
                                    }
                                }
                                MoveCursor(1);
                                if (FramePencil)
                                {
                                    FramePencilLastCross = 0;
                                    UndoBufferStop();
                                }
                                break;
                            case "LeftArrow":
                            case "Left":
                                if (FramePencil)
                                {
                                    UndoBufferStart();
                                    switch (FramePencilLastCross)
                                    {
                                        case 0:
                                            {
                                                Semigraphics_.FrameCharPut(2);
                                            }
                                            break;
                                        case 2:
                                            {
                                                Semigraphics_.FrameCharPut(5);
                                            }
                                            break;
                                        case 3:
                                            {
                                                Semigraphics_.FrameCharPut(6);
                                            }
                                            break;
                                    }
                                }
                                MoveCursor(2);
                                if (FramePencil)
                                {
                                    FramePencilLastCross = 0;
                                    UndoBufferStop();
                                }
                                break;
                            case "RightArrow":
                            case "Right":
                                if (FramePencil)
                                {
                                    UndoBufferStart();
                                    switch (FramePencilLastCross)
                                    {
                                        case 0:
                                            {
                                                Semigraphics_.FrameCharPut(3);
                                            }
                                            break;
                                        case 1:
                                            {
                                                Semigraphics_.FrameCharPut(4);
                                            }
                                            break;
                                        case 4:
                                            {
                                                Semigraphics_.FrameCharPut(7);
                                            }
                                            break;
                                    }
                                }
                                MoveCursor(3);
                                if (FramePencil)
                                {
                                    FramePencilLastCross = 0;
                                    UndoBufferStop();
                                }
                                break;
                            case "PageUp":
                            case "Prior":
                                if (FramePencil)
                                {
                                    UndoBufferStart();
                                    Semigraphics_.FrameCharPut(4);
                                }
                                MoveCursor(4);
                                if (FramePencil)
                                {
                                    FramePencilLastCross = 1;
                                    UndoBufferStop();
                                }
                                break;
                            case "End":
                                if (FramePencil)
                                {
                                    UndoBufferStart();
                                    Semigraphics_.FrameCharPut(5);
                                }
                                MoveCursor(5);
                                if (FramePencil)
                                {
                                    FramePencilLastCross = 2;
                                    UndoBufferStop();
                                }
                                break;
                            case "Home":
                                if (FramePencil)
                                {
                                    UndoBufferStart();
                                    Semigraphics_.FrameCharPut(6);
                                }
                                MoveCursor(6);
                                if (FramePencil)
                                {
                                    FramePencilLastCross = 3;
                                    UndoBufferStop();
                                }
                                break;
                            case "PageDown":
                            case "Next":
                                if (FramePencil)
                                {
                                    UndoBufferStart();
                                    Semigraphics_.FrameCharPut(7);
                                }
                                MoveCursor(7);
                                if (FramePencil)
                                {
                                    FramePencilLastCross = 4;
                                    UndoBufferStop();
                                }
                                break;

                            case "W":
                                CursorYSize--;
                                break;
                            case "S":
                                CursorYSize++;
                                break;
                            case "A":
                                CursorXSize--;
                                break;
                            case "D":
                                CursorXSize++;
                                break;
                                
                            case "Q":
                                CursorEquivPos(-1);
                                break;
                            case "E":
                                CursorEquivPos(1);
                                break;


                            case "D1": // Change shape
                                Semigraphics_.DiamondType++;
                                if (Semigraphics_.DiamondType == 10)
                                {
                                    Semigraphics_.DiamondType = 0;
                                }
                                break;
                            case "D2": // Character set
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    Semigraphics_.SetFrameNext(1);
                                }
                                else
                                {
                                    Semigraphics_.SetFrameNext(2);
                                }
                                break;
                            case "D3": // Hollow
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    Semigraphics_.RectangleDraw(0, 0, CursorXSize, CursorYSize, 1);
                                }
                                else
                                {
                                    Semigraphics_.DiamondDraw(0, 0, CursorXSize, CursorYSize, 1, -1);
                                }
                                UndoBufferStop();
                                break;
                            case "D4": // Filled
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    Semigraphics_.RectangleDraw(0, 0, CursorXSize, CursorYSize, 2);
                                }
                                else
                                {
                                    Semigraphics_.DiamondDraw(0, 0, CursorXSize, CursorYSize, 2, -1);
                                }
                                UndoBufferStop();
                                break;
                            case "D5": // Frame
                                FramePencil = !FramePencil;
                                FramePencilLastCross = 0;
                                break;

                            case "T":
                            case "NumPad7":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[2]);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[13]);
                                }
                                UndoBufferStop();
                                break;
                            case "Y":
                            case "NumPad8":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[3]);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[14]);
                                }
                                UndoBufferStop();
                                break;
                            case "U":
                            case "NumPad9":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[4]);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[15]);
                                }
                                UndoBufferStop();
                                break;
                            case "G":
                            case "NumPad4":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[5]);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[16]);
                                }
                                UndoBufferStop();
                                break;
                            case "H":
                            case "NumPad5":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[6]);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[17]);
                                }
                                UndoBufferStop();
                                break;
                            case "J":
                            case "NumPad6":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[7]);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[18]);
                                }
                                UndoBufferStop();
                                break;
                            case "B":
                            case "NumPad1":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[8]);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[19]);
                                }
                                UndoBufferStop();
                                break;
                            case "N":
                            case "NumPad2":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[9]);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[20]);
                                }
                                UndoBufferStop();
                                break;
                            case "M":
                            case "NumPad3":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[10]);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[21]);
                                }
                                UndoBufferStop();
                                break;

                            case "I":
                            case "Add":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[1]);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[12]);
                                }
                                UndoBufferStop();
                                break;
                            case "K":
                            case "Subtract":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[0]);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[11]);
                                }
                                UndoBufferStop();
                                break;
                            case "Spacebar":
                            case "Space":
                            case "NumPad0":
                                UndoBufferStart();
                                CharPut(CursorX, CursorY, Semigraphics_.DrawCharI);
                                UndoBufferStop();
                                break;

                            case "Enter":
                            case "Return":
                                TextInsDelMode++;
                                if (TextInsDelMode == 4)
                                {
                                    TextInsDelMode = 0;
                                }
                                break;
                            case "Insert":
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    UndoBufferStart();
                                    TextInsert(CursorX, CursorY, CursorXSize, CursorYSize, TextInsDelMode);
                                    UndoBufferStop();
                                }
                                break;
                            case "Delete":
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    UndoBufferStart();
                                    TextDelete(CursorX, CursorY, CursorXSize, CursorYSize, TextInsDelMode);
                                    UndoBufferStop();
                                }
                                break;
                                
                            case "C":
                                Clipboard_.DiamondType = Semigraphics_.DiamondType;
                                Clipboard_.TextClipboardWork(0, 0, CursorXSize, CursorYSize, false);
                                break;
                            case "V":
                                UndoBufferStart();
                                Clipboard_.TextClipboardWork(0, 0, CursorXSize, CursorYSize, true);
                                UndoBufferStop();
                                break;
                        }
                    }
                    break;
                case WorkStateDef.DrawPixel: // Pixel paint
                    {
                        switch (KeyName)
                        {
                            case "UpArrow":
                            case "Up":
                            case "DownArrow":
                            case "Down":
                            case "LeftArrow":
                            case "Left":
                            case "RightArrow":
                            case "Right":
                            case "PageUp":
                            case "Prior":
                            case "End":
                            case "Home":
                            case "PageDown":
                            case "Next":
                                if (PixelPaint_.PaintPencil)
                                {
                                    UndoBufferStart();
                                }
                                switch (KeyName)
                                {
                                    case "UpArrow":
                                    case "Up":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 0);
                                        break;
                                    case "DownArrow":
                                    case "Down":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 1);
                                        break;
                                    case "LeftArrow":
                                    case "Left":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 2);
                                        break;
                                    case "RightArrow":
                                    case "Right":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 3);
                                        break;
                                    case "PageUp":
                                    case "Prior":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 4);
                                        break;
                                    case "End":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 5);
                                        break;
                                    case "Home":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 6);
                                        break;
                                    case "PageDown":
                                    case "Next":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 7);
                                        break;
                                }
                                if (PixelPaint_.RequestMoveU) { MoveCursor(0); PixelPaint_.RequestMoveU = false; }
                                if (PixelPaint_.RequestMoveD) { MoveCursor(1); PixelPaint_.RequestMoveD = false; }
                                if (PixelPaint_.RequestMoveL) { MoveCursor(2); PixelPaint_.RequestMoveL = false; }
                                if (PixelPaint_.RequestMoveR) { MoveCursor(3); PixelPaint_.RequestMoveR = false; }
                                if (PixelPaint_.PaintPencil)
                                {
                                    PixelPaint_.Paint();
                                    UndoBufferStop();
                                }
                                break;

                            case "Q":
                                PixelPaint_.SwapCursors(-1);
                                CursorLimit();
                                break;
                            case "E":
                                PixelPaint_.SwapCursors(1);
                                CursorLimit();
                                break;

                            case "R":
                                PixelPaint_.PaintColor++;
                                if (PixelPaint_.PaintColor == 3)
                                {
                                    PixelPaint_.PaintColor = 0;
                                }
                                if (PixelPaint_.PaintColor == 6)
                                {
                                    PixelPaint_.PaintColor = 3;
                                }
                                break;
                            case "F":
                                if (PixelPaint_.PaintColor < 3)
                                {
                                    PixelPaint_.PaintColor += 3;
                                }
                                else
                                {
                                    PixelPaint_.PaintColor -= 3;
                                }
                                break;

                            case "N":
                                UndoBufferStart();
                                PixelPaint_.PaintInvert();
                                UndoBufferStop();
                                break;

                            case "W":
                                PixelPaint_.SizeY--;
                                break;
                            case "S":
                                PixelPaint_.SizeY++;
                                break;
                            case "A":
                                PixelPaint_.SizeX--;
                                break;
                            case "D":
                                PixelPaint_.SizeX++;
                                break;

                            case "I":
                            case "K":
                            case "J":
                            case "L":
                                {
                                    UndoBufferStart();
                                    switch (KeyName)
                                    {
                                        case "I":
                                            PixelPaint_.PaintMove(0);
                                            break;
                                        case "K":
                                            PixelPaint_.PaintMove(1);
                                            break;
                                        case "J":
                                            PixelPaint_.PaintMove(2);
                                            break;
                                        case "L":
                                            PixelPaint_.PaintMove(3);
                                            break;
                                    }
                                    UndoBufferStop();
                                }
                                break;

                            case "C":
                                PixelPaint_.ClipboardCopy();
                                break;
                            case "V":
                                UndoBufferStart();
                                PixelPaint_.ClipboardPaste();
                                UndoBufferStop();
                                break;

                            case "D1":
                                PixelPaint_.PaintModeN++;
                                if (PixelPaint_.PaintModeN >= PixelPaint_.PaintModeCount)
                                {
                                    PixelPaint_.PaintModeN = 0;
                                }
                                PixelPaint_.SelectPaintMode();
                                break;
                            case "D2":
                                PixelPaint_.DefaultColor = !PixelPaint_.DefaultColor;
                                break;
                            case "D3":
                                UndoBufferStart();
                                if (PixelPaint_.PaintColor < 3)
                                {
                                    PixelPaint_.Paint();
                                }
                                else
                                {
                                    PixelPaint_.PaintFill();
                                }
                                UndoBufferStop();
                                break;
                            case "D4":
                                UndoBufferStart();
                                PixelPaint_.PaintLine();
                                UndoBufferStop();
                                break;
                            case "D5":
                                UndoBufferStart();
                                PixelPaint_.PaintRect();
                                UndoBufferStop();
                                break;
                            case "D6":
                                UndoBufferStart();
                                PixelPaint_.PaintEllipse();
                                UndoBufferStop();
                                break;
                            case "P":
                                PixelPaint_.PaintPencil = !PixelPaint_.PaintPencil;
                                if (PixelPaint_.PaintPencil)
                                {
                                    PixelPaint_.Paint();
                                }
                                break;
                            case "M":
                                PixelPaint_.PaintMoveRoll++;
                                if (PixelPaint_.PaintMoveRoll == 5)
                                {
                                    PixelPaint_.PaintMoveRoll = 0;
                                }
                                break;
                        }
                    }
                    break;
            }
            ScreenRefresh(false);
        }


        void FileSave0()
        {
            if (TextCipher_.CipherEnabled)
            {
                if (TextCipher_.CipherConfPassword != "")
                {
                    TextCipher_.SetPassword(TextCipher_.CipherConfPassword);
                    FileSave0_();
                }
                else
                {
                    TextCipher_.PasswordInput(1);
                }
            }
            else
            {
                FileSave0_();
            }
        }

        public void FileSave0_()
        {
            FileSave(CurrentFileName);
        }


        void FileLoad0()
        {
            if (TextCipher_.CipherEnabled)
            {
                if (TextCipher_.CipherConfPassword != "")
                {
                    TextCipher_.SetPassword(TextCipher_.CipherConfPassword);
                    FileLoad0_();
                }
                else
                {
                    TextCipher_.PasswordInput(2);
                }
            }
            else
            {
                FileLoad0_();
            }
        }

        public void FileLoad0_()
        {
            FileLoad(CurrentFileName);
            TextDisplay(0);
            ScreenRefresh(true);
        }

        void TextInsert(int X, int Y, int W, int H, int InsDelMode)
        {
            if (W < 0)
            {
                TextInsert(X + W, Y, 0 - W, H, InsDelMode);
                return;
            }
            if (H < 0)
            {
                TextInsert(X, Y + H, W, 0 - H, InsDelMode);
                return;
            }
            if (X < 0)
            {
                TextInsert(0, Y, W + X, H, InsDelMode);
                return;
            }
            if (Y < 0)
            {
                TextInsert(X, 0, W, H + Y, InsDelMode);
                return;
            }
            bool OpExist = false;

            switch (InsDelMode)
            {
                case 0:
                case 10:
                    for (int i = Y; i <= (Y + H); i++)
                    {
                        if (TextBuffer.Count > i)
                        {
                            if (TextWork.TrimEndLength(TextBuffer[i]) > X)
                            {
                                OpExist = true;
                                TextBuffer[i].InsertRange(X, TextWork.Spaces(W + 1));
                            }
                        }
                    }
                    break;
                case 1:
                case 11:
                    if (Y < TextBuffer.Count)
                    {
                        for (int i = Y; i < TextBuffer.Count; i++)
                        {
                            if (TextBuffer[i].Count < (X + W + 2))
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces((X + W + 2) - TextBuffer[i].Count));
                            }
                        }
                        for (int i = 0; i <= H; i++)
                        {
                            TextBuffer.Add(TextWork.Spaces(X + W + 2));
                        }
                        for (int i = (TextBuffer.Count - H - 1); i > Y; i--)
                        {
                            List<int> TextBufOld = TextWork.TrimEnd(TextBuffer[i + H]);
                            TextBuffer[i + H].RemoveRange(X, W + 1);
                            TextBuffer[i + H].InsertRange(X, TextBuffer[i - 1].GetRange(X, W + 1));
                            TextBufferTrimLine(i + H);
                            if (!TextWork.Equals(TextBufOld, TextWork.TrimEnd(TextBuffer[i + H])))
                            {
                                OpExist = true;
                            }
                        }
                        for (int i = Y; i <= (Y + H); i++)
                        {
                            List<int> TextBufOld = TextWork.TrimEnd(TextBuffer[i]);
                            TextBuffer[i].RemoveRange(X, W + 1);
                            TextBuffer[i].InsertRange(X, TextWork.Spaces(W + 1));
                            TextBufferTrimLine(i);
                            if (!TextWork.Equals(TextBufOld, TextWork.TrimEnd(TextBuffer[i])))
                            {
                                OpExist = true;
                            }
                        }
                    }
                    TextBufferTrim();
                    break;
                case 2:
                case 12:
                    for (int i = 0; i < TextBuffer.Count; i++)
                    {
                        if (TextWork.TrimEndLength(TextBuffer[i]) > X)
                        {
                            OpExist = true;
                            TextBuffer[i].InsertRange(X, TextWork.Spaces(W + 1));
                        }
                    }
                    break;
                case 3:
                case 13:
                    if (Y < TextBuffer.Count)
                    {
                        OpExist = true;
                        for (int i = 0; i <= H; i++)
                        {
                            TextBuffer.Insert(Y, new List<int>());
                        }
                    }
                    break;
            }
            if (InsDelMode < 10)
            {
                if (OpExist)
                {
                    UndoBufferItem_.X.Add(X);
                    UndoBufferItem_.Y.Add(Y);
                    UndoBufferItem_.CharOld.Add(-1);
                    UndoBufferItem_.CharNew.Add(-1);
                    UndoBufferItem_.OpParams.Add(new int[] { InsDelMode, W, H });
                }
                TextDisplay(0);
            }
            switch (InsDelMode)
            {
                case 0:
                case 10:
                case 1:
                case 11:
                    if (Semigraphics_.DrawCharI != TextWork.SpaceChar0)
                    {
                        for (int i_Y = Y; i_Y < (Y + H + 1); i_Y++)
                        {
                            for (int i_X = X; i_X < (X + W + 1); i_X++)
                            {
                                while (TextBuffer.Count <= (Y + H + 1))
                                {
                                    TextBuffer.Add(new List<int>());
                                }
                                if (TextBuffer[i_Y].Count < (X + W + 1))
                                {
                                    TextBuffer[i_Y].AddRange(TextWork.Spaces((X + W + 1) - TextBuffer[i_Y].Count));
                                }
                                TextBuffer[i_Y][i_X] = Semigraphics_.DrawCharI;
                                if (InsDelMode < 10)
                                {
                                    UndoBufferItem_.X.Add(i_X);
                                    UndoBufferItem_.Y.Add(i_Y);
                                    UndoBufferItem_.CharOld.Add(TextWork.SpaceChar0);
                                    UndoBufferItem_.CharNew.Add(Semigraphics_.DrawCharI);
                                    UndoBufferItem_.OpParams.Add(null);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        void TextDelete(int X, int Y, int W, int H, int InsDelMode)
        {
            if (W < 0)
            {
                TextDelete(X + W, Y, 0 - W, H, InsDelMode);
                return;
            }
            if (H < 0)
            {
                TextDelete(X, Y + H, W, 0 - H, InsDelMode);
                return;
            }
            if (X < 0)
            {
                TextDelete(0, Y, W + X, H, InsDelMode);
                return;
            }
            if (Y < 0)
            {
                TextDelete(X, 0, W, H + Y, InsDelMode);
                return;
            }
            bool OpExist = false;

            switch (InsDelMode)
            {
                case 0:
                case 10:
                    for (int i = Y; i <= (Y + H); i++)
                    {
                        if (TextBuffer.Count > i)
                        {
                            if (TextWork.TrimEndLength(TextBuffer[i]) > X)
                            {
                                int RemCount = W + 1;
                                if (TextBuffer[i].Count <= (X + W + 1))
                                {
                                    RemCount = TextBuffer[i].Count - X;
                                }
                                if (InsDelMode < 10)
                                {
                                    for (int ii = 0; ii < RemCount; ii++)
                                    {
                                        if (TextBuffer[i][X + ii] != TextWork.SpaceChar0)
                                        {
                                            UndoBufferItem_.X.Add(X + ii);
                                            UndoBufferItem_.Y.Add(i);
                                            UndoBufferItem_.CharOld.Add(TextBuffer[i][X + ii]);
                                            UndoBufferItem_.CharNew.Add(TextWork.SpaceChar0);
                                            UndoBufferItem_.OpParams.Add(null);
                                        }
                                    }
                                }
                                TextBuffer[i].RemoveRange(X, RemCount);
                                OpExist = true;
                                TextBufferTrimLine(i);
                            }
                        }
                    }
                    break;
                case 1:
                case 11:
                    if (Y < TextBuffer.Count)
                    {
                        for (int i = Y; i < TextBuffer.Count; i++)
                        {
                            if (TextBuffer[i].Count < (X + W + 2))
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces((X + W + 2) - TextBuffer[i].Count));
                            }
                        }
                        for (int i = Y; i <= (Y + H); i++)
                        {
                            for (int ii = X; ii <= (X + W); ii++)
                            {
                                if (TextBuffer.Count > i)
                                {
                                    if (TextBuffer[i].Count > ii)
                                    {
                                        if (TextBuffer[i][ii] != TextWork.SpaceChar0)
                                        {
                                            UndoBufferItem_.X.Add(ii);
                                            UndoBufferItem_.Y.Add(i);
                                            UndoBufferItem_.CharOld.Add(TextBuffer[i][ii]);
                                            UndoBufferItem_.CharNew.Add(TextWork.SpaceChar0);
                                            UndoBufferItem_.OpParams.Add(null);
                                        }
                                    }
                                }
                            }
                        }
                        for (int i = Y; i < TextBuffer.Count; i++)
                        {
                            TextBuffer[i].RemoveRange(X, W + 1);
                            List<int> TempOld = TextWork.TrimEnd(TextBuffer[i]);
                            if ((i + H + 1) < TextBuffer.Count)
                            {
                                TextBuffer[i].InsertRange(X, TextBuffer[i + H + 1].GetRange(X, W + 1));
                            }
                            else
                            {
                                TextBuffer[i].InsertRange(X, TextWork.Spaces(W + 1));
                            }
                            if (!TextWork.Equals(TempOld, TextWork.TrimEnd(TextBuffer[i])))
                            {
                                OpExist = true;
                            }
                            TextBufferTrimLine(i);
                        }
                    }
                    break;
                case 2:
                case 12:
                    for (int i = 0; i < TextBuffer.Count; i++)
                    {
                        if (TextWork.TrimEndLength(TextBuffer[i]) > X)
                        {
                            int RemCount = W + 1;
                            if (TextBuffer[i].Count <= (X + W + 1))
                            {
                                RemCount = TextBuffer[i].Count - X;
                            }
                            if (InsDelMode < 10)
                            {
                                for (int ii = 0; ii < RemCount; ii++)
                                {
                                    if (TextBuffer[i][X + ii] != TextWork.SpaceChar0)
                                    {
                                        UndoBufferItem_.X.Add(X + ii);
                                        UndoBufferItem_.Y.Add(i);
                                        UndoBufferItem_.CharOld.Add(TextBuffer[i][X + ii]);
                                        UndoBufferItem_.CharNew.Add(TextWork.SpaceChar0);
                                        UndoBufferItem_.OpParams.Add(null);
                                    }
                                }
                            }
                            TextBuffer[i].RemoveRange(X, RemCount);
                            OpExist = true;
                            TextBufferTrimLine(i);
                        }
                    }
                    break;
                case 3:
                case 13:
                    if (Y < TextBuffer.Count)
                    {
                        for (int i = Y; i < TextBuffer.Count; i++)
                        {
                            if (TextWork.TrimEndLength(TextBuffer[i]) > 0)
                            {
                                OpExist = true;
                            }
                        }
                        for (int i = 0; i <= H; i++)
                        {
                            if (TextBuffer.Count > Y)
                            {
                                for (int ii = 0; ii < TextBuffer[Y].Count; ii++)
                                {
                                    if (TextBuffer[Y][ii] != TextWork.SpaceChar0)
                                    {
                                        UndoBufferItem_.X.Add(ii);
                                        UndoBufferItem_.Y.Add(Y + i);
                                        UndoBufferItem_.CharOld.Add(TextBuffer[Y][ii]);
                                        UndoBufferItem_.CharNew.Add(TextWork.SpaceChar0);
                                        UndoBufferItem_.OpParams.Add(null);
                                    }
                                }
                                TextBuffer.RemoveAt(Y);
                            }
                        }
                    }
                    break;
            }
            if (InsDelMode < 10)
            {
                if (OpExist)
                {
                    UndoBufferItem_.X.Add(X);
                    UndoBufferItem_.Y.Add(Y);
                    UndoBufferItem_.CharOld.Add(-1);
                    UndoBufferItem_.CharNew.Add(-1);
                    UndoBufferItem_.OpParams.Add(new int[] { InsDelMode + 4, W, H });
                }
                TextBufferTrim();
                TextDisplay(0);
            }
        }
        
        void TextBufferTrim()
        {
            while ((TextBuffer.Count > 0) && (TextBuffer[TextBuffer.Count - 1].Count == 0))
            {
                TextBuffer.RemoveAt(TextBuffer.Count - 1);
            }
        }
        
        void TextBufferTrimLine(int i)
        {
            while ((TextBuffer[i].Count > 0) && (TextWork.SpaceChars.Contains(TextBuffer[i][TextBuffer[i].Count - 1])))
            {
                TextBuffer[i].RemoveRange(TextBuffer[i].Count - 1, 1);
            }
        }


        struct UndoBufferItem
        {
            public int CursorXOld;
            public int CursorYOld;
            public int CursorWOld;
            public int CursorHOld;
            public int CursorXNew;
            public int CursorYNew;
            public int CursorWNew;
            public int CursorHNew;
            public int CursorPxlXOld;
            public int CursorPxlYOld;
            public int CursorPxlWOld;
            public int CursorPxlHOld;
            public int CursorPxlXNew;
            public int CursorPxlYNew;
            public int CursorPxlWNew;
            public int CursorPxlHNew;
            public int PxlModeOld;
            public int PxlModeNew;
            public List<int> X;
            public List<int> Y;
            public List<int> CharOld;
            public List<int> CharNew;
            public List<int[]> OpParams;
        }

        List<UndoBufferItem> UndoBuffer = new List<UndoBufferItem>();
        UndoBufferItem UndoBufferItem_;
        int UndoBufferIndex = 0;

        bool UndoBufferEnabled = false;

        public void UndoBufferClear()
        {
            UndoBuffer.Clear();
            UndoBufferIndex = 0;
        }

        public void UndoBufferStart()
        {
            UndoBufferItem_ = new UndoBufferItem();
            UndoBufferItem_.CursorXOld = CursorX;
            UndoBufferItem_.CursorYOld = CursorY;
            UndoBufferItem_.CursorWOld = CursorXSize;
            UndoBufferItem_.CursorHOld = CursorYSize;
            UndoBufferItem_.PxlModeOld = PixelPaint_.PaintModeN;
            UndoBufferItem_.CursorPxlXOld = PixelPaint_.CharX;
            UndoBufferItem_.CursorPxlYOld = PixelPaint_.CharY;
            UndoBufferItem_.CursorPxlWOld = PixelPaint_.SizeX;
            UndoBufferItem_.CursorPxlHOld = PixelPaint_.SizeY;
            UndoBufferItem_.X = new List<int>();
            UndoBufferItem_.Y = new List<int>();
            UndoBufferItem_.CharOld = new List<int>();
            UndoBufferItem_.CharNew = new List<int>();
            UndoBufferItem_.OpParams = new List<int[]>();
            UndoBufferEnabled = true;
        }

        public void UndoBufferStop()
        {
            UndoBufferEnabled = false;
            UndoBufferItem_.CursorXNew = CursorX;
            UndoBufferItem_.CursorYNew = CursorY;
            UndoBufferItem_.CursorWNew = CursorXSize;
            UndoBufferItem_.CursorHNew = CursorYSize;
            UndoBufferItem_.PxlModeNew = PixelPaint_.PaintModeN;
            UndoBufferItem_.CursorPxlXNew = PixelPaint_.CharX;
            UndoBufferItem_.CursorPxlYNew = PixelPaint_.CharY;
            UndoBufferItem_.CursorPxlWNew = PixelPaint_.SizeX;
            UndoBufferItem_.CursorPxlHNew = PixelPaint_.SizeY;

            if (UndoBufferItem_.X.Count > 0)
            {
                if (UndoBuffer.Count > UndoBufferIndex)
                {
                    UndoBuffer[UndoBufferIndex] = UndoBufferItem_;
                    if (UndoBuffer.Count > (UndoBufferIndex + 1))
                    {
                        UndoBuffer.RemoveRange(UndoBufferIndex + 1, UndoBuffer.Count - (UndoBufferIndex + 1));
                    }
                }
                else
                {
                    UndoBuffer.Add(UndoBufferItem_);
                }
                UndoBufferIndex++;
            }
            TextBufferTrim();
            CursorLimit();
            TextDisplay(0);
        }

        public void UndoBufferUndo()
        {
            if (UndoBufferIndex > 0)
            {
                UndoBufferIndex--;
                UndoBufferItem_ = UndoBuffer[UndoBufferIndex];
                for (int i = (UndoBufferItem_.X.Count - 1); i >= 0; i--)
                {
                    if (UndoBufferItem_.CharOld[i] >= 0)
                    {
                        CharPut(UndoBufferItem_.X[i], UndoBufferItem_.Y[i], UndoBufferItem_.CharOld[i]);
                    }
                    else
                    {
                        if (UndoBufferItem_.OpParams[i][0] < 4)
                        {
                            TextDelete(UndoBufferItem_.X[i], UndoBufferItem_.Y[i], UndoBufferItem_.OpParams[i][1], UndoBufferItem_.OpParams[i][2], UndoBufferItem_.OpParams[i][0] + 10);
                        }
                        else
                        {
                            TextInsert(UndoBufferItem_.X[i], UndoBufferItem_.Y[i], UndoBufferItem_.OpParams[i][1], UndoBufferItem_.OpParams[i][2], UndoBufferItem_.OpParams[i][0] + 6);
                        }
                    }
                }
                CursorX = UndoBufferItem_.CursorXOld;
                CursorY = UndoBufferItem_.CursorYOld;
                CursorXSize = UndoBufferItem_.CursorWOld;
                CursorYSize = UndoBufferItem_.CursorHOld;
                PixelPaint_.PaintModeN = UndoBufferItem_.PxlModeOld;
                PixelPaint_.SelectPaintMode();
                PixelPaint_.CharX = UndoBufferItem_.CursorPxlXOld;
                PixelPaint_.CharY = UndoBufferItem_.CursorPxlYOld;
                PixelPaint_.SizeX = UndoBufferItem_.CursorPxlWOld;
                PixelPaint_.SizeY = UndoBufferItem_.CursorPxlHOld;
                if (PixelPaint_.CharX >= PixelPaint_.CharW)
                {
                    PixelPaint_.CharX = PixelPaint_.CharW - 1;
                }
                if (PixelPaint_.CharY >= PixelPaint_.CharH)
                {
                    PixelPaint_.CharY = PixelPaint_.CharH - 1;
                }
            }
            TextBufferTrim();
            CursorLimit();
            TextDisplay(0);
        }

        public void UndoBufferRedo()
        {
            if (UndoBufferIndex < UndoBuffer.Count)
            {
                UndoBufferItem_ = UndoBuffer[UndoBufferIndex];
                for (int i = 0; i < UndoBufferItem_.X.Count; i++)
                {
                    if (UndoBufferItem_.CharNew[i] > 0)
                    {
                        CharPut(UndoBufferItem_.X[i], UndoBufferItem_.Y[i], UndoBufferItem_.CharNew[i]);
                    }
                    else
                    {
                        if (UndoBufferItem_.OpParams[i][0] < 4)
                        {
                            TextInsert(UndoBufferItem_.X[i], UndoBufferItem_.Y[i], UndoBufferItem_.OpParams[i][1], UndoBufferItem_.OpParams[i][2], UndoBufferItem_.OpParams[i][0] + 10);
                        }
                        else
                        {
                            TextDelete(UndoBufferItem_.X[i], UndoBufferItem_.Y[i], UndoBufferItem_.OpParams[i][1], UndoBufferItem_.OpParams[i][2], UndoBufferItem_.OpParams[i][0] + 6);
                        }
                    }
                }
                CursorX = UndoBufferItem_.CursorXNew;
                CursorY = UndoBufferItem_.CursorYNew;
                CursorXSize = UndoBufferItem_.CursorWNew;
                CursorYSize = UndoBufferItem_.CursorHNew;
                PixelPaint_.PaintModeN = UndoBufferItem_.PxlModeNew;
                PixelPaint_.SelectPaintMode();
                PixelPaint_.CharX = UndoBufferItem_.CursorPxlXNew;
                PixelPaint_.CharY = UndoBufferItem_.CursorPxlYNew;
                PixelPaint_.SizeX = UndoBufferItem_.CursorPxlWNew;
                PixelPaint_.SizeY = UndoBufferItem_.CursorPxlHNew;
                if (PixelPaint_.CharX >= PixelPaint_.CharW)
                {
                    PixelPaint_.CharX = PixelPaint_.CharW - 1;
                }
                if (PixelPaint_.CharY >= PixelPaint_.CharH)
                {
                    PixelPaint_.CharY = PixelPaint_.CharH - 1;
                }
                UndoBufferIndex++;
            }
            TextBufferTrim();
            CursorLimit();
            TextDisplay(0);
        }
    }
}
