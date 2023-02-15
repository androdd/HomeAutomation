namespace AdSoft.Hardware.UI
{
    using System;

    public class NumericBox
    {
        private readonly Lcd2004 _screen;
        private readonly IKeyboard _keyboard;

        private int _digitIndex;

        private int _valueMaxLength;
        private int _minValue;
        private int _maxValue;
        
        public int MinValue
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                _valueMaxLength = GetValueMaxLength();
            }
        }

        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                _valueMaxLength = GetValueMaxLength();
            }
        }

        public int Col { get; set; }
        public int Row { get; set; }
        public int Value { get; set; }

        public NumericBox(Lcd2004 screen, IKeyboard keyboard)
        {
            _screen = screen;
            _keyboard = keyboard;
        }

        public void Setup(int minValue, int maxValue)
        {
            if (minValue >= maxValue || minValue < 0)
            {
                throw new ArgumentOutOfRangeException("minValue", "should be greater than zero and less than maxValue");
            }

            MinValue = minValue;
            MaxValue = maxValue;
            _valueMaxLength = GetValueMaxLength();
        }

        public void Show(int col, int row)
        {
            string placeHolder = Value.ToString();
            int valueLength = placeHolder.Length;

            for (int i = 0; i < _valueMaxLength - valueLength; i++)
            {
                placeHolder = " " + placeHolder;
            }

            _screen.Write(col, row, placeHolder);

            Col = col;
            Row = row;
        }

        public void Hide()
        {
            string placeHolder = "";
            for (int i = 0; i < _valueMaxLength; i++)
            {
                placeHolder += " ";
            }

            _screen.Write(Col, Row, placeHolder);
        }

        public void Focus()
        {
            ResetCursor();

            _keyboard.OnButtonPress += KeyboardOnOnButtonPress;
        }
        
        private void KeyboardOnOnButtonPress(Key key)
        {
            var pow = Math.Pow(10.0, _digitIndex);
            int digit = (int)(Value / pow % 10);
            int cursorPos = Col + _valueMaxLength - _digitIndex - 1;
            int digitCount = GetDigitCount(Value);

            int newDigit;
            int newValue = Value;

            switch (key)
            {
                case Key.UpArrow:
                    if (digit == 9)
                    {
                        newDigit = 0;
                        newValue -= (int)(9 * pow);
                    }
                    else
                    {
                        newDigit = (digit + 1) % 10;
                        newValue += (int)pow;
                    }

                    SetAndWriteNewValue(newValue, newDigit);
                    break;
                case Key.DownArrow:
                    if (digit == 0)
                    {
                        newDigit = 9;
                        newValue += (int)(9 * pow);
                    }
                    else
                    {
                        newDigit = (digit - 1) % 10;
                        newValue -= (int)pow;
                    }

                    SetAndWriteNewValue(newValue, newDigit);
                    break;
                case Key.LeftArrow:
                    if (_digitIndex + 1 > digitCount || _digitIndex + 1 == _valueMaxLength)
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

        private void SetAndWriteNewValue(int newValue, int newDigit)
        {
            if (MinValue <= newValue && newValue <= MaxValue)
            {
                Value = newValue;
                _screen.WriteCharAtCursor((byte)(48 + newDigit));
            }
            else
            {
                if (newValue > MaxValue)
                {
                    Value = MaxValue;
                    WriteValueAndResetCursor();
                }
                else if (newValue < MinValue)
                {
                    Value = MinValue;
                    WriteValueAndResetCursor();
                }
            }
        }

        private void WriteValueAndResetCursor()
        {
            string value = Value.ToString();
            int spaces = _valueMaxLength - value.Length;

            for (int i = 0; i < spaces; i++)
            {
                value = " " + value;
            }

            _screen.Write(Col, Row, value);
            ResetCursor();
        }

        private void ResetCursor()
        {
            _screen.SetCursor(Col + _valueMaxLength - 1, Row, true);
            _digitIndex = 0;
        }

        private int GetValueMaxLength()
        {
            int result = Math.Max(MinValue.ToString().Length, MaxValue.ToString().Length);

            if (MinValue < 0)
            {
                result++;
            }

            return result;
        }

        static int GetDigitCount(int n)
        {
            int count = 0;
            while (n != 0)
            {
                n = n / 10;
                ++count;
            }
            return count;
        }
    }
}
