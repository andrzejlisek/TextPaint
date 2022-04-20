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


        public int ColorToInt(int Back, int Fore)
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

        public void ColorFromInt(int Col, out int Back, out int Fore)
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

        public void FileAdd(int DataI, ref List<int> DataO)
        {
            switch (DataI)
            {
                case 13:
                    if (ANSI_CR == 0)
                    {
                        DataO.Add(13);
                    }
                    if (ANSI_CR == 1)
                    {
                        DataO.Add(13);
                        DataO.Add(10);
                    }
                    break;
                case 10:
                    if (ANSI_LF == 0)
                    {
                        DataO.Add(10);
                    }
                    if (ANSI_LF == 1)
                    {
                        DataO.Add(13);
                        DataO.Add(10);
                    }
                    break;
                default:
                    DataO.Add(DataI);
                    break;
            }
        }

        public void FileLoad(string FileName)
        {
            TempMemo.Push(ToggleDrawColo ? 1 : 0);
            TempMemo.Push(ToggleDrawText ? 1 : 0);
            ToggleDrawColo = true;
            ToggleDrawText = true;
            TextBuffer.Clear();
            TextColBuf.Clear();
            if (FileName == "")
            {
                return;
            }
            try
            {
                TextCipher_.Reset();
                FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                StreamReader SR;
                if (FileREnc != "")
                {
                    SR = new StreamReader(FS, TextWork.EncodingFromName(FileREnc));
                }
                else
                {
                    SR = new StreamReader(FS);
                }

                AnsiProcessReset(true);

                string Buf = null;
                int TestLines = 0;
                //Console.Clear();
                List<int> EOL = new List<int>();
                EOL.Add(13);
                EOL.Add(10);

                if (UseAnsiLoad)
                {
                    Buf = SR.ReadToEnd();
                    List<int> TextFileLine_ = TextCipher_.Crypt(TextWork.StrToInt(Buf), true);
                    List<int> TextFileLine = new List<int>();
                    for (int i = 0; i < TextFileLine_.Count; i++)
                    {
                        FileAdd(TextFileLine_[i], ref TextFileLine);
                    }
                    AnsiProcess(TextFileLine);
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
            if (FileName == "")
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
                if (FileWEnc != "")
                {
                    SW = new StreamWriter(FS, TextWork.EncodingFromName(FileWEnc));
                }
                else
                {
                    SW = new StreamWriter(FS);
                }
                int LastB = -1;
                int LastF = -1;
                for (int i = 0; i < TextBuffer.Count; i++)
                {
                    List<int> TextFileLine;
                    if (UseAnsiSave)
                    {
                        TextFileLine = new List<int>();

                        if (i == 0)
                        {
                            TextFileLine.Add(27);
                            TextFileLine.Add('[');
                            TextFileLine.Add('0');
                            TextFileLine.Add('m');
                        }

                        for (int ii = 0; ii < TextBuffer[i].Count; ii++)
                        {
                            int TempB;
                            int TempF;
                            ColorFromInt(TextColBuf[i][ii], out TempB, out TempF);
                            if ((LastB != TempB) || (LastF != TempF))
                            {
                                if ((TempB < 0) || (TempF < 0))
                                {
                                    TextFileLine.Add(27);
                                    TextFileLine.Add('[');
                                    TextFileLine.Add('0');
                                    TextFileLine.Add('m');
                                    LastB = -1;
                                    LastF = -1;
                                }
                                if (LastB != TempB)
                                {
                                    if ((TempB >= 0) && (TempB <= 7))
                                    {
                                        TextFileLine.Add(27);
                                        TextFileLine.Add('[');
                                        TextFileLine.Add('4');
                                        TextFileLine.Add(48 + TempB);
                                        TextFileLine.Add('m');
                                    }
                                    if ((TempB >= 8) && (TempB <= 15))
                                    {
                                        TextFileLine.Add(27);
                                        TextFileLine.Add('[');
                                        TextFileLine.Add('1');
                                        TextFileLine.Add('0');
                                        TextFileLine.Add(40 + TempB);
                                        TextFileLine.Add('m');
                                    }
                                    LastB = TempB;
                                }
                                if (LastF != TempF)
                                {
                                    if ((TempF >= 0) && (TempF <= 7))
                                    {
                                        TextFileLine.Add(27);
                                        TextFileLine.Add('[');
                                        TextFileLine.Add('3');
                                        TextFileLine.Add(48 + TempF);
                                        TextFileLine.Add('m');
                                    }
                                    if ((TempF >= 8) && (TempF <= 15))
                                    {
                                        TextFileLine.Add(27);
                                        TextFileLine.Add('[');
                                        TextFileLine.Add('9');
                                        TextFileLine.Add(40 + TempF);
                                        TextFileLine.Add('m');
                                    }
                                    LastF = TempF;
                                }
                            }
                            TextFileLine.Add(TextBuffer[i][ii]);
                        }

                        if (i == (TextBuffer.Count - 1))
                        {
                            TextFileLine.Add(27);
                            TextFileLine.Add('[');
                            TextFileLine.Add('0');
                            TextFileLine.Add('m');
                        }
                        TextFileLine.Add(13);
                        TextFileLine.Add(10);
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
