using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextPaint
{
    public class ToolEncoding : Tool
    {
        public ToolEncoding(ConfigFile CF) : base(CF)
        {
        }

        public override void Start()
        {
            string DirName = CF.ParamGetS("EncodingDir");

            Console.WriteLine("Creating encoding files...");
            if (!Directory.Exists(DirName))
            {
                Directory.CreateDirectory(DirName);
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
                    string EncodingFileName = Path.Combine(DirName, e.CodePage.ToString().PadLeft(5, '0') + ".txt");
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
        }
    }
}
