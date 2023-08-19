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
        public Stack<bool> TempMemoB = new Stack<bool>();

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

        bool __AnsiLineOccupy1_Use = false;
        bool __AnsiLineOccupy2_Use = false;

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

        void CreateColor256()
        {
            int C11 = 63 - 1;
            int C12 = 64 + 1;
            int C21 = 191 - 1;
            int C22 = 192 + 2;
            int[] Val6 = new int[] { 0, 51, 102, 153, 204, 255 };
            Color256[0] = AnsiColor16(0, 0, 0);
            Color256[1] = AnsiColor16(255, C11, C11);
            Color256[2] = AnsiColor16(C11, 255, C11);
            Color256[3] = AnsiColor16(C21, C21, 0);
            Color256[4] = AnsiColor16(C11, C11, 255);
            Color256[5] = AnsiColor16(C21, 0, C21);
            Color256[6] = AnsiColor16(0, C21, C21);
            Color256[7] = AnsiColor16(128, 128, 128);
            Color256[8] = AnsiColor16(127, 127, 127);
            Color256[9] = AnsiColor16(255, C12, C12);
            Color256[10] = AnsiColor16(C12, 255, C12);
            Color256[11] = AnsiColor16(C22, C22, 0);
            Color256[12] = AnsiColor16(C12, C12, 255);
            Color256[13] = AnsiColor16(C22, 0, C22);
            Color256[14] = AnsiColor16(0, C22, C22);
            Color256[15] = AnsiColor16(255, 255, 255);
            for (int i_R = 0; i_R < 6; i_R++)
            {
                for (int i_G = 0; i_G < 6; i_G++)
                {
                    for (int i_B = 0; i_B < 6; i_B++)
                    {
                        int i_ = i_R * 36 + i_G * 6 + i_B + 16;
                        Color256[i_] = AnsiColor16(Val6[i_R], Val6[i_G], Val6[i_B]);
                    }
                }
            }
            Color256[232 + 0] = AnsiColor16(0, 0, 0);
            Color256[232 + 1] = AnsiColor16(11, 11, 11);
            Color256[232 + 2] = AnsiColor16(22, 22, 22);
            Color256[232 + 3] = AnsiColor16(33, 33, 33);
            Color256[232 + 4] = AnsiColor16(44, 44, 44);
            Color256[232 + 5] = AnsiColor16(55, 55, 55);
            Color256[232 + 6] = AnsiColor16(67, 67, 67);
            Color256[232 + 7] = AnsiColor16(78, 78, 78);
            Color256[232 + 8] = AnsiColor16(89, 89, 89);
            Color256[232 + 9] = AnsiColor16(100, 100, 100);
            Color256[232 + 10] = AnsiColor16(111, 111, 111);
            Color256[232 + 11] = AnsiColor16(122, 122, 122);
            Color256[232 + 12] = AnsiColor16(133, 133, 133);
            Color256[232 + 13] = AnsiColor16(144, 144, 144);
            Color256[232 + 14] = AnsiColor16(155, 155, 155);
            Color256[232 + 15] = AnsiColor16(166, 166, 166);
            Color256[232 + 16] = AnsiColor16(177, 177, 177);
            Color256[232 + 17] = AnsiColor16(188, 188, 188);
            Color256[232 + 18] = AnsiColor16(200, 200, 200);
            Color256[232 + 19] = AnsiColor16(211, 211, 211);
            Color256[232 + 20] = AnsiColor16(222, 222, 222);
            Color256[232 + 21] = AnsiColor16(233, 233, 233);
            Color256[232 + 22] = AnsiColor16(244, 244, 244);
            Color256[232 + 23] = AnsiColor16(255, 255, 255);
        }

        public Core()
        {
            CommandEndChar = TextWork.StrToInt(CommandEndChar_);
        }

        AnsiLineOccupyEx TextBuffer = new AnsiLineOccupyEx();

        AnsiLineOccupyEx ScrChar_ = new AnsiLineOccupyEx();
        AnsiLineOccupyEx ScrCharDisp_ = new AnsiLineOccupyEx();

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
                        ScrChar_.SetLineString(i, InfoTemp);
                        ScrChar_.PadRightSpace(i, WinTxtW);
                    }
                    else
                    {
                        ScrChar_.SetLineString(i, "");
                        ScrChar_.PadRightSpace(i, WinTxtW);
                    }
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
                        if ((i + DisplayY) < TextBuffer.CountLines())
                        {
                            ScrChar_.LineCopy(TextBuffer, i + DisplayY, i);
                            if (DisplayX > 0)
                            {
                                if (ScrChar_.CountItems(i) > DisplayX)
                                {
                                    ScrChar_.DeleteLeft(i, DisplayX);
                                }
                                else
                                {
                                    ScrChar_.ClearLine(i);
                                }
                            }
                            if (ScrChar_.CountItems(i) < WinTxtW)
                            {
                                ScrChar_.BlankChar();
                                ScrChar_.Item_Type = 1;
                                ScrChar_.PadRight(i, WinTxtW);
                            }
                            else
                            {
                                ScrChar_.Crop(i, 0, WinTxtW);
                            }
                        }
                        else
                        {
                            ScrChar_.ClearLine(i);
                            ScrChar_.BlankChar();
                            ScrChar_.Item_Type = 2;
                            ScrChar_.PadRight(i, WinTxtW);
                        }
                    }
                }
                else
                {
                    int CurOffset = (Mode == 3) ? 0 : WinTxtW - 1;
                    for (int i = I1; i <= I2; i++)
                    {
                        ScrChar_.BlankChar();
                        if ((i + DisplayY) < TextBuffer.CountLines())
                        {
                            if (TextBuffer.CountItems(i + DisplayY) > (DisplayX + CurOffset))
                            {
                                TextBuffer.Get(i + DisplayY, DisplayX + CurOffset);
                                ScrChar_.CopyItem(TextBuffer);
                                ScrChar_.Item_Type = 0;
                            }
                            else
                            {
                                ScrChar_.Item_Type = 1;
                            }
                        }
                        else
                        {
                            ScrChar_.Item_Type = 2;
                        }
                        if (Mode == 3)
                        {
                            ScrChar_.Set(i, 0);
                        }
                        else
                        {
                            ScrChar_.Set(i, WinTxtW - 1);
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

        private int ElementGet(AnsiLineOccupyEx Data, int KindType, int X, int Y, int Default)
        {
            ElementGet_Kind.Clear();
            ElementGet_Count.Clear();
            int MostCount = 1;
            for (int YY = Y; YY < (Y + CursorFontH); YY++)
            {
                for (int XX = X; XX < (X + CursorFontW); XX++)
                {
                    int Kind = Default;
                    if ((Data.CountLines() > YY) && (YY >= 0))
                    {
                        if ((Data.CountItems(YY) > XX) && (XX >= 0))
                        {
                            Data.Get_(YY, XX);
                            switch (KindType)
                            {
                                case 0: Kind = Data.Item_Char; break;
                                case 1: Kind = Data.Item_ColorB; break;
                                case 2: Kind = Data.Item_ColorF; break;
                                case 3: Kind = Data.Item_ColorA; break;
                                case 4: Kind = Data.Item_FontW; break;
                                case 5: Kind = Data.Item_FontH; break;
                            }
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


        public AnsiLineOccupyItem ElementGetObj(int X, int Y, bool Space, bool SingleCell)
        {
            AnsiLineOccupyItem Item = new AnsiLineOccupyItem();
            Item.Item_Char = ElementGetVal(X, Y, Space, SingleCell, 0);
            Item.Item_ColorB = ElementGetVal(X, Y, Space, SingleCell, 1);
            Item.Item_ColorF = ElementGetVal(X, Y, Space, SingleCell, 2);
            Item.Item_ColorA = ElementGetVal(X, Y, Space, SingleCell, 3);
            Item.Item_FontW = ElementGetVal(X, Y, Space, SingleCell, 4);
            Item.Item_FontH = ElementGetVal(X, Y, Space, SingleCell, 5);
            return Item;
        }

        public int ElementGetVal(int X, int Y, bool Space, bool SingleCell, int KindType)
        {
            if (SingleCell || ((CursorFontW == 1) && (CursorFontH == 1)))
            {
                if ((TextBuffer.CountLines() > Y) && (Y >= 0))
                {
                    if ((TextBuffer.CountItems(Y) > X) && (X >= 0))
                    {
                        TextBuffer.Get(Y, X);
                        switch (KindType)
                        {
                            case 0: return TextBuffer.Item_Char;
                            case 1: return TextBuffer.Item_ColorB;
                            case 2: return TextBuffer.Item_ColorF;
                            case 3: return TextBuffer.Item_ColorA;
                            case 4: return TextBuffer.Item_FontW;
                            case 5: return TextBuffer.Item_FontH;
                        }
                    }
                }
                switch (KindType)
                {
                    case 0:
                        if (Space)
                        {
                            return TextWork.SpaceChar0;
                        }
                        else
                        {
                            return -1;
                        }
                    case 1:
                    case 2:
                        return -1;
                    default:
                        return 0;
                }
            }
            else
            {
                switch (KindType)
                {
                    case 0:
                        return ElementGet(TextBuffer, KindType, X, Y, Space ? TextWork.SpaceChar0 : -1);
                    case 1:
                    case 2:
                        return ElementGet(TextBuffer, KindType, X, Y, -1);
                    default:
                        return ElementGet(TextBuffer, KindType, X, Y, 0);
                }
            }
        }

        public int CharGet(int X, int Y, bool Space, bool SingleCell)
        {
            return ElementGetVal(X, Y, Space, SingleCell, 0);
        }

        public int ColoBGet(int X, int Y, bool Space, bool SingleCell)
        {
            return ElementGetVal(X, Y, Space, SingleCell, 1);
        }

        public int ColoFGet(int X, int Y, bool Space, bool SingleCell)
        {
            return ElementGetVal(X, Y, Space, SingleCell, 2);
        }

        public void CharPut0(int X, int Y, int Ch)
        {
            AnsiLineOccupyItem Item = new AnsiLineOccupyItem();
            Item.Item_Char = Ch;
            Item.Item_ColorB = Semigraphics_.DrawColoBI;
            Item.Item_ColorF = Semigraphics_.DrawColoFI;
            Item.Item_ColorA = Semigraphics_.DrawColoAI;
            Item.Item_FontW = 0;
            Item.Item_FontH = 0;
            CharPut(X, Y, Item, false);
        }

        public void CharPut(int X, int Y, int Ch)
        {
            AnsiLineOccupyItem Item = new AnsiLineOccupyItem();
            Item.Item_Char = Ch;
            Item.Item_ColorB = Semigraphics_.DrawColoBI;
            Item.Item_ColorF = Semigraphics_.DrawColoFI;
            Item.Item_ColorA = Semigraphics_.DrawColoAI;
            Item.Item_FontW = -1;
            Item.Item_FontH = -1;
            CharPut(X, Y, Item, false);
        }

        public void CharPut(int X, int Y, AnsiLineOccupyItem Item, bool SingleCell)
        {
            AnsiLineOccupyItem Item_ = new AnsiLineOccupyItem();

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

            while (TextBuffer.CountLines() <= (Y + CursorFontH - 1))
            {
                TextBuffer.AppendLine();
                TextDisplayLine(TextBuffer.CountLines() - 1);
            }

            for (int YY = 0; YY < CursorFontH_; YY++)
            {
                for (int XX = 0; XX < CursorFontW_; XX++)
                {
                    Item_.Item_Char = ElementGetVal(X + XX, Y + YY, true, true, 0);
                    Item_.Item_ColorB = ElementGetVal(X + XX, Y + YY, true, true, 1);
                    Item_.Item_ColorF = ElementGetVal(X + XX, Y + YY, true, true, 2);
                    Item_.Item_ColorA = ElementGetVal(X + XX, Y + YY, true, true, 3);
                    Item_.Item_FontW = ElementGetVal(X + XX, Y + YY, true, false, 4);
                    Item_.Item_FontH = ElementGetVal(X + XX, Y + YY, true, false, 5);

                    if (!ToggleDrawText)
                    {
                        Item.Item_Char = Item_.Item_Char;
                        Item.Item_FontW = Item_.Item_FontW;
                        Item.Item_FontH = Item_.Item_FontH;
                    }
                    if (!ToggleDrawColo)
                    {
                        Item.Item_ColorB = Item_.Item_ColorB;
                        Item.Item_ColorF = Item_.Item_ColorF;
                        Item.Item_ColorA = Item_.Item_ColorA;
                    }

                    if (!SingleCell)
                    {
                        if (ToggleDrawText)
                        {
                            Item.Item_FontW = FontSizeCode(CursorFontW_, XX);
                            Item.Item_FontH = FontSizeCode(CursorFontH_, YY);
                        }
                    }

                    if (TextBuffer.CountItems(Y + YY) > (X + XX))
                    {
                        TextBuffer.CopyItem(Item);
                        TextBuffer.Set_(Y + YY, X + XX);
                        TextBuffer.Trim(Y + YY);
                        if (TextWork.SpaceChars.Contains(Item.Item_Char) && (Item.Item_ColorB < 0) && (Item.Item_ColorF < 0) && (Item.Item_ColorA == 0))
                        {
                            TextBuffer.Trim(Y + YY);
                        }
                    }
                    else
                    {
                        if ((!TextWork.SpaceChars.Contains(Item.Item_Char)) || (Item.Item_ColorB >= 0) || (Item.Item_ColorF >= 0) || (Item.Item_ColorA > 0))
                        {
                            TextBuffer.CopyItem(Item);
                            TextBuffer.Set_(Y + YY, X + XX);
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
                                    UndoBufferItem_.ItemNew[i] = Item;
                                    UndoBufNew = false;
                                    break;
                                }
                            }
                        }
                        if (UndoBufNew)
                        {
                            UndoBufferItem_.X.Add(X + XX);
                            UndoBufferItem_.Y.Add(Y + YY);
                            UndoBufferItem_.ItemOld.Add(Item_);
                            UndoBufferItem_.ItemNew.Add(Item);
                            UndoBufferItem_.OpParams.Add(null);
                        }
                    }
                }
            }
            while ((TextBuffer.CountLines() > 0) && (TextBuffer.CountItems(TextBuffer.CountLines() - 1) == 0))
            {
                TextBuffer.DeleteLine(TextBuffer.CountLines() - 1);
                TextDisplayLine(TextBuffer.CountLines());
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
                    ScrChar_.Get(YY, XX);
                    if (Show)
                    {
                        if (ScrChar_.Item_Type < 3)
                        {
                            ScrChar_.Item_Type += 3;
                        }
                    }
                    else
                    {
                        if (ScrChar_.Item_Type > 2)
                        {
                            ScrChar_.Item_Type -= 3;
                        }
                    }
                    ScrChar_.Set(YY, XX);
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
                        ScrChar_.Get(YY, XX);
                        if (Show)
                        {
                            if (ScrChar_.Item_Type < 3)
                            {
                                ScrChar_.Item_Type += 3;
                            }
                        }
                        else
                        {
                            if (ScrChar_.Item_Type > 2)
                            {
                                ScrChar_.Item_Type -= 3;
                            }
                        }
                        ScrChar_.Set(YY, XX);
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
                ScrChar_.InsertLine(0);
                ScrChar_.DeleteLine(WinTxtH);

                ScrCharDisp_.InsertLine(0);
                ScrCharDisp_.PadRightTab(0, WinTxtW);
                ScrCharDisp_.DeleteLine(WinTxtH);
                TextDisplay(1);
            }
            while (DisplayY < (CursorY - WinTxtH + 1))
            {
                DisplayY++;
                Screen_.Move(0, 1, 0, 0, WinTxtW, WinTxtH - 1);
                ScrChar_.DeleteLine(0);
                ScrChar_.AppendLine();

                ScrCharDisp_.DeleteLine(0);
                ScrCharDisp_.AppendLine();
                int II = ScrCharDisp_.CountLines() - 1;
                for (int I = 0; I < WinTxtW; I++)
                {
                    ScrCharDisp_.PadRightTab(II, WinTxtW);
                }
                TextDisplay(2);
            }
            while (DisplayX > CursorX)
            {
                DisplayX--;
                Screen_.Move(0, 0, 1, 0, WinTxtW - 1, WinTxtH);
                ScrChar_.BlankChar();
                ScrCharDisp_.BlankChar();
                ScrCharDisp_.Item_Type = 32;
                for (int i = 0; i < WinTxtH; i++)
                {
                    ScrChar_.Insert(i, 0);
                    ScrChar_.Delete(i, WinTxtW);

                    ScrCharDisp_.Insert(i, 0);
                    ScrCharDisp_.Delete(i, WinTxtW);
                }
                TextDisplay(3);
            }
            while (DisplayX < (CursorX - WinTxtW + 1))
            {
                DisplayX++;
                Screen_.Move(1, 0, 0, 0, WinTxtW - 1, WinTxtH);
                ScrChar_.BlankChar();
                ScrCharDisp_.BlankChar();
                ScrCharDisp_.Item_Type = 32;
                for (int i = 0; i < WinTxtH; i++)
                {
                    ScrChar_.Delete(i, 0);
                    ScrChar_.Append(i);

                    ScrCharDisp_.Delete(i, 0);
                    ScrCharDisp_.Append(i);
                }
                TextDisplay(4);
            }
        }



        public Semigraphics Semigraphics_;
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

            CurrentFileName_ = PrepareFileName(CurrentFileName_);

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
            ScrChar_ = new AnsiLineOccupyEx();
            TextBuffer = new AnsiLineOccupyEx();
            ScrCharDisp_ = new AnsiLineOccupyEx();
            TextBuffer.Clear();
            FileREnc = CF.ParamGetS("FileReadEncoding");
            FileWEnc = CF.ParamGetS("FileWriteEncoding");
            FileReadSteps = CF.ParamGetI("FileReadSteps");
            UseAnsiLoad = CF.ParamGetB("ANSIRead");
            UseAnsiSave = CF.ParamGetB("ANSIWrite");
            AnsiMaxX = CF.ParamGetI("ANSIWidth");
            AnsiMaxY = CF.ParamGetI("ANSIHeight");
            __AnsiLineOccupy1_Use = CF.ParamGetB("ANSIBufferAbove");
            __AnsiLineOccupy2_Use = CF.ParamGetB("ANSIBufferBelow");
            ANSI_CR = CF.ParamGetI("ANSIReadCR");
            ANSI_LF = CF.ParamGetI("ANSIReadLF");
            TextBeyondLineMargin = CF.ParamGetI("BeyondLineMargin");

            ANSIDOS = CF.ParamGetB("ANSIDOS");
            ANSI8bit = CF.ParamGetB("ANSI8bit");
            ANSIPrintBackspace = CF.ParamGetB("ANSIPrintBackspace");
            ANSIPrintTab = CF.ParamGetB("ANSIPrintTab");

            AnsiTerminalResize(AnsiMaxX, AnsiMaxY);

            ANSIScrollChars = CF.ParamGetI("ANSIScrollChars");
            ANSIScrollBuffer = CF.ParamGetI("ANSIScrollBuffer");
            ANSIScrollSmooth = CF.ParamGetI("ANSIScrollSmooth");

            ColorThresholdBlackWhite = CF.ParamGetI("ANSIColorThresholdBlackWhite");
            ColorThresholdGray = CF.ParamGetI("ANSIColorThresholdGray");
            CreateColor256();

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
                    Screen_ = new ScreenWindowGUI(this, CF.ParamGetI("WinFixed"), CF, WinW__, WinH__, ColorBlending, ColorBlendingConfig, false);
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
                for (int _1 = 0; _1 < EncodingList.Count; _1++)
                {
                    for (int _2 = 0; _2 < EncodingList.Count; _2++)
                    {
                        if (EncodingCodePage[_1] < EncodingCodePage[_2])
                        {
                            int I = EncodingCodePage[_1];
                            EncodingCodePage[_1] = EncodingCodePage[_2];
                            EncodingCodePage[_2] = I;
                            string S = EncodingList[_1];
                            EncodingList[_1] = EncodingList[_2];
                            EncodingList[_2] = S;
                            List<int> L = EncodingInfo[_1];
                            EncodingInfo[_1] = EncodingInfo[_2];
                            EncodingInfo[_2] = L;
                        }
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
                Screen_ = new ScreenWindowGUI(this, 0, CF, 1, 1, ColorBlending, ColorBlendingConfig, true);
                RenderSliceX = CF.ParamGetI("RenderSliceX");
                RenderSliceY = CF.ParamGetI("RenderSliceY");
                RenderSliceW = CF.ParamGetI("RenderSliceW");
                RenderSliceH = CF.ParamGetI("RenderSliceH");
                RenderStart(CF);
            }
        }

        public void StartUp()
        {
            Screen_.Clear(TextNormalBack, TextNormalFore);
            if (WorkMode == 0)
            {
                FileLoad0(true);
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
                    ScrChar_.Get(Y, X);
                    ScrCharDisp_.Get(Y, X);
                    bool Difference = false;
                    if (ScrCharDisp_.Item_Char != ScrChar_.Item_Char) Difference = true;
                    if (ScrCharDisp_.Item_ColorB != ScrChar_.Item_ColorB) Difference = true;
                    if (ScrCharDisp_.Item_ColorF != ScrChar_.Item_ColorF) Difference = true;
                    if (ScrCharDisp_.Item_ColorA != ScrChar_.Item_ColorA) Difference = true;
                    if (ScrCharDisp_.Item_FontW != ScrChar_.Item_FontW) Difference = true;
                    if (ScrCharDisp_.Item_FontH != ScrChar_.Item_FontH) Difference = true;
                    if (ScrCharDisp_.Item_Type != ScrChar_.Item_Type) Difference = true;
                    if (Force || Difference)
                    {
                        bool InsideText = true;
                        switch (ScrChar_.Item_Type)
                        {
                            case 1:
                                if (((X + DisplayX) < TextBeyondLineMargin) || ((TextBeyondLineMargin < 0) && ((X + DisplayX) < AnsiMaxX)))
                                {
                                    ScrChar_.Item_ColorB = -1;
                                    ScrChar_.Item_ColorF = -1;
                                }
                                else
                                {
                                    ScrChar_.Item_ColorB = TextBeyondLineBack;
                                    ScrChar_.Item_ColorF = TextBeyondLineFore;
                                    InsideText = false;
                                }
                                break;
                            case 2:
                                ScrChar_.Item_ColorB = TextBeyondEndBack;
                                ScrChar_.Item_ColorF = TextBeyondEndFore;
                                InsideText = false;
                                break;
                            case 3:
                            case 4:
                            case 5:
                                ScrChar_.Item_ColorB = CursorBack;
                                ScrChar_.Item_ColorF = CursorFore;
                                InsideText = false;
                                break;
                        }
                        ScrCharDisp_.CopyItem(ScrChar_);
                        ScrCharDisp_.Set(Y, X);
                        if (InsideText)
                        {
                            if (Force)
                            {
                                Screen_.PutChar(X, Y, 32, -1, -1);
                            }
                            Screen_.PutChar(X, Y, ScrChar_.Item_Char, ScrChar_.Item_ColorB, ScrChar_.Item_ColorF, ScrChar_.Item_FontW, ScrChar_.Item_FontH, ScrChar_.Item_ColorA);
                        }
                        else
                        {
                            Screen_.PutChar(X, Y, ScrChar_.Item_Char, ScrChar_.Item_ColorB, ScrChar_.Item_ColorF);
                        }
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

                    ScrChar_.Clear();
                    ScrCharDisp_.Clear();
                    ScrCharDisp_.BlankChar();
                    ScrCharDisp_.Item_Type = 32;
                    for (int i = 0; i < WinTxtH; i++)
                    {
                        ScrChar_.AppendLine();
                        ScrCharDisp_.AppendLine();
                        ScrCharDisp_.PadRight(i, WinTxtW);
                    }
                    TextDisplay(0);
                    CursorLimit();
                }
            }
            if ((WorkMode == 1) || (WorkMode == 2))
            {
                if (Screen_.WindowResize())
                {
                    WinW = Screen_.WinW;
                    WinH = Screen_.WinH;
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
            if ((CursorY + CursorFontH - 1) < TextBuffer.CountLines())
            {
                for (int i = 0; i < CursorFontH; i++)
                {
                    if ((CursorX + CursorFontW - 1) >= TextBuffer.CountItems(CursorY + i))
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
        int StatusCursorColoB = -1;
        int StatusCursorColoF = -1;
        int StatusCursorColoA = 0;
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
                    Semigraphics_.CursorChar = ElementGetVal(CursorX, CursorY, true, false, 0);
                    Semigraphics_.CursorColoB = ElementGetVal(CursorX, CursorY, true, false, 1);
                    Semigraphics_.CursorColoF = ElementGetVal(CursorX, CursorY, true, false, 2);
                    Semigraphics_.CursorColoA = ElementGetVal(CursorX, CursorY, true, false, 3);
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
                            StatusCursorColoB = Semigraphics_.CursorColoB;
                            StatusCursorColoF = Semigraphics_.CursorColoF;
                            StatusCursorColoA = Semigraphics_.CursorColoA;

                        }
                        StatusText.Append(" " + TextWork.CharCode(StatusCursorChar, 1) + " " + TextWork.CharToStr(StatusCursorChar) + " ");
                        if ((StatusCursorColoB >= 0) && (StatusCursorColoB <= 15))
                        {
                            StatusText.Append(StatusCursorColoB.ToString("X"));
                        }
                        else
                        {
                            StatusText.Append("-");
                        }
                        if ((StatusCursorColoF >= 0) && (StatusCursorColoF <= 15))
                        {
                            StatusText.Append(StatusCursorColoF.ToString("X"));
                        }
                        else
                        {
                            StatusText.Append("-");
                        }
                        StatusText.Append(" ");
                        StatusText.Append(GetAttribText(StatusCursorColoA));
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
                    Telnet_.CoreEvent(KeyName, KeyChar, ModShift, ModCtrl, ModAlt);
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
                                Screen_.PutChar(i, WinH - 1, KeyInfoText[i], TextNormalBack, TextNormalFore);
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
                                ScrChar_.InsertLine(0);
                                ScrChar_.DeleteLine(WinTxtH);
                                ScrCharDisp_.InsertLine(0);
                                ScrCharDisp_.PadRightTab(0, WinTxtW);
                                ScrCharDisp_.DeleteLine(WinTxtH);
                                TextDisplay(1);
                                break;
                            case 3:
                                Screen_.Move(0, 1, 0, 0, WinTxtW, WinTxtH - 1);
                                ScrChar_.DeleteLine(0);
                                ScrChar_.AppendLine();
                                ScrCharDisp_.DeleteLine(0);
                                ScrCharDisp_.AppendLine();
                                int II = ScrCharDisp_.CountLines() - 1;
                                ScrCharDisp_.PadRightTab(II, WinTxtW);
                                TextDisplay(2);
                                break;
                            case 4:
                                Screen_.Move(0, 0, 1, 0, WinTxtW - 1, WinTxtH);
                                ScrChar_.BlankChar();
                                ScrCharDisp_.BlankChar();
                                ScrCharDisp_.Item_Type = 32;
                                for (int i = 0; i < WinTxtH; i++)
                                {
                                    ScrChar_.Insert(i, 0);
                                    ScrChar_.Delete(i, WinTxtW);
                                    ScrCharDisp_.Insert(i, 0);
                                    ScrCharDisp_.Delete(i, WinTxtW);
                                }
                                TextDisplay(3);
                                break;
                            case 5:
                                Screen_.Move(1, 0, 0, 0, WinTxtW - 1, WinTxtH);
                                ScrChar_.BlankChar();
                                ScrCharDisp_.BlankChar();
                                ScrCharDisp_.Item_Type = 32;
                                for (int i = 0; i < WinTxtH; i++)
                                {
                                    ScrChar_.Delete(i, 0);
                                    ScrChar_.Append(i);
                                    ScrCharDisp_.Delete(i, 0);
                                    ScrCharDisp_.Append(i);
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
                    FileLoad0(false);
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
                                            CharPut0(CursorX, CursorY, Semigraphics_.FavChar[KeyChar]);
                                        }
                                    }
                                    else
                                    {
                                        CharPut0(CursorX, CursorY, KeyChar);
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
                            case "Space":
                            case "NumPad0":
                                UndoBufferStart();
                                CharPut(CursorX, CursorY, Semigraphics_.DrawCharI);
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
                                if (PixelPaint_.PPS.PaintColor == 4)
                                {
                                    PixelPaint_.PPS.PaintColor = 0;
                                }
                                if (PixelPaint_.PPS.PaintColor == 8)
                                {
                                    PixelPaint_.PPS.PaintColor = 4;
                                }
                                break;
                            case "F":
                                if (PixelPaint_.PPS.PaintColor < 4)
                                {
                                    PixelPaint_.PPS.PaintColor += 4;
                                }
                                else
                                {
                                    PixelPaint_.PPS.PaintColor -= 4;
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

        void FileLoad0(bool ForceDummyLoad)
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
                if (ForceDummyLoad || (FileExists(NewFile)))
                {
                    CurrentFileName = NewFile;
                    CursorX = 0;
                    CursorY = 0;
                    DisplayX = 0;
                    DisplayY = 0;
                }
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

            TextBuffer.BlankChar();
            AnsiLineOccupyEx ColoBuffer = TextBuffer.CloneData();
            AnsiLineOccupyEx TempBuffer = new AnsiLineOccupyEx();
            switch (InsDelMode)
            {
                case 0:
                case 10:
                    for (int i = Y; i < (Y + H_); i++)
                    {
                        if (TextBuffer.CountLines() > i)
                        {
                            if (TextBuffer.CountItemsTrim(i) > X)
                            {
                                if (ToggleDrawText)
                                {
                                    TextBuffer.Insert(i, X, W_);
                                }
                                else
                                {
                                    TextBuffer.Append(i, W_);
                                }
                                if (ToggleDrawColo)
                                {
                                    ColoBuffer.Insert(i, X, W_);
                                }
                                else
                                {
                                    ColoBuffer.Append(i, W_);
                                }
                            }
                        }
                    }
                    break;
                case 1:
                case 11:
                    if (Y < TextBuffer.CountLines())
                    {
                        for (int i = Y; i < TextBuffer.CountLines(); i++)
                        {
                            TextBuffer.PadRight(i, X + W_ + 1);
                            ColoBuffer.PadRight(i, X + W_ + 1);
                        }
                        for (int i = 0; i < H_; i++)
                        {
                            TextBuffer.AppendLine();
                            TextBuffer.Append(TextBuffer.CountLines() - 1, X + W_ + 1);
                            ColoBuffer.AppendLine();
                            ColoBuffer.Append(ColoBuffer.CountLines() - 1, X + W_ + 1);
                        }
                        for (int i = (TextBuffer.CountLines() - H_ - 0); i > Y; i--)
                        {
                            if (ToggleDrawText)
                            {
                                TempBuffer.Clear();
                                TempBuffer.AppendLineCopy(TextBuffer, i - 1);
                                TempBuffer.Crop(0, X, W_);

                                TextBuffer.Delete(i + H_ - 1, X, W_);
                                TextBuffer.Insert(i + H_ - 1, X, TempBuffer, 0);
                            }
                            if (ToggleDrawColo)
                            {
                                TempBuffer.Clear();
                                TempBuffer.AppendLineCopy(ColoBuffer, i - 1);
                                TempBuffer.Crop(0, X, W_);

                                ColoBuffer.Delete(i + H_ - 1, X, W_);
                                ColoBuffer.Insert(i + H_ - 1, X, TempBuffer, 0);
                            }
                            TextBuffer.Trim(i + H_ - 1);
                            ColoBuffer.Trim(i + H_ - 1);
                        }
                        TextBuffer.BlankChar();
                        ColoBuffer.BlankChar();
                        for (int i = Y; i < (Y + H_); i++)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer.Delete(i, X, W_);
                                TextBuffer.Insert(i, X, W_);
                            }
                            if (ToggleDrawColo)
                            {
                                ColoBuffer.Delete(i, X, W_);
                                ColoBuffer.Insert(i, X, W_);
                            }
                            TextBuffer.Trim(i);
                            ColoBuffer.Trim(i);
                        }
                    }
                    break;
                case 2:
                case 12:
                    for (int i = 0; i < TextBuffer.CountLines(); i++)
                    {
                        if (TextBuffer.CountItemsTrim(i) > X)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer.Insert(i, X, W_);
                            }
                            else
                            {
                                TextBuffer.Append(i, W_);
                            }
                            if (ToggleDrawColo)
                            {
                                ColoBuffer.Insert(i, X, W_);
                            }
                            else
                            {
                                ColoBuffer.Append(i, W_);
                            }
                        }
                    }
                    break;
                case 3:
                case 13:
                    if (Y < TextBuffer.CountLines())
                    {
                        for (int i = 0; i < H_; i++)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer.InsertLine(Y);
                            }
                            else
                            {
                                TextBuffer.AppendLine();
                            }
                            if (ToggleDrawColo)
                            {
                                ColoBuffer.InsertLine(Y);
                            }
                            else
                            {
                                ColoBuffer.AppendLine();
                            }
                        }
                        for (int i = Y; i < TextBuffer.CountLines(); i++)
                        {
                            if (TextBuffer.CountItems(i) < ColoBuffer.CountItems(i))
                            {
                                TextBuffer.PadRight(i, ColoBuffer.CountItems(i));
                            }
                            if (ColoBuffer.CountItems(i) < TextBuffer.CountItems(i))
                            {
                                ColoBuffer.PadRight(i, TextBuffer.CountItems(i));
                            }
                        }
                    }
                    break;
                case 4:
                case 14:
                    for (int i = Y; i < (Y + H_); i++)
                    {
                        if (TextBuffer.CountLines() > i)
                        {
                            if (TextBuffer.CountItemsTrim(i) > X)
                            {
                                TextBuffer.BlankChar();
                                TextBuffer.Append(i, W_);
                                ColoBuffer.BlankChar();
                                ColoBuffer.Append(i, W_);
                                if (ToggleDrawText)
                                {
                                    TempBuffer.Clear();
                                    TempBuffer.AppendLineCopy(TextBuffer, i);
                                    TempBuffer.Crop(0, X + W_ - CursorFontW, CursorFontW);
                                    TextBuffer.Delete(i, X + W_ - CursorFontW, CursorFontW);
                                    TextBuffer.Insert(i, X, TempBuffer, 0);
                                }
                                if (ToggleDrawColo)
                                {
                                    TempBuffer.Clear();
                                    TempBuffer.AppendLineCopy(ColoBuffer, i);
                                    TempBuffer.Crop(0, X + W_ - CursorFontW, CursorFontW);
                                    ColoBuffer.Delete(i, X + W_ - CursorFontW, CursorFontW);
                                    ColoBuffer.Insert(i, X, TempBuffer, 0);
                                }
                                TextBuffer.Trim(i);
                                ColoBuffer.Trim(i);
                            }
                        }
                    }
                    break;
                case 5:
                case 15:
                    if (Y < TextBuffer.CountLines())
                    {
                        TextBuffer.BlankChar();
                        ColoBuffer.BlankChar();
                        for (int i = Y; i < (Y + H_); i++)
                        {
                            if (TextBuffer.CountLines() <= i)
                            {
                                TextBuffer.AppendLine();
                            }
                            TextBuffer.PadRight(i, (X + W_ + 1));
                            if (ColoBuffer.CountLines() <= i)
                            {
                                ColoBuffer.AppendLine();
                            }
                            ColoBuffer.PadRight(i, (X + W_ + 1));
                        }
                        TempBuffer.Clear();
                        TempBuffer.AppendLine();
                        TempBuffer.AppendLine();
                        for (int i = 0; i < CursorFontH; i++)
                        {
                            TempBuffer.AppendLineCopy(TextBuffer, Y + i + H_ - CursorFontH);
                            TempBuffer.AppendLineCopy(ColoBuffer, Y + i + H_ - CursorFontH);
                            TempBuffer.Crop((i << 1) + 2, X, W_);
                            TempBuffer.Crop((i << 1) + 3, X, W_);
                        }
                        for (int i = (Y + H_ - CursorFontH - 1); i > (Y - 1); i--)
                        {
                            if (ToggleDrawText)
                            {
                                TempBuffer.LineCopy(TextBuffer, i, 0);
                                TempBuffer.Crop(0, X, W_);

                                TextBuffer.Delete(i + CursorFontH, X, W_);
                                TextBuffer.Insert(i + CursorFontH, X, TempBuffer, 0);
                            }
                            if (ToggleDrawColo)
                            {
                                TempBuffer.LineCopy(ColoBuffer, i, 1);
                                TempBuffer.Crop(1, X, W_);

                                ColoBuffer.Delete(i + CursorFontH, X, W_);
                                ColoBuffer.Insert(i + CursorFontH, X, TempBuffer, 1);
                            }
                            TextBuffer.Trim(i + CursorFontH);
                            ColoBuffer.Trim(i + CursorFontH);
                        }
                        for (int i = 0; i < CursorFontH; i++)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer.Delete(Y + i, X, W_);
                                TextBuffer.Insert(Y + i, X, TempBuffer, (i << 1) + 2);
                            }
                            if (ToggleDrawColo)
                            {
                                ColoBuffer.Delete(Y + i, X, W_);
                                ColoBuffer.Insert(Y + i, X, TempBuffer, (i << 1) + 3);
                            }
                            TextBuffer.Trim(Y + i);
                            ColoBuffer.Trim(Y + i);
                        }
                    }
                    break;
            }
            if (InsDelMode < 10)
            {
                UndoBufferItem_.X.Add(X);
                UndoBufferItem_.Y.Add(Y);
                UndoBufferItem_.ItemOld.Add(null);
                UndoBufferItem_.ItemNew.Add(null);
                UndoBufferItem_.OpParams.Add(new int[] { InsDelMode, W, H });
                TextDisplay(0);
            }
            TextBuffer.MergeColor(ColoBuffer);
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
                                    while (TextBuffer.CountLines() <= (Y + H_))
                                    {
                                        TextBuffer.AppendLine();
                                    }
                                    TextBuffer.BlankChar();
                                    TextBuffer.PadRight(i_Y + i_YY, X + W_);

                                    AnsiLineOccupyItem Temp__ = new AnsiLineOccupyItem();

                                    TextBuffer.Get(i_Y + i_YY, i_X + i_XX);
                                    Temp__.CopyItem(TextBuffer);
                                    if (ToggleDrawColo)
                                    {
                                        TextBuffer.Item_ColorB = Semigraphics_.DrawColoBI;
                                        TextBuffer.Item_ColorF = Semigraphics_.DrawColoFI;
                                        TextBuffer.Item_ColorA = Semigraphics_.DrawColoAI;
                                    }
                                    if (ToggleDrawText)
                                    {
                                        TextBuffer.Item_FontW = FontSizeCode(CursorFontW, i_XX);
                                        TextBuffer.Item_FontH = FontSizeCode(CursorFontH, i_YY);
                                    }
                                    if (ToggleDrawText)
                                    {
                                        TextBuffer.Item_Char = Semigraphics_.DrawCharI;
                                    }
                                    if (ToggleDrawColo)
                                    {
                                        TextBuffer.Item_ColorB = Semigraphics_.DrawColoBI;
                                        TextBuffer.Item_ColorF = Semigraphics_.DrawColoFI;
                                        TextBuffer.Item_ColorA = Semigraphics_.DrawColoAI;
                                    }
                                    if (InsDelMode < 10)
                                    {
                                        TextBuffer.Get(i_Y + i_YY, i_X + i_XX);
                                        UndoBufferItem_.X.Add(i_X + i_XX);
                                        UndoBufferItem_.Y.Add(i_Y + i_YY);
                                        UndoBufferItem_.ItemOld.Add(new AnsiLineOccupyItem());
                                        UndoBufferItem_.ItemNew.Add(TextBuffer.CopyItemObj());
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

            AnsiLineOccupyEx ColoBuffer = TextBuffer.CloneData();
            AnsiLineOccupyEx TempBuffer = new AnsiLineOccupyEx();
            switch (InsDelMode)
            {
                case 0:
                case 10:
                    for (int i = Y; i < (Y + H_); i++)
                    {
                        if (TextBuffer.CountLines() > i)
                        {
                            if (TextBuffer.CountItemsTrim(i) > X)
                            {
                                int RemCount = W_;
                                if (TextBuffer.CountItems(i) <= (X + W_))
                                {
                                    RemCount = TextBuffer.CountItems(i) - X;
                                }
                                if (InsDelMode < 10)
                                {
                                    for (int ii = 0; ii < RemCount; ii++)
                                    {
                                        TextBuffer.Get(i, X + ii);
                                        UndoBufferItem_.X.Add(X + ii);
                                        UndoBufferItem_.Y.Add(i);
                                        UndoBufferItem_.ItemOld.Add(TextBuffer.CopyItemObj());
                                        UndoBufferItem_.ItemNew.Add(new AnsiLineOccupyItem());
                                        UndoBufferItem_.OpParams.Add(null);
                                    }
                                }
                                if (ToggleDrawText)
                                {
                                    TextBuffer.BlankChar();
                                    TextBuffer.Append(i, RemCount);
                                    TextBuffer.Delete(i, X, RemCount);
                                }
                                if (ToggleDrawColo)
                                {
                                    ColoBuffer.BlankChar();
                                    ColoBuffer.Append(i, RemCount);
                                    ColoBuffer.Delete(i, X, RemCount);
                                }
                                TextBuffer.Trim(i);
                                ColoBuffer.Trim(i);
                            }
                        }
                    }
                    break;
                case 1:
                case 11:
                    if (Y < TextBuffer.CountLines())
                    {
                        for (int i = Y; i < TextBuffer.CountLines(); i++)
                        {
                            TextBuffer.BlankChar();
                            TextBuffer.PadRight(i, X + W_ + 1);
                            ColoBuffer.BlankChar();
                            ColoBuffer.PadRight(i, X + W_ + 1);
                        }
                        if (InsDelMode < 10)
                        {
                            for (int i = Y; i < (Y + H_); i++)
                            {
                                for (int ii = X; ii < (X + W_); ii++)
                                {
                                    if (TextBuffer.CountLines() > i)
                                    {
                                        if (TextBuffer.CountItems(i) > ii)
                                        {
                                            TextBuffer.Get(i, ii);
                                            UndoBufferItem_.X.Add(ii);
                                            UndoBufferItem_.Y.Add(i);
                                            UndoBufferItem_.ItemOld.Add(TextBuffer.CopyItemObj());
                                            UndoBufferItem_.ItemNew.Add(new AnsiLineOccupyItem());
                                            UndoBufferItem_.OpParams.Add(null);
                                        }
                                    }
                                }
                            }
                        }
                        for (int i = Y; i < TextBuffer.CountLines(); i++)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer.Delete(i, X, W_);
                            }
                            if (ToggleDrawColo)
                            {
                                ColoBuffer.Delete(i, X, W_);
                            }
                            if (ToggleDrawText)
                            {
                                if ((i + H_) < TextBuffer.CountLines())
                                {
                                    TempBuffer.Clear();
                                    TempBuffer.AppendLineCopy(TextBuffer, i + H_);
                                    TempBuffer.Crop(0, X, W_);
                                    TextBuffer.Insert(i, X, TempBuffer, 0);
                                }
                                else
                                {
                                    TextBuffer.BlankChar();
                                    TextBuffer.Insert(i, X, W_);
                                }
                            }
                            if (ToggleDrawColo)
                            {
                                if ((i + H_) < ColoBuffer.CountLines())
                                {
                                    TempBuffer.Clear();
                                    TempBuffer.AppendLineCopy(ColoBuffer, i + H_);
                                    TempBuffer.Crop(0, X, W_);
                                    ColoBuffer.Insert(i, X, TempBuffer, 0);
                                }
                                else
                                {
                                    ColoBuffer.BlankChar();
                                    ColoBuffer.Insert(i, X, W_);
                                }
                            }

                            TextBuffer.Trim(i);
                            ColoBuffer.Trim(i);
                        }
                    }
                    break;
                case 2:
                case 12:
                    for (int i = 0; i < TextBuffer.CountLines(); i++)
                    {
                        if (TextBuffer.CountItemsTrim(i) > X)
                        {
                            int RemCount = W_;
                            if (TextBuffer.CountItems(i) <= (X + W_))
                            {
                                RemCount = TextBuffer.CountItems(i) - X;
                            }
                            if (InsDelMode < 10)
                            {
                                for (int ii = 0; ii < RemCount; ii++)
                                {
                                    TextBuffer.Get(i, X + ii);
                                    UndoBufferItem_.X.Add(X + ii);
                                    UndoBufferItem_.Y.Add(i);
                                    UndoBufferItem_.ItemOld.Add(TextBuffer.CopyItemObj());
                                    UndoBufferItem_.ItemNew.Add(new AnsiLineOccupyItem());
                                    UndoBufferItem_.OpParams.Add(null);
                                }
                            }
                            if (ToggleDrawText)
                            {
                                TextBuffer.BlankChar();
                                TextBuffer.Append(i, RemCount);
                                TextBuffer.Delete(i, X, RemCount);
                            }
                            if (ToggleDrawColo)
                            {
                                ColoBuffer.BlankChar();
                                ColoBuffer.Append(i, RemCount);
                                ColoBuffer.Delete(i, X, RemCount);
                            }
                            TextBuffer.Trim(i);
                            ColoBuffer.Trim(i);
                        }
                    }
                    break;
                case 3:
                case 13:
                    if (Y < TextBuffer.CountLines())
                    {
                        for (int i = 0; i < H_; i++)
                        {
                            if (TextBuffer.CountLines() > Y)
                            {
                                if (InsDelMode < 10)
                                {
                                    for (int ii = 0; ii < TextBuffer.CountItems(Y); ii++)
                                    {
                                        TextBuffer.Get(Y, ii);
                                        UndoBufferItem_.X.Add(ii);
                                        UndoBufferItem_.Y.Add(Y + i);
                                        UndoBufferItem_.ItemOld.Add(TextBuffer.CopyItemObj());
                                        UndoBufferItem_.ItemNew.Add(new AnsiLineOccupyItem());
                                        UndoBufferItem_.OpParams.Add(null);
                                    }
                                }
                                if (ToggleDrawText)
                                {
                                    TextBuffer.DeleteLine(Y);
                                    TextBuffer.AppendLine();
                                }
                                if (ToggleDrawColo)
                                {
                                    ColoBuffer.DeleteLine(Y);
                                    ColoBuffer.AppendLine();
                                }
                            }
                        }
                        for (int i = Y; i < TextBuffer.CountLines(); i++)
                        {
                            if (TextBuffer.CountItems(i) < ColoBuffer.CountItems(i))
                            {
                                TextBuffer.PadRight(i, ColoBuffer.CountItems(i));
                            }
                            if (ColoBuffer.CountItems(i) < TextBuffer.CountItems(i))
                            {
                                ColoBuffer.PadRight(i, TextBuffer.CountItems(i));
                            }
                        }
                    }
                    break;
                case 4:
                case 14:
                    for (int i = Y; i < (Y + H_); i++)
                    {
                        TextBuffer.BlankChar();
                        if (TextBuffer.CountLines() > i)
                        {
                            if (TextBuffer.CountItemsTrim(i) > X)
                            {
                                TextBuffer.Append(i, W_);
                                ColoBuffer.Append(i, W_);
                                if (ToggleDrawText)
                                {
                                    TempBuffer.Clear();
                                    TempBuffer.AppendLineCopy(TextBuffer, i);
                                    TempBuffer.Crop(0, X, CursorFontW);

                                    TextBuffer.Delete(i, X, CursorFontW);
                                    TextBuffer.Insert(i, X + W_ - CursorFontW, TempBuffer, 0);
                                }
                                if (ToggleDrawColo)
                                {
                                    TempBuffer.Clear();
                                    TempBuffer.AppendLineCopy(ColoBuffer, i);
                                    TempBuffer.Crop(0, X, CursorFontW);

                                    ColoBuffer.Delete(i, X, CursorFontW);
                                    ColoBuffer.Insert(i, X + W_ - CursorFontW, TempBuffer, 0);
                                }
                                TextBuffer.Trim(i);
                                ColoBuffer.Trim(i);
                            }
                        }
                    }
                    break;
                case 5:
                case 15:
                    if (Y < TextBuffer.CountLines())
                    {
                        TextBuffer.BlankChar();
                        ColoBuffer.BlankChar();
                        for (int i = Y; i < (Y + H_); i++)
                        {
                            if (TextBuffer.CountLines() <= i)
                            {
                                TextBuffer.AppendLine();
                            }
                            TextBuffer.PadRight(i, X + W_ + 1);
                            if (ColoBuffer.CountLines() <= i)
                            {
                                ColoBuffer.AppendLine();
                            }
                            ColoBuffer.PadRight(i, X + W_ + 1);
                        }
                        TempBuffer.Clear();
                        TempBuffer.AppendLine();
                        TempBuffer.AppendLine();
                        for (int i = 0; i < CursorFontH; i++)
                        {
                            TempBuffer.AppendLineCopy(TextBuffer, Y + i);
                            TempBuffer.AppendLineCopy(ColoBuffer, Y + i);
                            TempBuffer.Crop((i << 1) + 2, X, W_);
                            TempBuffer.Crop((i << 1) + 3, X, W_);
                        }
                        for (int i = (Y + CursorFontH); i < (Y + H_); i++)
                        {
                            if (ToggleDrawText)
                            {
                                TempBuffer.LineCopy(TextBuffer, i, 0);
                                TempBuffer.Crop(0, X, W_);

                                TextBuffer.Delete(i - CursorFontH, X, W_);
                                TextBuffer.Insert(i - CursorFontH, X, TempBuffer, 0);
                            }
                            if (ToggleDrawColo)
                            {
                                TempBuffer.LineCopy(ColoBuffer, i, 1);
                                TempBuffer.Crop(1, X, W_);

                                ColoBuffer.Delete(i - CursorFontH, X, W_);
                                ColoBuffer.Insert(i - CursorFontH, X, TempBuffer, 1);
                            }
                            TextBuffer.Trim(i - CursorFontH);
                            ColoBuffer.Trim(i - CursorFontH);
                        }
                        for (int i = 0; i < CursorFontH; i++)
                        {
                            if (ToggleDrawText)
                            {
                                TextBuffer.Delete(Y + H_ - CursorFontH + i, X, W_);
                                TextBuffer.Insert(Y + H_ - CursorFontH + i, X, TempBuffer, (i << 1) + 2);
                            }
                            if (ToggleDrawColo)
                            {
                                ColoBuffer.Delete(Y + H_ - CursorFontH + i, X, W_);
                                ColoBuffer.Insert(Y + H_ - CursorFontH + i, X, TempBuffer, (i << 1) + 3);
                            }
                            TextBuffer.Trim(Y + H_ - CursorFontH + i);
                            ColoBuffer.Trim(Y + H_ - CursorFontH + i);
                        }
                    }
                    break;
            }
            if (InsDelMode < 10)
            {
                UndoBufferItem_.X.Add(X);
                UndoBufferItem_.Y.Add(Y);
                UndoBufferItem_.ItemOld.Add(null);
                UndoBufferItem_.ItemNew.Add(null);
                UndoBufferItem_.OpParams.Add(new int[] { InsDelMode + 4, W, H });
                TextBuffer.TrimLines();
                TextDisplay(0);
            }
            TextBuffer.MergeColor(ColoBuffer);
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
            public List<AnsiLineOccupyItem> ItemOld;
            public List<AnsiLineOccupyItem> ItemNew;
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
            UndoBufferItem_.ItemOld = new List<AnsiLineOccupyItem>();
            UndoBufferItem_.ItemNew = new List<AnsiLineOccupyItem>();
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
            TextBuffer.TrimLines();
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
                    if (UndoBufferItem_.ItemOld[i] != null)
                    {
                        CharPut(UndoBufferItem_.X[i], UndoBufferItem_.Y[i], UndoBufferItem_.ItemOld[i].CopyItemObj(), true);
                    }
                    else
                    {
                        if (UndoBufferItem_.OpParams[i][0] < 6)
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
            TextBuffer.TrimLines();
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
                    if (UndoBufferItem_.ItemNew[i] != null)
                    {
                        CharPut(UndoBufferItem_.X[i], UndoBufferItem_.Y[i], UndoBufferItem_.ItemNew[i].CopyItemObj(), true);
                    }
                    else
                    {
                        if (UndoBufferItem_.OpParams[i][0] < 6)
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
            TextBuffer.TrimLines();
            CursorLimit();
            TextDisplay(0);
        }
    }
}
