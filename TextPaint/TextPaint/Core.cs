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
using System.Threading;

namespace TextPaint
{
    /// <summary>
    /// Description of Core.
    /// </summary>
    public partial class Core
    {

        public int AnsiMaxVal = 2000000000;

        // 0 - Text editor
        // 1 - ANSI display and server
        // 2 - Telnet client
        // 3 - Keystroke tester
        public int WorkMode = 0;

        public Telnet Telnet_;

        public Stack<int> TempMemo = new Stack<int>();

        public bool ToggleDrawText = true;
        public bool ToggleDrawColo = true;

        public Screen Screen_;

        private bool UseWindow = false;

        private TextCipher TextCipher_;

        private InfoScreen InfoScreen_ = new InfoScreen();

        private PixelPaint PixelPaint_ = new PixelPaint();

        List<string> EncodingList = new List<string>();
        int EncodingListI;

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
        List<List<int>> TextColBuf = new List<List<int>>();
        List<List<int>> ScrCharType;
        List<List<int>> ScrCharStr;
        List<List<int>> ScrCharCol;

        List<List<int>> ScrCharTypeDisp;
        List<List<int>> ScrCharStrDisp;
        List<List<int>> ScrCharColDisp;

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

        public string FileREnc = "";
        public string FileWEnc = "";

        public List<int> BlankDispLineC()
        {
            List<int> T = new List<int>();
            for (int i = 0; i < WinTxtW; i++)
            {
                T.Add(0);
            }
            return T;
        }

        public List<int> BlankDispLineT()
        {
            List<int> T = new List<int>();
            for (int i = 0; i < WinTxtW; i++)
            {
                T.Add('\t');
            }
            return T;
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
                        ScrCharCol[i] = TextWork.BlkCol(InfoTemp.Length);
                        ScrCharStr[i].AddRange(TextWork.Spaces(WinTxtW - InfoTemp.Length));
                        ScrCharCol[i].AddRange(TextWork.BlkCol(WinTxtW - InfoTemp.Length));
                    }
                    else
                    {
                        ScrCharStr[i] = TextWork.Spaces(WinTxtW);
                        ScrCharCol[i] = TextWork.BlkCol(WinTxtW);
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
                            List<int> C = TextColBuf[i + DisplayY];
                            if (DisplayX > 0)
                            {
                                if (S.Count > DisplayX)
                                {
                                    S = S.GetRange(DisplayX, S.Count - DisplayX);
                                    C = C.GetRange(DisplayX, C.Count - DisplayX);
                                }
                                else
                                {
                                    S = new List<int>();
                                    C = new List<int>();
                                }
                            }
                            if (S.Count < WinTxtW)
                            {
                                ScrCharStr[i] = TextWork.Concat(S, TextWork.Spaces(WinTxtW - S.Count));
                                ScrCharCol[i] = TextWork.Concat(C, TextWork.BlkCol(WinTxtW - C.Count));
                                ScrCharType[i] = TextWork.Concat(TextWork.Pad(S.Count, 0), TextWork.Pad(WinTxtW - S.Count, 1));
                            }
                            else
                            {
                                ScrCharStr[i] = S.GetRange(0, WinTxtW);
                                ScrCharCol[i] = C.GetRange(0, WinTxtW);
                                ScrCharType[i] = TextWork.Pad(WinTxtW, 0);
                            }
                        }
                        else
                        {
                            ScrCharStr[i] = TextWork.Spaces(WinTxtW);
                            ScrCharCol[i] = TextWork.BlkCol(WinTxtW);
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
                        int ChCol = 0;
                        if ((i + DisplayY) < TextBuffer.Count)
                        {
                            if (TextBuffer[i + DisplayY].Count > (DisplayX + CurOffset))
                            {
                                ChStr = TextBuffer[i + DisplayY][DisplayX + CurOffset];
                                ChCol = TextColBuf[i + DisplayY][DisplayX + CurOffset];
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
                            ScrCharCol[i][0] = ChCol;
                        }
                        else
                        {
                            ScrCharType[i][WinTxtW - 1] = ChType;
                            ScrCharStr[i][WinTxtW - 1] = ChStr;
                            ScrCharCol[i][WinTxtW - 1] = ChCol;
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

        public int ColoGet(int X, int Y, bool Space)
        {
            if ((TextColBuf.Count > Y) && (Y >= 0))
            {
                if ((TextColBuf[Y].Count > X) && (X >= 0))
                {
                    return TextColBuf[Y][X];
                }
            }
            if (Space)
            {
                return 0;
            }
            else
            {
                return -1;
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

        public void CharPut(int X, int Y, int Ch, int Col)
        {
            if (!ToggleDrawText)
            {
                Ch = CharGet(X, Y, true);
            }
            if (!ToggleDrawColo)
            {
                Col = ColoGet(X, Y, true);
            }
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
                            UndoBufferItem_.CharNew[i] = Ch;
                            UndoBufferItem_.ColoNew[i] = Col;
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
                    UndoBufferItem_.ColoOld.Add(ColoGet(X, Y, true));
                    UndoBufferItem_.CharNew.Add(Ch);
                    UndoBufferItem_.ColoNew.Add(Col);
                    UndoBufferItem_.OpParams.Add(null);
                }
            }
            while (TextBuffer.Count <= Y)
            {
                TextBuffer.Add(new List<int>());
                TextColBuf.Add(new List<int>());
                TextDisplayLine(TextBuffer.Count - 1);
            }
            if (TextBuffer[Y].Count > X)
            {
                if (ToggleDrawText)
                {
                    TextBuffer[Y][X] = Ch;
                }
                if (ToggleDrawColo)
                {
                    TextColBuf[Y][X] = Col;
                }
                if (TextWork.SpaceChars.Contains(Ch) && (Col == 0))
                {
                    TextBufferTrimLine(Y);
                }
            }
            else
            {
                if ((!TextWork.SpaceChars.Contains(Ch)) || (Col != 0))
                {
                    if (TextBuffer[Y].Count < X)
                    {
                        TextBuffer[Y].AddRange(TextWork.Spaces(X - TextBuffer[Y].Count));
                        TextColBuf[Y].AddRange(TextWork.BlkCol(X - TextColBuf[Y].Count));
                    }
                    if (ToggleDrawText)
                    {
                        TextBuffer[Y].Add(Ch);
                    }
                    else
                    {
                        TextBuffer[Y].Add(TextWork.SpaceChar0);
                    }
                    if (ToggleDrawColo)
                    {
                        TextColBuf[Y].Add(Col);
                    }
                    else
                    {
                        TextColBuf[Y].Add(0);
                    }
                }
            }
            while ((TextBuffer.Count > 0) && (TextBuffer[TextBuffer.Count - 1].Count == 0))
            {
                TextBuffer.RemoveAt(TextBuffer.Count - 1);
                TextColBuf.RemoveAt(TextColBuf.Count - 1);
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
                ScrCharCol.Insert(0, new List<int>());
                ScrCharStr.RemoveAt(WinTxtH);
                ScrCharCol.RemoveAt(WinTxtH);
                ScrCharTypeDisp.Insert(0, BlankDispLineT());
                ScrCharTypeDisp.RemoveAt(WinTxtH);
                ScrCharStrDisp.Insert(0, BlankDispLineT());
                ScrCharColDisp.Insert(0, BlankDispLineC());
                ScrCharStrDisp.RemoveAt(WinTxtH);
                ScrCharColDisp.RemoveAt(WinTxtH);
                TextDisplay(1);
            }
            while (DisplayY < (CursorY - WinTxtH + 1))
            {
                DisplayY++;
                Screen_.Move(0, 1, 0, 0, WinTxtW, WinTxtH - 1);
                ScrCharType.RemoveAt(0);
                ScrCharType.Add(new List<int>());
                ScrCharStr.RemoveAt(0);
                ScrCharCol.RemoveAt(0);
                ScrCharStr.Add(new List<int>());
                ScrCharCol.Add(new List<int>());
                ScrCharTypeDisp.RemoveAt(0);
                ScrCharTypeDisp.Add(BlankDispLineT());
                ScrCharStrDisp.RemoveAt(0);
                ScrCharColDisp.RemoveAt(0);
                ScrCharStrDisp.Add(BlankDispLineT());
                ScrCharColDisp.Add(BlankDispLineC());
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
                    ScrCharCol[i].Insert(0, 0);
                    ScrCharStr[i].RemoveAt(WinTxtW);
                    ScrCharCol[i].RemoveAt(WinTxtW);
                    ScrCharTypeDisp[i].Insert(0, TextWork.SpaceChar0);
                    ScrCharTypeDisp[i].RemoveAt(WinTxtW);
                    ScrCharStrDisp[i].Insert(0, TextWork.SpaceChar0);
                    ScrCharColDisp[i].Insert(0, 0);
                    ScrCharStrDisp[i].RemoveAt(WinTxtW);
                    ScrCharColDisp[i].RemoveAt(WinTxtW);
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
                    ScrCharCol[i].RemoveAt(0);
                    ScrCharStr[i].Add(TextWork.SpaceChar0);
                    ScrCharCol[i].Add(0);
                    ScrCharTypeDisp[i].Add(TextWork.SpaceChar0);
                    ScrCharTypeDisp[i].RemoveAt(0);
                    ScrCharStrDisp[i].Add(TextWork.SpaceChar0);
                    ScrCharColDisp[i].Add(0);
                    ScrCharStrDisp[i].RemoveAt(0);
                    ScrCharColDisp[i].RemoveAt(0);
                }
                TextDisplay(4);
            }
        }



        Semigraphics Semigraphics_;
        Clipboard Clipboard_;

        public string CurrentFileName = "";

        void ReadColor(string SettingValue, ref int ColorB, ref int ColorF)
        {
            if (SettingValue.Length == 2)
            {
                try
                {
                    int V = int.Parse(SettingValue, System.Globalization.NumberStyles.HexNumber);
                    ColorB = V / 16;
                    ColorF = V % 16;
                }
                catch
                {
                }
            }
        }

        public void Init(string CurrentFileName_, string[] CmdArgs)
        {
            Semigraphics_ = new Semigraphics(this);
            Clipboard_ = new Clipboard(this);
            
            ConfigFile CF = new ConfigFile();
            //Console.WriteLine(AppDir());
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
            WorkMode = CF.ParamGetI("WorkMode");
            if ((WorkMode < 0) || (WorkMode > 4))
            {
                WorkMode = 0;
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

            CurrentFileName_ = PrepareFileNameStr(CurrentFileName_);

            if (CurrentFileName_ == "")
            {
                CurrentFileName_ = AppDir() + "Config.txt";
            }

            CurrentFileName = CurrentFileName_;
            
            WinW = -1;
            WinH = -1;
            ScrCharType = new List<List<int>>();
            ScrCharStr = new List<List<int>>();
            ScrCharCol = new List<List<int>>();
            TextBuffer = new List<List<int>>();
            TextColBuf = new List<List<int>>();
            ScrCharTypeDisp = new List<List<int>>();
            ScrCharStrDisp = new List<List<int>>();
            ScrCharColDisp = new List<List<int>>();
            TextBuffer.Clear();
            TextColBuf.Clear();
            FileREnc = CF.ParamGetS("FileReadEncoding");
            FileWEnc = CF.ParamGetS("FileWriteEncoding");
            UseAnsiLoad = CF.ParamGetB("ANSIRead");
            UseAnsiSave = CF.ParamGetB("ANSIWrite");
            AnsiMaxX = CF.ParamGetI("ANSIWidth");
            AnsiMaxY = CF.ParamGetI("ANSIHeight");
            ANSI_CR = CF.ParamGetI("ANSIReadCR");
            ANSI_LF = CF.ParamGetI("ANSIReadLF");
            AnsiColorBackBlink = CF.ParamGetB("ANSIWriteBlink");
            AnsiColorForeBold = CF.ParamGetB("ANSIWriteBold");
            TextBeyondLineMargin = CF.ParamGetI("BeyondLineMargin");

            if (CF.ParamGetB("ANSIDOS"))
            {
                ANSIPrintControlChars = true;
                ANSIMusic = true;
                ANSIDOSNewLine = true;
                ANSIMoveRightWrapLine = true;
                ANSIIgnoreVerticalTab = true;
                ANSIIgnoreHorizontalTab = true;
            }
            else
            {
                ANSIPrintControlChars = false;
                ANSIMusic = false;
                ANSIDOSNewLine = false;
                ANSIMoveRightWrapLine = false;
                ANSIIgnoreVerticalTab = false;
                ANSIIgnoreHorizontalTab = false;
            }

            ReadColor(CF.ParamGetS("ColorNormal"), ref TextNormalBack, ref TextNormalFore);
            ReadColor(CF.ParamGetS("ColorBeyondLine"), ref TextBeyondLineBack, ref TextBeyondLineFore);
            ReadColor(CF.ParamGetS("ColorBeyondEnd"), ref TextBeyondEndBack, ref TextBeyondEndFore);
            ReadColor(CF.ParamGetS("ColorCursor"), ref CursorBack, ref CursorFore);
            ReadColor(CF.ParamGetS("ColorStatus"), ref StatusBack, ref StatusFore);
            ReadColor(CF.ParamGetS("ColorPopup"), ref PopupBack, ref PopupFore);

            ANSIIgnoreBlink = CF.ParamGetB("ANSIIgnoreBlink");
            ANSIIgnoreBold = CF.ParamGetB("ANSIIgnoreBold");
            UseWindow = (CF.ParamGetI("WinUse") > 0);
            CursorDisplay = CF.ParamGetB("CursorDisplay");
            bool ColorBlending = CF.ParamGetB("WinColorBlending");
            if (WorkMode != 4)
            {
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
                    Screen_ = new ScreenWindow(this, CF, WinW__, WinH__, ColorBlending, false);
                }
                else
                {
                    Screen_ = new ScreenConsole(this, CF, TextNormalBack, TextNormalFore);
                    Screen_.UseMemo = CF.ParamGetI("ConUseMemo");
                }
            }
            if (AnsiMaxX <= 0)
            {
                AnsiMaxX = AnsiMaxVal;
            }
            if (AnsiMaxY <= 0)
            {
                AnsiMaxY = AnsiMaxVal;
            }
            if (WorkMode == 1)
            {
                Telnet_ = new Telnet(this, CF);
            }
            if (WorkMode == 2)
            {
                Telnet_ = new Telnet(this, CF);
            }
            if (WorkMode == 3)
            {
                Screen_.RawKeyName = true;
                EncodingList.Clear();
                EncodingListI = 0;
                int MaxSize = 0;

                foreach (EncodingInfo ei in Encoding.GetEncodings())
                {
                    Encoding e = ei.GetEncoding();
                    string EncName = e.CodePage.ToString().PadLeft(5);
                    List<string> EncNameL = new List<string>();
                    EncNameL.Add(e.CodePage.ToString());

                    if ((!EncNameL.Contains(ei.Name)) && (TextWork.EncodingCheckName(e, ei.Name)))
                    {
                        EncName = EncName + ((EncNameL.Count == 1) ? ": " : ", ") + ei.Name;
                        EncNameL.Add(ei.Name);
                    }
                    if ((!EncNameL.Contains(e.WebName)) && (TextWork.EncodingCheckName(e, e.WebName)))
                    {
                        EncName = EncName + ((EncNameL.Count == 1) ? ": " : ", ") + e.WebName;
                        EncNameL.Add(e.WebName);
                    }
                    EncName = EncName + "  ";



                    //EncName = EncName + e.IsSingleByte.ToString() + "  ";

                    EncodingList.Add(EncName);
                    if (MaxSize < EncName.Length)
                    {
                        MaxSize = EncName.Length;
                    }
                }
                for (int i = 0; i < EncodingList.Count; i++)
                {
                    EncodingList[i] = EncodingList[i].PadRight(MaxSize);
                }
                EncodingList.Insert(0, ("Items: " + EncodingList.Count).PadRight(MaxSize));
                EncodingList.Add("".PadRight(MaxSize));
            }
            KeyCounter = 0;
            KeyCounterLast = "";
            if (WorkMode != 4)
            {
                Screen_.StartApp();
            }
            else
            {
                Screen_ = new ScreenWindow(this, CF, 1, 1, ColorBlending, true);
                RenderStart(CF.ParamGetS("RenderFile"), CF.ParamGetI("RenderStep"), CF.ParamGetI("RenderOffset"), CF.ParamGetB("RenderCursor"));
            }
        }

        public void StartUp()
        {
            Screen_.Clear(TextNormalBack, TextNormalFore);
            if (WorkMode == 0)
            {
                FileLoad0();
            }
            if (WorkMode == 1)
            {
                Telnet_.Open(false);
            }
            if (WorkMode == 2)
            {
                Telnet_.Open(true);
            }
            if (WorkMode == 3)
            {
                Screen_.SetCursorPosition(0, WinH - 1);
            }
        }

        public void TextRepaint(bool Force)
        {
            for (int Y = 0; Y < WinTxtH; Y++)
            {
                for (int X = 0; X < WinTxtW; X++)
                {
                    if (Force || (ScrCharStrDisp[Y][X] != ScrCharStr[Y][X]) || (ScrCharColDisp[Y][X] != ScrCharCol[Y][X]) || (ScrCharTypeDisp[Y][X] != ScrCharType[Y][X]))
                    {
                        int ColorB = 0;
                        int ColorF = 0;
                        switch (ScrCharType[Y][X])
                        {
                            case 0:
                                ColorFromInt(ScrCharCol[Y][X], out ColorB, out ColorF);
                                if (ColorB < 0) { ColorB = TextNormalBack; }
                                if (ColorF < 0) { ColorF = TextNormalFore; }
                                break;
                            case 1:
                                if (((X + DisplayX) < TextBeyondLineMargin) || ((TextBeyondLineMargin < 0) && (AnsiMaxX < AnsiMaxVal) && ((X + DisplayX) < AnsiMaxX)))
                                {
                                    ColorB = TextNormalBack;
                                    ColorF = TextNormalFore;
                                }
                                else
                                {
                                    ColorB = TextBeyondLineBack;
                                    ColorF = TextBeyondLineFore;
                                }
                                break;
                            case 2:
                                ColorB = TextBeyondEndBack;
                                ColorF = TextBeyondEndFore;
                                break;
                            case 3:
                            case 4:
                            case 5:
                                ColorB = CursorBack;
                                ColorF = CursorFore;
                                break;
                        }
                        ScrCharTypeDisp[Y][X] = ScrCharType[Y][X];
                        ScrCharStrDisp[Y][X] = ScrCharStr[Y][X];
                        ScrCharColDisp[Y][X] = ScrCharCol[Y][X];
                        Screen_.PutChar(X, Y, ScrCharStr[Y][X], ColorB, ColorF);
                    }
                }
            }
        }

        bool WindowResizeInfo = false;
        public void WindowResize()
        {
            if (WorkMode == 0)
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
                    ScrCharCol.Clear();
                    ScrCharTypeDisp.Clear();
                    ScrCharStrDisp.Clear();
                    ScrCharColDisp.Clear();
                    for (int i = 0; i < WinTxtH; i++)
                    {
                        ScrCharType.Add(new List<int>());
                        ScrCharStr.Add(new List<int>());
                        ScrCharCol.Add(new List<int>());
                        ScrCharTypeDisp.Add(BlankDispLineT());
                        ScrCharStrDisp.Add(BlankDispLineT());
                        ScrCharColDisp.Add(BlankDispLineC());
                    }
                    TextDisplay(0);
                    CursorLimit();
                }
            }
            if ((WorkMode == 1) || (WorkMode == 2))
            {
                if (Screen_.WindowResize())
                {
                    if (Telnet_ != null)
                    {
                        Telnet_.TelnetRepaint();
                    }
                }
                WinW = Screen_.WinW;
                WinH = Screen_.WinH;
            }
            if (WorkMode == 3)
            {
                Screen_.WindowResize();
                WinW = Screen_.WinW;
                WinH = Screen_.WinH;
            }
        }

        string BeyondIndicator()
        {
            if (CursorY < TextBuffer.Count)
            {
                if (CursorX < TextBuffer[CursorY].Count)
                {
                    return ":";
                }
                else
                {
                    return ";";
                }
            }
            else
            {
                return ",";
            }
        }

        int StatusCursorChar = 32;
        int StatusCursorColo = 0;
        public void ScreenRefresh(bool Force)
        {
            WindowResize();
            if (WorkMode == 0)
            {
                CursorLine(true);
                TextRepaint(Force);
                StringBuilder StatusText = new StringBuilder();
                Semigraphics_.CursorChar = ScrCharStr[CursorY - DisplayY][CursorX - DisplayX];
                Semigraphics_.CursorColo = ScrCharCol[CursorY - DisplayY][CursorX - DisplayX];
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
                    StatusText.Append(BeyondIndicator());
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
                    StatusText.Append(BeyondIndicator());
                    StatusText.Append(CursorY.ToString());
                    if (!InfoScreen_.Shown)
                    {
                        StatusCursorChar = Semigraphics_.CursorChar;
                        StatusCursorColo = Semigraphics_.CursorColo;

                    }
                    StatusText.Append(" " + TextWork.CharCode(StatusCursorChar, 1) + " " + TextWork.CharToStr(StatusCursorChar) + " ");
                    int _ColorB, _ColorF;
                    ColorFromInt(StatusCursorColo, out _ColorB, out _ColorF);
                    if ((_ColorB >= 0) && (_ColorB <= 15))
                    {
                        StatusText.Append(_ColorB.ToString("X"));
                    }
                    else
                    {
                        StatusText.Append("-");
                    }
                    if ((_ColorF >= 0) && (_ColorF <= 15))
                    {
                        StatusText.Append(_ColorF.ToString("X"));
                    }
                    else
                    {
                        StatusText.Append("-");
                    }
                    StatusText.Append(" ");
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

                Screen_.SetStatusText(StatusText.ToString(), StatusBack, StatusFore);
                if (InfoScreen_.Shown)
                {
                    Screen_.SetCursorPosition(WinW - 1, WinH - 1);
                }
                else
                {
                    Screen_.SetCursorPosition(CursorX - DisplayX, CursorY - DisplayY);
                }
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

        int KeyCounter = 0;
        string KeyCounterLast = "";

        public void CoreEvent(string KeyName, char KeyChar, bool ModShift, bool ModCtrl, bool ModAlt)
        {
            if (WorkMode != 0)
            {
                if ((WorkMode == 1) || (WorkMode == 2))
                {
                    Telnet_.CoreEvent(KeyName, KeyChar);
                }
                if (WorkMode == 3)
                {
                    WindowResize();

                    if ((KeyCounter >= 3) || (KeyName == "WindowClose"))
                    {
                        Screen_.CloseApp(TextNormalBack, TextNormalFore);
                    }
                    else
                    {
                        string KeyId = KeyName + "|" + ((int)KeyChar).ToString() + "|" + ModShift.ToString() + ModCtrl.ToString() + ModAlt.ToString();
                        if (KeyCounterLast == KeyId)
                        {
                            KeyCounter++;
                        }
                        else
                        {
                            KeyCounterLast = KeyId;
                            KeyCounter = 0;
                        }

                        List<int> KeyInfoText = new List<int>();
                        KeyInfoText.AddRange(TextWork.StrToInt(EncodingList[EncodingListI]));
                        KeyInfoText.Add(34);
                        KeyInfoText.AddRange(TextWork.StrToInt(KeyName));
                        KeyInfoText.Add(34);
                        KeyInfoText.Add(32);
                        KeyInfoText.AddRange(TextWork.StrToInt(((int)KeyChar).ToString()));
                        KeyInfoText.Add(32);
                        KeyInfoText.AddRange(TextWork.StrToInt(TextWork.CharCode((int)KeyChar, 0) + "h"));
                        if (KeyChar >= 32)
                        {
                            KeyInfoText.Add(32);
                            KeyInfoText.Add(39);
                            KeyInfoText.Add(KeyChar);
                            KeyInfoText.Add(39);
                        }
                        KeyInfoText.Add(32);
                        if (ModShift)
                        {
                            KeyInfoText.AddRange(TextWork.StrToInt("[Shift]"));
                        }
                        if (ModCtrl)
                        {
                            KeyInfoText.AddRange(TextWork.StrToInt("[Ctrl]"));
                        }
                        if (ModAlt)
                        {
                            KeyInfoText.AddRange(TextWork.StrToInt("[Alt]"));
                        }
                        while (KeyInfoText.Count < WinW)
                        {
                            KeyInfoText.Add(32);
                        }

                        Screen_.Move(0, 1, 0, 0, WinW, WinH - 1);
                        for (int i = 0; i < WinW; i++)
                        {
                            Screen_.PutChar(i, WinH - 1, KeyInfoText[i], TextNormalBack, TextNormalFore);
                        }

                        Screen_.SetCursorPosition(0, WinH - 1);

                        EncodingListI++;
                        if (EncodingListI == EncodingList.Count)
                        {
                            EncodingListI = 0;
                        }
                    }
                }
                return;
            }

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
                                ScrCharCol.Insert(0, new List<int>());
                                ScrCharStr.RemoveAt(WinTxtH);
                                ScrCharCol.RemoveAt(WinTxtH);
                                ScrCharTypeDisp.Insert(0, BlankDispLineT());
                                ScrCharTypeDisp.RemoveAt(WinTxtH);
                                ScrCharStrDisp.Insert(0, BlankDispLineT());
                                ScrCharColDisp.Insert(0, BlankDispLineC());
                                ScrCharStrDisp.RemoveAt(WinTxtH);
                                ScrCharColDisp.RemoveAt(WinTxtH);
                                TextDisplay(1);
                                break;
                            case 3:
                                Screen_.Move(0, 1, 0, 0, WinTxtW, WinTxtH - 1);
                                ScrCharType.RemoveAt(0);
                                ScrCharType.Add(new List<int>());
                                ScrCharStr.RemoveAt(0);
                                ScrCharCol.RemoveAt(0);
                                ScrCharStr.Add(new List<int>());
                                ScrCharCol.Add(new List<int>());
                                ScrCharTypeDisp.RemoveAt(0);
                                ScrCharTypeDisp.Add(BlankDispLineT());
                                ScrCharStrDisp.RemoveAt(0);
                                ScrCharColDisp.RemoveAt(0);
                                ScrCharStrDisp.Add(BlankDispLineT());
                                ScrCharColDisp.Add(BlankDispLineC());
                                TextDisplay(2);
                                break;
                            case 4:
                                Screen_.Move(0, 0, 1, 0, WinTxtW - 1, WinTxtH);
                                for (int i = 0; i < WinTxtH; i++)
                                {
                                    ScrCharType[i].Insert(0, 0);
                                    ScrCharType[i].RemoveAt(WinTxtW);
                                    ScrCharStr[i].Insert(0, TextWork.SpaceChar0);
                                    ScrCharCol[i].Insert(0, 0);
                                    ScrCharStr[i].RemoveAt(WinTxtW);
                                    ScrCharCol[i].RemoveAt(WinTxtW);
                                    ScrCharTypeDisp[i].Insert(0, TextWork.SpaceChar0);
                                    ScrCharTypeDisp[i].RemoveAt(WinTxtW);
                                    ScrCharStrDisp[i].Insert(0, TextWork.SpaceChar0);
                                    ScrCharColDisp[i].Insert(0, 0);
                                    ScrCharStrDisp[i].RemoveAt(WinTxtW);
                                    ScrCharColDisp[i].RemoveAt(WinTxtW);
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
                                    ScrCharCol[i].RemoveAt(0);
                                    ScrCharStr[i].Add(TextWork.SpaceChar0);
                                    ScrCharCol[i].Add(0);
                                    ScrCharTypeDisp[i].Add(TextWork.SpaceChar0);
                                    ScrCharTypeDisp[i].RemoveAt(0);
                                    ScrCharStrDisp[i].Add(TextWork.SpaceChar0);
                                    ScrCharColDisp[i].Add(0);
                                    ScrCharStrDisp[i].RemoveAt(0);
                                    ScrCharColDisp[i].RemoveAt(0);
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
                case "FileDrop1":
                    CursorX = 0;
                    TempMemo.Push(ToggleDrawText ? 1 : 0);
                    TempMemo.Push(TextMoveDir);
                    TextMoveDir = 0;
                    MoveCursor(2);
                    return;
                case "FileDrop2":
                    FileLoad0();
                    TextMoveDir = TempMemo.Pop();
                    ToggleDrawText = (TempMemo.Pop() != 0);
                    break;
                case "F9":
                    PixelCharSet();
                    Semigraphics_.AnsiMaxX_ = AnsiMaxX;
                    Semigraphics_.AnsiMaxY_ = AnsiMaxY;
                    Semigraphics_.ANSI_CR_ = ANSI_CR;
                    Semigraphics_.ANSI_LF_ = ANSI_LF;
                    if (Semigraphics_.AnsiMaxX_ == AnsiMaxVal)
                    {
                        Semigraphics_.AnsiMaxX_ = 0;
                    }
                    if (Semigraphics_.AnsiMaxY_ == AnsiMaxVal)
                    {
                        Semigraphics_.AnsiMaxY_ = 0;
                    }
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

                case "WindowClose":
                case "F12":
                    Screen_.CloseApp(TextNormalBack, TextNormalFore);
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
                                MoveCursor(0);
                                break;
                            case "DownArrow":
                                MoveCursor(1);
                                break;
                            case "LeftArrow":
                                MoveCursor(2);
                                break;
                            case "RightArrow":
                                MoveCursor(3);
                                break;
                            case "PageUp":
                                MoveCursor(4);
                                break;
                            case "End":
                                MoveCursor(5);
                                break;
                            case "Home":
                                MoveCursor(6);
                                break;
                            case "PageDown":
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
                                            CharPut(CursorX, CursorY, Semigraphics_.FavChar[KeyChar], Semigraphics_.DrawColoI);
                                        }
                                    }
                                    else
                                    {
                                        CharPut(CursorX, CursorY, KeyChar, Semigraphics_.DrawColoI);
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
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[2], Semigraphics_.DrawColoI);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[13], Semigraphics_.DrawColoI);
                                }
                                UndoBufferStop();
                                break;
                            case "Y":
                            case "NumPad8":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[3], Semigraphics_.DrawColoI);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[14], Semigraphics_.DrawColoI);
                                }
                                UndoBufferStop();
                                break;
                            case "U":
                            case "NumPad9":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[4], Semigraphics_.DrawColoI);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[15], Semigraphics_.DrawColoI);
                                }
                                UndoBufferStop();
                                break;
                            case "G":
                            case "NumPad4":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[5], Semigraphics_.DrawColoI);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[16], Semigraphics_.DrawColoI);
                                }
                                UndoBufferStop();
                                break;
                            case "H":
                            case "NumPad5":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[6], Semigraphics_.DrawColoI);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[17], Semigraphics_.DrawColoI);
                                }
                                UndoBufferStop();
                                break;
                            case "J":
                            case "NumPad6":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[7], Semigraphics_.DrawColoI);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[18], Semigraphics_.DrawColoI);
                                }
                                UndoBufferStop();
                                break;
                            case "B":
                            case "NumPad1":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[8], Semigraphics_.DrawColoI);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[19], Semigraphics_.DrawColoI);
                                }
                                UndoBufferStop();
                                break;
                            case "N":
                            case "NumPad2":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[9], Semigraphics_.DrawColoI);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[20], Semigraphics_.DrawColoI);
                                }
                                UndoBufferStop();
                                break;
                            case "M":
                            case "NumPad3":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[10], Semigraphics_.DrawColoI);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[21], Semigraphics_.DrawColoI);
                                }
                                UndoBufferStop();
                                break;

                            case "I":
                            case "Add":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[1], Semigraphics_.DrawColoI);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[12], Semigraphics_.DrawColoI);
                                }
                                UndoBufferStop();
                                break;
                            case "K":
                            case "Subtract":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[0], Semigraphics_.DrawColoI);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[11], Semigraphics_.DrawColoI);
                                }
                                UndoBufferStop();
                                break;
                            case "Space":
                            case "NumPad0":
                                UndoBufferStart();
                                CharPut(CursorX, CursorY, Semigraphics_.DrawCharI, Semigraphics_.DrawColoI);
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
                            case "DownArrow":
                            case "LeftArrow":
                            case "RightArrow":
                            case "PageUp":
                            case "End":
                            case "Home":
                            case "PageDown":
                                if (PixelPaint_.PaintPencil)
                                {
                                    UndoBufferStart();
                                }
                                switch (KeyName)
                                {
                                    case "UpArrow":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 0);
                                        break;
                                    case "DownArrow":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 1);
                                        break;
                                    case "LeftArrow":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 2);
                                        break;
                                    case "RightArrow":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 3);
                                        break;
                                    case "PageUp":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 4);
                                        break;
                                    case "End":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 5);
                                        break;
                                    case "Home":
                                        PixelPaint_.MoveCursor(CursorX, CursorY, 6);
                                        break;
                                    case "PageDown":
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

        public string PrepareFileNameStr(string NewFile_)
        {
            while ((NewFile_.Length > 0) && (TextWork.SpaceChars.Contains(NewFile_[0])))
            {
                NewFile_ = NewFile_.Substring(1);
            }
            while ((NewFile_.Length > 0) && (TextWork.SpaceChars.Contains(NewFile_[NewFile_.Length - 1])))
            {
                NewFile_ = NewFile_.Substring(0, NewFile_.Length - 1);
            }
            if (NewFile_.Length > 2)
            {
                if (((NewFile_[0] == '\"') && (NewFile_[NewFile_.Length - 1] == '\"')) || ((NewFile_[0] == '\'') && (NewFile_[NewFile_.Length - 1] == '\'')))
                {
                    NewFile_ = NewFile_.Substring(1);
                    NewFile_ = NewFile_.Substring(0, NewFile_.Length - 1);
                }
                return NewFile_;
            }
            return "";
        }

        public string PrepareFileName(List<int> NewFile_)
        {
            while ((NewFile_.Count > 0) && (TextWork.SpaceChars.Contains(NewFile_[0])))
            {
                NewFile_.RemoveAt(0);
            }
            while ((NewFile_.Count > 0) && (TextWork.SpaceChars.Contains(NewFile_[NewFile_.Count - 1])))
            {
                NewFile_.RemoveAt(NewFile_.Count - 1);
            }
            if (NewFile_.Count > 2)
            {
                if (((NewFile_[0] == 34) && (NewFile_[NewFile_.Count - 1] == 34)) || ((NewFile_[0] == 39) && (NewFile_[NewFile_.Count - 1] == 39)))
                {
                    NewFile_.RemoveAt(0);
                    NewFile_.RemoveAt(NewFile_.Count - 1);
                }
            }
            if (NewFile_.Count > 0)
            {
                string NewFile = TextWork.IntToStr(NewFile_);
                if (File.Exists(NewFile))
                {
                    return NewFile;
                }
            }
            return "";
        }

        void FileLoad0()
        {
            List<int> NewFile_ = new List<int>();
            int TempX = CursorX;
            int TempY = CursorY;
            int DeltaX = 0;
            int DeltaY = 0;
            int DeltaMargin = 3;
            switch (TextMoveDir)
            {
                case 0:
                    DeltaX = 1;
                    break;
                case 1:
                    DeltaX = 1;
                    DeltaY = 1;
                    break;
                case 2:
                    DeltaY = 1;
                    break;
                case 3:
                    DeltaX = -1;
                    DeltaY = 1;
                    break;
                case 4:
                    DeltaX = -1;
                    break;
                case 5:
                    DeltaX = -1;
                    DeltaY = -1;
                    break;
                case 6:
                    DeltaY = -1;
                    break;
                case 7:
                    DeltaX = 1;
                    DeltaY = -1;
                    break;
            }
            while ((TempX >= 0) && (TempY >= 0) && ((CharGet(TempX, TempY, false) >= 0) || (DeltaMargin > 0)))
            {
                TempX -= DeltaX;
                TempY -= DeltaY;
                DeltaMargin--;
            }
            while ((TempX != CursorX) || (TempY != CursorY))
            {
                NewFile_.Add(CharGet(TempX, TempY, true));
                TempX += DeltaX;
                TempY += DeltaY;
            }


            string NewFile = PrepareFileName(NewFile_);

            if (NewFile != "")
            {
                CurrentFileName = NewFile;
                CursorX = 0;
                CursorY = 0;
                DisplayX = 0;
                DisplayY = 0;
            }

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
            TempMemo.Push(ToggleDrawText ? 1 : 0);
            TempMemo.Push(ToggleDrawColo ? 1 : 0);
            ToggleDrawText = true;
            ToggleDrawColo = true;
            FileLoad(CurrentFileName);
            TextDisplay(0);
            ScreenRefresh(true);
            ToggleDrawColo = (TempMemo.Pop() != 0);
            ToggleDrawText = (TempMemo.Pop() != 0);
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
                                if (ToggleDrawText)
                                {
                                    TextBuffer[i].InsertRange(X, TextWork.Spaces(W + 1));
                                }
                                else
                                {
                                    TextBuffer[i].AddRange(TextWork.Spaces(W + 1));
                                }
                                if (ToggleDrawColo)
                                {
                                    TextColBuf[i].InsertRange(X, TextWork.BlkCol(W + 1));
                                }
                                else
                                {
                                    TextColBuf[i].AddRange(TextWork.BlkCol(W + 1));
                                }
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
                                TextColBuf[i].AddRange(TextWork.BlkCol((X + W + 2) - TextColBuf[i].Count));
                            }
                        }
                        for (int i = 0; i <= H; i++)
                        {
                            TextBuffer.Add(TextWork.Spaces(X + W + 2));
                            TextColBuf.Add(TextWork.BlkCol(X + W + 2));
                        }
                        for (int i = (TextBuffer.Count - H - 1); i > Y; i--)
                        {
                            List<int> TextBufOldT = TextWork.TrimEnd(TextBuffer[i + H]);
                            List<int> TextBufOldC = TextWork.TrimEnd(TextColBuf[i + H]);
                            if (ToggleDrawText)
                            {
                                TextBuffer[i + H].RemoveRange(X, W + 1);
                                TextBuffer[i + H].InsertRange(X, TextBuffer[i - 1].GetRange(X, W + 1));
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[i + H].RemoveRange(X, W + 1);
                                TextColBuf[i + H].InsertRange(X, TextColBuf[i - 1].GetRange(X, W + 1));
                            }
                            TextBufferTrimLine(i + H);
                            if (!TextWork.Equals(TextBufOldT, TextWork.TrimEnd(TextBuffer[i + H])))
                            {
                                OpExist = true;
                            }
                            if (!TextWork.Equals(TextBufOldC, TextWork.TrimEnd(TextColBuf[i + H])))
                            {
                                OpExist = true;
                            }
                        }
                        for (int i = Y; i <= (Y + H); i++)
                        {
                            List<int> TextBufOldT = TextWork.TrimEnd(TextBuffer[i]);
                            List<int> TextBufOldC = TextWork.TrimEnd(TextColBuf[i]);
                            if (ToggleDrawText)
                            {
                                TextBuffer[i].RemoveRange(X, W + 1);
                                TextBuffer[i].InsertRange(X, TextWork.Spaces(W + 1));
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[i].RemoveRange(X, W + 1);
                                TextColBuf[i].InsertRange(X, TextWork.BlkCol(W + 1));
                            }
                            TextBufferTrimLine(i);
                            if (!TextWork.Equals(TextBufOldT, TextWork.TrimEnd(TextBuffer[i])))
                            {
                                OpExist = true;
                            }
                            if (!TextWork.Equals(TextBufOldC, TextWork.TrimEnd(TextColBuf[i])))
                            {
                                OpExist = true;
                            }
                        }
                    }
                    break;
                case 2:
                case 12:
                    for (int i = 0; i < TextBuffer.Count; i++)
                    {
                        if (TextWork.TrimEndLength(TextBuffer[i]) > X)
                        {
                            OpExist = true;
                            if (ToggleDrawText)
                            {
                                TextBuffer[i].InsertRange(X, TextWork.Spaces(W + 1));
                            }
                            else
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces(W + 1));
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[i].InsertRange(X, TextWork.BlkCol(W + 1));
                            }
                            else
                            {
                                TextColBuf[i].AddRange(TextWork.BlkCol(W + 1));
                            }
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
                            if (ToggleDrawText)
                            {
                                TextBuffer.Insert(Y, new List<int>());
                            }
                            else
                            {
                                TextBuffer.Add(new List<int>());
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf.Insert(Y, new List<int>());
                            }
                            else
                            {
                                TextColBuf.Add(new List<int>());
                            }
                        }
                        for (int i = Y; i < TextBuffer.Count; i++)
                        {
                            if (TextBuffer[i].Count < TextColBuf[i].Count)
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces(TextColBuf[i].Count - TextBuffer[i].Count));
                            }
                            if (TextColBuf[i].Count < TextBuffer[i].Count)
                            {
                                TextColBuf[i].AddRange(TextWork.BlkCol(TextBuffer[i].Count - TextColBuf[i].Count));
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
                    UndoBufferItem_.ColoOld.Add(-1);
                    UndoBufferItem_.ColoNew.Add(-1);
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
                                    TextColBuf.Add(new List<int>());
                                }
                                if (TextBuffer[i_Y].Count < (X + W + 1))
                                {
                                    TextBuffer[i_Y].AddRange(TextWork.Spaces((X + W + 1) - TextBuffer[i_Y].Count));
                                    TextColBuf[i_Y].AddRange(TextWork.BlkCol((X + W + 1) - TextColBuf[i_Y].Count));
                                }
                                TextBuffer[i_Y][i_X] = Semigraphics_.DrawCharI;
                                TextColBuf[i_Y][i_X] = Semigraphics_.DrawColoI;
                                if (InsDelMode < 10)
                                {
                                    UndoBufferItem_.X.Add(i_X);
                                    UndoBufferItem_.Y.Add(i_Y);
                                    UndoBufferItem_.CharOld.Add(TextWork.SpaceChar0);
                                    UndoBufferItem_.ColoOld.Add(0);
                                    UndoBufferItem_.CharNew.Add(Semigraphics_.DrawCharI);
                                    UndoBufferItem_.ColoNew.Add(Semigraphics_.DrawColoI);
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
                            if ((TextWork.TrimEndLength(TextBuffer[i]) > X) || (TextWork.TrimEndLenCol(TextColBuf[i]) > X))
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
                                        if ((TextBuffer[i][X + ii] != TextWork.SpaceChar0) || (TextColBuf[i][X + ii] != 0))
                                        {
                                            UndoBufferItem_.X.Add(X + ii);
                                            UndoBufferItem_.Y.Add(i);
                                            UndoBufferItem_.CharOld.Add(TextBuffer[i][X + ii]);
                                            UndoBufferItem_.CharNew.Add(TextWork.SpaceChar0);
                                            UndoBufferItem_.ColoOld.Add(TextColBuf[i][X + ii]);
                                            UndoBufferItem_.ColoNew.Add(0);
                                            UndoBufferItem_.OpParams.Add(null);
                                        }
                                    }
                                }
                                if (ToggleDrawText)
                                {
                                    TextBuffer[i].AddRange(TextWork.Spaces(RemCount));
                                    TextBuffer[i].RemoveRange(X, RemCount);
                                }
                                if (ToggleDrawColo)
                                {
                                    TextColBuf[i].AddRange(TextWork.BlkCol(RemCount));
                                    TextColBuf[i].RemoveRange(X, RemCount);
                                }
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
                                TextColBuf[i].AddRange(TextWork.BlkCol((X + W + 2) - TextColBuf[i].Count));
                            }
                        }
                        if (InsDelMode < 10)
                        {
                            for (int i = Y; i <= (Y + H); i++)
                            {
                                for (int ii = X; ii <= (X + W); ii++)
                                {
                                    if (TextBuffer.Count > i)
                                    {
                                        if (TextBuffer[i].Count > ii)
                                        {
                                            if ((TextBuffer[i][ii] != TextWork.SpaceChar0) || (TextColBuf[i][ii] != 0))
                                            {
                                                UndoBufferItem_.X.Add(ii);
                                                UndoBufferItem_.Y.Add(i);
                                                UndoBufferItem_.CharOld.Add(TextBuffer[i][ii]);
                                                UndoBufferItem_.CharNew.Add(TextWork.SpaceChar0);
                                                UndoBufferItem_.ColoOld.Add(TextColBuf[i][ii]);
                                                UndoBufferItem_.ColoNew.Add(0);
                                                UndoBufferItem_.OpParams.Add(null);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        for (int i = Y; i < TextBuffer.Count; i++)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer[i].RemoveRange(X, W + 1);
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[i].RemoveRange(X, W + 1);
                            }
                            List<int> TempOldT = TextWork.TrimEnd(TextBuffer[i]);
                            List<int> TempOldC = TextWork.TrimEnd(TextColBuf[i]);

                            if (ToggleDrawText)
                            {
                                if ((i + H + 1) < TextBuffer.Count)
                                {
                                    TextBuffer[i].InsertRange(X, TextBuffer[i + H + 1].GetRange(X, W + 1));
                                }
                                else
                                {
                                    TextBuffer[i].InsertRange(X, TextWork.Spaces(W + 1));
                                }
                            }
                            if (ToggleDrawColo)
                            {
                                if ((i + H + 1) < TextColBuf.Count)
                                {
                                    TextColBuf[i].InsertRange(X, TextColBuf[i + H + 1].GetRange(X, W + 1));
                                }
                                else
                                {
                                    TextColBuf[i].InsertRange(X, TextWork.BlkCol(W + 1));
                                }
                            }

                            if (!TextWork.Equals(TempOldT, TextWork.TrimEnd(TextBuffer[i])))
                            {
                                OpExist = true;
                            }
                            if (!TextWork.Equals(TempOldC, TextWork.TrimEnC(TextColBuf[i])))
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
                        if ((TextWork.TrimEndLength(TextBuffer[i]) > X) || (TextWork.TrimEndLenCol(TextColBuf[i]) > X))
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
                                    if ((TextBuffer[i][X + ii] != TextWork.SpaceChar0) || (TextColBuf[i][X + ii] != 0))
                                    {
                                        UndoBufferItem_.X.Add(X + ii);
                                        UndoBufferItem_.Y.Add(i);
                                        UndoBufferItem_.CharOld.Add(TextBuffer[i][X + ii]);
                                        UndoBufferItem_.CharNew.Add(TextWork.SpaceChar0);
                                        UndoBufferItem_.ColoOld.Add(TextColBuf[i][X + ii]);
                                        UndoBufferItem_.ColoNew.Add(0);
                                        UndoBufferItem_.OpParams.Add(null);
                                    }
                                }
                            }
                            if (ToggleDrawText)
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces(RemCount));
                                TextBuffer[i].RemoveRange(X, RemCount);
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[i].AddRange(TextWork.BlkCol(RemCount));
                                TextColBuf[i].RemoveRange(X, RemCount);
                            }
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
                            if (TextWork.TrimEndLenCol(TextColBuf[i]) > 0)
                            {
                                OpExist = true;
                            }
                        }
                        for (int i = 0; i <= H; i++)
                        {
                            if (TextBuffer.Count > Y)
                            {
                                if (InsDelMode < 10)
                                {
                                    for (int ii = 0; ii < TextBuffer[Y].Count; ii++)
                                    {
                                        if ((TextBuffer[Y][ii] != TextWork.SpaceChar0) || (TextColBuf[Y][ii] != 0))
                                        {
                                            UndoBufferItem_.X.Add(ii);
                                            UndoBufferItem_.Y.Add(Y + i);
                                            UndoBufferItem_.CharOld.Add(TextBuffer[Y][ii]);
                                            UndoBufferItem_.CharNew.Add(TextWork.SpaceChar0);
                                            UndoBufferItem_.ColoOld.Add(TextColBuf[Y][ii]);
                                            UndoBufferItem_.ColoNew.Add(0);
                                            UndoBufferItem_.OpParams.Add(null);
                                        }
                                    }
                                }
                                if (ToggleDrawText)
                                {
                                    TextBuffer.RemoveAt(Y);
                                    TextBuffer.Add(new List<int>());
                                }
                                if (ToggleDrawColo)
                                {
                                    TextColBuf.RemoveAt(Y);
                                    TextColBuf.Add(new List<int>());
                                }
                            }
                        }
                        for (int i = Y; i < TextBuffer.Count; i++)
                        {
                            if (TextBuffer[i].Count < TextColBuf[i].Count)
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces(TextColBuf[i].Count - TextBuffer[i].Count));
                            }
                            if (TextColBuf[i].Count < TextBuffer[i].Count)
                            {
                                TextColBuf[i].AddRange(TextWork.BlkCol(TextBuffer[i].Count - TextColBuf[i].Count));
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
                    UndoBufferItem_.ColoOld.Add(-1);
                    UndoBufferItem_.ColoNew.Add(-1);
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
                TextColBuf.RemoveAt(TextColBuf.Count - 1);
            }
        }
        
        void TextBufferTrimLine(int i)
        {
            while ((TextBuffer[i].Count > 0) && (TextWork.SpaceChars.Contains(TextBuffer[i][TextBuffer[i].Count - 1])) && (TextColBuf[i][TextColBuf[i].Count - 1] == 0))
            {
                TextBuffer[i].RemoveRange(TextBuffer[i].Count - 1, 1);
                TextColBuf[i].RemoveRange(TextColBuf[i].Count - 1, 1);
            }
        }


        struct UndoBufferItem
        {
            public bool ToggleDrawText;
            public bool ToggleDrawColo;
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
            public List<int> ColoOld;
            public List<int> ColoNew;
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
            UndoBufferItem_.ToggleDrawText = ToggleDrawText;
            UndoBufferItem_.ToggleDrawColo = ToggleDrawColo;
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
            UndoBufferItem_.ColoOld = new List<int>();
            UndoBufferItem_.ColoNew = new List<int>();
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
                bool ToggleDrawText_ = ToggleDrawText;
                bool ToggleDrawColo_ = ToggleDrawColo;
                UndoBufferIndex--;
                UndoBufferItem_ = UndoBuffer[UndoBufferIndex];
                ToggleDrawText = UndoBufferItem_.ToggleDrawText;
                ToggleDrawColo = UndoBufferItem_.ToggleDrawColo;
                for (int i = (UndoBufferItem_.X.Count - 1); i >= 0; i--)
                {
                    if ((UndoBufferItem_.CharOld[i] >= 0) || (UndoBufferItem_.ColoOld[i] >= 0))
                    {
                        CharPut(UndoBufferItem_.X[i], UndoBufferItem_.Y[i], UndoBufferItem_.CharOld[i], UndoBufferItem_.ColoOld[i]);
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
                ToggleDrawText = ToggleDrawText_;
                ToggleDrawColo = ToggleDrawColo_;
            }
            TextBufferTrim();
            CursorLimit();
            TextDisplay(0);
        }

        public void UndoBufferRedo()
        {
            if (UndoBufferIndex < UndoBuffer.Count)
            {
                bool ToggleDrawText_ = ToggleDrawText;
                bool ToggleDrawColo_ = ToggleDrawColo;
                UndoBufferItem_ = UndoBuffer[UndoBufferIndex];
                ToggleDrawText = UndoBufferItem_.ToggleDrawText;
                ToggleDrawColo = UndoBufferItem_.ToggleDrawColo;
                for (int i = 0; i < UndoBufferItem_.X.Count; i++)
                {
                    if ((UndoBufferItem_.CharNew[i] >= 0) || (UndoBufferItem_.ColoNew[i] >= 0))
                    {
                        CharPut(UndoBufferItem_.X[i], UndoBufferItem_.Y[i], UndoBufferItem_.CharNew[i], UndoBufferItem_.ColoNew[i]);
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
                ToggleDrawText = ToggleDrawText_;
                ToggleDrawColo = ToggleDrawColo_;
            }
            TextBufferTrim();
            CursorLimit();
            TextDisplay(0);
        }
    }
}
