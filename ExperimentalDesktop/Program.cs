namespace ExperimentalDesktop
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 1; i <= 12; i++)
            {
                int month = i;

                month = month % 12 + 1;
                var now = DateTime.Now;
                var date = new DateTime(now.Year, month, 1).Subtract(new TimeSpan(7, 0, 0, 0));
                var add = (7 - (int)date.DayOfWeek) % 7;
                var lastSunday = date.AddDays(add);

                Console.WriteLine(i + " - " + lastSunday.ToString("M"));
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
