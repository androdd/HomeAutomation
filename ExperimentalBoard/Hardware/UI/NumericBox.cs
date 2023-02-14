namespace AdSoft.Hardware.UI
{
    using System;

    public class NumericBox
    {
        private readonly Lcd2004 _screen;
        private readonly IKeyboard _keyboard;

        private int _valueMaxLength;
        private int _length;
        private int _titleLength;
        private int _minValue;
        private int _maxValue;
        private bool _allowNegative;

        public string Title { get; set; }

        public int MinValue
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                _valueMaxLength = GetValueMaxLength();
                _length = Title.Length + _valueMaxLength;
            }
        }

        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                _valueMaxLength = GetValueMaxLength();
                _length = Title.Length + _valueMaxLength;
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

        public void Setup(string title, int minValue, int maxValue)
        {
            Title = title;
            MinValue = minValue;
            MaxValue = maxValue;
            _valueMaxLength = GetValueMaxLength();
            _titleLength = Title.Length;
            _length = _titleLength + _valueMaxLength;
            _allowNegative = minValue < 0;
        }

        public void Show(int col, int row)
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
            _screen.SetCursor(Col + _length - 1, Row, true);

            _keyboard.OnButtonPress += KeyboardOnOnButtonPress;
        }

        private int _digitIndex = 0;
        
        private void KeyboardOnOnButtonPress(Key key)
        {
            var pow = Math.Pow(10.0, _digitIndex);
            int sign = Value < 0 ? -1 : 1;
            int digit = (int)(sign * Value / pow % 10);
            int cursorPos = Col + _length - _digitIndex - 1;
            int digitCount = GetDigitCount(Value);

            int newDigit;
            int newValue = Value;

            switch (key)
            {
                case Key.UpArrow:
                    if (cursorPos == Col + _titleLength && _allowNegative)
                    {
                        if (Value < 0)
                        {
                            newDigit = -16; // " "
                        }
                        else
                        {
                            newDigit = -3; // "-"
                        }

                        newValue *= -1;
                    }
                    else
                    {
                        if (digit == 9)
                        {
                            newDigit = 0;
                            newValue -= sign * (int)(9 * pow);
                        }
                        else
                        {
                            newDigit = (digit + 1) % 10;
                            newValue += sign * (int)pow;
                        }
                    }

                    SetAndWriteNewValue(newValue, newDigit);
                    break;
                case Key.DownArrow:
                    if (cursorPos == Col + _titleLength && _allowNegative)
                    {
                        if (Value < 0)
                        {
                            newDigit = -16; // " "
                        }
                        else
                        {
                            newDigit = -3; // "-"
                        }

                        newValue *= -1;
                    }
                    else
                    {
                        if (digit == 0)
                        {
                            newDigit = 9;
                            newValue += sign * (int)(9 * pow);
                        }
                        else
                        {
                            newDigit = (digit - 1) % 10;
                            newValue -= sign * (int)pow;
                        }    
                    }

                    SetAndWriteNewValue(newValue, newDigit);
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
                    WriteValueAndReturnCursor();
                }
                else if (newValue < MinValue)
                {
                    Value = MinValue;
                    WriteValueAndReturnCursor();
                }
            }
        }

        private void WriteValueAndReturnCursor()
        {
            int newValue;
            string maxValue = "";

            if (Value < 0)
            {
                maxValue += "-";
                newValue = -1 * Value;
            }
            else
            {
                maxValue += " ";
                newValue = Value;
            }

            string positiveString = newValue.ToString();
            int spaces = _valueMaxLength - positiveString.Length - 1;

            for (int i = 0; i < spaces; i++)
            {
                maxValue += " ";
            }

            maxValue += positiveString;

            _screen.WriteAndReturnCursor(Col + _titleLength, Row, maxValue);
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
