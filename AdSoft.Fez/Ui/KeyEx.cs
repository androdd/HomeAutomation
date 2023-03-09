namespace AdSoft.Fez.Ui
{
    using System;

    public static class KeyEx
    {
        public static int GetDigit(Key key)
        {
            switch (key)
            {
                case Key.D0:
                    return 0;
                case Key.D1:
                    return 1;
                case Key.D2:
                    return 2;
                case Key.D3:
                    return 3;
                case Key.D4:
                    return 4;
                case Key.D5:
                    return 5;
                case Key.D6:
                    return 6;
                case Key.D7:
                    return 7;
                case Key.D8:
                    return 8;
                case Key.D9:
                    return 9;
                default:
                    throw new ArgumentOutOfRangeException("key", "Valid values are from D0 to D9");
            }
        }

        public static string KeyToString(Key key)
        {
            string result;

            switch (key)
            {
                case Key.Backspace:
                    result = "Backspace";
                    break;
                case Key.Tab:
                    result = "Tab";
                    break;
                case Key.Clear:
                    result = "Clear";
                    break;
                case Key.Enter:
                    result = "Enter";
                    break;
                case Key.Pause:
                    result = "Pause";
                    break;
                case Key.Escape:
                    result = "Escape";
                    break;
                case Key.Spacebar:
                    result = "Spacebar";
                    break;
                case Key.PageUp:
                    result = "PageUp";
                    break;
                case Key.PageDown:
                    result = "PageDown";
                    break;
                case Key.End:
                    result = "End";
                    break;
                case Key.Home:
                    result = "Home";
                    break;
                case Key.LeftArrow:
                    result = "LeftArrow";
                    break;
                case Key.UpArrow:
                    result = "UpArrow";
                    break;
                case Key.RightArrow:
                    result = "RightArrow";
                    break;
                case Key.DownArrow:
                    result = "DownArrow";
                    break;
                case Key.Select:
                    result = "Select";
                    break;
                case Key.Print:
                    result = "Print";
                    break;
                case Key.Execute:
                    result = "Execute";
                    break;
                case Key.PrintScreen:
                    result = "PrintScreen";
                    break;
                case Key.Insert:
                    result = "Insert";
                    break;
                case Key.Delete:
                    result = "Delete";
                    break;
                case Key.Help:
                    result = "Help";
                    break;
                case Key.D0:
                    result = "D0";
                    break;
                case Key.D1:
                    result = "D1";
                    break;
                case Key.D2:
                    result = "D2";
                    break;
                case Key.D3:
                    result = "D3";
                    break;
                case Key.D4:
                    result = "D4";
                    break;
                case Key.D5:
                    result = "D5";
                    break;
                case Key.D6:
                    result = "D6";
                    break;
                case Key.D7:
                    result = "D7";
                    break;
                case Key.D8:
                    result = "D8";
                    break;
                case Key.D9:
                    result = "D9";
                    break;
                case Key.A:
                    result = "A";
                    break;
                case Key.B:
                    result = "B";
                    break;
                case Key.C:
                    result = "C";
                    break;
                case Key.D:
                    result = "D";
                    break;
                case Key.E:
                    result = "E";
                    break;
                case Key.F:
                    result = "F";
                    break;
                case Key.G:
                    result = "G";
                    break;
                case Key.H:
                    result = "H";
                    break;
                case Key.I:
                    result = "I";
                    break;
                case Key.J:
                    result = "J";
                    break;
                case Key.K:
                    result = "K";
                    break;
                case Key.L:
                    result = "L";
                    break;
                case Key.M:
                    result = "M";
                    break;
                case Key.N:
                    result = "N";
                    break;
                case Key.O:
                    result = "O";
                    break;
                case Key.P:
                    result = "P";
                    break;
                case Key.Q:
                    result = "Q";
                    break;
                case Key.R:
                    result = "R";
                    break;
                case Key.S:
                    result = "S";
                    break;
                case Key.T:
                    result = "T";
                    break;
                case Key.U:
                    result = "U";
                    break;
                case Key.V:
                    result = "V";
                    break;
                case Key.W:
                    result = "W";
                    break;
                case Key.X:
                    result = "X";
                    break;
                case Key.Y:
                    result = "Y";
                    break;
                case Key.Z:
                    result = "Z";
                    break;
                case Key.LeftWindows:
                    result = "LeftWindows";
                    break;
                case Key.RightWindows:
                    result = "RightWindows";
                    break;
                case Key.Applications:
                    result = "Applications";
                    break;
                case Key.Sleep:
                    result = "Sleep";
                    break;
                case Key.NumPad0:
                    result = "NumPad0";
                    break;
                case Key.NumPad1:
                    result = "NumPad1";
                    break;
                case Key.NumPad2:
                    result = "NumPad2";
                    break;
                case Key.NumPad3:
                    result = "NumPad3";
                    break;
                case Key.NumPad4:
                    result = "NumPad4";
                    break;
                case Key.NumPad5:
                    result = "NumPad5";
                    break;
                case Key.NumPad6:
                    result = "NumPad6";
                    break;
                case Key.NumPad7:
                    result = "NumPad7";
                    break;
                case Key.NumPad8:
                    result = "NumPad8";
                    break;
                case Key.NumPad9:
                    result = "NumPad9";
                    break;
                case Key.Multiply:
                    result = "Multiply";
                    break;
                case Key.Add:
                    result = "Add";
                    break;
                case Key.Separator:
                    result = "Separator";
                    break;
                case Key.Subtract:
                    result = "Subtract";
                    break;
                case Key.Decimal:
                    result = "Decimal";
                    break;
                case Key.Divide:
                    result = "Divide";
                    break;
                case Key.F1:
                    result = "F1";
                    break;
                case Key.F2:
                    result = "F2";
                    break;
                case Key.F3:
                    result = "F3";
                    break;
                case Key.F4:
                    result = "F4";
                    break;
                case Key.F5:
                    result = "F5";
                    break;
                case Key.F6:
                    result = "F6";
                    break;
                case Key.F7:
                    result = "F7";
                    break;
                case Key.F8:
                    result = "F8";
                    break;
                case Key.F9:
                    result = "F9";
                    break;
                case Key.F10:
                    result = "F10";
                    break;
                case Key.F11:
                    result = "F11";
                    break;
                case Key.F12:
                    result = "F12";
                    break;
                case Key.F13:
                    result = "F13";
                    break;
                case Key.F14:
                    result = "F14";
                    break;
                case Key.F15:
                    result = "F15";
                    break;
                case Key.F16:
                    result = "F16";
                    break;
                case Key.F17:
                    result = "F17";
                    break;
                case Key.F18:
                    result = "F18";
                    break;
                case Key.F19:
                    result = "F19";
                    break;
                case Key.F20:
                    result = "F20";
                    break;
                case Key.F21:
                    result = "F21";
                    break;
                case Key.F22:
                    result = "F22";
                    break;
                case Key.F23:
                    result = "F23";
                    break;
                case Key.F24:
                    result = "F24";
                    break;
                case Key.BrowserBack:
                    result = "BrowserBack";
                    break;
                case Key.BrowserForward:
                    result = "BrowserForward";
                    break;
                case Key.BrowserRefresh:
                    result = "BrowserRefresh";
                    break;
                case Key.BrowserStop:
                    result = "BrowserStop";
                    break;
                case Key.BrowserSearch:
                    result = "BrowserSearch";
                    break;
                case Key.BrowserFavorites:
                    result = "BrowserFavorites";
                    break;
                case Key.BrowserHome:
                    result = "BrowserHome";
                    break;
                case Key.VolumeMute:
                    result = "VolumeMute";
                    break;
                case Key.VolumeDown:
                    result = "VolumeDown";
                    break;
                case Key.VolumeUp:
                    result = "VolumeUp";
                    break;
                case Key.MediaNext:
                    result = "MediaNext";
                    break;
                case Key.MediaPrevious:
                    result = "MediaPrevious";
                    break;
                case Key.MediaStop:
                    result = "MediaStop";
                    break;
                case Key.MediaPlay:
                    result = "MediaPlay";
                    break;
                case Key.LaunchMail:
                    result = "LaunchMail";
                    break;
                case Key.LaunchMediaSelect:
                    result = "LaunchMediaSelect";
                    break;
                case Key.LaunchApp1:
                    result = "LaunchApp1";
                    break;
                case Key.LaunchApp2:
                    result = "LaunchApp2";
                    break;
                case Key.Oem1:
                    result = "Oem1";
                    break;
                case Key.OemPlus:
                    result = "OemPlus";
                    break;
                case Key.OemComma:
                    result = "OemComma";
                    break;
                case Key.OemMinus:
                    result = "OemMinus";
                    break;
                case Key.OemPeriod:
                    result = "OemPeriod";
                    break;
                case Key.Oem2:
                    result = "Oem2";
                    break;
                case Key.Oem3:
                    result = "Oem3";
                    break;
                case Key.Oem4:
                    result = "Oem4";
                    break;
                case Key.Oem5:
                    result = "Oem5";
                    break;
                case Key.Oem6:
                    result = "Oem6";
                    break;
                case Key.Oem7:
                    result = "Oem7";
                    break;
                case Key.Oem8:
                    result = "Oem8";
                    break;
                case Key.Oem102:
                    result = "Oem102";
                    break;
                case Key.Process:
                    result = "Process";
                    break;
                case Key.Packet:
                    result = "Packet";
                    break;
                case Key.Attention:
                    result = "Attention";
                    break;
                case Key.CrSel:
                    result = "CrSel";
                    break;
                case Key.ExSel:
                    result = "ExSel";
                    break;
                case Key.EraseEndOfFile:
                    result = "EraseEndOfFile";
                    break;
                case Key.Play:
                    result = "Play";
                    break;
                case Key.Zoom:
                    result = "Zoom";
                    break;
                case Key.NoName:
                    result = "NoName";
                    break;
                case Key.Pa1:
                    result = "Pa1";
                    break;
                case Key.OemClear:
                    result = "OemClear";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("key");
            }

            return result;
        }
    }
}