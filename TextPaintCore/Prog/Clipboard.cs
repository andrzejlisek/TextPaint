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
                return await Avalonia.Application.Current.Clipboard.GetTextAsync();
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
                await Avalonia.Application.Current.Clipboard.SetTextAsync(Txt);
                return await Avalonia.Application.Current.Clipboard.GetTextAsync();
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
