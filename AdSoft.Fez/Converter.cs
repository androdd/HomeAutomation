namespace AdSoft.Fez
{
    using System;

    public static class Converter
    {
        public static bool TryParse(string val, out int result)
        {
            try
            {
                result = int.Parse(val);
            }
            catch
            {
                result = 0;
                return false;
            }

            return true;
        }

        public static bool TryParse(string val, out byte result)
        {
            try
            {
                result = byte.Parse(val);
            }
            catch
            {
                result = 0;
                return false;
            }

            return true;
        }

        public static bool TryParse(string val, out ushort result)
        {
            try
            {
                result = ushort.Parse(val);
            }
            catch
            {
                result = 0;
                return false;
            }

            return true;
        }

        public static bool TryParse(string val, out double result)
        {
            try
            {
                result = double.Parse(val);
            }
            catch
            {
                result = 0;
                return false;
            }

            return true;
        }

        public static bool TryParseTime(string val, out DateTime result)
        {
            var parts = val.Split(':', '|'); // In configuration file : is used for delimiting Keys from Values

            try
            {
                if (parts.Length == 2)
                {
                    result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(parts[0]), int.Parse(parts[1]), 0);
                }
                else if (parts.Length == 3)
                {
                    result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
                }
                else
                {
                    result = DateTime.MinValue;
                    return false;
                }

                return true;
            }
            catch{}

            result = DateTime.MinValue;
            return false;
        }
    }
}