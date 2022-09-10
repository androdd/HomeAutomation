namespace HomeAutomation.Services
{
    using System;

    using HomeAutomation.Hardware;
    using HomeAutomation.Tools;

    internal class LightsService
    {
        private const int LightsRelay = 7;

        private readonly Log _log;
        private readonly Configuration _config;
        private readonly TimerEx _timerEx;
        private readonly RelaysManager _relaysManager;

        public LightsService(Log log, Configuration config, TimerEx timerEx, RelaysManager relaysManager)
        {
            _log = log;
            _config = config;
            _timerEx = timerEx;
            _relaysManager = relaysManager;
        }

        public void ScheduleLights(bool onReload)
        {
            var now = Program.Now;

            var sunrise = _config.Sunrise.AddMinutes(_config.SunriseOffsetMin);
            var sunset = _config.Sunset.AddMinutes(_config.SunsetOffsetMin);

            if (_config.IsDst)
            {
                sunrise = sunrise.AddHours(1);
                sunset = sunset.AddHours(1);
            }

            if (now < sunrise)
            {
                _timerEx.TryScheduleRunAt(sunrise, SunriseAction, "Sunrise ");
            }
            else if (now < sunset)
            {
                _timerEx.TryScheduleRunAt(sunset, SunsetAction, "Sunset ");
            }

            if (onReload)
            {
                var lightsOn = now < sunrise || sunset <= now;
                SetLights(lightsOn, "Init ");
            }
        }

        public void SetLights(bool lightsOn, string logPrefix = "")
        {
            _relaysManager.Set(LightsRelay, lightsOn);
            _log.Write(logPrefix + "Lights " + (lightsOn ? "ON" : "OFF"));
        }

        private void SunriseAction(object state)
        {
            ScheduleLights(false);
            SetLights(false, "Sunrise ");
            _timerEx.TryDispose((Guid)state);
        }

        private void SunsetAction(object state)
        {
            SetLights(true, "Sunset ");
            _timerEx.TryDispose((Guid)state);
        }
    }
}
