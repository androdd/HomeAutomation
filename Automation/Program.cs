namespace HomeAutomation
{
    using System;
    using System.Reflection;
    using System.Threading;

    using AdSoft.Fez;
    using AdSoft.Fez.Configuration;
    using AdSoft.Fez.Hardware;
    using AdSoft.Fez.Hardware.SdCard;

    using GHIElectronics.NETMF.Hardware;

    using HomeAutomation.Hardware.Mocks;
    using HomeAutomation.Services;
    using HomeAutomation.Services.AutoTurnOffPump;
    using HomeAutomation.Tools;
    using HomeAutomation.Ui;

    using Microsoft.SPOT;

    using Configuration = HomeAutomation.Tools.Configuration;

    public class Program
    {
        private static Log _log;
        private static Configuration _configuration;
        private static ConfigurationManager _configurationManager;
        private static RealTimer _realTimer;
        private static LightsService _lightsService;
        private static AutoTurnOffPumpService _autoTurnOffPumpService;
        private static PressureLoggingService _pressureLoggingService;
        
        private static HardwareManager _hardwareManager;
        private static UiManager _uiManager;
        private static SettingsFile _settingsFile;

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

#if DEBUG_SET_RTC
            RealTimeClock.SetTime(new DateTime(2023, 4, 30, 22, 0, 0));
#endif

            DebugEx.Targets = DebugEx.Target.None;
            //DebugEx.Targets |= DebugEx.Target.ScreenSaver;
            //DebugEx.Targets |= DebugEx.Target.Ui;
            DebugEx.Targets |= DebugEx.Target.Log;
            DebugEx.Targets |= DebugEx.Target.Keyboard;
            DebugEx.Targets |= DebugEx.Target.PressureLog;
            //DebugEx.Targets |= DebugEx.Target.Lcd2004;
            //DebugEx.Targets |= DebugEx.Target.ScreenPowerButton;
            Debug.EnableGCMessages(false);

            var sdCard = new SdCard();
            _configuration = new Configuration();
            _log = new Log(_configuration, sdCard);

            _log.Write("Starting hardware...");
            
            _hardwareManager = new HardwareManager(_log, sdCard);
            _hardwareManager.Setup();

            _log.Write("Starting...");

            SetupToolsAndServices();
            
            ReloadConfig();

            _hardwareManager.PressureSensor.PressureMultiplier = _configuration.PressureSensorMultiplier;
            _hardwareManager.FlowRateSensor.FlowRateMultiplier = _configuration.FlowRateSensorMultiplier;

            //_remoteCommandsService.Init();
            _pressureLoggingService.Init(_configuration.PressureLogIntervalMin);
            _autoTurnOffPumpService.Init();
            
            _uiManager = new UiManager(_configuration, _configurationManager, _hardwareManager, _lightsService);
            _uiManager.Setup();

            #region Manual DST Adjustment

            if (_configuration.ManualStartDst)
            {
                _configurationManager.SaveDst();
                Now = Now.AddHours(1);
                _log.Write("Time manually adjusted with 1 hour.");
            }

            if (_configuration.ManualEndDst)
            {
                _configurationManager.DeleteDst();
                Now = Now.AddHours(-1);
                _log.Write("Time manually adjusted with -1 hour.");
            }

            #endregion

            ScheduleConfigReload();

#if DEBUG_PRESSURE_SENSOR
            while (true)
            {
                var pressure = _pressureSensor.Pressure;
                var voltage = _pressureSensor.Voltage;
                
                pressure = pressure < 0
                    ? 0
                    : pressure;

                var pressureByte = (byte)MathEx.Truncate(pressure * 50); // Fit up to 5 bar in 1 byte 5 * 50 = 250 < 256

                Debug.Print("Voltage: " + voltage + " V; Pressure: " + pressure.ToString("F") + " bar; PressureByte: " + pressureByte);
                
                for (int i = 0; i <= 7; i++)
                {
                    _relaysArray.Set(7 - i, (pressureByte & (1 << i)) == 1 << i);
                }

                Thread.Sleep(10 * 1000);
            };
#endif

            _log.Write("Started");

            _hardwareManager.MbLed.Blink(3);

#if MOCK_FLOW_RATE
            var method = typeof(FlowRateSensor).GetMethod("OnInterrupt",
                BindingFlags.NonPublic | BindingFlags.Instance);

            for (int i = 0; i < 5000; i++)
            {
                method.Invoke(_hardwareManager.FlowRateSensor, new object[] { (uint)0, (uint)0, DateTime.Now });
                Thread.Sleep(20);
            }

            Debug.Print(_hardwareManager.FlowRateSensor.Volume + " l.");
#endif

            Thread.Sleep(Timeout.Infinite);
        }

        private static void SetupToolsAndServices()
        {
            _configuration = new Configuration();
            _settingsFile = new SettingsFile(_hardwareManager.SdCard, "config.txt");
            _configurationManager = new ConfigurationManager(_configuration, _settingsFile, _hardwareManager.SdCard, _log);

            _realTimer = new RealTimer(_log);
            _lightsService = new LightsService(_log, _configuration, _realTimer, _hardwareManager.RelaysArray, _hardwareManager.LightsRelayId);
            _autoTurnOffPumpService = new AutoTurnOffPumpService(_log, _configuration, _hardwareManager.PressureSensor, _hardwareManager.PumpStateSensor, _hardwareManager.RelaysArray, _hardwareManager.AutoTurnOffPumpRelayId);
            
#if TEST_AUTO_TURN_OFF_SERVICE
            _remoteCommandsService = new AutoTurnOffPumpServiceTestRemoteCommandService(_log,
                _hardwareManager.LegoRemote,
                _hardwareManager.PumpStateSensor,
                _hardwareManager.PressureSensor);
            _remoteCommandsService.Init;
#endif

            _pressureLoggingService = new PressureLoggingService(_configuration, _log, _hardwareManager.SdCard, _hardwareManager.PressureSensor);
        }

        private static void ScheduleConfigReload()
        {
            var nextDay = Now.AddDays(1);
            var nextMidnight = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day);

            _realTimer.TryScheduleRunAt(nextMidnight, ReloadConfigCallback, new TimeSpan(24, 0, 0), "Config Reload ");
        }

        private static void ReloadConfigCallback(object state)
        {
            ReloadConfig();
        }

        private static void ReloadConfig()
        {
            _configurationManager.Load();

            #region Automatic DST Adjustment
            // https://www.timeanddate.com/time/change/bulgaria
            if (IsLastSunday(3)) // Last Sunday of March add 1 hour
            {
#if DEBUG_DST
                _timerEx.TryScheduleRunAt(Now.AddMinutes(1), DstStart, "DST Start ");
#else
                _realTimer.TryScheduleRunAt(Now.AddHours(3), DstStart, "DST Start ");
#endif
            }

            if (IsLastSunday(10)) // Last Sunday of October subtract 1 hour
            {
#if DEBUG_DST
                _timerEx.TryScheduleRunAt(Now.AddMinutes(1), DstEnd, "DST End ");
#else
                _realTimer.TryScheduleRunAt(Now.AddHours(4), DstEnd, "DST End ");
#endif
            }
            #endregion

            _lightsService.ScheduleLights(true);
        }

        private static void DstStart(object state)
        {
            _configurationManager.SaveDst();

            AdjustTimeAndRestart(1);
        }

        private static void DstEnd(object state)
        {
            _configurationManager.DeleteDst();

            AdjustTimeAndRestart(-1);
        }

        private static void AdjustTimeAndRestart(int hours)
        {
            Now = Now.AddHours(hours);
            _realTimer.DisposeAll();

            _log.Write("Time adjusted with " + hours + " hour.");

            ReloadConfig();
            ScheduleConfigReload();
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
                   Now.Hour == 0 && // returns false when called again at 3 or 4 o'clock on restarting
                   Now.AddDays(7).Month != month;
        }
    }
}
