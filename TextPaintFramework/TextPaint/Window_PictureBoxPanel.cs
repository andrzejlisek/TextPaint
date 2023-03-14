using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace TextPaint
{
    public class Window_PictureBoxPanel : Panel
    {
        LowLevelBitmap Image_0;
        public int DrawW = 0;
        public int DrawH = 0;

        Graphics ImageG_;

        public LowLevelBitmap Image_
        {
            set
            {
                Image_0 = value;
                Repaint();
            }
        }

        bool BitmapStretch = false;

        public Window_PictureBoxPanel(bool BitmapStretch_)
        {
            BitmapStretch = BitmapStretch_;
            Image_0 = null;
            ImageG_ = this.CreateGraphics();
        }

        void RepaintS()
        {
            ImageG_ = this.CreateGraphics();
            Repaint();
        }

        void Repaint()
        {
            if (Image_0 != null)
            {
                Bitmap Bmp;
                if (BitmapStretch)
                {
                    Bmp = Image_0.ToBitmap(DrawW, DrawH);
                }
                else
                {
                    Bmp = Image_0.ToBitmap();
                }

                if ((Bmp.Width == DrawW) && (Bmp.Height == DrawH))
                {
                    ImageG_.DrawImageUnscaledAndClipped(Bmp, new Rectangle(0, 0, DrawW, DrawH));
                }
                else
                {
                    ImageG_.DrawImage(Bmp, 0, 0, DrawW, DrawH);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            Repaint();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
            Repaint();
        }

        public override void Refresh()
        {
            //base.Refresh();
            Repaint();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            RepaintS();
        }

        protected override void OnAutoSizeChanged(EventArgs e)
        {
            base.OnAutoSizeChanged(e);
            RepaintS();
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            RepaintS();
        }
    }
}
