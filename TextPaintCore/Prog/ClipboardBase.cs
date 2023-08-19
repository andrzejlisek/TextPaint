using System;
using System.Threading.Tasks;

namespace TextPaint
{
    public class ClipboardBase
    {
        public static AnsiLineOccupyEx TextClipboard = new AnsiLineOccupyEx();

        protected static string LastSysText = "";

        public Core Core_;

        public ClipboardBase(Core Core__)
        {
            Core_ = Core__;
        }

        public int DiamondType = 0;

        bool IsInsideSelection(int W, int H, int XX, int YY, int TX, int TY)
        {
            if (DiamondType > 0)
            {
                // Top left edge
                int D = 0;
                if ((XX + YY + D - TX) < 0)
                {
                    return false;
                }

                // Top right edge
                D = 0;
                if ((DiamondType == 2) || (DiamondType == 4) || (DiamondType >= 6))
                {
                    D = 1;
                }
                if ((XX - YY - D - TX) > 0)
                {
                    return false;
                }

                // Bottom left edge
                D = 0;
                if ((DiamondType == 3) || (DiamondType == 5) || (DiamondType >= 6))
                {
                    D = 1;
                }
                if ((YY - XX - D - TX) > 0)
                {
                    return false;
                }

                // Bottom right edge
                D = 0;
                if ((DiamondType == 3) || (DiamondType == 5) || (DiamondType >= 6))
                {
                    D = 1;
                }
                if ((YY - W + XX - D - TY) > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public delegate void TextClipboardPasteEventHandler(string Raw);
        public event TextClipboardPasteEventHandler TextClipboardPasteEvent;

        public void TextClipboardCopy(string Txt)
        {
            Clipboard.SysClipboardSetSystem(Txt);
        }

        public void TextClipboardPaste()
        {
            TextClipboardPaste_();
        }

        async void TextClipboardPaste_()
        {
            string Txt_ = await Clipboard.SysClipboardGetSystem();
            if (Txt_ != null)
            {
                if (Txt_.Length > 0)
                {
                    TextClipboardPasteEvent(Txt_);
                }
            }
        }

        public delegate void TextClipboardWorkEvent1Handler(bool Paste);
        public event TextClipboardWorkEvent1Handler TextClipboardWorkEvent1;

        public delegate void TextClipboardWorkEvent2Handler(bool Paste);
        public event TextClipboardWorkEvent2Handler TextClipboardWorkEvent2;

        public void TextClipboardWork(int X, int Y, int W, int H, int FontW, int FontH, bool Paste)
        {
            int W_, H_, TX, TY;
            if (DiamondType > 0)
            {
                if (W < 0)
                {
                    TextClipboardWork(X + W, Y + W, 0 - W, H, FontW, FontH, Paste);
                    return;
                }
                if (H < 0)
                {
                    TextClipboardWork(X - H, Y + H, W, 0 - H, FontW, FontH, Paste);
                    return;
                }

                X = X - (H * FontW);
                W_ = W + H;
                H_ = W + H;

                TX = H;
                TY = W;

                if ((DiamondType == 2) || (DiamondType == 6) || (DiamondType == 7))
                {
                    W_++;
                }
                if ((DiamondType == 3) || (DiamondType == 7) || (DiamondType == 8))
                {
                    H_++;
                }
                if ((DiamondType == 4) || (DiamondType == 8) || (DiamondType == 9))
                {
                    X--;
                    W_++;
                }
                if ((DiamondType == 5) || (DiamondType == 9) || (DiamondType == 6))
                {
                    Y--;
                    H_++;
                }
            }
            else
            {
                if (W < 0)
                {
                    TextClipboardWork(X + W, Y, 0 - W, H, FontW, FontH, Paste);
                    return;
                }
                if (H < 0)
                {
                    TextClipboardWork(X, Y + H, W, 0 - H, FontW, FontH, Paste);
                    return;
                }
                W_ = W;
                H_ = H;
                TX = 0;
                TY = 0;
            }

            TextClipboardWork_(X, Y, TX, TY, W_, H_, FontW, FontH, Paste);
        }

        private async void TextClipboardWork_(int X, int Y, int TX, int TY, int W_, int H_, int FontW, int FontH, bool Paste)
        {
            TextClipboardWorkEvent1(Paste);

            bool PreserveFont = true;
            if ((FontW > 1) || (FontH > 1))
            {
                PreserveFont = false;
            }

            if (Paste)
            {
                if (await Clipboard.SysClipboardGet())
                {
                    for (int YY = 0; YY <= H_; YY++)
                    {
                        for (int XX = 0; XX <= W_; XX++)
                        {
                            if (IsInsideSelection(W_, H_, XX, YY, TX, TY))
                            {
                                TextClipboard.Get_(YY, XX);
                                if (!PreserveFont)
                                {
                                    TextClipboard.Item_FontW = 0;
                                    TextClipboard.Item_FontH = 0;
                                }
                                Core_.CharPut(X + (XX * FontW), Y + (YY * FontH), TextClipboard, true);
                            }
                        }
                    }
                }
            }
            else
            {
                TextClipboard.Clear();
                for (int YY = 0; YY <= H_; YY++)
                {
                    TextClipboard.AppendLine();
                    for (int XX = 0; XX <= W_; XX++)
                    {
                        TextClipboard.BlankChar();
                        if (IsInsideSelection(W_, H_, XX, YY, TX, TY))
                        {
                            TextClipboard.Item_Char = Core_.ElementGetVal(X + (XX * FontW), Y + (YY * FontH), true, false, 0);
                            TextClipboard.Item_ColorB = Core_.ElementGetVal(X + (XX * FontW), Y + (YY * FontH), true, false, 1);
                            TextClipboard.Item_ColorF = Core_.ElementGetVal(X + (XX * FontW), Y + (YY * FontH), true, false, 2);
                            TextClipboard.Item_ColorA = Core_.ElementGetVal(X + (XX * FontW), Y + (YY * FontH), true, false, 3);
                            if (PreserveFont)
                            {
                                TextClipboard.Item_FontW = Core_.ElementGetVal(X + (XX * FontW), Y + (YY * FontH), true, false, 4);
                                TextClipboard.Item_FontH = Core_.ElementGetVal(X + (XX * FontW), Y + (YY * FontH), true, false, 5);
                            }
                        }
                        TextClipboard.Append(YY);
                    }
                }
                await Clipboard.SysClipboardSet();
            }

            TextClipboardWorkEvent2(Paste);
        }
    }
}
