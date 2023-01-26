using System;
using System.Collections.Generic;
using System.Text;

namespace TextPaint
{
    public class PixelPaint
    {
        public int AnsiColor = 0;


        public int CharX = 0;
        public int CharY = 0;
        public int CharW = 1;
        public int CharH = 1;

        public int SizeX = 0;
        public int SizeY = 0;

        public bool DefaultColor = false;


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
        public int PaintModeN = 0;
        public int PaintModeCount = 0;
        Core C;

        public bool IsCharPaint()
        {
            if (PaintModeN == 0)
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
            SizeX = SizeX / CharW;
            SizeY = SizeY / CharH;
            CharW = PaintMode_[PaintModeN].CharW;
            CharH = PaintMode_[PaintModeN].CharH;
            CharX = 0;
            CharY = 0;
            SizeX = SizeX * CharW;
            SizeY = SizeY * CharH;
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
            PaintModeN = 0;
            PaintModeCount = PaintMode_.Count;
            PaintPencil = false;
            PaintColor = 0;
            SelectPaintMode();
        }

        public void PaintStart()
        {
            PaintPencil = false;
            SelectPaintMode();
        }

        public bool RequestMoveU = false;
        public bool RequestMoveD = false;
        public bool RequestMoveL = false;
        public bool RequestMoveR = false;

        public void MoveCursor(int CurrentX, int CurrentY, int Direction)
        {
            if (Direction < 10)
            {
                RequestMoveU = false;
                RequestMoveD = false;
                RequestMoveL = false;
                RequestMoveR = false;
            }
            switch (Direction)
            {
                case 0: // Up
                case 10: // Up
                    if (CharY > 0)
                    {
                        CharY--;
                    }
                    else
                    {
                        if (CurrentY > 0)
                        {
                            CharY = CharH - 1;
                            RequestMoveU = true;
                        }
                    }
                    break;
                case 1: // Down
                case 11: // Down
                    if (CharY < (CharH - 1))
                    {
                        CharY++;
                    }
                    else
                    {
                        CharY = 0;
                        RequestMoveD = true;
                    }
                    break;
                case 2: // Left
                case 12: // Left
                    if (CharX > 0)
                    {
                        CharX--;
                    }
                    else
                    {
                        if (CurrentX > 0)
                        {
                            CharX = CharW - 1;
                            RequestMoveL = true;
                        }
                    }
                    break;
                case 3: // Right
                case 13: // Right
                    if (CharX < (CharW - 1))
                    {
                        CharX++;
                    }
                    else
                    {
                        CharX = 0;
                        RequestMoveR = true;
                    }
                    break;
                case 4: // Up right
                    MoveCursor(CurrentX, CurrentY, 10);
                    MoveCursor(CurrentX, CurrentY, 13);
                    break;
                case 5: // Down left
                    MoveCursor(CurrentX, CurrentY, 11);
                    MoveCursor(CurrentX, CurrentY, 12);
                    break;
                case 6: // Up left
                    MoveCursor(CurrentX, CurrentY, 10);
                    MoveCursor(CurrentX, CurrentY, 12);
                    break;
                case 7: // Down right
                    MoveCursor(CurrentX, CurrentY, 11);
                    MoveCursor(CurrentX, CurrentY, 13);
                    break;
            }
        }

        void SwapCursorsX()
        {
            int X = C.CursorX * CharW + CharX + SizeX;
            if (X < 0)
            {
                SizeX -= X;
                X = 0;
            }
            C.CursorX = X / CharW;
            CharX = X % CharW;
            SizeX = 0 - SizeX;
        }

        void SwapCursorsY()
        {
            int Y = C.CursorY * CharH + CharY + SizeY;
            if (Y < 0)
            {
                SizeY -= Y;
                Y = 0;
            }
            C.CursorY = Y / CharH;
            CharY = Y % CharH;
            SizeY = 0 - SizeY;
        }

        public void SwapCursors(int RotMode)
        {
            if ((SizeX == 0) && (SizeY != 0))
            {
                SwapCursorsY();
                return;
            }
            if ((SizeX != 0) && (SizeY == 0))
            {
                SwapCursorsX();
                return;
            }
            if ((SizeX != 0) && (SizeY != 0))
            {
                if (((SizeX < 0) && (SizeY < 0)) || ((SizeX > 0) && (SizeY > 0)))
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
                if (((SizeX < 0) && (SizeY > 0)) || ((SizeX > 0) && (SizeY < 0)))
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
            if ((PaintColor == 0) || (PaintColor == 3))
            {
                return 1;
            }
            if ((PaintColor == 1) || (PaintColor == 4))
            {
                return 0;
            }
            if ((PaintColor == 2) || (PaintColor == 5))
            {
                if (GetPixel0(C.CursorX * CharW, C.CursorY * CharH) == 1)
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

        public bool PaintPencil = false;
        public int PaintMoveRoll = 0;
        public int PaintColor = 0;

        public void Paint()
        {
            SetPixel(C.CursorX * CharW + CharX, C.CursorY * CharH + CharY);
        }

        public string GetStatusInfo()
        {
            StringBuilder Sb = new StringBuilder();
            Sb.Append("  ");
            string ModeName = PaintMode_[PaintModeN].Name;
            if (IsCharPaint())
            {
                ModeName = TextWork.CharCode(PaintMode_[0].IntToChar[0], 1) + "/" + TextWork.CharCode(PaintMode_[0].IntToChar[1], 1);
            }
            Sb.Append(DefaultColor ? "Pxl-F" : "Pxl-B");
            if (PaintPencil)
            {
                Sb.Append("* ");
            }
            else
            {
                Sb.Append("  ");
            }
            Sb.Append(ModeName + " ");
            switch (PaintColor)
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
            switch (PaintMoveRoll)
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
            return C.ColoGet(X / CharW, Y / CharH, true);
        }

        public int GetPixel(int X, int Y)
        {
            PxlNum = 0;
            PxlBit = 1 << (((Y % CharH) * CharW) + (X % CharW));
            int CharNum = C.CharGet(X / CharW, Y / CharH, false);
            if ((CharNum < 0) || (X < 0) || (Y < 0))
            {
                return -1;
            }
            if (DefaultColor)
            {
                PxlNum = (1 << (CharH * CharW)) - 1;
            }
            if (PaintMode_[PaintModeN].CharToInt.ContainsKey(CharNum))
            {
                PxlNum = PaintMode_[PaintModeN].CharToInt[CharNum];
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
            if ((PaintColor == 1) || (PaintColor == 4) || (((PaintColor == 2) || (PaintColor == 5)) && (G == 1)))
            {
                PxlNum = PxlNum & (0xFFFFFF - PxlBit);
            }
            if ((PaintColor == 0) || (PaintColor == 3) || (((PaintColor == 2) || (PaintColor == 5)) && (G == 0)))
            {
                PxlNum = PxlNum | PxlBit;
            }
            int NewChar = PaintMode_[PaintModeN].IntToChar[PxlNum];
            int NewColo = AnsiColor;
            if (!C.ToggleDrawText)
            {
                NewChar = C.CharGet(X / CharW, Y / CharH, true);
            }
            if (!C.ToggleDrawColo)
            {
                NewColo = C.ColoGet(X / CharW, Y / CharH, true);
            }
            C.CharPut(X / CharW, Y / CharH, NewChar, NewColo);
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
            int NewChar = PaintMode_[PaintModeN].IntToChar[PxlNum];
            int NewColo = AnsiC;
            if (!C.ToggleDrawText)
            {
                NewChar = C.CharGet(X / CharW, Y / CharH, true);
            }
            if (!C.ToggleDrawColo)
            {
                NewColo = C.ColoGet(X / CharW, Y / CharH, true);
            }
            C.CharPut(X / CharW, Y / CharH, NewChar, NewColo);
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
            int X0 = C.CursorX * CharW + CharX;
            int Y0 = C.CursorY * CharH + CharY;
            int X1 = C.CursorX * CharW + CharX + SizeX;
            int Y1 = C.CursorY * CharH + CharY + SizeY;
            int DX = X1 - X0 + 1;
            int DY = Y1 - Y0 + 1;
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
                for (I = 0; I < DX; I++)
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
                for (I = 0; I < DY; I++)
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
            int X0 = C.CursorX * CharW + CharX;
            int Y0 = C.CursorY * CharH + CharY;
            int X1 = C.CursorX * CharW + CharX + SizeX;
            int Y1 = C.CursorY * CharH + CharY + SizeY;
            if (X1 < X0) { int _ = X0; X0 = X1; X1 = _; }
            if (Y1 < Y0) { int _ = Y0; Y0 = Y1; Y1 = _; }
            if (PaintColor >= 3)
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
            bool Fill = (PaintColor >= 3);
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
            int X0 = C.CursorX * CharW + CharX;
            int Y0 = C.CursorY * CharH + CharY;
            int X1 = C.CursorX * CharW + CharX + SizeX;
            int Y1 = C.CursorY * CharH + CharY + SizeY;
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
            int X0 = C.CursorX * CharW + CharX;
            int Y0 = C.CursorY * CharH + CharY;

            int FillColor = GetPixel0(X0, Y0);

            if (FillColor == 0)
            {
                if ((PaintColor == 1) || (PaintColor == 4))
                {
                    return;
                }
            }
            if (FillColor == 1)
            {
                if ((PaintColor == 0) || (PaintColor == 3))
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
            int X0 = C.CursorX * CharW + CharX;
            int Y0 = C.CursorY * CharH + CharY;
            int X1 = C.CursorX * CharW + CharX + SizeX;
            int Y1 = C.CursorY * CharH + CharY + SizeY;
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
                                Temp1 = C.CharGet(i_X, Y0 + i_Y, true);
                                Temp2 = C.CharGet(i_X, Y1 - i_Y, true);
                                Temp1C = C.ColoGet(i_X, Y0 + i_Y, true);
                                Temp2C = C.ColoGet(i_X, Y1 - i_Y, true);
                                C.CharPut(i_X, Y0 + i_Y, Temp2, Temp2C);
                                C.CharPut(i_X, Y1 - i_Y, Temp1, Temp1C);
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
                                Temp1 = C.CharGet(X0 + i_X, i_Y, true);
                                Temp2 = C.CharGet(X1 - i_X, i_Y, true);
                                Temp1C = C.ColoGet(X0 + i_X, i_Y, true);
                                Temp2C = C.ColoGet(X1 - i_X, i_Y, true);
                                C.CharPut(X0 + i_X, i_Y, Temp2, Temp2C);
                                C.CharPut(X1 - i_X, i_Y, Temp1, Temp1C);
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
                                    Temp1 = C.CharGet(X0 + i_X, Y0 + i_Y, true);
                                    Temp2 = C.CharGet(X1 - i_Y, Y0 + i_X, true);
                                    Temp3 = C.CharGet(X1 - i_X, Y1 - i_Y, true);
                                    Temp4 = C.CharGet(X0 + i_Y, Y1 - i_X, true);
                                    Temp1C = C.ColoGet(X0 + i_X, Y0 + i_Y, true);
                                    Temp2C = C.ColoGet(X1 - i_Y, Y0 + i_X, true);
                                    Temp3C = C.ColoGet(X1 - i_X, Y1 - i_Y, true);
                                    Temp4C = C.ColoGet(X0 + i_Y, Y1 - i_X, true);
                                    C.CharPut(X0 + i_X, Y0 + i_Y, Temp2, Temp2C);
                                    C.CharPut(X1 - i_Y, Y0 + i_X, Temp3, Temp3C);
                                    C.CharPut(X1 - i_X, Y1 - i_Y, Temp4, Temp4C);
                                    C.CharPut(X0 + i_Y, Y1 - i_X, Temp1, Temp1C);
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
                                    Temp1 = C.CharGet(X0 + i_X, Y0 + i_Y, true);
                                    Temp2 = C.CharGet(X1 - i_Y, Y0 + i_X, true);
                                    Temp3 = C.CharGet(X1 - i_X, Y1 - i_Y, true);
                                    Temp4 = C.CharGet(X0 + i_Y, Y1 - i_X, true);
                                    Temp1C = C.ColoGet(X0 + i_X, Y0 + i_Y, true);
                                    Temp2C = C.ColoGet(X1 - i_Y, Y0 + i_X, true);
                                    Temp3C = C.ColoGet(X1 - i_X, Y1 - i_Y, true);
                                    Temp4C = C.ColoGet(X0 + i_Y, Y1 - i_X, true);
                                    C.CharPut(X0 + i_X, Y0 + i_Y, Temp4, Temp4C);
                                    C.CharPut(X1 - i_Y, Y0 + i_X, Temp1, Temp1C);
                                    C.CharPut(X1 - i_X, Y1 - i_Y, Temp2, Temp2C);
                                    C.CharPut(X0 + i_Y, Y1 - i_X, Temp3, Temp3C);
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
            int X0 = C.CursorX * CharW + CharX;
            int Y0 = C.CursorY * CharH + CharY;
            int X1 = C.CursorX * CharW + CharX + SizeX;
            int Y1 = C.CursorY * CharH + CharY + SizeY;
            if (X1 < X0) { int _ = X0; X0 = X1; X1 = _; }
            if (Y1 < Y0) { int _ = Y0; Y0 = Y1; Y1 = _; }
            if (PaintMoveRoll == 4)
            {
                PaintFlipRot(Direction);
                return;
            }
            int[] PixelEdge = null;
            int[] PixelEdg_ = null;
            switch (Direction)
            {
                case 0: // Up
                    PixelEdge = new int[X1 - X0 + 1];
                    PixelEdg_ = new int[X1 - X0 + 1];
                    if (IsCharPaint())
                    {
                        for (int i = X0; i <= X1; i++)
                        {
                            PixelEdge[i - X0] = C.CharGet(i, Y0, true);
                            PixelEdg_[i - X0] = C.ColoGet(i, Y0, true);
                        }
                        for (int ii = Y0; ii < Y1; ii++)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                C.CharPut(i, ii, C.CharGet(i, ii + 1, true), C.ColoGet(i, ii + 1, true));
                            }
                        }
                        if (PaintMoveRoll == 1)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                C.CharPut(i, Y1, PixelEdge[i - X0], PixelEdg_[i - X0]);
                            }
                        }
                    }
                    else
                    {
                        for (int i = X0; i <= X1; i++)
                        {
                            PixelEdge[i - X0] = GetPixel0(i, Y0);
                            PixelEdg_[i - X0] = GetPxlCo0(i, Y0);
                        }
                        for (int ii = Y0; ii < Y1; ii++)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                SetPixel(i, ii, GetPixel0(i, ii + 1), GetPxlCo0(i, ii + 1));
                            }
                        }
                        if (PaintMoveRoll == 1)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                SetPixel(i, Y1, PixelEdge[i - X0], PixelEdg_[i - X0]);
                            }
                        }
                    }
                    if (PaintMoveRoll == 2)
                    {
                        for (int i = X0; i <= X1; i++)
                        {
                            SetPixel(i, Y1, 0, AnsiColor);
                        }
                    }
                    if (PaintMoveRoll == 3)
                    {
                        for (int i = X0; i <= X1; i++)
                        {
                            SetPixel(i, Y1, 1, AnsiColor);
                        }
                    }
                    break;
                case 1: // Down
                    PixelEdge = new int[X1 - X0 + 1];
                    PixelEdg_ = new int[X1 - X0 + 1];
                    if (IsCharPaint())
                    {
                        for (int i = X0; i <= X1; i++)
                        {
                            PixelEdge[i - X0] = C.CharGet(i, Y1, true);
                            PixelEdg_[i - X0] = C.ColoGet(i, Y1, true);
                        }
                        for (int ii = Y1; ii > Y0; ii--)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                C.CharPut(i, ii, C.CharGet(i, ii - 1, true), C.ColoGet(i, ii - 1, true));
                            }
                        }
                        if (PaintMoveRoll == 1)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                C.CharPut(i, Y0, PixelEdge[i - X0], PixelEdg_[i - X0]);
                            }
                        }
                    }
                    else
                    {
                        for (int i = X0; i <= X1; i++)
                        {
                            PixelEdge[i - X0] = GetPixel0(i, Y1);
                            PixelEdg_[i - X0] = GetPxlCo0(i, Y1);
                        }
                        for (int ii = Y1; ii > Y0; ii--)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                SetPixel(i, ii, GetPixel0(i, ii - 1), GetPxlCo0(i, ii - 1));
                            }
                        }
                        if (PaintMoveRoll == 1)
                        {
                            for (int i = X0; i <= X1; i++)
                            {
                                SetPixel(i, Y0, PixelEdge[i - X0], PixelEdg_[i - X0]);
                            }
                        }
                    }
                    if (PaintMoveRoll == 2)
                    {
                        for (int i = X0; i <= X1; i++)
                        {
                            SetPixel(i, Y0, 0, AnsiColor);
                        }
                    }
                    if (PaintMoveRoll == 3)
                    {
                        for (int i = X0; i <= X1; i++)
                        {
                            SetPixel(i, Y0, 1, AnsiColor);
                        }
                    }
                    break;
                case 2: // Left
                    PixelEdge = new int[Y1 - Y0 + 1];
                    PixelEdg_ = new int[Y1 - Y0 + 1];
                    if (IsCharPaint())
                    {
                        for (int i = Y0; i <= Y1; i++)
                        {
                            PixelEdge[i - Y0] = C.CharGet(X0, i, true);
                            PixelEdg_[i - Y0] = C.ColoGet(X0, i, true);
                        }
                        for (int ii = X0; ii < X1; ii++)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                C.CharPut(ii, i, C.CharGet(ii + 1, i, true), C.ColoGet(ii + 1, i, true));
                            }
                        }
                        if (PaintMoveRoll == 1)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                C.CharPut(X1, i, PixelEdge[i - Y0], PixelEdg_[i - Y0]);
                            }
                        }
                    }
                    else
                    {
                        for (int i = Y0; i <= Y1; i++)
                        {
                            PixelEdge[i - Y0] = GetPixel0(X0, i);
                            PixelEdg_[i - Y0] = GetPxlCo0(X0, i);
                        }
                        for (int ii = X0; ii < X1; ii++)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                SetPixel(ii, i, GetPixel0(ii + 1, i), GetPxlCo0(ii + 1, i));
                            }
                        }
                        if (PaintMoveRoll == 1)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                SetPixel(X1, i, PixelEdge[i - Y0], PixelEdg_[i - Y0]);
                            }
                        }
                    }
                    if (PaintMoveRoll == 2)
                    {
                        for (int i = Y0; i <= Y1; i++)
                        {
                            SetPixel(X1, i, 0, AnsiColor);
                        }
                    }
                    if (PaintMoveRoll == 3)
                    {
                        for (int i = Y0; i <= Y1; i++)
                        {
                            SetPixel(X1, i, 1, AnsiColor);
                        }
                    }
                    break;
                case 3: // Right
                    PixelEdge = new int[Y1 - Y0 + 1];
                    PixelEdg_ = new int[Y1 - Y0 + 1];
                    if (IsCharPaint())
                    {
                        for (int i = Y0; i <= Y1; i++)
                        {
                            PixelEdge[i - Y0] = C.CharGet(X1, i, true);
                            PixelEdg_[i - Y0] = C.ColoGet(X1, i, true);
                        }
                        for (int ii = X1; ii > X0; ii--)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                C.CharPut(ii, i, C.CharGet(ii - 1, i, true), C.ColoGet(ii - 1, i, true));
                            }
                        }
                        if (PaintMoveRoll == 1)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                C.CharPut(X0, i, PixelEdge[i - Y0], PixelEdg_[i - Y0]);
                            }
                        }
                    }
                    else
                    {
                        for (int i = Y0; i <= Y1; i++)
                        {
                            PixelEdge[i - Y0] = GetPixel0(X1, i);
                            PixelEdg_[i - Y0] = GetPxlCo0(X1, i);
                        }
                        for (int ii = X1; ii > X0; ii--)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                SetPixel(ii, i, GetPixel0(ii - 1, i), GetPxlCo0(ii - 1, i));
                            }
                        }
                        if (PaintMoveRoll == 1)
                        {
                            for (int i = Y0; i <= Y1; i++)
                            {
                                SetPixel(X0, i, PixelEdge[i - Y0], PixelEdg_[i - Y0]);
                            }
                        }
                    }
                    if (PaintMoveRoll == 2)
                    {
                        for (int i = Y0; i <= Y1; i++)
                        {
                            SetPixel(X0, i, 0, AnsiColor);
                        }
                    }
                    if (PaintMoveRoll == 3)
                    {
                        for (int i = Y0; i <= Y1; i++)
                        {
                            SetPixel(X0, i, 1, AnsiColor);
                        }
                    }
                    break;
            }
        }

        public void PaintInvert()
        {
            int X0 = C.CursorX * CharW + CharX;
            int Y0 = C.CursorY * CharH + CharY;
            int X1 = C.CursorX * CharW + CharX + SizeX;
            int Y1 = C.CursorY * CharH + CharY + SizeY;
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

        public void ClipboardCopy()
        {
            int X0 = C.CursorX * CharW + CharX;
            int Y0 = C.CursorY * CharH + CharY;
            int X1 = C.CursorX * CharW + CharX + SizeX;
            int Y1 = C.CursorY * CharH + CharY + SizeY;
            if (X1 < X0) { int _ = X0; X0 = X1; X1 = _; }
            if (Y1 < Y0) { int _ = Y0; Y0 = Y1; Y1 = _; }

            Clipboard.TextClipboardClear();
            for (int i_Y = Y0; i_Y <= Y1; i_Y++)
            {
                for (int i_X = X0; i_X <= X1; i_X++)
                {
                    if (GetPixel(i_X, i_Y) == 1)
                    {
                        Clipboard.TextClipboardSet(i_X - X0, i_Y - Y0, PaintMode_[0].IntToChar[1], GetPxlCo(i_X, i_Y));
                    }
                    else
                    {
                        Clipboard.TextClipboardSet(i_X - X0, i_Y - Y0, PaintMode_[0].IntToChar[0], GetPxlCo(i_X, i_Y));
                    }
                }
            }
            Clipboard.SysClipboardSet();
        }

        public void ClipboardPaste()
        {
            int X0 = C.CursorX * CharW + CharX;
            int Y0 = C.CursorY * CharH + CharY;
            int X1 = C.CursorX * CharW + CharX + SizeX;
            int Y1 = C.CursorY * CharH + CharY + SizeY;
            if (X1 < X0) { int _ = X0; X0 = X1; X1 = _; }
            if (Y1 < Y0) { int _ = Y0; Y0 = Y1; Y1 = _; }

            if (Clipboard.SysClipboardGet())
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
        }

    }
}
