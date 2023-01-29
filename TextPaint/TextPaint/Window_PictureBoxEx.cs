using System;
using System.Drawing;
using System.Windows.Forms;

namespace TextPaint
{
    public class Window_PictureBoxEx
    {
        public Window_PictureBoxEx(int Custom)
        {
            switch (Custom)
            {
                case 0:
                    Ctrl = new PictureBox();
                    break;
                case 1:
                    Ctrl = new Window_PictureBoxMonitor();
                    break;
                case 2:
                    Ctrl = new Window_PictureBoxExPanel();
                    break;
            }
        }

        public bool UseMonitor
        {
            set
            {
                if (Ctrl is Window_PictureBoxMonitor)
                {
                    ((Window_PictureBoxMonitor)Ctrl).UseMonitor = value;
                }

            }
        }

        public Control Ctrl;
        public LowLevelBitmap Image
        {
            set
            {
                if (Ctrl is Window_PictureBoxMonitor)
                {
                    ((Window_PictureBoxMonitor)Ctrl).Image_ = value;
                }
                if (Ctrl is Window_PictureBoxExPanel)
                {
                    ((Window_PictureBoxExPanel)Ctrl).Image__ = value;
                }
            }
        }
    }
}
