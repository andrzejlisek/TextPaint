/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-05-31
 * Time: 07:51
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace TextPaint
{
    /// <summary>
    /// Configuration file
    /// </summary>
    public class ConfigFile
    {
        private Dictionary<string, string> Raw = new Dictionary<string, string>();

        public void FileLoad(string FileName)
        {
            ParamClear();
            try
            {
                FileStream F_ = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                StreamReader F = new StreamReader(F_);
                while (!F.EndOfStream)
                {
                    string S = F.ReadLine();
                    int I = S.IndexOf("=");
                    if (I >= 0)
                    {
                        string RawK = S.Substring(0, I);
                        if (!Raw.ContainsKey(RawK))
                        {
                            if (S.Length > (I + 1))
                            {
                                Raw.Add(RawK, S.Substring(I + 1));
                            }
                            else
                            {
                                Raw.Add(RawK, "");
                            }
                        }
                    }
                }
                F.Close();
                F_.Close();
            }
            catch
            {

            }
        }

        public void FileSave(string FileName)
        {
            try
            {
                FileStream F_ = new FileStream(FileName, FileMode.Create, FileAccess.Write);
                StreamWriter F = new StreamWriter(F_);
                foreach (KeyValuePair<string, string> item in Raw)
                {
                    F.Write(item.Key);
                    F.Write("=");
                    F.Write(item.Value);
                    F.WriteLine();
                }
                F.Close();
                F_.Close();
            }
            catch
            {

            }
        }

        public void ParamClear()
        {
            Raw.Clear();
        }

        public void ParamRemove(string Name)
        {
            if (Raw.ContainsKey(Name))
            {
                Raw.Remove(Name);
            }
        }

        public void ParamSet(string Name, string Value)
        {
            if (Raw.ContainsKey(Name))
            {
                Raw[Name] = Value;
            }
            else
            {
                Raw.Add(Name, Value);
            }
        }

        public void ParamSet(string Name, int Value)
        {
            ParamSet(Name, Value.ToString());
        }

        public void ParamSet(string Name, long Value)
        {
            ParamSet(Name, Value.ToString());
        }

        public void ParamSet(string Name, bool Value)
        {
            ParamSet(Name, Value ? "1" : "0");
        }

        public bool ParamGet(string Name, ref string Value)
        {
            if (Raw.ContainsKey(Name))
            {
                Value = Raw[Name];
                return true;
            }
            return false;
        }

        public bool ParamGet(string Name, ref int Value)
        {
            if (Raw.ContainsKey(Name))
            {
                try
                {
                    Value = int.Parse(Raw[Name]);
                    return true;
                }
                catch
                {

                }
            }
            return false;
        }

        public bool ParamGet(string Name, ref long Value)
        {
            if (Raw.ContainsKey(Name))
            {
                try
                {
                    Value = long.Parse(Raw[Name]);
                    return true;
                }
                catch
                {

                }
            }
            return false;
        }

        public bool ParamGet(string Name, ref bool Value)
        {
            if (Raw.ContainsKey(Name))
            {
                if ((Raw[Name] == "1") || (Raw[Name].ToUpperInvariant() == "TRUE") || (Raw[Name].ToUpperInvariant() == "YES") || (Raw[Name].ToUpperInvariant() == "T") || (Raw[Name].ToUpperInvariant() == "Y"))
                {
                    Value = true;
                    return true;
                }
                if ((Raw[Name] == "0") || (Raw[Name].ToUpperInvariant() == "FALSE") || (Raw[Name].ToUpperInvariant() == "NO") || (Raw[Name].ToUpperInvariant() == "F") || (Raw[Name].ToUpperInvariant() == "N"))
                {
                    Value = false;
                    return true;
                }
            }
            return false;
        }

        public string ParamGetS(string Name, string X)
        {
            ParamGet(Name, ref X);
            return X;
        }

        public int ParamGetI(string Name, int X)
        {
            ParamGet(Name, ref X);
            return X;
        }

        public long ParamGetL(string Name, long X)
        {
            ParamGet(Name, ref X);
            return X;
        }

        public bool ParamGetB(string Name, bool X)
        {
            ParamGet(Name, ref X);
            return X;
        }

        public string ParamGetS(string Name)
        {
            string X = "";
            ParamGet(Name, ref X);
            return X;
        }

        public int ParamGetI(string Name)
        {
            int X = 0;
            ParamGet(Name, ref X);
            return X;
        }

        public long ParamGetL(string Name)
        {
            long X = 0;
            ParamGet(Name, ref X);
            return X;
        }

        public bool ParamGetB(string Name)
        {
            bool X = false;
            ParamGet(Name, ref X);
            return X;
        }

        public bool ParamExists(string Name)
        {
            return Raw.ContainsKey(Name);
        }
    }
}
