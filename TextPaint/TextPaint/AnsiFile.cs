using System;
using System.Collections.Generic;

namespace TextPaint
{
    public class AnsiFile
    {
        int LastB = -1;
        int LastF = -1;
        bool LastBlink = false;
        bool LastBold = false;
        int LastFontW = 0;
        int LastFontH = 0;

        public void Reset()
        {
            LastB = -1;
            LastF = -1;
            LastBlink = false;
            LastBold = false;
            LastFontW = 0;
            LastFontH = 0;
        }

        public List<int> Process(List<int> TextBuffer, List<int> TextColBuf, bool DOS, bool LinePrefix, bool LinePostfix, int AnsiMaxX, bool AnsiColorBackBlink, bool AnsiColorForeBold)
        {
            List<int> TextFileLine = new List<int>();

            // Beginning of line - reset colors and attributes
            if (LinePrefix)
            {
                //TextFileLine.Add(27);
                //TextFileLine.Add('[');
                //TextFileLine.Add('0');
                //TextFileLine.Add('m');
            }

            LastFontW = 0;
            LastFontH = 0;

            for (int ii = 0; ii < TextBuffer.Count; ii++)
            {
                // Get color of current character
                int TempB;
                int TempF;
                int TempFontW;
                int TempFontH;

                Core.ColorFromInt(TextColBuf[ii], out TempB, out TempF, out TempFontW, out TempFontH);

                // Font size change
                if ((LastFontW != TempFontW) || (LastFontH != TempFontH))
                {
                    TextFileLine.Add(27);
                    TextFileLine.AddRange(TextWork.StrToInt("[0;"));
                    TextFileLine.AddRange(TextWork.StrToInt(TempFontW.ToString()));
                    TextFileLine.Add(';');
                    TextFileLine.AddRange(TextWork.StrToInt(TempFontH.ToString()));
                    TextFileLine.Add('V');
                    LastFontW = TempFontW;
                    LastFontH = TempFontH;
                }

                // Use escape codes only if color differs from last color
                if ((LastB != TempB) || (LastF != TempF))
                {
                    // Default color - reset attributes
                    if (((TempB < 0) && (LastB >= 0)) || ((TempF < 0) && (LastF >= 0)))
                    {
                        TextFileLine.Add(27);
                        TextFileLine.Add('[');
                        TextFileLine.Add('0');
                        TextFileLine.Add('m');
                        LastB = -1;
                        LastF = -1;
                        LastBlink = false;
                        LastBold = false;
                    }

                    // Background color change
                    if (LastB != TempB)
                    {
                        if ((TempB >= 0) && (TempB <= 7))
                        {
                            TextFileLine.Add(27);
                            TextFileLine.Add('[');
                            TextFileLine.Add('4');
                            TextFileLine.Add(48 + TempB);
                            if (AnsiColorBackBlink)
                            {
                                if (LastBlink)
                                {
                                    TextFileLine.Add(';');
                                    TextFileLine.Add('2');
                                    TextFileLine.Add('5');
                                    LastBlink = false;
                                }
                            }
                            TextFileLine.Add('m');
                        }
                        if ((TempB >= 8) && (TempB <= 15))
                        {
                            if (AnsiColorBackBlink)
                            {
                                TextFileLine.Add(27);
                                TextFileLine.Add('[');
                                TextFileLine.Add('4');
                                TextFileLine.Add(40 + TempB);
                                if (AnsiColorBackBlink)
                                {
                                    if (!LastBlink)
                                    {
                                        TextFileLine.Add(';');
                                        TextFileLine.Add('5');
                                        LastBlink = true;
                                    }
                                }
                                TextFileLine.Add('m');
                            }
                            else
                            {
                                TextFileLine.Add(27);
                                TextFileLine.Add('[');
                                TextFileLine.Add('1');
                                TextFileLine.Add('0');
                                TextFileLine.Add(40 + TempB);
                                TextFileLine.Add('m');
                            }
                        }
                        LastB = TempB;
                    }

                    // Foreground color change
                    if (LastF != TempF)
                    {
                        if ((TempF >= 0) && (TempF <= 7))
                        {
                            TextFileLine.Add(27);
                            TextFileLine.Add('[');
                            TextFileLine.Add('3');
                            TextFileLine.Add(48 + TempF);
                            if (AnsiColorForeBold)
                            {
                                if (LastBold)
                                {
                                    TextFileLine.Add(';');
                                    TextFileLine.Add('2');
                                    TextFileLine.Add('2');
                                    LastBold = false;
                                }
                            }
                            TextFileLine.Add('m');
                        }
                        if ((TempF >= 8) && (TempF <= 15))
                        {
                            if (AnsiColorForeBold)
                            {
                                TextFileLine.Add(27);
                                TextFileLine.Add('[');
                                TextFileLine.Add('3');
                                TextFileLine.Add(40 + TempF);
                                if (!LastBold)
                                {
                                    TextFileLine.Add(';');
                                    TextFileLine.Add('1');
                                    LastBold = true;
                                }
                                TextFileLine.Add('m');
                            }
                            else
                            {
                                TextFileLine.Add(27);
                                TextFileLine.Add('[');
                                TextFileLine.Add('9');
                                TextFileLine.Add(40 + TempF);
                                TextFileLine.Add('m');
                            }
                        }
                        LastF = TempF;
                    }
                }
                if (ii < AnsiMaxX)
                {
                    TextFileLine.Add(TextBuffer[ii]);
                    if (LastFontW > 0)
                    {
                        LastFontW++;
                        switch (LastFontW)
                        {
                            case 3:
                                LastFontW -= 2;
                                break;
                            case 6:
                                LastFontW -= 3;
                                break;
                            case 10:
                                LastFontW -= 4;
                                break;
                        }
                    }
                }
            }

            // End of line - reset colors and attributes
            if (LinePostfix)
            {
                //TextFileLine.Add(27);
                //TextFileLine.Add('[');
                //TextFileLine.Add('0');
                //TextFileLine.Add('m');
            }

            // End of line characters
            if ((!DOS) || (TextBuffer.Count < AnsiMaxX))
            {
                TextFileLine.Add(13);
                TextFileLine.Add(10);
            }

            return TextFileLine;
        }
    }
}
