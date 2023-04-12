namespace AdSoft.Fez
{
    using System;

    using Microsoft.SPOT;

    public static class DebugEx
    {
        public static void Print(string text, Exception ex)
        {
            Debug.Print(text + ": " + ex.Message);
        }
        
        public static void WriteBits(byte data)
        {
            WriteBits("", data);
        }

        public static void WriteBits(string text, byte data)
        {
            text += (data / (1 << 7) % 2).ToString();
            text += (data / (1 << 6) % 2).ToString();
            text += (data / (1 << 5) % 2).ToString();
            text += (data / (1 << 4) % 2).ToString();
            text += (data / (1 << 3) % 2).ToString();
            text += (data / (1 << 2) % 2).ToString();
            text += (data / (1 << 1) % 2).ToString();
            text += (data % 2).ToString();
            Debug.Print(text);
        }
    }
}
