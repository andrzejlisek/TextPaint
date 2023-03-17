using System;
using System.Collections.Generic;

namespace TextPaint
{
    public class AnsiState
    {
        public long __AnsiProcessStep = 0;
        public long __AnsiProcessDelay = 0;
        public long __AnsiProcessDelayMin = 1;
        public long __AnsiProcessDelayMax = -1;


        public bool __AnsiMusic = false;
        public bool __AnsiUseEOF = false;
        public bool __AnsiBeyondEOF = false;
        public bool __AnsiNoWrap = false;

        public int __AnsiBackScroll = -1;
        public int __AnsiForeScroll = -1;
        public int __AnsiBack = -1;
        public int __AnsiFore = -1;
        public int __AnsiBackWork = -1;
        public int __AnsiForeWork = -1;
        public bool __AnsiFontBold = false;
        public bool __AnsiFontInverse = false;
        public bool __AnsiFontBlink = false;
        public bool __AnsiFontInvisible = false;
        public int __AnsiFontSizeW = 0;
        public int __AnsiFontSizeH = 0;


        public int __AnsiX = 0;
        public int __AnsiY = 0;

        public bool StatusBar = true;

        public int __AnsiBack_ = -1;
        public int __AnsiFore_ = -1;
        public int __AnsiX_ = 0;
        public int __AnsiY_ = 0;
        public bool __AnsiFontBold_ = false;
        public bool __AnsiFontInverse_ = false;
        public bool __AnsiFontBlink_ = false;
        public bool __AnsiFontInvisible_ = false;

        public List<int> __AnsiCmd = new List<int>();

        public List<int> __AnsiTabs = new List<int>();

        public string __AnsiDCS = "";
        public bool __AnsiDCS_ = false;

        public bool __AnsiVT52 = false;
        public bool[] VT100_SemigraphDef = new bool[4];
        public int VT100_SemigraphNum = 0;
        public bool[] VT100_SemigraphDef_ = new bool[4];
        public int VT100_SemigraphNum_ = 0;
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

        public List<List<int>> __AnsiLineOccupy = new List<List<int>>();
        public List<List<int>> __AnsiLineOccupy1 = new List<List<int>>();
        public List<List<int>> __AnsiLineOccupy2 = new List<List<int>>();


        public bool AnsiScrollRev = false;
        public int AnsiScrollLinesI = 0;
        public int AnsiScrollCounter = 0;
        public enum AnsiScrollCommandDef { None, Char, FirstLast, Tab };
        public AnsiScrollCommandDef AnsiScrollCommand = AnsiScrollCommandDef.None;
        public int AnsiScrollParam1 = 0;
        public int AnsiScrollParam2 = 0;
        public int AnsiScrollParam3 = 0;

        public int AnsiBufferI = 0;

        public int ScrollLastOffset = 0;
        public int AnsiRingBellCount = 0;

        public int PrintCharCounter = 0;
        public int PrintCharCounterOver = 0;
        public int PrintCharInsDel = 0;
        public int PrintCharScroll = 0;

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

        public void Reset(int AnsiMaxX, int AnsiMaxY)
        {
            StatusBar = false;

            PrintCharCounter = 0;
            PrintCharCounterOver = 0;
            PrintCharInsDel = 0;
            PrintCharScroll = 0;

            __AnsiFontSizeAttr.Clear();
            __AnsiLineOccupy.Clear();
            __AnsiLineOccupy1.Clear();
            __AnsiLineOccupy2.Clear();
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
            VT100_SemigraphDef_[0] = false;
            VT100_SemigraphDef_[1] = false;
            VT100_SemigraphDef_[2] = false;
            VT100_SemigraphDef_[3] = false;
            VT100_SemigraphNum_ = 0;
            VT52_SemigraphDef = false;
            __AnsiVT52 = false;

            __AnsiX = 0;
            __AnsiY = 0;

            __AnsiX_ = 0;
            __AnsiY_ = 0;
            __AnsiBack_ = -1;
            __AnsiFore_ = -1;
            __AnsiFontBold_ = false;
            __AnsiFontInverse_ = false;
            __AnsiFontBlink_ = false;
            __AnsiFontInvisible_ = false;

            __AnsiOrigin = false;
            __AnsiMarginLeftRight = false;
            __AnsiInsertMode = false;
            __AnsiFontSizeW = 0;
            __AnsiFontSizeH = 0;

            __AnsiScrollFirst = 0;
            __AnsiScrollLast = AnsiMaxY - 1;
            __AnsiMarginLeft = 0;
            __AnsiMarginRight = AnsiMaxX - 1;

            __AnsiBack = -1;
            __AnsiFore = -1;
            __AnsiFontBold = false;
            __AnsiFontInverse = false;
            __AnsiFontBlink = false;
            __AnsiFontInvisible = false;

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
            Dst.PrintCharCounter = Src.PrintCharCounter;
            Dst.PrintCharCounterOver = Src.PrintCharCounterOver;
            Dst.PrintCharInsDel = Src.PrintCharInsDel;
            Dst.PrintCharScroll = Src.PrintCharScroll;

            Dst.__AnsiUseEOF = Src.__AnsiUseEOF;

            CopyListInt(ref Src.__AnsiFontSizeAttr, ref Dst.__AnsiFontSizeAttr);
            CopyListInt(ref Src.__AnsiCmd, ref Dst.__AnsiCmd);
            CopyListInt(ref Src.__AnsiTabs, ref Dst.__AnsiTabs);
            CopyListList(ref Src.__AnsiLineOccupy, ref Dst.__AnsiLineOccupy);
            CopyListList(ref Src.__AnsiLineOccupy1, ref Dst.__AnsiLineOccupy1);
            CopyListList(ref Src.__AnsiLineOccupy2, ref Dst.__AnsiLineOccupy2);
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
            Dst.VT100_SemigraphDef_[0] = Src.VT100_SemigraphDef_[0];
            Dst.VT100_SemigraphDef_[1] = Src.VT100_SemigraphDef_[1];
            Dst.VT100_SemigraphDef_[2] = Src.VT100_SemigraphDef_[2];
            Dst.VT100_SemigraphDef_[3] = Src.VT100_SemigraphDef_[3];
            Dst.VT100_SemigraphNum_ = Src.VT100_SemigraphNum_;
            Dst.VT52_SemigraphDef = Src.VT52_SemigraphDef;
            Dst.__AnsiVT52 = Src.__AnsiVT52;

            Dst.__AnsiX = Src.__AnsiX;
            Dst.__AnsiY = Src.__AnsiY;

            Dst.__AnsiX_ = Src.__AnsiX_;
            Dst.__AnsiY_ = Src.__AnsiY_;
            Dst.__AnsiBack_ = Src.__AnsiBack_;
            Dst.__AnsiFore_ = Src.__AnsiFore_;
            Dst.__AnsiFontBold_ = Src.__AnsiFontBold_;
            Dst.__AnsiFontInverse_ = Src.__AnsiFontInverse_;
            Dst.__AnsiFontBlink_ = Src.__AnsiFontBlink_;
            Dst.__AnsiFontInvisible_ = Src.__AnsiFontInvisible_;

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
            Dst.__AnsiFontBold = Src.__AnsiFontBold;
            Dst.__AnsiFontInverse = Src.__AnsiFontInverse;
            Dst.__AnsiFontBlink = Src.__AnsiFontBlink;
            Dst.__AnsiFontInvisible = Src.__AnsiFontInvisible;

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
