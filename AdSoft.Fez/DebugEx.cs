namespace AdSoft.Fez
{
    using System;

    using Microsoft.SPOT;

    public static class DebugEx
    {
        public static Target Targets { get; set; }

        public static void Print(Target target, string text)
        {
            if ((Targets & target) > 0)
            {
                Debug.Print(text);
            }
        }

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

        [Flags]
        public enum Target
        {
            None = 0,
            ScreenSaver = 1,
            Ui = 1 << 1,
            Log = 1 << 2,
            Keyboard = 1 << 3,
            PressureLog = 1 << 4
        }
    }
}
