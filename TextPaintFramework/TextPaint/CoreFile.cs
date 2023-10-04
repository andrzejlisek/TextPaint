using System;
using System.Collections.Generic;
using System.IO;

namespace TextPaint
{
    public class CoreFile
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

        public string FileREnc = "";
        public string FileWEnc = "";

        public Core Core_;

        public CoreFile(Core Core__, ConfigFile CF)
        {
            Core_ = Core__;
            FileREnc = CF.ParamGetS("FileReadEncoding");
            FileWEnc = CF.ParamGetS("FileWriteEncoding");
            FileReadSteps = CF.ParamGetI("FileReadSteps");
            UseAnsiLoad = CF.ParamGetB("ANSIRead");
            UseAnsiSave = CF.ParamGetB("ANSIWrite");
        }

        public void FileLoad(string FileName)
        {
            Core_.TempMemo.Push(Core_.ToggleDrawColo ? 1 : 0);
            Core_.TempMemo.Push(Core_.ToggleDrawText ? 1 : 0);
            Core_.ToggleDrawColo = true;
            Core_.ToggleDrawText = true;
            Core_.TextBuffer.Clear();
            if ("".Equals(FileName))
            {
                return;
            }
            try
            {
                Core_.TextCipher_.Reset();
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

                Core_.CoreAnsi_.AnsiProcessReset(true, true, 0);
                Core_.CoreAnsi_.AnsiRingBell = false;

                string Buf;
                int TestLines = 0;

                if (UseAnsiLoad)
                {
                    Buf = SR.ReadToEnd();
                    List<int> TextFileLine_ = Core_.TextCipher_.Crypt(TextWork.StrToInt(Buf), true);
                    Core_.CoreAnsi_.AnsiProcessSupply(TextFileLine_);
                    if (FileReadSteps > 0)
                    {
                        Core_.CoreAnsi_.AnsiProcess(FileReadSteps);
                    }
                    else
                    {
                        Core_.CoreAnsi_.AnsiProcess(-1);
                    }
                    Core_.TextBuffer.Clear();
                    int i_ = 0;


                    // Before screen
                    for (int i = 0; i < Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy1__.CountLines(); i++)
                    {
                        Core_.TextBuffer.AppendLine();
                        int LineMax = (Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy1__.CountItems(i)) - 1;
                        for (int ii = 0; ii <= LineMax; ii++)
                        {
                            Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy1__.Get(i, ii);
                            Core_.TextBuffer.CopyItem(Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy1__);
                            Core_.TextBuffer.Append(i_);
                        }
                        i_++;
                    }

                    // Screen
                    for (int i = 0; i < Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__.CountLines(); i++)
                    {
                        Core_.TextBuffer.AppendLine();
                        int LineMax = (Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__.CountItems(i)) - 1;
                        for (int ii = 0; ii <= LineMax; ii++)
                        {
                            Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__.Get(i, ii);
                            Core_.TextBuffer.CopyItem(Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__);
                            Core_.TextBuffer.Append(i_);
                        }
                        i_++;
                    }

                    // After screen
                    for (int i = (Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy2__.CountLines() - 1); i >= 0; i--)
                    {
                        Core_.TextBuffer.AppendLine();
                        int LineMax = (Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy2__.CountItems(i)) - 1;
                        for (int ii = 0; ii <= LineMax; ii++)
                        {
                            Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy2__.Get(i, ii);
                            Core_.TextBuffer.CopyItem(Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy2__);
                            Core_.TextBuffer.Append(i_);
                        }
                        i_++;
                    }
                }
                else
                {
                    Core_.TextBuffer.BlankChar();
                    Buf = SR.ReadLine();
                    int i_ = 0;
                    while (Buf != null)
                    {
                        TestLines++;
                        List<int> TextFileLine = Core_.TextCipher_.Crypt(TextWork.StrToInt(Buf), true);
                        Core_.TextBuffer.AppendLine();
                        Core_.TextBuffer.SetLineString(i_, TextFileLine);
                        Buf = SR.ReadLine();
                        i_++;
                    }
                }
                SR.Close();
                FS.Close();
                Core_.TextBuffer.TrimLines();
                Core_.UndoBufferClear();
            }
            catch (Exception E)
            {
                Core_.CoreAnsi_.AnsiProcessReset(true, true, 0);
                Core_.CoreAnsi_.AnsiProcessSupply(TextWork.StrToInt(E.Message));
                Core_.CoreAnsi_.AnsiProcess(-1);
                Core_.TextBuffer.Clear();
                for (int i = 0; i < Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__.CountLines(); i++)
                {
                    int LineMax = (Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__.CountItems(i)) - 1;
                    Core_.TextBuffer.AppendLine();
                    for (int ii = 0; ii <= LineMax; ii++)
                    {
                        Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__.Get(i, ii);
                        Core_.TextBuffer.CopyItem(Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__);
                        Core_.TextBuffer.Append(i);
                    }
                }
            }
            Core_.ToggleDrawText = (Core_.TempMemo.Pop() == 1);
            Core_.ToggleDrawColo = (Core_.TempMemo.Pop() == 1);
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
                Core_.TextCipher_.Reset();
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
                for (int i = 0; i < Core_.TextBuffer.CountLines(); i++)
                {
                    List<int> TextFileLine;
                    if (UseAnsiSave)
                    {
                        bool LinePrefix = (i == 0);
                        bool LinePostfix = (i == (Core_.TextBuffer.CountLines() - 1));
                        TextFileLine = AnsiFile_.Process(Core_.TextBuffer, i, LinePrefix, LinePostfix, Core_.CoreAnsi_.AnsiMaxX, Core_.Screen_.CharDoubleTable, Core_.Screen_.CharDoubleTableInv);
                        SW.Write(TextWork.IntToStr(Core_.TextCipher_.Crypt(TextFileLine, false)));
                    }
                    else
                    {
                        TextFileLine = Core_.TextBuffer.GetLineString(i);
                        SW.WriteLine(TextWork.IntToStr(Core_.TextCipher_.Crypt(TextFileLine, false)));
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
