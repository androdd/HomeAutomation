namespace ExperimentalDesktop
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            UInt16 debug = 0;



            debug = (UInt16)(0 | DebugTarget.Ui);
            
            Console.WriteLine(ToBits(debug));

            if ((debug & (ushort)DebugTarget.Ui) > 0)
            {
                Console.WriteLine("UI");
            }
            if ((debug & (ushort)DebugTarget.ScreenSaver) > 0)
            {
                Console.WriteLine("SS");
            }
            if ((debug & (ushort)DebugTarget.Log) > 0)
            {
                Console.WriteLine("Log");
            }
        }

        private static string ToBits(UInt16 data)
        {
            var text = "";
            text += (data / (1 << 15) % 2).ToString();
            text += (data / (1 << 14) % 2).ToString();
            text += (data / (1 << 13) % 2).ToString();
            text += (data / (1 << 12) % 2).ToString() + " ";
            text += (data / (1 << 11) % 2).ToString();
            text += (data / (1 << 10) % 2).ToString();
            text += (data / (1 << 9) % 2).ToString();
            text += (data / (1 << 8) % 2).ToString() + " ";

            text += (data / (1 << 7) % 2).ToString();
            text += (data / (1 << 6) % 2).ToString();
            text += (data / (1 << 5) % 2).ToString();
            text += (data / (1 << 4) % 2).ToString() + " ";
            text += (data / (1 << 3) % 2).ToString();
            text += (data / (1 << 2) % 2).ToString();
            text += (data / (1 << 1) % 2).ToString();
            text += (data % 2).ToString();

            return text;
        }

        public enum DebugTarget : ushort
        {
            ScreenSaver = 1 << 2,
            Ui = 1 << 14,
            Log = 1 << 6
        }
    }
}
