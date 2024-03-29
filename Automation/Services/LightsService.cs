namespace HomeAutomation.Services
{
    using System;

    using AdSoft.Fez.Hardware;

    using HomeAutomation.Tools;

    using Microsoft.SPOT;

    public class LightsService
    {
        private readonly int _relayId;

        private readonly Log _log;
        private readonly Configuration _config;
        private readonly RealTimer _realTimer;
        private readonly RelaysArray _relaysArray;

        public LightsService(Log log, Configuration config, RealTimer realTimer, RelaysArray relaysArray, int relayId)
        {
            _log = log;
            _config = config;
            _realTimer = realTimer;
            _relaysArray = relaysArray;

            _relayId = relayId;
        }

        public void ScheduleLights(bool onReload)
        {
            var now = Program.Now;

            var sunrise = _config.Sunrise.AddMinutes(_config.SunriseOffsetMin);
            var sunset = _config.Sunset.AddMinutes(_config.SunsetOffsetMin);

            Debug.Print("ScheduleLights - sunrise: " + Base.Format(sunrise));
            Debug.Print("ScheduleLights - sunset : " + Base.Format(sunset));
            
            if (now.AddMinutes(1) < sunrise)
            {
                _realTimer.TryScheduleRunAt(sunrise, SunriseAction, "Sunrise ");
            }
            else if (now < sunset)
            {
                _realTimer.TryScheduleRunAt(sunset, SunsetAction, "Sunset ");
            }

            if (onReload)
            {
                var lightsOn = now < sunrise || sunset <= now;
                SetLights(lightsOn, "Init ");
            }
        }

        public bool GetLightsState()
        {
            return _relaysArray.Get(_relayId);
        }

        public void SetLights(bool lightsOn, string logPrefix = "")
        {
            _relaysArray.Set(_relayId, lightsOn);
            _log.Write(logPrefix + "Lights " + (lightsOn ? "ON" : "OFF"));
        }

        private void SunriseAction(TimerState state)
        {
            ScheduleLights(false);
            SetLights(false, "Sunrise ");
        }

        private void SunsetAction(TimerState state)
        {
            SetLights(true, "Sunset ");
        }
    }
}
