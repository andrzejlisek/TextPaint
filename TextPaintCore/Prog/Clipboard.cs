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
using System.Threading.Tasks;

namespace TextPaint
{
    /// <summary>
    /// Description of Clipboard.
    /// </summary>
    public class Clipboard
    {
        static List<List<int>> TextClipboardT = new List<List<int>>();
        static List<List<int>> TextClipboardC = new List<List<int>>();
        static List<List<int>> TextClipboardF = new List<List<int>>();

        public static void TextClipboardClear()
        {
            TextClipboardT.Clear();
            TextClipboardC.Clear();
            TextClipboardF.Clear();
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

        public static int TextClipboardGetF(int X, int Y)
        {
            if (TextClipboardF.Count > Y)
            {
                if (TextClipboardF[Y].Count > X)
                {
                    return TextClipboardF[Y][X];
                }
            }
            return 0;
        }

        public static void TextClipboardSet(int X, int Y, int T, int C, int F)
        {
            while (TextClipboardT.Count <= Y)
            {
                TextClipboardT.Add(new List<int>());
                TextClipboardC.Add(new List<int>());
                TextClipboardF.Add(new List<int>());
            }
            while (TextClipboardT[Y].Count <= X)
            {
                TextClipboardT[Y].Add(TextWork.SpaceChar0);
                TextClipboardC[Y].Add(0);
                TextClipboardF[Y].Add(0);
            }
            TextClipboardT[Y][X] = T;
            TextClipboardC[Y][X] = C;
            TextClipboardF[Y][X] = F;
        }

        private static string LastSysText = "";

        private static async Task<string> SysClipboardGetSystem()
        {
            try
            {
                return await Avalonia.Application.Current.Clipboard.GetTextAsync();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> SysClipboardSetSystem(string Txt)
        {
            try
            {
                await Avalonia.Application.Current.Clipboard.SetTextAsync(Txt);
                return await Avalonia.Application.Current.Clipboard.GetTextAsync();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> SysClipboardGet()
        {
            string Txt_ = await SysClipboardGetSystem();
            if (Txt_ == null)
            {
                LastSysText = "";
                return false;
            }
            if (Txt_ != LastSysText)
            {
                string[] Txt = Txt_.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                TextClipboardT.Clear();
                TextClipboardC.Clear();
                TextClipboardF.Clear();
                for (int i = 0; i < Txt.Length; i++)
                {
                    TextClipboardT.Add(TextWork.StrToInt(Txt[i]));
                    TextClipboardC.Add(TextWork.BlkCol(TextClipboardT[i].Count));
                    TextClipboardF.Add(TextWork.BlkCol(TextClipboardT[i].Count));
                }
            }
            return true;
        }

        public static async Task<int> SysClipboardSet()
        {
            System.Text.StringBuilder Txt = new System.Text.StringBuilder();
            for (int i = 0; i < TextClipboardT.Count; i++)
            {
                Txt.AppendLine(TextWork.IntToStr(TextClipboardT[i]));
            }
            LastSysText = await SysClipboardSetSystem(Txt.ToString());
            if (LastSysText == null)
            {
                LastSysText = "";
            }
            return 0;
        }


        public Core Core_;

        public Clipboard(Core Core__)
        {
            Core_ = Core__;
        }

        public void CharPut(int X, int Y, int Ch, int Col, int Fon)
        {
            if (Fon >= 0)
            {
                Core_.CharPut(X, Y, Ch, Col, Fon, true);
            }
            else
            {
                Core_.CharPut(X, Y, Ch, Col, 0, false);
            }
        }

        public int CharGet(int X, int Y)
        {
            return Core_.CharGet(X, Y, true, false);
        }

        public int ColoGet(int X, int Y)
        {
            return Core_.ColoGet(X, Y, true, false);
        }

        public int FontGet(int X, int Y)
        {
            return Core_.FontGet(X, Y);
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
                if (await SysClipboardGet())
                {
                    for (int YY = 0; YY <= H_; YY++)
                    {
                        for (int XX = 0; XX <= W_; XX++)
                        {
                            if (IsInsideSelection(W_, H_, XX, YY, TX, TY))
                            {
                                int ElementT = TextClipboardGetT(XX, YY);
                                int ElementC = TextClipboardGetC(XX, YY);
                                int ElementF = -1;
                                if (PreserveFont)
                                {
                                    ElementF = TextClipboardGetF(XX, YY);
                                }
                                CharPut(X + (XX * FontW), Y + (YY * FontH), ElementT, ElementC, ElementF);
                            }
                        }
                    }
                }
            }
            else
            {
                TextClipboardClear();
                for (int YY = 0; YY <= H_; YY++)
                {
                    for (int XX = 0; XX <= W_; XX++)
                    {
                        if (IsInsideSelection(W_, H_, XX, YY, TX, TY))
                        {
                            int ElementT = CharGet(X + (XX * FontW), Y + (YY * FontH));
                            int ElementC = ColoGet(X + (XX * FontW), Y + (YY * FontH));
                            int ElementF = 0;
                            if (PreserveFont)
                            {
                                ElementF = FontGet(X + (XX * FontW), Y + (YY * FontH));
                            }
                            TextClipboardSet(XX, YY, ElementT, ElementC, ElementF);
                        }
                        else
                        {
                            TextClipboardSet(XX, YY, TextWork.SpaceChar0, 0, 0);
                        }
                    }
                }
                await SysClipboardSet();
            }

            TextClipboardWorkEvent2(Paste);
        }
    }
}
