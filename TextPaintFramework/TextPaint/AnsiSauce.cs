using System;
using System.Collections.Generic;

namespace TextPaint
{
    public class AnsiSauce
    {
        string[] NameDataType = new string[] { "None", "Character", "Bitmap", "Vector", "Audio", "BinaryText", "XBin", "Archive", "Executable" };
        string[] NameFileType0 = new string[] { };
        string[] NameFileType1 = new string[] { "ASCII", "ANSi", "ANSiMation", "RIP script", "PCBoard", "Avatar", "HTML", "Source", "TundraDraw" };
        string[] NameFileType2 = new string[] { "GIF", "PCX", "LBM/IFF", "TGA", "FLI", "FLC", "BMP", "GL", "DL", "WPG", "PNG", "JPG/JPeg", "MPG", "AVI" };
        string[] NameFileType3 = new string[] { "DXF", "DWG", "WPG", "3DS" };
        string[] NameFileType4 = new string[] { "MOD", "669", "STM", "S3M", "MTM", "FAR", "ULT", "AMF", "DMF", "OKT", "ROL", "CMF", "MID", "SADT", "VOC", "WAV", "SMP8", "SMP8S", "SMP16", "SMP16S", "PATCH8", "PATCH16", "XM", "HSC", "IT" };
        string[] NameFileType5 = new string[] { };
        string[] NameFileType6 = new string[] { };
        string[] NameFileType7 = new string[] { "ZIP", "ARJ", "LZH", "ARC", "TAR", "ZOO", "RAR", "UC2", "PAK", "SQZ" };
        string[] NameFileType8 = new string[] { };

        public AnsiSauce()
        {
        }

        int SauceIdx = -1;
        int CommentIdx = -1;

        string Field01Version = "";
        string Field02Title = "";
        string Field03Author = "";
        string Field04Group = "";
        string Field05Date = "";
        long Field06FileSize = 0;
        int Field07DataType = 0;
        int Field08FileType = 0;
        int Field09TInfo1 = 0;
        int Field10TInfo2 = 0;
        int Field11TInfo3 = 0;
        int Field12TInfo4 = 0;
        int Field13Comments = 0;
        int Field14TFlags = 0;
        string Field15TInfoS = "";
        List<string> Comment = new List<string>();

        int GetRawNum(List<int> FileCtx, int Idx)
        {
            if (FileCtx.Count > Idx)
            {
                return FileCtx[Idx];
            }
            return 0;
        }

        string GetRawChar(List<int> FileCtx, int Idx)
        {
            if (FileCtx.Count > Idx)
            {
                return TextWork.IntToStr(FileCtx[Idx]);
            }
            return "_";
        }

        string GetChar(int Idx)
        {
            byte X = (byte)GetByte(Idx);
            if ((X >= 32) && (X <= 127))
            {
                return ((char)X).ToString();
            }
            else
            {
                if (X == 0)
                {
                    return " ";
                }
                else
                {
                    return "#";
                }
            }
        }

        int GetByte(int Idx)
        {
            if ((Idx >= 0) && (Raw.Length > Idx))
            {
                return Raw[Idx];
            }
            return 0;
        }

        byte[] Raw;

        void ReadSAUCE()
        {
            InfoRaw = new byte[Raw.Length - SauceIdx - 1];
            Array.Copy(Raw, SauceIdx + 1, InfoRaw, 0, InfoRaw.Length);

            int Ptr = SauceIdx + 6;
            for (int i = 0; i < 2; i++)
            {
                Field01Version = Field01Version + GetChar(Ptr);
                Ptr++;
            }
            for (int i = 0; i < 35; i++)
            {
                Field02Title = Field02Title + GetChar(Ptr);
                Ptr++;
            }
            for (int i = 0; i < 20; i++)
            {
                Field03Author = Field03Author + GetChar(Ptr);
                Ptr++;
            }
            for (int i = 0; i < 20; i++)
            {
                Field04Group = Field04Group + GetChar(Ptr);
                Ptr++;
            }
            for (int i = 0; i < 8; i++)
            {
                Field05Date = Field05Date + GetChar(Ptr);
                Ptr++;
            }
            Field06FileSize += ((long)GetByte(Ptr + 0));
            Field06FileSize += ((long)GetByte(Ptr + 1) * 256L);
            Field06FileSize += ((long)GetByte(Ptr + 2) * 65536L);
            Field06FileSize += ((long)GetByte(Ptr + 3) * 16777216L);
            Ptr += 4;
            Field07DataType += GetByte(Ptr);
            Ptr += 1;
            Field08FileType += GetByte(Ptr);
            Ptr += 1;
            Field09TInfo1 += (GetByte(Ptr + 0));
            Field09TInfo1 += (GetByte(Ptr + 1) * 256);
            Ptr += 2;
            Field10TInfo2 += (GetByte(Ptr + 0));
            Field10TInfo2 += (GetByte(Ptr + 1) * 256);
            Ptr += 2;
            Field11TInfo3 += (GetByte(Ptr + 0));
            Field11TInfo3 += (GetByte(Ptr + 1) * 256);
            Ptr += 2;
            Field12TInfo4 += (GetByte(Ptr + 0));
            Field12TInfo4 += (GetByte(Ptr + 1) * 256);
            Ptr += 2;
            Field13Comments += GetByte(Ptr);
            Ptr += 1;
            Field14TFlags += GetByte(Ptr);
            Ptr += 1;
            for (int i = 0; i < 22; i++)
            {
                Field15TInfoS = Field15TInfoS + GetChar(Ptr);
                Ptr++;
            }
        }

        void ReadCOMNT()
        {
            InfoRaw = new byte[Raw.Length - CommentIdx - 1];
            Array.Copy(Raw, CommentIdx + 1, InfoRaw, 0, InfoRaw.Length);

            int Ptr = CommentIdx + 6;
            while (true)
            {
                string CommentLine = "";
                for (int i = 0; i < 64; i++)
                {
                    CommentLine = CommentLine + GetChar(Ptr + i);
                }
                Comment.Add(CommentLine);
                Ptr += 64;
                if (Ptr >= Raw.Length)
                {
                    break;
                }
                if ((SauceIdx >= 0) && (Ptr >= SauceIdx))
                {
                    break;
                }
            }
        }

        public bool Exists = false;
        public List<string> Info = new List<string>();
        public byte[] InfoRaw = null;

        List<string> NonSauceInfo1 = new List<string>();
        List<string> NonSauceInfo2 = new List<string>();

        public void NonSauceInfo(string Label, long Value)
        {
            NonSauceInfo(Label, Value.ToString());
        }

        public void NonSauceInfo(string Label, string Value)
        {
            NonSauceInfo1.Add(Label);
            NonSauceInfo2.Add(Value);
        }

        public void CreateInfo()
        {
            InfoRaw = null;
            Exists = false;
            SauceIdx = -1;
            CommentIdx = -1;

            if (SauceIdx < 0)
            {
                SauceIdx = Raw.Length - 129;
                if (GetByte(SauceIdx + 1) != 'S') { SauceIdx = -1; }
                if (GetByte(SauceIdx + 2) != 'A') { SauceIdx = -1; }
                if (GetByte(SauceIdx + 3) != 'U') { SauceIdx = -1; }
                if (GetByte(SauceIdx + 4) != 'C') { SauceIdx = -1; }
                if (GetByte(SauceIdx + 5) != 'E') { SauceIdx = -1; }
            }


            Field01Version = "";
            Field02Title = "";
            Field03Author = "";
            Field04Group = "";
            Field05Date = "";
            Field06FileSize = 0;
            Field07DataType = 0;
            Field08FileType = 0;
            Field09TInfo1 = 0;
            Field10TInfo2 = 0;
            Field11TInfo3 = 0;
            Field12TInfo4 = 0;
            Field13Comments = 0;
            Field14TFlags = 0;
            Field15TInfoS = "";
            Comment.Clear();
            Info.Clear();

            for (int i = 0; i < NonSauceInfo1.Count; i++)
            {
                Info.Add(NonSauceInfo1[i] + ": " + NonSauceInfo2[i]);
            }
            Info.Add("");

            if (SauceIdx >= 0)
            {
                Exists = true;
                ReadSAUCE();
                if (Field13Comments > 0)
                {
                    CommentIdx = SauceIdx - (64 * Field13Comments) - 5;
                    while ((CommentIdx + 64) < 0)
                    {
                        CommentIdx += 64;
                    }
                    ReadCOMNT();
                }

                string Info_;

                Info.Add("Version: " + Field01Version.Trim());
                Info.Add("Title: " + Field02Title.Trim());
                Info.Add("Author: " + Field03Author.Trim());
                Info.Add("Group: " + Field04Group.Trim());
                Info.Add("Date: " + Field05Date.Trim());
                Info.Add("FileSize: " + Field06FileSize);
                if (Field07DataType <= 8)
                {
                    Info.Add("DataType: " + Field07DataType + " -> " + NameDataType[Field07DataType]);
                    string[] Name_ = null;
                    switch (Field07DataType)
                    {
                        case 0: Name_ = NameFileType0; break;
                        case 1: Name_ = NameFileType1; break;
                        case 2: Name_ = NameFileType2; break;
                        case 3: Name_ = NameFileType3; break;
                        case 4: Name_ = NameFileType4; break;
                        case 5: Name_ = NameFileType5; break;
                        case 6: Name_ = NameFileType6; break;
                        case 7: Name_ = NameFileType7; break;
                        case 8: Name_ = NameFileType8; break;
                    }
                    if (Field08FileType < Name_.Length)
                    {
                        Info.Add("FileType: " + Field08FileType + " -> " + Name_[Field08FileType]);
                    }
                    else
                    {
                        Info.Add("FileType: " + Field08FileType);
                    }
                }
                else
                {
                    Info.Add("DataType: " + Field07DataType);
                    Info.Add("FileType: " + Field08FileType);
                }
                Info.Add("Info number 1: " + Field09TInfo1);
                Info.Add("Info number 2: " + Field10TInfo2);
                Info.Add("Info number 3: " + Field11TInfo3);
                Info.Add("Info number 4: " + Field12TInfo4);
                Info.Add("Comment lines: " + Field13Comments);
                Info_ = "Flags: ";
                Info_ = Info_ + (((Field14TFlags & 128) > 0) ? "1" : "0");
                Info_ = Info_ + (((Field14TFlags & 64) > 0) ? "1" : "0");
                Info_ = Info_ + (((Field14TFlags & 32) > 0) ? "1" : "0");
                Info_ = Info_ + (((Field14TFlags & 16) > 0) ? "1" : "0");
                Info_ = Info_ + (((Field14TFlags & 8) > 0) ? "1" : "0");
                Info_ = Info_ + (((Field14TFlags & 4) > 0) ? "1" : "0");
                Info_ = Info_ + (((Field14TFlags & 2) > 0) ? "1" : "0");
                Info_ = Info_ + (((Field14TFlags & 1) > 0) ? "1" : "0");
                Info_ = Info_ + " -> ";
                switch (Field14TFlags & (16 + 8))
                {
                    case 0:
                        Info_ = Info_ + "AR:none, ";
                        break;
                    case 8:
                        Info_ = Info_ + "AR:legacy, ";
                        break;
                    case 16:
                        Info_ = Info_ + "AR:modern, ";
                        break;
                    case 24:
                        Info_ = Info_ + "AR:unknown, ";
                        break;
                }
                switch (Field14TFlags & (4 + 2))
                {
                    case 0:
                        Info_ = Info_ + "Sp:legacy, ";
                        break;
                    case 2:
                        Info_ = Info_ + "Sp:8px, ";
                        break;
                    case 4:
                        Info_ = Info_ + "Sp:9px, ";
                        break;
                    case 6:
                        Info_ = Info_ + "Sp:unknown, ";
                        break;
                }
                switch (Field14TFlags & 1)
                {
                    case 0:
                        Info_ = Info_ + "Bk:7+blink ";
                        break;
                    case 1:
                        Info_ = Info_ + "Bk:16 ";
                        break;
                }
                Info.Add(Info_);
                Info.Add("Text info: " + Field15TInfoS.Trim());
                for (int i = 0; i < Comment.Count; i++)
                {
                    Info.Add("Comment " + (i + 1) + ": " + Comment[i].Trim());
                }
            }
            else
            {
                Info.Add("SAUCE information not exists");
            }
        }

        public void LoadRaw(byte[] Raw_)
        {
            NonSauceInfo1.Clear();
            NonSauceInfo2.Clear();
            Raw = Raw_;
        }
    }
}
