namespace AdSoft.Hardware.UI
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
                return new DateTime(0, 0, 0, _hourBox.Value, _minuteBox.Value, 0);
            }
            set
            {
                _hourBox.Value = value.Hour;
                _minuteBox.Value = value.Minute;
            }
        }

        public TimePicker(Lcd2004 screen, IKeyboard keyboard) : base(screen, keyboard)
        {
            _hourBox = new NumericBox(Screen, Keyboard);
            _minuteBox = new NumericBox(Screen, Keyboard);
        }

        public override void Setup()
        {
            _hourBox.Setup(0, 23);

            _minuteBox.Setup(0, 59);

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

        protected override int GetLength()
        {
            return 5;
        }
    }
}
