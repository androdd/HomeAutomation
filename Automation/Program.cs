namespace HomeAutomation
{
    using System;
    using System.Threading;

    using GHIElectronics.NETMF.FEZ;
    using GHIElectronics.NETMF.Hardware;

    using HomeAutomation.Hardware;
    using HomeAutomation.Hardware.Interfaces;
    using HomeAutomation.Services;
    using HomeAutomation.Tools;

    using Microsoft.SPOT.Hardware;

    using Configuration = HomeAutomation.Tools.Configuration;

    public class Program
    {
        private static SdCard _sdCard;
        private static Log _log;
        private static RelaysArray _relaysArray;
        private static Configuration _config;
        private static RealTimer _realTimer;
        private static LightsService _lightsService;
        private static AutoTurnOffPumpService _autoTurnOffPumpService;
        private static IPressureSensor _pressureSensor;
        private static PumpStateSensor _pumpStateSensor;
        private static LegoRemote _legoRemote;
        private static RemoteCommandsService _remoteCommandsService;
        private static PressureLoggingService _pressureLoggingService;
        private static WaterFlowSensor _waterFlowSensor;

        private static int _lightsRelayId;
        private static int _autoTurnOffPumpRelayId;

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
            
            SetupHardware();

            SetupToolsAndServices();

            _log.Write("Starting...");
            
            _relaysArray.Init();
            _pressureSensor.Init();
            _pumpStateSensor.Init();
            _legoRemote.Init();
            _waterFlowSensor.Init();

            ReloadConfig();

            _remoteCommandsService.Init();
            _pressureLoggingService.Init(_config.PressureLogIntervalMin);
            _autoTurnOffPumpService.Init();

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

            using (var led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, false))
            {
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(300);
                    led.Write(true);
                    Thread.Sleep(400);
                    led.Write(false);
                }
            }

            Thread.Sleep(Timeout.Infinite);
        }

        private static void SetupHardware()
        {
            _sdCard = new SdCard();
            _relaysArray = new RelaysArray(new[]
            {
                FEZ_Pin.Digital.Di0,
                FEZ_Pin.Digital.Di1,
                FEZ_Pin.Digital.Di4,
                FEZ_Pin.Digital.Di5,
                FEZ_Pin.Digital.Di6,
                FEZ_Pin.Digital.Di7,
                FEZ_Pin.Digital.Di8,
                FEZ_Pin.Digital.Di9
            });
            _pressureSensor = new PressureSensor80(FEZ_Pin.AnalogIn.An1);
            _pumpStateSensor = new PumpStateSensor(FEZ_Pin.Digital.An0);
            _legoRemote = new LegoRemote(FEZ_Pin.Interrupt.Di11);
            _waterFlowSensor = new WaterFlowSensor(FEZ_Pin.Interrupt.Di12);

            _lightsRelayId = 7;
            _autoTurnOffPumpRelayId = 5;
        }

        private static void SetupToolsAndServices()
        {
            _log = new Log(_sdCard);
            _config = new Configuration(_sdCard, _log);

            _realTimer = new RealTimer(_log);
            _lightsService = new LightsService(_log, _config, _realTimer, _relaysArray, _lightsRelayId);
            _autoTurnOffPumpService = new AutoTurnOffPumpService(_log, _config, _pressureSensor, _pumpStateSensor, _relaysArray, _autoTurnOffPumpRelayId);
            _remoteCommandsService = new RemoteCommandsService(_legoRemote, _lightsService);
            _pressureLoggingService = new PressureLoggingService(_log, _sdCard, _pressureSensor);
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
