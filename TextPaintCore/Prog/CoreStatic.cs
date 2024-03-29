﻿using System;
using System.Collections.Generic;
using System.IO;

namespace TextPaint
{
    public class CoreStatic
    {
        public static int FontCounter(int CurrentValue)
        {
            if (CurrentValue == 0)
            {
                return 0;
            }
            switch (CurrentValue)
            {
                case 2: return 1;
                case 5: return 3;
                case 9: return 6;
                case 14: return 10;
                case 20: return 15;
                case 27: return 21;
                case 35: return 28;
                case 44: return 36;
                case 54: return 45;
                case 65: return 55;
                case 77: return 66;
                case 90: return 78;
                case 104: return 91;
                case 119: return 105;
                case 135: return 120;
                case 152: return 136;
                case 170: return 153;
                case 189: return 171;
                case 209: return 190;
                case 230: return 210;
                case 252: return 231;
                case 275: return 253;
                case 299: return 276;
                case 324: return 300;
                case 350: return 325;
                case 377: return 351;
                case 405: return 378;
                case 434: return 406;
                case 464: return 435;
                case 495: return 465;
                case 527: return 496;
                default: return CurrentValue + 1;
            }
        }

        public static int FontSizeCode(int S, int N)
        {
            switch (S)
            {
                default: return 0;
                case 2: return N + 1;
                case 3: return N + 3;
                case 4: return N + 6;
                case 5: return N + 10;
                case 6: return N + 15;
                case 7: return N + 21;
                case 8: return N + 28;
                case 9: return N + 36;
                case 10: return N + 45;
                case 11: return N + 55;
                case 12: return N + 66;
                case 13: return N + 78;
                case 14: return N + 91;
                case 15: return N + 105;
                case 16: return N + 120;
                case 17: return N + 136;
                case 18: return N + 153;
                case 19: return N + 171;
                case 20: return N + 190;
                case 21: return N + 210;
                case 22: return N + 231;
                case 23: return N + 253;
                case 24: return N + 276;
                case 25: return N + 300;
                case 26: return N + 325;
                case 27: return N + 351;
                case 28: return N + 378;
                case 29: return N + 406;
                case 30: return N + 435;
                case 31: return N + 465;
                case 32: return N + 496;
            }
        }

        public static bool GetAttribBit(int Attrib, int Bit)
        {
            switch (Bit)
            {
                case 0:
                    return ((Attrib & 0x01) > 0);
                case 1:
                    return ((Attrib & 0x02) > 0);
                case 2:
                    return ((Attrib & 0x04) > 0);
                case 3:
                    return ((Attrib & 0x08) > 0);
                case 4:
                    return ((Attrib & 0x10) > 0);
                case 5:
                    return ((Attrib & 0x20) > 0);
                case 6:
                    return ((Attrib & 0x40) > 0);
                case 7:
                    return ((Attrib & 0x80) > 0);
            }
            return false;
        }

        public static int SetAttribBit(int Attrib, int Bit, bool Val)
        {
            switch (Bit)
            {
                case 0:
                    {
                        if (Val) { Attrib = Attrib | 0x01; } else { Attrib = Attrib & 0xFE; }
                    }
                    break;
                case 1:
                    {
                        if (Val) { Attrib = Attrib | 0x02; } else { Attrib = Attrib & 0xFD; }
                    }
                    break;
                case 2:
                    {
                        if (Val) { Attrib = Attrib | 0x04; } else { Attrib = Attrib & 0xFB; }
                    }
                    break;
                case 3:
                    {
                        if (Val) { Attrib = Attrib | 0x08; } else { Attrib = Attrib & 0xF7; }
                    }
                    break;
                case 4:
                    {
                        if (Val) { Attrib = Attrib | 0x10; } else { Attrib = Attrib & 0xEF; }
                    }
                    break;
                case 5:
                    {
                        if (Val) { Attrib = Attrib | 0x20; } else { Attrib = Attrib & 0xDF; }
                    }
                    break;
                case 6:
                    {
                        if (Val) { Attrib = Attrib | 0x40; } else { Attrib = Attrib & 0xBF; }
                    }
                    break;
                case 7:
                    {
                        if (Val) { Attrib = Attrib | 0x80; } else { Attrib = Attrib & 0x7F; }
                    }
                    break;
            }
            return Attrib;
        }

        public static string GetAttribText(int Attrib)
        {
            //Bold
            // Italic
            //  Underline
            //   Strikethrough
            //    blinK
            //     Reverse
            //      Concealed
            string X = "";
            if (GetAttribBit(Attrib, 0)) { X = X + "B"; } else { X = X + "_"; }
            if (GetAttribBit(Attrib, 1)) { X = X + "I"; } else { X = X + "_"; }
            if (GetAttribBit(Attrib, 2)) { X = X + "U"; } else { X = X + "_"; }
            if (GetAttribBit(Attrib, 6)) { X = X + "S"; } else { X = X + "_"; }
            if (GetAttribBit(Attrib, 3)) { X = X + "K"; } else { X = X + "_"; }
            if (GetAttribBit(Attrib, 4)) { X = X + "R"; } else { X = X + "_"; }
            if (GetAttribBit(Attrib, 5)) { X = X + "C"; } else { X = X + "_"; }
            return X;
        }

        public static bool FileExists(string FileName)
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
