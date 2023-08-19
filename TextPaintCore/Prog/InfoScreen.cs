using System;
using System.Collections.Generic;
using System.IO;

namespace TextPaint
{
    public class InfoScreen
    {
        public InfoScreen()
        {
            int CurrentIdx = -1;
            FileStream F = new FileStream(Core.AppDir() + "Info.txt", FileMode.Open, FileAccess.Read);
            StreamReader F_ = new StreamReader(F);
            string Buf = F_.ReadLine();
            while (Buf != null)
            {
                if (Buf.StartsWith("@", StringComparison.InvariantCulture))
                {
                    CurrentIdx = -1;
                    if (Buf.Length > 1)
                    {
                        CurrentIdx = int.Parse(Buf.Substring(1));
                    }
                }
                else
                {
                    if (CurrentIdx >= 0)
                    {
                        if (!InfoText_.ContainsKey(CurrentIdx))
                        {
                            InfoText_.Add(CurrentIdx, new List<string>());
                        }
                        InfoText_[CurrentIdx].Add(Buf);
                    }
                }
                Buf = F_.ReadLine();
            }
            F_.Close();
            F.Close();

            int CommonNr1 = 0;
            int CommonNr2 = 9;
            if (InfoText_.ContainsKey(CommonNr1))
            {
                for (int i = InfoText_[CommonNr1].Count - 1; i >= 0; i--)
                {
                    foreach (var item in InfoText_)
                    {
                        if ((item.Key != CommonNr1) && (item.Key != CommonNr2))
                        {
                            item.Value.Insert(0, InfoText_[CommonNr1][i]);
                        }
                    }
                }
            }
            if (InfoText_.ContainsKey(CommonNr2))
            {
                for (int i = 0; i < InfoText_[CommonNr2].Count; i++)
                {
                    foreach (var item in InfoText_)
                    {
                        if ((item.Key != CommonNr1) && (item.Key != CommonNr2))
                        {
                            item.Value.Add(InfoText_[CommonNr2][i]);
                        }
                    }
                }
            }
        }

        Dictionary<int, List<string>> InfoText_ = new Dictionary<int, List<string>>();
        public List<string> InfoText = new List<string>();
        public int InfoX = 0;
        public int InfoY = 0;
        public int InfoW = 0;
        public int InfoH = 0;

        public bool Shown = false;
        public bool RequestHide = false;
        public bool RequestClose = false;

        public int ScreenNeedRepaint = 0;

        public bool ScreenKey(string KeyName, char KeyChar)
        {
            ScreenNeedRepaint = 0;
            if (Shown)
            {
                switch (KeyName)
                {
                    case "UpArrow":
                    case "Up":
                        if (InfoY > 0)
                        {
                            InfoY--;
                            ScreenNeedRepaint = 2;
                        }
                        break;
                    case "DownArrow":
                    case "Down":
                        if (InfoY < InfoH)
                        {
                            InfoY++;
                            ScreenNeedRepaint = 3;
                        }
                        else
                        {
                            if (InfoY != InfoH)
                            {
                                InfoY = InfoH;
                                ScreenNeedRepaint = 1;
                            }
                        }
                        break;
                    case "LeftArrow":
                    case "Left":
                        if (InfoX > 0)
                        {
                            InfoX--;
                            ScreenNeedRepaint = 4;
                        }
                        break;
                    case "RightArrow":
                    case "Right":
                        if (InfoX < InfoW)
                        {
                            InfoX++;
                            ScreenNeedRepaint = 5;
                        }
                        else
                        {
                            if (InfoX != InfoW)
                            {
                                InfoX = InfoW;
                                ScreenNeedRepaint = 1;
                            }
                        }
                        break;

                    case "Enter":
                    case "Return":
                    case "Escape":
                    case "F1":
                    case "F2":
                    case "F3":
                    case "F4":
                    case "F5":
                    case "F6":
                    case "F7":
                    case "F8":
                    case "F9":
                    case "F10":
                    case "F11":
                    case "F12":
                        RequestHide = true;
                        break;
                    case "WindowClose":
                        RequestClose = true;
                        break;
                }
            }
            return Shown;
        }

        public void ScreenShow(int TextNo)
        {
            RequestHide = false;
            RequestClose = false;
            if (InfoText_.ContainsKey(TextNo))
            {
                InfoText = InfoText_[TextNo];
            }
            else
            {
                InfoText = new List<string>();
            }
            InfoX = 0;
            InfoY = 0;
            InfoW = 0;
            for (int i = 0; i < InfoText.Count; i++)
            {
                if (InfoW < (InfoText[i].Length - 1))
                {
                    InfoW = (InfoText[i].Length - 1);
                }
            }
            InfoH = InfoText.Count - 1;
            Shown = true;
        }

        public void ScreenHide()
        {
            RequestHide = false;
            Shown = false;
        }
    }
}
