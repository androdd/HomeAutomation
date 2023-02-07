namespace AdSoft.Hardware.UI
{
    using AdSoft.Hardware;

    public class Menu
    {
        private readonly Lcd2004 _lcd2004;
        private readonly IKeyboard _keyboard;
        private string[] _menuItems;
        private byte _lastSelectedRow;

        public Menu(Lcd2004 lcd2004, IKeyboard keyboard)
        {
            _lcd2004 = lcd2004;
            _keyboard = keyboard;

            _lastSelectedRow = byte.MaxValue;
        }

        public void Create(string[] menuItems)
        {
            _menuItems = menuItems;

            _keyboard.OnButtonPress += KeyboardOnOnButtonPress; 
        }

        private void KeyboardOnOnButtonPress(Key key)
        {
            switch (key)
            {
                case Key.UpArrow:
                    if (_lastSelectedRow != 0)
                    {
                        Select((byte)(_lastSelectedRow - 1));
                    }
                    break;
                case Key.DownArrow:
                    if (_lastSelectedRow != 3)
                    {
                        Select((byte)(_lastSelectedRow + 1));
                    }
                    break;
            }   
        }

        public void Show()
        {
            var showItems = _menuItems.Length;
            if (showItems > 4)
            {
                showItems = 4;
            }

            for (byte i = 0; i < showItems; i++)
            {
                _lcd2004.Write(0, i, " " + _menuItems[i]);
            }
        }

        public void Select(byte row)
        {
            if (_lastSelectedRow != byte.MaxValue)
            {
                _lcd2004.Write(0, _lastSelectedRow, " ");
            }

            _lcd2004.Write(0, row, new string((char)0x7E, 1));

            _lastSelectedRow = row;
        }
    }
}
