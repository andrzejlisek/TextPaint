using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextPaint
{
    public class CoreRender
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

        Core Core_;

        public CoreRender(Core Core__, ConfigFile CF)
        {
            Core_ = Core__;
            RenderSliceX = CF.ParamGetI("RenderSliceX");
            RenderSliceY = CF.ParamGetI("RenderSliceY");
            RenderSliceW = CF.ParamGetI("RenderSliceW");
            RenderSliceH = CF.ParamGetI("RenderSliceH");
        }

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

            if (Core_.CurrentFileName.ToUpperInvariant().StartsWith("?TOOL"))
            {
                if (Core_.CurrentFileName.ToUpperInvariant().EndsWith("?"))
                {
                    Tool Tool_ = new Tool(CF);
                    if ("?TOOL_ENCODING?".Equals(Core_.CurrentFileName.ToUpperInvariant()))
                    {
                        Tool_ = new ToolEncoding(CF);
                    }
                    if ("?TOOL_FONTDISP?".Equals(Core_.CurrentFileName.ToUpperInvariant()))
                    {
                        Tool_ = new ToolFontDisp(CF);
                    }
                    if ("?TOOL_FONTFILTER?".Equals(Core_.CurrentFileName.ToUpperInvariant()))
                    {
                        Tool_ = new ToolFontFilter(CF);
                    }
                    if ("?TOOL_FONTPARSE?".Equals(Core_.CurrentFileName.ToUpperInvariant()))
                    {
                        Tool_ = new ToolFontParse(CF);
                    }
                    if ("?TOOL_FONTHEX?".Equals(Core_.CurrentFileName.ToUpperInvariant()))
                    {
                        Tool_ = new ToolFontHex(CF);
                    }
                    Tool_.Start();
                    return;
                }
            }


            RenderFile = RenderFile_;
            RenderStep = RenderStep_;
            Core_.CoreAnsi_.__AnsiProcessDelayFactor = RenderFrame_;
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
                XBIN_.RenderXBIN(Core_.CurrentFileName, RenderFile_, (RenderType == RenderTypeDef.XBIN), Core_.CoreFile_.FileREnc, Core_.CoreFile_.FileWEnc);
                return;
            }

            if ((RenderType == RenderTypeDef.CONVERT))
            {
                try
                {
                    FileStream FileI_ = new FileStream(Core_.CurrentFileName, FileMode.Open, FileAccess.Read);
                    StreamReader FileI;
                    if ("".Equals(Core_.CoreFile_.FileREnc))
                    {
                        FileI = new StreamReader(FileI_);
                    }
                    else
                    {
                        FileI = new StreamReader(FileI_, TextWork.EncodingFromName(Core_.CoreFile_.FileREnc));
                    }
                    CoreStatic.SaveFileDirectory(RenderFile_);
                    FileStream FileO_ = new FileStream(RenderFile_, FileMode.Create, FileAccess.Write);
                    StreamWriter FileO;
                    if ("".Equals(Core_.CoreFile_.FileWEnc))
                    {
                        FileO = new StreamWriter(FileO_);
                    }
                    else
                    {
                        FileO = new StreamWriter(FileO_, TextWork.EncodingFromName(Core_.CoreFile_.FileWEnc));
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

            int BufW = Core_.CoreAnsi_.AnsiMaxX;
            int BufH = Core_.CoreAnsi_.AnsiMaxY;
            if (RenderStep > 0)
            {
                if (!Core_.CoreFile_.UseAnsiLoad)
                {
                    Console.WriteLine("Rendering movie for plain text file is not possible.");
                    Console.WriteLine("Use ANSIRead=1 for render movie.");
                    return;
                }
            }
            if (Core_.CoreFile_.UseAnsiLoad)
            {
                if ((BufW == 0) || (BufH == 0))
                {
                    Console.WriteLine("You have to set ANSIWidth and ANSIHeight greater than 0.");
                    return;
                }
                ((ScreenWindow)(Core_.Screen_)).DummyResize(BufW, BufH);
            }

            try
            {
                Core_.TextCipher_.Reset();
                FileStream FS = new FileStream(Core_.CurrentFileName, FileMode.Open, FileAccess.Read);
                StreamReader SR;
                if ("".Equals(Core_.CoreFile_.FileREnc))
                {
                    SR = new StreamReader(FS);
                }
                else
                {
                    SR = new StreamReader(FS, TextWork.EncodingFromName(Core_.CoreFile_.FileREnc));
                }

                Core_.CoreAnsi_.AnsiProcessReset(true, true, 0);
                Core_.Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);

                List<int> BellList = new List<int>();

                string Buf = null;
                int TestLines = 0;

                int RenderCounterI = 0;

                int ScrMaxX = 0;
                int ScrMaxY = 0;
                if (Core_.CoreFile_.UseAnsiLoad)
                {
                    int RenderDummyLead = 0;

                    int RenderBlinkCounter = RenderBlinkOffset % RenderBlinkPeriod;
                    bool RenderBlinkSwitch = (RenderBlinkOffset % (RenderBlinkPeriod + RenderBlinkPeriod)) >= RenderBlinkPeriod;

                    Buf = SR.ReadToEnd();
                    List<int> TextFileLine_ = Core_.TextCipher_.Crypt(TextWork.StrToInt(Buf), true);

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
                    Core_.CoreAnsi_.AnsiProcessSupply(TextFileLine_);
                    if (RenderStep > 0)
                    {
                        if (RenderOffset_ < 0)
                        {
                            RenderOffset_ = 0;
                        }

                        if (RenderDummyLead > 0)
                        {
                            Core_.CoreAnsi_.AnsiProcess(RenderDummyLead);
                        }

                        RenderSave(RenderFileName(RenderCounterI), RenderBlinkSwitch);
                        RenderBlinkCounter++;
                        if (RenderBlinkCounter >= RenderBlinkPeriod)
                        {
                            RenderBlinkSwitch = !RenderBlinkSwitch;
                            RenderBlinkCounter = 0;
                        }
                        Console.WriteLine("Frame " + RenderCounterI + " - " + (Core_.CoreAnsi_.AnsiState_.AnsiBufferI * 100 / Core_.CoreAnsi_.AnsiBuffer.Count) + "%");
                        int RenderStep___ = RenderStep;
                        if (RenderOffset_ > 0)
                        {
                            RenderStep___ = RenderOffset_;
                        }
                        bool RenderWork = true;
                        while (RenderWork)
                        {
                            Core_.Screen_.BellOccured = false;
                            RenderWork = (Core_.CoreAnsi_.AnsiProcess(RenderStep___) != 0);
                            Core_.CoreAnsi_.AnsiRepaint(false);
                            RenderStep___ = RenderStep;
                            RenderCounterI++;
                            Console.WriteLine("Frame " + RenderCounterI + " - " + (Core_.CoreAnsi_.AnsiState_.AnsiBufferI * 100 / Core_.CoreAnsi_.AnsiBuffer.Count) + "%");
                            if (Core_.Screen_.BellOccured)
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
                            ScrMaxX = Math.Max(Core_.CoreAnsi_.__ScreenMaxX, Core_.CoreAnsi_.AnsiState_.__AnsiX) + 1;
                            ScrMaxY = Math.Max(Core_.CoreAnsi_.__ScreenMaxY, Core_.CoreAnsi_.AnsiState_.__AnsiY) + 1;
                        }
                        else
                        {
                            ScrMaxX = Core_.CoreAnsi_.__ScreenMaxX + 1;
                            ScrMaxY = Core_.CoreAnsi_.__ScreenMaxY + 1;
                        }
                        ScrMaxX = Math.Max(Core_.CoreAnsi_.__ScreenMaxX, Core_.CoreAnsi_.AnsiState_.__AnsiX) + 1;
                        ScrMaxY = Math.Max(Core_.CoreAnsi_.__ScreenMaxY, Core_.CoreAnsi_.AnsiState_.__AnsiY) + 1;
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
                            Core_.CoreAnsi_.AnsiProcess(RenderOffset_);
                        }
                        else
                        {
                            Core_.CoreAnsi_.AnsiProcess(-1);
                        }
                        int ScrRealH = Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy__.CountLines() + Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy1__.CountLines() + Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy2__.CountLines();
                        RenderBufOffset = Core_.CoreAnsi_.AnsiState_.__AnsiLineOccupy1__.CountLines();
                        ((ScreenWindow)(Core_.Screen_)).DummyResize(Core_.Screen_.WinW, Math.Max(ScrRealH, Core_.CoreAnsi_.AnsiState_.__AnsiY + RenderBufOffset + 1));
                        Core_.CoreAnsi_.AnsiRepaint(true);
                        if (RenderCursor_)
                        {
                            ScrMaxX = Math.Max(Core_.CoreAnsi_.__ScreenMaxX, Core_.CoreAnsi_.AnsiState_.__AnsiX) + 1;
                            ScrMaxY = Math.Max(Core_.CoreAnsi_.__ScreenMaxY, Core_.CoreAnsi_.AnsiState_.__AnsiY + RenderBufOffset) + 1;
                        }
                        else
                        {
                            ScrMaxX = Core_.CoreAnsi_.__ScreenMaxX + 1;
                            ScrMaxY = Core_.CoreAnsi_.__ScreenMaxY + 1;
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
                        List<int> TextFileLine = Core_.TextCipher_.Crypt(TextWork.StrToInt(Buf), true);
                        if (ScrMaxX < TextFileLine.Count)
                        {
                            ScrMaxX = TextFileLine.Count;
                        }
                        Core_.TextBuffer.AppendLine();
                        Core_.TextBuffer.SetLineString(Core_.TextBuffer.CountLines() - 1, TextFileLine);
                        ScrMaxY++;
                        Buf = SR.ReadLine();
                    }
                    Core_.CoreAnsi_.AnsiState_.__AnsiX = 0;
                    Core_.CoreAnsi_.AnsiState_.__AnsiY = 0;
                    if (Core_.TextBuffer.CountLines() > 0)
                    {
                        Core_.CoreAnsi_.AnsiState_.__AnsiY = Core_.TextBuffer.CountLines() - 1;
                        Core_.CoreAnsi_.AnsiState_.__AnsiX = Core_.TextBuffer.CountItems(Core_.CoreAnsi_.AnsiState_.__AnsiY);
                    }
                    ((ScreenWindow)(Core_.Screen_)).DummyResize(ScrMaxX, ScrMaxY);
                    Core_.Screen_.Clear(Core_.TextNormalBack, Core_.TextNormalFore);
                    for (int YY = 0; YY < Core_.TextBuffer.CountLines(); YY++)
                    {
                        for (int XX = 0; XX < Core_.TextBuffer.CountItems(YY); XX++)
                        {
                            Core_.TextBuffer.Get(YY, XX);
                            Core_.Screen_.PutChar(XX, YY, Core_.TextBuffer.Item_Char, Core_.TextNormalBack, Core_.TextNormalFore, 0, 0, 0);
                        }
                    }
                    RenderSave(RenderFileName(0), false);
                }
                SR.Close();
                FS.Close();
                Core_.TextBuffer.TrimLines();
                Core_.UndoBufferClear();
                Console.WriteLine("Rendering canvas size: " + Core_.Screen_.WinW + "x" + Core_.Screen_.WinH);
                Console.WriteLine("Used text area: " + ScrMaxX + "x" + ScrMaxY);
                if (Core_.CoreAnsi_.AnsiState_.__AnsiProcessDelayMin <= Core_.CoreAnsi_.AnsiState_.__AnsiProcessDelayMax)
                {
                    Console.WriteLine("Minimum time marker dummy steps: " + Core_.CoreAnsi_.AnsiState_.__AnsiProcessDelayMin);
                    Console.WriteLine("Maximum time marker dummy steps: " + Core_.CoreAnsi_.AnsiState_.__AnsiProcessDelayMax);
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
            int SliceW = Core_.Screen_.WinW;
            int SliceH = Core_.Screen_.WinH;
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
                while (SliceX < Core_.Screen_.WinW)
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
                while (SliceY < Core_.Screen_.WinH)
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
                while (SliceY < Core_.Screen_.WinH)
                {
                    SliceX = SliceX0;
                    SliceNumX = 1;
                    while (SliceX < Core_.Screen_.WinW)
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
            Core_.Screen_.SetCursorPositionNoRefresh(Core_.CoreAnsi_.AnsiState_.__AnsiX, Core_.CoreAnsi_.AnsiState_.__AnsiY + RenderBufOffset);
            switch (RenderType)
            {
                default:
                    LowLevelBitmap Bmp = ((ScreenWindow)(Core_.Screen_)).DummyGetScreenBitmap(Blink, RenderCursor, Core_.TextNormalBack, Core_.TextNormalFore, X, Y, W, H);
                    Bmp.SaveToFile(FileName);
                    break;
                case RenderTypeDef.TEXT:
                case RenderTypeDef.ANSI:
                case RenderTypeDef.ANSIDOS:
                    int IsAnsi = 0;
                    if (RenderType == RenderTypeDef.ANSI) IsAnsi = 1;
                    if (RenderType == RenderTypeDef.ANSIDOS) IsAnsi = 2;
                    string StrX = ((ScreenWindow)(Core_.Screen_)).DummyGetScreenText(IsAnsi, Core_.TextNormalBack, Core_.TextNormalFore, X, Y, W, H);
                    File.WriteAllText(FileName, StrX);
                    break;
            }
        }
    }
}
