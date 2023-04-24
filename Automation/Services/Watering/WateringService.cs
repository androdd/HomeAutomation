namespace HomeAutomation.Services.Watering
{
    using System;
    using System.Collections;
    using System.Threading;

    using AdSoft.Fez;
    using AdSoft.Fez.Hardware;
    using AdSoft.Fez.Hardware.Interfaces;

    using HomeAutomation.Tools;

    using Microsoft.SPOT;

    public class WateringService
    {
        private readonly Log _log;
        private readonly Configuration _configuration;
        private readonly RealTimer _realTimer;
        private readonly int _northMainValveRelayId;
        private readonly int _southMainValveRelayId;
        private readonly RelaysArray _relaysArray;
        private readonly IFlowRateSensor _flowRateSensor;

        private readonly ArrayList _timerKeys;

        private DateTime _lastManualWateringEnd;

        public double NorthVolume { get; private set; }

        public double SouthVolume { get; private set; }

        public int NorthSwitchState { get; private set; }

        public WateringService(
            Log log,
            Configuration configuration,
            RealTimer realTimer,
            int northMainValveRelayId,
            int southMainValveRelayId,
            RelaysArray relaysArray,
            IFlowRateSensor flowRateSensor)
        {
            _log = log;
            _configuration = configuration;
            _realTimer = realTimer;
            _southMainValveRelayId = southMainValveRelayId;
            _northMainValveRelayId = northMainValveRelayId;
            _relaysArray = relaysArray;
            _flowRateSensor = flowRateSensor;

            _timerKeys = new ArrayList();

            _lastManualWateringEnd = DateTime.Now;
        }

        /// <summary>
        /// Gets current state of valve on the south side
        /// </summary>
        /// <param name="valveId">From 1 to 4</param>
        /// <returns></returns>
        public ValveState GetValveSouth(int valveId)
        {
            if (valveId < 1)
            {
                return ValveState.Invalid;
            }

            var configuration = _configuration.SouthValveConfigurations[valveId - 1];

            if (!configuration.IsValid)
            {
                return ValveState.Invalid;
            }
            
            return _relaysArray.Get(configuration.RelayId) ? ValveState.On : (configuration.IsEnabled ? ValveState.Off : ValveState.Disabled);
        }
        
        public bool GetValveMainSouth()
        {
            return _relaysArray.Get(_southMainValveRelayId);
        }
        
        public bool GetValveMainNorth()
        {
            return _relaysArray.Get(_northMainValveRelayId);
        }

        public bool TryStart(int valveId, int minutes)
        {
            var configuration = _configuration.SouthValveConfigurations[valveId - 1];

            if (!configuration.IsValid)
            {
                return false;
            }

            var next = _lastManualWateringEnd < DateTime.Now
                ? DateTime.Now.AddSeconds(3)
                : _lastManualWateringEnd.AddMinutes(1);

            _lastManualWateringEnd = next.AddMinutes(minutes);

            var key = _realTimer.TryScheduleRunAt(next,
                state =>
                {
                    var wateringState = (WateringTimerState)state;

                    var isOn = _relaysArray.Get(wateringState.RelayId);

#if DEBUG_WATERING
                    if (isOn)
                    {
                        TurnOffWater();
                    }
                    else
                    {
                        TurnOnWater();
                    } 
#endif
                    
                    _relaysArray.Set(_southMainValveRelayId, !isOn);
                    Thread.Sleep(2000);
                    _relaysArray.Set(wateringState.RelayId, !isOn);

                    _log.Write(wateringState.Name + "is " + (isOn ? "closed - " + (int)_flowRateSensor.Volume + " l. used" : "opened") + ".");

                    SouthVolume += _flowRateSensor.Volume;
                    _flowRateSensor.Volume = 0;
                    
                    return !isOn;
                },
                new WateringTimerState { RelayId = configuration.RelayId },
                new TimeSpan(0, minutes, 0),
                "Valve " + valveId + " South ");

            if (!key.Equals(Guid.Empty))
            {
                _timerKeys.Add(key);
            }
            
            return true;
        }

        public void StopManual()
        {
            foreach (var timerKey in _timerKeys)
            {
                var key = (Guid)timerKey;
                _realTimer.TryDispose(key);
            }

            _relaysArray.Set(_southMainValveRelayId, false);

            Thread.Sleep(2000);

            for (int i = 0; i < 4; i++)
            {
                var configuration = _configuration.SouthValveConfigurations[i];

                if (!configuration.IsValid)
                {
                    continue;
                }

                _relaysArray.Set(configuration.RelayId, false);
            }

            _log.Write("Valve all manual watering stopped.");
        }

        public void ResetVolume()
        {
            _flowRateSensor.Volume = 0;
            NorthVolume = 0;
            SouthVolume = 0;
        }

        public void ScheduleWatering()
        {
            return;

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
                new WateringTimerState { Start = true, RelayId = _southMainValveRelayId },
                valveMainSouth);

            _realTimer.TryScheduleRunAt(mainEnd,
                TimerCallback,
                new WateringTimerState { Start = false, RelayId = _southMainValveRelayId },
                valveMainSouth);
        }

        private void TimerCallback(TimerState state)
        {
            var wateringTimerState = (WateringTimerState)state;

            _relaysArray.Set(wateringTimerState.RelayId, wateringTimerState.Start);

            _log.Write(wateringTimerState.Name + "valve is " + (wateringTimerState.Start ? "opened" : "closed") + ".");
        }

#if DEBUG_WATERING
        private Thread _thread;
        private AutoResetEvent _resetEvent;

        private void TurnOnWater()
        {
            Debug.Print("TurnOnWater");

            _resetEvent = new AutoResetEvent(false);
            _thread = new Thread(() =>
            {
                while (!_resetEvent.WaitOne(20, false))
                {
                    _flowRateSensor.OnInterrupt(0, 0, DateTime.Now);
                }

                _resetEvent = null;
                _thread = null;
            });
            
            _thread.Start();
        }

        private void TurnOffWater()
        {
            Debug.Print("TurnOffWater");
            _resetEvent.Set();
        }
#endif
    }
}
