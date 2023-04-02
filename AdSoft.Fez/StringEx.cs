namespace AdSoft.Fez
{
    public static class StringEx
    {
        private const string SpacePad = "          ";

        public static string PadRight(string value, int length)
        {
            if (length < 1)
            {
                return string.Empty;
            }

            while (value.Length < length)
            {
                value += SpacePad;
            }

            return value.Substring(0, length);
        }
    }
}
