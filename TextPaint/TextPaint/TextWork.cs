using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TextPaint
{
    public class TextWork
    {
        public TextWork()
        {
        }

        public static List<int> SpaceChars = null;

        public static int SpaceChar0 = 32;

        public static int CodeChar(string C)
        {
            C = C.Trim();
            if (C == "")
            {
                return -1;
            }
            return int.Parse(C, NumberStyles.HexNumber);
        }

        public static string CharCode(int C, bool Force5)
        {
            if ((C < 0) || (C > 0x10FFFF))
            {
                if (Force5)
                {
                    return " ????";
                }
                else
                {
                    return "????";
                }
            }
            string CharCodeX = C.ToString("X").PadLeft(4, '0');
            if (C >= 0x100000)
            {
                CharCodeX = "G" + CharCodeX.Substring(2);
            }
            if (Force5)
            {
                return CharCodeX.PadLeft(5, ' ');
            }
            else
            {
                return CharCodeX;
            }
        }

        public static string CharToStr(int C)
        {
            if (C <= 65535)
            {
                return ((char)C).ToString();
            }
            else
            {
                return char.ConvertFromUtf32(C);
            }
        }

        public static List<int> StrToInt(string T)
        {
            List<int> L = new List<int>();
            for (int i = 0; i < T.Length; i++)
            {
                int C = T[i];
                if ((C < 0xD800) || (C > 0xDFFF))
                {
                    L.Add(C);
                }
                else
                {
                    if (T.Length > (i + 1))
                    {
                        int C1 = (T[i] & 1023) << 10;
                        int C2 = T[i + 1] & 1023;
                        C1 += 0x10000;
                        L.Add(C1 + C2);
                    }
                    else
                    {
                        L.Add(' ');
                    }
                    i++;
                }
            }
            return L;
        }

        public static string IntToStr(List<int> L)
        {
            StringBuilder S = new StringBuilder();
            for (int i = 0; i < L.Count; i++)
            {
                if (L[i] >= 0)
                {
                    if (L[i] <= 65535)
                    {
                        S.Append((char)L[i]);
                    }
                    else
                    {
                        int C1 = (((L[i] - 0x10000) >> 10) & 1023);
                        int C2 = (((L[i] - 0x10000)) & 1023);
                        S.Append((char)(C1 + 0xD800));
                        S.Append((char)(C2 + 0xDC00));
                    }
                }
            }
            return S.ToString();
        }

        public static List<int> Concat(List<int> L1, List<int> L2)
        {
            List<int> L = new List<int>();
            for (int i = 0; i < L1.Count; i++)
            {
                L.Add(L1[i]);
            }
            for (int i = 0; i < L2.Count; i++)
            {
                L.Add(L2[i]);
            }
            return L;
        }

        public static List<int> Pad(int I, int C)
        {
            List<int> L = new List<int>();
            while (I > 0)
            {
                L.Add(C);
                I--;
            }
            return L;
        }

        public static int TrimEndLength(List<int> Text)
        {
            int I = Text.Count;
            bool Work = (I > 0);
            if (Work)
            {
                Work = SpaceChars.Contains(Text[I - 1]);
            }
            while (Work)
            {
                I--;
                Work = (I > 0);
                if (Work)
                {
                    Work = SpaceChars.Contains(Text[I - 1]);
                }
            }
            return I;
        }

        public static bool Equals(List<int> T1, List<int> T2)
        {
            if (T1.Count != T2.Count)
            {
                return false;
            }
            for (int i = 0; i < T1.Count; i++)
            {
                if (T1[i] != T2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static List<int> TrimEnd(List<int> Text)
        {
            List<int> L = Copy(Text);
            bool Work = (L.Count > 0);
            if (Work)
            {
                Work = SpaceChars.Contains(L[L.Count - 1]);
            }
            while (Work)
            {
                L.RemoveAt(L.Count - 1);
                Work = (L.Count > 0);
                if (Work)
                {
                    Work = SpaceChars.Contains(L[L.Count - 1]);
                }
            }
            return L;
        }

        public static List<int> Spaces(int I)
        {
            List<int> L = new List<int>();
            while (I > 0)
            {
                L.Add(SpaceChar0);
                I--;
            }
            return L;
        }

        public static List<int> Copy(List<int> L0)
        {
            List<int> L = new List<int>();
            for (int i = 0; i < L0.Count; i++)
            {
                L.Add(L0[i]);
            }
            return L;
        }
    }
}
