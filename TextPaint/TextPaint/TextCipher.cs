using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TextPaint
{
    public class TextCipher
    {
        public bool CipherEnabled = false;

        List<int> CipherChar1;
        List<int> CipherChar2;
        Core Core_;
        List<int> CipherAlphabet = new List<int>();
        int CipherMode = 0;

        public string CipherConfPassword = "";

        public TextCipher(ConfigFile CF, Core Core__)
        {
            Core_ = Core__;
            string[] Buf1 = CF.ParamGetS("CipherBegin").Split(',');
            string[] Buf2 = CF.ParamGetS("CipherEnd").Split(',');
            CipherAlphabet = TextWork.StrToInt(CF.ParamGetS("CipherAlphabet"));
            CipherMode = CF.ParamGetI("CipherMode");
            if (CipherMode > 3)
            {
                CipherMode = 0;
            }
            CipherConfPassword = CF.ParamGetS("CipherPassword");
            int[] BufI1 = new int[Buf1.Length];
            int[] BufI2 = new int[Buf2.Length];
            for (int i = 0; i < Buf1.Length; i++)
            {
                BufI1[i] = TextWork.CodeChar(Buf1[i]);
            }
            for (int i = 0; i < Buf2.Length; i++)
            {
                BufI2[i] = TextWork.CodeChar(Buf2[i]);
            }
            CipherChar1 = new List<int>();
            CipherChar2 = new List<int>();

            for (int i = 0; i < BufI1.Length; i++)
            {
                if ((BufI2[i] >= 0) && (!CipherAlphabet.Contains(BufI1[i])))
                {
                    CipherChar1.Add(BufI1[i]);
                }
            }
            for (int i = 0; i < BufI2.Length; i++)
            {
                if ((BufI2[i] >= 0) && (!CipherAlphabet.Contains(BufI2[i])))
                {
                    CipherChar2.Add(BufI2[i]);
                }
            }

            if ((CipherMode > 0) && (CipherChar1.Count > 0) && (CipherChar2.Count > 0) && (CipherAlphabet.Count > 0))
            {
                CipherEnabled = true;
            }
            else
            {
                CipherEnabled = false;
            }
        }

        int CryptCounter = 0;
        bool IsCrypt = false;

        public void Reset()
        {
            CryptCounter = 0;
            IsCrypt = false;
            WorkPassword = new int[Password.Length];
            for (int i = 0; i < Password.Length; i++)
            {
                WorkPassword[i] = CipherAlphabet.IndexOf(Password[i]);
            }
        }

        public int PasswordState = 0;
        string Password = "";
        string Password0 = "";
        int PasswordCursor = 0;
        string PasswordHeader = "";

        int[] WorkPassword;

        public void SetPassword(string Password_)
        {
            Password = Password_;
            PasswordApply();
        }

        public List<int> Crypt(List<int> Str, bool Inverse)
        {
            if ((WorkPassword.Length == 0) || (!CipherEnabled))
            {
                return TextWork.Copy(Str);
            }

            List<int> Str_ = new List<int>();
            for (int i = 0; i < Str.Count; i++)
            {
                int C = Str[i];
                if (CipherChar1.Contains(C) && CipherChar2.Contains(C))
                {
                    IsCrypt = !IsCrypt;
                }
                else
                {
                    if (CipherChar1.Contains(C))
                    {
                        IsCrypt = true;
                    }
                    if (CipherChar2.Contains(C))
                    {
                        IsCrypt = false;
                    }
                }
                if (IsCrypt && CipherAlphabet.Contains(C))
                {
                    int N = CipherAlphabet.IndexOf(C);
                    int N0 = N;
                    if (Inverse)
                    {
                        N = N - WorkPassword[CryptCounter];
                        while (N < 0)
                        {
                            N += CipherAlphabet.Count;
                        }
                        if (CipherMode == 2)
                        {
                            WorkPassword[CryptCounter] = N;
                        }
                        if (CipherMode == 3)
                        {
                            WorkPassword[CryptCounter] = N0;
                        }
                    }
                    else
                    {
                        N = N + WorkPassword[CryptCounter];
                        while (N >= CipherAlphabet.Count)
                        {
                            N -= CipherAlphabet.Count;
                        }
                        if (CipherMode == 2)
                        {
                            WorkPassword[CryptCounter] = N0;
                        }
                        if (CipherMode == 3)
                        {
                            WorkPassword[CryptCounter] = N;
                        }
                    }
                    C = CipherAlphabet[N];
                    CryptCounter++;
                    if (CryptCounter == WorkPassword.Length)
                    {
                        CryptCounter = 0;
                    }
                }
                Str_.Add(C);
            }
            return Str_;
        }


        void PasswordApply()
        {
            string Password_ = "";
            if (Password.Length > 2)
            {
                List<int> PassI = TextWork.StrToInt(Password);
                if (CipherChar1.Contains(PassI[0]))
                {
                    if (CipherChar2.Contains(PassI[PassI.Count - 1]))
                    {
                        try
                        {
                            Password = File.ReadAllText(Password.Substring(1, Password.Length - 2));
                        }
                        catch
                        {
                            Password = "";
                        }
                    }
                }
            }
            for (int i = 0; i < Password.Length; i++)
            {
                if (CipherAlphabet.Contains(Password[i]))
                {
                    Password_ = Password_ + ((char)Password[i]).ToString();
                }
            }
            Password = Password_;
        }

        bool IsPasswordCorrect()
        {
            for (int i = 0; i < Password.Length; i++)
            {
                if (!CipherAlphabet.Contains(Password[i]))
                {
                    return false;
                }
            }
            return true;
        }


        void PasswordStateDisp()
        {
            string Password_ = Password;
            Core_.Screen_.SetStatusText(PasswordHeader + Password_, Core_.StatusBack, Core_.StatusFore);
            int CurPos = PasswordCursor + PasswordHeader.Length;
            Core_.Screen_.PutChar(CurPos, Core_.Screen_.WinH - 1, (Password_ + " ")[PasswordCursor], Core_.StatusBack, Core_.StatusFore, 0, 0);
            Core_.Screen_.SetCursorPosition(CurPos, Core_.Screen_.WinH - 1);
        }

        public void PasswordInput(int NewState)
        {
            Password = "";
            PasswordCursor = 0;
            PasswordState = NewState;
            PasswordHeader = "Password: ";
            PasswordStateDisp();
        }

        public void PasswordExit()
        {
            switch (PasswordState)
            {
                case 1:
                    if (Password != null)
                    {
                        PasswordApply();
                        if (IsPasswordCorrect())
                        {
                            Password0 = Password;
                            Password = "";
                            PasswordCursor = 0;
                            PasswordState = 3;
                            PasswordHeader = "Confirm: ";
                            PasswordStateDisp();
                        }
                        else
                        {
                            Password = "";
                            PasswordCursor = 0;
                            PasswordStateDisp();
                        }
                    }
                    else
                    {
                        PasswordState = 0;
                        Core_.ScreenRefresh(true);
                    }
                    break;
                case 2:
                    if (Password != null)
                    {
                        PasswordApply();
                        if (IsPasswordCorrect())
                        {
                            PasswordState = 0;
                            Core_.FileLoad0_();
                        }
                        else
                        {
                            Password = "";
                            PasswordCursor = 0;
                            PasswordStateDisp();
                        }
                    }
                    else
                    {
                        PasswordState = 0;
                        Core_.ScreenRefresh(true);
                    }
                    break;
                case 3:
                    if (Password != null)
                    {
                        PasswordApply();
                        if (Password == Password0)
                        {
                            PasswordState = 0;
                            Core_.FileSave0_();
                            Core_.ScreenRefresh(true);
                        }
                        else
                        {
                            Password = "";
                            PasswordCursor = 0;
                            PasswordState = 1;
                            PasswordHeader = "Password: ";
                            PasswordStateDisp();
                        }
                    }
                    else
                    {
                        PasswordState = 0;
                        Core_.ScreenRefresh(true);
                    }
                    break;
            }
        }

        public void PasswordEvent(string KeyName, char KeyChar)
        {
            switch (KeyName)
            {
                case "Home":
                    PasswordCursor = 0;
                    PasswordStateDisp();
                    break;
                case "End":
                    PasswordCursor = Password.Length;
                    PasswordStateDisp();
                    break;
                case "LeftArrow":
                case "Left":
                    if (PasswordCursor > 0)
                    {
                        PasswordCursor--;
                        PasswordStateDisp();
                    }
                    break;
                case "RightArrow":
                case "Right":
                    if (PasswordCursor < Password.Length)
                    {
                        PasswordCursor++;
                        PasswordStateDisp();
                    }
                    break;
                case "Escape":
                    Password = null;
                    PasswordExit();
                    break;
                case "Enter":
                case "Return":
                    PasswordExit();
                    break;
                case "Backspace":
                case "Back":
                case "Delete":
                    if (KeyName == "Delete")
                    {
                        PasswordCursor++;
                    }
                    if ((Password.Length > 0) && (PasswordCursor > 0) && (PasswordCursor <= Password.Length))
                    {
                        if (PasswordCursor == Password.Length)
                        {
                            if (PasswordCursor == 1)
                            {
                                Password = "";
                            }
                            else
                            {
                                Password = Password.Substring(0, PasswordCursor - 1);
                            }
                        }
                        else
                        {
                            if (PasswordCursor > 1)
                            {
                                Password = Password.Substring(0, PasswordCursor - 1) + Password.Substring(PasswordCursor);
                            }
                            else
                            {
                                Password = Password.Substring(1);
                            }
                        }
                        PasswordCursor--;
                        PasswordStateDisp();
                    }
                    else
                    {
                        if (KeyName == "Delete")
                        {
                            PasswordCursor--;
                        }
                    }
                    break;
                default:
                    if (KeyChar >= 32)
                    {
                        Password = Password + KeyChar.ToString();
                        PasswordCursor++;
                        PasswordStateDisp();
                    }
                    break;
            }
        }
    }
}
