namespace AdSoft.Fez.Ui
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    public class ScreenSaver : IDisposable
    {
        private readonly Lcd2004 _screen;
        private readonly IKeyboard _keyboard;
        private Timer _timer;
        private int _seconds;
        private bool _isOn;

        public ScreenSaver(Lcd2004 screen, IKeyboard keyboard)
        {
            _screen = screen;
            _keyboard = keyboard;
        }

        public void Init(int seconds, bool enabled)
        {
            _seconds = seconds;

            if (enabled)
            {
                CreateTimer();
            }

            _keyboard.KeyPressed += KeyboardOnKeyPressed;
        }

        public void Disable()
        {
            _timer.Dispose();
        }
        
        public void Enable()
        {
            CreateTimer();
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }

        private void CreateTimer()
        {
            if (_timer == null)
            {
                _timer = new Timer(TimeElapsed, null, _seconds * 1000, Timeout.Infinite);
            }
        }

        private void KeyboardOnKeyPressed(Key key)
        {
            if (_isOn)
            {
                _isOn = false;
                _screen.BackLightOn();
            }

            if (_timer != null)
            {
                _timer.Change(_seconds * 1000, Timeout.Infinite);
            }
        }

        private void TimeElapsed(object state)
        {
            _isOn = true;
            _screen.BackLightOff();
        }
    }
}
