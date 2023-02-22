namespace AdSoft.Fez.Ui.Menu
{
    using AdSoft.Fez;
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    public class Menu : Control
    {
        private MenuItem[] _menuItems;
        private int _lastSelectedRow;
        private int _itemsOnScreen;
        private int _firstItemIndex;

        private int MenuItemsCount
        {
            get { return _menuItems.Length; }
        }

        private byte CurrentItemKey
        {
            get { return _menuItems[_firstItemIndex + _lastSelectedRow].Key; }
        }

        public delegate void MenuItemEventHandler(byte key);

        public event MenuItemEventHandler MenuItemEnter;
        public event MenuItemEventHandler MenuItemSelected;

        public Menu(string name, Lcd2004 screen, IKeyboard keyboard) : base(name, screen, keyboard)
        {
            _lastSelectedRow = int.MaxValue;
        }

        public void Setup(MenuItem[] menuItems) 
        {
            if (menuItems == null)
            {
                return;
            }

            _menuItems = menuItems;
            
            base.Setup(0, 0);
        }

        public override void Show()
        {
            _firstItemIndex = 0;

            _itemsOnScreen = MenuItemsCount;

            ShowNext();

            base.Show();
        }

        public override void Hide()
        {
            Screen.Clear(0, 0, MaxLength, Screen.Rows - 1);
            
            Unfocus();

            IsVisible = false;

            DebugEx.UiPrint(Name, "Hide");
        }

        public override void Focus()
        {
            Select(0);

            base.Focus();
        }

        public void Select(int row)
        {
            if (_lastSelectedRow != int.MaxValue)
            {
                Screen.Write(0, _lastSelectedRow, " ");
            }

            Screen.Write(0, row, new string((char)0x7E, 1));

            _lastSelectedRow = row;

            if (MenuItemSelected != null)
            {
                MenuItemSelected(CurrentItemKey);
            }
        }

        private void ShowNext()
        {
            if (_itemsOnScreen > Screen.Rows)
            {
                _itemsOnScreen = Screen.Rows;
            }

            Screen.Clear(0, 0, MaxLength, Screen.Rows - 1);

            for (int i = _firstItemIndex; i < _firstItemIndex + _itemsOnScreen; i++)
            {
                Screen.Write(0, i - _firstItemIndex, " " + _menuItems[i].Title);
            }
        }

        protected override void OnKeyPressed(Key key)
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

                        _firstItemIndex -= Screen.Rows;
                        _itemsOnScreen = Screen.Rows;

                        ShowNext();
                        Select((Screen.Rows - 1));
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
                    if (MenuItemEnter != null)
                    {
                        MenuItemEnter(CurrentItemKey);
                    }
                    break;
                case Key.Escape:
                    Hide();
                    break;
            }

            base.OnKeyPressed(key);
        }
        
        protected override int GetLength()
        {
            var result = 0;

            foreach (var item in _menuItems)
            {
                if (item.Title.Length > result)
                {
                    result = item.Title.Length;
                }

                if (result >= Screen.Cols - 1)
                {
                    break;
                }
            }

            return result + 1;
        }
    }
}
