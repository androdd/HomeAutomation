namespace HomeAutomation.Hardware.UI
{
    using System;
    using System.Threading;

    public class Clock : Control, IDisposable
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
            _isStarted = false;
            _timer.Dispose();
        }

        public override void Setup()
        {
            _timePicker.Setup();

            base.Setup();
        }

        public void Edit()
        {
            if (!IsVisible || SetTime == null || GetTime == null)
            {
                return;
            }

            Stop();
            _timePicker.Value = GetTime();
            _timePicker.Show(Col, Row);
            _timePicker.Focus();
            _timePicker.KeyPressed += TimePickerOnKeyPressed;
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
            Start();
        }

        public override void Show(int col, int row)
        {
            base.Show(col, row);

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
