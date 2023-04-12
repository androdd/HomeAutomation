namespace AdSoft.Fez.Ui
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    using Microsoft.SPOT;

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
        }

        public void Disable()
        {
            if (_timer != null)
            {
#if DEBUG_SS
                Debug.Print("ScreenSaver -  Disable");
#endif

                _keyboard.KeyPressed -= KeyboardOnKeyPressed;

                _timer.Dispose();
                _timer = null;
            }
        }
        
        public void Enable(bool isEnabled = true)
        {
            if (isEnabled)
            {
#if DEBUG_SS
                Debug.Print("ScreenSaver -  Enable");
#endif
                CreateTimer();
            }
            else
            {
                Disable();
            }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
#if DEBUG_SS
                Debug.Print("ScreenSaver -  Dispose");
#endif
                _timer.Dispose();
                _timer = null;
            }
        }

        private void CreateTimer()
        {
            if (_timer == null)
            {
#if DEBUG_SS
                Debug.Print("ScreenSaver -  New timer");
#endif
                _timer = new Timer(TimeElapsed, null, _seconds * 1000, Timeout.Infinite);

                _keyboard.KeyPressed += KeyboardOnKeyPressed;
            }
        }

        private void KeyboardOnKeyPressed(Key key)
        {
            if (_isOn)
            {
#if DEBUG_SS
                Debug.Print("ScreenSaver -  Off");
#endif
                _isOn = false;
                _screen.DisplayOn();
                _screen.BackLightOn();
            }

            if (_timer != null)
            {
#if DEBUG_SS
                Debug.Print("ScreenSaver -  Timer reset");
#endif
                _timer.Change(_seconds * 1000, Timeout.Infinite);
            }
        }

        private void TimeElapsed(object state)
        {
#if DEBUG_SS
            Debug.Print("ScreenSaver -  On");
#endif
            _isOn = true;
            _screen.BackLightOff();
            _screen.DisplayOff();
        }
    }
}
