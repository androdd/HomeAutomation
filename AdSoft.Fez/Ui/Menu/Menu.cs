namespace AdSoft.Fez.Ui.Menu
{
    using AdSoft.Fez;
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    using Microsoft.SPOT;

    public class Menu : Control
    {
        private MenuItem[] _menuItems;
        private int _rows;
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
            Setup(menuItems, Screen.Rows);
        }

        public void Setup(MenuItem[] menuItems, int rows) 
        {
            if (menuItems == null)
            {
                return;
            }

            if (rows < 1 || rows > Screen.Rows)
            {
                rows = Screen.Rows;
            }

            _menuItems = menuItems;
            _rows = rows;
            
            base.Setup(0, 0);
        }

        public override void Show(bool show = true)
        {
            if (show)
            {
                _firstItemIndex = 0;

                _itemsOnScreen = MenuItemsCount;

                ShowNext();

                base.Show(show);
            }
            else
            {
                Screen.Clear();

                Unfocus();

                IsVisible = false;

#if DEBUG_UI
                Debug.Print("UI - " + Name + ".Hide");
#endif
            }
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
            if (_itemsOnScreen > _rows)
            {
                _itemsOnScreen = _rows;
            }

            Screen.Sync(() =>
            {
                Screen.Clear();

                for (int i = _firstItemIndex; i < _firstItemIndex + _itemsOnScreen; i++)
                {
                    Screen.WriteLine(i - _firstItemIndex, " " + _menuItems[i].Title, true);
                }
            });
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

                        _firstItemIndex -= _rows;
                        _itemsOnScreen = _rows;

                        ShowNext();
                        Select(_rows - 1);
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
                    Show(false);
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

        public void ChangeTitle(byte key, string newTitle)
        {
            foreach (var menuItem in _menuItems)
            {
                if (menuItem.Key != key)
                {
                    continue;
                }

                menuItem.Title = newTitle;
                break;
            }
        }
    }
}
