using System;
using System.Collections.Generic;

namespace TextPaint
{
    public partial class Core
    {
        List<int> __AnsiFontSizeAttr = new List<int>();

        public void AnsiFontReset()
        {
            __AnsiFontSizeAttr.Clear();
        }

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
            if (__AnsiFontSizeAttr.Count > N)
            {
                return __AnsiFontSizeAttr[N];
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
                __AnsiFontSizeAttr.Clear();
            }
            else
            {
                if (FromPos < __AnsiFontSizeAttr.Count)
                {
                    __AnsiFontSizeAttr.RemoveRange(FromPos, __AnsiFontSizeAttr.Count - FromPos);
                }
            }
        }

        public void AnsiSetFontSize(int N, int V, bool Rearrange)
        {
            // 0 - Single-width
            // 1 - Double-width
            // 2 - Double-height, top half
            // 3 - Double-height, bottom half

            while (__AnsiFontSizeAttr.Count <= N)
            {
                __AnsiFontSizeAttr.Add(0);
            }
            int OldV = AnsiGetFontSize(N);
            __AnsiFontSizeAttr[N] = V;

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
    }
}
