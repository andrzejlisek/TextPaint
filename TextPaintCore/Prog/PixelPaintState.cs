using System;
namespace TextPaint
{
    public class PixelPaintState
    {
        public PixelPaintState()
        {
        }
        public int PaintModeN = 0;

        public int CanvasXBase = 0;
        public int CanvasYBase = 0;
        public int CanvasX = 0;
        public int CanvasY = 0;
        public int SizeX = 0;
        public int SizeY = 0;

        public int CharX = 0;
        public int CharY = 0;
        public int CharW = 1;
        public int CharH = 1;

        public int FontW = 1;
        public int FontH = 1;

        public bool DefaultColor = false;

        public bool PaintPencil = false;
        public int PaintMoveRoll = 0;
        public int PaintColor = 0;


        void ObjCopy(PixelPaintState Src, PixelPaintState Dst)
        {
            Dst.PaintModeN = Src.PaintModeN;
            Dst.CanvasXBase = Src.CanvasXBase;
            Dst.CanvasYBase = Src.CanvasYBase;
            Dst.CanvasX = Src.CanvasX;
            Dst.CanvasY = Src.CanvasY;
            Dst.SizeX = Src.SizeX;
            Dst.SizeY = Src.SizeY;
            Dst.CharX = Src.CharX;
            Dst.CharY = Src.CharY;
            Dst.CharW = Src.CharW;
            Dst.CharH = Src.CharH;
            Dst.FontW = Src.FontW;
            Dst.FontH = Src.FontH;
            Dst.DefaultColor = Src.DefaultColor;
            Dst.PaintColor = Src.PaintColor;
            Dst.PaintPencil = Src.PaintPencil;
            Dst.PaintMoveRoll = Src.PaintMoveRoll;
        }

        public void SetState(PixelPaintState _)
        {
            ObjCopy(_, this);
        }

        public PixelPaintState GetState()
        {
            PixelPaintState _ = new PixelPaintState();
            ObjCopy(this, _);
            return _;
        }
    }
}
