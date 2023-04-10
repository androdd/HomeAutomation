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
                switch (target)
                {
                    case Target.None:
                        break;
                    case Target.ScreenSaver:
                        text = "ScreenSaver - " + text;
                        break;
                    case Target.Ui:
                        text = "Ui - " + text;
                        break;
                    case Target.Log:
                        text = "Log - " + text;
                        break;
                    case Target.Keyboard:
                        text = "Keyboard - " + text;
                        break;
                    case Target.PressureLog:
                        text = "PressureLog - " + text;
                        break;
                    case Target.Lcd2004:
                        text = "Lcd2004 - " + text;
                        break;
                    case Target.ScreenPowerButton:
                        text = "ScreenPowerButton - " + text;
                        break;
                    case Target.WateringService:
                        text = "WateringService - " + text;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("target");
                }

                Debug.Print(text);
            }
        }

        public static void Print(string text)
        {
            Debug.Print(text);
        }

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

        [Flags]
        public enum Target
        {
            None = 0,
            ScreenSaver = 1,
            Ui = 1 << 1,
            Log = 1 << 2,
            Keyboard = 1 << 3,
            PressureLog = 1 << 4,
            Lcd2004 = 1 << 5,
            ScreenPowerButton = 1 << 6,
            WateringService = 1 << 7
        }
    }
}
