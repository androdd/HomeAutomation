namespace HomeAutomation.Hardware.UI
{
    using System;

    public class TimePicker : Control
    {
        private readonly NumericBox _hourBox;
        private readonly NumericBox _minuteBox;

        public DateTime Value
        {
            get
            {
                return new DateTime(2020, 1, 1, _hourBox.Value, _minuteBox.Value, 0);
            }
            set
            {
                _hourBox.Value = value.Hour;
                _minuteBox.Value = value.Minute;
            }
        }

        public TimePicker(string name, Lcd2004 screen, IKeyboard keyboard) : base(name, screen, keyboard)
        {
            _hourBox = new NumericBox(name + "_HB", Screen, Keyboard);
            _minuteBox = new NumericBox(name + "_MB", Screen, Keyboard);
        }

        public override void Setup()
        {
            _hourBox.Setup(0, 23, exitRight: true);

            _minuteBox.Setup(0, 59, exitLeft: true);

            _hourBox.ExitRight += () => _minuteBox.Focus();
            _minuteBox.ExitLeft += () => _hourBox.Focus();

            base.Setup();
        }

        public override void Show(int col, int row)
        {
            _hourBox.Show(col, row);

            Screen.Write(col + 2, row, ":");

            _minuteBox.Show(col + 3, row);

            base.Show(col, row);
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
