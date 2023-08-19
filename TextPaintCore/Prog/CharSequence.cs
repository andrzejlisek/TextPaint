using System;
using System.Collections.Generic;

namespace TextPaint
{
    public class CharSequence
    {
        List<int> SeqChar = new List<int>();
        List<int> SeqCount = new List<int>();
        public long Count0 = 0;
        public long Count = 0;
        int CharPutPos = -1;
        int CharGetPos = 0;
        int LastChar = int.MaxValue;

        public CharSequence()
        {
        }

        public List<int> CharGet(long N)
        {
            List<int> T = new List<int>();
            if (N > Count)
            {
                N = (int)Count;
            }
            while (N > 0)
            {
                T.Add(CharGet());
                N--;
            }
            return T;
        }

        public int CharGet()
        {
            Count--;
            int T = SeqChar[CharGetPos];
            if (SeqCount[CharGetPos] == 1)
            {
                CharGetPos++;
                CharPutPos--;
            }
            else
            {
                SeqCount[CharGetPos]--;
            }
            return T;
        }

        public void CharPut(int NewChar)
        {
            if (LastChar == NewChar)
            {
                if (CharPutPos < 0)
                {
                    SeqChar.Add(NewChar);
                    SeqCount.Add(1);
                    CharPutPos++;
                }
                else
                {
                    SeqCount[CharPutPos]++;
                }
            }
            else
            {
                SeqChar.Add(NewChar);
                SeqCount.Add(1);
                CharPutPos++;
                LastChar = NewChar;
            }
            Count++;
            Count0++;
        }
    }
}
