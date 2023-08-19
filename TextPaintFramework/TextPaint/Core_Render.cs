using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextPaint
{
    public partial class Core
    {
        string RenderFile = "";
        int RenderStep = 0;
        int RenderCounterL = 8;
        bool RenderCursor = false;
        enum RenderTypeDef { PNG, XBIN, BIN, TEXT, ANSI, ANSIDOS, CONVERT };
        RenderTypeDef RenderType;
        int RenderBufOffset = 0;
        int RenderSliceX = 0;
        int RenderSliceY = 0;
        int RenderSliceW = 0;
        int RenderSliceH = 0;
        int RenderSliceL = 4;

        string RenderFileName(int RenderCounterI)
        {
            string FileExt = ".xxx";
            switch (RenderType)
            {
                case RenderTypeDef.PNG:
                    FileExt = ".png";
                    break;
                case RenderTypeDef.TEXT:
                    FileExt = ".txt";
                    break;
                case RenderTypeDef.ANSI:
                case RenderTypeDef.ANSIDOS:
                    FileExt = ".ans";
                    break;
            }
            if (RenderStep > 0)
            {
                return Path.Combine(RenderFile, RenderCounterI.ToString().PadLeft(RenderCounterL, '0') + FileExt);
            }
            else
            {
                if (!RenderFile.ToLowerInvariant().EndsWith(FileExt))
                {
                    RenderFile = RenderFile + FileExt;
                }
                return RenderFile;
            }
        }

        public void RenderStart(ConfigFile CF)
        {
            string RenderFile_ = CF.ParamGetS("RenderFile");
            int RenderStep_ = CF.ParamGetI("RenderStep");
            int RenderOffset_ = CF.ParamGetI("RenderOffset");
            int RenderFrame_ = CF.ParamGetI("RenderFrame");
            bool RenderCursor_ = CF.ParamGetB("RenderCursor");
            string RenderType_ = CF.ParamGetS("RenderType");

            int RenderLeading = CF.ParamGetI("RenderLeading");
            int RenderTrailing = CF.ParamGetI("RenderTrailing");
            int RenderBlinkPeriod = CF.ParamGetI("RenderBlinkPeriod");
            int RenderBlinkOffset = CF.ParamGetI("RenderBlinkOffset");

            while (RenderBlinkOffset < 0)
            {
                RenderBlinkOffset = RenderBlinkOffset +(RenderBlinkPeriod + RenderBlinkPeriod);
            }

            if ("?ENCODING?".Equals(CurrentFileName.ToUpperInvariant()))
            {
                Console.WriteLine("Creating encoding files...");
                if (!Directory.Exists(RenderFile_))
                {
                    Directory.CreateDirectory(RenderFile_);
                }
                int FileI = 0;
                OneByteEncoding OBE = new OneByteEncoding();
                foreach (EncodingInfo ei in Encoding.GetEncodings())
                {
                    Encoding e = ei.GetEncoding();
                    string EncName = e.CodePage.ToString().PadLeft(5);
                    List<string> EncNameL = new List<string>();
                    EncNameL.Add(e.CodePage.ToString());

                    if ((!EncNameL.Contains(ei.Name)) && (TextWork.EncodingCheckName(e, ei.Name)))
                    {
                        EncName = EncName + ((EncNameL.Count == 1) ? ": " : ", ") + ei.Name;
                        EncNameL.Add(ei.Name);
                    }
                    if ((!EncNameL.Contains(e.WebName)) && (TextWork.EncodingCheckName(e, e.WebName)))
                    {
                        EncName = EncName + ((EncNameL.Count == 1) ? ": " : ", ") + e.WebName;
                        EncNameL.Add(e.WebName);
                    }
                    Console.Write(EncName);
                    Console.Write(" - ");
                    if (OBE.DefImport(e))
                    {
                        string EncodingFileName = Path.Combine(RenderFile_, e.CodePage.ToString().PadLeft(5, '0') + ".txt");
                        ConfigFile CF0 = new ConfigFile();
                        for (int i = 0; i < EncNameL.Count; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    CF0.ParamSet("Codepage", EncNameL[i]);
                                    break;
                                case 1:
                                    CF0.ParamSet("Name", EncNameL[i]);
                                    break;
                                case 2:
                                    CF0.ParamSet("AlternativeName", EncNameL[i]);
                                    break;
                            }
                        }
                        OBE.DefExport(CF0);
                        CF0.FileSave(EncodingFileName);
                        FileI++;
                        Console.WriteLine("created");
                    }
                    else
                    {
                        Console.WriteLine("not 8-bit");
                    }
                }
                Console.WriteLine("Created " + FileI + " files.");
                return;
            }


            RenderFile = RenderFile_;
            RenderStep = RenderStep_;
            __AnsiProcessDelayFactor = RenderFrame_;
            RenderCursor = RenderCursor_;

            switch (RenderType_.ToUpperInvariant())
            {
                case "PNG":
                    RenderType = RenderTypeDef.PNG;
                    break;
                case "TXT":
                case "TEXT":
                    RenderType = RenderTypeDef.TEXT;
                    break;
                case "ANS":
                case "ANSI":
                    RenderType = RenderTypeDef.ANSI;
                    break;
                case "DOS":
                case "ANSDOS":
                case "ANSIDOS":
                    RenderType = RenderTypeDef.ANSIDOS;
                    break;
                case "BIN":
                    RenderType = RenderTypeDef.BIN;
                    break;
                case "XB":
                case "XBIN":
                    RenderType = RenderTypeDef.XBIN;
                    break;
                case "CONV":
                case "CONVERT":
                    RenderType = RenderTypeDef.CONVERT;
                    break;
                default:
                    Console.WriteLine("Valid render types: PNG, TEXT, ANSI, DOS, BIN, XBIN, CONV");
                    return;
            }

            if ((RenderType == RenderTypeDef.XBIN) || (RenderType == RenderTypeDef.BIN))
            {
                XBIN XBIN_ = new XBIN();
                XBIN_.RenderXBIN(CurrentFileName, RenderFile_, (RenderType == RenderTypeDef.XBIN), FileREnc, FileWEnc);
                return;
            }

            if ((RenderType == RenderTypeDef.CONVERT))
            {
                try
                {
                    FileStream FileI_ = new FileStream(CurrentFileName, FileMode.Open, FileAccess.Read);
                    StreamReader FileI;
                    if ("".Equals(FileREnc))
                    {
                        FileI = new StreamReader(FileI_);
                    }
                    else
                    {
                        FileI = new StreamReader(FileI_, TextWork.EncodingFromName(FileREnc));
                    }
                    SaveFileDirectory(RenderFile_);
                    FileStream FileO_ = new FileStream(RenderFile_, FileMode.Create, FileAccess.Write);
                    StreamWriter FileO;
                    if ("".Equals(FileWEnc))
                    {
                        FileO = new StreamWriter(FileO_);
                    }
                    else
                    {
                        FileO = new StreamWriter(FileO_, TextWork.EncodingFromName(FileWEnc));
                    }
                    string FileBuf = FileI.ReadToEnd();
                    FileO.Write(FileBuf);

                    FileI.Close();
                    FileI_.Close();
                    FileO.Close();
                    FileO_.Close();

                    Console.WriteLine("Text conversion from [" + TextWork.EncodingGetName(FileI.CurrentEncoding) + "] to [" + TextWork.EncodingGetName(FileO.Encoding) + "] is done.");
                }
                catch (Exception E)
                {
                    Console.WriteLine("Text conversion error: " + E.Message);
                }
                return;
            }

            int BufW = AnsiMaxX;
            int BufH = AnsiMaxY;
            if (RenderStep > 0)
            {
                if (!UseAnsiLoad)
                {
                    Console.WriteLine("Rendering movie for plain text file is not possible.");
                    Console.WriteLine("Use ANSIRead=1 for render movie.");
                    return;
                }
            }
            if (UseAnsiLoad)
            {
                if ((BufW == 0) || (BufH == 0))
                {
                    Console.WriteLine("You have to set ANSIWidth and ANSIHeight greater than 0.");
                    return;
                }
                ((ScreenWindow)Screen_).DummyResize(BufW, BufH);
                WinW = Screen_.WinW;
                WinH = Screen_.WinH;
            }

            try
            {
                TextCipher_.Reset();
                FileStream FS = new FileStream(CurrentFileName, FileMode.Open, FileAccess.Read);
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
                Screen_.Clear(TextNormalBack, TextNormalFore);

                List<int> BellList = new List<int>();

                string Buf = null;
                int TestLines = 0;

                int RenderCounterI = 0;

                int ScrMaxX = 0;
                int ScrMaxY = 0;
                if (UseAnsiLoad)
                {
                    int RenderDummyLead = 0;

                    int RenderBlinkCounter = RenderBlinkOffset % RenderBlinkPeriod;
                    bool RenderBlinkSwitch = (RenderBlinkOffset % (RenderBlinkPeriod + RenderBlinkPeriod)) >= RenderBlinkPeriod;

                    Buf = SR.ReadToEnd();
                    List<int> TextFileLine_ = TextCipher_.Crypt(TextWork.StrToInt(Buf), true);

                    if (RenderStep > 0)
                    {
                        if (RenderLeading > 0)
                        {
                            RenderDummyLead = 4;

                            List<int> Dummy = new List<int>();
                            int I = RenderLeading;
                            while (I > 0)
                            {
                                Dummy.Add(0x1B);
                                I--;
                            }
                            Dummy.Add(0x1B);
                            Dummy.Add('[');
                            Dummy.Add('0');
                            Dummy.Add('m');

                            TextFileLine_.InsertRange(0, Dummy);
                        }
                        if (RenderTrailing > 0)
                        {
                            int I = RenderTrailing;
                            while (I > 0)
                            {
                                TextFileLine_.Add(0x1B);
                                I--;
                            }
                        }
                    }

                    RenderCounterI = 0;
                    AnsiProcessSupply(TextFileLine_);
                    if (RenderStep > 0)
                    {
                        if (RenderOffset_ < 0)
                        {
                            RenderOffset_ = 0;
                        }

                        if (RenderDummyLead > 0)
                        {
                            AnsiProcess(RenderDummyLead);
                        }

                        RenderSave(RenderFileName(RenderCounterI), RenderBlinkSwitch);
                        RenderBlinkCounter++;
                        if (RenderBlinkCounter >= RenderBlinkPeriod)
                        {
                            RenderBlinkSwitch = !RenderBlinkSwitch;
                            RenderBlinkCounter = 0;
                        }
                        Console.WriteLine("Frame " + RenderCounterI + " - " + (AnsiState_.AnsiBufferI * 100 / AnsiBuffer.Count) + "%");
                        int RenderStep___ = RenderStep;
                        if (RenderOffset_ > 0)
                        {
                            RenderStep___ = RenderOffset_;
                        }
                        bool RenderWork = true;
                        while (RenderWork)
                        {
                            Screen_.BellOccured = false;
                            RenderWork = (AnsiProcess(RenderStep___) != 0);
                            AnsiRepaint(false);
                            RenderStep___ = RenderStep;
                            RenderCounterI++;
                            Console.WriteLine("Frame " + RenderCounterI + " - " + (AnsiState_.AnsiBufferI * 100 / AnsiBuffer.Count) + "%");
                            if (Screen_.BellOccured)
                            {
                                BellList.Add(RenderCounterI);
                            }
                            RenderSave(RenderFileName(RenderCounterI), RenderBlinkSwitch);
                            RenderBlinkCounter++;
                            if (RenderBlinkCounter >= RenderBlinkPeriod)
                            {
                                RenderBlinkSwitch = !RenderBlinkSwitch;
                                RenderBlinkCounter = 0;
                            }
                        }
                        if (RenderCursor_)
                        {
                            ScrMaxX = Math.Max(__ScreenMaxX, AnsiState_.__AnsiX) + 1;
                            ScrMaxY = Math.Max(__ScreenMaxY, AnsiState_.__AnsiY) + 1;
                        }
                        else
                        {
                            ScrMaxX = __ScreenMaxX + 1;
                            ScrMaxY = __ScreenMaxY + 1;
                        }
                        ScrMaxX = Math.Max(__ScreenMaxX, AnsiState_.__AnsiX) + 1;
                        ScrMaxY = Math.Max(__ScreenMaxY, AnsiState_.__AnsiY) + 1;
                        if (BellList.Count > 0)
                        {
                            Console.WriteLine("Bell signal occurred within " + BellList.Count + " frames:");
                            for (int i = 0; i < BellList.Count; i++)
                            {
                                Console.WriteLine(BellList[i]);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Bell signal not occurred.");
                        }
                    }
                    else
                    {
                        if (RenderOffset_ > 0)
                        {
                            AnsiProcess(RenderOffset_);
                        }
                        else
                        {
                            AnsiProcess(-1);
                        }
                        int ScrRealH = AnsiState_.__AnsiLineOccupy__.CountLines() + AnsiState_.__AnsiLineOccupy1__.CountLines() + AnsiState_.__AnsiLineOccupy2__.CountLines();
                        RenderBufOffset = AnsiState_.__AnsiLineOccupy1__.CountLines();
                        ((ScreenWindow)Screen_).DummyResize(Screen_.WinW, Math.Max(ScrRealH, AnsiState_.__AnsiY + RenderBufOffset + 1));
                        AnsiRepaint(true);
                        if (RenderCursor_)
                        {
                            ScrMaxX = Math.Max(__ScreenMaxX, AnsiState_.__AnsiX) + 1;
                            ScrMaxY = Math.Max(__ScreenMaxY, AnsiState_.__AnsiY + RenderBufOffset) + 1;
                        }
                        else
                        {
                            ScrMaxX = __ScreenMaxX + 1;
                            ScrMaxY = __ScreenMaxY + 1;
                        }
                        RenderSave(RenderFileName(0), RenderBlinkSwitch);
                    }
                }
                else
                {
                    ScrMaxX = 0;
                    ScrMaxY = 0;
                    Buf = SR.ReadLine();
                    while (Buf != null)
                    {
                        TestLines++;
                        List<int> TextFileLine = TextCipher_.Crypt(TextWork.StrToInt(Buf), true);
                        if (ScrMaxX < TextFileLine.Count)
                        {
                            ScrMaxX = TextFileLine.Count;
                        }
                        TextBuffer.AppendLine();
                        TextBuffer.SetLineString(TextBuffer.CountLines() - 1, TextFileLine);
                        ScrMaxY++;
                        Buf = SR.ReadLine();
                    }
                    AnsiState_.__AnsiX = 0;
                    AnsiState_.__AnsiY = 0;
                    if (TextBuffer.CountLines() > 0)
                    {
                        AnsiState_.__AnsiY = TextBuffer.CountLines() - 1;
                        AnsiState_.__AnsiX = TextBuffer.CountItems(AnsiState_.__AnsiY);
                    }
                    ((ScreenWindow)Screen_).DummyResize(ScrMaxX, ScrMaxY);
                    Screen_.Clear(TextNormalBack, TextNormalFore);
                    for (int YY = 0; YY < TextBuffer.CountLines(); YY++)
                    {
                        for (int XX = 0; XX < TextBuffer.CountItems(YY); XX++)
                        {
                            TextBuffer.Get(YY, XX);
                            Screen_.PutChar(XX, YY, TextBuffer.Item_Char, TextNormalBack, TextNormalFore, 0, 0, 0);
                        }
                    }
                    RenderSave(RenderFileName(0), false);
                }
                AnsiEnd();
                SR.Close();
                FS.Close();
                TextBuffer.TrimLines();
                UndoBufferClear();
                Console.WriteLine("Rendering canvas size: " + Screen_.WinW + "x" + Screen_.WinH);
                Console.WriteLine("Used text area: " + ScrMaxX + "x" + ScrMaxY);
                if (AnsiState_.__AnsiProcessDelayMin <= AnsiState_.__AnsiProcessDelayMax)
                {
                    Console.WriteLine("Minimum time marker dummy steps: " + AnsiState_.__AnsiProcessDelayMin);
                    Console.WriteLine("Maximum time marker dummy steps: " + AnsiState_.__AnsiProcessDelayMax);
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Render error: " + E.Message);
            }
        }

        void RenderSave(string FileName, bool Blink)
        {
            int ExtPos = FileName.LastIndexOf('.');
            int SliceX = 0;
            int SliceY = 0;
            int SliceW = Screen_.WinW;
            int SliceH = Screen_.WinH;
            int SliceNumX = 1;
            int SliceNumY = 1;
            string FileNameS = FileName;

            // No slice
            if ((RenderSliceW <= 0) && (RenderSliceH <= 0))
            {
                RenderSaveOneFile(FileName, Blink, SliceX, SliceY, SliceW, SliceH);
            }

            // Slice horizontally
            if ((RenderSliceW > 0) && (RenderSliceH <= 0))
            {
                SliceX = RenderSliceX;
                SliceW = RenderSliceW;
                while (SliceX < 0)
                {
                    SliceX += RenderSliceW;
                }
                while (SliceX > 0)
                {
                    SliceX -= RenderSliceW;
                }
                while (SliceX < Screen_.WinW)
                {
                    string SliceNumber = "_" + SliceNumX.ToString().PadLeft(RenderSliceL, '0');
                    if (ExtPos > 0)
                    {
                        FileNameS= FileName.Insert(ExtPos, SliceNumber);
                    }
                    else
                    {
                        FileNameS = FileName + SliceNumber;
                    }
                    RenderSaveOneFile(FileNameS, Blink, SliceX, SliceY, SliceW, SliceH);
                    SliceX += RenderSliceW;
                    SliceNumX++;
                }
            }

            // Slice vertically
            if ((RenderSliceW <= 0) && (RenderSliceH > 0))
            {
                SliceY = RenderSliceY;
                SliceH = RenderSliceH;
                while (SliceY < 0)
                {
                    SliceY += RenderSliceH;
                }
                while (SliceY > 0)
                {
                    SliceY -= RenderSliceH;
                }
                while (SliceY < Screen_.WinH)
                {
                    string SliceNumber = "_" + SliceNumY.ToString().PadLeft(RenderSliceL, '0');
                    if (ExtPos > 0)
                    {
                        FileNameS = FileName.Insert(ExtPos, SliceNumber);
                    }
                    else
                    {
                        FileNameS = FileName + SliceNumber;
                    }
                    RenderSaveOneFile(FileNameS, Blink, SliceX, SliceY, SliceW, SliceH);
                    SliceY += RenderSliceH;
                    SliceNumY++;
                }
            }

            // Slice horizontally and vertically
            if ((RenderSliceW > 0) && (RenderSliceH > 0))
            {
                SliceX = RenderSliceX;
                SliceW = RenderSliceW;
                while (SliceX < 0)
                {
                    SliceX += RenderSliceW;
                }
                while (SliceX > 0)
                {
                    SliceX -= RenderSliceW;
                }
                SliceY = RenderSliceY;
                SliceH = RenderSliceH;
                while (SliceY < 0)
                {
                    SliceY += RenderSliceH;
                }
                while (SliceY > 0)
                {
                    SliceY -= RenderSliceH;
                }
                int SliceX0 = SliceX;
                while (SliceY < Screen_.WinH)
                {
                    SliceX = SliceX0;
                    SliceNumX = 1;
                    while (SliceX < Screen_.WinW)
                    {
                        string SliceNumber = "_" + SliceNumY.ToString().PadLeft(RenderSliceL, '0');
                        SliceNumber = SliceNumber + "_" + SliceNumX.ToString().PadLeft(RenderSliceL, '0');
                        if (ExtPos > 0)
                        {
                            FileNameS = FileName.Insert(ExtPos, SliceNumber);
                        }
                        else
                        {
                            FileNameS = FileName + SliceNumber;
                        }
                        RenderSaveOneFile(FileNameS, Blink, SliceX, SliceY, SliceW, SliceH);
                        SliceX += RenderSliceW;
                        SliceNumX++;
                    }
                    SliceY += RenderSliceH;
                    SliceNumY++;
                }
            }
        }

        void RenderSaveOneFile(string FileName, bool Blink, int X, int Y, int W, int H)
        {
            //string FileNameDisp = FileName;
            //if (FileNameDisp.Length > 30) { FileNameDisp = FileNameDisp.Substring(FileNameDisp.Length - 30); }
            //Console.WriteLine(FileNameDisp + "    " + X + "_" + Y + "__" + W + "_" + H);
            string FileNameD = Path.GetDirectoryName(FileName);
            if (!("".Equals(FileNameD)))
            {
                if (!Directory.Exists(FileNameD))
                {
                    Directory.CreateDirectory(FileNameD);
                }
            }
            Screen_.SetCursorPositionNoRefresh(AnsiState_.__AnsiX, AnsiState_.__AnsiY + RenderBufOffset);
            switch (RenderType)
            {
                default:
                    LowLevelBitmap Bmp = ((ScreenWindow)Screen_).DummyGetScreenBitmap(Blink, RenderCursor, TextNormalBack, TextNormalFore, X, Y, W, H);
                    Bmp.SaveToFile(FileName);
                    break;
                case RenderTypeDef.TEXT:
                case RenderTypeDef.ANSI:
                case RenderTypeDef.ANSIDOS:
                    int IsAnsi = 0;
                    if (RenderType == RenderTypeDef.ANSI) IsAnsi = 1;
                    if (RenderType == RenderTypeDef.ANSIDOS) IsAnsi = 2;
                    string StrX = ((ScreenWindow)Screen_).DummyGetScreenText(IsAnsi, TextNormalBack, TextNormalFore, X, Y, W, H);
                    File.WriteAllText(FileName, StrX);
                    break;
            }
        }
    }
}
