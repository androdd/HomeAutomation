namespace AdSoft.Fez.Ui
{
    using System;

    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    public class DatePicker : Control
    {
        private readonly NumericBox _yearBox;
        private readonly NumericBox _monthBox;
        private readonly NumericBox _dayBox;
        
        public DateTime Value
        {
            get
            {
                return new DateTime(_yearBox.Value, _monthBox.Value, _dayBox.Value, 0, 0, 0);
            }
            set
            {
                _yearBox.Value = value.Year;
                _monthBox.Value = value.Month;
                _dayBox.Value = value.Day;
                SetDaysInMonth();
            }
        }

        public DatePicker(string name, Lcd2004 screen, IKeyboard keyboard)
            : base(name, screen, keyboard)
        {
            _yearBox = new NumericBox(name + "_YB", Screen, Keyboard);
            _monthBox = new NumericBox(name + "_MB", Screen, Keyboard);
            _dayBox = new NumericBox(name + "_DB", screen, Keyboard);
        }

        public new void Setup(int col, int row)
        {
            _yearBox.Setup(col, row, 2020, 5000, exitRight: true);
            _monthBox.Setup(col + 5, row, 1, 12, exitLeft: true, exitRight: true);
            _dayBox.Setup(col + 8, row, 1, 31, exitLeft: true);
            
            _yearBox.ExitRight += () => _monthBox.Focus();
            _monthBox.ExitLeft += () => _yearBox.Focus();
            _monthBox.ExitRight += () => _dayBox.Focus();
            _dayBox.ExitLeft += () => _monthBox.Focus();

            _monthBox.KeyPressed += key => { SetDaysInMonth(); };

            base.Setup(col, row);
        }

        public override void Show(bool show = true)
        {
            _yearBox.Show();

            Screen.Write(Col + 4, Row, "-");

            _monthBox.Show();

            Screen.Write(Col + 7, Row, "-");

            _dayBox.Show();

            base.Show(show);
        }

        public override void Focus()
        {
            _dayBox.Focus();

            base.Focus();
        }

        public override void Unfocus()
        {
            _yearBox.Unfocus();
            _monthBox.Unfocus();
            _dayBox.Unfocus();

            base.Unfocus();
        }

        protected override int GetLength()
        {
            return 10;
        }

        private void SetDaysInMonth()
        {
            _dayBox.MaxValue = DateTime.DaysInMonth(_yearBox.Value, _monthBox.Value);
        }
    }
}