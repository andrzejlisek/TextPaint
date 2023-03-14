using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TextPaint
{
    public class TextWork
    {
        public static byte[] TelnetTimerBegin = new byte[] { 0x1B, 0x5B, 0x31, 0x3B };
        public static byte[] TelnetTimerEnd = new byte[] { 0x56 };

        public static int TelnetDummyChar = 0xFEFF;

        static Dictionary<int, string> SystemEncodingNames;

        static TextWork()
        {
            SystemEncodingNames = new Dictionary<int, string>();
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                Encoding e = ei.GetEncoding();
                string EncName = "";
                List<string> EncNameL = new List<string>();
                EncNameL.Add(e.CodePage.ToString());
                if ((!EncNameL.Contains(ei.Name)) && (TextWork.EncodingCheckName(e, ei.Name)))
                {
                    EncName = EncName + ((EncNameL.Count == 1) ? "" : ", ") + ei.Name;
                    EncNameL.Add(ei.Name);
                }
                if ((!EncNameL.Contains(e.WebName)) && (TextWork.EncodingCheckName(e, e.WebName)))
                {
                    EncName = EncName + ((EncNameL.Count == 1) ? "" : ", ") + e.WebName;
                    EncNameL.Add(e.WebName);
                }
                if (EncName.Equals("") && (e.CodePage != 0))
                {
                    EncName = e.CodePage.ToString();
                }
                SystemEncodingNames.Add(e.CodePage, EncName);
            }
        }

        public static string NumDecimal(double Num, int Points)
        {
            return Num.ToString("F2", CultureInfo.InvariantCulture);
        }

        public static string NumPlusMinus(long Num)
        {
            if (Num > 0)
            {
                return "+" + Num.ToString();
            }
            if (Num < 0)
            {
                return Num.ToString();
            }
            return "+" + Num.ToString();
        }

        public static int FullUTF8(List<byte> Data, int DataCount)
        {
            if (DataCount == 0)
            {
                return 0;
            }
            int I = 0;
            int Idelta = 0;
            while (true)
            {
                byte Item = Data[I];
                Idelta = 0;
                if ((Item & 0x80) == 0) // 0xxxxxxx
                {
                    Idelta = 1;
                }
                else
                {
                    if ((Item & 0xE0) == 0xC0) // 110xxxxx
                    {
                        Idelta = 2;
                    }
                    else
                    {
                        if ((Item & 0xF0) == 0xE0) // 1110xxxx
                        {
                            Idelta = 3;
                        }
                        else
                        {
                            if ((Item & 0xF8) == 0xF0) // 11110xxx
                            {
                                Idelta = 4;
                            }
                            else
                            {
                                if ((Item & 0xFC) == 0xF8) // 111110xx
                                {
                                    Idelta = 5;
                                }
                                else
                                {
                                    if ((Item & 0xFE) == 0xFC) // 1111110x
                                    {
                                        Idelta = 6;
                                    }
                                    else
                                    {
                                        Idelta = 1;
                                    }
                                }
                            }
                        }
                    }
                }
                if (Idelta == 0)
                {
                    break;
                }
                else
                {
                    I += Idelta;
                    if (I >= DataCount)
                    {
                        if (I > DataCount)
                        {
                            I -= Idelta;
                        }
                        break;
                    }
                }
            }
            return I;
        }

        public static int FullUTF16LE(List<byte> Data, int DataCount)
        {
            DataCount = DataCount - (DataCount & 1);
            if (DataCount >= 2)
            {
                if ((Data[DataCount - 1] & 0xFC) == 0xD8)
                {
                    DataCount -= 2;
                }
            }
            return DataCount;
        }

        public static int FullUTF16BE(List<byte> Data, int DataCount)
        {
            DataCount = DataCount - (DataCount & 1);
            if (DataCount >= 2)
            {
                if ((Data[DataCount - 2] & 0xFC) == 0xD8)
                {
                    DataCount -= 2;
                }
            }
            return DataCount;
        }

        public static int FullUTF32(List<byte> Data, int DataCount)
        {
            return DataCount - (DataCount & 3);
        }

        public static string EncodingGetName(Encoding E)
        {
            if (E is OneByteEncoding)
            {
                if (!(((OneByteEncoding)E).EncodingName_.Equals("")))
                {
                    return ((OneByteEncoding)E).EncodingName_;
                }
                else
                {
                    return "FILE";
                }
            }
            else
            {
                if (SystemEncodingNames.ContainsKey(E.CodePage))
                {
                    return SystemEncodingNames[E.CodePage];
                }
                else
                {
                    return "UNKNOWN";
                }
            }
        }

        public static bool EncodingCheckName(Encoding E0, string Name)
        {
            try
            {
                Encoding E = Encoding.GetEncoding(Name);
                if (E0.Equals(E))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static Encoding EncodingFromName(string Name)
        {
            if ("".Equals(Name))
            {
                return Encoding.Default;
            }
            string FName = Core.PrepareFileName(Name);
            if (System.IO.File.Exists(FName))
            {
                OneByteEncoding OneByteEncoding_ = new OneByteEncoding();
                ConfigFile CF = new ConfigFile();
                CF.FileLoad(FName);
                OneByteEncoding_.DefImport(CF);
                return OneByteEncoding_;
            }
            bool DigitOnly = true;
            for (int i = 0; i < Name.Length; i++)
            {
                if ((Name[i] < '0') || (Name[i] > '9'))
                {
                    DigitOnly = false;
                }
            }
            try
            {
                if (DigitOnly)
                {
                    return Encoding.GetEncoding(int.Parse(Name));
                }
                else
                {
                    return Encoding.GetEncoding(Name);
                }
            }
            catch
            {
                return Encoding.Default;
            }
        }

        public static List<int> SpaceChars = null;

        public static int SpaceChar0 = 32;

        public static int CodeChar(string C)
        {
            C = C.Trim();
            if ("".Equals(C))
            {
                return -1;
            }
            return int.Parse(C, NumberStyles.HexNumber);
        }

        public static string CharCode(int C, int Mode)
        {
            if ((C < 0) || (C > 0x10FFFF))
            {
                switch (Mode)
                {
                    case 0:
                        return "??";
                    case 1:
                        return "????";
                    case 2:
                        return " ????";
                }
            }
            string CharCodeX = C.ToString("X");
            if (C >= 0x100000)
            {
                CharCodeX = "G" + CharCodeX.PadLeft(4, '0').Substring(2);
            }
            if (Mode == 2)
            {
                return CharCodeX.PadLeft(4, '0').PadLeft(5, ' ');
            }
            else
            {
                if (Mode == 1)
                {
                    return CharCodeX.PadLeft(4, '0');
                }
                else
                {
                    if (CharCodeX.Length <= 2)
                    {
                        return CharCodeX.PadLeft(2, '0');
                    }
                    else
                    {
                        return CharCodeX.PadLeft(4, '0');
                    }
                }
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
                        L.Add(32);
                    }
                    i++;
                }
            }
            return L;
        }

        public static List<int> CodeToInt(string STR)
        {
            List<int> S = new List<int>();
            for (int i = 0; i < STR.Length / 2; i++)
            {
                string STR_Char = STR.Substring(i * 2, 2);
                if (STR_Char[0] == '_')
                {
                    S.Add((int)Encoding.UTF8.GetBytes(STR_Char)[1]);
                }
                else
                {
                    S.Add(int.Parse(STR_Char, NumberStyles.HexNumber));
                }
            }
            return S;
        }

        public static string IntToStr(int I)
        {
            List<int> L = new List<int>();
            L.Add(I);
            return IntToStr(L);
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

        public static int TrimEndLenCol(List<int> Text)
        {
            int I = Text.Count;
            bool Work = (I > 0);
            if (Work)
            {
                Work = (Text[I - 1] == 0);
            }
            while (Work)
            {
                I--;
                Work = (I > 0);
                if (Work)
                {
                    Work = (Text[I - 1] == 0);
                }
            }
            return I;
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

        public static List<int> TrimEnC(List<int> Text)
        {
            List<int> L = Copy(Text);
            bool Work = (L.Count > 0);
            if (Work)
            {
                Work = (L[L.Count - 1] == 0);
            }
            while (Work)
            {
                L.RemoveAt(L.Count - 1);
                Work = (L.Count > 0);
                if (Work)
                {
                    Work = (L[L.Count - 1] == 0);
                }
            }
            return L;
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

        public static List<int> BlkCol(int I)
        {
            List<int> L = new List<int>();
            while (I > 0)
            {
                L.Add(0);
                I--;
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
