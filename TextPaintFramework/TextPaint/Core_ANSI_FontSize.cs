using System;
using System.Collections.Generic;

namespace TextPaint
{
    public partial class Core
    {
        public int AnsiGetFontW(int N, int X)
        {
            if (AnsiGetFontSize(N) == 0)
            {
                return 0;
            }
            else
            {
                if ((X % 2) == 0)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
        }

        public int AnsiGetFontH(int N)
        {
            int S = AnsiGetFontSize(N);
            if (S == 2)
            {
                return 1;
            }
            if (S == 3)
            {
                return 1;
            }
            return 0;
        }


        public int AnsiGetFontSize(int N)
        {
            if (AnsiState_.__AnsiFontSizeAttr.Count > N)
            {
                return AnsiState_.__AnsiFontSizeAttr[N];
            }
            else
            {
                return 0;
            }
        }

        public void AnsiClearFontSize(int FromPos)
        {
            if (FromPos < 0)
            {
                AnsiState_.__AnsiFontSizeAttr.Clear();
            }
            else
            {
                if (FromPos < AnsiState_.__AnsiFontSizeAttr.Count)
                {
                    AnsiState_.__AnsiFontSizeAttr.RemoveRange(FromPos, AnsiState_.__AnsiFontSizeAttr.Count - FromPos);
                }
            }
        }

        public void AnsiSetFontSize(int N, int V, bool Rearrange)
        {
            // 0 - Single-width
            // 1 - Double-width
            // 2 - Double-height, top half
            // 3 - Double-height, bottom half

            while (AnsiState_.__AnsiFontSizeAttr.Count <= N)
            {
                AnsiState_.__AnsiFontSizeAttr.Add(0);
            }
            int OldV = AnsiGetFontSize(N);
            AnsiState_.__AnsiFontSizeAttr[N] = V;

            if (Rearrange)
            {
                int FontH_ = 0;
                if (V == 2) FontH_ = 1;
                if (V == 3) FontH_ = 2;

                // Refresh font size
                if ((OldV > 0) && (V > 0))
                {
                    int ChrC;
                    int ChrB = TextNormalBack;
                    int ChrF = TextNormalFore;
                    int FontW = 0;
                    int FontH = 0;
                    for (int i = 0; i < AnsiMaxX; i++)
                    {
                        AnsiGet(i, N, out ChrC, out ChrB, out ChrF, out FontW, out FontH);
                        AnsiChar(i, N, ChrC, ChrB, ChrF, FontW, FontH_);
                    }
                    AnsiRepaintLine(N);
                }

                // Stretch text
                if ((OldV == 0) && (V > 0))
                {
                    int ChrC;
                    int ChrB = TextNormalBack;
                    int ChrF = TextNormalFore;
                    int FontW = 0;
                    int FontH = 0;
                    for (int i = (AnsiMaxX - 1); i >= 0; i--)
                    {
                        AnsiGet(i, N, out ChrC, out ChrB, out ChrF, out FontW, out FontH);
                        AnsiChar(i * 2 + 0, N, ChrC, ChrB, ChrF, 1, FontH_);
                        AnsiChar(i * 2 + 1, N, ChrC, ChrB, ChrF, 2, FontH_);
                    }
                    AnsiRepaintLine(N);
                }

                // Shrink text
                if ((OldV > 0) && (V == 0))
                {
                    int ChrC;
                    int ChrB = TextNormalBack;
                    int ChrF = TextNormalFore;
                    int FontW = 0;
                    int FontH = 0;
                    for (int i = 0; i < AnsiMaxX; i++)
                    {
                        AnsiGet(i * 2, N, out ChrC, out ChrB, out ChrF, out FontW, out FontH);
                        AnsiChar(i, N, ChrC, ChrB, ChrF, 0, FontH_);
                    }
                    AnsiRepaintLine(N);
                }
            }
        }

        private int AnsiCalcColorSwapLoHi(int C)
        {
            if (C < 8)
            {
                return C + 8;
            }
            else
            {
                return C - 7;
            }
        }

        private void AnsiCalcColor()
        {
            AnsiCalcColor(AnsiState_.__AnsiBack, AnsiState_.__AnsiFore);
        }

        private void AnsiCalcColor(int B, int F)
        {
            AnsiState_.__AnsiBackWork = B;
            AnsiState_.__AnsiForeWork = F;

            if (AnsiState_.__AnsiBackWork < 0)
            {
                AnsiState_.__AnsiBackWork = TextNormalBack;
            }
            if (AnsiState_.__AnsiForeWork < 0)
            {
                AnsiState_.__AnsiForeWork = TextNormalFore;
            }
            AnsiState_.__AnsiBackScroll = AnsiState_.__AnsiBackWork;
            AnsiState_.__AnsiForeScroll = AnsiState_.__AnsiForeWork;

            if (ANSIReverseMode == 0)
            {
                if (AnsiState_.__AnsiFontInverse)
                {
                    int Temp = AnsiState_.__AnsiForeWork;
                    AnsiState_.__AnsiForeWork = AnsiState_.__AnsiBackWork;
                    AnsiState_.__AnsiBackWork = Temp;
                }
            }

            if (AnsiState_.__AnsiFontBold && (!ANSIIgnoreBold))
            {
                if (AnsiState_.__AnsiForeWork < 8)
                {
                    if ((AnsiState_.__AnsiForeWork >= 0) && (AnsiState_.__AnsiForeWork < 8))
                    {
                        AnsiState_.__AnsiForeWork += 8;
                    }
                }
                else
                {
                    if ((AnsiState_.__AnsiForeWork >= 8) && (AnsiState_.__AnsiForeWork < 16))
                    {
                        AnsiState_.__AnsiForeWork -= 8;
                    }
                }
            }

            if (AnsiState_.__AnsiFontBlink && (!ANSIIgnoreBlink))
            {
                if (AnsiState_.__AnsiBackWork < 8)
                {
                    if ((AnsiState_.__AnsiBackWork >= 0) && (AnsiState_.__AnsiBackWork < 8))
                    {
                        AnsiState_.__AnsiBackWork += 8;
                    }
                }
                else
                {
                    if ((AnsiState_.__AnsiBackWork >= 8) && (AnsiState_.__AnsiBackWork < 16))
                    {
                        AnsiState_.__AnsiBackWork -= 8;
                    }
                }
            }

            if (ANSIReverseMode == 1)
            {
                if (AnsiState_.__AnsiFontInverse)
                {
                    int Temp = AnsiState_.__AnsiForeWork;
                    AnsiState_.__AnsiForeWork = AnsiState_.__AnsiBackWork;
                    AnsiState_.__AnsiBackWork = Temp;
                }
            }

            if (AnsiState_.__AnsiFontInvisible && (!ANSIIgnoreConcealed))
            {
                AnsiState_.__AnsiForeWork = AnsiState_.__AnsiBackWork;
            }

            if (B < 0)
            {
                if (AnsiState_.__AnsiBackWork == TextNormalBack)
                {
                    AnsiState_.__AnsiBackWork = -1;
                }
                if (AnsiState_.__AnsiBackScroll == TextNormalBack)
                {
                    AnsiState_.__AnsiBackScroll = -1;
                }
            }
            if (F < 0)
            {
                if (AnsiState_.__AnsiForeWork == TextNormalFore)
                {
                    AnsiState_.__AnsiForeWork = -1;
                }
                if (AnsiState_.__AnsiForeScroll == TextNormalFore)
                {
                    AnsiState_.__AnsiForeScroll = -1;
                }
            }
        }

        void AnsiDetectAttributesInv(int B, int F)
        {
            AnsiState_.__AnsiFontInverse = false;
            int BlinkB = B;
            int BlinkF = F;
            if (BlinkB > 7) { BlinkB -= 7; }
            if (BlinkF > 7) { BlinkF -= 7; }
            if (TextNormalBack < TextNormalFore)
            {
                if (BlinkB > BlinkF)
                {
                    AnsiState_.__AnsiFontInverse = true;
                }
            }
            if (TextNormalBack > TextNormalFore)
            {
                if (BlinkB < BlinkF)
                {
                    AnsiState_.__AnsiFontInverse = true;
                }
            }
        }

        void AnsiDetectAttributes(int B, int F)
        {
            if (B < 0) { B = TextNormalBack; }
            if (F < 0) { F = TextNormalFore; }

            if (ANSIReverseMode == 1)
            {
                AnsiDetectAttributesInv(B, F);
                if (AnsiState_.__AnsiFontInverse)
                {
                    int T = B;
                    B = F;
                    F = T;
                }
            }

            AnsiState_.__AnsiFontBold = false;
            if (!ANSIIgnoreBold)
            {
                if (F >= 8)
                {
                    AnsiState_.__AnsiFontBold = true;
                }
            }

            AnsiState_.__AnsiFontBlink = false;
            if (!ANSIIgnoreBlink)
            {
                if (B >= 8)
                {
                    AnsiState_.__AnsiFontBlink = true;
                }
            }

            if (ANSIReverseMode == 0)
            {
                AnsiDetectAttributesInv(B, F);
            }
        }

        void AnsiAttriburesSave()
        {
            TempMemo.Push(AnsiState_.__AnsiBackWork);
            TempMemo.Push(AnsiState_.__AnsiForeWork);
            TempMemo.Push(AnsiState_.__AnsiBackScroll);
            TempMemo.Push(AnsiState_.__AnsiForeScroll);
            TempMemoB.Push(AnsiState_.__AnsiFontBold);
            TempMemoB.Push(AnsiState_.__AnsiFontBlink);
            TempMemoB.Push(AnsiState_.__AnsiFontInverse);
            TempMemoB.Push(AnsiState_.__AnsiFontInvisible);
        }

        void AnsiAttributesLoad()
        {
            AnsiState_.__AnsiFontInvisible = TempMemoB.Pop();
            AnsiState_.__AnsiFontInverse = TempMemoB.Pop();
            AnsiState_.__AnsiFontBlink = TempMemoB.Pop();
            AnsiState_.__AnsiFontBold = TempMemoB.Pop();
            AnsiState_.__AnsiForeScroll = TempMemo.Pop();
            AnsiState_.__AnsiBackScroll = TempMemo.Pop();
            AnsiState_.__AnsiForeWork = TempMemo.Pop();
            AnsiState_.__AnsiBackWork = TempMemo.Pop();
        }

        void TestAttributeToColor()
        {
            ANSIIgnoreBold = false;
            ANSIIgnoreBlink = false;
            ANSIReverseMode = 1;
            Console.Clear();
            int B = -1;
            int F = -1;
            for (int i = 0; i < 8; i++)
            {
                if (ANSIReverseMode == 0)
                {
                    AnsiState_.__AnsiFontBold = ((i & 1) > 0);
                    AnsiState_.__AnsiFontBlink = ((i & 2) > 0);
                    AnsiState_.__AnsiFontInverse = ((i & 4) > 0);
                }
                if (ANSIReverseMode == 1)
                {
                    AnsiState_.__AnsiFontBold = ((i & 2) > 0);
                    AnsiState_.__AnsiFontBlink = ((i & 4) > 0);
                    AnsiState_.__AnsiFontInverse = ((i & 1) > 0);
                }
                AnsiCalcColor(B, F);
                Console.BackgroundColor = TestConsoleColor(AnsiState_.__AnsiBackWork, TextNormalBack);
                Console.ForegroundColor = TestConsoleColor(AnsiState_.__AnsiForeWork, TextNormalFore);
                Console.Write(AnsiState_.__AnsiFontBold.ToString() + AnsiState_.__AnsiFontBlink.ToString() + AnsiState_.__AnsiFontInverse.ToString());
                AnsiDetectAttributes(AnsiState_.__AnsiBackWork, AnsiState_.__AnsiForeWork);
                Console.Write(" -> ");
                Console.Write(AnsiState_.__AnsiFontBold.ToString() + AnsiState_.__AnsiFontBlink.ToString() + AnsiState_.__AnsiFontInverse.ToString());
                Console.WriteLine();
            }
            Console.ReadLine();
        }

        int ColorThresholdBlackWhite = 48;
        int ColorThresholdGray = 20;

        Dictionary<int, int> AnsiColor16_ = new Dictionary<int, int>();

        public int AnsiColor16(int R, int G, int B)
        {
            int RGBIdx = R * 65536 + G * 256 + B;
            if (AnsiColor16_.ContainsKey(RGBIdx))
            {
                return AnsiColor16_[RGBIdx];
            }

            int RGBmax = Math.Max(Math.Max(R, G), B);
            int RGBmin = Math.Min(Math.Min(R, G), B);
            int RGBdiff = (RGBmax - RGBmin);
            int L = R + G + B;
            int H = 0;

            if (RGBdiff > 0)
            {
                if (RGBmax == R)
                {
                    H = ((G - B) * 600) / RGBdiff;
                    if (H < 0)
                    {
                        H += 3600;
                    }
                }
                if (RGBmax == G)
                {
                    H = ((B - R) * 600) / RGBdiff + 1200;
                }
                if (RGBmax == B)
                {
                    H = ((R - G) * 600) / RGBdiff + 2400;
                }
            }
            int S = 0;
            int _128_3 = 128 * 3;
            int _255_3 = 255 * 3;
            if (L < _128_3)
            {
                if (L > 0)
                {
                    S = ((3 * 20 * RGBdiff) / (L));
                }
            }
            else
            {
                if (L < _255_3)
                {
                    S = ((3 * 20 * RGBdiff) / (_255_3 - L));
                }
            }

            int ColorIdx = -1;

            if (L < (ColorThresholdBlackWhite) * 3)
            {
                ColorIdx = 0;
            }
            else
            {
                if (L > ((255 - ColorThresholdBlackWhite) * 3))
                {
                    ColorIdx = 15;
                }
                else
                {
                    if (L < _128_3)
                    {
                        if (S <= ColorThresholdGray)
                        {
                            ColorIdx = 8;
                        }
                        else
                        {
                            if ((H >= 3300) || (H <= 300))
                            {
                                ColorIdx = 1;
                            }
                            if ((H > 300) && (H < 900))
                            {
                                ColorIdx = 3;
                            }
                            if ((H >= 900) && (H <= 1500))
                            {
                                ColorIdx = 2;
                            }
                            if ((H > 1500) && (H < 2100))
                            {
                                ColorIdx = 6;
                            }
                            if ((H >= 2100) && (H <= 2700))
                            {
                                ColorIdx = 4;
                            }
                            if ((H > 2700) && (H < 3300))
                            {
                                ColorIdx = 5;
                            }
                        }
                    }
                    else
                    {
                        if (S <= ColorThresholdGray)
                        {
                            ColorIdx = 7;
                        }
                        else
                        {
                            if ((H > 3300) || (H < 300))
                            {
                                ColorIdx = 9;
                            }
                            if ((H >= 300) && (H <= 900))
                            {
                                ColorIdx = 11;
                            }
                            if ((H > 900) && (H < 1500))
                            {
                                ColorIdx = 10;
                            }
                            if ((H >= 1500) && (H <= 2100))
                            {
                                ColorIdx = 14;
                            }
                            if ((H > 2100) && (H < 2700))
                            {
                                ColorIdx = 12;
                            }
                            if ((H >= 2700) && (H <= 3300))
                            {
                                ColorIdx = 13;
                            }
                        }
                    }
                }
            }

            AnsiColor16_.Add(RGBIdx, ColorIdx);

            return ColorIdx;
        }

        ConsoleColor TestConsoleColor(int N, int D)
        {
            switch (N)
            {
                default: return TestConsoleColor(D, 0);
                case 0: return ConsoleColor.Black;
                case 1: return ConsoleColor.DarkRed;
                case 2: return ConsoleColor.DarkGreen;
                case 3: return ConsoleColor.DarkYellow;
                case 4: return ConsoleColor.DarkBlue;
                case 5: return ConsoleColor.DarkMagenta;
                case 6: return ConsoleColor.DarkCyan;
                case 7: return ConsoleColor.Gray;
                case 8: return ConsoleColor.DarkGray;
                case 9: return ConsoleColor.Red;
                case 10: return ConsoleColor.Green;
                case 11: return ConsoleColor.Yellow;
                case 12: return ConsoleColor.Blue;
                case 13: return ConsoleColor.Magenta;
                case 14: return ConsoleColor.Cyan;
                case 15: return ConsoleColor.White;
            }
        }
    }
}
