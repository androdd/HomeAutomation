namespace AdSoft.Hardware.UI
{
    public class Menu
    {
        private readonly Lcd2004 _screen;
        private readonly IKeyboard _keyboard;
        private MenuItem[] _menuItems;
        private int _lastSelectedRow;
        private int _itemsOnScreen;
        private int _firstItemIndex;
        private int _width;
        private bool _isVisible;

        private int MenuItemsCount
        {
            get { return _menuItems.Length; }
        }

        private int CurrentItemKey
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

            _lastSelectedRow = int.MaxValue;
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
                    _width = item.Title.Length;
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

            _keyboard.KeyPressed += KeyboardButtonPressed;

            _isVisible = true;
        }

        public void Hide()
        {
            if (!_isVisible)
            {
                return;
            }

            _screen.Clear(0, 0, _width + 1, _screen.Rows - 1);

            _keyboard.KeyPressed -= KeyboardButtonPressed;

            _isVisible = false;
        }

        public void Select(int row)
        {
            if (_lastSelectedRow != int.MaxValue)
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

            _screen.Clear(0, 0, _width + 1, _screen.Rows - 1);

            for (int i = _firstItemIndex; i < _firstItemIndex + _itemsOnScreen; i++)
            {
                _screen.Write(0, i - _firstItemIndex, " " + _menuItems[i].Title);
            }
        }

        private void KeyboardButtonPressed(Key key)
        {
            switch (key)
            {
                case Key.UpArrow:
                    if (_lastSelectedRow != 0)
                    {
                        Select(_lastSelectedRow - 1);
                        break;
                    }

                    if (_firstItemIndex != 0)
                    {
                        // There are more items up

                        _firstItemIndex -= _screen.Rows;
                        _itemsOnScreen = _screen.Rows;

                        ShowNext();
                        Select((_screen.Rows - 1));
                    }
                    break;
                case Key.DownArrow:
                    if (_lastSelectedRow != _itemsOnScreen - 1)
                    {
                        Select(_lastSelectedRow + 1);
                        break;
                    }

                    if (_firstItemIndex + _itemsOnScreen < MenuItemsCount)
                    {
                        // There are more items down

                        _firstItemIndex += _itemsOnScreen;
                        _itemsOnScreen = MenuItemsCount - _firstItemIndex;

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
