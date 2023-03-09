namespace AdSoft.Fez
{
    using Microsoft.SPOT;

    public static class DebugEx
    {
        public static void Print(string text)
        {
            Debug.Print(text);
        }
        
        public static void UiPrint(string name, string method, string text = null)
        {
#if DEBUG_UI
            Debug.Print(name + "." + method + (text == null ? "" : ": ") + text);
#endif
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
