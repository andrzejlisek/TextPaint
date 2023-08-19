using System;
using System.Collections.Generic;

namespace TextPaint
{
    public class AnsiFile
    {
        int LastB = -1;
        int LastF = -1;
        int LastA = -1;
        int LastFontW = 0;
        int LastFontH = 0;

        public void Reset()
        {
            LastB = -1;
            LastF = -1;
            LastA = -1;
            LastFontW = 0;
            LastFontH = 0;
        }

        public List<int> Process(AnsiLineOccupyEx TextBuffer, int TextBufferI, bool LinePrefix, bool LinePostfix, int AnsiMaxX)
        {
            List<int> TextFileLine = new List<int>();

            for (int ii = 0; ii < TextBuffer.CountItems(TextBufferI); ii++)
            {
                // Get color of current character
                TextBuffer.Get(TextBufferI, ii);

                // Font size change
                if ((LastFontW != TextBuffer.Item_FontW) || (LastFontH != TextBuffer.Item_FontH))
                {
                    TextFileLine.Add(27);
                    TextFileLine.AddRange(TextWork.StrToInt("[0;"));
                    TextFileLine.AddRange(TextWork.StrToInt(TextBuffer.Item_FontW.ToString()));
                    TextFileLine.Add(';');
                    TextFileLine.AddRange(TextWork.StrToInt(TextBuffer.Item_FontH.ToString()));
                    TextFileLine.Add('V');
                    LastFontW = TextBuffer.Item_FontW;
                    LastFontH = TextBuffer.Item_FontH;
                }

                // Use escape codes only if color differs from the last color
                if ((LastB != TextBuffer.Item_ColorB) || (LastF != TextBuffer.Item_ColorF) || (LastA != TextBuffer.Item_ColorA))
                {
                    // Attribute change prefix
                    TextFileLine.Add(27);
                    TextFileLine.Add('[');

                    bool TagSeparator = false;

                    // Default color - reset attributes
                    if (((TextBuffer.Item_ColorB < 0) && (LastB >= 0)) || ((TextBuffer.Item_ColorF < 0) && (LastF >= 0)))
                    {
                        if (TagSeparator) { TextFileLine.Add(';'); }
                        TextFileLine.Add('0');
                        LastB = -1;
                        LastF = -1;
                        LastA = 0;
                        TagSeparator = true;
                    }

                    // Background color change
                    if (LastB != TextBuffer.Item_ColorB)
                    {
                        if (TagSeparator) { TextFileLine.Add(';'); }
                        if ((TextBuffer.Item_ColorB >= 0) && (TextBuffer.Item_ColorB <= 7))
                        {
                            TextFileLine.Add('4');
                            TextFileLine.Add(48 + TextBuffer.Item_ColorB);
                        }
                        if ((TextBuffer.Item_ColorB >= 8) && (TextBuffer.Item_ColorB <= 15))
                        {
                            TextFileLine.Add('1');
                            TextFileLine.Add('0');
                            TextFileLine.Add(40 + TextBuffer.Item_ColorB);
                        }
                        TagSeparator = true;
                        LastB = TextBuffer.Item_ColorB;
                    }

                    // Foreground color change
                    if (LastF != TextBuffer.Item_ColorF)
                    {
                        if (TagSeparator) { TextFileLine.Add(';'); }
                        if ((TextBuffer.Item_ColorF >= 0) && (TextBuffer.Item_ColorF <= 7))
                        {
                            TextFileLine.Add('3');
                            TextFileLine.Add(48 + TextBuffer.Item_ColorF);
                        }
                        if ((TextBuffer.Item_ColorF >= 8) && (TextBuffer.Item_ColorF <= 15))
                        {
                            TextFileLine.Add('9');
                            TextFileLine.Add(40 + TextBuffer.Item_ColorF);
                        }
                        TagSeparator = true;
                        LastF = TextBuffer.Item_ColorF;
                    }

                    // Attribute change
                    if ((LastA & 0x7F) != (TextBuffer.Item_ColorA & 0x7F))
                    {
                        // Enable bold
                        if (((LastA & 0x01) == 0) && ((TextBuffer.Item_ColorA & 0x01) > 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('1');
                            TagSeparator = true;
                        }

                        // Disable bold
                        if (((LastA & 0x01) > 0) && ((TextBuffer.Item_ColorA & 0x01) == 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('2');
                            TextFileLine.Add('2');
                            TagSeparator = true;
                        }

                        // Enable italic
                        if (((LastA & 0x02) == 0) && ((TextBuffer.Item_ColorA & 0x02) > 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('3');
                            TagSeparator = true;
                        }

                        // Disable italic
                        if (((LastA & 0x02) > 0) && ((TextBuffer.Item_ColorA & 0x02) == 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('2');
                            TextFileLine.Add('3');
                            TagSeparator = true;
                        }

                        // Enable underline
                        if (((LastA & 0x04) == 0) && ((TextBuffer.Item_ColorA & 0x04) > 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('4');
                            TagSeparator = true;
                        }

                        // Disable underline
                        if (((LastA & 0x04) > 0) && ((TextBuffer.Item_ColorA & 0x04) == 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('2');
                            TextFileLine.Add('4');
                            TagSeparator = true;
                        }

                        // Enable blink
                        if (((LastA & 0x08) == 0) && ((TextBuffer.Item_ColorA & 0x08) > 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('5');
                            TagSeparator = true;
                        }

                        // Disable blink
                        if (((LastA & 0x08) > 0) && ((TextBuffer.Item_ColorA & 0x08) == 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('2');
                            TextFileLine.Add('5');
                            TagSeparator = true;
                        }

                        // Enable reverse
                        if (((LastA & 0x10) == 0) && ((TextBuffer.Item_ColorA & 0x10) > 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('7');
                            TagSeparator = true;
                        }

                        // Disable reverse
                        if (((LastA & 0x10) > 0) && ((TextBuffer.Item_ColorA & 0x10) == 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('2');
                            TextFileLine.Add('7');
                            TagSeparator = true;
                        }

                        // Enable conceale
                        if (((LastA & 0x20) == 0) && ((TextBuffer.Item_ColorA & 0x20) > 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('8');
                            TagSeparator = true;
                        }

                        // Disable conceale
                        if (((LastA & 0x20) > 0) && ((TextBuffer.Item_ColorA & 0x20) == 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('2');
                            TextFileLine.Add('8');
                            TagSeparator = true;
                        }

                        // Enable strikethrough
                        if (((LastA & 0x40) == 0) && ((TextBuffer.Item_ColorA & 0x40) > 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('9');
                            TagSeparator = true;
                        }

                        // Disable strikethrough
                        if (((LastA & 0x40) > 0) && ((TextBuffer.Item_ColorA & 0x40) == 0))
                        {
                            if (TagSeparator) { TextFileLine.Add(';'); }
                            TextFileLine.Add('2');
                            TextFileLine.Add('9');
                            TagSeparator = true;
                        }

                        LastA = TextBuffer.Item_ColorA;
                    }

                    // Attribute change suffix
                    TextFileLine.Add('m');
                }
                if (ii < AnsiMaxX)
                {
                    TextFileLine.Add(TextBuffer.Item_Char);
                    LastFontW = Core.FontCounter(LastFontW);
                }
            }

            if ((LastB >= 0) || (LastF >= 0) || (LastA > 0))
            {
                TextFileLine.Add(27);
                TextFileLine.Add('[');
                TextFileLine.Add('0');
                TextFileLine.Add('m');
                LastB = -1;
                LastF = -1;
                LastA = 0;
            }

            // End of line characters
            if (TextBuffer.CountLines() < AnsiMaxX)
            {
                TextFileLine.Add(13);
                TextFileLine.Add(10);
            }

            return TextFileLine;
        }
    }
}
