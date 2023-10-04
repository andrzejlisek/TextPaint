using System;
namespace TextPaint
{
    public class Tool
    {
        protected ConfigFile CF;
        public Tool(ConfigFile CF_)
        {
            CF = CF_;
            ESC = ((char)27).ToString();
            CSI = ((char)27).ToString() + "[";
            EOL = "\r\n";
        }

        protected string ESC;
        protected string CSI;
        protected string EOL;

        public virtual void Start()
        {
            Console.WriteLine("Invalid tool");
        }
    }
}
