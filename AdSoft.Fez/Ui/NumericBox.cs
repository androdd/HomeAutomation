namespace AdSoft.Fez.Ui
{
    using System;

    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    public class NumericBox : Control
    {
        private int _digitIndex;
        
        private int _minValue;
        private int _maxValue;
        private bool _exitLeft;
        private bool _exitRight;

        public int MinValue
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                MaxLength = GetLength();
            }
        }

        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                MaxLength = GetLength();
            }
        }

        public int Value { get; set; }
        
        public NumericBox(string name, Lcd2004 screen, IKeyboard keyboard) : base(name, screen, keyboard)
        {
        }

        public void Setup(int col, int row, int minValue, int maxValue, bool exitLeft = false, bool exitRight = false)
        {
            if (minValue >= maxValue || minValue < 0)
            {
                throw new ArgumentOutOfRangeException("minValue", "should be greater than zero and less than maxValue");
            }

            MinValue = minValue;
            MaxValue = maxValue;

            _exitLeft = exitLeft;
            _exitRight = exitRight;

            base.Setup(col, row);
        }

        public override void Show()
        {
            string placeHolder = Value.ToString();
            int valueLength = placeHolder.Length;

            for (int i = 0; i < MaxLength - valueLength; i++)
            {
                placeHolder = " " + placeHolder;
            }

            Screen.Write(Col, Row, placeHolder);

            base.Show();
        }

        public override void Focus()
        {
            ResetCursor();

            base.Focus();
        }
        
        protected override void OnKeyPressed(Key key)
        {
            var pow = Math.Pow(10.0, _digitIndex);
            int digit = (int)(Value / pow % 10);
            int cursorPos = Col + MaxLength - _digitIndex - 1;
            int digitCount = GetDigitCount();

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
                    if (_digitIndex + 1 > digitCount || _digitIndex + 1 == MaxLength)
                    {
                        if (_exitLeft)
                        {
                            OnExitLeft();
                        }

                        break;
                    }

                    cursorPos--;
                    _digitIndex++;
                    Screen.SetCursor(cursorPos, Row);
                    break;
                case Key.RightArrow:
                    if (_digitIndex == 0)
                    {
                        if (_exitRight)
                        {
                            OnExitRight();
                        }

                        break;
                    }

                    cursorPos++;
                    _digitIndex--;
                    Screen.SetCursor(cursorPos, Row);
                    break;
            }

            base.OnKeyPressed(key);
        }

        protected override void OnExitRight()
        {
            Unfocus();
            base.OnExitRight();
        }

        protected override void OnExitLeft()
        {
            Unfocus();
            base.OnExitLeft();
        }

        protected override int GetLength()
        {
            int result = Math.Max(MinValue.ToString().Length, MaxValue.ToString().Length);

            if (MinValue < 0)
            {
                result++;
            }

            return result;
        }

        private void SetAndWriteNewValue(int newValue, int newDigit)
        {
            if (MinValue <= newValue && newValue <= MaxValue)
            {
                Value = newValue;
                Screen.WriteCharAtCursor((byte)(48 + newDigit));
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
            int spaces = MaxLength - value.Length;

            for (int i = 0; i < spaces; i++)
            {
                value = " " + value;
            }

            Screen.Sync(() =>
            {
                Screen.Write(Col, Row, value);
                ResetCursor();
            });
        }

        private void ResetCursor()
        {
            Screen.SetCursor(Col + MaxLength - 1, Row, true);
            _digitIndex = 0;
        }

        private int GetDigitCount()
        {
            int value = Value;

            if (value == 0)
            {
                return 1;
            }

            int count = 0;
            while (value != 0)
            {
                value = value / 10;
                ++count;
            }
            return count;
        }
    }
}
