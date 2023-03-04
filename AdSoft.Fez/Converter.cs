namespace AdSoft.Fez
{
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
    }
}