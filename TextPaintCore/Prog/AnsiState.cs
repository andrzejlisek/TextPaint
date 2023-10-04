using System;
using System.Collections.Generic;
using System.Text;

namespace TextPaint
{
    public class AnsiState
    {
        public static void InitCharMap(ConfigFile CF)
        {

        }

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
        public bool VT52_SemigraphDef = false;

        private string[] CharMapName = new string[4];
        private string[] CharMapName0 = new string[4];
        private string[] CharMapName_ = new string[4];
        private string[] CharMapName_0 = new string[4];
        private int[,] CharMap = new int[4, 128];
        private int[,] CharMap0 = new int[4, 128];
        private int[,] CharMap_ = new int[4, 128];
        private int[,] CharMap_0 = new int[4, 128];
        public int CharMapNumGL = 0;
        private int CharMapNumGL0 = 0;
        private int CharMapNumGL_ = 0;
        private int CharMapNumGL_0 = 0;
        public int CharMapNumGR = 2;
        private int CharMapNumGR0 = 2;
        private int CharMapNumGR_ = 2;
        private int CharMapNumGR_0 = 2;
        public bool CharMapNRCS = false;

        public int GetChar(int CharCode)
        {
            if ((CharCode >= 32) && (CharCode <= 127))
            {
                if (CharMapNumGL >= 10)
                {
                    if ((CharMapNumGL >= 20) && (CharMapNumGL <= 29))
                    {
                        CharMapNumGL = CharMapNumGL % 10;
                        return CharMap[2, CharCode];
                    }
                    if ((CharMapNumGL >= 30) && (CharMapNumGL <= 39))
                    {
                        CharMapNumGL = CharMapNumGL % 10;
                        return CharMap[3, CharCode];
                    }
                }
                return CharMap[CharMapNumGL, CharCode];
            }
            if ((CharCode >= 160) && (CharCode <= 255))
            {
                return CharMap[CharMapNumGR, CharCode - 128];
            }
            return CharCode;
        }

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
                string _S;

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

                for (int ii = 0; ii < 4; ii++)
                {
                    _S = CharMapName[ii];
                    CharMapName[ii] = CharMapName0[ii];
                    CharMapName0[ii] = _S;
                    _S = CharMapName_[ii];
                    CharMapName_[ii] = CharMapName_0[ii];
                    CharMapName_0[ii] = _S;
                    for (int i = 32; i < 128; i++)
                    {
                        _I = CharMap[ii, i]; CharMap[ii, i] = CharMap0[ii, i]; CharMap0[ii, i] = _I;
                        _I = CharMap_[ii, i]; CharMap_[ii, i] = CharMap_0[ii, i]; CharMap_0[ii, i] = _I;
                    }
                }
                _I = CharMapNumGL; CharMapNumGL = CharMapNumGL0 = CharMapNumGL0 = _I;
                _I = CharMapNumGL_; CharMapNumGL_ = CharMapNumGL_0 = CharMapNumGL_0 = _I;
                _I = CharMapNumGR; CharMapNumGR = CharMapNumGR0 = CharMapNumGR0 = _I;
                _I = CharMapNumGR_; CharMapNumGR_ = CharMapNumGR_0 = CharMapNumGR_0 = _I;

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
                string _S;


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

                for (int ii = 0; ii < 4; ii++)
                {
                    _S = CharMapName[ii];
                    CharMapName[ii] = CharMapName0[ii];
                    CharMapName0[ii] = _S;
                    _S = CharMapName_[ii];
                    CharMapName_[ii] = CharMapName_0[ii];
                    CharMapName_0[ii] = _S;
                    for (int i = 32; i < 128; i++)
                    {
                        _I = CharMap[ii, i]; CharMap[ii, i] = CharMap0[ii, i]; CharMap0[ii, i] = _I;
                        _I = CharMap_[ii, i]; CharMap_[ii, i] = CharMap_0[ii, i]; CharMap_0[ii, i] = _I;
                    }
                }
                _I = CharMapNumGL; CharMapNumGL = CharMapNumGL0 = CharMapNumGL0 = _I;
                _I = CharMapNumGL_; CharMapNumGL_ = CharMapNumGL_0 = CharMapNumGL_0 = _I;
                _I = CharMapNumGR; CharMapNumGR = CharMapNumGR0 = CharMapNumGR0 = _I;
                _I = CharMapNumGR_; CharMapNumGR_ = CharMapNumGR_0 = CharMapNumGR_0 = _I;

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
            for (int ii = 0; ii < 4; ii++)
            {
                CharMapName_[ii] = CharMapName[ii];
                for (int i = 32; i < 128; i++)
                {
                    CharMap_[ii, i] = CharMap[ii, i];
                }
            }
            CharMapNumGL_ = CharMapNumGL;
            CharMapNumGR_ = CharMapNumGR;
        }

        public void CursorLoad()
        {
            __AnsiX = __AnsiX_;
            __AnsiY = __AnsiY_;
            __AnsiBack = __AnsiBack_;
            __AnsiFore = __AnsiFore_;
            __AnsiAttr = __AnsiAttr_;
            __AnsiWrapFlag = __AnsiWrapFlag_;
            for (int ii = 0; ii < 4; ii++)
            {
                CharMapName[ii] = CharMapName_[ii];
                for (int i = 32; i < 128; i++)
                {
                    CharMap[ii, i] = CharMap_[ii, i];
                }
            }
            CharMapNumGL = CharMapNumGL_;
            CharMapNumGR = CharMapNumGR_;
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

        public void Reset(int AnsiMaxX, int AnsiMaxY, int NormalB, int NormalF, int ANSIDOS)
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

            for (int i = 32; i < 128; i++)
            {
                for (int ii = 0; ii < 4; ii++)
                {
                    CharMap[ii, i] = i;
                    CharMap_[ii, i] = i;
                    CharMap0[ii, i] = i;
                    CharMap_0[ii, i] = i;
                }
            }
            CharMapNumGL = 0;
            CharMapNumGL_ = 0;
            CharMapNumGL0 = 0;
            CharMapNumGL_0 = 0;
            CharMapNumGR = 2;
            CharMapNumGR_ = 2;
            CharMapNumGR0 = 2;
            CharMapNumGR_0 = 2;

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

            CharMapNRCS = false;

            CharMapName[0] = "1B";
            CharMapName[1] = "1B";
            if (ANSIDOS == 2)
            {
                //CharMapName[2] = "1%5";
                //CharMapName[3] = "1%5";
                CharMapName[2] = "1<";
                CharMapName[3] = "1<";
            }
            else
            {
                CharMapName[2] = "2A";
                CharMapName[3] = "2A";
            }

            RefreshCharMaps();
        }

        public void RefreshCharMaps()
        {
            SetCharMap(0, CharMapName[0]);
            SetCharMap(1, CharMapName[1]);
            SetCharMap(2, CharMapName[2]);
            SetCharMap(3, CharMapName[3]);
        }

        public void SetCharMap(int Num, string MapId)
        {
            CharMapName[Num] = MapId;
            int Blank_ = 0x0020;
            Blank_ = 0x2E2E;
            Blank_ = 0xFFFD;

            switch (MapId)
            {
                default:
                    for (int i = 32; i < 128; i++)
                    {
                        CharMap[Num, i] = i;
                    }
                    break;
                case "1<": //H2 DEC Supplemental
                case "1%5": //H3 DEC Supplemental Graphic
                case "1%0": //H5 Turkish (DEC)
                case "1\"?": //H5 Greek (DEC)
                case "1\"4": //H5 Hebrew (DEC)
                case "2A": //H3 ISO Latin-1
                case "2B": //H5 ISO Latin-2 Supplemental
                case "2F": //H5 ISO Greek Supplemental
                case "2H": //H5 ISO Hebrew Supplemental
                case "2L": //F5 ISO Latin-Cyrillic
                case "2M": //H5 ISO Latin-5 Supplemental
                    for (int i = 32; i < 128; i++)
                    {
                        CharMap[Num, i] = i + 128;
                    }

                    switch (MapId)
                    {
                        case "2A": //H3 ISO Latin-1
                        case "2B": //H5 ISO Latin-2 Supplemental
                        case "2F": //H5 ISO Greek Supplemental
                        case "2H": //H5 ISO Hebrew Supplemental
                        case "2L": //F5 ISO Latin-Cyrillic
                        case "2M": //H5 ISO Latin-5 Supplemental
                            break;
                        default:
                            CharMap[Num, 0x24] = Blank_;
                            CharMap[Num, 0x26] = Blank_;
                            CharMap[Num, 0x28] = 0x00A4;
                            CharMap[Num, 0x2C] = Blank_;
                            CharMap[Num, 0x2D] = Blank_;
                            CharMap[Num, 0x2E] = Blank_;
                            CharMap[Num, 0x2F] = Blank_;
                            CharMap[Num, 0x34] = Blank_;
                            CharMap[Num, 0x38] = Blank_;
                            CharMap[Num, 0x3E] = Blank_;
                            CharMap[Num, 0x50] = Blank_;
                            CharMap[Num, 0x57] = 0x0152;
                            CharMap[Num, 0x5E] = Blank_;
                            CharMap[Num, 0x70] = Blank_;
                            CharMap[Num, 0x77] = 0x0153;
                            CharMap[Num, 0x7E] = Blank_;
                            CharMap[Num, 0x7F] = 0x007F;
                            break;
                    }
                    break;
            }

            switch (MapId)
            {
                case "1A": //L1 British
                    CharMap[Num, 0x23] = 0x00A3;
                    break;
                case "10": //L1 DEC Special graphics and line drawing
                case "12": //L1 DEC Alternate character ROM special graphics
                    CharMap[Num, 0x5F] = 0x0020;
                    CharMap[Num, 0x60] = 0x25C6; CharMap[Num, 0x61] = 0x2592; CharMap[Num, 0x62] = 0x2409; CharMap[Num, 0x63] = 0x240C;
                    CharMap[Num, 0x64] = 0x240D; CharMap[Num, 0x65] = 0x240A; CharMap[Num, 0x66] = 0x00B0; CharMap[Num, 0x67] = 0x00B1;
                    CharMap[Num, 0x68] = 0x2424; CharMap[Num, 0x69] = 0x240B; CharMap[Num, 0x6A] = 0x2518; CharMap[Num, 0x6B] = 0x2510;
                    CharMap[Num, 0x6C] = 0x250C; CharMap[Num, 0x6D] = 0x2514; CharMap[Num, 0x6E] = 0x253C; CharMap[Num, 0x6F] = 0x23BA;
                    CharMap[Num, 0x70] = 0x23BB; CharMap[Num, 0x71] = 0x2500; CharMap[Num, 0x72] = 0x23BC; CharMap[Num, 0x73] = 0x23BD;
                    CharMap[Num, 0x74] = 0x251C; CharMap[Num, 0x75] = 0x2524; CharMap[Num, 0x76] = 0x2534; CharMap[Num, 0x77] = 0x252C;
                    CharMap[Num, 0x78] = 0x2502; CharMap[Num, 0x79] = 0x2264; CharMap[Num, 0x7A] = 0x2265; CharMap[Num, 0x7B] = 0x03C0;
                    CharMap[Num, 0x7C] = 0x2260; CharMap[Num, 0x7D] = 0x00A3; CharMap[Num, 0x7E] = 0x00B7;
                    break;

                case "1<": //H2 DEC Supplemental
                    break;
                case "14": //L2 Dutch
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x23] = 0x00A3;
                        CharMap[Num, 0x40] = 0x00BE;
                        CharMap[Num, 0x5B] = 0x0133;
                        CharMap[Num, 0x5C] = 0x00BD;
                        CharMap[Num, 0x5D] = 0x007C;
                        CharMap[Num, 0x7B] = 0x00A8;
                        CharMap[Num, 0x7C] = 0x0192;
                        CharMap[Num, 0x7D] = 0x00BC;
                        CharMap[Num, 0x7E] = 0x00B4;
                    }
                    break;
                case "15": //L2 Finnish
                case "1C": //L2 Finnish
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x5B] = 0x00C4;
                        CharMap[Num, 0x5C] = 0x00D6;
                        CharMap[Num, 0x5D] = 0x00C5;
                        CharMap[Num, 0x5E] = 0x00DC;
                        CharMap[Num, 0x60] = 0x00E9;
                        CharMap[Num, 0x7B] = 0x00E4;
                        CharMap[Num, 0x7C] = 0x00F6;
                        CharMap[Num, 0x7D] = 0x00E5;
                        CharMap[Num, 0x7E] = 0x00FC;
                    }
                    break;
                case "1R": //L2 French
                case "1f": //L2 French
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x23] = 0x00A3;
                        CharMap[Num, 0x40] = 0x00E0;
                        CharMap[Num, 0x5B] = 0x00B0;
                        CharMap[Num, 0x5C] = 0x00E7;
                        CharMap[Num, 0x5D] = 0x00A7;
                        CharMap[Num, 0x7B] = 0x00E9;
                        CharMap[Num, 0x7C] = 0x00F9;
                        CharMap[Num, 0x7D] = 0x00E8;
                        CharMap[Num, 0x7E] = 0x00A8;
                    }
                    break;
                case "19": //L2 French Canadian
                case "1Q": //L2 French Canadian
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x40] = 0x00E0;
                        CharMap[Num, 0x5B] = 0x00E2;
                        CharMap[Num, 0x5C] = 0x00E7;
                        CharMap[Num, 0x5D] = 0x00EA;
                        CharMap[Num, 0x5E] = 0x00EE;
                        CharMap[Num, 0x60] = 0x00F4;
                        CharMap[Num, 0x7B] = 0x00E9;
                        CharMap[Num, 0x7C] = 0x00F9;
                        CharMap[Num, 0x7D] = 0x00E8;
                        CharMap[Num, 0x7E] = 0x00FB;
                    }
                    break;
                case "1K": //L2 German
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x40] = 0x00A7;
                        CharMap[Num, 0x5B] = 0x00C4;
                        CharMap[Num, 0x5C] = 0x00D6;
                        CharMap[Num, 0x5D] = 0x00DC;
                        CharMap[Num, 0x7B] = 0x00E4;
                        CharMap[Num, 0x7C] = 0x00F6;
                        CharMap[Num, 0x7D] = 0x00FC;
                        CharMap[Num, 0x7E] = 0x00DF;
                    }
                    break;
                case "1Y": //L2 Italian
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x23] = 0x00A3;
                        CharMap[Num, 0x40] = 0x00A7;
                        CharMap[Num, 0x5B] = 0x00B0;
                        CharMap[Num, 0x5C] = 0x00E7;
                        CharMap[Num, 0x5D] = 0x00E9;
                        CharMap[Num, 0x60] = 0x00F9;
                        CharMap[Num, 0x7B] = 0x00E0;
                        CharMap[Num, 0x7C] = 0x00F2;
                        CharMap[Num, 0x7D] = 0x00E8;
                        CharMap[Num, 0x7E] = 0x00EC;
                    }
                    break;
                case "1`": //L2 Norwegian/Danish
                case "16": //L2 Norwegian/Danish
                case "1E": //L2 Norwegian/Danish
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x40] = 0x00C4;
                        CharMap[Num, 0x5B] = 0x00C6;
                        CharMap[Num, 0x5C] = 0x00D8;
                        CharMap[Num, 0x5D] = 0x00C5;
                        CharMap[Num, 0x5E] = 0x00DC;
                        CharMap[Num, 0x60] = 0x00E4;
                        CharMap[Num, 0x7B] = 0x00E6;
                        CharMap[Num, 0x7C] = 0x00F8;
                        CharMap[Num, 0x7D] = 0x00E5;
                        CharMap[Num, 0x7E] = 0x00FC;
                    }
                    break;
                case "1Z": //L2 Spanish
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x23] = 0x00A3;
                        CharMap[Num, 0x40] = 0x00A7;
                        CharMap[Num, 0x5B] = 0x00A1;
                        CharMap[Num, 0x5C] = 0x00D1;
                        CharMap[Num, 0x5D] = 0x00BF;
                        CharMap[Num, 0x7B] = 0x00B0;
                        CharMap[Num, 0x7C] = 0x00F1;
                        CharMap[Num, 0x7D] = 0x00E7;
                    }
                    break;
                case "17": //L2 Swedish
                case "1H": //L2 Swedish
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x40] = 0x00C9;
                        CharMap[Num, 0x5B] = 0x00C4;
                        CharMap[Num, 0x5C] = 0x00D6;
                        CharMap[Num, 0x5D] = 0x00C5;
                        CharMap[Num, 0x5E] = 0x00DC;
                        CharMap[Num, 0x60] = 0x00E9;
                        CharMap[Num, 0x7B] = 0x00E4;
                        CharMap[Num, 0x7C] = 0x00F6;
                        CharMap[Num, 0x7D] = 0x00E5;
                        CharMap[Num, 0x7E] = 0x00FC;
                    }
                    break;
                case "1=": //L2 Swiss
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x23] = 0x00F9;
                        CharMap[Num, 0x40] = 0x00E0;
                        CharMap[Num, 0x5B] = 0x00E9;
                        CharMap[Num, 0x5C] = 0x00E7;
                        CharMap[Num, 0x5D] = 0x00EA;
                        CharMap[Num, 0x5E] = 0x00EE;
                        CharMap[Num, 0x5F] = 0x00E8;
                        CharMap[Num, 0x60] = 0x00F4;
                        CharMap[Num, 0x7B] = 0x00E4;
                        CharMap[Num, 0x7C] = 0x00F6;
                        CharMap[Num, 0x7D] = 0x00FC;
                        CharMap[Num, 0x7E] = 0x00FB;
                    }
                    break;

                case "1%5": //H3 DEC Supplemental Graphic
                    break;
                case "1>": //F3 DEC Technical
                    CharMap[Num, 0x20] = 0x0020; CharMap[Num, 0x21] = 0x23B7; CharMap[Num, 0x22] = 0x250C; CharMap[Num, 0x23] = 0x2500; CharMap[Num, 0x24] = 0x2320; CharMap[Num, 0x25] = 0x2321; CharMap[Num, 0x26] = 0x2502; CharMap[Num, 0x27] = 0x23A1;
                    //arMap[Num, 0x28] = 0x23A3; CharMap[Num, 0x29] = 0x23A4; CharMap[Num, 0x2A] = 0x23A6; CharMap[Num, 0x2B] = 0x239B; CharMap[Num, 0x2C] = 0x239D; CharMap[Num, 0x2D] = 0x239E; CharMap[Num, 0x2E] = 0x23A0; CharMap[Num, 0x2F] = 0x23A8;
                    CharMap[Num, 0x28] = 0x23A3; CharMap[Num, 0x29] = 0x23A4; CharMap[Num, 0x2A] = 0x23A6; CharMap[Num, 0x2B] = 0x23A7; CharMap[Num, 0x2C] = 0x23A9; CharMap[Num, 0x2D] = 0x23AB; CharMap[Num, 0x2E] = 0x23AD; CharMap[Num, 0x2F] = 0x23A8;
                    CharMap[Num, 0x30] = 0x23AC; CharMap[Num, 0x31] = 0x23B2; CharMap[Num, 0x32] = 0x23B3; CharMap[Num, 0x33] = 0x2572; CharMap[Num, 0x34] = 0x2571; CharMap[Num, 0x35] = 0x231D; CharMap[Num, 0x36] = 0x231F; CharMap[Num, 0x37] = 0x27E9;
                    CharMap[Num, 0x38] = 0x2E2E; CharMap[Num, 0x39] = Blank_; CharMap[Num, 0x3A] = Blank_; CharMap[Num, 0x3B] = Blank_; CharMap[Num, 0x3C] = 0x2264; CharMap[Num, 0x3D] = 0x2260; CharMap[Num, 0x3E] = 0x2265; CharMap[Num, 0x3F] = 0x222B;
                    CharMap[Num, 0x40] = 0x2234; CharMap[Num, 0x41] = 0x221D; CharMap[Num, 0x42] = 0x221E; CharMap[Num, 0x43] = 0x00F7; CharMap[Num, 0x44] = 0x0394; CharMap[Num, 0x45] = 0x2207; CharMap[Num, 0x46] = 0x03A6; CharMap[Num, 0x47] = 0x0393;
                    CharMap[Num, 0x48] = 0x223C; CharMap[Num, 0x49] = 0x2243; CharMap[Num, 0x4A] = 0x0398; CharMap[Num, 0x4B] = 0x00D7; CharMap[Num, 0x4C] = 0x039B; CharMap[Num, 0x4D] = 0x21D4; CharMap[Num, 0x4E] = 0x21D2; CharMap[Num, 0x4F] = 0x2261;
                    CharMap[Num, 0x50] = 0x03A0; CharMap[Num, 0x51] = 0x03A8; CharMap[Num, 0x52] = Blank_; CharMap[Num, 0x53] = 0x03A3; CharMap[Num, 0x54] = Blank_; CharMap[Num, 0x55] = Blank_; CharMap[Num, 0x56] = 0x221A; CharMap[Num, 0x57] = 0x03A9;
                    CharMap[Num, 0x58] = 0x039E; CharMap[Num, 0x59] = 0x03A5; CharMap[Num, 0x5A] = 0x2282; CharMap[Num, 0x5B] = 0x2283; CharMap[Num, 0x5C] = 0x2229; CharMap[Num, 0x5D] = 0x222A; CharMap[Num, 0x5E] = 0x2227; CharMap[Num, 0x5F] = 0x2228;
                    CharMap[Num, 0x60] = 0x00AC; CharMap[Num, 0x61] = 0x03B1; CharMap[Num, 0x62] = 0x03B2; CharMap[Num, 0x63] = 0x03C7; CharMap[Num, 0x64] = 0x03B4; CharMap[Num, 0x65] = 0x03B5; CharMap[Num, 0x66] = 0x03C6; CharMap[Num, 0x67] = 0x03B3;
                    CharMap[Num, 0x68] = 0x03B7; CharMap[Num, 0x69] = 0x03B9; CharMap[Num, 0x6A] = 0x03B8; CharMap[Num, 0x6B] = 0x03BA; CharMap[Num, 0x6C] = 0x03BB; CharMap[Num, 0x6D] = Blank_; CharMap[Num, 0x6E] = 0x03BD; CharMap[Num, 0x6F] = 0x2202;
                    CharMap[Num, 0x70] = 0x03C0; CharMap[Num, 0x71] = 0x03C8; CharMap[Num, 0x72] = 0x03C1; CharMap[Num, 0x73] = 0x03C3; CharMap[Num, 0x74] = 0x03C4; CharMap[Num, 0x75] = 0x2E2E; CharMap[Num, 0x76] = 0x0192; CharMap[Num, 0x77] = 0x03C9;
                    CharMap[Num, 0x78] = 0x03BE; CharMap[Num, 0x79] = 0x03C5; CharMap[Num, 0x7A] = 0x03B6; CharMap[Num, 0x7B] = 0x2190; CharMap[Num, 0x7C] = 0x2191; CharMap[Num, 0x7D] = 0x2192; CharMap[Num, 0x7E] = 0x2193; CharMap[Num, 0x7F] = 0x007F;



                    break;
                case "1%6": //L3 Portuguese
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x5B] = 0x00C3; 
                        CharMap[Num, 0x5C] = 0x00C7;
                        CharMap[Num, 0x5D] = 0x00D5;
                        CharMap[Num, 0x7B] = 0x00E3;
                        CharMap[Num, 0x7C] = 0x00E7;
                        CharMap[Num, 0x7D] = 0x00F5;
                    }
                    break;

                case "1%=": //L5 Hebrew
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x60] = 0x05D0; CharMap[Num, 0x61] = 0x05D1; CharMap[Num, 0x62] = 0x05D2; CharMap[Num, 0x63] = 0x05D3;
                        CharMap[Num, 0x64] = 0x05D4; CharMap[Num, 0x65] = 0x05D5; CharMap[Num, 0x66] = 0x05D6; CharMap[Num, 0x67] = 0x05D7;
                        CharMap[Num, 0x68] = 0x05D8; CharMap[Num, 0x69] = 0x05D9; CharMap[Num, 0x6A] = 0x05DA; CharMap[Num, 0x6B] = 0x05DB;
                        CharMap[Num, 0x6C] = 0x05DC; CharMap[Num, 0x6D] = 0x05DD; CharMap[Num, 0x6E] = 0x05DE; CharMap[Num, 0x6F] = 0x05DF;
                        CharMap[Num, 0x70] = 0x05E0; CharMap[Num, 0x71] = 0x05E1; CharMap[Num, 0x72] = 0x05E2; CharMap[Num, 0x73] = 0x05E3;
                        CharMap[Num, 0x74] = 0x05E4; CharMap[Num, 0x75] = 0x05E5; CharMap[Num, 0x76] = 0x05E6; CharMap[Num, 0x77] = 0x05E7;
                        CharMap[Num, 0x78] = 0x05E8; CharMap[Num, 0x79] = 0x05E9; CharMap[Num, 0x7A] = 0x05EA;
                    }
                    break;
                case "1\"4": //H5 Hebrew (DEC)
                    for (int i = 0x40; i <= 0x5F; i++)
                    {
                        CharMap[Num, i] = Blank_;
                    }
                    CharMap[Num, 0x60] = 0x05D0; CharMap[Num, 0x61] = 0x05D1; CharMap[Num, 0x62] = 0x05D2; CharMap[Num, 0x63] = 0x05D3;
                    CharMap[Num, 0x64] = 0x05D4; CharMap[Num, 0x65] = 0x05D5; CharMap[Num, 0x66] = 0x05D6; CharMap[Num, 0x67] = 0x05D7;
                    CharMap[Num, 0x68] = 0x05D8; CharMap[Num, 0x69] = 0x05D9; CharMap[Num, 0x6A] = 0x05DA; CharMap[Num, 0x6B] = 0x05DB;
                    CharMap[Num, 0x6C] = 0x05DC; CharMap[Num, 0x6D] = 0x05DD; CharMap[Num, 0x6E] = 0x05DE; CharMap[Num, 0x6F] = 0x05DF;
                    CharMap[Num, 0x70] = 0x05E0; CharMap[Num, 0x71] = 0x05E1; CharMap[Num, 0x72] = 0x05E2; CharMap[Num, 0x73] = 0x05E3;
                    CharMap[Num, 0x74] = 0x05E4; CharMap[Num, 0x75] = 0x05E5; CharMap[Num, 0x76] = 0x05E6; CharMap[Num, 0x77] = 0x05E7;
                    CharMap[Num, 0x78] = 0x05E8; CharMap[Num, 0x79] = 0x05E9; CharMap[Num, 0x7A] = 0x05EA; CharMap[Num, 0x7B] = Blank_;
                    CharMap[Num, 0x7C] = Blank_; CharMap[Num, 0x7D] = Blank_; CharMap[Num, 0x7E] = Blank_; CharMap[Num, 0x7F] = 0x007F;
                    break;
                case "1&4": //F5 Cyrillic (DEC)
                    for (int i = 0x21; i <= 0x3F; i++)
                    {
                        CharMap[Num, i] = Blank_;
                    }
                    CharMap[Num, 0x40] = 0x044E; CharMap[Num, 0x41] = 0x0430; CharMap[Num, 0x42] = 0x0431; CharMap[Num, 0x43] = 0x0446; CharMap[Num, 0x44] = 0x0434; CharMap[Num, 0x45] = 0x0435; CharMap[Num, 0x46] = 0x0444; CharMap[Num, 0x47] = 0x0433;
                    CharMap[Num, 0x48] = 0x0445; CharMap[Num, 0x49] = 0x0438; CharMap[Num, 0x4A] = 0x0439; CharMap[Num, 0x4B] = 0x043A; CharMap[Num, 0x4C] = 0x043B; CharMap[Num, 0x4D] = 0x043C; CharMap[Num, 0x4E] = 0x043D; CharMap[Num, 0x4F] = 0x043E;
                    CharMap[Num, 0x50] = 0x043F; CharMap[Num, 0x51] = 0x044F; CharMap[Num, 0x52] = 0x0440; CharMap[Num, 0x53] = 0x0441; CharMap[Num, 0x54] = 0x0442; CharMap[Num, 0x55] = 0x0443; CharMap[Num, 0x56] = 0x0436; CharMap[Num, 0x57] = 0x0432;
                    CharMap[Num, 0x58] = 0x044C; CharMap[Num, 0x59] = 0x044B; CharMap[Num, 0x5A] = 0x0437; CharMap[Num, 0x5B] = 0x0448; CharMap[Num, 0x5C] = 0x044D; CharMap[Num, 0x5D] = 0x0449; CharMap[Num, 0x5E] = 0x0447; CharMap[Num, 0x5F] = 0x044A;
                    CharMap[Num, 0x60] = 0x042E; CharMap[Num, 0x61] = 0x0410; CharMap[Num, 0x62] = 0x0411; CharMap[Num, 0x63] = 0x0426; CharMap[Num, 0x64] = 0x0414; CharMap[Num, 0x65] = 0x0415; CharMap[Num, 0x66] = 0x0424; CharMap[Num, 0x67] = 0x0413;
                    CharMap[Num, 0x68] = 0x0425; CharMap[Num, 0x69] = 0x0418; CharMap[Num, 0x6A] = 0x0419; CharMap[Num, 0x6B] = 0x041A; CharMap[Num, 0x6C] = 0x041B; CharMap[Num, 0x6D] = 0x041C; CharMap[Num, 0x6E] = 0x041D; CharMap[Num, 0x6F] = 0x041E;
                    CharMap[Num, 0x70] = 0x041F; CharMap[Num, 0x71] = 0x042F; CharMap[Num, 0x72] = 0x0420; CharMap[Num, 0x73] = 0x0421; CharMap[Num, 0x74] = 0x0422; CharMap[Num, 0x75] = 0x0423; CharMap[Num, 0x76] = 0x0416; CharMap[Num, 0x77] = 0x0412;
                    CharMap[Num, 0x78] = 0x042C; CharMap[Num, 0x79] = 0x042B; CharMap[Num, 0x7A] = 0x0417; CharMap[Num, 0x7B] = 0x0428; CharMap[Num, 0x7C] = 0x042D; CharMap[Num, 0x7D] = 0x0429; CharMap[Num, 0x7E] = 0x0427;
                    break;
                case "1\">": //L5 Greek
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x61] = 0x0391;
                        CharMap[Num, 0x62] = 0x0392;
                        CharMap[Num, 0x63] = 0x0393;
                        CharMap[Num, 0x64] = 0x0394;
                        CharMap[Num, 0x65] = 0x0395;
                        CharMap[Num, 0x66] = 0x0396;
                        CharMap[Num, 0x67] = 0x0397;
                        CharMap[Num, 0x68] = 0x0398;
                        CharMap[Num, 0x69] = 0x0399;
                        CharMap[Num, 0x6A] = 0x039A;
                        CharMap[Num, 0x6B] = 0x039B;
                        CharMap[Num, 0x6C] = 0x039C;
                        CharMap[Num, 0x6D] = 0x039D;
                        CharMap[Num, 0x6E] = 0x039E;
                        CharMap[Num, 0x6F] = 0x039F;
                        CharMap[Num, 0x70] = 0x03A0;
                        CharMap[Num, 0x71] = 0x03A1;
                        CharMap[Num, 0x72] = 0x03A3;
                        CharMap[Num, 0x73] = 0x03A4;
                        CharMap[Num, 0x74] = 0x03A5;
                        CharMap[Num, 0x75] = 0x03A6;
                        CharMap[Num, 0x76] = 0x03A7;
                        CharMap[Num, 0x77] = 0x03A8;
                        CharMap[Num, 0x78] = 0x03A9;
                        CharMap[Num, 0x79] = 0x0020;
                        CharMap[Num, 0x7A] = 0x0020;
                    }
                    break;
                case "1\"?": //H5 Greek (DEC)
                    {
                        CharMap[Num, 0x40] = 0x03CA; CharMap[Num, 0x41] = 0x0391; CharMap[Num, 0x42] = 0x0392; CharMap[Num, 0x43] = 0x0393; CharMap[Num, 0x44] = 0x0394; CharMap[Num, 0x45] = 0x0395; CharMap[Num, 0x46] = 0x0396; CharMap[Num, 0x47] = 0x0397;
                        CharMap[Num, 0x48] = 0x0398; CharMap[Num, 0x49] = 0x0399; CharMap[Num, 0x4A] = 0x039A; CharMap[Num, 0x4B] = 0x039B; CharMap[Num, 0x4C] = 0x039C; CharMap[Num, 0x4D] = 0x039D; CharMap[Num, 0x4E] = 0x039E; CharMap[Num, 0x4F] = 0x039F;
                        CharMap[Num, 0x50] = Blank_; CharMap[Num, 0x51] = 0x03A0; CharMap[Num, 0x52] = 0x03A1; CharMap[Num, 0x53] = 0x03A3; CharMap[Num, 0x54] = 0x03A4; CharMap[Num, 0x55] = 0x03A5; CharMap[Num, 0x56] = 0x03A6; CharMap[Num, 0x57] = 0x03A7;
                        CharMap[Num, 0x58] = 0x03A8; CharMap[Num, 0x59] = 0x03A9; CharMap[Num, 0x5A] = 0x03AC; CharMap[Num, 0x5B] = 0x03AD; CharMap[Num, 0x5C] = 0x03AE; CharMap[Num, 0x5D] = 0x03AF; CharMap[Num, 0x5E] = Blank_; CharMap[Num, 0x5F] = 0x03CC;
                        CharMap[Num, 0x60] = 0x03CB; CharMap[Num, 0x61] = 0x03B1; CharMap[Num, 0x62] = 0x03B2; CharMap[Num, 0x63] = 0x03B3; CharMap[Num, 0x64] = 0x03B4; CharMap[Num, 0x65] = 0x03B5; CharMap[Num, 0x66] = 0x03B6; CharMap[Num, 0x67] = 0x03B7;
                        CharMap[Num, 0x68] = 0x03B8; CharMap[Num, 0x69] = 0x03B9; CharMap[Num, 0x6A] = 0x03BA; CharMap[Num, 0x6B] = 0x03BB; CharMap[Num, 0x6C] = 0x03BC; CharMap[Num, 0x6D] = 0x03BD; CharMap[Num, 0x6E] = 0x03BE; CharMap[Num, 0x6F] = 0x03BF;
                        CharMap[Num, 0x70] = Blank_; CharMap[Num, 0x71] = 0x03C0; CharMap[Num, 0x72] = 0x03C1; CharMap[Num, 0x73] = 0x03C3; CharMap[Num, 0x74] = 0x03C4; CharMap[Num, 0x75] = 0x03C5; CharMap[Num, 0x76] = 0x03C6; CharMap[Num, 0x77] = 0x03C7;
                        CharMap[Num, 0x78] = 0x03C8; CharMap[Num, 0x79] = 0x03C9; CharMap[Num, 0x7A] = 0x03C2; CharMap[Num, 0x7B] = 0x03CD; CharMap[Num, 0x7C] = 0x03CE; CharMap[Num, 0x7D] = 0x0384; CharMap[Num, 0x7E] = Blank_;
                    }
                    break;
                case "1%2": //L5 Turkish
                    if (CharMapNRCS)
                    {
                        CharMap[Num, 0x26] = 0x011F;
                        CharMap[Num, 0x40] = 0x0130;
                        CharMap[Num, 0x5B] = 0x015E;
                        CharMap[Num, 0x5C] = 0x00D6;
                        CharMap[Num, 0x5D] = 0x00C7;
                        CharMap[Num, 0x5E] = 0x00DC;
                        CharMap[Num, 0x60] = 0x011E;
                        CharMap[Num, 0x7B] = 0x015F;
                        CharMap[Num, 0x7C] = 0x00F6;
                        CharMap[Num, 0x7D] = 0x00E7;
                        CharMap[Num, 0x7E] = 0x00FC;
                    }
                    break;
                case "1%0": //H5 Turkish (DEC)
                    CharMap[Num, 0x2E] = 0x0130;
                    CharMap[Num, 0x3E] = 0x0131;
                    CharMap[Num, 0x50] = 0x011E; CharMap[Num, 0x57] = 0x0152;
                    CharMap[Num, 0x5D] = 0x0178; CharMap[Num, 0x5E] = 0x015E;
                    CharMap[Num, 0x70] = 0x011F; CharMap[Num, 0x77] = 0x0153;
                    CharMap[Num, 0x7D] = 0x00FF; CharMap[Num, 0x7E] = 0x015F;
                    break;

                case "2H": //H5 ISO Hebrew Supplemental
                    CharMap[Num, 0x21] = Blank_;
                    CharMap[Num, 0x2A] = 0x00D7;
                    CharMap[Num, 0x3A] = 0x00F7; CharMap[Num, 0x3F] = Blank_;
                    for (int i = 0x40; i <= 0x5F; i++)
                    {
                        CharMap[Num, i] = Blank_;
                    }
                    CharMap[Num, 0x5F] = 0x2017;
                    CharMap[Num, 0x60] = 0x05D0; CharMap[Num, 0x61] = 0x05D1; CharMap[Num, 0x62] = 0x05D2; CharMap[Num, 0x63] = 0x05D3;
                    CharMap[Num, 0x64] = 0x05D4; CharMap[Num, 0x65] = 0x05D5; CharMap[Num, 0x66] = 0x05D6; CharMap[Num, 0x67] = 0x05D7;
                    CharMap[Num, 0x68] = 0x05D8; CharMap[Num, 0x69] = 0x05D9; CharMap[Num, 0x6A] = 0x05DA; CharMap[Num, 0x6B] = 0x05DB;
                    CharMap[Num, 0x6C] = 0x05DC; CharMap[Num, 0x6D] = 0x05DD; CharMap[Num, 0x6E] = 0x05DE; CharMap[Num, 0x6F] = 0x05DF;
                    CharMap[Num, 0x70] = 0x05E0; CharMap[Num, 0x71] = 0x05E1; CharMap[Num, 0x72] = 0x05E2; CharMap[Num, 0x73] = 0x05E3;
                    CharMap[Num, 0x74] = 0x05E4; CharMap[Num, 0x75] = 0x05E5; CharMap[Num, 0x76] = 0x05E6; CharMap[Num, 0x77] = 0x05E7;
                    CharMap[Num, 0x78] = 0x05E8; CharMap[Num, 0x79] = 0x05E9; CharMap[Num, 0x7A] = 0x05EA; CharMap[Num, 0x7B] = Blank_;
                    CharMap[Num, 0x7C] = Blank_; CharMap[Num, 0x7D] = 0x0020; CharMap[Num, 0x7E] = 0x0020; CharMap[Num, 0x7F] = Blank_;
                    break;
                case "2F": //H5 ISO Greek Supplemental
                    CharMap[Num, 0x21] = 0x2018; CharMap[Num, 0x22] = 0x2019; CharMap[Num, 0x24] = 0x20AC; CharMap[Num, 0x25] = 0x20AF;
                    CharMap[Num, 0x2A] = 0x037A; CharMap[Num, 0x2E] = Blank_; CharMap[Num, 0x2F] = 0x2015;
                    CharMap[Num, 0x34] = 0x0384; CharMap[Num, 0x35] = 0x0385; CharMap[Num, 0x36] = 0x0386;
                    CharMap[Num, 0x38] = 0x0388; CharMap[Num, 0x39] = 0x0389; CharMap[Num, 0x3A] = 0x038A; CharMap[Num, 0x3C] = 0x038C; CharMap[Num, 0x3E] = 0x038E; CharMap[Num, 0x3F] = 0x038F;
                    CharMap[Num, 0x40] = 0x0390; CharMap[Num, 0x41] = 0x0391; CharMap[Num, 0x42] = 0x0392; CharMap[Num, 0x43] = 0x0393; CharMap[Num, 0x44] = 0x0394; CharMap[Num, 0x45] = 0x0395; CharMap[Num, 0x46] = 0x0396; CharMap[Num, 0x47] = 0x0397;
                    CharMap[Num, 0x48] = 0x0398; CharMap[Num, 0x49] = 0x0399; CharMap[Num, 0x4A] = 0x039A; CharMap[Num, 0x4B] = 0x039B; CharMap[Num, 0x4C] = 0x039C; CharMap[Num, 0x4D] = 0x039D; CharMap[Num, 0x4E] = 0x039E; CharMap[Num, 0x4F] = 0x039F;
                    CharMap[Num, 0x50] = 0x03A0; CharMap[Num, 0x51] = 0x03A1; CharMap[Num, 0x52] = Blank_; CharMap[Num, 0x53] = 0x03A3; CharMap[Num, 0x54] = 0x03A4; CharMap[Num, 0x55] = 0x03A5; CharMap[Num, 0x56] = 0x03A6; CharMap[Num, 0x57] = 0x03A7;
                    CharMap[Num, 0x58] = 0x03A8; CharMap[Num, 0x59] = 0x03A9; CharMap[Num, 0x5A] = 0x03AA; CharMap[Num, 0x5B] = 0x03AB; CharMap[Num, 0x5C] = 0x03AC; CharMap[Num, 0x5D] = 0x03AD; CharMap[Num, 0x5E] = 0x03AE; CharMap[Num, 0x5F] = 0x03AF;
                    CharMap[Num, 0x60] = 0x03B0; CharMap[Num, 0x61] = 0x03B1; CharMap[Num, 0x62] = 0x03B2; CharMap[Num, 0x63] = 0x03B3; CharMap[Num, 0x64] = 0x03B4; CharMap[Num, 0x65] = 0x03B5; CharMap[Num, 0x66] = 0x03B6; CharMap[Num, 0x67] = 0x03B7;
                    CharMap[Num, 0x68] = 0x03B8; CharMap[Num, 0x69] = 0x03B9; CharMap[Num, 0x6A] = 0x03BA; CharMap[Num, 0x6B] = 0x03BB; CharMap[Num, 0x6C] = 0x03BC; CharMap[Num, 0x6D] = 0x03BD; CharMap[Num, 0x6E] = 0x03BE; CharMap[Num, 0x6F] = 0x03BF;
                    CharMap[Num, 0x70] = 0x03C0; CharMap[Num, 0x71] = 0x03C1; CharMap[Num, 0x72] = 0x03C2; CharMap[Num, 0x73] = 0x03C3; CharMap[Num, 0x74] = 0x03C4; CharMap[Num, 0x75] = 0x03C5; CharMap[Num, 0x76] = 0x03C6; CharMap[Num, 0x77] = 0x03C7;
                    CharMap[Num, 0x78] = 0x03C8; CharMap[Num, 0x79] = 0x03C9; CharMap[Num, 0x7A] = 0x03CA; CharMap[Num, 0x7B] = 0x03CB; CharMap[Num, 0x7C] = 0x03CC; CharMap[Num, 0x7D] = 0x03CD; CharMap[Num, 0x7E] = 0x03CE; CharMap[Num, 0x7F] = Blank_;
                    break;
                case "2L": //F5 ISO Latin-Cyrillic
                    CharMap[Num, 0x20] = 0x0020; CharMap[Num, 0x21] = 0x0401; CharMap[Num, 0x22] = 0x0402; CharMap[Num, 0x23] = 0x0403; CharMap[Num, 0x24] = 0x0404; CharMap[Num, 0x25] = 0x0405; CharMap[Num, 0x26] = 0x0406; CharMap[Num, 0x27] = 0x0407;
                    CharMap[Num, 0x28] = 0x0408; CharMap[Num, 0x29] = 0x0409; CharMap[Num, 0x2A] = 0x040A; CharMap[Num, 0x2B] = 0x040B; CharMap[Num, 0x2C] = 0x040C; CharMap[Num, 0x2D] = 0x00AD; CharMap[Num, 0x2E] = 0x040E; CharMap[Num, 0x2F] = 0x040F;
                    CharMap[Num, 0x30] = 0x0410; CharMap[Num, 0x31] = 0x0411; CharMap[Num, 0x32] = 0x0412; CharMap[Num, 0x33] = 0x0413; CharMap[Num, 0x34] = 0x0414; CharMap[Num, 0x35] = 0x0415; CharMap[Num, 0x36] = 0x0416; CharMap[Num, 0x37] = 0x0417;
                    CharMap[Num, 0x38] = 0x0418; CharMap[Num, 0x39] = 0x0419; CharMap[Num, 0x3A] = 0x041A; CharMap[Num, 0x3B] = 0x041B; CharMap[Num, 0x3C] = 0x041C; CharMap[Num, 0x3D] = 0x041D; CharMap[Num, 0x3E] = 0x041E; CharMap[Num, 0x3F] = 0x041F;
                    CharMap[Num, 0x40] = 0x0420; CharMap[Num, 0x41] = 0x0421; CharMap[Num, 0x42] = 0x0422; CharMap[Num, 0x43] = 0x0423; CharMap[Num, 0x44] = 0x0424; CharMap[Num, 0x45] = 0x0425; CharMap[Num, 0x46] = 0x0426; CharMap[Num, 0x47] = 0x0427;
                    CharMap[Num, 0x48] = 0x0428; CharMap[Num, 0x49] = 0x0429; CharMap[Num, 0x4A] = 0x042A; CharMap[Num, 0x4B] = 0x042B; CharMap[Num, 0x4C] = 0x042C; CharMap[Num, 0x4D] = 0x042D; CharMap[Num, 0x4E] = 0x042E; CharMap[Num, 0x4F] = 0x042F;
                    CharMap[Num, 0x50] = 0x0430; CharMap[Num, 0x51] = 0x0431; CharMap[Num, 0x52] = 0x0432; CharMap[Num, 0x53] = 0x0433; CharMap[Num, 0x54] = 0x0434; CharMap[Num, 0x55] = 0x0435; CharMap[Num, 0x56] = 0x0436; CharMap[Num, 0x57] = 0x0437;
                    CharMap[Num, 0x58] = 0x0438; CharMap[Num, 0x59] = 0x0439; CharMap[Num, 0x5A] = 0x043A; CharMap[Num, 0x5B] = 0x043B; CharMap[Num, 0x5C] = 0x043C; CharMap[Num, 0x5D] = 0x043D; CharMap[Num, 0x5E] = 0x043E; CharMap[Num, 0x5F] = 0x043F;
                    CharMap[Num, 0x60] = 0x0440; CharMap[Num, 0x61] = 0x0441; CharMap[Num, 0x62] = 0x0442; CharMap[Num, 0x63] = 0x0443; CharMap[Num, 0x64] = 0x0444; CharMap[Num, 0x65] = 0x0445; CharMap[Num, 0x66] = 0x0446; CharMap[Num, 0x67] = 0x0447;
                    CharMap[Num, 0x68] = 0x0448; CharMap[Num, 0x69] = 0x0449; CharMap[Num, 0x6A] = 0x044A; CharMap[Num, 0x6B] = 0x044B; CharMap[Num, 0x6C] = 0x044C; CharMap[Num, 0x6D] = 0x044D; CharMap[Num, 0x6E] = 0x044E; CharMap[Num, 0x6F] = 0x044F;
                    CharMap[Num, 0x70] = 0x2116; CharMap[Num, 0x71] = 0x0451; CharMap[Num, 0x72] = 0x0452; CharMap[Num, 0x73] = 0x0453; CharMap[Num, 0x74] = 0x0454; CharMap[Num, 0x75] = 0x0455; CharMap[Num, 0x76] = 0x0456; CharMap[Num, 0x77] = 0x0457;
                    CharMap[Num, 0x78] = 0x0458; CharMap[Num, 0x79] = 0x0459; CharMap[Num, 0x7A] = 0x045A; CharMap[Num, 0x7B] = 0x045B; CharMap[Num, 0x7C] = 0x045C; CharMap[Num, 0x7D] = 0x00A7; CharMap[Num, 0x7E] = 0x045E; CharMap[Num, 0x7F] = 0x045F;
                    break;
                case "2M": //H5 ISO Latin-5 Supplemental
                    CharMap[Num, 0x50] = 0x011E;
                    CharMap[Num, 0x5D] = 0x0130;
                    CharMap[Num, 0x5E] = 0x015E;
                    CharMap[Num, 0x70] = 0x011F;
                    CharMap[Num, 0x7D] = 0x0131;
                    CharMap[Num, 0x7E] = 0x015F;
                    break;
                case "2B": //H5 ISO Latin-2 Supplemental
                    CharMap[Num, 0x20] = 0x0020; CharMap[Num, 0x21] = 0x0104; CharMap[Num, 0x22] = 0x02D8; CharMap[Num, 0x23] = 0x0141; CharMap[Num, 0x25] = 0x013D; CharMap[Num, 0x26] = 0x015A;
                    CharMap[Num, 0x29] = 0x0160; CharMap[Num, 0x2A] = 0x015E; CharMap[Num, 0x2B] = 0x0164; CharMap[Num, 0x2C] = 0x0179; CharMap[Num, 0x2E] = 0x017D; CharMap[Num, 0x2F] = 0x017B;
                    CharMap[Num, 0x31] = 0x0105; CharMap[Num, 0x32] = 0x02DB; CharMap[Num, 0x33] = 0x0142; CharMap[Num, 0x35] = 0x013E; CharMap[Num, 0x36] = 0x015B; CharMap[Num, 0x37] = 0x02C7;
                    CharMap[Num, 0x39] = 0x0161; CharMap[Num, 0x3A] = 0x015F; CharMap[Num, 0x3B] = 0x0165; CharMap[Num, 0x3C] = 0x017A; CharMap[Num, 0x3D] = 0x02DD; CharMap[Num, 0x3E] = 0x017E; CharMap[Num, 0x3F] = 0x017C;
                    CharMap[Num, 0x40] = 0x0154; CharMap[Num, 0x43] = 0x0102; CharMap[Num, 0x45] = 0x0139; CharMap[Num, 0x46] = 0x0106;
                    CharMap[Num, 0x48] = 0x010C; CharMap[Num, 0x4A] = 0x0118; CharMap[Num, 0x4C] = 0x011A; CharMap[Num, 0x4F] = 0x010E;
                    CharMap[Num, 0x50] = 0x0110; CharMap[Num, 0x51] = 0x0143; CharMap[Num, 0x52] = 0x0147; CharMap[Num, 0x55] = 0x0150;
                    CharMap[Num, 0x58] = 0x0158; CharMap[Num, 0x59] = 0x016E; CharMap[Num, 0x5B] = 0x0170; CharMap[Num, 0x5E] = 0x0162;
                    CharMap[Num, 0x60] = 0x0155; CharMap[Num, 0x63] = 0x0103; CharMap[Num, 0x65] = 0x013A; CharMap[Num, 0x66] = 0x0107;
                    CharMap[Num, 0x68] = 0x010D; CharMap[Num, 0x6A] = 0x0119; CharMap[Num, 0x6C] = 0x011B; CharMap[Num, 0x6F] = 0x010F;
                    CharMap[Num, 0x70] = 0x0111; CharMap[Num, 0x71] = 0x0144; CharMap[Num, 0x72] = 0x0148; CharMap[Num, 0x75] = 0x0151;
                    CharMap[Num, 0x78] = 0x0159; CharMap[Num, 0x79] = 0x016F; CharMap[Num, 0x7B] = 0x0171; CharMap[Num, 0x7E] = 0x0163; CharMap[Num, 0x7F] = 0x02D9;
                    break;

                case "1B": //L1 US ASCII
                case "11": //L1 DEC Alternate character ROM standard characters
                case "1%3": //L5 SCS
                case "1&5": //L5 Russian
                case "2A": //H3 ISO Latin-1
                    break;
            }
        }

        public string GetScreen(int X1, int Y1, int X2, int Y2)
        {
            if (X1 > X2)
            {
                return GetScreen(X2, Y1, X1, Y2);
            }
            if (Y1 > Y2)
            {
                return GetScreen(X1, Y2, X2, Y1);
            }

            List<List<int>> ScreenLine = new List<List<int>>();
            for (int Y = Y1; Y <= Y2; Y++)
            {
                List<int> ScreenBuf = new List<int>();
                for (int X = X1; X <= X2; X++)
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
                if (i > 0)
                {
                    Sb.AppendLine();
                }
                Sb.Append(TextWork.IntToStr(ScreenLine[i]));
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

            for (int i = 32; i < 128; i++)
            {
                for (int ii = 0; ii < 4; ii++)
                {
                    Dst.CharMap[ii, i] = Src.CharMap[ii, i];
                    Dst.CharMap_[ii, i] = Src.CharMap_[ii, i];
                    Dst.CharMap0[ii, i] = Src.CharMap0[ii, i];
                    Dst.CharMap_0[ii, i] = Src.CharMap_0[ii, i];
                }
            }
            Dst.CharMapNumGL = Src.CharMapNumGL;
            Dst.CharMapNumGL_ = Src.CharMapNumGL_;
            Dst.CharMapNumGL0 = Src.CharMapNumGL0;
            Dst.CharMapNumGL_0 = Src.CharMapNumGL_0;
            Dst.CharMapNumGR = Src.CharMapNumGR;
            Dst.CharMapNumGR_ = Src.CharMapNumGR_;
            Dst.CharMapNumGR0 = Src.CharMapNumGR0;
            Dst.CharMapNumGR_0 = Src.CharMapNumGR_0;
            Dst.CharMapNRCS = Src.CharMapNRCS;

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
