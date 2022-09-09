namespace HomeAutomation
{
    using System;
    using System.Threading;

    using GHIElectronics.NETMF.FEZ;
    using GHIElectronics.NETMF.Hardware;

    using Microsoft.SPOT.Hardware;

    public class Program
    {
        private static Log _log;
        private static RelaysManager _relaysManager;
        private static Configuration _config;
        private static TimerEx _timerEx;

        public static DateTime Now
        {
            get
            {
                return RealTimeClock.GetTime();
            }
            set
            {
                RealTimeClock.SetTime(value);
            }
        }

        public static void Main()
        {
            //Now = new DateTime(2022, 9, 09, 21, 28, 3);
            
            var sdCard = new SdCard();
            _log = new Log(sdCard);
            _config = new Configuration(sdCard, _log);
            _relaysManager = new RelaysManager();
            _timerEx = new TimerEx(_log);

            _log.Write("Starting...");

            _relaysManager.Init();
            ReloadConfig();
            
            ScheduleConfigReload();

            //InterruptPort interrupt = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.An0,
            //    false,
            //    Port.ResistorMode.Disabled,
            //    Port.InterruptMode.InterruptEdgeBoth);

            //interrupt.OnInterrupt += Interrupt_OnInterrupt;
            
            //var nextRun = now.AddSeconds(20);

            //timerEx.TryScheduleRunAt(nextRun, NextRun);

            _log.Write("Started");

            Thread.Sleep(Timeout.Infinite);
        }

#if DEBUG_DST
        private static bool _isChanged;
#endif

        private static bool IsLastSunday(int month)
        {
#if DEBUG_DST
            if (!_isChanged && month == 10)
            {
                _log.Write("Last Sunday detected :)");
                _isChanged = true;
                return true;
            }
#endif

            return Now.Month == month &&
                   Now.DayOfWeek == DayOfWeek.Sunday &&
                   Now.Hour == 0 && // returns false when called again at 3 or 4 a clock on restarting
                   Now.AddDays(7).Month != month;
        }

        #region Reload Configuration

        private static void ScheduleConfigReload()
        {
            var nextDay = Now.AddDays(1);
            var nextMidnight = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day);

            _timerEx.TryScheduleRunAt(nextMidnight, ReloadConfigCallback, new TimeSpan(24, 0, 0), "Config Reload ");
        }

        private static void ReloadConfigCallback(object state)
        {
            ReloadConfig();
        }

        private static void ReloadConfig()
        {
            _config.Load();

            #region Daylight Saving Time
            // https://www.timeanddate.com/time/change/bulgaria
            if (IsLastSunday(3)) // Last Sunday of March add 1 hour
            {
#if DEBUG_DST
                _timerEx.TryScheduleRunAt(Now.AddMinutes(1), DstStart, "DST Start ");
#else
                _timerEx.TryScheduleRunAt(Now.AddHours(3), DstStart, "DST Start ");
#endif
            }

            if (IsLastSunday(10)) // Last Sunday of October subtract 1 hour
            {
#if DEBUG_DST
                _timerEx.TryScheduleRunAt(Now.AddMinutes(1), DstEnd, "DST End ");
#else
                _timerEx.TryScheduleRunAt(Now.AddHours(4), DstEnd, "DST End ");
#endif
            }
            #endregion

            ScheduleLights(true);
        }

        private static void DstStart(object state)
        {
            AdjustTimeAndRestart(1);
        }

        private static void DstEnd(object state)
        {
            AdjustTimeAndRestart(-1);
        }

        private static void AdjustTimeAndRestart(int hours)
        {
            Now = Now.AddHours(hours);
            _timerEx.DisposeAll();

            _log.Write("Time adjusted with " + hours + " hour.");

            ReloadConfig();
            ScheduleConfigReload();
        }

        #endregion

        #region Turn On/Off Lights

        private static void ScheduleLights(bool onReload)
        {
            var now = Now;

            var sunrise = _config.Sunrise.AddMinutes(_config.SunriseOffsetMin);
            var sunset = _config.Sunset.AddMinutes(_config.SunsetOffsetMin);
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
                SetLights(lightsOn);
            }
        }

        private static void SunriseAction(object state)
        {
            ScheduleLights(false);
            SetLights(false);
            _timerEx.TryDispose((Guid)state);
        }

        private static void SunsetAction(object state)
        {
            SetLights(true);
            _timerEx.TryDispose((Guid)state);
        }

        private static void SetLights(bool lightsOn)
        {
            _relaysManager.Set(3, lightsOn);
            _log.Write("Lights" + (lightsOn ? "ON" : "OFF"));
        }

        #endregion

        private static void NextRun(object state)
        {
            _relaysManager.Set(4, true);
        }

        private static void Interrupt_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            _config.Load();
            
        }

        public static void MainOld()
        {
            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, false);

            LegoRemote remote = new LegoRemote((Cpu.Pin)FEZ_Pin.Interrupt.Di11);
            remote.OnLegoButtonPress += remote_OnLegoButtonPress;

            InputPort pressureSensor = new InputPort((Cpu.Pin)FEZ_Pin.Digital.Di9, false, Port.ResistorMode.PullUp);

            led.Write(true);
            Thread.Sleep(600);
            led.Write(false);
            Thread.Sleep(600);

            while (true)
            {
                var hasPressure = pressureSensor.Read();

                _relaysManager.Set(5, hasPressure);

                Thread.Sleep(1000);
            }
        }

        static void remote_OnLegoButtonPress(Message msg)
        {
            if (msg.CommandA == Command.ComboDirectForward)
            {
                _relaysManager.Set(4, true);
            }

            if (msg.CommandA == Command.ComboDirectBackward)
            {
                _relaysManager.Set(4, true);
            }
        }
    }
}
