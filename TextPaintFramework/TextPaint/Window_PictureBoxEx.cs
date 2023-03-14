using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace TextPaint
{
    public class Window_PictureBoxEx : PictureBox
    {
        public LowLevelBitmap Image_ = null;
        public int DrawW = 0;
        public int DrawH = 0;

        bool BitmapStretch = false;

        public Window_PictureBoxEx(bool BitmapStretch_)
        {
            BitmapStretch = BitmapStretch_;
            SizeMode = PictureBoxSizeMode.StretchImage;
        }

        public override void Refresh()
        {
            if (Image_ != null)
            {
                if (BitmapStretch)
                {
                    Image = Image_.ToBitmap(DrawW, DrawH);
                }
                else
                {
                    Image = Image_.ToBitmap();
                }
            }
            base.Refresh();
        }
    }
}
