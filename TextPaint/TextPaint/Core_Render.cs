using System;
using System.Collections.Generic;
using System.Drawing;
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
        enum RenderTypeDef { PNG, XBIN, BIN, TEXT, ANSI, ANSIDOS };
        RenderTypeDef RenderType;
        int RenderBufOffset = 0;

        string RenderFileName(int RenderCounterI)
        {
            string FileExt = "";
            switch (RenderType)
            {
                default:
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

        public void RenderStart(string RenderFile_, int RenderStep_, int RenderOffset_, int RenderFrame_, bool RenderCursor_, string RenderType_)
        {
            if (CurrentFileName.ToUpperInvariant() == "?ENCODING?")
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
                        ConfigFile CF = new ConfigFile();
                        for (int i = 0; i < EncNameL.Count; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    CF.ParamSet("Codepage", EncNameL[i]);
                                    break;
                                case 1:
                                    CF.ParamSet("Name", EncNameL[i]);
                                    break;
                                case 2:
                                    CF.ParamSet("AlternativeName", EncNameL[i]);
                                    break;
                            }
                        }
                        OBE.DefExport(CF);
                        CF.FileSave(EncodingFileName);
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
                default:
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
            }

            if ((RenderType == RenderTypeDef.XBIN) || (RenderType == RenderTypeDef.BIN))
            {
                XBIN XBIN_ = new XBIN(ANSIIgnoreBlink, ANSIIgnoreBold);
                XBIN_.RenderXBIN(CurrentFileName, RenderFile_, (RenderType == RenderTypeDef.XBIN), FileREnc, FileWEnc);
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


            TextCipher_.Reset();
            FileStream FS = new FileStream(CurrentFileName, FileMode.Open, FileAccess.Read);
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
            Screen_.Clear(TextNormalBack, TextNormalFore);

            string Buf = null;
            int TestLines = 0;

            int RenderCounterI = 0;

            int ScrMaxX = 0;
            int ScrMaxY = 0;
            if (UseAnsiLoad)
            {
                Buf = SR.ReadToEnd();
                List<int> TextFileLine_ = TextCipher_.Crypt(TextWork.StrToInt(Buf), true);
                RenderCounterI = 0;
                AnsiProcessSupply(TextFileLine_);
                if (RenderStep > 0)
                {
                    if (RenderOffset_ < 0)
                    {
                        RenderOffset_ = 0;
                    }

                    RenderSave(RenderFileName(RenderCounterI));
                    Console.WriteLine("Frame " + RenderCounterI + " - " + (AnsiBufferI * 100 / AnsiBuffer.Count) + "%");
                    int RenderStep___ = RenderStep;
                    if (RenderOffset_ > 0)
                    {
                        RenderStep___ = RenderOffset_;
                    }
                    bool RenderWork = true;
                    while (RenderWork)
                    {
                        RenderWork = (AnsiProcess(RenderStep___) != 0);
                        AnsiRepaint(false);
                        RenderStep___ = RenderStep;
                        RenderCounterI++;
                        Console.WriteLine("Frame " + RenderCounterI + " - " + (AnsiBufferI * 100 / AnsiBuffer.Count) + "%");
                        RenderSave(RenderFileName(RenderCounterI));
                    }
                    if (RenderCursor_)
                    {
                        ScrMaxX = Math.Max(__ScreenMaxX, __AnsiX) + 1;
                        ScrMaxY = Math.Max(__ScreenMaxY, __AnsiY) + 1;
                    }
                    else
                    {
                        ScrMaxX = __ScreenMaxX + 1;
                        ScrMaxY = __ScreenMaxY + 1;
                    }
                    ScrMaxX = Math.Max(__ScreenMaxX, __AnsiX) + 1;
                    ScrMaxY = Math.Max(__ScreenMaxY, __AnsiY) + 1;
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
                    int ScrRealH = __AnsiLineOccupy.Count + __AnsiLineOccupy1.Count + __AnsiLineOccupy2.Count;
                    RenderBufOffset = __AnsiLineOccupy1.Count;
                    ((ScreenWindow)Screen_).DummyResize(Screen_.WinW, Math.Max(ScrRealH, __AnsiY + RenderBufOffset + 1));
                    AnsiRepaint(true);
                    if (RenderCursor_)
                    {
                        ScrMaxX = Math.Max(__ScreenMaxX, __AnsiX) + 1;
                        ScrMaxY = Math.Max(__ScreenMaxY, __AnsiY + RenderBufOffset) + 1;
                    }
                    else
                    {
                        ScrMaxX = __ScreenMaxX + 1;
                        ScrMaxY = __ScreenMaxY + 1;
                    }
                    RenderSave(RenderFileName(0));
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
                    TextBuffer.Add(TextFileLine);
                    TextColBuf.Add(TextWork.BlkCol(TextFileLine.Count));
                    ScrMaxY++;
                    Buf = SR.ReadLine();
                }
                __AnsiX = 0;
                __AnsiY = 0;
                if (TextBuffer.Count > 0)
                {
                    __AnsiY = TextBuffer.Count - 1;
                    __AnsiX = TextBuffer[__AnsiY].Count;
                }
                ((ScreenWindow)Screen_).DummyResize(ScrMaxX, ScrMaxY);
                Screen_.Clear(TextNormalBack, TextNormalFore);
                for (int YY = 0; YY < TextBuffer.Count; YY++)
                {
                    for (int XX = 0; XX < TextBuffer[YY].Count; XX++)
                    {
                        Screen_.PutChar(XX, YY, TextBuffer[YY][XX], TextNormalBack, TextNormalFore, 0, 0);
                    }
                }
                RenderSave(RenderFileName(0));
            }
            AnsiEnd();
            SR.Close();
            FS.Close();
            TextBufferTrim();
            UndoBufferClear();
            Console.WriteLine("Rendering canvas size: " + Screen_.WinW + "x" + Screen_.WinH);
            Console.WriteLine("Used text area: " + ScrMaxX + "x" + ScrMaxY);
            if (__AnsiProcessDelayMin <= __AnsiProcessDelayMax)
            {
                Console.WriteLine("Minimum time marker dummy steps: " + __AnsiProcessDelayMin);
                Console.WriteLine("Maximum time marker dummy steps: " + __AnsiProcessDelayMax);
            }

        }

        void RenderSave(string FileName)
        {
            string FileNameD = Path.GetDirectoryName(FileName);
            if (!Directory.Exists(FileNameD))
            {
                Directory.CreateDirectory(FileNameD);
            }
            Screen_.SetCursorPositionNoRefresh(__AnsiX, __AnsiY + RenderBufOffset);
            switch (RenderType)
            {
                default:
                    Bitmap Bmp = ((ScreenWindow)Screen_).DummyGetScreenBitmap(RenderCursor);
                    Bmp.Save(FileName, System.Drawing.Imaging.ImageFormat.Png);
                    break;
                case RenderTypeDef.TEXT:
                case RenderTypeDef.ANSI:
                case RenderTypeDef.ANSIDOS:
                    int IsAnsi = 0;
                    if (RenderType == RenderTypeDef.ANSI) IsAnsi = 1;
                    if (RenderType == RenderTypeDef.ANSIDOS) IsAnsi = 2;
                    string StrX = ((ScreenWindow)Screen_).DummyGetScreenText(IsAnsi, TextNormalBack, TextNormalFore);
                    File.WriteAllText(FileName, StrX);
                    break;
            }
        }
    }
}
