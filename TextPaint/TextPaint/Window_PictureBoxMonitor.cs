using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace TextPaint
{
    public class Window_PictureBoxMonitor : PictureBox
    {
        public LowLevelBitmap Image_ = null;
        public bool UseMonitor = true;

        bool Mon = false;

        bool Monitor_Enter()
        {
            if (Image_ != null)
            {
                if (UseMonitor)
                {
                    Monitor.Enter(Image_);
                }
                Mon = true;
            }
            else
            {
                Mon = false;
            }
            return Mon;
        }

        void Monitor_Exit()
        {
            if (Mon)
            {
                if (UseMonitor)
                {
                    Monitor.Exit(Image_);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Monitor_Enter();
            try
            {
                base.OnPaint(e);
            }
            catch
            {

            }
            Monitor_Exit();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Monitor_Enter();
            try
            {
                base.OnPaintBackground(e);
            }
            catch
            {

            }
            Monitor_Exit();
        }

        public override void Refresh()
        {
            Monitor_Enter();
            Image = Image_.ToBitmap();
            try
            {
                base.Refresh();
            }
            catch
            {

            }
            Monitor_Exit();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Monitor_Enter();
            base.OnSizeChanged(e);
            Monitor_Exit();
        }

        protected override void OnAutoSizeChanged(EventArgs e)
        {
            Monitor_Enter();
            base.OnAutoSizeChanged(e);
            Monitor_Exit();
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            Monitor_Enter();
            base.OnClientSizeChanged(e);
            Monitor_Exit();
        }
    }
}
