namespace HomeAutomation.Services.Watering
{
    using System;

    using AdSoft.Fez;

    using HomeAutomation.Tools;

    public class WateringService
    {
        private readonly Log _log;
        private readonly Configuration _configuration;
        private readonly RealTimer _realTimer;
        private readonly HardwareManager _hardwareManager;

        public int NorthVolume { get; private set; }

        public int SouthVolume { get; private set; }

        public int NorthSwitchState { get; private set; }
        
        public WateringService(Log log, Configuration configuration,  RealTimer realTimer, HardwareManager hardwareManager)
        {
            _log = log;
            _configuration = configuration;
            _realTimer = realTimer;
            _hardwareManager = hardwareManager;
        }

        public void Start()
        {
            //DebugEx.Print(DebugEx.Target.WateringService, DateTime.Now.ToString("s") + " Start");

            DateTime firstStarTime = DateTime.MaxValue;
            DateTime lastEndTime = DateTime.MinValue;

            for (var i = 0; i < _configuration.SouthValveConfigurations.Length; i++)
            {
                var configuration = _configuration.SouthValveConfigurations[i];

                //DebugEx.Print(DebugEx.Target.WateringService,
                //    "Configuration " + (i + 1) + ": IsValid:" + configuration.IsValid + " IsEnabled:" + configuration.IsEnabled +
                //    " ContainsToday:" + configuration.ContainsDay(DateTime.Now.DayOfWeek) + " IsDue:" + (configuration.StartTime > DateTime.Now));
                
                if (!configuration.IsValid || !configuration.IsEnabled || !configuration.ContainsDay(DateTime.Now.DayOfWeek) ||
                    configuration.StartTime <= DateTime.Now)
                {
                    continue;
                }

                if (configuration.StartTime < firstStarTime)
                {
                    firstStarTime = configuration.StartTime;
                }

                DateTime endTime = configuration.StartTime.AddMinutes(configuration.Duration);

                if (endTime > lastEndTime)
                {
                    lastEndTime = endTime;
                }

                var name = "Valve " + (i + 1) + " South ";

                _realTimer.TryScheduleRunAt(configuration.StartTime,
                    TimerCallback,
                    new WateringTimerState { Start = true, RelayId = configuration.RelayId },
                    name);

                _realTimer.TryScheduleRunAt(endTime,
                    TimerCallback,
                    new WateringTimerState { Start = false, RelayId = configuration.RelayId },
                    name);

                //DebugEx.Print(DebugEx.Target.WateringService,
                //    "Index " + (i + 1) + " set. First start: " + firstStarTime.ToString("s") + ". Last end: " + lastEndTime.ToString("s"));
            }

            var mainStart = firstStarTime.Subtract(new TimeSpan(0, 0, 2));
            var mainEnd = lastEndTime.Subtract(new TimeSpan(0, 0, 5));

            //DebugEx.Print(DebugEx.Target.WateringService, "Main start: " + mainStart.ToString("s") + ". Main end: " + mainEnd.ToString("s"));

            var valveMainSouth = "Valve Main South ";
            _realTimer.TryScheduleRunAt(mainStart,
                TimerCallback,
                new WateringTimerState { Start = true, RelayId = _hardwareManager.SouthMainValveRelayId },
                valveMainSouth);

            _realTimer.TryScheduleRunAt(mainEnd,
                TimerCallback,
                new WateringTimerState { Start = false, RelayId = _hardwareManager.SouthMainValveRelayId },
                valveMainSouth);
        }

        private void TimerCallback(TimerState state)
        {
            var wateringTimerState = (WateringTimerState)state;

            _hardwareManager.RelaysArray.Set(wateringTimerState.RelayId, wateringTimerState.Start);

            _log.Write(wateringTimerState.Name + "valve is " + (wateringTimerState.Start ? "opened" : "closed") + ".");
        }
        
        public bool GetValveSouth(int index)
        {
            var configuration = _configuration.SouthValveConfigurations[index - 1];
            
            return configuration.IsValid &&  _hardwareManager.RelaysArray.Get(configuration.RelayId);
        }
        
        public bool GetValveMainSouth()
        {
            return _hardwareManager.RelaysArray.Get(_hardwareManager.SouthMainValveRelayId);
        }
        
        public bool GetValveMainNorth()
        {
            return _hardwareManager.RelaysArray.Get(_hardwareManager.NorthMainValveRelayId);
        }
    }

    public class WateringTimerState : TimerState
    {
        public int RelayId { get; set; }

        public bool Start { get; set; }
    }
}
