/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-12
 * Time: 21:53
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TextPaint
{
    /// <summary>
    /// Description of Clipboard.
    /// </summary>
    public class Clipboard : ClipboardBase
    {
        public static async Task<string> SysClipboardGetSystem()
        {
            try
            {
                if (System.Windows.Forms.Clipboard.ContainsText())
                {
                    string Txt_ = System.Windows.Forms.Clipboard.GetText();
                    if (Txt_ == null)
                    {
                        Txt_ = "";
                    }
                    return Txt_;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> SysClipboardSetSystem(string Txt)
        {
            try
            {
                System.Windows.Forms.Clipboard.SetText(Txt);
                return System.Windows.Forms.Clipboard.GetText();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> SysClipboardGet()
        {
            string Txt_ = await SysClipboardGetSystem();
            if (Txt_ == null)
            {
                LastSysText = "";
                return false;
            }
            if (Txt_ != LastSysText)
            {
                string[] Txt = Txt_.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                TextClipboard.Clear();
                for (int i = 0; i < Txt.Length; i++)
                {
                    TextClipboard.AppendLine();
                    TextClipboard.SetLineString(i, TextWork.StrToInt(Txt[i]));
                }
            }
            return true;
        }

        public static async Task<int> SysClipboardSet()
        {
            System.Text.StringBuilder Txt = new System.Text.StringBuilder();
            for (int i = 0; i < TextClipboard.CountLines(); i++)
            {
                Txt.AppendLine(TextWork.IntToStr(TextClipboard.GetLineString(i)));
            }
            LastSysText = await SysClipboardSetSystem(Txt.ToString());
            if (LastSysText == null)
            {
                LastSysText = "";
            }
            return 0;
        }

        public Clipboard(Core Core__) : base(Core__)
        {
            Core_ = Core__;
        }
    }
}
