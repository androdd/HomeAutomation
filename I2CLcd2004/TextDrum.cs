namespace I2CLcd2004
{
    using System.Runtime.CompilerServices;

    public class TextDrum
    {
        private readonly Lcd2004 _lcd2004;
        private readonly byte _startCol;
        private readonly byte _startRow;
        private byte _nextRow;

        public TextDrum(Lcd2004 lcd2004, byte startCol, byte startRow)
        {
            if (startRow > 2)
            {
                startRow = 2;
            }

            if (startCol > 19)
            {
                startCol = 19;
            }

            _lcd2004 = lcd2004;
            _startCol = startCol;
            _startRow = startRow;
            _nextRow = startRow;
        }

        public void Write(string text)
        {
            if (text.Length + _startCol > 20)
            {
                text = text.Substring(0, 20 - _startCol);
            }
            _lcd2004.Write(_startCol, _nextRow, text);

            if (_nextRow == 3)
            {
                _nextRow = _startRow;
            }
            else
            {
                _nextRow++;
            }
        }
    }
}
