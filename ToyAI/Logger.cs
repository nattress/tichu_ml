using System;

namespace TichuAI
{
    public class Logger
    {
        private static Logger s_log = new Logger();

        private Logger() {}
        public bool Enabled { get; set; }

        public static Logger Log => s_log;

        public void WriteLine(string value)
        {
            if (!Enabled)
                return;

            Console.WriteLine(value);
        }

        public void Write(string value)
        {
            if (!Enabled)
                return;

            Console.Write(value);
        }
    }
}