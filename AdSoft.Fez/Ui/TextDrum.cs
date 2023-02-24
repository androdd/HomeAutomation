namespace AdSoft.Fez.Ui
{
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    public class TextDrum : Control
    {
        private int _width;
        private int _height;
        private int _lastRow;
        private int _nextRow;

        public TextDrum(string name, Lcd2004 screen, IKeyboard keyboard)
            : base(name, screen, keyboard)
        {
        }

        public void Setup(int col, int row, int width, int height)
        {
            int lastColIndex = Screen.Cols - 2;

            if (col > lastColIndex)
            {
                col = lastColIndex;
            }

            int lastRowsIndex = Screen.Rows - 2;

            if (row > lastRowsIndex)
            {
                row = lastRowsIndex;
            }

            if (col + width > Screen.Cols)
            {
                width = Screen.Cols - col;
            }

            if (row + height > Screen.Rows)
            {
                height = Screen.Rows - row;
            }

            _width = width;
            _height = height;
            _lastRow = row;
            _nextRow = row;

            base.Setup(col, row);
        }

        public void Write(string text)
        {
            if (text.Length > _width - 1)
            {
                text = text.Substring(0, _width - 1);
            }

            int cursorCol, cursorRow;
            Screen.GetCursor(out cursorCol, out cursorRow);

            if (_lastRow != _nextRow)
            {
                Screen.Write(Col, _lastRow, " ");
            }

            Screen.Write(Col, _nextRow, (char)0xA5 + text); // dot in the middle

            Screen.SetCursor(cursorCol, cursorRow);

            _lastRow = _nextRow;

            if (_nextRow == Row + _height - 1)
            {
                _nextRow = Row; 
            }
            else
            {
                _nextRow++;
            }
        }

        protected override int GetLength()
        {
            return _width;
        }
    }
}
