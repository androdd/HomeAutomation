namespace AdSoft.Fez.Ui
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    public class Clock : Control, IDisposable, IUiWorker
    {
        private Timer _timer;
        private readonly TimePicker _timePicker;
        private bool _isStarted;

        public delegate DateTime GetTimeEventHandler();
        public delegate void SetTimeEventHandler(DateTime time);

        public event GetTimeEventHandler GetTime;
        public event SetTimeEventHandler SetTime;

        public Clock(string name, Lcd2004 screen, IKeyboard keyboard) : base(name, screen, keyboard)
        {
            _timePicker = new TimePicker(name + "_TP", screen, keyboard);
        }

        public void Start()
        {
            if (_isStarted || GetTime == null)
            {
                return;
            }

            _isStarted = true;
            _timer = new Timer(state =>
                {
                    var time = GetTime().ToString("HH:mm"); // 5 bytes
                    Screen.WriteAndReturnCursor(Col, Row, time);
                },
                null,
                0,
                30 * 1000);
        }

        public void Stop()
        {
            if (!_isStarted)
            {
                return;
            }

            _isStarted = false;
            _timer.Dispose();
        }

        public new void Setup(int col, int row)
        {
            _timePicker.Setup(col, row);

            base.Setup(col, row);
        }

        public void Edit()
        {
            if (!IsVisible || SetTime == null || GetTime == null)
            {
                return;
            }

            Stop();
            _timePicker.Value = GetTime();
            _timePicker.Show();
            _timePicker.Focus();
            _timePicker.KeyPressed += TimePickerOnKeyPressed;

            IsFocused = true;
        }

        private void TimePickerOnKeyPressed(Key key)
        {
            switch (key)
            {
                case Key.Enter:
                    if (SetTime != null)
                    {
                        SetTime(_timePicker.Value);
                    }
                    break;
                case Key.Escape:
                    break;
                default:
                    return;
            }

            _timePicker.Hide();
            IsFocused = false;
            Start();
        }

        public override void Show()
        {
            base.Show();

            Start();
        }

        public override void Hide()
        {
            Stop();

            base.Hide();
        }

        public override void Focus()
        {
        }

        protected override int GetLength()
        {
            return 5;
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }
    }
}
