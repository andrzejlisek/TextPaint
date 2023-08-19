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
        public int FileReadSteps = 0;


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


        public static void ColorFromInt(int Col, out int Back, out int Fore, out int Attrib)
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
            Attrib = 0;
        }

        public static void FontSFromInt(int Col, out int FontW, out int FontH)
        {
            FontW = 0;
            FontH = 0;
            FontW = (Col) & 1023;
            FontH = (Col >> 10) & 1023;
        }

        public bool FileExists(string FileName)
        {
            if (File.Exists(FileName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void FileLoad(string FileName)
        {
            TempMemo.Push(ToggleDrawColo ? 1 : 0);
            TempMemo.Push(ToggleDrawText ? 1 : 0);
            ToggleDrawColo = true;
            ToggleDrawText = true;
            TextBuffer.Clear();
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

                AnsiProcessReset(true, true, 0);
                AnsiRingBell = false;

                string Buf;
                int TestLines = 0;

                if (UseAnsiLoad)
                {
                    Buf = SR.ReadToEnd();
                    List<int> TextFileLine_ = TextCipher_.Crypt(TextWork.StrToInt(Buf), true);
                    AnsiProcessSupply(TextFileLine_);
                    if (FileReadSteps > 0)
                    {
                        AnsiProcess(FileReadSteps);
                    }
                    else
                    {
                        AnsiProcess(-1);
                    }
                    TextBuffer.Clear();
                    int i_ = 0;


                    // Before screen
                    for (int i = 0; i < AnsiState_.__AnsiLineOccupy1__.CountLines(); i++)
                    {
                        TextBuffer.AppendLine();
                        int LineMax = (AnsiState_.__AnsiLineOccupy1__.CountItems(i)) - 1;
                        for (int ii = 0; ii <= LineMax; ii++)
                        {
                            AnsiState_.__AnsiLineOccupy1__.Get(i, ii);
                            TextBuffer.CopyItem(AnsiState_.__AnsiLineOccupy1__);
                            TextBuffer.Append(i_);
                        }
                        i_++;
                    }

                    // Screen
                    for (int i = 0; i < AnsiState_.__AnsiLineOccupy__.CountLines(); i++)
                    {
                        TextBuffer.AppendLine();
                        int LineMax = (AnsiState_.__AnsiLineOccupy__.CountItems(i)) - 1;
                        for (int ii = 0; ii <= LineMax; ii++)
                        {
                            AnsiState_.__AnsiLineOccupy__.Get(i, ii);
                            TextBuffer.CopyItem(AnsiState_.__AnsiLineOccupy__);
                            TextBuffer.Append(i_);
                        }
                        i_++;
                    }

                    // After screen
                    for (int i = (AnsiState_.__AnsiLineOccupy2__.CountLines() - 1); i >= 0; i--)
                    {
                        TextBuffer.AppendLine();
                        int LineMax = (AnsiState_.__AnsiLineOccupy2__.CountItems(i)) - 1;
                        for (int ii = 0; ii <= LineMax; ii++)
                        {
                            AnsiState_.__AnsiLineOccupy2__.Get(i, ii);
                            TextBuffer.CopyItem(AnsiState_.__AnsiLineOccupy2__);
                            TextBuffer.Append(i_);
                        }
                        i_++;
                    }
                }
                else
                {
                    TextBuffer.BlankChar();
                    Buf = SR.ReadLine();
                    int i_ = 0;
                    while (Buf != null)
                    {
                        TestLines++;
                        List<int> TextFileLine = TextCipher_.Crypt(TextWork.StrToInt(Buf), true);
                        TextBuffer.AppendLine();
                        TextBuffer.SetLineString(i_, TextFileLine);
                        Buf = SR.ReadLine();
                        i_++;
                    }
                }
                AnsiEnd();
                SR.Close();
                FS.Close();
                TextBuffer.TrimLines();
                UndoBufferClear();
            }
            catch (Exception E)
            {
                AnsiProcessReset(true, true, 0);
                AnsiProcessSupply(TextWork.StrToInt(E.Message));
                AnsiProcess(-1);
                TextBuffer.Clear();
                for (int i = 0; i < AnsiState_.__AnsiLineOccupy__.CountLines(); i++)
                {
                    int LineMax = (AnsiState_.__AnsiLineOccupy__.CountItems(i)) - 1;
                    TextBuffer.AppendLine();
                    for (int ii = 0; ii <= LineMax; ii++)
                    {
                        AnsiState_.__AnsiLineOccupy__.Get(i, ii);
                        TextBuffer.CopyItem(AnsiState_.__AnsiLineOccupy__);
                        TextBuffer.Append(i);
                    }
                }
                AnsiEnd();
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
                for (int i = 0; i < TextBuffer.CountLines(); i++)
                {
                    List<int> TextFileLine;
                    if (UseAnsiSave)
                    {
                        bool LinePrefix = (i == 0);
                        bool LinePostfix = (i == (TextBuffer.CountLines() - 1));
                        TextFileLine = AnsiFile_.Process(TextBuffer, i, LinePrefix, LinePostfix, AnsiMaxX);
                        SW.Write(TextWork.IntToStr(TextCipher_.Crypt(TextFileLine, false)));
                    }
                    else
                    {
                        TextFileLine = TextBuffer.GetLineString(i);
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

        public static string PrepareFileName(string NewFile_)
        {
            while ((NewFile_.Length > 0) && (TextWork.SpaceChars.Contains(NewFile_[0])))
            {
                NewFile_ = NewFile_.Substring(1);
            }
            while ((NewFile_.Length > 0) && (TextWork.SpaceChars.Contains(NewFile_[NewFile_.Length - 1])))
            {
                NewFile_ = NewFile_.Substring(0, NewFile_.Length - 1);
            }
            if (NewFile_.Length > 2)
            {
                if ((NewFile_[0] == '\"') && (NewFile_[NewFile_.Length - 1] == '\"'))
                {
                    NewFile_ = NewFile_.Substring(1, NewFile_.Length - 2);
                    return NewFile_;
                }
                if (((NewFile_[0] == '\'') || ((NewFile_[0] == '$') && (NewFile_[1] == '\''))) && (NewFile_[NewFile_.Length - 1] == '\''))
                {
                    bool InsideESC = false;
                    if (NewFile_[0] == '$')
                    {
                        InsideESC = true;
                        NewFile_ = NewFile_.Substring(2, NewFile_.Length - 3);
                    }
                    else
                    {
                        NewFile_ = NewFile_.Substring(1, NewFile_.Length - 2);
                    }
                    string NewFile_0 = "";
                    for (int i = 0; i < NewFile_.Length; i++)
                    {
                        if (NewFile_[i] == '\'')
                        {
                            InsideESC = !InsideESC;
                        }
                        else
                        {
                            if (InsideESC && (NewFile_[i] == '\\'))
                            {
                                i++;
                                switch (NewFile_[i])
                                {
                                    case '\'':
                                    case '\"':
                                    case '\\':
                                        NewFile_0 = NewFile_0 + NewFile_[i];
                                        break;
                                }
                            }
                            else
                            {
                                NewFile_0 = NewFile_0 + NewFile_[i];
                            }
                        }
                    }
                    return NewFile_0;
                }
                if ((NewFile_[0] == '/') && (NewFile_[1] != ':'))
                {
                    for (int i = 0; i < NewFile_.Length; i++)
                    {
                        if (NewFile_[i] == '\\')
                        {
                            NewFile_ = NewFile_.Remove(i, 1);
                        }
                    }
                }
            }
            return NewFile_;
        }

        public static string PrepareFileName(List<int> NewFile_)
        {
            while ((NewFile_.Count > 0) && (TextWork.SpaceChars.Contains(NewFile_[0])))
            {
                NewFile_.RemoveAt(0);
            }
            while ((NewFile_.Count > 0) && (TextWork.SpaceChars.Contains(NewFile_[NewFile_.Count - 1])))
            {
                NewFile_.RemoveAt(NewFile_.Count - 1);
            }
            if (NewFile_.Count > 2)
            {
                if ((NewFile_[0] == '\"') && (NewFile_[NewFile_.Count - 1] == '\"'))
                {
                    NewFile_ = NewFile_.GetRange(1, NewFile_.Count - 2);
                    return TextWork.IntToStr(NewFile_);
                }
                if (((NewFile_[0] == '\'') || ((NewFile_[0] == '$') && (NewFile_[1] == '\''))) && (NewFile_[NewFile_.Count - 1] == '\''))
                {
                    bool InsideESC = false;
                    if (NewFile_[0] == '$')
                    {
                        InsideESC = true;
                        NewFile_ = NewFile_.GetRange(2, NewFile_.Count - 3);
                    }
                    else
                    {
                        NewFile_ = NewFile_.GetRange(1, NewFile_.Count - 2);
                    }
                    List<int> NewFile_0 = new List<int>();
                    for (int i = 0; i < NewFile_.Count; i++)
                    {
                        if (NewFile_[i] == '\'')
                        {
                            InsideESC = !InsideESC;
                        }
                        else
                        {
                            if (InsideESC && (NewFile_[i] == '\\'))
                            {
                                i++;
                                switch (NewFile_[i])
                                {
                                    case '\'':
                                    case '\"':
                                    case '\\':
                                        NewFile_0.Add(NewFile_[i]);
                                        break;
                                }
                            }
                            else
                            {
                                NewFile_0.Add(NewFile_[i]);
                            }
                        }
                    }
                    return TextWork.IntToStr(NewFile_0);
                }
                if ((NewFile_[0] == '/') && (NewFile_[1] != ':'))
                {
                    for (int i = 0; i < NewFile_.Count; i++)
                    {
                        if (NewFile_[i] == '\\')
                        {
                            NewFile_.RemoveAt(i);
                        }
                    }
                }
            }
            return TextWork.IntToStr(NewFile_);
        }

        static void GetFileListSearchIndex(List<string> FileList, string FileName)
        {
            int GetFileListIdx0 = -1;
            GetFileListIdx = -1;
            for (int i = 0; i < FileList.Count; i++)
            {
                if (FileList[i] == FileName)
                {
                    GetFileListIdx = i;
                }
                if (FileList[i].ToLowerInvariant() == FileName.ToLowerInvariant())
                {
                    GetFileListIdx0 = i;
                }
            }
            if (GetFileListIdx < 0) { GetFileListIdx = GetFileListIdx0; }
        }

        public static int GetFileListIdx = 0;

        public static string GetFileListExtraInfo = "*";

        public static List<string> GetFileList(string FileName, string Wildcard)
        {
            List<string> FileList = new List<string>();

            FileName = Path.GetFullPath(FileName);

            if (File.Exists(FileName))
            {
                string FileDir = Path.GetDirectoryName(FileName);
                string[] Wildcard_ = Wildcard.Split(';');
                for (int i = 0; i < Wildcard_.Length; i++)
                {
                    string[] TempList = Directory.GetFiles(FileDir, Wildcard_[i], SearchOption.TopDirectoryOnly);
                    for (int ii = 0; ii < TempList.Length; ii++)
                    {
                        if (!FileList.Contains(TempList[ii]))
                        {
                            FileList.Add(TempList[ii]);
                        }
                    }
                }
                FileList.Sort();
                GetFileListSearchIndex(FileList, FileName);
                if (GetFileListIdx < 0)
                {
                    FileList.Add(FileName);
                    FileList.Sort();
                    GetFileListSearchIndex(FileList, FileName);
                    FileList[GetFileListIdx] = GetFileListExtraInfo + FileList[GetFileListIdx];
                }
            }
            else
            {
                FileList.Add(FileName);
                GetFileListIdx = 0;
            }
            return FileList;
        }

        public static void SaveFileDirectory(string FileName)
        {
            string FileName_D = Path.GetDirectoryName(FileName);
            if (!("".Equals(FileName_D)))
            {
                if (!Directory.Exists(FileName_D))
                {
                    Directory.CreateDirectory(FileName_D);
                }
            }
        }
    }
}
