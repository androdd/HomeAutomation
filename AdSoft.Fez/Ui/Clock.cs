namespace AdSoft.Fez.Ui
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

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
            _timer = new Timer(state => { WriteTime(); },
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
                    if (SetTime != null && GetTime != null)
                    {
                        var now = GetTime();

                        var newDateTime = new DateTime(now.Year, now.Month, now.Day, _timePicker.Value.Hour, _timePicker.Value.Minute, 0);

                        SetTime(newDateTime);
                    }
                    break;
                case Key.Escape:
                    break;
                default:
                    return;
            }

            _timePicker.Show(false);
            IsFocused = false;
            Start();
        }

        public override void Show(bool show = true)
        {
            if (show)
            {
                WriteTime();

                Start();
            }
            else
            {
                Stop();    
            }

            base.Show(show);
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

        private void WriteTime()
        {
            if (GetTime == null)
            {
                return;
            }

            var time = GetTime().ToString("HH:mm"); // 5 bytes
            Screen.WriteAndReturnCursor(Col, Row, time);
        }
    }
}
