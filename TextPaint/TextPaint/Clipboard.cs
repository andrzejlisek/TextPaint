/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-12
 * Time: 21:53
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace TextPaint
{
    /// <summary>
    /// Description of Clipboard.
    /// </summary>
    public class Clipboard
    {
        static List<List<int>> TextClipboardT = new List<List<int>>();
        static List<List<int>> TextClipboardC = new List<List<int>>();

        public static void TextClipboardClear()
        {
            TextClipboardT.Clear();
            TextClipboardC.Clear();
        }

        public static int TextClipboardGetT(int X, int Y)
        {
            if (TextClipboardT.Count > Y)
            {
                if (TextClipboardT[Y].Count > X)
                {
                    return TextClipboardT[Y][X];
                }
            }
            return TextWork.SpaceChar0;
        }

        public static int TextClipboardGetC(int X, int Y)
        {
            if (TextClipboardC.Count > Y)
            {
                if (TextClipboardC[Y].Count > X)
                {
                    return TextClipboardC[Y][X];
                }
            }
            return 0;
        }


        public static void TextClipboardSet(int X, int Y, int T, int C)
        {
            while (TextClipboardT.Count <= Y)
            {
                TextClipboardT.Add(new List<int>());
                TextClipboardC.Add(new List<int>());
            }
            while (TextClipboardT[Y].Count <= X)
            {
                TextClipboardT[Y].Add(TextWork.SpaceChar0);
                TextClipboardC[Y].Add(0);
            }
            TextClipboardT[Y][X] = T;
            TextClipboardC[Y][X] = C;
        }

        private static string LastSysText = "";

        public static bool SysClipboardGet()
        {
            if (System.Windows.Forms.Clipboard.ContainsText())
            {
                string Txt_ = System.Windows.Forms.Clipboard.GetText();
                if (Txt_ != LastSysText)
                {
                    string[] Txt = Txt_.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    TextClipboardT.Clear();
                    TextClipboardC.Clear();
                    for (int i = 0; i < Txt.Length; i++)
                    {
                        TextClipboardT.Add(TextWork.StrToInt(Txt[i]));
                        TextClipboardC.Add(TextWork.BlkCol(TextClipboardT[i].Count));
                    }
                }
                return true;
            }
            else
            {
                LastSysText = "";
                return false;
            }
        }

        public static void SysClipboardSet()
        {
            System.Text.StringBuilder Txt = new System.Text.StringBuilder();
            for (int i = 0; i < TextClipboardT.Count; i++)
            {
                Txt.AppendLine(TextWork.IntToStr(TextClipboardT[i]));
            }
            System.Windows.Forms.Clipboard.SetText(Txt.ToString());
            LastSysText = System.Windows.Forms.Clipboard.GetText();
        }


        public Core Core_;

        public Clipboard(Core Core__)
        {
            Core_ = Core__;
        }

        public void CharPut(int X, int Y, int Ch, int Col)
        {
            Core_.CharPut(Core_.CursorX + X, Core_.CursorY + Y, Ch, Col);
        }

        public int CharGet(int X, int Y)
        {
            return Core_.CharGet(Core_.CursorX + X, Core_.CursorY + Y, true);
        }

        public int ColoGet(int X, int Y)
        {
            return Core_.ColoGet(Core_.CursorX + X, Core_.CursorY + Y, true);
        }




        public int DiamondType = 0;


        public void TextClipboardPutChar(int X, int Y, int W, int H, int XX, int YY, int Ch, int Col)
        {
            if (DiamondType > 0)
            {
                // Top left edge
                int D = 0;
                if ((DiamondType == 4) || (DiamondType == 5) || (DiamondType == 6) || (DiamondType == 8))
                {
                    D = 1;
                }
                if ((DiamondType == 9))
                {
                    D = 2;
                }
                if ((XX + YY + D - X - Y) < 0)
                {
                    return;
                }
                
                // Top right edge
                D = 0;
                if ((DiamondType == 2) || (DiamondType == 5) || (DiamondType == 7) || (DiamondType == 9))
                {
                    D = 1;
                }
                if ((DiamondType == 6))
                {
                    D = 2;
                }
                if ((XX - YY - D - X + Y) > 0)
                {
                    return;
                }
                
                // Bottom left edge
                D = 0;
                if ((DiamondType == 3) || (DiamondType == 4) || (DiamondType == 7) || (DiamondType == 9))
                {
                    D = 1;
                }
                if ((DiamondType == 8))
                {
                    D = 2;
                }
                if ((YY - XX - H - H - D + X - Y) > 0)
                {
                    return;
                }
                
                // Bottom right edge
                D = 0;
                if ((DiamondType == 2) || (DiamondType == 3) || (DiamondType == 6) || (DiamondType == 8))
                {
                    D = 1;
                }
                if ((DiamondType == 7))
                {
                    D = 2;
                }
                if ((YY - W + XX - W - D - X - Y) > 0)
                {
                    return;
                }
            }
            CharPut(XX, YY, Ch, Col);
        }
        
        public void TextClipboardWork(int X, int Y, int W, int H, bool Paste)
        {
            int X1, X2, Y1, Y2;
            if (DiamondType > 0)
            {
                if (W < 0)
                {
                    TextClipboardWork(X + W, Y + W, 0 - W, H, Paste);
                    return;
                }
                if (H < 0)
                {
                    TextClipboardWork(X - H, Y + H, W, 0 - H, Paste);
                    return;
                }

                Y1 = Y;
                Y2 = Y + W + H;
                X1 = X - H;
                X2 = X + W;
                if ((DiamondType == 2) || (DiamondType == 6) || (DiamondType == 7))
                {
                    X2++;
                }
                if ((DiamondType == 3) || (DiamondType == 7) || (DiamondType == 8))
                {
                    Y2++;
                }
                if ((DiamondType == 4) || (DiamondType == 8) || (DiamondType == 9))
                {
                    X1--;
                }
                if ((DiamondType == 5) || (DiamondType == 9) || (DiamondType == 6))
                {
                    Y1--;
                }
            }
            else
            {
                if (W < 0)
                {
                    TextClipboardWork(X + W, Y, 0 - W, H, Paste);
                    return;
                }
                if (H < 0)
                {
                    TextClipboardWork(X, Y + H, W, 0 - H, Paste);
                    return;
                }

                Y1 = Y;
                Y2 = Y + H;
                X1 = X;
                X2 = X + W;
            }


            if (Paste)
            {
                if (SysClipboardGet())
                {
                    for (int YY = Y1; YY <= Y2; YY++)
                    {
                        for (int XX = X1; XX <= X2; XX++)
                        {
                            TextClipboardPutChar(X, Y, W, H, XX, YY, TextClipboardGetT(XX - X1, YY - Y1), TextClipboardGetC(XX - X1, YY - Y1));
                        }
                    }
                }
            }
            else
            {
                TextClipboardClear();
                for (int YY = Y1; YY <= Y2; YY++)
                {
                    for (int XX = X1; XX <= X2; XX++)
                    {
                        TextClipboardSet(XX - X1, YY - Y1, CharGet(XX, YY), ColoGet(XX, YY));
                    }
                }
                SysClipboardSet();
            }
        }




    }
}
