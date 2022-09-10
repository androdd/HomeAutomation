namespace HomeAutomation
{
    using System;
    using System.Threading;

    using GHIElectronics.NETMF.Hardware;

    using HomeAutomation.Hardware;
    using HomeAutomation.Services;
    using HomeAutomation.Tools;

    using Configuration = HomeAutomation.Tools.Configuration;

    public class Program
    {
        private static SdCard _sdCard;
        private static Log _log;
        private static RelaysManager _relaysManager;
        private static Configuration _config;
        private static TimerEx _timerEx;
        private static LightsService _lightsService;

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
            
            _sdCard = new SdCard();
            _log = new Log(_sdCard);
            _config = new Configuration(_sdCard, _log);
            _relaysManager = new RelaysManager();
            _timerEx = new TimerEx(_log);
            _lightsService = new LightsService(_log, _config, _timerEx, _relaysManager);
            var remoteCommandsService = new RemoteCommandsService(_lightsService);

            _log.Write("Starting...");

            remoteCommandsService.Init();
            _relaysManager.Init();
            ReloadConfig();

            #region Manual DST Adjustment

            if (_config.ManualStartDst)
            {
                _config.SaveDst();
                Now = Now.AddHours(1);
                _log.Write("Time manually adjusted with 1 hour.");
            }

            if (_config.ManualEndDst)
            {
                _config.DeleteDst();
                Now = Now.AddHours(-1);
                _log.Write("Time manually adjusted with -1 hour.");
            }

            #endregion
            
            ScheduleConfigReload();

            //InterruptPort interrupt = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.An0,
            //    false,
            //    Port.ResistorMode.Disabled,
            //    Port.InterruptMode.InterruptEdgeBoth);

            _log.Write("Started");

            Thread.Sleep(Timeout.Infinite);
        }

#if DEBUG_DST
        //Automatic DST Adjustment
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

            #region Automatic DST Adjustment
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

            _lightsService.ScheduleLights(true);
        }

        private static void DstStart(object state)
        {
            _config.SaveDst();

            AdjustTimeAndRestart(1);
        }

        private static void DstEnd(object state)
        {
            _config.DeleteDst();

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
    }
}
