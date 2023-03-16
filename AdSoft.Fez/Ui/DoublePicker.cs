namespace AdSoft.Fez.Ui
{
    using System;

    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    public class DoublePicker : Control
    {
        private readonly NumericBox _wholeBox;
        private readonly NumericBox _decimalBox;

        private int _decimalDigits;
        private double _pow10;

        public double Value
        {
            get
            {
                return _wholeBox.Value + _decimalBox.Value / _pow10;
            }
            set
            {
                _wholeBox.Value = (int)value;
                _decimalBox.Value = (int)((value - (int)value) * _pow10);
            }
        }

        public DoublePicker(string name, Lcd2004 screen, IKeyboard keyboard) 
            : base(name, screen, keyboard)
        {
            _wholeBox = new NumericBox(name + "_WB", Screen, Keyboard);
            _decimalBox = new NumericBox(name + "_DB", Screen, Keyboard);
        }

        public void Setup(int decimalDigits, int minValue, int maxValue, int col, int row)
        {
            if (decimalDigits > 10)
            {
                decimalDigits = 10;
            }

            if (decimalDigits < 1)
            {
                decimalDigits = 1;
            }

            _decimalDigits = decimalDigits;
            _pow10 = Math.Pow(10, _decimalDigits);

            _wholeBox.Setup(col, row, minValue, maxValue - 1, exitRight: true);
            
            _decimalBox.Setup(col + _wholeBox.MaxLength + 1, row, 0, (int)_pow10 - 1, exitLeft: true);

            _wholeBox.ExitRight += () => _decimalBox.Focus();
            _decimalBox.ExitLeft += () => _wholeBox.Focus();

            base.Setup(col, row);
        }

        public override void Show(bool show = true)
        {
            _wholeBox.Show();

            Screen.Write(Col + _wholeBox.MaxLength, Row, ".");

            _decimalBox.Show();

            base.Show(show);
        }

        public override void Focus()
        {
            _wholeBox.Focus();

            base.Focus();
        }

        public override void Unfocus()
        {
            _wholeBox.Unfocus();
            _decimalBox.Unfocus();

            base.Unfocus();
        }

        protected override int GetLength()
        {
            return _wholeBox.MaxLength + _decimalDigits + 1;
        }
    }
}