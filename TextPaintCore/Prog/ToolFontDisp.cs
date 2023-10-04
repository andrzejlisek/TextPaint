using System;
using System.IO;
using System.Threading;

namespace TextPaint
{
    public class ToolFontDisp : Tool
    {
        public ToolFontDisp(ConfigFile CF_) : base(CF_)
        {
        }

        void PrintColor(StreamWriter FS_, int Back, int Fore)
        {
            if ((Back < 0) || (Fore < 0))
            {
                FS_.Write(CSI + "0m");
            }
            else
            {
                FS_.Write(CSI);
                if ((Back >= 0) && (Back <= 7))
                {
                    FS_.Write((Back + 40).ToString());
                }
                if ((Back >= 8) && (Back <= 15))
                {
                    FS_.Write((Back + 100 - 8).ToString());
                }
                FS_.Write(";");
                if ((Fore >= 0) && (Fore <= 7))
                {
                    FS_.Write((Fore + 30).ToString());
                }
                if ((Fore >= 8) && (Fore <= 15))
                {
                    FS_.Write((Fore + 90 - 8).ToString());
                }
                FS_.Write("m");
            }
        }

        void PrintClear(StreamWriter FS_)
        {
            FS_.Write(CSI + "2J");
            PrintCursorPos(FS_, 0, 0);
        }

        void PrintCursorPos(StreamWriter FS_, int X, int Y)
        {
            FS_.Write(CSI + (Y + 1) + ";" + (X + 1) + "H");
        }

        int DelayType;
        int DelayPos;

        void PrintBreak(StreamWriter FS_, int Interval)
        {
            if (DelayType == 0)
            {
                DelayPos += Interval;
                FS_.Write(CSI + "1;" + DelayPos + ";V");
            }
            if (DelayType == 1)
            {
                for (int i = 0; i < Interval; i++)
                {
                    PrintCursorPos(FS_, 0, 0);
                }
            }
        }

        public override void Start()
        {
            DelayPos = 0;
            DelayType = CF.ParamGetI("DelayType");
            string AnsiFile = CF.ParamGetS("AnsiFile");
            int PageStart = TextWork.CodeChar(CF.ParamGetS("PageFirst"));
            int PageStop = TextWork.CodeChar(CF.ParamGetS("PageLast"));
            int Interval = CF.ParamGetI("Interval");
            int BreakTime = CF.ParamGetI("Break");
            int CharCode1 = TextWork.CodeChar(CF.ParamGetS("Char1"));
            int CharCode0 = TextWork.CodeChar(CF.ParamGetS("Char0"));
            if (CharCode1 < 32) { CharCode1 = 0x2588; }
            if (CharCode0 < 32) { CharCode0 = 0x0020; }
            int ColorChar1 = CF.ParamGetI("ColorChar1");
            int ColorChar0 = CF.ParamGetI("ColorChar0");
            int ColorBack1 = CF.ParamGetI("ColorBack1");
            int ColorBack0 = CF.ParamGetI("ColorBack0");

            FileStream FS = new FileStream(AnsiFile, FileMode.Create, FileAccess.Write);
            StreamWriter FS_ = new StreamWriter(FS);

            PrintColor(FS_, -1, -1);
            PrintClear(FS_);
            PrintBreak(FS_, BreakTime);

            for (int i = PageStart; i <= PageStop; i++)
            {
                PrintColor(FS_, ColorBack0, ColorBack1);
                PrintClear(FS_);
                for (int YY = 1; YY < 24; YY++)
                {
                    PrintCursorPos(FS_, 1, YY);
                    for (int XX = 0; XX < 78; XX++)
                    {
                        FS_.Write("X");
                    }
                }
                PrintColor(FS_, ColorChar0, ColorChar1);
                PrintCursorPos(FS_, 0, 0);
                FS_.Write(i.ToString("X").PadLeft(4, '0'));
                FS_.Write(" [");
                string CharBlock1 = ((char)CharCode1).ToString();
                string CharBlock0 = ((char)CharCode0).ToString();
                int Val = i;
                if (Val >= 32768) { FS_.Write(CharBlock1); Val -= 32768; } else { FS_.Write(CharBlock0); }
                if (Val >= 16384) { FS_.Write(CharBlock1); Val -= 16384; } else { FS_.Write(CharBlock0); }
                if (Val >= 8192) { FS_.Write(CharBlock1); Val -= 8192; } else { FS_.Write(CharBlock0); }
                if (Val >= 4096) { FS_.Write(CharBlock1); Val -= 4096; } else { FS_.Write(CharBlock0); }
                if (Val >= 2048) { FS_.Write(CharBlock1); Val -= 2048; } else { FS_.Write(CharBlock0); }
                if (Val >= 1024) { FS_.Write(CharBlock1); Val -= 1024; } else { FS_.Write(CharBlock0); }
                if (Val >= 512) { FS_.Write(CharBlock1); Val -= 512; } else { FS_.Write(CharBlock0); }
                if (Val >= 256) { FS_.Write(CharBlock1); Val -= 256; } else { FS_.Write(CharBlock0); }
                if (Val >= 128) { FS_.Write(CharBlock1); Val -= 128; } else { FS_.Write(CharBlock0); }
                if (Val >= 64) { FS_.Write(CharBlock1); Val -= 64; } else { FS_.Write(CharBlock0); }
                if (Val >= 32) { FS_.Write(CharBlock1); Val -= 32; } else { FS_.Write(CharBlock0); }
                if (Val >= 16) { FS_.Write(CharBlock1); Val -= 16; } else { FS_.Write(CharBlock0); }
                if (Val >= 8) { FS_.Write(CharBlock1); Val -= 8; } else { FS_.Write(CharBlock0); }
                if (Val >= 4) { FS_.Write(CharBlock1); Val -= 4; } else { FS_.Write(CharBlock0); }
                if (Val >= 2) { FS_.Write(CharBlock1); Val -= 2; } else { FS_.Write(CharBlock0); }
                if (Val >= 1) { FS_.Write(CharBlock1); Val -= 1; } else { FS_.Write(CharBlock0); }
                FS_.Write("]" + CharBlock0 + CharBlock0);

                int Chr = 0;
                int Chr_ = (i * 256);
                for (int YY = 0; YY < 11; YY++)
                {
                    for (int XX = 0; XX < 25; XX++)
                    {
                        bool GoodChar = true;
                        if ((Chr_ >= 0x000000) && (Chr_ <= 0x00001F))
                        {
                            GoodChar = false;
                        }
                        if (GoodChar && (Chr < 256))
                        {
                            PrintCursorPos(FS_, 3 + (XX * 3), 2 + (YY * 2));
                            FS_.Write(" ");
                            PrintCursorPos(FS_, 3 + (XX * 3), 2 + (YY * 2));
                            if (((Chr_ >= 0x20) && (Chr_ < 0xD800)) || (Chr_ > 0xDFFF))
                            {
                                FS_.Write(char.ConvertFromUtf32(Chr_));
                            }
                            else
                            {
                                FS_.Write(" ");
                            }
                        }
                        Chr++;
                        Chr_++;
                    }
                }
                PrintCursorPos(FS_, 0, 0);
                PrintBreak(FS_, Interval);
                PrintCursorPos(FS_, 23, 0);
                FS_.Write(CharBlock1 + CharBlock1);
                PrintCursorPos(FS_, 0, 0);
                PrintBreak(FS_, Interval);
                PrintCursorPos(FS_, 23, 0);
                FS_.Write(CharBlock0 + CharBlock0);
                PrintCursorPos(FS_, 0, 0);
                PrintBreak(FS_, Interval);
            }
            if (BreakTime > 0)
            {
                PrintColor(FS_, -1, -1);
                PrintClear(FS_);
                PrintBreak(FS_, BreakTime);
            }

            FS_.Close();
            FS.Close();
        }
    }
}
