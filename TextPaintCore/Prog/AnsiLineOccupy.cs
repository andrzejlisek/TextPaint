using System;
using System.Collections.Generic;

namespace TextPaint
{
    public class AnsiLineOccupy : AnsiLineOccupyItem
    {
        public const int Factor = 6;
        protected List<List<int>> Data = new List<List<int>>();

        public AnsiLineOccupy()
        {
            Data = new List<List<int>>();
        }

        public void Clear()
        {
            Data.Clear();
        }

        public void Append(int Y)
        {
            Data[Y].Add(Item_Char);
            Data[Y].Add(Item_ColorB);
            Data[Y].Add(Item_ColorF);
            Data[Y].Add(Item_ColorA + (Item_Type << 8));
            Data[Y].Add(Item_FontW);
            Data[Y].Add(Item_FontH);
        }

        public void Insert(int Y, int X)
        {
            Data[Y].Insert(X * Factor, Item_FontH);
            Data[Y].Insert(X * Factor, Item_FontW);
            Data[Y].Insert(X * Factor, Item_ColorA + (Item_Type << 8));
            Data[Y].Insert(X * Factor, Item_ColorF);
            Data[Y].Insert(X * Factor, Item_ColorB);
            Data[Y].Insert(X * Factor, Item_Char);
        }

        public void Delete(int Y, int X)
        {
            Data[Y].RemoveRange(X * Factor, Factor);
        }

        public void AppendLine()
        {
            Data.Add(new List<int>());
        }

        public void InsertLine(int I)
        {
            Data.Insert(I, new List<int>());
        }

        public void AppendLine(AnsiLineOccupy X, int I)
        {
            Data.Add(X.Data[I]);
        }

        public void InsertLine(AnsiLineOccupy X, int I, int II)
        {
            Data.Insert(II, X.Data[I]);
        }

        public void DeleteLine(int I)
        {
            Data.RemoveAt(I);
        }

        public void Get(int Y, int X)
        {
            Item_Char = Data[Y][X * Factor + 0];
            Item_ColorB = Data[Y][X * Factor + 1];
            Item_ColorF = Data[Y][X * Factor + 2];
            Item_ColorA = Data[Y][X * Factor + 3] & 255;
            Item_Type = Data[Y][X * Factor + 3] >> 8;
            Item_FontW = Data[Y][X * Factor + 4];
            Item_FontH = Data[Y][X * Factor + 5];
        }

        public void Set(int Y, int X)
        {
            Data[Y][X * Factor + 0] = Item_Char;
            Data[Y][X * Factor + 1] = Item_ColorB;
            Data[Y][X * Factor + 2] = Item_ColorF;
            Data[Y][X * Factor + 3] = Item_ColorA + (Item_Type << 8);
            Data[Y][X * Factor + 4] = Item_FontW;
            Data[Y][X * Factor + 5] = Item_FontH;
        }

        public int CountLines()
        {
            return Data.Count;
        }

        public int CountItems(int I)
        {
            return Data[I].Count / Factor;
        }

        public static void Copy(ref AnsiLineOccupy Src, ref AnsiLineOccupy Dst)
        {
            Dst.Data.Clear();
            for (int i = 0; i < Src.Data.Count; i++)
            {
                List<int> Temp = new List<int>();
                for (int ii = 0; ii < Src.Data[i].Count; ii++)
                {
                    Temp.Add(Src.Data[i][ii]);
                }
                Dst.Data.Add(Temp);
            }
        }

        public static void Swap(ref AnsiLineOccupy Src, ref AnsiLineOccupy Dst)
        {
            AnsiLineOccupy Temp = new AnsiLineOccupy();
            Copy(ref Dst, ref Temp);
            Copy(ref Src, ref Dst);
            Copy(ref Temp, ref Src);
        }
    }
}
