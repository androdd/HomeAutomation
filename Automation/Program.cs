namespace HomeAutomation
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware;
    using AdSoft.Fez.Hardware.Interfaces;
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Hardware.LegoRemote;
    using AdSoft.Fez.Hardware.SdCard;
    using AdSoft.Fez.Ui;
    using AdSoft.Fez.Ui.Menu;

    using GHIElectronics.NETMF.FEZ;
    using GHIElectronics.NETMF.Hardware;

    using HomeAutomation.Hardware;
    using HomeAutomation.Hardware.Mocks;
    using HomeAutomation.Services;
    using HomeAutomation.Services.AutoTurnOffPump;
    using HomeAutomation.Services.Interfaces;
    using HomeAutomation.Tools;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    using Configuration = HomeAutomation.Tools.Configuration;

    public class Program
    {
        private static Log _log;
        private static Configuration _config;
        private static RealTimer _realTimer;
        private static LightsService _lightsService;
        private static AutoTurnOffPumpService _autoTurnOffPumpService;
        private static PressureLoggingService _pressureLoggingService;
        private static IRemoteCommandsService _remoteCommandsService;
        
        private static HardwareManager _hardwareManager;
        private static UiManager _uiManager;

        public static bool ManagementMode { get; set; }

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
            Debug.EnableGCMessages(false);
            ManagementMode = false;
            _log = new Log();

            _log.Write("Starting hardware...");
            
            _hardwareManager = new HardwareManager(_log);
            _hardwareManager.Setup();

            _log.AddSdCard(_hardwareManager.SdCard);
            
            _log.Write("Starting...");

            SetupToolsAndServices();
            
            ReloadConfig();

            //_remoteCommandsService.Init();
            _pressureLoggingService.Init(_config.PressureLogIntervalMin);
            _autoTurnOffPumpService.Init();

            _hardwareManager.SdCard.CardStatusChanged += SdCardOnCardStatusChanged;

            _uiManager = new UiManager(_log, _hardwareManager);
            _uiManager.Setup();

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

            Thread.Sleep(Timeout.Infinite);
        }

        private static void SdCardOnCardStatusChanged(Status status)
        {
            string statusText;
            switch (status)
            {
                case Status.Available:
                    statusText = "     ";
                    break;
                case Status.Unavailable:
                    statusText = "S:N/A";
                    break;
                case Status.Error:
                    statusText = "S:Err";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("status");
            }

            _uiManager.SdCardStatus.Text = statusText;
        }

        private static void SetupToolsAndServices()
        {
            _config = new Configuration(_hardwareManager.SdCard, _log);

            _realTimer = new RealTimer(_log);
            _lightsService = new LightsService(_log, _config, _realTimer, _hardwareManager.RelaysArray, _hardwareManager.LightsRelayId);
            _autoTurnOffPumpService = new AutoTurnOffPumpService(_log, _config, _hardwareManager.PressureSensor, _hardwareManager.PumpStateSensor, _hardwareManager.RelaysArray, _hardwareManager.AutoTurnOffPumpRelayId);
            _remoteCommandsService = new RemoteCommandsService(_hardwareManager.LegoRemote, _lightsService);

#if TEST_AUTO_TURN_OFF_SERVICE
            _remoteCommandsService = new AutoTurnOffPumpServiceTestRemoteCommandService(_log,
                _hardwareManager.LegoRemote,
                _hardwareManager.PumpStateSensor,
                _hardwareManager.PressureSensor);
            _remoteCommandsService.Init;
#endif

            _pressureLoggingService = new PressureLoggingService(_log, _hardwareManager.SdCard, _hardwareManager.PressureSensor);
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
            _config.Load();

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
