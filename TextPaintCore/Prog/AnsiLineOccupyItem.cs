using System;
namespace TextPaint
{
    public class AnsiLineOccupyItem
    {
        // Attribute bits:
        // 1. Bold             01  FE
        // 2. Italic           02  FD
        // 4. Underline        04  FB
        // 8. Blink            08  F7
        // 16. Reverse         10  EF
        // 32. Concealed       20  DF
        // 64. Strikethrough   40  BF
        // 128. ScreenNegate   80  7F

        public int Item_Char;
        public int Item_ColorB;
        public int Item_ColorF;
        public int Item_ColorA;
        public int Item_FontW;
        public int Item_FontH;
        public int Item_Type;

        public AnsiLineOccupyItem()
        {
            BlankChar();
        }

        public void BlankChar()
        {
            Item_Char = TextWork.SpaceChar0;
            Item_ColorB = -1;
            Item_ColorF = -1;
            Item_ColorA = 0;
            Item_FontW = 0;
            Item_FontH = 0;
            Item_Type = 0;
        }

        public void BlankChar(int ColorB_, int ColorF_, int ColorA_)
        {
            Item_Char = TextWork.SpaceChar0;
            Item_ColorB = ColorB_;
            Item_ColorF = ColorF_;
            Item_ColorA = ColorA_;
            Item_FontW = 0;
            Item_FontH = 0;
            Item_Type = 0;
        }

        public void CopyItem(AnsiLineOccupyItem Src)
        {
            Item_Char = Src.Item_Char;
            Item_ColorB = Src.Item_ColorB;
            Item_ColorF = Src.Item_ColorF;
            Item_ColorA = Src.Item_ColorA;
            Item_FontW = Src.Item_FontW;
            Item_FontH = Src.Item_FontH;
            Item_Type = Src.Item_Type;
        }

        public AnsiLineOccupyItem CopyItemObj()
        {
            AnsiLineOccupyItem Obj = new AnsiLineOccupyItem();
            Obj.CopyItem(this);
            return Obj;
        }
    }
}
