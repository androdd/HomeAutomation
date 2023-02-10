namespace AdSoft.Hardware.UI
{
    using System;

    public class NumericBox
    {
        private readonly Lcd2004 _screen;
        private readonly IKeyboard _keyboard;

        private byte _valueMaxLength;
        private byte _length;
        private int _minValue;
        private int _maxValue;

        public string Title { get; set; }

        public int MinValue
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                _valueMaxLength = GetValueMaxLength();
                _length = (byte)(Title.Length + _valueMaxLength);
            }
        }

        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                _valueMaxLength = GetValueMaxLength();
                _length = (byte)(Title.Length + _valueMaxLength);
            }
        }

        public byte Col { get; set; }
        public byte Row { get; set; }
        public int Value { get; set; }

        public NumericBox(Lcd2004 screen, IKeyboard keyboard)
        {
            _screen = screen;
            _keyboard = keyboard;
        }

        public void Setup(string title, int minValue, int maxValue)
        {
            Title = title;
            MinValue = minValue;
            MaxValue = maxValue;
            _valueMaxLength = GetValueMaxLength();
            _length = (byte)(Title.Length + _valueMaxLength);
        }

        public void Show(byte col, byte row)
        {
            string placeHolder = Value.ToString();
            int valueLength = placeHolder.Length;

            for (int i = 0; i < _valueMaxLength - valueLength; i++)
            {
                placeHolder = " " + placeHolder;
            }

            _screen.Write(col, row, Title + placeHolder);

            Col = col;
            Row = row;
        }

        public void Hide()
        {
            string placeHolder = "";
            for (int i = 0; i < _length; i++)
            {
                placeHolder += " ";
            }

            _screen.Write(Col, Row, placeHolder);
        }

        public void Focus()
        {
            _screen.SetCursor((byte)(Col + _length - 1), Row, true);

            _keyboard.OnButtonPress += KeyboardOnOnButtonPress;
        }

        private byte _digitIndex = 0;
        
        private void KeyboardOnOnButtonPress(Key key)
        {
            var pow = Math.Pow(10.0, _digitIndex);
            byte digit = (byte)(Value / pow % 10);
            byte cursorPos = (byte)(Col + _length - _digitIndex - 1);
            byte digitCount = GetDigitCount(Value);

            switch (key)
            {
                case Key.UpArrow:
                    if (digit == 9)
                    {
                        digit = 0;
                        Value -= (int)(9 * pow);
                    }
                    else
                    {
                        digit = (byte)((digit + 1) % 10);
                        Value += (int)pow;
                    }

                    _screen.WriteCharAtCursor((byte)(48 + digit));
                    break;
                case Key.DownArrow:
                    if (digit == 0)
                    {
                        digit = 9;
                        Value += (int)(9 * pow);
                    }
                    else
                    {
                        digit = (byte)((digit - 1) % 10);
                        Value -= (int)pow;
                    }

                    _screen.WriteCharAtCursor((byte)(48 + digit));
                    break;
                case Key.LeftArrow:
                    if (_digitIndex + 1 > digitCount)
                    {
                        return;
                    }

                    cursorPos--;
                    _digitIndex++;
                    _screen.SetCursor(cursorPos, Row);
                    break;
                case Key.RightArrow:
                    if (_digitIndex == 0)
                    {
                        break;
                    }

                    cursorPos++;
                    _digitIndex--;
                    _screen.SetCursor(cursorPos, Row);
                    break;
            }
            
            
            _screen.WriteAndReturnCursor(0, 3, (Value + "      ").Substring(0, 6));
        }

        public void Unfocus()
        {
            _screen.CursorOff();

            _keyboard.OnButtonPress -= KeyboardOnOnButtonPress;
        }

        private byte GetValueMaxLength()
        {
            byte result = (byte)Math.Max(MinValue.ToString().Length, MaxValue.ToString().Length);

            if (MinValue < 0)
            {
                result++;
            }

            return result;
        }

        static byte GetDigitCount(int n)
        {
            byte count = 0;
            while (n != 0)
            {
                n = n / 10;
                ++count;
            }
            return count;
        }
    }
}
