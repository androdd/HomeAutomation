namespace AdSoft.Hardware.UI
{
    public class Menu
    {
        private readonly Lcd2004 _screen;
        private readonly IKeyboard _keyboard;
        private MenuItem[] _menuItems;
        private byte _lastSelectedRow;
        private byte _itemsOnScreen;
        private byte _firstItemIndex;
        private byte _width;
        private bool _isVisible;

        private byte MenuItemsCount
        {
            get { return (byte)_menuItems.Length; }
        }

        private byte CurrentItemKey
        {
            get { return _menuItems[_firstItemIndex + _lastSelectedRow].Key; }
        }

        public delegate void MenuItemEventHandler(int key);

        public event MenuItemEventHandler OnMenuItemEnter;
        public event MenuItemEventHandler OnMenuItemSelected;

        public Menu(Lcd2004 screen, IKeyboard keyboard)
        {
            _screen = screen;
            _keyboard = keyboard;

            _lastSelectedRow = byte.MaxValue;
        }

        public void Create(MenuItem[] menuItems)
        {
            if (menuItems == null)
            {
                return;
            }

            _menuItems = menuItems;

            foreach (var item in _menuItems)
            {
                if (item.Title.Length > _width)
                {
                    _width = (byte)item.Title.Length;
                }

                if (_width >= _screen.Cols - 1)
                {
                    break;
                }
            }
        }

        public void Show()
        {
            if (_isVisible)
            {
                return;
            }

            _firstItemIndex = 0;

            _itemsOnScreen = MenuItemsCount;

            ShowNext();
            Select(0);

            _keyboard.OnButtonPress += KeyboardOnOnButtonPress;

            _isVisible = true;
        }

        public void Hide()
        {
            if (!_isVisible)
            {
                return;
            }

            _screen.Clear(0, 0, (byte)(_width + 1), (byte)(_screen.Rows - 1));

            _keyboard.OnButtonPress -= KeyboardOnOnButtonPress;

            _isVisible = false;
        }

        public void Select(byte row)
        {
            if (_lastSelectedRow != byte.MaxValue)
            {
                _screen.Write(0, _lastSelectedRow, " ");
            }

            _screen.Write(0, row, new string((char)0x7E, 1));

            _lastSelectedRow = row;

            if (OnMenuItemSelected != null)
            {
                OnMenuItemSelected(CurrentItemKey);
            }
        }

        private void ShowNext()
        {
            if (_itemsOnScreen > _screen.Rows)
            {
                _itemsOnScreen = _screen.Rows;
            }

            _screen.Clear(0, 0, (byte)(_width + 1), (byte)(_screen.Rows - 1));

            for (byte i = _firstItemIndex; i < _firstItemIndex + _itemsOnScreen; i++)
            {
                _screen.Write(0, (byte)(i - _firstItemIndex), " " + _menuItems[i].Title);
            }
        }

        private void KeyboardOnOnButtonPress(Key key)
        {
            switch (key)
            {
                case Key.UpArrow:
                    if (_lastSelectedRow != 0)
                    {
                        Select((byte)(_lastSelectedRow - 1));
                        break;
                    }

                    if (_firstItemIndex != 0)
                    {
                        // There are more items up

                        _firstItemIndex -= _screen.Rows;
                        _itemsOnScreen = _screen.Rows;

                        ShowNext();
                        Select((byte)(_screen.Rows - 1));
                    }
                    break;
                case Key.DownArrow:
                    if (_lastSelectedRow != _itemsOnScreen - 1)
                    {
                        Select((byte)(_lastSelectedRow + 1));
                        break;
                    }

                    if (_firstItemIndex + _itemsOnScreen < MenuItemsCount)
                    {
                        // There are more items down

                        _firstItemIndex = (byte)(_firstItemIndex + _itemsOnScreen);
                        _itemsOnScreen = (byte)(MenuItemsCount - _firstItemIndex);

                        ShowNext();
                        Select(0);
                    }

                    break;
                case Key.Enter:
                    if (OnMenuItemEnter != null)
                    {
                        OnMenuItemEnter(CurrentItemKey);
                    }
                    break;
                case Key.Escape:
                    Hide();
                    break;
            }
        }
    }
}
