using System;
using System.Text;

namespace TextPaint
{
    public class OneByteEncoding : Encoding
    {
        private char[] conversionArray;
        public string EncodingName = "";

        public OneByteEncoding()
        {
            conversionArray = new char[256];
            EncodingName = "";
            for (int i = 0; i < 256; i++)
            {
                conversionArray[i] = (char)i;
            }
        }

        public bool DefImport(Encoding EncX)
        {
            if (EncX == null)
            {
                return false;
            }
            EncodingName = EncX.CodePage.ToString();
            byte[] Raw = new byte[1];
            for (int i = 0; i < 0x110000; i++)
            {
                if (EncX.GetBytes(((char)i).ToString()).Length != 1)
                {
                    return false;
                }
            }
            for (int i = 0; i < 256; i++)
            {
                Raw[0] = (byte)i;
                if (EncX.GetChars(Raw).Length != 1)
                {
                    EncodingName = "";
                    for (int ii = 0; ii < 256; ii++)
                    {
                        conversionArray[ii] = (char)ii;
                    }
                    return false;
                }
                conversionArray[i] = EncX.GetChars(Raw)[0];
            }
            return true;
        }

        public bool DefImport(ConfigFile CF)
        {
            EncodingName = CF.ParamGetS("Name");
            for (int i = 0; i < 256; i++)
            {
                conversionArray[i] = (char)i;
                bool NotSet = true;
                string K1 = i.ToString("X").ToUpperInvariant();
                string K2 = i.ToString("x").ToLowerInvariant();
                for (int ii = 0; ii < 10; ii++)
                {
                    if (NotSet && (CF.ParamExists(K1)))
                    {
                        conversionArray[i] = (char)int.Parse(CF.ParamGetS(K1), System.Globalization.NumberStyles.HexNumber);
                        break;
                    }
                    if (NotSet && (CF.ParamExists(K2)))
                    {
                        conversionArray[i] = (char)int.Parse(CF.ParamGetS(K2), System.Globalization.NumberStyles.HexNumber);
                        break;
                    }
                    K1 = "0" + K1;
                    K2 = "0" + K2;
                }
            }
            return true;
        }

        public int[] DefExport()
        {
            int[] T = new int[256];
            for (int i = 0; i < 256; i++)
            {
                T[i] = conversionArray[i];
            }
            return T;
        }

        public int[] DefExport(ConfigFile CF)
        {
            for (int i = 0; i < 256; i++)
            {
                string K = TextWork.CharCode(i, 0);
                CF.ParamSet(K, TextWork.CharCode(conversionArray[i], 0));
            }
            return DefExport();
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return chars.Length;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            for (var i = 0; i < charCount; i++)
            {
                bytes[byteIndex + i] = GetByte(chars[charIndex + i]);
            }

            return charCount;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return bytes.Length;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            for (var i = 0; i < byteCount; i++)
            {
                chars[charIndex + i] = GetChar(bytes[byteIndex + i]);
            }

            return byteCount;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }

        private byte GetByte(char c)
        {
            for (var i = 0; i < conversionArray.Length; i++)
            {
                if (conversionArray[i] == c)
                {
                    return (byte)i;
                }
            }

            return 32;
        }

        private char GetChar(byte b)
        {
            return conversionArray[b];
        }
    }
}
