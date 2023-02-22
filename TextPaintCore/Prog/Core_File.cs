using System;
using System.Collections.Generic;
using System.IO;

namespace TextPaint
{
    public partial class Core
    {
        //public int TextNormalBack = 0;
        //public int TextNormalFore = 15;
        //public int TextBeyondLineBack = 8;
        //public int TextBeyondLineFore = 8;
        //public int TextBeyondEndBack = 7;
        //public int TextBeyondEndFore = 7;

        public bool UseAnsiLoad = false;
        public bool UseAnsiSave = false;
        public bool AnsiColorBackBlink = false;
        public bool AnsiColorForeBold = false;
        public int FileReadChars = 0;


        public int TextNormalBack = 0;
        public int TextNormalFore = 7;
        public int TextBeyondLineBack = 8;
        public int TextBeyondLineFore = 7;
        public int TextBeyondEndBack = 7;
        public int TextBeyondEndFore = 7;
        public int TextBeyondLineMargin = 0;

        public int CursorBack = 15;
        public int CursorFore = 0;
        public int StatusBack = 15;
        public int StatusFore = 0;
        public int PopupBack = 0;
        public int PopupFore = 15;


        public static int ColorToInt(int Back, int Fore)
        {
            int Col = 0;
            if (Back >= 0)
            {
                Col = Col + Back;
                Col = Col + (1 << 16);
            }
            if (Fore >= 0)
            {
                Col = Col + (Fore << 8);
                Col = Col + (1 << 17);
            }
            return Col;
        }

        public static int FontSToInt(int FontW, int FontH)
        {
            int Col = 0;
            Col = Col + (FontW);
            Col = Col + (FontH << 10);
            return Col;
        }


        public static void ColorFromInt(int Col, out int Back, out int Fore)
        {
            Back = -1;
            Fore = -1;
            if ((Col & (1 << 16)) > 0)
            {
                Back = Col & 255;
            }
            if ((Col & (1 << 17)) > 0)
            {
                Fore = (Col >> 8) & 255;
            }
        }

        public static void FontSFromInt(int Col, out int FontW, out int FontH)
        {
            FontW = 0;
            FontH = 0;
            FontW = (Col) & 1023;
            FontH = (Col >> 10) & 1023;
        }


        public void FileLoad(string FileName)
        {
            TempMemo.Push(ToggleDrawColo ? 1 : 0);
            TempMemo.Push(ToggleDrawText ? 1 : 0);
            ToggleDrawColo = true;
            ToggleDrawText = true;
            TextBuffer.Clear();
            TextColBuf.Clear();
            TextFonBuf.Clear();
            if ("".Equals(FileName))
            {
                return;
            }
            try
            {
                TextCipher_.Reset();
                FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                StreamReader SR;
                if ("".Equals(FileREnc))
                {
                    SR = new StreamReader(FS);
                }
                else
                {
                    SR = new StreamReader(FS, TextWork.EncodingFromName(FileREnc));
                }

                AnsiProcessReset(true);

                string Buf;
                int TestLines = 0;

                if (UseAnsiLoad)
                {
                    Buf = SR.ReadToEnd();
                    List<int> TextFileLine_ = TextCipher_.Crypt(TextWork.StrToInt(Buf), true);
                    if (FileReadChars > 0)
                    {
                        AnsiProcessSupply(TextFileLine_.GetRange(0, FileReadChars));
                    }
                    else
                    {
                        AnsiProcessSupply(TextFileLine_);
                    }
                    AnsiProcess(-1);
                    TextBuffer.Clear();
                    TextColBuf.Clear();
                    TextFonBuf.Clear();


                    // Before screen
                    for (int i = 0; i < __AnsiLineOccupy1.Count; i++)
                    {
                        List<int> TextBufferLine = new List<int>();
                        List<int> TextColBufLine = new List<int>();
                        List<int> TextFonBufLine = new List<int>();
                        int LineMax = (__AnsiLineOccupy1[i].Count / __AnsiLineOccupyFactor) - 1;
                        for (int ii = 0; ii <= LineMax; ii++)
                        {
                            TextBufferLine.Add(__AnsiLineOccupy1[i][ii * __AnsiLineOccupyFactor + 0]);
                            int ColorB = __AnsiLineOccupy1[i][ii * __AnsiLineOccupyFactor + 1];
                            int ColorF = __AnsiLineOccupy1[i][ii * __AnsiLineOccupyFactor + 2];
                            int FontW = __AnsiLineOccupy1[i][ii * __AnsiLineOccupyFactor + 3];
                            int FontH = __AnsiLineOccupy1[i][ii * __AnsiLineOccupyFactor + 4];
                            TextColBufLine.Add(ColorToInt(ColorB, ColorF));
                            TextFonBufLine.Add(FontSToInt(FontW, FontH));
                        }

                        TextBuffer.Add(TextBufferLine);
                        TextColBuf.Add(TextColBufLine);
                        TextFonBuf.Add(TextFonBufLine);
                    }

                    // Screen
                    for (int i = 0; i < __AnsiLineOccupy.Count; i++)
                    {
                        List<int> TextBufferLine = new List<int>();
                        List<int> TextColBufLine = new List<int>();
                        List<int> TextFonBufLine = new List<int>();
                        int LineMax = (__AnsiLineOccupy[i].Count / __AnsiLineOccupyFactor) - 1;
                        for (int ii = 0; ii <= LineMax; ii++)
                        {
                            TextBufferLine.Add(__AnsiLineOccupy[i][ii * __AnsiLineOccupyFactor + 0]);
                            int ColorB = __AnsiLineOccupy[i][ii * __AnsiLineOccupyFactor + 1];
                            int ColorF = __AnsiLineOccupy[i][ii * __AnsiLineOccupyFactor + 2];
                            int FontW = __AnsiLineOccupy[i][ii * __AnsiLineOccupyFactor + 3];
                            int FontH = __AnsiLineOccupy[i][ii * __AnsiLineOccupyFactor + 4];
                            TextColBufLine.Add(ColorToInt(ColorB, ColorF));
                            TextFonBufLine.Add(FontSToInt(FontW, FontH));
                        }

                        TextBuffer.Add(TextBufferLine);
                        TextColBuf.Add(TextColBufLine);
                        TextFonBuf.Add(TextFonBufLine);
                    }

                    // After screen
                    for (int i = (__AnsiLineOccupy2.Count - 1); i >= 0; i--)
                    {
                        List<int> TextBufferLine = new List<int>();
                        List<int> TextColBufLine = new List<int>();
                        List<int> TextFonBufLine = new List<int>();
                        int LineMax = (__AnsiLineOccupy2[i].Count / __AnsiLineOccupyFactor) - 1;
                        for (int ii = 0; ii <= LineMax; ii++)
                        {
                            TextBufferLine.Add(__AnsiLineOccupy2[i][ii * __AnsiLineOccupyFactor + 0]);
                            int ColorB = __AnsiLineOccupy2[i][ii * __AnsiLineOccupyFactor + 1];
                            int ColorF = __AnsiLineOccupy2[i][ii * __AnsiLineOccupyFactor + 2];
                            int FontW = __AnsiLineOccupy2[i][ii * __AnsiLineOccupyFactor + 3];
                            int FontH = __AnsiLineOccupy2[i][ii * __AnsiLineOccupyFactor + 4];
                            TextColBufLine.Add(ColorToInt(ColorB, ColorF));
                            TextFonBufLine.Add(FontSToInt(FontW, FontH));
                        }

                        TextBuffer.Add(TextBufferLine);
                        TextColBuf.Add(TextColBufLine);
                        TextFonBuf.Add(TextFonBufLine);
                    }
                }
                else
                {
                    Buf = SR.ReadLine();
                    while (Buf != null)
                    {
                        TestLines++;
                        List<int> TextFileLine = TextCipher_.Crypt(TextWork.StrToInt(Buf), true);
                        TextBuffer.Add(TextFileLine);
                        TextColBuf.Add(TextWork.BlkCol(TextFileLine.Count));
                        TextFonBuf.Add(TextWork.BlkCol(TextFileLine.Count));
                        Buf = SR.ReadLine();
                    }
                }
                AnsiEnd();
                SR.Close();
                FS.Close();
                TextBufferTrim();
                UndoBufferClear();
            }
            catch
            {

            }
            ToggleDrawText = (TempMemo.Pop() == 1);
            ToggleDrawColo = (TempMemo.Pop() == 1);
        }


        public void FileSave(string FileName)
        {
            if ("".Equals(FileName))
            {
                return;
            }
            try
            {
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
                TextCipher_.Reset();
                FileStream FS = new FileStream(FileName, FileMode.Create, FileAccess.Write);
                StreamWriter SW;
                if ("".Equals(FileWEnc))
                {
                    SW = new StreamWriter(FS);
                }
                else
                {
                    SW = new StreamWriter(FS, TextWork.EncodingFromName(FileWEnc));
                }
                AnsiFile AnsiFile_ = new AnsiFile();
                AnsiFile_.Reset();
                for (int i = 0; i < TextBuffer.Count; i++)
                {
                    List<int> TextFileLine;
                    if (UseAnsiSave)
                    {
                        bool LinePrefix = (i == 0);
                        bool LinePostfix = (i == (TextBuffer.Count - 1));
                        TextFileLine = AnsiFile_.Process(TextBuffer[i], TextColBuf[i], TextFonBuf[i], LinePrefix, LinePostfix, AnsiMaxX, AnsiColorBackBlink, AnsiColorForeBold);
                        SW.Write(TextWork.IntToStr(TextCipher_.Crypt(TextFileLine, false)));
                    }
                    else
                    {
                        TextFileLine = TextBuffer[i];
                        SW.WriteLine(TextWork.IntToStr(TextCipher_.Crypt(TextFileLine, false)));
                    }
                }
                SW.Close();
                FS.Close();
            }
            catch
            {

            }
        }
    }
}
