using System;
namespace TextPaint
{
    public partial class Core
    {
        public int CursorFontW = 1;
        public int CursorFontH = 1;

        public int FontMaxSize = 32;
        public int FontMaxSizeCode = 527;

        public static int FontCounter(int CurrentValue)
        {
            if (CurrentValue == 0)
            {
                return 0;
            }
            switch (CurrentValue)
            {
                case 2: return 1;
                case 5: return 3;
                case 9: return 6;
                case 14: return 10;
                case 20: return 15;
                case 27: return 21;
                case 35: return 28;
                case 44: return 36;
                case 54: return 45;
                case 65: return 55;
                case 77: return 66;
                case 90: return 78;
                case 104: return 91;
                case 119: return 105;
                case 135: return 120;
                case 152: return 136;
                case 170: return 153;
                case 189: return 171;
                case 209: return 190;
                case 230: return 210;
                case 252: return 231;
                case 275: return 253;
                case 299: return 276;
                case 324: return 300;
                case 350: return 325;
                case 377: return 351;
                case 405: return 378;
                case 434: return 406;
                case 464: return 435;
                case 495: return 465;
                case 527: return 496;
                default: return CurrentValue + 1;
            }
        }

        public int FontSizeCode(int S, int N)
        {
            switch (S)
            {
                default: return 0;
                case 2: return N + 1;
                case 3: return N + 3;
                case 4: return N + 6;
                case 5: return N + 10;
                case 6: return N + 15;
                case 7: return N + 21;
                case 8: return N + 28;
                case 9: return N + 36;
                case 10: return N + 45;
                case 11: return N + 55;
                case 12: return N + 66;
                case 13: return N + 78;
                case 14: return N + 91;
                case 15: return N + 105;
                case 16: return N + 120;
                case 17: return N + 136;
                case 18: return N + 153;
                case 19: return N + 171;
                case 20: return N + 190;
                case 21: return N + 210;
                case 22: return N + 231;
                case 23: return N + 253;
                case 24: return N + 276;
                case 25: return N + 300;
                case 26: return N + 325;
                case 27: return N + 351;
                case 28: return N + 378;
                case 29: return N + 406;
                case 30: return N + 435;
                case 31: return N + 465;
                case 32: return N + 496;
            }
        }

        public int CursorXBase()
        {
            return CursorX % CursorFontW;
        }

        public int CursorYBase()
        {
            return CursorY % CursorFontH;
        }

        int CursorX0()
        {
            int T = (CursorX - DisplayX) % CursorFontW;
            if (T == 0)
            {
                return 0;
            }
            else
            {
                return 0 - (CursorFontW - T);
            }
        }

        int CursorY0()
        {
            int T = (CursorY - DisplayY) % CursorFontH;
            if (T == 0)
            {
                return 0;
            }
            else
            {
                return 0 - (CursorFontH - T);
            }
        }
    }
}
