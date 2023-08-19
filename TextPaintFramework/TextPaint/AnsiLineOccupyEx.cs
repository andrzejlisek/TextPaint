using System;
using System.Collections.Generic;

namespace TextPaint
{
    public class AnsiLineOccupyEx : AnsiLineOccupy
    {
        public int GetDataChar(int Y, int X)
        {
            Get(Y, X);
            return Item_Char;
        }

        public int GetDataColo(int Y, int X)
        {
            Get(Y, X);
            return Item_ColorF;
        }

        public int GetDataAttr(int Y, int X)
        {
            Get(Y, X);
            return Item_ColorA;
        }

        public int GetDataFont(int Y, int X)
        {
            return 0;
        }

        public void Set_(int Y, int X)
        {
            while(Data.Count <= Y)
            {
                Data.Add(new List<int>());
            }
            PadRightSpace(Y, X + 1);
            Set(Y, X);
        }

        public void Get_(int Y, int X)
        {
            BlankChar();
            if ((Data.Count > Y) && (Y >= 0))
            {
                if ((Data[Y].Count > (X * Factor)) && (X >= 0))
                {
                    Get(Y, X);
                }
            }
        }

        public int CountItemsTrim(int Y)
        {
            ItemTempSave();
            int I = CountItems(Y);
            bool Work = (I > 0);
            if (Work)
            {
                Get(Y, I - 1);
                Work = ((TextWork.SpaceChars.Contains(Item_Char)) && (Item_ColorB < 0) && (Item_ColorF < 0) && (Item_ColorA == 0));
            }
            while (Work)
            {
                I--;
                Work = (I > 0);
                if (Work)
                {
                    Get(Y, I - 1);
                    Work = ((TextWork.SpaceChars.Contains(Item_Char)) && (Item_ColorB < 0) && (Item_ColorF < 0) && (Item_ColorA == 0));
                }
            }
            ItemTempLoad();
            return I;
        }

        public void Trim(int Y)
        {
            ItemTempSave();
            int X = CountItems(Y) - 1;
            if (X >= 0)
            {
                Get(Y, X);
                while ((X >= 0) && (TextWork.SpaceChars.Contains(Item_Char)) && (Item_ColorB < 0) && (Item_ColorF < 0) && (Item_ColorA == 0))
                {
                    Delete(Y, X);
                    X--;
                    if (X < 0)
                    {
                        break;
                    }
                    Get(Y, X);
                }
            }
            ItemTempLoad();
        }

        public void TrimLines()
        {
            int L = CountLines();
            int Y = 0;
            for (Y = 0; Y < L; Y++)
            {
                Trim(Y);
            }
            Y = L - 1;
            while (Y >= 0)
            {
                if (CountItems(Y) == 0)
                {
                    DeleteLine(Y);
                    Y--;
                }
                else
                {
                    break;
                }
            }
        }

        public void DeleteAll()
        {

        }

        public List<int> GetLineString(int Y)
        {
            List<int> Temp = new List<int>();
            for (int X = 0; X < CountItems(Y); X++)
            {
                Get(Y, X);
                Temp.Add(Item_Char);
            }
            return Temp;
        }

        public void SetLineString(int Y, List<int> Text)
        {
            Data[Y].Clear();
            BlankChar();
            for (int i = 0; i < Text.Count; i++)
            {
                Item_Char = Text[i];
                Append(Y);
            }
        }

        public void SetLineString(int Y, string Text)
        {
            Data[Y].Clear();
            List<int> Text_ = TextWork.StrToInt(Text);
            BlankChar();
            for (int i = 0; i < Text_.Count; i++)
            {
                Item_Char = Text_[i];
                Append(Y);
            }
        }

        public void ClearLine(int Y)
        {
            Data[Y].Clear();
        }

        Stack<int> ItemTemp = new Stack<int>();

        void ItemTempSave()
        {
            ItemTemp.Push(Item_Char);
            ItemTemp.Push(Item_ColorB);
            ItemTemp.Push(Item_ColorF);
            ItemTemp.Push(Item_ColorA);
            ItemTemp.Push(Item_FontW);
            ItemTemp.Push(Item_FontH);
        }

        void ItemTempLoad()
        {
            Item_FontH = ItemTemp.Pop();
            Item_FontW = ItemTemp.Pop();
            Item_ColorA = ItemTemp.Pop();
            Item_ColorF = ItemTemp.Pop();
            Item_ColorB = ItemTemp.Pop();
            Item_Char = ItemTemp.Pop();
        }

        public void PadRightTab(int Y, int Size)
        {
            ItemTempSave();
            BlankChar();
            Item_Char = '\t';
            while (CountItems(Y) < Size)
            {
                Append(Y);
            }
            ItemTempLoad();
        }

        public void PadRightSpace(int Y, int Size)
        {
            ItemTempSave();
            BlankChar();
            while (CountItems(Y) < Size)
            {
                Append(Y);
            }
            ItemTempLoad();
        }

        public void PadRight(int Y, int Size)
        {
            while (CountItems(Y) < Size)
            {
                Append(Y);
            }
        }

        public void DeleteLeft(int Y, int Size)
        {
            Size = Size * Factor;
            if (Data[Y].Count > (Size))
            {
                Data[Y].RemoveRange(0, Size);
            }
            else
            {
                Data[Y].Clear();
            }
        }

        public void Crop(int Y, int Start, int Size)
        {
            Start = Start * Factor;
            Size = Size * Factor;

            if (Data[Y].Count > (Start + Size))
            {
                Data[Y].RemoveRange(Start + Size, Data[Y].Count - (Start + Size));
            }
            if (Start > 0)
            {
                if (Data[Y].Count > Start)
                {
                    Data[Y].RemoveRange(0, Start);
                }
                else
                {
                    Data[Y].Clear();
                }
            }
        }

        public void LineCopy(AnsiLineOccupyEx Src, int SrcY, int DstY)
        {
            Data[DstY].Clear();
            for (int i = 0; i < Src.Data[SrcY].Count; i++)
            {
                Data[DstY].Add(Src.Data[SrcY][i]);
            }
        }

        public void AppendLineCopy(AnsiLineOccupyEx Src, int SrcY)
        {
            List<int> Temp = new List<int>();
            for (int i = 0; i < Src.Data[SrcY].Count; i++)
            {
                Temp.Add(Src.Data[SrcY][i]);
            }
            Data.Add(Temp);
        }

        public void DebugTest(int Offset)
        {
            for (int Y = (0 + Offset); Y < (10 + Offset); Y++)
            {
                if (Data.Count > Y)
                {
                    Console.Write((Data[Y].Count / Factor) + " > ");
                    for (int X = 0; X < 10; X++)
                    {
                        if (Data[Y].Count > (X * Factor))
                        {
                            Console.Write(Data[Y][X * Factor] + " ");
                        }
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.Write("EOF > ");
                    Console.WriteLine();
                }
            }
        }

        public void Insert(int Y, int X, int L)
        {
            List<int> DataX = new List<int>();
            DataX.Add(Item_Char);
            DataX.Add(Item_ColorB);
            DataX.Add(Item_ColorF);
            DataX.Add(Item_ColorA + (Item_Type << 8));
            DataX.Add(Item_FontW);
            DataX.Add(Item_FontH);
            while (L > 0)
            {
                Data[Y].InsertRange(X * Factor, DataX);
                L--;
            }
        }

        public void Insert(int Y, int X, AnsiLineOccupyEx Obj, int ObjY)
        {
            Data[Y].InsertRange(X * Factor, Obj.Data[ObjY]);
        }

        public void Append(int Y, int L)
        {
            List<int> DataX = new List<int>();
            DataX.Add(Item_Char);
            DataX.Add(Item_ColorB);
            DataX.Add(Item_ColorF);
            DataX.Add(Item_ColorA + (Item_Type << 8));
            DataX.Add(Item_FontW);
            DataX.Add(Item_FontH);
            while (L > 0)
            {
                Data[Y].AddRange(DataX);
                L--;
            }
        }

        public void Append(int Y, AnsiLineOccupyEx Obj, int ObjY)
        {
            Data[Y].AddRange(Obj.Data[ObjY]);
        }

        public void Delete(int Y, int X, int L)
        {
            Data[Y].RemoveRange(X * Factor, L * Factor);
        }

        public AnsiLineOccupyEx CloneData()
        {
            AnsiLineOccupyEx Obj = new AnsiLineOccupyEx();
            for (int i = 0; i < Data.Count; i++)
            {
                List<int> DataItem = new List<int>();
                for (int ii = 0; ii < Data[i].Count; ii++)
                {
                    DataItem.Add(Data[i][ii]);
                }
                Obj.Data.Add(DataItem);
            }
            Obj.CopyItem(this);
            return Obj;
        }

        public void MergeColor(AnsiLineOccupyEx ColorObj)
        {
            ColorObj.TrimLines();
        }
    }
}
