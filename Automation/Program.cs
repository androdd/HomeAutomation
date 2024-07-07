namespace HomeAutomation
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Configuration;
    using AdSoft.Fez.Hardware.Storage;

    using GHIElectronics.NETMF.Hardware;

    using HomeAutomation.Hardware;
    using HomeAutomation.Services;
    using HomeAutomation.Services.AutoTurnOffPump;
    using HomeAutomation.Services.Watering;
    using HomeAutomation.Tools;
    using HomeAutomation.Ui;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    using Configuration = HomeAutomation.Tools.Configuration;

    public class Program
    {
        internal const string Version = "1 24-07-04";

        private static Log _log;
        private static Configuration _configuration;
        private static ConfigurationManager _configurationManager;
        private static RealTimer _realTimer;
        private static LightsService _lightsService;
        private static AutoTurnOffPumpService _autoTurnOffPumpService;
        //private static PressureLoggingService _pressureLoggingService;
        private static WateringService _wateringService;
        
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
                Utility.SetLocalTime(value);
            }
        }

        public static void Main()
        {
#if DEBUG_SET_RTC
            RealTimeClock.SetTime(new DateTime(2023, 4, 25, 22, 0, 0));
#endif
            Utility.SetLocalTime(RealTimeClock.GetTime());

            //Debug.EnableGCMessages(true);

            var usbStick = new UsbStick();

            while (!usbStick.IsLoaded)
            {
                Thread.Sleep(500);
                Debug.Print("Loading configuration...");
            }

            _configuration = new Configuration();
            _log = new Log(usbStick);
            
            _log.Write("HomeAutomation v." + Version);
            _log.Write("Starting hardware...");

            _hardwareManager = new HardwareManager(usbStick);
            _hardwareManager.Setup();
            
            _log.Write("Starting...");

            SetupToolsAndServices();

            ReloadConfig();

            _hardwareManager.PressureSensor.PressureMultiplier = _configuration.PressureSensorMultiplier;
            _hardwareManager.FlowRateSensor.FlowRateMultiplier = _configuration.FlowRateSensorMultiplier;

            //_remoteCommandsService.Init();
            //_pressureLoggingService.Init();
            _autoTurnOffPumpService.Init();
            
            _uiManager = new UiManager(_configuration, _configurationManager, _hardwareManager, _lightsService, _autoTurnOffPumpService, _wateringService);
            _uiManager.Setup();

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

            _uiManager.SetStatus(Version);

#if DEBUG_FLOW_RATE
            for (int i = 0; i < 5000; i++)
            {
                _hardwareManager.FlowRateSensor.OnInterrupt(0, 0, DateTime.Now);
                Thread.Sleep(10);
            }

            Debug.Print(_hardwareManager.FlowRateSensor.Volume + " l.");
#endif
            Thread.Sleep(Timeout.Infinite);
        }

        private static void SetupToolsAndServices()
        {
            _configuration = new Configuration();
            _settingsFile = new SettingsFile(_hardwareManager.ExternalStorage, "config.txt");
            _configurationManager = new ConfigurationManager(_configuration, _settingsFile, _hardwareManager.ExternalStorage, _log);

            _realTimer = new RealTimer(_log);
            _lightsService = new LightsService(_log, _configuration, _realTimer, _hardwareManager.RelaysArray, _hardwareManager.LightsRelayId);
            _autoTurnOffPumpService = new AutoTurnOffPumpService(_log,
                _configuration,
                _realTimer,
                _hardwareManager.PressureSensor,
                _hardwareManager.PumpStateSensor,
                _hardwareManager.RelaysArray,
                _hardwareManager.AutoTurnOffPumpRelayId,
                _hardwareManager.WateringPumpTurnOffRelayId);
            
#if TEST_AUTO_TURN_OFF_SERVICE
            _remoteCommandsService = new AutoTurnOffPumpServiceTestRemoteCommandService(_log,
                _hardwareManager.LegoRemote,
                _hardwareManager.PumpStateSensor,
                _hardwareManager.PressureSensor);
            _remoteCommandsService.Init;
#endif

            //_pressureLoggingService = new PressureLoggingService(_configuration, _log, _hardwareManager.ExternalStorage, _hardwareManager.PressureSensor, _realTimer);
            _wateringService = new WateringService(_log,
                _configuration,
                _configurationManager,
                _realTimer,
                _hardwareManager.NorthMainValveRelayId,
                _hardwareManager.SouthMainValveRelayId,
                _hardwareManager.RelaysArray,
                _hardwareManager.FlowRateSensor);
        }

        private static void ScheduleConfigReload()
        {
            var nextDay = Now.AddDays(1);
            var nextMidnight = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 0, 1, 0);

            _realTimer.TryScheduleRunAt(nextMidnight, ReloadConfigCallback, new TimeSpan(24, 0, 0), "Config Reload ");
        }

        private static bool ReloadConfigCallback(object state)
        {
            ReloadConfig();

            return true;
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

            _wateringService.NorthSwitchState = _configuration.NorthSwitchState;

            _lightsService.ScheduleLights(true);
            _wateringService.ScheduleSouthWatering();
            _wateringService.ScheduleNorthWatering();
        }

        private static void DstStart(TimerState state)
        {
            AdjustTimeAndRestart(1);
        }

        private static void DstEnd(TimerState state)
        {
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
