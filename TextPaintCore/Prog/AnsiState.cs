using System;
using System.Collections.Generic;
using System.Text;

namespace TextPaint
{
    public class AnsiState
    {
        public int TerminalW = 0;
        public int TerminalH = 0;

        public long __AnsiProcessStep = 0;
        public long __AnsiProcessDelay = 0;
        public long __AnsiProcessDelayMin = 1;
        public long __AnsiProcessDelayMax = -1;


        public bool __AnsiMusic = false;
        public bool __AnsiUseEOF = false;
        public bool __AnsiBeyondEOF = false;
        public bool __AnsiNoWrap = false;

        public int __AnsiBack = -1;
        public int __AnsiFore = -1;
        public int __AnsiAttr = 0;
        public int __AnsiBack0 = -1;
        public int __AnsiFore0 = -1;
        public int __AnsiAttr0 = 0;

        public int __AnsiFontSizeW = 0;
        public int __AnsiFontSizeH = 0;

        public int __AnsiX = 0;
        public int __AnsiY = 0;
        public int __AnsiX0 = 0;
        public int __AnsiY0 = 0;
        public bool __AnsiWrapFlag = false;
        public bool __AnsiWrapFlag0 = false;

        public bool StatusBar = false;

        public int __AnsiBack_ = -1;
        public int __AnsiFore_ = -1;
        public int __AnsiAttr_ = 0;
        public int __AnsiX_ = 0;
        public int __AnsiY_ = 0;
        public int __AnsiBack_0 = -1;
        public int __AnsiFore_0 = -1;
        public int __AnsiAttr_0 = 0;
        public int __AnsiX_0 = 0;
        public int __AnsiY_0 = 0;
        public bool __AnsiWrapFlag_ = false;
        public bool __AnsiWrapFlag_0 = false;

        public bool DECCOLMPreserve = false;

        public List<int> __AnsiCmd = new List<int>();

        public List<int> __AnsiTabs = new List<int>();

        public string __AnsiDCS = "";
        public bool __AnsiDCS_ = false;

        public bool __AnsiVT52 = false;
        public bool[] VT100_SemigraphDef = new bool[4];
        public int VT100_SemigraphNum = 0;
        public bool[] VT100_SemigraphDef0 = new bool[4];
        public int VT100_SemigraphNum0 = 0;
        public bool[] VT100_SemigraphDef_ = new bool[4];
        public int VT100_SemigraphNum_ = 0;
        public bool[] VT100_SemigraphDef_0 = new bool[4];
        public int VT100_SemigraphNum_0 = 0;
        public bool VT52_SemigraphDef = false;

        public int __AnsiScrollFirst = 0;
        public int __AnsiScrollLast = 0;
        public bool __AnsiOrigin = false;
        public bool __AnsiMarginLeftRight = false;
        public int __AnsiMarginLeft = 0;
        public int __AnsiMarginRight = 0;

        public bool __AnsiInsertMode = false;

        public int __AnsiCounter = 0;

        public long __AnsiAdditionalChars = 0;

        public bool __AnsiCommand = false;
        public bool __AnsiSmoothScroll = false;

        public List<int> __AnsiFontSizeAttr = new List<int>();

        public AnsiLineOccupy __AnsiLineOccupy__ = new AnsiLineOccupy();
        public AnsiLineOccupy __AnsiLineOccupy1__ = new AnsiLineOccupy();
        public AnsiLineOccupy __AnsiLineOccupy2__ = new AnsiLineOccupy();

        public AnsiLineOccupy __AnsiLineOccupy__0 = new AnsiLineOccupy();
        public AnsiLineOccupy __AnsiLineOccupy1__0 = new AnsiLineOccupy();
        public AnsiLineOccupy __AnsiLineOccupy2__0 = new AnsiLineOccupy();

        public bool AnsiScrollRev = false;
        public int AnsiScrollLinesI = 0;
        public int AnsiScrollCounter = 0;
        public enum AnsiScrollCommandDef { None, Char, FirstLast, Tab };
        public AnsiScrollCommandDef AnsiScrollCommand = AnsiScrollCommandDef.None;
        public int AnsiScrollParam1 = 0;
        public int AnsiScrollParam2 = 0;
        public int AnsiScrollParam3 = 0;
        public int AnsiScrollParam4 = 0;

        public int AnsiBufferI = 0;

        public int ScrollLastOffset = 0;
        public int AnsiRingBellCount = 0;

        public int PrintCharCounter = 0;
        public int PrintCharCounterOver = 0;
        public int PrintCharInsDel = 0;
        public int PrintCharScroll = 0;

        private List<int> DecParamI = new List<int>();
        private List<int> DecParamV = new List<int>();

        public List<List<int>> CharProtection1 = new List<List<int>>();
        public List<List<int>> CharProtection2 = new List<List<int>>();
        public bool CharProtection1Print = false;
        public bool CharProtection2Print = false;
        public bool CharProtection2Ommit = false;

        public void CharProtection1Set(int X, int Y, bool V)
        {
            while (CharProtection1.Count <= Y)
            {
                CharProtection1.Add(new List<int>());
            }
            while (CharProtection1[Y].Count <= X)
            {
                CharProtection1[Y].Add(0);
            }
            CharProtection1[Y][X] = (V ? 1 : 0);
        }

        public void CharProtection2Set(int X, int Y, bool V)
        {
            while (CharProtection2.Count <= Y)
            {
                CharProtection2.Add(new List<int>());
            }
            while (CharProtection2[Y].Count <= X)
            {
                CharProtection2[Y].Add(0);
            }
            CharProtection2[Y][X] = (V ? 1 : 0);
        }

        public bool CharProtection1Get(int X, int Y)
        {
            if (CharProtection1.Count > Y)
            {
                if (CharProtection1[Y].Count > X)
                {
                    return (CharProtection1[Y][X] != 0);
                }
            }
            return false;
        }

        public bool CharProtection2Get(int X, int Y)
        {
            if (CharProtection2Ommit)
            {
                return false;
            }
            if (CharProtection2.Count > Y)
            {
                if (CharProtection2[Y].Count > X)
                {
                    return (CharProtection2[Y][X] != 0);
                }
            }
            return false;
        }

        public bool IsScreenAlternate = false;

        public void ScreenAlte()
        {
            if (!IsScreenAlternate)
            {
                AnsiLineOccupy.Swap(ref __AnsiLineOccupy__0, ref __AnsiLineOccupy__);
                AnsiLineOccupy.Swap(ref __AnsiLineOccupy1__0, ref __AnsiLineOccupy1__);
                AnsiLineOccupy.Swap(ref __AnsiLineOccupy2__0, ref __AnsiLineOccupy2__);

                int _I;
                bool _B;

                __AnsiBack0 = __AnsiBack;
                __AnsiFore0 = __AnsiFore;
                __AnsiAttr0 = __AnsiAttr;

                _I = __AnsiX; __AnsiX = __AnsiX0; __AnsiX0 = _I;
                _I = __AnsiY; __AnsiY = __AnsiY0; __AnsiY0 = _I;
                _I = __AnsiBack; __AnsiBack = __AnsiBack0; __AnsiBack0 = _I;
                _I = __AnsiFore; __AnsiFore = __AnsiFore0; __AnsiFore0 = _I;
                _I = __AnsiAttr; __AnsiAttr = __AnsiAttr0; __AnsiAttr0 = _I;
                _B = __AnsiWrapFlag; __AnsiWrapFlag = __AnsiWrapFlag0; __AnsiWrapFlag0 = _B;

                _I = __AnsiX_; __AnsiX_ = __AnsiX_0; __AnsiX_0 = _I;
                _I = __AnsiY_; __AnsiY_ = __AnsiY_0; __AnsiY_0 = _I;
                _I = __AnsiBack_; __AnsiBack_ = __AnsiBack_0; __AnsiBack_0 = _I;
                _I = __AnsiFore_; __AnsiFore_ = __AnsiFore_0; __AnsiFore_0 = _I;
                _I = __AnsiAttr_; __AnsiAttr_ = __AnsiAttr_0; __AnsiAttr_0 = _I;
                _B = __AnsiWrapFlag_; __AnsiWrapFlag_ = __AnsiWrapFlag_0; __AnsiWrapFlag_0 = _B;

                _B = VT100_SemigraphDef[0]; VT100_SemigraphDef[0] = VT100_SemigraphDef0[0]; VT100_SemigraphDef0[0] = _B;
                _B = VT100_SemigraphDef[1]; VT100_SemigraphDef[1] = VT100_SemigraphDef0[1]; VT100_SemigraphDef0[1] = _B;
                _B = VT100_SemigraphDef[2]; VT100_SemigraphDef[2] = VT100_SemigraphDef0[2]; VT100_SemigraphDef0[2] = _B;
                _B = VT100_SemigraphDef[3]; VT100_SemigraphDef[3] = VT100_SemigraphDef0[3]; VT100_SemigraphDef0[3] = _B;
                _I = VT100_SemigraphNum; VT100_SemigraphNum = VT100_SemigraphNum0; VT100_SemigraphNum0 = _I;

                _B = VT100_SemigraphDef_[0]; VT100_SemigraphDef_[0] = VT100_SemigraphDef_0[0]; VT100_SemigraphDef_0[0] = _B;
                _B = VT100_SemigraphDef_[1]; VT100_SemigraphDef_[1] = VT100_SemigraphDef_0[1]; VT100_SemigraphDef_0[1] = _B;
                _B = VT100_SemigraphDef_[2]; VT100_SemigraphDef_[2] = VT100_SemigraphDef_0[2]; VT100_SemigraphDef_0[2] = _B;
                _B = VT100_SemigraphDef_[3]; VT100_SemigraphDef_[3] = VT100_SemigraphDef_0[3]; VT100_SemigraphDef_0[3] = _B;
                _I = VT100_SemigraphNum_; VT100_SemigraphNum_ = VT100_SemigraphNum_0; VT100_SemigraphNum_0 = _I;

                IsScreenAlternate = true;
            }
        }

        public void ScreenMain()
        {
            if (IsScreenAlternate)
            {
                AnsiLineOccupy.Swap(ref __AnsiLineOccupy__0, ref __AnsiLineOccupy__);
                AnsiLineOccupy.Swap(ref __AnsiLineOccupy1__0, ref __AnsiLineOccupy1__);
                AnsiLineOccupy.Swap(ref __AnsiLineOccupy2__0, ref __AnsiLineOccupy2__);

                int _I;
                bool _B;

                _I = __AnsiX; __AnsiX = __AnsiX0; __AnsiX0 = _I;
                _I = __AnsiY; __AnsiY = __AnsiY0; __AnsiY0 = _I;
                _I = __AnsiBack; __AnsiBack = __AnsiBack0; __AnsiBack0 = _I;
                _I = __AnsiFore; __AnsiFore = __AnsiFore0; __AnsiFore0 = _I;
                _I = __AnsiAttr; __AnsiAttr = __AnsiAttr0; __AnsiAttr0 = _I;

                _I = __AnsiX_; __AnsiX_ = __AnsiX_0; __AnsiX_0 = _I;
                _I = __AnsiY_; __AnsiY_ = __AnsiY_0; __AnsiY_0 = _I;
                _I = __AnsiBack_; __AnsiBack_ = __AnsiBack_0; __AnsiBack_0 = _I;
                _I = __AnsiFore_; __AnsiFore_ = __AnsiFore_0; __AnsiFore_0 = _I;
                _I = __AnsiAttr_; __AnsiAttr_ = __AnsiAttr_0; __AnsiAttr_0 = _I;

                _B = VT100_SemigraphDef[0]; VT100_SemigraphDef[0] = VT100_SemigraphDef0[0]; VT100_SemigraphDef0[0] = _B;
                _B = VT100_SemigraphDef[1]; VT100_SemigraphDef[1] = VT100_SemigraphDef0[1]; VT100_SemigraphDef0[1] = _B;
                _B = VT100_SemigraphDef[2]; VT100_SemigraphDef[2] = VT100_SemigraphDef0[2]; VT100_SemigraphDef0[2] = _B;
                _B = VT100_SemigraphDef[3]; VT100_SemigraphDef[3] = VT100_SemigraphDef0[3]; VT100_SemigraphDef0[3] = _B;
                _I = VT100_SemigraphNum; VT100_SemigraphNum = VT100_SemigraphNum0; VT100_SemigraphNum0 = _I;

                _B = VT100_SemigraphDef_[0]; VT100_SemigraphDef_[0] = VT100_SemigraphDef_0[0]; VT100_SemigraphDef_0[0] = _B;
                _B = VT100_SemigraphDef_[1]; VT100_SemigraphDef_[1] = VT100_SemigraphDef_0[1]; VT100_SemigraphDef_0[1] = _B;
                _B = VT100_SemigraphDef_[2]; VT100_SemigraphDef_[2] = VT100_SemigraphDef_0[2]; VT100_SemigraphDef_0[2] = _B;
                _B = VT100_SemigraphDef_[3]; VT100_SemigraphDef_[3] = VT100_SemigraphDef_0[3]; VT100_SemigraphDef_0[3] = _B;
                _I = VT100_SemigraphNum_; VT100_SemigraphNum_ = VT100_SemigraphNum_0; VT100_SemigraphNum_0 = _I;

                IsScreenAlternate = false;
            }
        }

        public void CursorSave()
        {
            __AnsiX_ = __AnsiX;
            __AnsiY_ = __AnsiY;
            __AnsiBack_ = __AnsiBack;
            __AnsiFore_ = __AnsiFore;
            __AnsiAttr_ = __AnsiAttr;
            __AnsiWrapFlag_ = __AnsiWrapFlag;
            VT100_SemigraphDef_[0] = VT100_SemigraphDef[0];
            VT100_SemigraphDef_[1] = VT100_SemigraphDef[1];
            VT100_SemigraphDef_[2] = VT100_SemigraphDef[2];
            VT100_SemigraphDef_[3] = VT100_SemigraphDef[3];
            VT100_SemigraphNum_ = VT100_SemigraphNum;
        }

        public void CursorLoad()
        {
            __AnsiX = __AnsiX_;
            __AnsiY = __AnsiY_;
            __AnsiBack = __AnsiBack_;
            __AnsiFore = __AnsiFore_;
            __AnsiAttr = __AnsiAttr_;
            __AnsiWrapFlag = __AnsiWrapFlag_;
            VT100_SemigraphDef[0] = VT100_SemigraphDef_[0];
            VT100_SemigraphDef[1] = VT100_SemigraphDef_[1];
            VT100_SemigraphDef[2] = VT100_SemigraphDef_[2];
            VT100_SemigraphDef[3] = VT100_SemigraphDef_[3];
            VT100_SemigraphNum = VT100_SemigraphNum_;
        }

        public void DecParamSet(int N, int V)
        {
            for (int i = 0; i < DecParamI.Count; i++)
            {
                if (DecParamI[i] == N)
                {
                    DecParamV[i] = V;
                    return;
                }
            }
            DecParamI.Add(N);
            DecParamV.Add(V);
        }

        public int DecParamGet(int N)
        {
            for (int i = 0; i < DecParamI.Count; i++)
            {
                if (DecParamI[i] == N)
                {
                    return DecParamV[i];
                }
            }
            return 0;
        }

        public void AnsiParamSet(int N, int V)
        {
            N = 0 - (N + 1);
            for (int i = 0; i < DecParamI.Count; i++)
            {
                if (DecParamI[i] == N)
                {
                    DecParamV[i] = V;
                    return;
                }
            }
            DecParamI.Add(N);
            DecParamV.Add(V);
        }

        public int AnsiParamGet(int N)
        {
            N = 0 - (N + 1);
            for (int i = 0; i < DecParamI.Count; i++)
            {
                if (DecParamI[i] == N)
                {
                    return DecParamV[i];
                }
            }
            return 0;
        }

        public void Zero(bool __AnsiUseEOF_)
        {
            __AnsiUseEOF = __AnsiUseEOF_;
            AnsiBufferI = 0;

            __AnsiCounter = 0;
            __AnsiProcessStep = 0;
            __AnsiProcessDelay = 0;
            __AnsiProcessDelayMin = 1;
            __AnsiProcessDelayMax = -1;
            AnsiRingBellCount = 0;

        }

        public void Reset(int AnsiMaxX, int AnsiMaxY, int NormalB, int NormalF)
        {
            /*
            0 - not recognized
            1 - set
            2 - reset
            3 - permanently set
            4 - permanently reset
            */
            DecParamI.Clear();
            DecParamV.Clear();

            TerminalW = AnsiMaxX;
            TerminalH = AnsiMaxY;

            StatusBar = false;

            PrintCharCounter = 0;
            PrintCharCounterOver = 0;
            PrintCharInsDel = 0;
            PrintCharScroll = 0;

            IsScreenAlternate = false;

            DECCOLMPreserve = false;

            __AnsiFontSizeAttr.Clear();
            __AnsiLineOccupy__.Clear();
            __AnsiLineOccupy1__.Clear();
            __AnsiLineOccupy2__.Clear();
            __AnsiLineOccupy__0.Clear();
            __AnsiLineOccupy1__0.Clear();
            __AnsiLineOccupy2__0.Clear();
            __AnsiCmd.Clear();

            __AnsiTabs.Clear();
            __AnsiTabs.Add(-1);

            CharProtection1.Clear();
            CharProtection2.Clear();
            CharProtection1Print = false;
            CharProtection2Print = false;
            CharProtection2Ommit = false;

            __AnsiSmoothScroll = false;
            __AnsiCommand = false;

            __AnsiBeyondEOF = false;
            __AnsiMusic = false;
            __AnsiNoWrap = false;
            __AnsiDCS = "";
            __AnsiDCS_ = false;

            VT100_SemigraphDef[0] = false;
            VT100_SemigraphDef[1] = false;
            VT100_SemigraphDef[2] = false;
            VT100_SemigraphDef[3] = false;
            VT100_SemigraphNum = 0;
            VT100_SemigraphDef0[0] = false;
            VT100_SemigraphDef0[1] = false;
            VT100_SemigraphDef0[2] = false;
            VT100_SemigraphDef0[3] = false;
            VT100_SemigraphNum0 = 0;
            VT100_SemigraphDef_[0] = false;
            VT100_SemigraphDef_[1] = false;
            VT100_SemigraphDef_[2] = false;
            VT100_SemigraphDef_[3] = false;
            VT100_SemigraphNum_ = 0;
            VT100_SemigraphDef_0[0] = false;
            VT100_SemigraphDef_0[1] = false;
            VT100_SemigraphDef_0[2] = false;
            VT100_SemigraphDef_0[3] = false;
            VT100_SemigraphNum_0 = 0;
            VT52_SemigraphDef = false;
            __AnsiVT52 = false;

            __AnsiX = 0;
            __AnsiY = 0;
            __AnsiX0 = 0;
            __AnsiY0 = 0;
            __AnsiWrapFlag = false;
            __AnsiWrapFlag0 = false;

            __AnsiX_ = 0;
            __AnsiY_ = 0;
            __AnsiBack_ = -1;
            __AnsiFore_ = -1;
            __AnsiAttr_ = 0;
            __AnsiX_0 = 0;
            __AnsiY_0 = 0;
            __AnsiBack_0 = -1;
            __AnsiFore_0 = -1;
            __AnsiAttr_0 = 0;
            __AnsiWrapFlag_ = false;
            __AnsiWrapFlag_0 = false;

            __AnsiOrigin = false;
            __AnsiMarginLeftRight = false;
            __AnsiInsertMode = false;
            __AnsiFontSizeW = 0;
            __AnsiFontSizeH = 0;

            __AnsiScrollFirst = 0;
            __AnsiScrollLast = AnsiMaxY - 1;
            __AnsiMarginLeft = 0;
            __AnsiMarginRight = AnsiMaxX;

            __AnsiBack = -1;
            __AnsiFore = -1;
            __AnsiAttr = 0;
            __AnsiBack0 = -1;
            __AnsiFore0 = -1;
            __AnsiAttr0 = 0;

            __AnsiAdditionalChars = 0;



            AnsiScrollRev = false;
            AnsiScrollCounter = 0;
            AnsiScrollCommand = AnsiScrollCommandDef.None;
            AnsiScrollParam1 = 0;
            AnsiScrollParam2 = 0;
            AnsiScrollParam3 = 0;
            AnsiScrollLinesI = 0;


            ScrollLastOffset = 0;
        }

        public string GetScreen(int X1, int Y1, int X2, int Y2)
        {
            List<List<int>> ScreenLine = new List<List<int>>();
            for (int Y = Y1; Y < Y2; Y++)
            {
                List<int> ScreenBuf = new List<int>();
                for (int X = X1; X < X2; X++)
                {
                    if (__AnsiLineOccupy__.CountLines() > Y)
                    {
                        if (__AnsiLineOccupy__.CountItems(Y) > X)
                        {
                            __AnsiLineOccupy__.Get(Y, X);
                            ScreenBuf.Add(__AnsiLineOccupy__.Item_Char);
                        }
                    }
                }
                ScreenLine.Add(ScreenBuf);
                while ((ScreenBuf.Count > 0) && (ScreenBuf[ScreenBuf.Count - 1] == 32))
                {
                    ScreenBuf.RemoveAt(ScreenBuf.Count - 1);
                }
            }

            while ((ScreenLine.Count > 0) && (ScreenLine[0].Count == 0))
            {
                ScreenLine.RemoveAt(0);
            }
            while ((ScreenLine.Count > 0) && (ScreenLine[ScreenLine.Count - 1].Count == 0))
            {
                ScreenLine.RemoveAt(ScreenLine.Count - 1);
            }

            StringBuilder Sb = new StringBuilder();
            for (int i = 0; i < ScreenLine.Count; i++)
            {
                Sb.AppendLine(TextWork.IntToStr(ScreenLine[i]));
            }
            return Sb.ToString();
        }

        private static void CopyListInt(ref List<int> Src, ref List<int> Dst)
        {
            Dst.Clear();
            for (int i = 0; i < Src.Count; i++)
            {
                Dst.Add(Src[i]);
            }
        }

        private static void CopyListList(ref List<List<int>> Src, ref List<List<int>> Dst)
        {
            Dst.Clear();
            for (int i = 0; i < Src.Count; i++)
            {
                List<int> Temp = new List<int>();
                for (int ii = 0; ii < Src[i].Count; ii++)
                {
                    Temp.Add(Src[i][ii]);
                }
                Dst.Add(Temp);
            }
        }

        public static void Copy(AnsiState Src, AnsiState Dst)
        {
            Dst.TerminalW = Src.TerminalW;
            Dst.TerminalH = Src.TerminalH;

            Dst.PrintCharCounter = Src.PrintCharCounter;
            Dst.PrintCharCounterOver = Src.PrintCharCounterOver;
            Dst.PrintCharInsDel = Src.PrintCharInsDel;
            Dst.PrintCharScroll = Src.PrintCharScroll;

            Dst.DECCOLMPreserve = Src.DECCOLMPreserve;

            Dst.__AnsiUseEOF = Src.__AnsiUseEOF;
            Dst.IsScreenAlternate = Src.IsScreenAlternate;

            CopyListInt(ref Src.DecParamI, ref Dst.DecParamI);
            CopyListInt(ref Src.DecParamV, ref Dst.DecParamV);
            CopyListInt(ref Src.__AnsiFontSizeAttr, ref Dst.__AnsiFontSizeAttr);
            CopyListInt(ref Src.__AnsiCmd, ref Dst.__AnsiCmd);
            CopyListInt(ref Src.__AnsiTabs, ref Dst.__AnsiTabs);
            AnsiLineOccupy.Copy(ref Src.__AnsiLineOccupy__, ref Dst.__AnsiLineOccupy__);
            AnsiLineOccupy.Copy(ref Src.__AnsiLineOccupy1__, ref Dst.__AnsiLineOccupy1__);
            AnsiLineOccupy.Copy(ref Src.__AnsiLineOccupy2__, ref Dst.__AnsiLineOccupy2__);
            AnsiLineOccupy.Copy(ref Src.__AnsiLineOccupy__0, ref Dst.__AnsiLineOccupy__0);
            AnsiLineOccupy.Copy(ref Src.__AnsiLineOccupy1__0, ref Dst.__AnsiLineOccupy1__0);
            AnsiLineOccupy.Copy(ref Src.__AnsiLineOccupy2__0, ref Dst.__AnsiLineOccupy2__0);
            CopyListList(ref Src.CharProtection1, ref Dst.CharProtection1);
            CopyListList(ref Src.CharProtection2, ref Dst.CharProtection2);
            Dst.CharProtection1Print = Src.CharProtection1Print;
            Dst.CharProtection2Print = Src.CharProtection2Print;
            Dst.CharProtection2Ommit = Src.CharProtection2Ommit;

            Dst.__AnsiSmoothScroll = Src.__AnsiSmoothScroll;
            Dst.__AnsiCommand = Src.__AnsiCommand;

            Dst.__AnsiBeyondEOF = Src.__AnsiBeyondEOF;
            Dst.__AnsiMusic = Src.__AnsiMusic;
            Dst.__AnsiNoWrap = Src.__AnsiNoWrap;
            Dst.__AnsiDCS = Src.__AnsiDCS;
            Dst.__AnsiDCS_ = Src.__AnsiDCS_;

            Dst.StatusBar = Src.StatusBar;

            Dst.VT100_SemigraphDef[0] = Src.VT100_SemigraphDef[0];
            Dst.VT100_SemigraphDef[1] = Src.VT100_SemigraphDef[1];
            Dst.VT100_SemigraphDef[2] = Src.VT100_SemigraphDef[2];
            Dst.VT100_SemigraphDef[3] = Src.VT100_SemigraphDef[3];
            Dst.VT100_SemigraphNum = Src.VT100_SemigraphNum;
            Dst.VT100_SemigraphDef0[0] = Src.VT100_SemigraphDef0[0];
            Dst.VT100_SemigraphDef0[1] = Src.VT100_SemigraphDef0[1];
            Dst.VT100_SemigraphDef0[2] = Src.VT100_SemigraphDef0[2];
            Dst.VT100_SemigraphDef0[3] = Src.VT100_SemigraphDef0[3];
            Dst.VT100_SemigraphNum0 = Src.VT100_SemigraphNum0;
            Dst.VT100_SemigraphDef_[0] = Src.VT100_SemigraphDef_[0];
            Dst.VT100_SemigraphDef_[1] = Src.VT100_SemigraphDef_[1];
            Dst.VT100_SemigraphDef_[2] = Src.VT100_SemigraphDef_[2];
            Dst.VT100_SemigraphDef_[3] = Src.VT100_SemigraphDef_[3];
            Dst.VT100_SemigraphNum_ = Src.VT100_SemigraphNum_;
            Dst.VT100_SemigraphDef_0[0] = Src.VT100_SemigraphDef_0[0];
            Dst.VT100_SemigraphDef_0[1] = Src.VT100_SemigraphDef_0[1];
            Dst.VT100_SemigraphDef_0[2] = Src.VT100_SemigraphDef_0[2];
            Dst.VT100_SemigraphDef_0[3] = Src.VT100_SemigraphDef_0[3];
            Dst.VT100_SemigraphNum_0 = Src.VT100_SemigraphNum_0;
            Dst.VT52_SemigraphDef = Src.VT52_SemigraphDef;
            Dst.__AnsiVT52 = Src.__AnsiVT52;

            Dst.__AnsiX = Src.__AnsiX;
            Dst.__AnsiY = Src.__AnsiY;
            Dst.__AnsiX0 = Src.__AnsiX;
            Dst.__AnsiY0 = Src.__AnsiY;

            Dst.__AnsiX_ = Src.__AnsiX_;
            Dst.__AnsiY_ = Src.__AnsiY_;
            Dst.__AnsiBack_ = Src.__AnsiBack_;
            Dst.__AnsiFore_ = Src.__AnsiFore_;
            Dst.__AnsiAttr_ = Src.__AnsiAttr_;
            Dst.__AnsiX_0 = Src.__AnsiX_0;
            Dst.__AnsiY_0 = Src.__AnsiY_0;
            Dst.__AnsiBack_0 = Src.__AnsiBack_0;
            Dst.__AnsiFore_0 = Src.__AnsiFore_0;
            Dst.__AnsiAttr_0 = Src.__AnsiAttr_0;

            Dst.__AnsiWrapFlag = Src.__AnsiWrapFlag;
            Dst.__AnsiWrapFlag_ = Src.__AnsiWrapFlag_;
            Dst.__AnsiWrapFlag0 = Src.__AnsiWrapFlag0;
            Dst.__AnsiWrapFlag_0 = Src.__AnsiWrapFlag_0;

            Dst.__AnsiOrigin = Src.__AnsiOrigin;
            Dst.__AnsiMarginLeftRight = Src.__AnsiMarginLeftRight;
            Dst.__AnsiInsertMode = Src.__AnsiInsertMode;
            Dst.__AnsiFontSizeW = Src.__AnsiFontSizeW;
            Dst.__AnsiFontSizeH = Src.__AnsiFontSizeH;

            Dst.__AnsiScrollFirst = Src.__AnsiScrollFirst;
            Dst.__AnsiScrollLast = Src.__AnsiScrollLast;
            Dst.__AnsiMarginLeft = Src.__AnsiMarginLeft;
            Dst.__AnsiMarginRight = Src.__AnsiMarginRight;

            Dst.__AnsiBack = Src.__AnsiBack;
            Dst.__AnsiFore = Src.__AnsiFore;
            Dst.__AnsiAttr = Src.__AnsiAttr;
            Dst.__AnsiBack0 = Src.__AnsiBack0;
            Dst.__AnsiFore0 = Src.__AnsiFore0;
            Dst.__AnsiAttr0 = Src.__AnsiAttr0;

            Dst.__AnsiCounter = Src.__AnsiCounter;
            Dst.__AnsiAdditionalChars = Src.__AnsiAdditionalChars;

            Dst.AnsiScrollRev = Src.AnsiScrollRev;
            Dst.AnsiScrollCounter = Src.AnsiScrollCounter;
            Dst.AnsiScrollCommand = Src.AnsiScrollCommand;
            Dst.AnsiScrollParam1 = Src.AnsiScrollParam1;
            Dst.AnsiScrollParam2 = Src.AnsiScrollParam2;
            Dst.AnsiScrollParam3 = Src.AnsiScrollParam3;
            Dst.AnsiScrollLinesI = Src.AnsiScrollLinesI;

            Dst.__AnsiProcessStep = Src.__AnsiProcessStep;
            Dst.__AnsiProcessDelay = Src.__AnsiProcessDelay;
            Dst.__AnsiProcessDelayMin = Src.__AnsiProcessDelayMin;
            Dst.__AnsiProcessDelayMax = Src.__AnsiProcessDelayMax;

            Dst.AnsiBufferI = Src.AnsiBufferI;

            Dst.ScrollLastOffset = Src.ScrollLastOffset;
            Dst.AnsiRingBellCount = Src.AnsiRingBellCount;
        }

        public AnsiState Clone()
        {
            AnsiState Obj = new AnsiState();
            Copy(this, Obj);
            return Obj;
        }
    }
}
