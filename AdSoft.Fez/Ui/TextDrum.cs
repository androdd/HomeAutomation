namespace AdSoft.Fez.Ui
{
    using System.Threading;

    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    using Microsoft.SPOT;

    public class TextDrum : Control
    {
        private readonly AutoResetEvent _resetEvent;

        private int _width;
        private int _height;
        private int _lastRow;
        private int _nextRow;
        private Thread _thread;

        private string _spaces;

        public TextDrum(string name, Lcd2004 screen, IKeyboard keyboard)
            : base(name, screen, keyboard)
        {
            _resetEvent = new AutoResetEvent(false);
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

            _spaces = "";
            for (int i = 0; i < width; i++)
            {
                _spaces += " ";
            }

            base.Setup(col, row);
        }

        public void Write(string text)
        {
            text += _spaces;
            text = text.Substring(0, _width - 1);
            
            Screen.Sync(() =>
            {
                int cursorCol, cursorRow;
                Screen.GetCursor(out cursorCol, out cursorRow);

                if (_lastRow != _nextRow)
                {
                    Screen.Write(Col, _lastRow, " ");
                }

                Screen.Write(Col, _nextRow, (char)0xA5 + text); // dot in the middle

                Screen.SetCursor(cursorCol, cursorRow);
            });

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

        public void WriteInfinite(int millisecondsInterval, StringFunctionEventHandler getText)
        {
            if (_thread != null)
            {
                _resetEvent.Set();
            }

            if (getText == null)
            {
                return;
            }

            _thread = new Thread(() =>
            {
                while (!_resetEvent.WaitOne(millisecondsInterval, false))
                {
                    Write(getText());
                }
            });
            _thread.Start();
        }

        public void StopWriteInfinite()
        {
            if (_thread == null)
            {
                return;
            }

            _resetEvent.Set();
            _thread = null;
        }

        public override void Show(bool show = true)
        {
            Screen.Clear(Col, Row, Col + _width - 1, Row + _height - 1);

            if (show)
            {
                base.Show(show);
            }
            else
            {
                StopWriteInfinite();

                DebugEx.Print(DebugEx.Target.Ui, Name + ".Hide");

                IsVisible = false;
            }
        }

        public override void Focus()
        {
        }

        public override void Unfocus()
        {
        }

        protected override int GetLength()
        {
            return _width;
        }
    }
}
