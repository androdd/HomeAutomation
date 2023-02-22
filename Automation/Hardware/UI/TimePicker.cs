namespace HomeAutomation.Hardware.UI
{
    using System;

    public class TimePicker : Control
    {
        private readonly NumericBox _hourBox;
        private readonly NumericBox _minuteBox;

        private DateTime _value;

        public DateTime Value
        {
            get
            {
                _value = new DateTime(_value.Year, _value.Month, _value.Day, _hourBox.Value, _minuteBox.Value, 0);
                return _value;
            }
            set
            {
                _value = new DateTime(_value.Year, _value.Month, _value.Day, value.Hour, value.Minute, 0);
                _hourBox.Value = _value.Hour;
                _minuteBox.Value = _value.Minute;
            }
        }

        public TimePicker(string name, Lcd2004 screen, IKeyboard keyboard) : base(name, screen, keyboard)
        {
            _hourBox = new NumericBox(name + "_HB", Screen, Keyboard);
            _minuteBox = new NumericBox(name + "_MB", Screen, Keyboard);
        }

        public override void Setup(int col, int row)
        {
            _hourBox.Setup(col, row, 0, 23, exitRight: true);

            _minuteBox.Setup(col + 3, row, 0, 59, exitLeft: true);

            _hourBox.ExitRight += () => _minuteBox.Focus();
            _minuteBox.ExitLeft += () => _hourBox.Focus();

            base.Setup(col, row);
        }

        public override void Show()
        {
            _hourBox.Show();

            Screen.Write(Col + 2, Row, ":");

            _minuteBox.Show();

            base.Show();
        }

        public override void Focus()
        {
            _hourBox.Focus();

            base.Focus();
        }

        public override void Unfocus()
        {
            _hourBox.Unfocus();
            _minuteBox.Unfocus();

            base.Unfocus();
        }

        protected override int GetLength()
        {
            return 5;
        }
    }
}
