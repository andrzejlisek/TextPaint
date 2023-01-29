using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace TextPaint
{
    public class Window_PictureBoxExPanel : Panel
    {
        LowLevelBitmap Image_;
        Graphics ImageG_;

        public LowLevelBitmap Image__
        {
            set
            {
                Image_ = value;
                Repaint();
            }
        }

        public Window_PictureBoxExPanel()
        {
            Image_ = null;
            ImageG_ = this.CreateGraphics();
        }

        void RepaintS()
        {
            ImageG_ = this.CreateGraphics();
            Repaint();
        }

        void Repaint()
        {
            if (Image_ != null)
            {
                Monitor.Enter(Image_);
                try
                {
                    ImageG_.DrawImageUnscaledAndClipped(Image_.ToBitmap(), new Rectangle(0, 0, this.Width, this.Height));
                }
                catch
                {
                }
                Monitor.Exit(Image_);
            }
            else
            {
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
