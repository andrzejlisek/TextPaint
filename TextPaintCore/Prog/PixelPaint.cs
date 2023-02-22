using System;
using System.Collections.Generic;
using System.Text;

namespace TextPaint
{
    public class PixelPaint
    {
        public PixelPaintState PPS = new PixelPaintState();

        int AnsiColor = 0;

        public PixelPaint()
        {
        }

        class PaintMode
        {
            public string Name = "";
            public int CharW = 0;
            public int CharH = 0;
            public Dictionary<int, int> IntToChar = new Dictionary<int, int>();
            public Dictionary<int, int> CharToInt = new Dictionary<int, int>();
        }

        List<PaintMode> PaintMode_ = new List<PaintMode>();
        public int PaintModeCount = 0;
        Core C;

        public bool IsCharPaint()
        {
            if (PPS.PaintModeN == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SelectPaintMode()
        {
            PPS.CanvasX = PPS.CanvasX / PPS.CharW;
            PPS.CanvasY = PPS.CanvasY / PPS.CharH;
            PPS.SizeX = PPS.SizeX / PPS.CharW;
            PPS.SizeY = PPS.SizeY / PPS.CharH;
            PPS.CharW = PaintMode_[PPS.PaintModeN].CharW;
            PPS.CharH = PaintMode_[PPS.PaintModeN].CharH;
            PPS.CharX = 0;
            PPS.CharY = 0;
            PPS.CanvasX = PPS.CanvasX * PPS.CharW;
            PPS.CanvasY = PPS.CanvasY * PPS.CharH;
            PPS.SizeX = PPS.SizeX * PPS.CharW;
            PPS.SizeY = PPS.SizeY * PPS.CharH;
        }

        public void Init(ConfigFile CF, Core C_)
        {
            PaintMode PaintModeNew = new PaintMode();
            PaintModeNew.Name = "Char";
            PaintModeNew.CharW = 1;
            PaintModeNew.CharH = 1;

            string[] PixelCharDefault = CF.ParamGetS("PixelChar").Split(',');

            PaintModeNew.IntToChar.Add(0, TextWork.CodeChar(PixelCharDefault[0]));
            PaintModeNew.IntToChar.Add(1, TextWork.CodeChar(PixelCharDefault[1]));
            PaintModeNew.CharToInt.Add(TextWork.CodeChar(PixelCharDefault[0]), 0);
            PaintModeNew.CharToInt.Add(TextWork.CodeChar(PixelCharDefault[1]), 1);
            PaintMode_.Add(PaintModeNew);

            C = C_;
            int I = 1;
            string PaintDef = CF.ParamGetS("Pixel_" + I.ToString());
            while (!("".Equals(PaintDef)))
            {
                PaintModeNew = new PaintMode();
                PaintModeNew.Name = PaintDef.Split(',')[0];
                PaintModeNew.CharW = int.Parse(PaintDef.Split(',')[1]);
                PaintModeNew.CharH = int.Parse(PaintDef.Split(',')[2]);
                int II = 0;
                string[] CharVals = CF.ParamGetS("Pixel_" + I.ToString() + "_" + II).Split(',');
                int CharValN = 0;
                while ((CharVals.Length > 0) && (!("".Equals(CharVals[0]))))
                {
                    for (int III = 0; III < CharVals.Length; III++)
                    {
                        int TempCharCode = TextWork.CodeChar(CharVals[III]);
                        PaintModeNew.IntToChar.Add(CharValN, TempCharCode);
                        if (!PaintModeNew.CharToInt.ContainsKey(TempCharCode))
                        {
                            PaintModeNew.CharToInt.Add(TempCharCode, CharValN);
                        }
                        CharValN++;
                    }
                    II++;
                    CharVals = CF.ParamGetS("Pixel_" + I.ToString() + "_" + II).Split(',');
                }
                I++;
                PaintDef = CF.ParamGetS("Pixel_" + I.ToString());
                PaintMode_.Add(PaintModeNew);

            }
            PPS.PaintModeN = 0;
            PaintModeCount = PaintMode_.Count;
            PPS.PaintPencil = false;
            PPS.PaintColor = 0;
            SelectPaintMode();
        }

        public void PaintStart()
        {
            PPS.CanvasXBase = C.CursorXBase();
            PPS.CanvasYBase = C.CursorYBase();
            PPS.FontW = C.CursorFontW;
            PPS.FontH = C.CursorFontH;
            PPS.CanvasX = ((C.CursorX - PPS.CanvasXBase) * PPS.CharW) / PPS.FontW;
            PPS.CanvasY = ((C.CursorY - PPS.CanvasYBase) * PPS.CharH) / PPS.FontH;
            PPS.PaintPencil = false;
            SelectPaintMode();
        }

        public int GetCursorPosXSize()
        {
            return PPS.CanvasXBase + ((PPS.CanvasX + PPS.SizeX) / PPS.CharW) * PPS.FontW;
        }

        public int GetCursorPosYSize()
        {
            return PPS.CanvasYBase + ((PPS.CanvasY + PPS.SizeY) / PPS.CharH) * PPS.FontH;
        }

        public int GetCursorPosX()
        {
            return PPS.CanvasXBase + ((PPS.CanvasX - PPS.CharX) / PPS.CharW) * PPS.FontW;
        }

        public int GetCursorPosY()
        {
            return PPS.CanvasYBase + ((PPS.CanvasY - PPS.CharY) / PPS.CharH) * PPS.FontH;
        }

        int CharGet(int X, int Y, bool Space)
        {
            return C.CharGet((X / PPS.CharW) * PPS.FontW + PPS.CanvasXBase, (Y / PPS.CharH) * PPS.FontH + PPS.CanvasYBase, Space, false);
        }

        int ColoGet(int X, int Y, bool Space)
        {
            return C.ColoGet((X / PPS.CharW) * PPS.FontW + PPS.CanvasXBase, (Y / PPS.CharH) * PPS.FontH + PPS.CanvasYBase, Space, false);
        }

        void CharPut(int X, int Y, int Ch, int Col)
        {
            C.CharPut((X / PPS.CharW) * PPS.FontW + PPS.CanvasXBase, (Y / PPS.CharH) * PPS.FontH + PPS.CanvasYBase, Ch, Col, 0, false);
        }

        public void MoveCursor(int Direction)
        {
            switch (Direction)
            {
                case 0: // Up
                case 10: // Up
                    if (PPS.CanvasY > 0)
                    {
                        PPS.CanvasY--;
                        if (PPS.CharY > 0)
                        {
                            PPS.CharY--;
                        }
                        else
                        {
                            PPS.CharY = PPS.CharH - 1;
                        }
                    }
                    break;
                case 1: // Down
                case 11: // Down
                    PPS.CanvasY++;
                    if (PPS.CharY < (PPS.CharH - 1))
                    {
                        PPS.CharY++;
                    }
                    else
                    {
                        PPS.CharY = 0;
                    }
                    break;
                case 2: // Left
                case 12: // Left
                    if (PPS.CanvasX > 0)
                    {
                        PPS.CanvasX--;
                        if (PPS.CharX > 0)
                        {
                            PPS.CharX--;
                        }
                        else
                        {
                            PPS.CharX = PPS.CharW - 1;
                        }
                    }
                    break;
                case 3: // Right
                case 13: // Right
                    PPS.CanvasX++;
                    if (PPS.CharX < (PPS.CharW - 1))
                    {
                        PPS.CharX++;
                    }
                    else
                    {
                        PPS.CharX = 0;
                    }
                    break;
                case 4: // Up right
                    MoveCursor(10);
                    MoveCursor(13);
                    break;
                case 5: // Down left
                    MoveCursor(11);
                    MoveCursor(12);
                    break;
                case 6: // Up left
                    MoveCursor(10);
                    MoveCursor(12);
                    break;
                case 7: // Down right
                    MoveCursor(11);
                    MoveCursor(13);
                    break;
            }
        }

        void SwapCursorsX()
        {
            PPS.CanvasX = PPS.CanvasX + PPS.SizeX;
            PPS.CharX = PPS.CanvasX % PPS.CharW;
            PPS.SizeX = 0 - PPS.SizeX;
            if (PPS.CanvasX < 0)
            {
                PPS.CanvasX = 0;
            }
        }

        void SwapCursorsY()
        {
            PPS.CanvasY = PPS.CanvasY + PPS.SizeY;
            PPS.CharY = PPS.CanvasY % PPS.CharH;
            PPS.SizeY = 0 - PPS.SizeY;
            if (PPS.CanvasY < 0)
            {
                PPS.CanvasY = 0;
            }
        }

        public void SwapCursors(int RotMode)
        {
            if ((PPS.SizeX == 0) && (PPS.SizeY != 0))
            {
                SwapCursorsY();
                return;
            }
            if ((PPS.SizeX != 0) && (PPS.SizeY == 0))
            {
                SwapCursorsX();
                return;
            }
            if ((PPS.SizeX != 0) && (PPS.SizeY != 0))
            {
                if (((PPS.SizeX < 0) && (PPS.SizeY < 0)) || ((PPS.SizeX > 0) && (PPS.SizeY > 0)))
                {
                    if (RotMode > 0)
                    {
                        SwapCursorsX();
                    }
                    else
                    {
                        SwapCursorsY();
                    }
                    return;
                }
                if (((PPS.SizeX < 0) && (PPS.SizeY > 0)) || ((PPS.SizeX > 0) && (PPS.SizeY < 0)))
                {
                    if (RotMode > 0)
                    {
                        SwapCursorsY();
                    }
                    else
                    {
                        SwapCursorsX();
                    }
                    return;
                }
            }
        }

        public int CustomCharColor()
        {
            if ((PPS.PaintColor == 0) || (PPS.PaintColor == 3))
            {
                return 1;
            }
            if ((PPS.PaintColor == 1) || (PPS.PaintColor == 4))
            {
                return 0;
            }
            if ((PPS.PaintColor == 2) || (PPS.PaintColor == 5))
            {
                if (GetPixel0(PPS.CanvasX, PPS.CanvasY) == 1)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            return 0;
        }

        public void CustomCharSet(int C)
        {
            PaintMode_[0].IntToChar[CustomCharColor()] = C;
            PaintMode_[0].CharToInt.Clear();
            PaintMode_[0].CharToInt.Add(PaintMode_[0].IntToChar[0], 0);
            PaintMode_[0].CharToInt.Add(PaintMode_[0].IntToChar[1], 1);
        }

        public int CustomCharGet()
        {
            return PaintMode_[0].IntToChar[CustomCharColor()];
        }

        public void Paint()
        {
            SetPixel(PPS.CanvasX, PPS.CanvasY);
        }

        public string GetStatusInfo()
        {
            StringBuilder Sb = new StringBuilder();
            Sb.Append("  ");
            string ModeName = PaintMode_[PPS.PaintModeN].Name;
            if (IsCharPaint())
            {
                ModeName = TextWork.CharCode(PaintMode_[0].IntToChar[0], 1) + "/" + TextWork.CharCode(PaintMode_[0].IntToChar[1], 1);
            }
            Sb.Append(PPS.DefaultColor ? "Pxl-F" : "Pxl-B");
            if (PPS.PaintPencil)
            {
                Sb.Append("* ");
            }
            else
            {
                Sb.Append("  ");
            }
            Sb.Append(ModeName + " ");
            switch (PPS.PaintColor)
            {
                case 0:
                    Sb.Append("Fore-H");
                    break;
                case 1:
                    Sb.Append("Back-H");
                    break;
                case 2:
                    Sb.Append("Nega-H");
                    break;
                case 3:
                    Sb.Append("Fore-F");
                    break;
                case 4:
                    Sb.Append("Back-F");
                    break;
                case 5:
                    Sb.Append("Nega-F");
                    break;
            }
            switch (PPS.PaintMoveRoll)
            {
                case 0:
                    Sb.Append(" Repeat");
                    break;
                case 1:
                    Sb.Append(" Roll");
                    break;
                case 2:
                    Sb.Append(" Back");
                    break;
                case 3:
                    Sb.Append(" Fore");
                    break;
                case 4:
                    Sb.Append(" FlipRot");
                    break;
            }
            return Sb.ToString();
        }

        public int PxlNum = 0;
        public int PxlBit = 0;

        public int GetPxlCo0(int X, int Y)
        {
            int PxlC = GetPxlCo(X, Y);
            if (PxlC < 0)
            {
                return 0;
            }
            return PxlC;
        }

        public int GetPixel0(int X, int Y)
        {
            int PxlC = GetPixel(X, Y);
            if (PxlC < 0)
            {
                return 0;
            }
            return PxlC;
        }

        public int GetPxlCo(int X, int Y)
        {
            return ColoGet(X, Y, true);
        }

        public int GetPixel(int X, int Y)
        {
            PxlNum = 0;
            PxlBit = 1 << (((Y % PPS.CharH) * PPS.CharW) + (X % PPS.CharW));
            int CharNum = CharGet(X, Y, false);
            if ((CharNum < 0) || (X < 0) || (Y < 0))
            {
                return -1;
            }
            if (PPS.DefaultColor)
            {
                PxlNum = (1 << (PPS.CharH * PPS.CharW)) - 1;
            }
            if (PaintMode_[PPS.PaintModeN].CharToInt.ContainsKey(CharNum))
            {
                PxlNum = PaintMode_[PPS.PaintModeN].CharToInt[CharNum];
            }
            if ((PxlNum & PxlBit) > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public void SetPixel(int X, int Y)
        {
            if ((X < 0) || (Y < 0))
            {
                return;
            }
            int G = GetPixel0(X, Y);
            if ((PPS.PaintColor == 1) || (PPS.PaintColor == 4) || (((PPS.PaintColor == 2) || (PPS.PaintColor == 5)) && (G == 1)))
            {
                PxlNum = PxlNum & (0xFFFFFF - PxlBit);
            }
            if ((PPS.PaintColor == 0) || (PPS.PaintColor == 3) || (((PPS.PaintColor == 2) || (PPS.PaintColor == 5)) && (G == 0)))
            {
                PxlNum = PxlNum | PxlBit;
            }
            int NewChar = PaintMode_[PPS.PaintModeN].IntToChar[PxlNum];
            int NewColo = AnsiColor;
            if (!C.ToggleDrawText)
            {
                NewChar = CharGet(X, Y, true);
            }
            if (!C.ToggleDrawColo)
            {
                NewColo = ColoGet(X, Y, true);
            }
            CharPut(X, Y, NewChar, NewColo);
        }

        public void SetPixel(int X, int Y, int PxlColor, int AnsiC)
        {
            if ((X < 0) || (Y < 0))
            {
                return;
            }
            int G = GetPixel0(X, Y);
            if (PxlColor < 0)
            {
                PxlColor = 0;
            }
            if (PxlColor == 1)
            {
                PxlNum = PxlNum | PxlBit;
            }
            if (PxlColor == 0)
            {
                PxlNum = PxlNum & (0xFFFFFF - PxlBit);
            }
            int NewChar = PaintMode_[PPS.PaintModeN].IntToChar[PxlNum];
            int NewColo = AnsiC;
            if (!C.ToggleDrawText)
            {
                NewChar = CharGet(X, Y, true);
            }
            if (!C.ToggleDrawColo)
            {
                NewColo = ColoGet(X, Y, true);
            }
            CharPut(X, Y, NewChar, NewColo);
        }


        List<int> PixBufX = new List<int>();
        List<int> PixBufY = new List<int>();

        public void SetPixelBuf(int X, int Y)
        {
            for (int i = 0; i < PixBufX.Count; i++)
            {
                if ((PixBufX[i] == X) && (PixBufY[i] == Y))
                {
                    return;
                }
            }
            PixBufX.Add(X);
            PixBufY.Add(Y);
            SetPixel(X, Y);
        }

        void PixBufClear()
        {
            PixBufX.Clear();
            PixBufY.Clear();
        }

        public void PaintLine()
        {
            int X0 = PPS.CanvasX;
            int Y0 = PPS.CanvasY;
            int X1 = PPS.CanvasX + PPS.SizeX;
            int Y1 = PPS.CanvasY + PPS.SizeY;
            int DX = X1 - X0;
            int DY = Y1 - Y0;
            int IncX = (DX > 0 ? 1 : -1);
            int IncY = (DY > 0 ? 1 : -1);
            int D = 0;
            int DeltaA = 0;
            int DeltaB = 0;
            int X = 0;
            int Y = 0;
            int I;

            if (DX < 0) { DX = 0 - DX; }
            if (DY < 0) { DY = 0 - DY; }

            if (DX > DY)
            {
                D = 2 * DY - DX;
                DeltaA = 2 * DY;
                DeltaB = 2 * DY - 2 * DX;
                X = 0;
                Y = 0;
                for (I = 0; I <= DX; I++)
                {
                    SetPixel(X0 + X, Y0 + Y);
                    if (D > 0)
                    {
                        D = D + DeltaB;
                        X = X + IncX;
                        Y = Y + IncY;
                    }
                    else
                    {
                        D = D + DeltaA;
                        X = X + IncX;
                    }
                }
            }
            else
            {
                D = 2 * DX - DY;
                DeltaA = 2 * DX;
                DeltaB = 2 * DX - 2 * DY;
                X = 0;
                Y = 0;
                for (I = 0; I <= DY; I++)
                {
                    SetPixel(X0 + X, Y0 + Y);
                    if (D > 0)
                    {
                        D = D + DeltaB;
                        X = X + IncX;
                        Y = Y + IncY;
                    }
                    else
                    {
                        D = D + DeltaA;
                        Y = Y + IncY;
                    }
                }
            }
        }

        public void PaintRect()
        {
            int X0 = PPS.CanvasX;
            int Y0 = PPS.CanvasY;
            int X1 = PPS.CanvasX + PPS.SizeX;
            int Y1 = PPS.CanvasY + PPS.SizeY;
            if (X1 < X0) { int _ = X0; X0 = X1; X1 = _; }
            if (Y1 < Y0) { int _ = Y0; Y0 = Y1; Y1 = _; }
            if (PPS.PaintColor >= 3)
            {
                for (int Y = Y0; Y <= Y1; Y++)
                {
                    for (int X = X0; X <= X1; X++)
                    {
                        SetPixel(X, Y);
                    }
                }
            }
            else
            {
                for (int Y = Y0; Y <= Y1; Y++)
                {
                    SetPixel(X0, Y);
                    if (X0 != X1)
                    {
                        SetPixel(X1, Y);
                    }
                }
                for (int X = (X0 + 1); X < X1; X++)
                {
                    SetPixel(X, Y0);
                    if (Y0 != Y1)
                    {
                        SetPixel(X, Y1);
                    }
                }
            }
        }

        void PaintEllipseWork(int X0, int Y0, int RX, int RY, int PX, int PY, bool M1, bool M2)
        {
            bool Fill = (PPS.PaintColor >= 3);
            int RX2 = RX * RX;
            int RY2 = RY * RY;
            int D = 4 * RY2 - 4 * RY * RX2 + RX2;
            int DeltaA = 4 * 3 * RY2;
            int DeltaB = 4 * (3 * RY2 - 2 * RY * RX2 + 2 * RX2);
            int Limit = (RX2 * RX2) / (RX2 + RY2);
            int X = 0;
            int Y = RY;
            bool Working = true;
            while (Working)
            {
                if (M1)
                {
                    if (Fill)
                    {
                        for (int X_ = (0 - X); X_ <= (X + PX); X_++)
                        {
                            SetPixelBuf(X0 + X_, Y0 - Y);
                            SetPixelBuf(X0 + X_, Y0 + Y + PY);
                        }
                    }
                    else
                    {
                        SetPixelBuf(X0 + X + PX, Y0 + Y + PY);
                        SetPixelBuf(X0 - X, Y0 + Y + PY);
                        SetPixelBuf(X0 + X + PX, Y0 - Y);
                        SetPixelBuf(X0 - X, Y0 - Y);
                    }
                }
                if (M2)
                {
                    if (Fill)
                    {
                        for (int Y_ = (0 - Y); Y_ <= (Y + PY); Y_++)
                        {
                            SetPixelBuf(X0 + Y_, Y0 - X);
                            SetPixelBuf(X0 + Y_, Y0 + X + PX);
                        }
                    }
                    else
                    {
                        SetPixelBuf(X0 + Y + PY, Y0 + X + PX);
                        SetPixelBuf(X0 - Y, Y0 + X + PX);
                        SetPixelBuf(X0 + Y + PY, Y0 - X);
                        SetPixelBuf(X0 - Y, Y0 - X);
                    }
                }
                if ((X * X) >= Limit) { Working = false; }
                if (D > 0)
                {
                    D = D + DeltaB;
                    DeltaA = DeltaA + 4 * 2 * RY2;
                    DeltaB = DeltaB + 4 * (2 * RY2 + 2 * RX2);
                    X = X + 1;
                    Y = Y - 1;
                }
                else
                {
                    D = D + DeltaA;
                    DeltaA = DeltaA + 4 * 2 * RY2;
                    DeltaB = DeltaB + 4 * 2 * RY2;
                    X = X + 1;
                }
            }
        }

        public void PaintEllipse()
        {
            int X0 = PPS.CanvasX;
            int Y0 = PPS.CanvasY;
            int X1 = PPS.CanvasX + PPS.SizeX;
            int Y1 = PPS.CanvasY + PPS.SizeY;
            if (X1 < X0) { int _ = X0; X0 = X1; X1 = _; }
            if (Y1 < Y0) { int _ = Y0; Y0 = Y1; Y1 = _; }
            int PX = 0;
            int PY = 0;
            if (((X1 - X0) % 2) == 1) { X1--; PX = 1; }
            if (((Y1 - Y0) % 2) == 1) { Y1--; PY = 1; }
            int X = (X1 + X0) / 2;
            int Y = (Y1 + Y0) / 2;
            int RX = (X1 - X0) / 2;
            int RY = (Y1 - Y0) / 2;
            PixBufClear();
            PaintEllipseWork(X, Y, RX, RY, PX, PY, true, false);
            PaintEllipseWork(X, Y, RY, RX, PY, PX, false, true);
            PixBufClear();
        }

        public void PaintFill()
        {
            int X0 = PPS.CanvasX;
            int Y0 = PPS.CanvasY;

            int FillColor = GetPixel0(X0, Y0);

            if (FillColor == 0)
            {
                if ((PPS.PaintColor == 1) || (PPS.PaintColor == 4))
                {
                    return;
                }
            }
            if (FillColor == 1)
            {
                if ((PPS.PaintColor == 0) || (PPS.PaintColor == 3))
                {
                    return;
                }
            }

            Queue<int> QX = new Queue<int>();
            Queue<int> QY = new Queue<int>();
            int QN = 0;


            QX.Enqueue(X0);
            QY.Enqueue(Y0);
            QN++;

            int X;
            int Y;
            while (QN > 0)
            {
                X = QX.Dequeue();
                Y = QY.Dequeue();
                QN--;

                if (GetPixel(X, Y) == FillColor)
                {
                    SetPixel(X, Y);
                    if (GetPixel(X - 1, Y) == FillColor)
                    {
                        QX.Enqueue(X - 1);
                        QY.Enqueue(Y);
                        QN++;
                    }
                    if (GetPixel(X, Y - 1) == FillColor)
                    {
                        QX.Enqueue(X);
                        QY.Enqueue(Y - 1);
                        QN++;
                    }
                    if (GetPixel(X + 1, Y) == FillColor)
                    {
                        QX.Enqueue(X + 1);
                        QY.Enqueue(Y);
                        QN++;
                    }
                    if (GetPixel(X, Y + 1) == FillColor)
                    {
                        QX.Enqueue(X);
                        QY.Enqueue(Y + 1);
                        QN++;
                    }
                }
            }

        }

        public void PaintFlipRot(int Direction)
        {
            int X0 = PPS.CanvasX;
            int Y0 = PPS.CanvasY;
            int X1 = PPS.CanvasX + PPS.SizeX;
            int Y1 = PPS.CanvasY + PPS.SizeY;
            if (X1 < X0) { int _ = X0; X0 = X1; X1 = _; }
            if (Y1 < Y0) { int _ = Y0; Y0 = Y1; Y1 = _; }
            int Temp1;
            int Temp2;
            int Temp3;
            int Temp4;
            int Temp1C;
            int Temp2C;
            int Temp3C;
            int Temp4C;
            int W = (X1 - X0 + 1);
            if ((W & 1) != 0)
            {
                if (Direction < 2)
                {
                    W--;
                }
                else
                {
                    W++;
                }
            }
            W = W >> 1;
            int H = (Y1 - Y0 + 1);
            if ((H & 1) != 0)
            {
                H--;
            }
            H = H >> 1;

            switch (Direction)
            {
                case 0: // Flip V
                    if (IsCharPaint())
                    {
                        for (int i_X = X0; i_X <= X1; i_X++)
                        {
                            for (int i_Y = 0; i_Y < H; i_Y++)
                            {
                                Temp1 = CharGet(i_X, Y0 + i_Y, true);
                                Temp2 = CharGet(i_X, Y1 - i_Y, true);
                                Temp1C = ColoGet(i_X, Y0 + i_Y, true);
                                Temp2C = ColoGet(i_X, Y1 - i_Y, true);
                                CharPut(i_X, Y0 + i_Y, Temp2, Temp2C);
                                CharPut(i_X, Y1 - i_Y, Temp1, Temp1C);
                            }
                        }
                    }
                    else
                    {
                        for (int i_X = X0; i_X <= X1; i_X++)
                        {
                            for (int i_Y = 0; i_Y < H; i_Y++)
                            {
                                Temp1 = GetPixel0(i_X, Y0 + i_Y);
                                Temp2 = GetPixel0(i_X, Y1 - i_Y);
                                Temp1C = GetPxlCo0(i_X, Y0 + i_Y);
                                Temp2C = GetPxlCo0(i_X, Y1 - i_Y);
                                SetPixel(i_X, Y0 + i_Y, Temp2, Temp2C);
                                SetPixel(i_X, Y1 - i_Y, Temp1, Temp1C);
                            }
                        }
                    }
                    break;
                case 1: // Flip H
                    if (IsCharPaint())
                    {
                        for (int i_Y = Y0; i_Y <= Y1; i_Y++)
                        {
                            for (int i_X = 0; i_X < W; i_X++)
                            {
                                Temp1 = CharGet(X0 + i_X, i_Y, true);
                                Temp2 = CharGet(X1 - i_X, i_Y, true);
                                Temp1C = ColoGet(X0 + i_X, i_Y, true);
                                Temp2C = ColoGet(X1 - i_X, i_Y, true);
                                CharPut(X0 + i_X, i_Y, Temp2, Temp2C);
                                CharPut(X1 - i_X, i_Y, Temp1, Temp1C);
                            }
                        }
                    }
                    else
                    {
                        for (int i_Y = Y0; i_Y <= Y1; i_Y++)
                        {
                            for (int i_X = 0; i_X < W; i_X++)
                            {
                                Temp1 = GetPixel0(X0 + i_X, i_Y);
                                Temp2 = GetPixel0(X1 - i_X, i_Y);
                                Temp1C = GetPxlCo0(X0 + i_X, i_Y);
                                Temp2C = GetPxlCo0(X1 - i_X, i_Y);
                                SetPixel(X0 + i_X, i_Y, Temp2, Temp2C);
                                SetPixel(X1 - i_X, i_Y, Temp1, Temp1C);
                            }
                        }
                    }
                    break;
                case 2: // Rotate L
                    if ((X1 - X0) == (Y1 - Y0))
                    {
                        if (IsCharPaint())
                        {
                            for (int i_Y = 0; i_Y < H; i_Y++)
                            {
                                for (int i_X = 0; i_X < W; i_X++)
                                {
                                    Temp1 = CharGet(X0 + i_X, Y0 + i_Y, true);
                                    Temp2 = CharGet(X1 - i_Y, Y0 + i_X, true);
                                    Temp3 = CharGet(X1 - i_X, Y1 - i_Y, true);
                                    Temp4 = CharGet(X0 + i_Y, Y1 - i_X, true);
                                    Temp1C = ColoGet(X0 + i_X, Y0 + i_Y, true);
                                    Temp2C = ColoGet(X1 - i_Y, Y0 + i_X, true);
                                    Temp3C = ColoGet(X1 - i_X, Y1 - i_Y, true);
                                    Temp4C = ColoGet(X0 + i_Y, Y1 - i_X, true);
                                    CharPut(X0 + i_X, Y0 + i_Y, Temp2, Temp2C);
                                    CharPut(X1 - i_Y, Y0 + i_X, Temp3, Temp3C);
                                    CharPut(X1 - i_X, Y1 - i_Y, Temp4, Temp4C);
                                    CharPut(X0 + i_Y, Y1 - i_X, Temp1, Temp1C);
                                }
                            }
                        }
                        else
                        {
                            for (int i_Y = 0; i_Y < H; i_Y++)
                            {
                                for (int i_X = 0; i_X < W; i_X++)
                                {
                                    Temp1 = GetPixel0(X0 + i_X, Y0 + i_Y);
                                    Temp2 = GetPixel0(X1 - i_Y, Y0 + i_X);
                                    Temp3 = GetPixel0(X1 - i_X, Y1 - i_Y);
                                    Temp4 = GetPixel0(X0 + i_Y, Y1 - i_X);
                                    Temp1C = GetPxlCo0(X0 + i_X, Y0 + i_Y);
                                    Temp2C = GetPxlCo0(X1 - i_Y, Y0 + i_X);
                                    Temp3C = GetPxlCo0(X1 - i_X, Y1 - i_Y);
                                    Temp4C = GetPxlCo0(X0 + i_Y, Y1 - i_X);
                                    SetPixel(X0 + i_X, Y0 + i_Y, Temp2, Temp2C);
                                    SetPixel(X1 - i_Y, Y0 + i_X, Temp3, Temp3C);
                                    SetPixel(X1 - i_X, Y1 - i_Y, Temp4, Temp4C);
                                    SetPixel(X0 + i_Y, Y1 - i_X, Temp1, Temp1C);
                                }
                            }
                        }
                    }
                    break;
                case 3: // Rotate R
                    if ((X1 - X0) == (Y1 - Y0))
                    {
                        if (IsCharPaint())
                        {
                            for (int i_Y = 0; i_Y < H; i_Y++)
                            {
                                for (int i_X = 0; i_X < W; i_X++)
                                {
                                    Temp1 = CharGet(X0 + i_X, Y0 + i_Y, true);
                                    Temp2 = CharGet(X1 - i_Y, Y0 + i_X, true);
                                    Temp3 = CharGet(X1 - i_X, Y1 - i_Y, true);
                                    Temp4 = CharGet(X0 + i_Y, Y1 - i_X, true);
                                    Temp1C = ColoGet(X0 + i_X, Y0 + i_Y, true);
                                    Temp2C = ColoGet(X1 - i_Y, Y0 + i_X, true);
                                    Temp3C = ColoGet(X1 - i_X, Y1 - i_Y, true);
                                    Temp4C = ColoGet(X0 + i_Y, Y1 - i_X, true);
                                    CharPut(X0 + i_X, Y0 + i_Y, Temp4, Temp4C);
                                    CharPut(X1 - i_Y, Y0 + i_X, Temp1, Temp1C);
                                    CharPut(X1 - i_X, Y1 - i_Y, Temp2, Temp2C);
                                    CharPut(X0 + i_Y, Y1 - i_X, Temp3, Temp3C);
                                }
                            }
                        }
                        else
                        {
                            for (int i_Y = 0; i_Y < H; i_Y++)
                            {
                                for (int i_X = 0; i_X < W; i_X++)
                                {
                                    Temp1 = GetPixel0(X0 + i_X, Y0 + i_Y);
                                    Temp2 = GetPixel0(X1 - i_Y, Y0 + i_X);
                                    Temp3 = GetPixel0(X1 - i_X, Y1 - i_Y);
                                    Temp4 = GetPixel0(X0 + i_Y, Y1 - i_X);
                                    Temp1C = GetPxlCo0(X0 + i_X, Y0 + i_Y);
                                    Temp2C = GetPxlCo0(X1 - i_Y, Y0 + i_X);
                                    Temp3C = GetPxlCo0(X1 - i_X, Y1 - i_Y);
                                    Temp4C = GetPxlCo0(X0 + i_Y, Y1 - i_X);
                                    SetPixel(X0 + i_X, Y0 + i_Y, Temp4, Temp4C);
                                    SetPixel(X1 - i_Y, Y0 + i_X, Temp1, Temp1C);
                                    SetPixel(X1 - i_X, Y1 - i_Y, Temp2, Temp2C);
                                    SetPixel(X0 + i_Y, Y1 - i_X, Temp3, Temp3C);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        public void PaintMove(int Direction)
        {
            int X0 = PPS.CanvasX;
            int Y0 = PPS.CanvasY;
            int X1 = PPS.CanvasX + PPS.SizeX;
            int Y1 = PPS.CanvasY + PPS.SizeY;
            if (X1 < X0) { int _ = X0; X0 = X1; X1 = _; }
            if (Y1 < Y0) { int _ = Y0; Y0 = Y1; Y1 = _; }
            if (PPS.PaintMoveRoll == 4)
            {
                PaintFlipRot(Direction);
                return;
            }
            int X0_ = X0;
            int Y0_ = Y0;
            int X1_ = X1;
            int Y1_ = Y1;
            if (IsCharPaint())
            {
                X0 = X0 * PPS.FontW + PPS.CanvasXBase;
                Y0 = Y0 * PPS.FontH + PPS.CanvasYBase;
                X1 = X1 * PPS.FontW + PPS.CanvasXBase;
                Y1 = Y1 * PPS.FontH + PPS.CanvasYBase;
                if ((Direction == 0) || (Direction == 1))
                {
                    X1 = X1 + (PPS.FontW - 1);
                }
                if ((Direction == 2) || (Direction == 3))
                {
                    Y1 = Y1 + (PPS.FontH - 1);
                }
            }
            int[,] PixelEdge = null;
            int[,] PixelEdg_ = null;
            int[,] PixelEdgF = null;
            switch (Direction)
            {
                case 0: // Up
                    PixelEdge = new int[PPS.FontH, X1 - X0 + 1];
                    PixelEdg_ = new int[PPS.FontH, X1 - X0 + 1];
                    PixelEdgF = new int[PPS.FontH, X1 - X0 + 1];
                    if (IsCharPaint())
                    {
                        for (int i = X0; i <= X1; i++)
                        {
                            for (int i_ = 0; i_ < PPS.FontH; i_++)
                            {
                                PixelEdge[i_, i - X0] = C.CharGet(i, Y0 + i_, true, true);
                                PixelEdg_[i_, i - X0] = C.ColoGet(i, Y0 + i_, true, true);
                                PixelEdgF[i_, i - X0] = C.FontGet(i, Y0 + i_);
                            }
                        }
                        for (int ii = Y0; ii < Y1; ii += PPS.FontH)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                for (int i_ = 0; i_ < PPS.FontH; i_++)
                                {
                                    C.CharPut(i, ii + i_, C.CharGet(i, ii + i_ + PPS.FontH, true, true), C.ColoGet(i, ii + i_ + PPS.FontH, true, true), C.FontGet(i, ii + i_ + PPS.FontH), true);
                                }
                            }
                        }
                        if (PPS.PaintMoveRoll == 1)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                for (int i_ = 0; i_ < PPS.FontH; i_++)
                                {
                                    C.CharPut(i, Y1 + i_, PixelEdge[i_, i - X0], PixelEdg_[i_, i - X0], PixelEdgF[i_, i - X0], true);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = X0; i <= X1; i++)
                        {
                            PixelEdge[0, i - X0] = GetPixel0(i, Y0);
                            PixelEdg_[0, i - X0] = GetPxlCo0(i, Y0);
                        }
                        for (int ii = Y0; ii < Y1; ii++)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                SetPixel(i, ii, GetPixel0(i, ii + 1), GetPxlCo0(i, ii + 1));
                            }
                        }
                        if (PPS.PaintMoveRoll == 1)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                SetPixel(i, Y1, PixelEdge[0, i - X0], PixelEdg_[0, i - X0]);
                            }
                        }
                    }
                    if (PPS.PaintMoveRoll == 2)
                    {
                        for (int i = X0_; i <= X1_; i++)
                        {
                            SetPixel(i, Y1_, 0, AnsiColor);
                        }
                    }
                    if (PPS.PaintMoveRoll == 3)
                    {
                        for (int i = X0_; i <= X1_; i++)
                        {
                            SetPixel(i, Y1_, 1, AnsiColor);
                        }
                    }
                    break;
                case 1: // Down
                    PixelEdge = new int[PPS.FontH, X1 - X0 + 1];
                    PixelEdg_ = new int[PPS.FontH, X1 - X0 + 1];
                    PixelEdgF = new int[PPS.FontH, X1 - X0 + 1];
                    if (IsCharPaint())
                    {
                        for (int i = X0; i <= X1; i++)
                        {
                            for (int i_ = 0; i_ < PPS.FontH; i_++)
                            {
                                PixelEdge[i_, i - X0] = C.CharGet(i, Y1 + i_, true, true);
                                PixelEdg_[i_, i - X0] = C.ColoGet(i, Y1 + i_, true, true);
                                PixelEdgF[i_, i - X0] = C.FontGet(i, Y1 + i_);
                            }
                        }
                        for (int ii = Y1; ii > Y0; ii -= PPS.FontH)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                for (int i_ = 0; i_ < PPS.FontH; i_++)
                                {
                                    C.CharPut(i, ii + i_, C.CharGet(i, ii + i_ - PPS.FontH, true, true), C.ColoGet(i, ii + i_ - PPS.FontH, true, true), C.FontGet(i, ii + i_ - PPS.FontH), true);
                                }
                            }
                        }
                        if (PPS.PaintMoveRoll == 1)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                for (int i_ = 0; i_ < PPS.FontH; i_++)
                                {
                                    C.CharPut(i, Y0 + i_, PixelEdge[i_, i - X0], PixelEdg_[i_, i - X0], PixelEdgF[i_, i - X0], true);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = X0; i <= X1; i++)
                        {
                            PixelEdge[0, i - X0] = GetPixel0(i, Y1);
                            PixelEdg_[0, i - X0] = GetPxlCo0(i, Y1);
                        }
                        for (int ii = Y1; ii > Y0; ii--)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                SetPixel(i, ii, GetPixel0(i, ii - 1), GetPxlCo0(i, ii - 1));
                            }
                        }
                        if (PPS.PaintMoveRoll == 1)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                SetPixel(i, Y0, PixelEdge[0, i - X0], PixelEdg_[0, i - X0]);
                            }
                        }
                    }
                    if (PPS.PaintMoveRoll == 2)
                    {
                        for (int i = X0_; i <= X1_; i++)
                        {
                            SetPixel(i, Y0_, 0, AnsiColor);
                        }
                    }
                    if (PPS.PaintMoveRoll == 3)
                    {
                        for (int i = X0_; i <= X1_; i++)
                        {
                            SetPixel(i, Y0_, 1, AnsiColor);
                        }
                    }
                    break;
                case 2: // Left
                    PixelEdge = new int[PPS.FontW, Y1 - Y0 + 1];
                    PixelEdg_ = new int[PPS.FontW, Y1 - Y0 + 1];
                    PixelEdgF = new int[PPS.FontW, Y1 - Y0 + 1];
                    if (IsCharPaint())
                    {
                        for (int i = Y0; i <= Y1; i++)
                        {
                            for (int i_ = 0; i_ < PPS.FontW; i_++)
                            {
                                PixelEdge[i_, i - Y0] = C.CharGet(X0 + i_, i, true, true);
                                PixelEdg_[i_, i - Y0] = C.ColoGet(X0 + i_, i, true, true);
                                PixelEdgF[i_, i - Y0] = C.FontGet(X0 + i_, i);
                            }
                        }
                        for (int ii = X0; ii < X1; ii += PPS.FontW)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                for (int i_ = 0; i_ < PPS.FontW; i_++)
                                {
                                    C.CharPut(ii + i_, i, C.CharGet(ii + i_ + PPS.FontW, i, true, true), C.ColoGet(ii + i_ + PPS.FontW, i, true, true), C.FontGet(ii + i_ + PPS.FontW, i), true);
                                }
                            }
                        }
                        if (PPS.PaintMoveRoll == 1)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                for (int i_ = 0; i_ < PPS.FontW; i_++)
                                {
                                    C.CharPut(X1 + i_, i, PixelEdge[i_, i - Y0], PixelEdg_[i_, i - Y0], PixelEdgF[i_, i - Y0], true);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = Y0; i <= Y1; i++)
                        {
                            PixelEdge[0, i - Y0] = GetPixel0(X0, i);
                            PixelEdg_[0, i - Y0] = GetPxlCo0(X0, i);
                        }
                        for (int ii = X0; ii < X1; ii++)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                SetPixel(ii, i, GetPixel0(ii + 1, i), GetPxlCo0(ii + 1, i));
                            }
                        }
                        if (PPS.PaintMoveRoll == 1)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                SetPixel(X1, i, PixelEdge[0, i - Y0], PixelEdg_[0, i - Y0]);
                            }
                        }
                    }
                    if (PPS.PaintMoveRoll == 2)
                    {
                        for (int i = Y0_; i <= Y1_; i++)
                        {
                            SetPixel(X1_, i, 0, AnsiColor);
                        }
                    }
                    if (PPS.PaintMoveRoll == 3)
                    {
                        for (int i = Y0_; i <= Y1_; i++)
                        {
                            SetPixel(X1_, i, 1, AnsiColor);
                        }
                    }
                    break;
                case 3: // Right
                    PixelEdge = new int[PPS.FontW, Y1 - Y0 + 1];
                    PixelEdg_ = new int[PPS.FontW, Y1 - Y0 + 1];
                    PixelEdgF = new int[PPS.FontW, Y1 - Y0 + 1];
                    if (IsCharPaint())
                    {
                        for (int i = Y0; i <= Y1; i++)
                        {
                            for (int i_ = 0; i_ < PPS.FontW; i_++)
                            {
                                PixelEdge[i_, i - Y0] = C.CharGet(X1 + i_, i, true, true);
                                PixelEdg_[i_, i - Y0] = C.ColoGet(X1 + i_, i, true, true);
                                PixelEdgF[i_, i - Y0] = C.FontGet(X1 + i_, i);
                            }
                        }
                        for (int ii = X1; ii > X0; ii -= PPS.FontW)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                for (int i_ = 0; i_ < PPS.FontW; i_++)
                                {
                                    C.CharPut(ii + i_, i, C.CharGet(ii + i_ - PPS.FontW, i, true, true), C.ColoGet(ii + i_ - PPS.FontW, i, true, true), C.FontGet(ii + i_ - PPS.FontW, i), true);
                                }
                            }
                        }
                        if (PPS.PaintMoveRoll == 1)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                for (int i_ = 0; i_ < PPS.FontW; i_++)
                                {
                                    C.CharPut(X0 + i_, i, PixelEdge[i_, i - Y0], PixelEdg_[i_, i - Y0], PixelEdgF[i_, i - Y0], true);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = Y0; i <= Y1; i++)
                        {
                            PixelEdge[0, i - Y0] = GetPixel0(X1, i);
                            PixelEdg_[0, i - Y0] = GetPxlCo0(X1, i);
                        }
                        for (int ii = X1; ii > X0; ii--)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                SetPixel(ii, i, GetPixel0(ii - 1, i), GetPxlCo0(ii - 1, i));
                            }
                        }
                        if (PPS.PaintMoveRoll == 1)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                SetPixel(X0, i, PixelEdge[0, i - Y0], PixelEdg_[0, i - Y0]);
                            }
                        }
                    }
                    if (PPS.PaintMoveRoll == 2)
                    {
                        for (int i = Y0_; i <= Y1_; i++)
                        {
                            SetPixel(X0_, i, 0, AnsiColor);
                        }
                    }
                    if (PPS.PaintMoveRoll == 3)
                    {
                        for (int i = Y0_; i <= Y1_; i++)
                        {
                            SetPixel(X0_, i, 1, AnsiColor);
                        }
                    }
                    break;
            }
        }

        public void PaintInvert()
        {
            int X0 = PPS.CanvasX;
            int Y0 = PPS.CanvasY;
            int X1 = PPS.CanvasX + PPS.SizeX;
            int Y1 = PPS.CanvasY + PPS.SizeY;
            if (X1 < X0) { int _ = X0; X0 = X1; X1 = _; }
            if (Y1 < Y0) { int _ = Y0; Y0 = Y1; Y1 = _; }

            for (int i_Y = Y0; i_Y <= Y1; i_Y++)
            {
                for (int i_X = X0; i_X <= X1; i_X++)
                {
                    if (GetPixel(i_X, i_Y) == 1)
                    {
                        SetPixel(i_X, i_Y, 0, AnsiColor);
                    }
                    else
                    {
                        SetPixel(i_X, i_Y, 1, AnsiColor);
                    }
                }
            }
        }

        public delegate void ClipboardWorkEvent1Handler(bool Paste);
        public event ClipboardWorkEvent1Handler ClipboardWorkEvent1;
        public delegate void ClipboardWorkEvent2Handler(bool Paste);
        public event ClipboardWorkEvent2Handler ClipboardWorkEvent2;

        public void ClipboardCopy()
        {
            ClipboardCopy_();
        }

        public async void ClipboardCopy_()
        {
            ClipboardWorkEvent1(false);

            int X0 = PPS.CanvasX;
            int Y0 = PPS.CanvasY;
            int X1 = PPS.CanvasX + PPS.SizeX;
            int Y1 = PPS.CanvasY + PPS.SizeY;
            if (X1 < X0) { int _ = X0; X0 = X1; X1 = _; }
            if (Y1 < Y0) { int _ = Y0; Y0 = Y1; Y1 = _; }

            Clipboard.TextClipboardClear();
            for (int i_Y = Y0; i_Y <= Y1; i_Y++)
            {
                for (int i_X = X0; i_X <= X1; i_X++)
                {
                    if (GetPixel(i_X, i_Y) == 1)
                    {
                        Clipboard.TextClipboardSet(i_X - X0, i_Y - Y0, PaintMode_[0].IntToChar[1], GetPxlCo(i_X, i_Y), 0);
                    }
                    else
                    {
                        Clipboard.TextClipboardSet(i_X - X0, i_Y - Y0, PaintMode_[0].IntToChar[0], GetPxlCo(i_X, i_Y), 0);
                    }
                }
            }
            await Clipboard.SysClipboardSet();

            ClipboardWorkEvent2(false);
        }

        public void ClipboardPaste()
        {
            ClipboardPaste_();
        }

        public async void ClipboardPaste_()
        {
            ClipboardWorkEvent1(true);

            int X0 = PPS.CanvasX;
            int Y0 = PPS.CanvasY;
            int X1 = PPS.CanvasX + PPS.SizeX;
            int Y1 = PPS.CanvasY + PPS.SizeY;
            if (X1 < X0) { int _ = X0; X0 = X1; X1 = _; }
            if (Y1 < Y0) { int _ = Y0; Y0 = Y1; Y1 = _; }

            if (await Clipboard.SysClipboardGet())
            {
                for (int i_Y = Y0; i_Y <= Y1; i_Y++)
                {
                    for (int i_X = X0; i_X <= X1; i_X++)
                    {
                        if (Clipboard.TextClipboardGetT(i_X - X0, i_Y - Y0) == PaintMode_[0].IntToChar[1])
                        {
                            SetPixel(i_X, i_Y, 1, Clipboard.TextClipboardGetC(i_X - X0, i_Y - Y0));
                        }
                        else
                        {
                            SetPixel(i_X, i_Y, 0, Clipboard.TextClipboardGetC(i_X - X0, i_Y - Y0));
                        }
                    }
                }
            }

            ClipboardWorkEvent2(true);
        }

    }
}
