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
        const int MaxlineSize = 10000;

        // 0 - Text editor
        // 1 - ANSI display and server
        // 2 - Telnet client
        // 3 - Keystroke and encoding tester
        // 4 - Render
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
        List<List<int>> EncodingInfo = new List<List<int>>();
        List<int> EncodingCodePage = new List<int>();
        List<int[]> EncodingFile = new List<int[]>();
        int EncodingListI;
        int EncodingListL;
        int EncodingByte = -1;
        int[] EncodingArray;
        List<int> EncodingKey1 = new List<int>();
        List<int> EncodingKey2 = new List<int>();

        void EncodingAddParam(string ParamName, string DisplayName, ConfigFile CF)
        {
            if (CF.ParamExists(ParamName))
            {
                if (EncodingList[0].StartsWith("Items"))
                {
                    EncodingList.Insert(0, "");
                    EncodingInfo.Insert(0, null);
                    EncodingCodePage.Insert(0, -1);
                }
                Encoding ParEnc = TextWork.EncodingFromName(CF.ParamGetS(ParamName));
                string EncName = "";
                if (ParEnc is OneByteEncoding)
                {
                    EncName = DisplayName + ": " + TextWork.EncodingGetName(ParEnc);
                    EncodingList.Insert(0, EncName);
                    EncodingInfo.Insert(0, TextWork.StrToInt(" FILE: "));
                    int[] EncRaw = ((OneByteEncoding)ParEnc).DefExport();
                    EncodingCodePage.Insert(0, -2);
                    EncodingFile.Insert(0, EncRaw);
                }
                else
                {
                    EncName = DisplayName + ": " + ParEnc.CodePage;
                    EncodingList.Insert(0, EncName);
                    EncodingInfo.Insert(0, TextWork.StrToInt(ParEnc.CodePage.ToString().PadLeft(5) + ": "));
                    EncodingCodePage.Insert(0, ParEnc.CodePage);
                    EncodingFile.Insert(0, null);
                }
                if (EncodingListL < EncName.Length)
                {
                    EncodingListL = EncName.Length;
                }
            }
        }

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
            CommandEndChar = TextWork.StrToInt(CommandEndChar_);

            for (int i = 0; i < 16; i++)
            {
                Color256[i] = i;
            }
            for (int i_R = 0; i_R < 6; i_R++)
            {
                for (int i_G = 0; i_G < 6; i_G++)
                {
                    for (int i_B = 0; i_B < 6; i_B++)
                    {
                        int i_ = i_R * 36 + i_G * 6 + i_B + 16;
                        Color256[i_] = 0;
                        if (i_B >= 3)
                        {
                            Color256[i_] = Color256[i_] + 4;
                        }
                        if (i_G >= 3)
                        {
                            Color256[i_] = Color256[i_] + 2;
                        }
                        if (i_R >= 3)
                        {
                            Color256[i_] = Color256[i_] + 1;
                        }
                        if ((i_R + i_G + i_B) >= 8)
                        {
                            Color256[i_] += 8;
                        }
                    }
                }
            }
            for (int i = 0; i < 6; i++)
            {
                Color256[232 + i] = 0;
                Color256[238 + i] = 8;
                Color256[244 + i] = 7;
                Color256[250 + i] = 15;
            }
        }

        List<List<int>> TextBuffer = new List<List<int>>();
        List<List<int>> TextColBuf = new List<List<int>>();
        List<List<int>> TextFonBuf = new List<List<int>>();
        List<List<int>> ScrCharType;
        List<List<int>> ScrCharStr;
        List<List<int>> ScrCharCol;
        List<List<int>> ScrCharFon;

        List<List<int>> ScrCharTypeDisp;
        List<List<int>> ScrCharStrDisp;
        List<List<int>> ScrCharColDisp;
        List<List<int>> ScrCharFonDisp;

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
                        ScrCharFon[i] = TextWork.BlkCol(InfoTemp.Length);
                        ScrCharStr[i].AddRange(TextWork.Spaces(WinTxtW - InfoTemp.Length));
                        ScrCharCol[i].AddRange(TextWork.BlkCol(WinTxtW - InfoTemp.Length));
                        ScrCharFon[i].AddRange(TextWork.BlkCol(WinTxtW - InfoTemp.Length));
                    }
                    else
                    {
                        ScrCharStr[i] = TextWork.Spaces(WinTxtW);
                        ScrCharCol[i] = TextWork.BlkCol(WinTxtW);
                        ScrCharFon[i] = TextWork.BlkCol(WinTxtW);
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
                            List<int> F = TextFonBuf[i + DisplayY];
                            if (DisplayX > 0)
                            {
                                if (S.Count > DisplayX)
                                {
                                    S = S.GetRange(DisplayX, S.Count - DisplayX);
                                    C = C.GetRange(DisplayX, C.Count - DisplayX);
                                    F = F.GetRange(DisplayX, F.Count - DisplayX);
                                }
                                else
                                {
                                    S = new List<int>();
                                    C = new List<int>();
                                    F = new List<int>();
                                }
                            }
                            if (S.Count < WinTxtW)
                            {
                                ScrCharStr[i] = TextWork.Concat(S, TextWork.Spaces(WinTxtW - S.Count));
                                ScrCharCol[i] = TextWork.Concat(C, TextWork.BlkCol(WinTxtW - C.Count));
                                ScrCharFon[i] = TextWork.Concat(F, TextWork.BlkCol(WinTxtW - F.Count));
                                ScrCharType[i] = TextWork.Concat(TextWork.Pad(S.Count, 0), TextWork.Pad(WinTxtW - S.Count, 1));
                            }
                            else
                            {
                                ScrCharStr[i] = S.GetRange(0, WinTxtW);
                                ScrCharCol[i] = C.GetRange(0, WinTxtW);
                                ScrCharFon[i] = F.GetRange(0, WinTxtW);
                                ScrCharType[i] = TextWork.Pad(WinTxtW, 0);
                            }
                        }
                        else
                        {
                            ScrCharStr[i] = TextWork.Spaces(WinTxtW);
                            ScrCharCol[i] = TextWork.BlkCol(WinTxtW);
                            ScrCharFon[i] = TextWork.BlkCol(WinTxtW);
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
                        int ChFon = 0;
                        if ((i + DisplayY) < TextBuffer.Count)
                        {
                            if (TextBuffer[i + DisplayY].Count > (DisplayX + CurOffset))
                            {
                                ChStr = TextBuffer[i + DisplayY][DisplayX + CurOffset];
                                ChCol = TextColBuf[i + DisplayY][DisplayX + CurOffset];
                                ChFon = TextFonBuf[i + DisplayY][DisplayX + CurOffset];
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
                            ScrCharFon[i][0] = ChFon;
                        }
                        else
                        {
                            ScrCharType[i][WinTxtW - 1] = ChType;
                            ScrCharStr[i][WinTxtW - 1] = ChStr;
                            ScrCharCol[i][WinTxtW - 1] = ChCol;
                            ScrCharFon[i][WinTxtW - 1] = ChFon;
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

        List<int> ElementGet_Kind = new List<int>();
        List<int> ElementGet_Count = new List<int>();

        private int ElementGet(List<List<int>> Data, int X, int Y, int Default)
        {
            ElementGet_Kind.Clear();
            ElementGet_Count.Clear();
            int MostCount = 1;
            for (int YY = Y; YY < (Y + CursorFontH); YY++)
            {
                for (int XX = X; XX < (X + CursorFontW); XX++)
                {
                    int Kind = Default;
                    if ((Data.Count > YY) && (YY >= 0))
                    {
                        if ((Data[YY].Count > XX) && (XX >= 0))
                        {
                            Kind = Data[YY][XX];
                        }
                    }
                    int Idx = ElementGet_Kind.IndexOf(Kind);
                    if (Idx >= 0)
                    {
                        ElementGet_Count[Idx] = ElementGet_Count[Idx] + 1;
                        if (MostCount < ElementGet_Count[Idx])
                        {
                            MostCount = ElementGet_Count[Idx];
                        }
                    }
                    else
                    {
                        ElementGet_Kind.Add(Kind);
                        ElementGet_Count.Add(1);
                    }
                }
            }
            for (int i = 0; i < ElementGet_Kind.Count; i++)
            {
                if (ElementGet_Count[i] == MostCount)
                {
                    return ElementGet_Kind[i];
                }
            }
            return Default;
        }

        public int FontGet(int X, int Y)
        {
            if ((TextFonBuf.Count > Y) && (Y >= 0))
            {
                if ((TextFonBuf[Y].Count > X) && (X >= 0))
                {
                    return TextFonBuf[Y][X];
                }
            }
            return 0;
        }

        public int ColoGet(int X, int Y, bool Space, bool SingleCell)
        {
            if (SingleCell || ((CursorFontW == 1) && (CursorFontH == 1)))
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
            else
            {
                return ElementGet(TextColBuf, X, Y, Space ? 0 : -1);
            }
        }

        public int CharGet(int X, int Y, bool Space, bool SingleCell)
        {
            if (SingleCell || ((CursorFontW == 1) && (CursorFontH == 1)))
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
            else
            {
                return ElementGet(TextBuffer, X, Y, Space ? TextWork.SpaceChar0 : -1);
            }
        }


        public void CharPut(int X, int Y, int Ch, int Col, int Fon, bool SingleCell)
        {
            int CursorFontW_ = SingleCell ? 1 : CursorFontW;
            int CursorFontH_ = SingleCell ? 1 : CursorFontH;

            if (X < 0)
            {
                return;
            }
            if (Y < 0)
            {
                return;
            }

            int Font_W, Font_H;
            if (Fon >= 0)
            {
                FontSFromInt(Fon, out Font_W, out Font_H);
            }

            while (TextBuffer.Count <= (Y + CursorFontH - 1))
            {
                TextBuffer.Add(new List<int>());
                TextColBuf.Add(new List<int>());
                TextFonBuf.Add(new List<int>());
                TextDisplayLine(TextBuffer.Count - 1);
            }

            for (int YY = 0; YY < CursorFontH_; YY++)
            {
                for (int XX = 0; XX < CursorFontW_; XX++)
                {
                    int Ch_ = CharGet(X + XX, Y + YY, true, true);
                    int Col_ = ColoGet(X + XX, Y + YY, true, true);
                    int Fon_ = FontGet(X + XX, Y + YY);

                    if (!ToggleDrawText)
                    {
                        Ch = Ch_;
                        Fon = Fon_;
                    }
                    if (!ToggleDrawColo)
                    {
                        Col = Col_;
                    }

                    if (!SingleCell)
                    {
                        if (ToggleDrawText)
                        {
                            Font_W = FontSizeCode(CursorFontW_, XX);
                            Font_H = FontSizeCode(CursorFontH_, YY);
                            Fon = FontSToInt(Font_W, Font_H);
                        }
                    }

                    if (TextBuffer[Y + YY].Count > (X + XX))
                    {
                        TextBuffer[Y + YY][X + XX] = Ch;
                        TextColBuf[Y + YY][X + XX] = Col;
                        TextFonBuf[Y + YY][X + XX] = Fon;
                        if (TextWork.SpaceChars.Contains(Ch) && (Col == 0))
                        {
                            TextBufferTrimLine(Y + YY);
                        }
                    }
                    else
                    {
                        if ((!TextWork.SpaceChars.Contains(Ch)) || (Col != 0))
                        {
                            if (TextBuffer[Y + YY].Count < (X + XX))
                            {
                                TextBuffer[Y + YY].AddRange(TextWork.Spaces((X + XX) - TextBuffer[Y + YY].Count));
                                TextColBuf[Y + YY].AddRange(TextWork.BlkCol((X + XX) - TextColBuf[Y + YY].Count));
                                TextFonBuf[Y + YY].AddRange(TextWork.BlkCol((X + XX) - TextFonBuf[Y + YY].Count));
                            }
                            TextBuffer[Y + YY].Add(Ch);
                            TextColBuf[Y + YY].Add(Col);
                            TextFonBuf[Y + YY].Add(Fon);
                        }
                    }

                    if (UndoBufferEnabled)
                    {
                        bool UndoBufNew = true;
                        for (int i = 0; i < UndoBufferItem_.X.Count; i++)
                        {
                            if (UndoBufferItem_.X[i] == (X + XX))
                            {
                                if (UndoBufferItem_.Y[i] == (Y + YY))
                                {
                                    UndoBufferItem_.CharNew[i] = Ch;
                                    UndoBufferItem_.ColoNew[i] = Col;
                                    UndoBufferItem_.FontNew[i] = Fon;
                                    UndoBufNew = false;
                                    break;
                                }
                            }
                        }
                        if (UndoBufNew)
                        {
                            UndoBufferItem_.X.Add(X + XX);
                            UndoBufferItem_.Y.Add(Y + YY);
                            UndoBufferItem_.CharOld.Add(Ch_);
                            UndoBufferItem_.ColoOld.Add(Col_);
                            UndoBufferItem_.FontOld.Add(Fon_);
                            UndoBufferItem_.CharNew.Add(Ch);
                            UndoBufferItem_.ColoNew.Add(Col);
                            UndoBufferItem_.FontNew.Add(Fon);
                            UndoBufferItem_.OpParams.Add(null);
                        }
                    }
                }
            }
            while ((TextBuffer.Count > 0) && (TextBuffer[TextBuffer.Count - 1].Count == 0))
            {
                TextBuffer.RemoveAt(TextBuffer.Count - 1);
                TextColBuf.RemoveAt(TextColBuf.Count - 1);
                TextFonBuf.RemoveAt(TextFonBuf.Count - 1);
                TextDisplayLine(TextBuffer.Count);
            }
            for (int YY = 0; YY < CursorFontH_; YY++)
            {
                TextDisplayLine(Y + YY);
            }
        }






        public void CursorChar_(int XX, int YY, int X, int Y, bool Show)
        {
            if ((X != XX) || (Y != YY))
            {
                CursorChar(X, Y, Show);
            }
        }

        public void CursorChar(int X, int Y, bool Show)
        {
            if (InfoScreen_.Shown)
            {
                return;
            }
            int XMin = Math.Max(0, X);
            int XMax = Math.Min(X + CursorFontW, WinTxtW);
            int YMin = Math.Max(0, Y);
            int YMax = Math.Min(Y + CursorFontH, WinTxtH);
            for (int YY = YMin; YY < YMax; YY++)
            {
                for (int XX = XMin; XX < XMax; XX++)
                {
                    if (Show)
                    {
                        if (ScrCharType[YY][XX] < 3)
                        {
                            ScrCharType[YY][XX] += 3;
                        }
                    }
                    else
                    {
                        if (ScrCharType[YY][XX] > 2)
                        {
                            ScrCharType[YY][XX] -= 3;
                        }
                    }
                }
            }
        }

        public void CursorCharX(int X, int Y, bool Show)
        {
            if (InfoScreen_.Shown)
            {
                return;
            }
            int XMin = Math.Max(0, X);
            int XMax = Math.Min(X + CursorFontW, WinTxtW);
            int YMin = Math.Max(0, Y);
            int YMax = Math.Min(Y + CursorFontH, WinTxtH);
            for (int YY = YMin; YY < YMax; YY++)
            {
                for (int XX = XMin; XX < XMax; XX++)
                {
                    if ((XX != X) || (YY != Y))
                    {
                        if (Show)
                        {
                            if (ScrCharType[YY][XX] < 3)
                            {
                                ScrCharType[YY][XX] += 3;
                            }
                        }
                        else
                        {
                            if (ScrCharType[YY][XX] > 2)
                            {
                                ScrCharType[YY][XX] -= 3;
                            }
                        }
                    }
                }
            }
        }


        public void CursorLine(bool Show)
        {
            int XX = CursorX - DisplayX;
            int YY = CursorY - DisplayY;
            int XX0 = CursorX0();
            int YY0 = CursorY0();
            if (WorkState == WorkStateDef.DrawChar)
            {
                if (Semigraphics_.DiamondType == 0)
                {
                    int X1 = Math.Min(XX, XX + (CursorXSize * CursorFontW));
                    int X2 = Math.Max(XX, XX + (CursorXSize * CursorFontW));
                    int Y1 = Math.Min(YY, YY + (CursorYSize * CursorFontH));
                    int Y2 = Math.Max(YY, YY + (CursorYSize * CursorFontH));

                    X1 = Math.Max(Math.Min(X1, WinTxtW - 1), XX0);
                    X2 = Math.Max(Math.Min(X2, WinTxtW - 1), XX0);
                    Y1 = Math.Max(Math.Min(Y1, WinTxtH - 1), YY0);
                    Y2 = Math.Max(Math.Min(Y2, WinTxtH - 1), YY0);

                    for (int Y = Y1; Y <= Y2; Y += CursorFontH)
                    {
                        for (int X = X1; X <= X2; X += CursorFontW)
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
                    int X1 = Math.Min(0, CursorXSize);
                    int X2 = Math.Max(0, CursorXSize);
                    int Y1 = Math.Min(0, CursorYSize);
                    int Y2 = Math.Max(0, CursorYSize);

                    for (int X_ = X1; X_ <= X2; X_++)
                    {
                        for (int Y_ = Y1; Y_ <= Y2; Y_++)
                        {
                            int X__ = XX + ((X_ - Y_) * CursorFontW);
                            int Y__ = YY + ((X_ + Y_) * CursorFontH);

                            CursorChar_(XX, YY, X__, Y__, Show);

                            switch (Semigraphics_.DiamondType)
                            {
                                case 1:
                                    if ((X_ < X2) && (Y_ > Y1))
                                    {
                                        CursorChar_(XX, YY, X__ + CursorFontW, Y__, Show);
                                    }
                                    break;
                                case 2:
                                    CursorChar_(XX, YY, X__ + CursorFontW, Y__, Show);
                                    break;
                                case 3:
                                    CursorChar_(XX, YY, X__, Y__ + CursorFontH, Show);
                                    break;
                                case 4:
                                    CursorChar_(XX, YY, X__ - CursorFontW, Y__, Show);
                                    break;
                                case 5:
                                    CursorChar_(XX, YY, X__, Y__ - CursorFontH, Show);
                                    break;
                                case 6:
                                    CursorChar_(XX, YY, X__ + CursorFontW, Y__, Show);
                                    CursorChar_(XX, YY, X__, Y__ - CursorFontH, Show);
                                    CursorChar_(XX, YY, X__ + CursorFontW, Y__ - CursorFontH, Show);
                                    break;
                                case 7:
                                    CursorChar_(XX, YY, X__ + CursorFontW, Y__, Show);
                                    CursorChar_(XX, YY, X__, Y__ + CursorFontH, Show);
                                    CursorChar_(XX, YY, X__ + CursorFontW, Y__ + CursorFontH, Show);
                                    break;
                                case 8:
                                    CursorChar_(XX, YY, X__ - CursorFontW, Y__, Show);
                                    CursorChar_(XX, YY, X__, Y__ + CursorFontH, Show);
                                    CursorChar_(XX, YY, X__ - CursorFontW, Y__ + CursorFontH, Show);
                                    break;
                                case 9:
                                    CursorChar_(XX, YY, X__ - CursorFontW, Y__, Show);
                                    CursorChar_(XX, YY, X__, Y__ - CursorFontH, Show);
                                    CursorChar_(XX, YY, X__ - CursorFontW, Y__ - CursorFontH, Show);
                                    break;
                            }
                        }
                    }
                }
            }
            if (WorkState == WorkStateDef.DrawPixel)
            {
                int XX2 = PixelPaint_.GetCursorPosXSize() - DisplayX;
                int YY2 = PixelPaint_.GetCursorPosYSize() - DisplayY;
                if ((XX != XX2) || (YY != YY2))
                {
                    int XX3 = Math.Max(Math.Min(XX2, WinTxtW - 1), 0);
                    int YY3 = Math.Max(Math.Min(YY2, WinTxtH - 1), 0);
                    CursorChar_(XX, YY, XX3, YY3, Show);
                    if ((XX2 < 0) || (XX2 >= WinTxtW))
                    {
                        CursorChar_(XX, YY, XX3, YY3 - CursorFontH, Show);
                        CursorChar_(XX, YY, XX3, YY3 + CursorFontH, Show);
                    }
                    if ((YY2 < 0) || (YY2 >= WinTxtH))
                    {
                        CursorChar_(XX, YY, XX3 - CursorFontW, YY3, Show);
                        CursorChar_(XX, YY, XX3 + CursorFontW, YY3, Show);
                    }
                }
            }

            if (CursorDisplay)
            {
                CursorChar(XX, YY, Show);
            }
            else
            {
                CursorCharX(XX, YY, Show);
            }
            if ((CursorType == 1) || (CursorType == 3))
            {
                for (int X = XX - CursorFontW; X > (0 - CursorFontW); X -= CursorFontW)
                {
                    CursorChar(X, YY, Show);
                }
                for (int X = XX + CursorFontW; X < WinTxtW; X += CursorFontW)
                {
                    CursorChar(X, YY, Show);
                }
                for (int Y = YY - CursorFontH; Y > (0 - CursorFontH); Y -= CursorFontH)
                {
                    CursorChar(XX, Y, Show);
                }
                for (int Y = YY + CursorFontH; Y < WinTxtH; Y += CursorFontH)
                {
                    CursorChar(XX, Y, Show);
                }
            }
            if ((CursorType == 2) || (CursorType == 3))
            {
                int XX_, YY_;
                XX_ = XX - CursorFontW;
                YY_ = YY - CursorFontH;
                while ((XX_ > (0 - CursorFontW)) && (YY_ > (0 - CursorFontH)))
                {
                    CursorChar(XX_, YY_, Show);
                    XX_ -= CursorFontW;
                    YY_ -= CursorFontH;
                }
                XX_ = XX + CursorFontW;
                YY_ = YY + CursorFontH;
                while ((XX_ < WinTxtW) && (YY_ < WinTxtH))
                {
                    CursorChar(XX_, YY_, Show);
                    XX_ += CursorFontW;
                    YY_ += CursorFontH;
                }
                XX_ = XX - CursorFontW;
                YY_ = YY + CursorFontH;
                while ((XX_ > (0 - CursorFontW)) && (YY_ < WinTxtH))
                {
                    CursorChar(XX_, YY_, Show);
                    XX_ -= CursorFontW;
                    YY_ += CursorFontH;
                }
                XX_ = XX + CursorFontW;
                YY_ = YY - CursorFontH;
                while ((XX_ < WinTxtW) && (YY_ > (0 - CursorFontH)))
                {
                    CursorChar(XX_, YY_, Show);
                    XX_ += CursorFontW;
                    YY_ -= CursorFontH;
                }
            }
        }


        public void CursorEquivPos(int Dir)
        {
            if (WorkState == WorkStateDef.DrawChar)
            {
                int CursorXSize_X = CursorXSize * CursorFontW;
                int CursorYSize_Y = CursorYSize * CursorFontH;
                int CursorXSize_Y = CursorXSize * CursorFontH;
                int CursorYSize_X = CursorYSize * CursorFontW;
                if (Semigraphics_.DiamondType == 0)
                {
                    if (((CursorXSize < 0) & (CursorYSize < 0)) || ((CursorXSize > 0) & (CursorYSize > 0)))
                    {
                        if (Dir > 0) { CursorX += CursorXSize_X; CursorXSize = 0 - CursorXSize; }
                        if (Dir < 0) { CursorY += CursorYSize_Y; CursorYSize = 0 - CursorYSize; }
                        CursorLimit();
                        return;
                    }
                    if (((CursorXSize > 0) & (CursorYSize < 0)) || ((CursorXSize < 0) & (CursorYSize > 0)))
                    {
                        if (Dir > 0) { CursorY += CursorYSize_Y; CursorYSize = 0 - CursorYSize; }
                        if (Dir < 0) { CursorX += CursorXSize_X; CursorXSize = 0 - CursorXSize; }
                        CursorLimit();
                        return;
                    }
                    if ((CursorXSize == 0) || (CursorYSize == 0))
                    {
                        CursorX += CursorXSize_X; CursorXSize = 0 - CursorXSize;
                        CursorY += CursorYSize_Y; CursorYSize = 0 - CursorYSize;
                        CursorLimit();
                        return;
                    }
                }
                else
                {
                    if (((CursorXSize < 0) & (CursorYSize < 0)) || ((CursorXSize > 0) & (CursorYSize > 0)))
                    {
                        if (Dir > 0) { CursorX += CursorXSize_X; CursorY += CursorXSize_Y; CursorXSize = 0 - CursorXSize; }
                        if (Dir < 0) { CursorX -= CursorYSize_X; CursorY += CursorYSize_Y; CursorYSize = 0 - CursorYSize; }
                        CursorLimit();
                        return;
                    }
                    if (((CursorXSize > 0) & (CursorYSize < 0)) || ((CursorXSize < 0) & (CursorYSize > 0)))
                    {
                        if (Dir > 0) { CursorX -= CursorYSize_X; CursorY += CursorYSize_Y; CursorYSize = 0 - CursorYSize; }
                        if (Dir < 0) { CursorX += CursorXSize_X; CursorY += CursorXSize_Y; CursorXSize = 0 - CursorXSize; }
                        CursorLimit();
                        return;
                    }
                    if (((CursorXSize == 0) & (CursorYSize != 0)) || ((CursorXSize != 0) & (CursorYSize == 0)))
                    {
                        CursorX += (CursorXSize_X - CursorYSize_X);
                        CursorY += (CursorXSize_Y + CursorYSize_Y);
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
                    if (CursorY >= CursorFontH)
                    {
                        CursorY -= CursorFontH;
                    }
                    break;
                case 1:
                    {
                        CursorY += CursorFontH;
                    }
                    break;
                case 2:
                    if (CursorX >= CursorFontW)
                    {
                        CursorX -= CursorFontW;
                    }
                    break;
                case 3:
                    {
                        CursorX += CursorFontW;
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
                ScrCharFon.Insert(0, new List<int>());
                ScrCharStr.RemoveAt(WinTxtH);
                ScrCharCol.RemoveAt(WinTxtH);
                ScrCharFon.RemoveAt(WinTxtH);
                ScrCharTypeDisp.Insert(0, BlankDispLineT());
                ScrCharTypeDisp.RemoveAt(WinTxtH);
                ScrCharStrDisp.Insert(0, BlankDispLineT());
                ScrCharColDisp.Insert(0, BlankDispLineC());
                ScrCharFonDisp.Insert(0, BlankDispLineC());
                ScrCharStrDisp.RemoveAt(WinTxtH);
                ScrCharColDisp.RemoveAt(WinTxtH);
                ScrCharFonDisp.RemoveAt(WinTxtH);
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
                ScrCharFon.RemoveAt(0);
                ScrCharStr.Add(new List<int>());
                ScrCharCol.Add(new List<int>());
                ScrCharFon.Add(new List<int>());
                ScrCharTypeDisp.RemoveAt(0);
                ScrCharTypeDisp.Add(BlankDispLineT());
                ScrCharStrDisp.RemoveAt(0);
                ScrCharColDisp.RemoveAt(0);
                ScrCharFonDisp.RemoveAt(0);
                ScrCharStrDisp.Add(BlankDispLineT());
                ScrCharColDisp.Add(BlankDispLineC());
                ScrCharFonDisp.Add(BlankDispLineC());
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
                    ScrCharFon[i].Insert(0, 0);
                    ScrCharStr[i].RemoveAt(WinTxtW);
                    ScrCharCol[i].RemoveAt(WinTxtW);
                    ScrCharFon[i].RemoveAt(WinTxtW);
                    ScrCharTypeDisp[i].Insert(0, TextWork.SpaceChar0);
                    ScrCharTypeDisp[i].RemoveAt(WinTxtW);
                    ScrCharStrDisp[i].Insert(0, TextWork.SpaceChar0);
                    ScrCharColDisp[i].Insert(0, 0);
                    ScrCharFonDisp[i].Insert(0, 0);
                    ScrCharStrDisp[i].RemoveAt(WinTxtW);
                    ScrCharColDisp[i].RemoveAt(WinTxtW);
                    ScrCharFonDisp[i].RemoveAt(WinTxtW);
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
                    ScrCharFon[i].RemoveAt(0);
                    ScrCharStr[i].Add(TextWork.SpaceChar0);
                    ScrCharCol[i].Add(0);
                    ScrCharFon[i].Add(0);
                    ScrCharTypeDisp[i].Add(TextWork.SpaceChar0);
                    ScrCharTypeDisp[i].RemoveAt(0);
                    ScrCharStrDisp[i].Add(TextWork.SpaceChar0);
                    ScrCharColDisp[i].Add(0);
                    ScrCharFonDisp[i].Add(0);
                    ScrCharStrDisp[i].RemoveAt(0);
                    ScrCharColDisp[i].RemoveAt(0);
                    ScrCharFonDisp[i].RemoveAt(0);
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


        void ClipboardWorkEvent1(bool Paste)
        {
            if (Paste)
            {
                UndoBufferStart();
            }
        }
        void ClipboardWorkEvent2(bool Paste)
        {
            if (Paste)
            {
                UndoBufferStop();
                ScreenRefresh(false);
            }
        }

        public void Init(string CurrentFileName_, string[] CmdArgs)
        {
            ConfigFile CF = new ConfigFile();
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

            Semigraphics_ = new Semigraphics(this, (CF.ParamGetI("WinUse") > 0) ? CF.ParamGetS("WinFontName") : "");
            Clipboard_ = new Clipboard(this);
            Clipboard_.TextClipboardWorkEvent1 += ClipboardWorkEvent1;
            Clipboard_.TextClipboardWorkEvent2 += ClipboardWorkEvent2;

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
            PixelPaint_.ClipboardWorkEvent1 += ClipboardWorkEvent1;
            PixelPaint_.ClipboardWorkEvent2 += ClipboardWorkEvent2;

            TextCipher_ = new TextCipher(CF, this);

            CurrentFileName_ = PrepareFileNameStr(CurrentFileName_);

            if ("".Equals(CurrentFileName_))
            {
                CurrentFileName_ = AppDir() + "Config.txt";
            }


            // Load character maps
            string[] CharDOS = CF.ParamGetS("ANSICharsDOS").Split(',');
            string[] CharsVT100 = CF.ParamGetS("ANSICharsVT100").Split(',');
            string[] CharsVT52 = CF.ParamGetS("ANSICharsVT52").Split(',');
            for (int i = 0; i < 32; i++)
            {
                if (CharDOS.Length >= 32)
                {
                    int T = TextWork.CodeChar(CharDOS[i]);
                    if (T >= 32)
                    {
                        DosControl[i] = T;
                    }
                    else
                    {
                        DosControl[i] = 32;
                    }
                }
                if (CharsVT100.Length >= 32)
                {
                    int T = TextWork.CodeChar(CharsVT100[i]);
                    if (T >= 32)
                    {
                        VT100_SemigraphChars[i] = T;
                    }
                    else
                    {
                        VT100_SemigraphChars[i] = 32;
                    }
                }
                if (CharsVT52.Length >= 32)
                {
                    int T = TextWork.CodeChar(CharsVT52[i]);
                    if (T >= 32)
                    {
                        VT52_SemigraphChars[i] = T;
                    }
                    else
                    {
                        VT52_SemigraphChars[i] = 32;
                    }
                }
            }


            // Create key substitute map
            int SubI = 1;
            SubstituteKey = CF.ParamGetS("SunstituteKey");
            string SubO = CF.ParamGetS("SunstituteKey" + SubI.ToString() + "O");
            string SubR = CF.ParamGetS("SunstituteKey" + SubI.ToString() + "R");
            while ((!("".Equals(SubO))) && (!("".Equals(SubR))))
            {
                SubstituteMap.Add(SubR, SubO);
                SubI++;
                SubO = CF.ParamGetS("SunstituteKey" + SubI.ToString() + "O");
                SubR = CF.ParamGetS("SunstituteKey" + SubI.ToString() + "R");
            }
            if ("".Equals(SubstituteKey))
            {
                SubstituteKey = "_#_";
            }

            CurrentFileName = CurrentFileName_;
            
            WinW = -1;
            WinH = -1;
            ScrCharType = new List<List<int>>();
            ScrCharStr = new List<List<int>>();
            ScrCharCol = new List<List<int>>();
            ScrCharFon = new List<List<int>>();
            TextBuffer = new List<List<int>>();
            TextColBuf = new List<List<int>>();
            ScrCharTypeDisp = new List<List<int>>();
            ScrCharStrDisp = new List<List<int>>();
            ScrCharColDisp = new List<List<int>>();
            ScrCharFonDisp = new List<List<int>>();
            TextBuffer.Clear();
            TextColBuf.Clear();
            FileREnc = CF.ParamGetS("FileReadEncoding");
            FileWEnc = CF.ParamGetS("FileWriteEncoding");
            FileReadChars = CF.ParamGetI("FileReadChars");
            UseAnsiLoad = CF.ParamGetB("ANSIRead");
            UseAnsiSave = CF.ParamGetB("ANSIWrite");
            AnsiMaxX = CF.ParamGetI("ANSIWidth");
            AnsiMaxY = CF.ParamGetI("ANSIHeight");
            __AnsiLineOccupy1_Use = CF.ParamGetB("ANSIBufferAbove");
            __AnsiLineOccupy2_Use = CF.ParamGetB("ANSIBufferBelow");
            ANSI_CR = CF.ParamGetI("ANSIReadCR");
            ANSI_LF = CF.ParamGetI("ANSIReadLF");
            AnsiColorBackBlink = CF.ParamGetB("ANSIWriteBlink");
            AnsiColorForeBold = CF.ParamGetB("ANSIWriteBold");
            TextBeyondLineMargin = CF.ParamGetI("BeyondLineMargin");

            ANSIDOS = CF.ParamGetB("ANSIDOS");
            ANSIPrintBackspace = CF.ParamGetB("ANSIPrintBackspace");

            AnsiTerminalResize(AnsiMaxX, AnsiMaxY);

            ANSIScrollChars = CF.ParamGetI("ANSIScrollChars");
            ANSIScrollBuffer = CF.ParamGetI("ANSIScrollBuffer");
            ANSIScrollSmooth = CF.ParamGetI("ANSIScrollSmooth");

            if (WorkMode == 2)
            {
                if (CF.ParamGetL("TerminalTimeResolution") <= 0)
                {
                    ANSIScrollChars = 0;
                    ANSIScrollBuffer = 0;
                    ANSIScrollSmooth = 0;
                }
                if (CF.ParamGetI("TerminalStep") <= 0)
                {
                    ANSIScrollChars = 0;
                    ANSIScrollBuffer = 0;
                    ANSIScrollSmooth = 0;
                }
            }

            if (ANSIScrollChars <= 0)
            {
                ANSIScrollSmooth = 0;
            }
            if (ANSIScrollSmooth > 4)
            {
                ANSIScrollSmooth = 0;
            }

            ReadColor(CF.ParamGetS("ColorNormal"), ref TextNormalBack, ref TextNormalFore);
            ReadColor(CF.ParamGetS("ColorBeyondLine"), ref TextBeyondLineBack, ref TextBeyondLineFore);
            ReadColor(CF.ParamGetS("ColorBeyondEnd"), ref TextBeyondEndBack, ref TextBeyondEndFore);
            ReadColor(CF.ParamGetS("ColorCursor"), ref CursorBack, ref CursorFore);
            ReadColor(CF.ParamGetS("ColorStatus"), ref StatusBack, ref StatusFore);
            ReadColor(CF.ParamGetS("ColorPopup"), ref PopupBack, ref PopupFore);

            ANSIIgnoreBlink = CF.ParamGetB("ANSIIgnoreBlink");
            ANSIIgnoreBold = CF.ParamGetB("ANSIIgnoreBold");
            ANSIIgnoreConcealed = CF.ParamGetB("ANSIIgnoreConcealed");
            ANSIReverseMode = CF.ParamGetI("ANSIReverseMode");
            UseWindow = (CF.ParamGetI("WinUse") > 0);
            CursorDisplay = UseWindow ? true : CF.ParamGetB("ConCursorDisplay");
            bool ColorBlending = CF.ParamGetB("WinColorBlending");
            List<string> ColorBlendingConfig = new List<string>();
            int Idx = 1;
            while ((CF.ParamExists("WinColorBlending_" + Idx.ToString())) && (!("".Equals(CF.ParamGetS("WinColorBlending_" + Idx.ToString())))))
            {
                ColorBlendingConfig.Add(CF.ParamGetS("WinColorBlending_" + Idx.ToString()));
                Idx++;
            }

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
                    Screen_ = new ScreenWindowAvalonia(this, CF.ParamGetI("WinFixed"), CF, WinW__, WinH__, ColorBlending, ColorBlendingConfig, false);
                    ((ScreenWindow)Screen_).SteadyCursor = CF.ParamGetB("WinSteadyCursor");
                }
                else
                {
                    Screen_ = new ScreenConsole(this, CF.ParamGetI("WinFixed"), CF, TextNormalBack, TextNormalFore);
                    Screen_.UseMemo = CF.ParamGetI("ConUseMemo");
                }
                Screen_.BellMethod = CF.ParamGetI("Bell");
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
                EncodingInfo.Clear();
                EncodingCodePage.Clear();
                EncodingFile.Clear();
                EncodingListI = 0;
                EncodingListL = 24;
                EncodingKey1.Clear();
                EncodingKey1.Add('`');
                EncodingKey1.Add('~');
                EncodingKey1.Add(27);
                EncodingKey1.Add(9);
                EncodingKey2.Clear();
                EncodingKey2.Add(8);
                EncodingKey2.Add(13);
                EncodingKey2.Add(10);
                EncodingKey2.Add(32);

                foreach (EncodingInfo ei in Encoding.GetEncodings())
                {
                    Encoding e = ei.GetEncoding();
                    string EncName = e.CodePage.ToString().PadLeft(5) + ": " + TextWork.EncodingGetName(e) + "  ";
                    EncodingList.Add(EncName);
                    EncodingInfo.Add(TextWork.StrToInt(e.CodePage.ToString().PadLeft(5) + ": "));
                    EncodingCodePage.Add(e.CodePage);
                    if (EncodingListL < EncName.Length)
                    {
                        EncodingListL = EncName.Length;
                    }
                }
                EncodingList.Insert(0, ("Items: " + EncodingList.Count).PadRight(EncodingListL));
                EncodingInfo.Insert(0, null);
                EncodingCodePage.Insert(0, -1);
                EncodingList.Add("".PadRight(EncodingListL));
                EncodingInfo.Add(null);
                EncodingCodePage.Add(-1);
                EncodingAddParam("TerminalEncoding", "Terminal", CF);
                EncodingAddParam("ServerEncoding", "Server", CF);
                EncodingAddParam("FileWriteEncoding", "File write", CF);
                EncodingAddParam("FileReadEncoding", "File read", CF);
                EncodingAddParam("ConOutputEncoding", "Console O", CF);
                EncodingAddParam("ConInputEncoding", "Console I", CF);

                for (int i = 0; i < EncodingList.Count; i++)
                {
                    EncodingList[i] = EncodingList[i].PadRight(EncodingListL);
                }
            }
            KeyCounter = 0;
            KeyCounterLast = "";
            if (WorkMode != 4)
            {
                Screen_.StartApp();
            }
            else
            {
                Screen_ = new ScreenWindowAvalonia(this, 0, CF, 1, 1, ColorBlending, ColorBlendingConfig, true);
                RenderStart(CF.ParamGetS("RenderFile"), CF.ParamGetI("RenderStep"), CF.ParamGetI("RenderOffset"), CF.ParamGetI("RenderFrame"), CF.ParamGetB("RenderCursor"), CF.ParamGetS("RenderType"));
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
                    if (Force || (ScrCharStrDisp[Y][X] != ScrCharStr[Y][X]) || (ScrCharColDisp[Y][X] != ScrCharCol[Y][X]) || (ScrCharFonDisp[Y][X] != ScrCharFon[Y][X]) || (ScrCharTypeDisp[Y][X] != ScrCharType[Y][X]))
                    {
                        int ColorB = 0;
                        int ColorF = 0;
                        int FontW = 0;
                        int FontH = 0;
                        ColorFromInt(ScrCharCol[Y][X], out ColorB, out ColorF);
                        FontSFromInt(ScrCharFon[Y][X], out FontW, out FontH);
                        switch (ScrCharType[Y][X])
                        {
                            case 0:
                                if (ColorB < 0) { ColorB = TextNormalBack; }
                                if (ColorF < 0) { ColorF = TextNormalFore; }
                                break;
                            case 1:
                                if (((X + DisplayX) < TextBeyondLineMargin) || ((TextBeyondLineMargin < 0) && ((X + DisplayX) < AnsiMaxX)))
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
                        ScrCharFonDisp[Y][X] = ScrCharFon[Y][X];
                        Screen_.PutChar(X, Y, ScrCharStr[Y][X], ColorB, ColorF, FontW, FontH);
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
                    ScrCharFon.Clear();
                    ScrCharTypeDisp.Clear();
                    ScrCharStrDisp.Clear();
                    ScrCharColDisp.Clear();
                    ScrCharFonDisp.Clear();
                    for (int i = 0; i < WinTxtH; i++)
                    {
                        ScrCharType.Add(new List<int>());
                        ScrCharStr.Add(new List<int>());
                        ScrCharCol.Add(new List<int>());
                        ScrCharFon.Add(new List<int>());
                        ScrCharTypeDisp.Add(BlankDispLineT());
                        ScrCharStrDisp.Add(BlankDispLineT());
                        ScrCharColDisp.Add(BlankDispLineC());
                        ScrCharFonDisp.Add(BlankDispLineC());
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
                        if (WorkMode == 1)
                        {
                            Telnet_.ForceRepaint = true;
                        }
                        if (WorkMode == 2)
                        {
                            Telnet_.TelnetRepaint();
                        }
                    }
                }
                WinW = Screen_.WinW;
                WinH = Screen_.WinH;
            }
            if (WorkMode == 3)
            {
                if (Screen_.WindowResize())
                {
                    WinW = Screen_.WinW;
                    WinH = Screen_.WinH;
                    Screen_.Clear(TextNormalBack, TextNormalFore);
                }
            }
        }

        string BeyondIndicator()
        {
            if ((CursorY + CursorFontH - 1) < TextBuffer.Count)
            {
                for (int i = 0; i < CursorFontH; i++)
                {
                    if ((CursorX + CursorFontW - 1) >= TextBuffer[CursorY + i].Count)
                    {
                        return ";";
                    }
                }
                return ":";
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
                if (TextCipher_.PasswordState == 0)
                {
                    StringBuilder StatusText = new StringBuilder();
                    Semigraphics_.CursorChar = CharGet(CursorX, CursorY, true, false);
                    Semigraphics_.CursorColo = ColoGet(CursorX, CursorY, true, false);
                    if (WorkState == WorkStateDef.DrawPixel)
                    {
                        StatusText.Append(PixelPaint_.PPS.FontW.ToString());
                        StatusText.Append("x");
                        StatusText.Append(PixelPaint_.PPS.FontH.ToString());
                        StatusText.Append(" ");

                        StatusText.Append(PixelPaint_.PPS.CanvasX.ToString());
                        if (PixelPaint_.PPS.SizeX >= 0)
                        {
                            StatusText.Append("+");
                        }
                        else
                        {
                            StatusText.Append("-");
                        }
                        StatusText.Append(Math.Abs(PixelPaint_.PPS.SizeX));
                        StatusText.Append(BeyondIndicator());
                        StatusText.Append(PixelPaint_.PPS.CanvasY.ToString());
                        if (PixelPaint_.PPS.SizeY >= 0)
                        {
                            StatusText.Append("+");
                        }
                        else
                        {
                            StatusText.Append("-");
                        }
                        StatusText.Append(Math.Abs(PixelPaint_.PPS.SizeY));
                    }
                    else
                    {
                        StatusText.Append(CursorFontW.ToString());
                        StatusText.Append("x");
                        StatusText.Append(CursorFontH.ToString());
                        StatusText.Append(" ");

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
                            case 4: StatusText.Append(" H-roll"); break;
                            case 5: StatusText.Append(" V-roll"); break;
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
                else
                {
                    TextCipher_.PasswordStateDisp();
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

        string SubstituteKey = "";
        int SubstituteState = 0;
        Dictionary<string, string> SubstituteMap = new Dictionary<string, string>();

        public void CoreEvent(string KeyName, char KeyChar, bool ModShift, bool ModCtrl, bool ModAlt)
        {
            if ((KeyName == SubstituteKey) || (((int)KeyChar).ToString() == SubstituteKey))
            {
                if ((!ModShift) && (!ModCtrl) && (!ModAlt))
                {
                    switch (SubstituteState)
                    {
                        case 0:
                            SubstituteState = 1;
                            break;
                        case 1:
                            SubstituteState = 0;
                            CoreEvent_(KeyName, KeyChar, ModShift, ModCtrl, ModAlt);
                            break;
                        case 2:
                            SubstituteState = 3;
                            break;
                        case 3:
                            SubstituteState = 2;
                            CoreEvent_(KeyName, KeyChar, ModShift, ModCtrl, ModAlt);
                            break;
                    }
                    return;
                }
            }
            bool UseSubstitute = false;
            switch (SubstituteState)
            {
                case 1:
                    SubstituteState = 2;
                    UseSubstitute = true;
                    break;
                case 2:
                    UseSubstitute = true;
                    break;
                case 3:
                    SubstituteState = 0;
                    break;
            }
            if (UseSubstitute)
            {
                string KeyName0 = KeyName;
                int KeyNameI = KeyName0.IndexOf("\"->\"");
                if (KeyNameI >= 0)
                {
                    KeyName0 = KeyName0.Substring(KeyNameI + 4);
                }
                if (!SubstituteMap.ContainsKey(KeyName0))
                {
                    KeyName0 = ((int)KeyChar).ToString();
                }
                if (SubstituteMap.ContainsKey(KeyName0))
                {
                    KeyName = SubstituteMap[KeyName0];
                    KeyChar = (char)0;
                }
            }
            CoreEvent_(KeyName, KeyChar, ModShift, ModCtrl, ModAlt);
        }

        public void CoreEvent_(string KeyName, char KeyChar, bool ModShift, bool ModCtrl, bool ModAlt)
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

                    if (!("Resize".Equals(KeyName)))
                    {
                        if ((KeyCounter >= 3) || ("WindowClose".Equals(KeyName)))
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

                            Screen_.Move(0, 1, 0, 0, WinW, WinH - 1);
                            if (EncodingByte < 0)
                            {
                                if (EncodingKey2.Contains(KeyChar))
                                {
                                    if (EncodingListI > 0)
                                    {
                                        if (EncodingInfo[EncodingListI - 1] != null)
                                        {
                                            if (EncodingCodePage[EncodingListI - 1] < 0)
                                            {
                                                EncodingArray = EncodingFile[EncodingListI - 1];
                                                EncodingListI--;
                                                EncodingByte = 0;
                                            }
                                            else
                                            {
                                                OneByteEncoding OBE = new OneByteEncoding();
                                                if (OBE.DefImport(TextWork.EncodingFromName(EncodingCodePage[EncodingListI - 1].ToString())))
                                                {
                                                    EncodingArray = OBE.DefExport();
                                                    EncodingListI--;
                                                    EncodingByte = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (EncodingKey1.Contains(KeyChar))
                                {
                                    EncodingListI -= 2;
                                    if (EncodingListI < 0)
                                    {
                                        EncodingListI += EncodingList.Count;
                                    }
                                }
                            }
                            else
                            {
                                if (EncodingKey2.Contains(KeyChar))
                                {
                                    EncodingByte += 15;
                                    if (EncodingByte >= (256 + 16))
                                    {
                                        EncodingByte -= (256 + 16);
                                    }
                                }
                                if (EncodingKey1.Contains(KeyChar))
                                {
                                    EncodingByte = -1;
                                }
                            }
                            if (EncodingByte < 0)
                            {
                                KeyInfoText.AddRange(TextWork.StrToInt(EncodingList[EncodingListI]));
                            }
                            else
                            {
                                KeyInfoText.AddRange(EncodingInfo[EncodingListI]);
                                if (EncodingByte < 16)
                                {
                                    for (int i = 0; i < 16; i++)
                                    {
                                        int ii = EncodingByte * 16 + i;
                                        KeyInfoText.Add(EncodingArray[ii]);
                                    }
                                }
                                else
                                {
                                    KeyInfoText.Add(((EncodingByte - 16) / 16).ToString("X")[0]);
                                    KeyInfoText.Add(((EncodingByte - 16) % 16).ToString("X")[0]);
                                    KeyInfoText.Add('=');
                                    KeyInfoText.AddRange(TextWork.StrToInt(TextWork.CharCode(EncodingArray[(EncodingByte - 16)], 0)));
                                    KeyInfoText.Add(32);
                                    KeyInfoText.Add(39);
                                    KeyInfoText.Add(EncodingArray[(EncodingByte - 16)]);
                                    KeyInfoText.Add(39);
                                }
                                while (KeyInfoText.Count < EncodingListL)
                                {
                                    KeyInfoText.Add(32);
                                }
                            }
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
                            if (EncodingByte < 0)
                            {
                                EncodingListI++;
                                if (EncodingListI == EncodingList.Count)
                                {
                                    EncodingListI = 0;
                                }
                            }
                            else
                            {
                                EncodingByte++;
                                if (EncodingByte >= (256 + 16))
                                {
                                    EncodingByte -= (256 + 16);
                                }
                            }
                            while (KeyInfoText.Count < WinW)
                            {
                                KeyInfoText.Add(32);
                            }
                            for (int i = 0; i < WinW; i++)
                            {
                                Screen_.PutChar(i, WinH - 1, KeyInfoText[i], TextNormalBack, TextNormalFore, 0, 0);
                            }

                            Screen_.SetCursorPosition(0, WinH - 1);
                        }
                    }
                }
                return;
            }

            if (InfoScreen_.ScreenKey(KeyName, KeyChar))
            {
                if ((InfoScreen_.RequestHide) || (InfoScreen_.RequestClose))
                {
                    if (InfoScreen_.RequestHide)
                    {
                        InfoScreen_.ScreenHide();
                        ScreenRefresh(true);
                    }
                    if (InfoScreen_.RequestClose)
                    {
                        InfoScreen_.ScreenHide();
                        Screen_.CloseApp(TextNormalBack, TextNormalFore);
                    }
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
                                ScrCharFon.Insert(0, new List<int>());
                                ScrCharStr.RemoveAt(WinTxtH);
                                ScrCharCol.RemoveAt(WinTxtH);
                                ScrCharFon.RemoveAt(WinTxtH);
                                ScrCharTypeDisp.Insert(0, BlankDispLineT());
                                ScrCharTypeDisp.RemoveAt(WinTxtH);
                                ScrCharStrDisp.Insert(0, BlankDispLineT());
                                ScrCharColDisp.Insert(0, BlankDispLineC());
                                ScrCharFonDisp.Insert(0, BlankDispLineC());
                                ScrCharStrDisp.RemoveAt(WinTxtH);
                                ScrCharColDisp.RemoveAt(WinTxtH);
                                ScrCharFonDisp.RemoveAt(WinTxtH);
                                TextDisplay(1);
                                break;
                            case 3:
                                Screen_.Move(0, 1, 0, 0, WinTxtW, WinTxtH - 1);
                                ScrCharType.RemoveAt(0);
                                ScrCharType.Add(new List<int>());
                                ScrCharStr.RemoveAt(0);
                                ScrCharCol.RemoveAt(0);
                                ScrCharFon.RemoveAt(0);
                                ScrCharStr.Add(new List<int>());
                                ScrCharCol.Add(new List<int>());
                                ScrCharFon.Add(new List<int>());
                                ScrCharTypeDisp.RemoveAt(0);
                                ScrCharTypeDisp.Add(BlankDispLineT());
                                ScrCharStrDisp.RemoveAt(0);
                                ScrCharColDisp.RemoveAt(0);
                                ScrCharFonDisp.RemoveAt(0);
                                ScrCharStrDisp.Add(BlankDispLineT());
                                ScrCharColDisp.Add(BlankDispLineC());
                                ScrCharFonDisp.Add(BlankDispLineC());
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
                                    ScrCharFon[i].Insert(0, 0);
                                    ScrCharStr[i].RemoveAt(WinTxtW);
                                    ScrCharCol[i].RemoveAt(WinTxtW);
                                    ScrCharFon[i].RemoveAt(WinTxtW);
                                    ScrCharTypeDisp[i].Insert(0, TextWork.SpaceChar0);
                                    ScrCharTypeDisp[i].RemoveAt(WinTxtW);
                                    ScrCharStrDisp[i].Insert(0, TextWork.SpaceChar0);
                                    ScrCharColDisp[i].Insert(0, 0);
                                    ScrCharFonDisp[i].Insert(0, 0);
                                    ScrCharStrDisp[i].RemoveAt(WinTxtW);
                                    ScrCharColDisp[i].RemoveAt(WinTxtW);
                                    ScrCharFonDisp[i].RemoveAt(WinTxtW);
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
                                    ScrCharFon[i].RemoveAt(0);
                                    ScrCharStr[i].Add(TextWork.SpaceChar0);
                                    ScrCharCol[i].Add(0);
                                    ScrCharFon[i].Add(0);
                                    ScrCharTypeDisp[i].Add(TextWork.SpaceChar0);
                                    ScrCharTypeDisp[i].RemoveAt(0);
                                    ScrCharStrDisp[i].Add(TextWork.SpaceChar0);
                                    ScrCharColDisp[i].Add(0);
                                    ScrCharFonDisp[i].Add(0);
                                    ScrCharStrDisp[i].RemoveAt(0);
                                    ScrCharColDisp[i].RemoveAt(0);
                                    ScrCharFonDisp[i].RemoveAt(0);
                                }
                                TextDisplay(4);
                                break;
                        }
                        TextRepaint(false);
                        Screen_.SetCursorPosition(WinW - 1, WinH - 1);
                    }
                }
                ScreenRefresh(false);
                return;
            }

            if (TextCipher_.PasswordState != 0)
            {
                TextCipher_.PasswordEvent(KeyName, KeyChar);
                ScreenRefresh(false);
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
                    return;
                case "FileDrop2":
                    return;
                case "F9":
                    PixelCharSet();
                    Semigraphics_.AnsiMaxX_ = AnsiMaxX;
                    Semigraphics_.AnsiMaxY_ = AnsiMaxY;
                    Semigraphics_.ANSI_CR_ = ANSI_CR;
                    Semigraphics_.ANSI_LF_ = ANSI_LF;
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
                                    if (TextInsDelMode == 6)
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
                                            CharPut(CursorX, CursorY, Semigraphics_.FavChar[KeyChar], Semigraphics_.DrawColoI, 0, false);
                                        }
                                    }
                                    else
                                    {
                                        CharPut(CursorX, CursorY, KeyChar, Semigraphics_.DrawColoI, 0, false);
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
                                                Semigraphics_.FrameCharPut(0, CursorFontW, CursorFontH);
                                            }
                                            break;
                                        case 3:
                                            {
                                                Semigraphics_.FrameCharPut(6, CursorFontW, CursorFontH);
                                            }
                                            break;
                                        case 1:
                                            {
                                                Semigraphics_.FrameCharPut(4, CursorFontW, CursorFontH);
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
                                                Semigraphics_.FrameCharPut(1, CursorFontW, CursorFontH);
                                            }
                                            break;
                                        case 2:
                                            {
                                                Semigraphics_.FrameCharPut(5, CursorFontW, CursorFontH);
                                            }
                                            break;
                                        case 4:
                                            {
                                                Semigraphics_.FrameCharPut(7, CursorFontW, CursorFontH);
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
                                                Semigraphics_.FrameCharPut(2, CursorFontW, CursorFontH);
                                            }
                                            break;
                                        case 2:
                                            {
                                                Semigraphics_.FrameCharPut(5, CursorFontW, CursorFontH);
                                            }
                                            break;
                                        case 3:
                                            {
                                                Semigraphics_.FrameCharPut(6, CursorFontW, CursorFontH);
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
                                                Semigraphics_.FrameCharPut(3, CursorFontW, CursorFontH);
                                            }
                                            break;
                                        case 1:
                                            {
                                                Semigraphics_.FrameCharPut(4, CursorFontW, CursorFontH);
                                            }
                                            break;
                                        case 4:
                                            {
                                                Semigraphics_.FrameCharPut(7, CursorFontW, CursorFontH);
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
                                    Semigraphics_.FrameCharPut(4, CursorFontW, CursorFontH);
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
                                    Semigraphics_.FrameCharPut(5, CursorFontW, CursorFontH);
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
                                    Semigraphics_.FrameCharPut(6, CursorFontW, CursorFontH);
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
                                    Semigraphics_.FrameCharPut(7, CursorFontW, CursorFontH);
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
                                    Semigraphics_.RectangleDraw(0, 0, CursorXSize, CursorYSize, 1, CursorFontW, CursorFontH);
                                }
                                else
                                {
                                    Semigraphics_.DiamondDraw(0, 0, CursorXSize, CursorYSize, 1, -1, CursorFontW, CursorFontH);
                                }
                                UndoBufferStop();
                                break;
                            case "D4": // Filled
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    Semigraphics_.RectangleDraw(0, 0, CursorXSize, CursorYSize, 2, CursorFontW, CursorFontH);
                                }
                                else
                                {
                                    Semigraphics_.DiamondDraw(0, 0, CursorXSize, CursorYSize, 2, -1, CursorFontW, CursorFontH);
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
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[2], Semigraphics_.DrawColoI, -1, false);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[13], Semigraphics_.DrawColoI, -1, false);
                                }
                                UndoBufferStop();
                                break;
                            case "Y":
                            case "NumPad8":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[3], Semigraphics_.DrawColoI, -1, false);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[14], Semigraphics_.DrawColoI, -1, false);
                                }
                                UndoBufferStop();
                                break;
                            case "U":
                            case "NumPad9":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[4], Semigraphics_.DrawColoI, -1, false);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[15], Semigraphics_.DrawColoI, -1, false);
                                }
                                UndoBufferStop();
                                break;
                            case "G":
                            case "NumPad4":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[5], Semigraphics_.DrawColoI, -1, false);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[16], Semigraphics_.DrawColoI, -1, false);
                                }
                                UndoBufferStop();
                                break;
                            case "H":
                            case "NumPad5":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[6], Semigraphics_.DrawColoI, -1, false);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[17], Semigraphics_.DrawColoI, -1, false);
                                }
                                UndoBufferStop();
                                break;
                            case "J":
                            case "NumPad6":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[7], Semigraphics_.DrawColoI, -1, false);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[18], Semigraphics_.DrawColoI, -1, false);
                                }
                                UndoBufferStop();
                                break;
                            case "B":
                            case "NumPad1":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[8], Semigraphics_.DrawColoI, -1, false);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[19], Semigraphics_.DrawColoI, -1, false);
                                }
                                UndoBufferStop();
                                break;
                            case "N":
                            case "NumPad2":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[9], Semigraphics_.DrawColoI, -1, false);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[20], Semigraphics_.DrawColoI, -1, false);
                                }
                                UndoBufferStop();
                                break;
                            case "M":
                            case "NumPad3":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[10], Semigraphics_.DrawColoI, -1, false);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[21], Semigraphics_.DrawColoI, -1, false);
                                }
                                UndoBufferStop();
                                break;

                            case "I":
                            case "Add":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[1], Semigraphics_.DrawColoI, -1, false);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[12], Semigraphics_.DrawColoI, -1, false);
                                }
                                UndoBufferStop();
                                break;
                            case "K":
                            case "Subtract":
                                UndoBufferStart();
                                if (Semigraphics_.DiamondType == 0)
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[0], Semigraphics_.DrawColoI, -1, false);
                                }
                                else
                                {
                                    CharPut(CursorX, CursorY, Semigraphics_.FrameChar[11], Semigraphics_.DrawColoI, -1, false);
                                }
                                UndoBufferStop();
                                break;
                            case "Space":
                            case "NumPad0":
                                UndoBufferStart();
                                CharPut(CursorX, CursorY, Semigraphics_.DrawCharI, Semigraphics_.DrawColoI, -1, false);
                                UndoBufferStop();
                                break;

                            case "Enter":
                            case "Return":
                                TextInsDelMode++;
                                if (TextInsDelMode == 6)
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
                                Clipboard_.TextClipboardWork(CursorX, CursorY, CursorXSize, CursorYSize, CursorFontW, CursorFontH, false);
                                break;
                            case "V":
                                Clipboard_.TextClipboardWork(CursorX, CursorY, CursorXSize, CursorYSize, CursorFontW, CursorFontH, true);
                                break;


                            case "D6":
                                if (CursorFontW > 1)
                                {
                                    CursorFontW--;
                                }
                                break;
                            case "D7":
                                if (CursorFontW < FontMaxSize)
                                {
                                    CursorFontW++;
                                }
                                break;
                            case "D8":
                                if (CursorFontH > 1)
                                {
                                    CursorFontH--;
                                }
                                break;
                            case "D9":
                                if (CursorFontH < FontMaxSize)
                                {
                                    CursorFontH++;
                                }
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
                                if (PixelPaint_.PPS.PaintPencil)
                                {
                                    UndoBufferStart();
                                }
                                switch (KeyName)
                                {
                                    case "UpArrow":
                                        PixelPaint_.MoveCursor(0);
                                        break;
                                    case "DownArrow":
                                        PixelPaint_.MoveCursor(1);
                                        break;
                                    case "LeftArrow":
                                        PixelPaint_.MoveCursor(2);
                                        break;
                                    case "RightArrow":
                                        PixelPaint_.MoveCursor(3);
                                        break;
                                    case "PageUp":
                                        PixelPaint_.MoveCursor(4);
                                        break;
                                    case "End":
                                        PixelPaint_.MoveCursor(5);
                                        break;
                                    case "Home":
                                        PixelPaint_.MoveCursor(6);
                                        break;
                                    case "PageDown":
                                        PixelPaint_.MoveCursor(7);
                                        break;
                                }
                                CursorX = PixelPaint_.GetCursorPosX();
                                CursorY = PixelPaint_.GetCursorPosY();
                                CursorLimit();
                                if (PixelPaint_.PPS.PaintPencil)
                                {
                                    PixelPaint_.Paint();
                                    UndoBufferStop();
                                }
                                break;

                            case "Q":
                                PixelPaint_.SwapCursors(-1);
                                CursorX = PixelPaint_.GetCursorPosX();
                                CursorY = PixelPaint_.GetCursorPosY();
                                CursorLimit();
                                break;
                            case "E":
                                PixelPaint_.SwapCursors(1);
                                CursorX = PixelPaint_.GetCursorPosX();
                                CursorY = PixelPaint_.GetCursorPosY();
                                CursorLimit();
                                break;

                            case "R":
                                PixelPaint_.PPS.PaintColor++;
                                if (PixelPaint_.PPS.PaintColor == 3)
                                {
                                    PixelPaint_.PPS.PaintColor = 0;
                                }
                                if (PixelPaint_.PPS.PaintColor == 6)
                                {
                                    PixelPaint_.PPS.PaintColor = 3;
                                }
                                break;
                            case "F":
                                if (PixelPaint_.PPS.PaintColor < 3)
                                {
                                    PixelPaint_.PPS.PaintColor += 3;
                                }
                                else
                                {
                                    PixelPaint_.PPS.PaintColor -= 3;
                                }
                                break;

                            case "N":
                                UndoBufferStart();
                                PixelPaint_.PaintInvert();
                                UndoBufferStop();
                                break;

                            case "W":
                                PixelPaint_.PPS.SizeY--;
                                break;
                            case "S":
                                PixelPaint_.PPS.SizeY++;
                                break;
                            case "A":
                                PixelPaint_.PPS.SizeX--;
                                break;
                            case "D":
                                PixelPaint_.PPS.SizeX++;
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
                                PixelPaint_.ClipboardPaste();
                                break;

                            case "D1":
                                PixelPaint_.PPS.PaintModeN++;
                                if (PixelPaint_.PPS.PaintModeN >= PixelPaint_.PaintModeCount)
                                {
                                    PixelPaint_.PPS.PaintModeN = 0;
                                }
                                PixelPaint_.SelectPaintMode();
                                break;
                            case "D2":
                                PixelPaint_.PPS.DefaultColor = !PixelPaint_.PPS.DefaultColor;
                                break;
                            case "D3":
                                UndoBufferStart();
                                if (PixelPaint_.PPS.PaintColor < 3)
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
                                PixelPaint_.PPS.PaintPencil = !PixelPaint_.PPS.PaintPencil;
                                if (PixelPaint_.PPS.PaintPencil)
                                {
                                    UndoBufferStart();
                                    PixelPaint_.Paint();
                                    UndoBufferStop();
                                }
                                break;
                            case "M":
                                PixelPaint_.PPS.PaintMoveRoll++;
                                if (PixelPaint_.PPS.PaintMoveRoll == 5)
                                {
                                    PixelPaint_.PPS.PaintMoveRoll = 0;
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
                if (!("".Equals(TextCipher_.CipherConfPassword)))
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

        public static string PrepareFileNameStr(string NewFile_)
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
            while ((TempX >= 0) && (TempY >= 0) && ((CharGet(TempX, TempY, false, true) >= 0) || (DeltaMargin > 0)))
            {
                TempX -= DeltaX;
                TempY -= DeltaY;
                DeltaMargin--;
            }
            while ((TempX != CursorX) || (TempY != CursorY))
            {
                NewFile_.Add(CharGet(TempX, TempY, true, true));
                TempX += DeltaX;
                TempY += DeltaY;
            }


            string NewFile = PrepareFileName(NewFile_);

            if (!("".Equals(NewFile)))
            {
                CurrentFileName = NewFile;
                CursorX = 0;
                CursorY = 0;
                DisplayX = 0;
                DisplayY = 0;
            }

            if (TextCipher_.CipherEnabled)
            {
                if ("".Equals(TextCipher_.CipherConfPassword))
                {
                    TextCipher_.PasswordInput(2);
                }
                else
                {
                    TextCipher_.SetPassword(TextCipher_.CipherConfPassword);
                    FileLoad0_();
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

            int W_ = (W + 1) * CursorFontW;
            int H_ = (H + 1) * CursorFontH;

            switch (InsDelMode)
            {
                case 0:
                case 10:
                    for (int i = Y; i < (Y + H_); i++)
                    {
                        if (TextBuffer.Count > i)
                        {
                            if ((TextWork.TrimEndLength(TextBuffer[i]) > X) || (TextWork.TrimEndLenCol(TextColBuf[i]) > X) || (TextWork.TrimEndLenCol(TextFonBuf[i]) > X))
                            {
                                if (ToggleDrawText)
                                {
                                    TextBuffer[i].InsertRange(X, TextWork.Spaces(W_));
                                    TextFonBuf[i].InsertRange(X, TextWork.BlkCol(W_));
                                }
                                else
                                {
                                    TextBuffer[i].AddRange(TextWork.Spaces(W_));
                                    TextFonBuf[i].AddRange(TextWork.BlkCol(W_));
                                }
                                if (ToggleDrawColo)
                                {
                                    TextColBuf[i].InsertRange(X, TextWork.BlkCol(W_));
                                }
                                else
                                {
                                    TextColBuf[i].AddRange(TextWork.BlkCol(W_));
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
                            if (TextBuffer[i].Count < (X + W_ + 1))
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces((X + W_ + 1) - TextBuffer[i].Count));
                                TextColBuf[i].AddRange(TextWork.BlkCol((X + W_ + 1) - TextColBuf[i].Count));
                                TextFonBuf[i].AddRange(TextWork.BlkCol((X + W_ + 1) - TextFonBuf[i].Count));
                            }
                        }
                        for (int i = 0; i < H_; i++)
                        {
                            TextBuffer.Add(TextWork.Spaces(X + W_ + 1));
                            TextColBuf.Add(TextWork.BlkCol(X + W_ + 1));
                            TextFonBuf.Add(TextWork.BlkCol(X + W_ + 1));
                        }
                        for (int i = (TextBuffer.Count - H_ - 0); i > Y; i--)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer[i + H_ - 1].RemoveRange(X, W_);
                                TextBuffer[i + H_ - 1].InsertRange(X, TextBuffer[i - 1].GetRange(X, W_));
                                TextFonBuf[i + H_ - 1].RemoveRange(X, W_);
                                TextFonBuf[i + H_ - 1].InsertRange(X, TextFonBuf[i - 1].GetRange(X, W_));
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[i + H_ - 1].RemoveRange(X, W_);
                                TextColBuf[i + H_ - 1].InsertRange(X, TextColBuf[i - 1].GetRange(X, W_));
                            }
                            TextBufferTrimLine(i + H_ - 1);
                        }
                        for (int i = Y; i < (Y + H_); i++)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer[i].RemoveRange(X, W_);
                                TextBuffer[i].InsertRange(X, TextWork.Spaces(W_));
                                TextFonBuf[i].RemoveRange(X, W_);
                                TextFonBuf[i].InsertRange(X, TextWork.BlkCol(W_));
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[i].RemoveRange(X, W_);
                                TextColBuf[i].InsertRange(X, TextWork.BlkCol(W_));
                            }
                            TextBufferTrimLine(i);
                        }
                    }
                    break;
                case 2:
                case 12:
                    for (int i = 0; i < TextBuffer.Count; i++)
                    {
                        if ((TextWork.TrimEndLength(TextBuffer[i]) > X) || (TextWork.TrimEndLenCol(TextColBuf[i]) > X) || (TextWork.TrimEndLenCol(TextFonBuf[i]) > X))
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer[i].InsertRange(X, TextWork.Spaces(W_));
                                TextFonBuf[i].InsertRange(X, TextWork.BlkCol(W_));
                            }
                            else
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces(W_));
                                TextFonBuf[i].AddRange(TextWork.BlkCol(W_));
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[i].InsertRange(X, TextWork.BlkCol(W_));
                            }
                            else
                            {
                                TextColBuf[i].AddRange(TextWork.BlkCol(W_));
                            }
                        }
                    }
                    break;
                case 3:
                case 13:
                    if (Y < TextBuffer.Count)
                    {
                        for (int i = 0; i < H_; i++)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer.Insert(Y, new List<int>());
                                TextFonBuf.Insert(Y, new List<int>());
                            }
                            else
                            {
                                TextBuffer.Add(new List<int>());
                                TextFonBuf.Add(new List<int>());
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
                                TextFonBuf[i].AddRange(TextWork.BlkCol(TextColBuf[i].Count - TextBuffer[i].Count));
                                TextBuffer[i].AddRange(TextWork.Spaces(TextColBuf[i].Count - TextBuffer[i].Count));
                            }
                            if (TextColBuf[i].Count < TextBuffer[i].Count)
                            {
                                TextColBuf[i].AddRange(TextWork.BlkCol(TextBuffer[i].Count - TextColBuf[i].Count));
                            }
                        }
                    }
                    break;
                case 4:
                case 14:
                    for (int i = Y; i < (Y + H_); i++)
                    {
                        if (TextBuffer.Count > i)
                        {
                            if ((TextWork.TrimEndLength(TextBuffer[i]) > X) || (TextWork.TrimEndLenCol(TextColBuf[i]) > X) || (TextWork.TrimEndLenCol(TextFonBuf[i]) > X))
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces(W_));
                                TextFonBuf[i].AddRange(TextWork.BlkCol(W_));
                                TextColBuf[i].AddRange(TextWork.BlkCol(W_));
                                List<int> Temp;

                                if (ToggleDrawText)
                                {
                                    Temp = TextBuffer[i].GetRange(X + W_ - CursorFontW, CursorFontW);
                                    TextBuffer[i].RemoveRange(X + W_ - CursorFontW, CursorFontW);
                                    TextBuffer[i].InsertRange(X, Temp);

                                    Temp = TextFonBuf[i].GetRange(X + W_ - CursorFontW, CursorFontW);
                                    TextFonBuf[i].RemoveRange(X + W_ - CursorFontW, CursorFontW);
                                    TextFonBuf[i].InsertRange(X, Temp);
                                }
                                if (ToggleDrawColo)
                                {
                                    Temp = TextColBuf[i].GetRange(X + W_ - CursorFontW, CursorFontW);
                                    TextColBuf[i].RemoveRange(X + W_ - CursorFontW, CursorFontW);
                                    TextColBuf[i].InsertRange(X, Temp);
                                }
                                TextBufferTrimLine(i);
                            }
                        }
                    }
                    break;
                case 5:
                case 15:
                    if (Y < TextBuffer.Count)
                    {
                        for (int i = Y; i < (Y + H_); i++)
                        {
                            if (TextBuffer.Count <= i)
                            {
                                TextBuffer.Add(new List<int>());
                                TextColBuf.Add(new List<int>());
                                TextFonBuf.Add(new List<int>());
                            }
                            if (TextBuffer[i].Count < (X + W_ + 1))
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces((X + W_ + 1) - TextBuffer[i].Count));
                                TextColBuf[i].AddRange(TextWork.BlkCol((X + W_ + 1) - TextColBuf[i].Count));
                                TextFonBuf[i].AddRange(TextWork.BlkCol((X + W_ + 1) - TextFonBuf[i].Count));
                            }
                        }
                        List<List<int>> Temp1 = new List<List<int>>();
                        List<List<int>> Temp2 = new List<List<int>>();
                        List<List<int>> Temp3 = new List<List<int>>();
                        for (int i = 0; i < CursorFontH; i++)
                        {
                            Temp1.Add(TextBuffer[Y + i + H_ - CursorFontH].GetRange(X, W_));
                            Temp2.Add(TextFonBuf[Y + i + H_ - CursorFontH].GetRange(X, W_));
                            Temp3.Add(TextColBuf[Y + i + H_ - CursorFontH].GetRange(X, W_));
                        }
                        for (int i = (Y + H_ - CursorFontH - 1); i > (Y - 1); i--)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer[i + CursorFontH].RemoveRange(X, W_);
                                TextBuffer[i + CursorFontH].InsertRange(X, TextBuffer[i].GetRange(X, W_));
                                TextFonBuf[i + CursorFontH].RemoveRange(X, W_);
                                TextFonBuf[i + CursorFontH].InsertRange(X, TextFonBuf[i].GetRange(X, W_));
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[i + CursorFontH].RemoveRange(X, W_);
                                TextColBuf[i + CursorFontH].InsertRange(X, TextColBuf[i].GetRange(X, W_));
                            }
                            TextBufferTrimLine(i + CursorFontH);
                        }
                        for (int i = 0; i < CursorFontH; i++)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer[Y + i].RemoveRange(X, W_);
                                TextBuffer[Y + i].InsertRange(X, Temp1[i]);

                                TextFonBuf[Y + i].RemoveRange(X, W_);
                                TextFonBuf[Y + i].InsertRange(X, Temp2[i]);
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[Y + i].RemoveRange(X, W_);
                                TextColBuf[Y + i].InsertRange(X, Temp3[i]);
                            }
                            TextBufferTrimLine(Y + i);
                        }
                    }
                    break;
            }
            if (InsDelMode < 10)
            {
                UndoBufferItem_.X.Add(X);
                UndoBufferItem_.Y.Add(Y);
                UndoBufferItem_.CharOld.Add(-1);
                UndoBufferItem_.CharNew.Add(-1);
                UndoBufferItem_.ColoOld.Add(-1);
                UndoBufferItem_.ColoNew.Add(-1);
                UndoBufferItem_.FontOld.Add(-1);
                UndoBufferItem_.FontNew.Add(-1);
                UndoBufferItem_.OpParams.Add(new int[] { InsDelMode, W, H });
                TextDisplay(0);
            }
            switch (InsDelMode)
            {
                case 0:
                case 10:
                case 1:
                case 11:
                    for (int i_Y = Y; i_Y < (Y + H_); i_Y += CursorFontH)
                    {
                        for (int i_X = X; i_X < (X + W_); i_X += CursorFontW)
                        {
                            for (int i_YY = 0; i_YY < CursorFontH; i_YY++)
                            {
                                for (int i_XX = 0; i_XX < CursorFontW; i_XX++)
                                {
                                    while (TextBuffer.Count <= (Y + H_))
                                    {
                                        TextBuffer.Add(new List<int>());
                                        TextColBuf.Add(new List<int>());
                                        TextFonBuf.Add(new List<int>());
                                    }
                                    if (TextBuffer[i_Y + i_YY].Count < (X + W_))
                                    {
                                        TextBuffer[i_Y + i_YY].AddRange(TextWork.Spaces((X + W_) - TextBuffer[i_Y + i_YY].Count));
                                        TextColBuf[i_Y + i_YY].AddRange(TextWork.BlkCol((X + W_) - TextColBuf[i_Y + i_YY].Count));
                                        TextFonBuf[i_Y + i_YY].AddRange(TextWork.BlkCol((X + W_) - TextFonBuf[i_Y + i_YY].Count));
                                    }

                                    int Temp_B, Temp_F, Temp_W, Temp_H;
                                    if (ToggleDrawColo)
                                    {
                                        ColorFromInt(Semigraphics_.DrawColoI, out Temp_B, out Temp_F);
                                    }
                                    else
                                    {
                                        ColorFromInt(TextColBuf[i_Y + i_YY][i_X + i_XX], out Temp_B, out Temp_F);
                                    }
                                    if (ToggleDrawText)
                                    {
                                        Temp_W = FontSizeCode(CursorFontW, i_XX);
                                        Temp_H = FontSizeCode(CursorFontH, i_YY);
                                    }
                                    else
                                    {
                                        FontSFromInt(TextFonBuf[i_Y + i_YY][i_X + i_XX], out Temp_W, out Temp_H);
                                    }
                                    if (ToggleDrawText)
                                    {
                                        TextBuffer[i_Y + i_YY][i_X + i_XX] = Semigraphics_.DrawCharI;
                                        TextFonBuf[i_Y + i_YY][i_X + i_XX] = FontSToInt(Temp_W, Temp_H);
                                    }
                                    if (ToggleDrawColo)
                                    {
                                        TextColBuf[i_Y + i_YY][i_X + i_XX] = Semigraphics_.DrawColoI;
                                    }
                                    if (InsDelMode < 10)
                                    {
                                        UndoBufferItem_.X.Add(i_X + i_XX);
                                        UndoBufferItem_.Y.Add(i_Y + i_YY);
                                        UndoBufferItem_.CharOld.Add(TextWork.SpaceChar0);
                                        UndoBufferItem_.ColoOld.Add(0);
                                        UndoBufferItem_.FontOld.Add(0);
                                        UndoBufferItem_.CharNew.Add(TextBuffer[i_Y + i_YY][i_X + i_XX]);
                                        UndoBufferItem_.ColoNew.Add(TextColBuf[i_Y + i_YY][i_X + i_XX]);
                                        UndoBufferItem_.FontNew.Add(TextFonBuf[i_Y + i_YY][i_X + i_XX]);
                                        UndoBufferItem_.OpParams.Add(null);
                                    }
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

            int W_ = (W + 1) * CursorFontW;
            int H_ = (H + 1) * CursorFontH;

            switch (InsDelMode)
            {
                case 0:
                case 10:
                    for (int i = Y; i < (Y + H_); i++)
                    {
                        if (TextBuffer.Count > i)
                        {
                            if ((TextWork.TrimEndLength(TextBuffer[i]) > X) || (TextWork.TrimEndLenCol(TextColBuf[i]) > X) || (TextWork.TrimEndLenCol(TextFonBuf[i]) > X))
                            {
                                int RemCount = W_;
                                if (TextBuffer[i].Count <= (X + W_))
                                {
                                    RemCount = TextBuffer[i].Count - X;
                                }
                                if (InsDelMode < 10)
                                {
                                    for (int ii = 0; ii < RemCount; ii++)
                                    {
                                        UndoBufferItem_.X.Add(X + ii);
                                        UndoBufferItem_.Y.Add(i);
                                        UndoBufferItem_.CharOld.Add(TextBuffer[i][X + ii]);
                                        UndoBufferItem_.CharNew.Add(TextWork.SpaceChar0);
                                        UndoBufferItem_.ColoOld.Add(TextColBuf[i][X + ii]);
                                        UndoBufferItem_.ColoNew.Add(0);
                                        UndoBufferItem_.FontOld.Add(TextFonBuf[i][X + ii]);
                                        UndoBufferItem_.FontNew.Add(0);
                                        UndoBufferItem_.OpParams.Add(null);
                                    }
                                }
                                if (ToggleDrawText)
                                {
                                    TextBuffer[i].AddRange(TextWork.Spaces(RemCount));
                                    TextBuffer[i].RemoveRange(X, RemCount);
                                    TextFonBuf[i].AddRange(TextWork.BlkCol(RemCount));
                                    TextFonBuf[i].RemoveRange(X, RemCount);
                                }
                                if (ToggleDrawColo)
                                {
                                    TextColBuf[i].AddRange(TextWork.BlkCol(RemCount));
                                    TextColBuf[i].RemoveRange(X, RemCount);
                                }
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
                            if (TextBuffer[i].Count < (X + W_ + 1))
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces((X + W_ + 1) - TextBuffer[i].Count));
                                TextColBuf[i].AddRange(TextWork.BlkCol((X + W_ + 1) - TextColBuf[i].Count));
                                TextFonBuf[i].AddRange(TextWork.BlkCol((X + W_ + 1) - TextFonBuf[i].Count));
                            }
                        }
                        if (InsDelMode < 10)
                        {
                            for (int i = Y; i < (Y + H_); i++)
                            {
                                for (int ii = X; ii < (X + W_); ii++)
                                {
                                    if (TextBuffer.Count > i)
                                    {
                                        if (TextBuffer[i].Count > ii)
                                        {
                                            UndoBufferItem_.X.Add(ii);
                                            UndoBufferItem_.Y.Add(i);
                                            UndoBufferItem_.CharOld.Add(TextBuffer[i][ii]);
                                            UndoBufferItem_.CharNew.Add(TextWork.SpaceChar0);
                                            UndoBufferItem_.ColoOld.Add(TextColBuf[i][ii]);
                                            UndoBufferItem_.ColoNew.Add(0);
                                            UndoBufferItem_.FontOld.Add(TextFonBuf[i][ii]);
                                            UndoBufferItem_.FontNew.Add(0);
                                            UndoBufferItem_.OpParams.Add(null);
                                        }
                                    }
                                }
                            }
                        }
                        for (int i = Y; i < TextBuffer.Count; i++)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer[i].RemoveRange(X, W_);
                                TextFonBuf[i].RemoveRange(X, W_);
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[i].RemoveRange(X, W_);
                            }
                            if (ToggleDrawText)
                            {
                                if ((i + H_) < TextBuffer.Count)
                                {
                                    TextBuffer[i].InsertRange(X, TextBuffer[i + H_].GetRange(X, W_));
                                    TextFonBuf[i].InsertRange(X, TextFonBuf[i + H_].GetRange(X, W_));
                                }
                                else
                                {
                                    TextBuffer[i].InsertRange(X, TextWork.Spaces(W_));
                                    TextFonBuf[i].InsertRange(X, TextWork.BlkCol(W_));
                                }
                            }
                            if (ToggleDrawColo)
                            {
                                if ((i + H_) < TextColBuf.Count)
                                {
                                    TextColBuf[i].InsertRange(X, TextColBuf[i + H_].GetRange(X, W_));
                                }
                                else
                                {
                                    TextColBuf[i].InsertRange(X, TextWork.BlkCol(W_));
                                }
                            }

                            TextBufferTrimLine(i);
                        }
                    }
                    break;
                case 2:
                case 12:
                    for (int i = 0; i < TextBuffer.Count; i++)
                    {
                        if ((TextWork.TrimEndLength(TextBuffer[i]) > X) || (TextWork.TrimEndLenCol(TextColBuf[i]) > X) || (TextWork.TrimEndLenCol(TextFonBuf[i]) > X))
                        {
                            int RemCount = W_;
                            if (TextBuffer[i].Count <= (X + W_))
                            {
                                RemCount = TextBuffer[i].Count - X;
                            }
                            if (InsDelMode < 10)
                            {
                                for (int ii = 0; ii < RemCount; ii++)
                                {
                                    UndoBufferItem_.X.Add(X + ii);
                                    UndoBufferItem_.Y.Add(i);
                                    UndoBufferItem_.CharOld.Add(TextBuffer[i][X + ii]);
                                    UndoBufferItem_.CharNew.Add(TextWork.SpaceChar0);
                                    UndoBufferItem_.ColoOld.Add(TextColBuf[i][X + ii]);
                                    UndoBufferItem_.ColoNew.Add(0);
                                    UndoBufferItem_.FontOld.Add(TextFonBuf[i][X + ii]);
                                    UndoBufferItem_.FontNew.Add(0);
                                    UndoBufferItem_.OpParams.Add(null);
                                }
                            }
                            if (ToggleDrawText)
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces(RemCount));
                                TextBuffer[i].RemoveRange(X, RemCount);
                                TextFonBuf[i].AddRange(TextWork.BlkCol(RemCount));
                                TextFonBuf[i].RemoveRange(X, RemCount);
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[i].AddRange(TextWork.BlkCol(RemCount));
                                TextColBuf[i].RemoveRange(X, RemCount);
                            }
                            TextBufferTrimLine(i);
                        }
                    }
                    break;
                case 3:
                case 13:
                    if (Y < TextBuffer.Count)
                    {
                        for (int i = 0; i < H_; i++)
                        {
                            if (TextBuffer.Count > Y)
                            {
                                if (InsDelMode < 10)
                                {
                                    for (int ii = 0; ii < TextBuffer[Y].Count; ii++)
                                    {
                                        UndoBufferItem_.X.Add(ii);
                                        UndoBufferItem_.Y.Add(Y + i);
                                        UndoBufferItem_.CharOld.Add(TextBuffer[Y][ii]);
                                        UndoBufferItem_.CharNew.Add(TextWork.SpaceChar0);
                                        UndoBufferItem_.ColoOld.Add(TextColBuf[Y][ii]);
                                        UndoBufferItem_.ColoNew.Add(0);
                                        UndoBufferItem_.FontOld.Add(TextFonBuf[Y][ii]);
                                        UndoBufferItem_.FontNew.Add(0);
                                        UndoBufferItem_.OpParams.Add(null);
                                    }
                                }
                                if (ToggleDrawText)
                                {
                                    TextBuffer.RemoveAt(Y);
                                    TextBuffer.Add(new List<int>());
                                    TextFonBuf.RemoveAt(Y);
                                    TextFonBuf.Add(new List<int>());
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
                                TextFonBuf[i].AddRange(TextWork.BlkCol(TextColBuf[i].Count - TextBuffer[i].Count));
                                TextBuffer[i].AddRange(TextWork.Spaces(TextColBuf[i].Count - TextBuffer[i].Count));
                            }
                            if (TextColBuf[i].Count < TextBuffer[i].Count)
                            {
                                TextColBuf[i].AddRange(TextWork.BlkCol(TextBuffer[i].Count - TextColBuf[i].Count));
                            }
                        }
                    }
                    break;
                case 4:
                case 14:
                    for (int i = Y; i < (Y + H_); i++)
                    {
                        if (TextBuffer.Count > i)
                        {
                            if ((TextWork.TrimEndLength(TextBuffer[i]) > X) || (TextWork.TrimEndLenCol(TextColBuf[i]) > X))
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces(W_));
                                TextFonBuf[i].AddRange(TextWork.BlkCol(W_));
                                TextColBuf[i].AddRange(TextWork.BlkCol(W_));
                                List<int> Temp;

                                if (ToggleDrawText)
                                {
                                    Temp = TextBuffer[i].GetRange(X, CursorFontW);
                                    TextBuffer[i].RemoveRange(X, CursorFontW);
                                    TextBuffer[i].InsertRange(X + W_ - CursorFontW, Temp);

                                    Temp = TextFonBuf[i].GetRange(X, CursorFontW);
                                    TextFonBuf[i].RemoveRange(X, CursorFontW);
                                    TextFonBuf[i].InsertRange(X + W_ - CursorFontW, Temp);
                                }
                                if (ToggleDrawColo)
                                {
                                    Temp = TextColBuf[i].GetRange(X, CursorFontW);
                                    TextColBuf[i].RemoveRange(X, CursorFontW);
                                    TextColBuf[i].InsertRange(X + W_ - CursorFontW, Temp);
                                }
                                TextBufferTrimLine(i);
                            }
                        }
                    }
                    break;
                case 5:
                case 15:
                    if (Y < TextBuffer.Count)
                    {
                        for (int i = Y; i < (Y + H_); i++)
                        {
                            if (TextBuffer.Count <= i)
                            {
                                TextBuffer.Add(new List<int>());
                                TextColBuf.Add(new List<int>());
                                TextFonBuf.Add(new List<int>());
                            }
                            if (TextBuffer[i].Count < (X + W_ + 1))
                            {
                                TextBuffer[i].AddRange(TextWork.Spaces((X + W_ + 1) - TextBuffer[i].Count));
                                TextColBuf[i].AddRange(TextWork.BlkCol((X + W_ + 1) - TextColBuf[i].Count));
                                TextFonBuf[i].AddRange(TextWork.BlkCol((X + W_ + 1) - TextFonBuf[i].Count));
                            }
                        }
                        List<List<int>> Temp1 = new List<List<int>>();
                        List<List<int>> Temp2 = new List<List<int>>();
                        List<List<int>> Temp3 = new List<List<int>>();
                        for (int i = 0; i < CursorFontH; i++)
                        {
                            Temp1.Add(TextBuffer[Y + i].GetRange(X, W_));
                            Temp2.Add(TextFonBuf[Y + i].GetRange(X, W_));
                            Temp3.Add(TextColBuf[Y + i].GetRange(X, W_));
                        }
                        for (int i = (Y + CursorFontH); i < (Y + H_); i++)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer[i - CursorFontH].RemoveRange(X, W_);
                                TextBuffer[i - CursorFontH].InsertRange(X, TextBuffer[i].GetRange(X, W_));
                                TextFonBuf[i - CursorFontH].RemoveRange(X, W_);
                                TextFonBuf[i - CursorFontH].InsertRange(X, TextFonBuf[i].GetRange(X, W_));
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[i - CursorFontH].RemoveRange(X, W_);
                                TextColBuf[i - CursorFontH].InsertRange(X, TextColBuf[i].GetRange(X, W_));
                            }
                            TextBufferTrimLine(i - CursorFontH);
                        }
                        for (int i = 0; i < CursorFontH; i++)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer[Y + H_ - CursorFontH + i].RemoveRange(X, W_);
                                TextBuffer[Y + H_ - CursorFontH + i].InsertRange(X, Temp1[i]);

                                TextFonBuf[Y + H_ - CursorFontH + i].RemoveRange(X, W_);
                                TextFonBuf[Y + H_ - CursorFontH + i].InsertRange(X, Temp2[i]);
                            }
                            if (ToggleDrawColo)
                            {
                                TextColBuf[Y + H_ - CursorFontH + i].RemoveRange(X, W_);
                                TextColBuf[Y + H_ - CursorFontH + i].InsertRange(X, Temp3[i]);
                            }
                            TextBufferTrimLine(Y + H_ - CursorFontH + i);
                        }
                    }
                    break;
            }
            if (InsDelMode < 10)
            {
                UndoBufferItem_.X.Add(X);
                UndoBufferItem_.Y.Add(Y);
                UndoBufferItem_.CharOld.Add(-1);
                UndoBufferItem_.CharNew.Add(-1);
                UndoBufferItem_.ColoOld.Add(-1);
                UndoBufferItem_.ColoNew.Add(-1);
                UndoBufferItem_.FontOld.Add(-1);
                UndoBufferItem_.FontNew.Add(-1);
                UndoBufferItem_.OpParams.Add(new int[] { InsDelMode + 4, W, H });
                TextBufferTrim();
                TextDisplay(0);
            }
        }

        void TextBufferTrim()
        {
            for (int i = 0; i < TextBuffer.Count; i++)
            {
                TextBufferTrimLine(i);
            }
            while ((TextBuffer.Count > 0) && (TextBuffer[TextBuffer.Count - 1].Count == 0))
            {
                TextBuffer.RemoveAt(TextBuffer.Count - 1);
                TextFonBuf.RemoveAt(TextFonBuf.Count - 1);
                TextColBuf.RemoveAt(TextColBuf.Count - 1);
            }
        }
        
        void TextBufferTrimLine(int i)
        {
            while ((TextBuffer[i].Count > 0) && (TextWork.SpaceChars.Contains(TextBuffer[i][TextBuffer[i].Count - 1])) && (TextColBuf[i][TextColBuf[i].Count - 1] == 0))
            {
                TextBuffer[i].RemoveRange(TextBuffer[i].Count - 1, 1);
                TextFonBuf[i].RemoveRange(TextFonBuf[i].Count - 1, 1);
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
            public int FontWOld;
            public int FontHOld;
            public int FontWNew;
            public int FontHNew;
            public int CursorXNew;
            public int CursorYNew;
            public int CursorWNew;
            public int CursorHNew;
            public PixelPaintState PixelPaintStateOld;
            public PixelPaintState PixelPaintStateNew;
            public List<int> X;
            public List<int> Y;
            public List<int> CharOld;
            public List<int> CharNew;
            public List<int> ColoOld;
            public List<int> ColoNew;
            public List<int> FontOld;
            public List<int> FontNew;
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
            UndoBufferItem_.FontWOld = CursorFontW;
            UndoBufferItem_.FontHOld = CursorFontH;
            UndoBufferItem_.PixelPaintStateOld = PixelPaint_.PPS.GetState();
            UndoBufferItem_.X = new List<int>();
            UndoBufferItem_.Y = new List<int>();
            UndoBufferItem_.CharOld = new List<int>();
            UndoBufferItem_.CharNew = new List<int>();
            UndoBufferItem_.ColoOld = new List<int>();
            UndoBufferItem_.ColoNew = new List<int>();
            UndoBufferItem_.FontOld = new List<int>();
            UndoBufferItem_.FontNew = new List<int>();
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
            UndoBufferItem_.FontWNew = CursorFontW;
            UndoBufferItem_.FontHNew = CursorFontH;
            UndoBufferItem_.PixelPaintStateNew = PixelPaint_.PPS.GetState();

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
                        CharPut(UndoBufferItem_.X[i], UndoBufferItem_.Y[i], UndoBufferItem_.CharOld[i], UndoBufferItem_.ColoOld[i], UndoBufferItem_.FontOld[i], true);
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
                CursorFontW = UndoBufferItem_.FontWOld;
                CursorFontH = UndoBufferItem_.FontHOld;
                PixelPaint_.PPS.SetState(UndoBufferItem_.PixelPaintStateOld);

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
                        CharPut(UndoBufferItem_.X[i], UndoBufferItem_.Y[i], UndoBufferItem_.CharNew[i], UndoBufferItem_.ColoNew[i], UndoBufferItem_.FontNew[i], true);
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
                CursorFontW = UndoBufferItem_.FontWNew;
                CursorFontH = UndoBufferItem_.FontHNew;
                PixelPaint_.PPS.SetState(UndoBufferItem_.PixelPaintStateNew);
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
