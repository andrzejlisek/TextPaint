/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-07-02
 * Time: 01:09
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace TextPaint
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Core Core_ = new Core();
            if (args.Length > 0)
            {
                Core_.Init(args[0], args);
            }
            else
            {
                Core_.Init("", args);
            }
        }
    }
}