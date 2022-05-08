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

        string RenderFileName(int RenderCounterI)
        {
            if (RenderStep > 0)
            {
                return Path.Combine(RenderFile, RenderCounterI.ToString().PadLeft(RenderCounterL, '0') + ".png");
            }
            else
            {
                if (!RenderFile.ToLowerInvariant().EndsWith(".png"))
                {
                    RenderFile = RenderFile + ".png";
                }
                return RenderFile;
            }
        }

        public void RenderStart(string RenderFile_, int RenderStep_, int RenderOffset_, bool RenderCursor_)
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
            RenderCursor = RenderCursor_;


            int BufW = 0;
            int BufH = 0;
            if (AnsiMaxX != AnsiMaxVal)
            {
                BufW = AnsiMaxX;
            }
            if (AnsiMaxY != AnsiMaxVal)
            {
                BufH = AnsiMaxY;
            }
            if (RenderStep > 0)
            {
                if (!UseAnsiLoad)
                {
                    Console.WriteLine("Rendering movie for plain text file is not possible.");
                    Console.WriteLine("Use ANSIRead=1 for render movie.");
                    return;
                }
                if ((BufW == 0) || (BufH == 0))
                {
                    Console.WriteLine("To render movie, you have to set ANSIWidth and ANSIHeight greater than 0.");
                    Console.WriteLine("To render image, you have to set RenderStep=0.");
                    return;
                }
                ((ScreenWindow)Screen_).DummyResize(BufW, BufH);
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

            string Buf = null;
            int TestLines = 0;
            List<int> EOL = new List<int>();
            EOL.Add(13);
            EOL.Add(10);

            int RenderCounterI = 0;
            int FrameNum = 0;
            if (UseAnsiLoad)
            {
                Buf = SR.ReadToEnd();
                List<int> TextFileLine_ = TextCipher_.Crypt(TextWork.StrToInt(Buf), true);
                List<int> TextFileLine = new List<int>();
                for (int i = 0; i < TextFileLine_.Count; i++)
                {
                    FileAdd(TextFileLine_[i], ref TextFileLine);
                }
                RenderCounterI = 0;
                if (RenderStep > 0)
                {
                    while (RenderOffset_ >= RenderStep_)
                    {
                        RenderOffset_ -= RenderStep_;
                    }
                    while (RenderOffset_ < 0)
                    {
                        RenderOffset_ += RenderStep_;
                    }
                    FrameNum = ((TextFileLine.Count - RenderOffset_) + 1) / RenderStep_;
                    FrameNum += 2;
                    if (RenderOffset_ > 0)
                    {
                        AnsiProcess(TextFileLine.GetRange(0, Math.Min(RenderOffset_, TextFileLine.Count)));
                    }
                    Console.WriteLine("Frame " + RenderCounterI + "/" + FrameNum);
                    RenderSave(RenderFileName(RenderCounterI));
                    while (((RenderCounterI * RenderStep) + RenderOffset_) < TextFileLine.Count)
                    {
                        AnsiProcess(TextFileLine.GetRange((RenderCounterI * RenderStep) + RenderOffset_, Math.Min(RenderStep, TextFileLine.Count - ((RenderCounterI * RenderStep) + RenderOffset_))));
                        RenderCounterI++;
                        Console.WriteLine("Frame " + RenderCounterI + "/" + FrameNum);
                        RenderSave(RenderFileName(RenderCounterI));
                    }
                }
                else
                {
                    AnsiProcess(TextFileLine);
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
                    Buf = SR.ReadLine();
                }
                __AnsiX = 0;
                __AnsiY = 0;
                if (TextBuffer.Count > 0)
                {
                    __AnsiY = TextBuffer.Count - 1;
                    __AnsiX = TextBuffer[__AnsiY].Count;
                }
            }
            AnsiEnd();
            SR.Close();
            FS.Close();
            TextBufferTrim();
            UndoBufferClear();

            if (BufH < TextBuffer.Count)
            {
                BufH = TextBuffer.Count;
            }
            for (int i = 0; i < TextBuffer.Count; i++)
            {
                if (BufW < TextBuffer[i].Count)
                {
                    BufW = TextBuffer[i].Count;
                }
            }

            if (RenderStep == 0)
            {
                if (AnsiMaxX == AnsiMaxVal)
                {
                    if (BufW <= __AnsiX)
                    {
                        BufW = __AnsiX + 1;
                    }
                }
                if (AnsiMaxY == AnsiMaxVal)
                {
                    if (BufH <= __AnsiY)
                    {
                        BufH = __AnsiY + 1;
                    }
                }
                ((ScreenWindow)Screen_).DummyResize(BufW, BufH);
                RenderSave(RenderFileName(0));
            }
            else
            {
                RenderCounterI++;
                Console.WriteLine("Frame " + RenderCounterI + "/" + FrameNum);
                RenderSave(RenderFileName(RenderCounterI));
            }
            Console.WriteLine("Rendered text size: " + BufW + "x" + BufH);
        }

        void RenderSave(string FileName)
        {
            Screen_.Clear(TextNormalBack, TextNormalFore);
            for (int Y = 0; Y < TextBuffer.Count; Y++)
            {
                for (int X = 0; X < TextBuffer[Y].Count; X++)
                {
                    int ColorB;
                    int ColorF;
                    ColorFromInt(TextColBuf[Y][X], out ColorB, out ColorF);
                    if (ColorB < 0)
                    {
                        ColorB = TextNormalBack;
                    }
                    if (ColorF < 0)
                    {
                        ColorF = TextNormalFore;
                    }
                    Screen_.PutChar(X, Y, TextBuffer[Y][X], ColorB, ColorF);
                }
            }
            Screen_.SetCursorPositionNoRefresh(__AnsiX, __AnsiY);
            Bitmap Bmp = ((ScreenWindow)Screen_).DummyGetScreenBitmap(RenderCursor);
            string FileNameD = Path.GetDirectoryName(FileName);
            if (!Directory.Exists(FileNameD))
            {
                Directory.CreateDirectory(FileNameD);
            }
            Bmp.Save(FileName, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
