namespace AdSoft.Hardware.UI
{
    using System;
    using System.Threading;

    using Microsoft.SPOT.Hardware;

    public class Clock : Control, IDisposable
    {
        private Timer _timer;
        private readonly TimePicker _timePicker;

        public Clock(Lcd2004 screen, IKeyboard keyboard) : base(screen, keyboard)
        {
            _timePicker = new TimePicker(screen, keyboard);
        }

        public void Start()
        {
            _timer = new Timer(state =>
                {
                    var time = DateTime.Now.ToString("HH:mm"); // 5 bytes
                    Screen.WriteAndReturnCursor(Col, Row, time);
                },
                null,
                0,
                30 * 1000);
        }

        public void Stop()
        {
            _timer.Dispose();
        }

        public override void Setup()
        {
            _timePicker.Setup();
            _timePicker.Value = DateTime.Now;
            
            base.Setup();
        }

        public void Edit()
        {
            if (!IsVisible)
            {
                return;
            }

            Stop();
            _timePicker.Show(Col, Row);
            _timePicker.Focus();
            _timePicker.KeyPressed += TimePickerOnKeyPressed;
        }

        private void TimePickerOnKeyPressed(Key key)
        {
            switch (key)
            {
                case Key.Enter:
                    Utility.SetLocalTime(_timePicker.Value);
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
