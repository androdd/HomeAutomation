namespace HomeAutomation.Services.Watering
{
    using System;
    using System.Collections;
    using System.Threading;

    using AdSoft.Fez.Hardware;
    using AdSoft.Fez.Hardware.Interfaces;

    using HomeAutomation.Tools;

    public class WateringService
    {
        private readonly Log _log;
        private readonly Configuration _configuration;
        private readonly RealTimer _realTimer;
        private readonly int _northMainValveRelayId;
        private readonly int _southMainValveRelayId;
        private readonly RelaysArray _relaysArray;
        private readonly IFlowRateSensor _flowRateSensor;

        private readonly ArrayList _manualTimerKeys;
        private readonly ArrayList _automaticTimerKeys;
        private readonly ArrayList _runningTimerKeys;

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

            _manualTimerKeys = new ArrayList();
            _automaticTimerKeys = new ArrayList();
            _runningTimerKeys = new ArrayList();

            _lastManualWateringEnd = DateTime.Now;

            NorthSwitchState = 1;
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

        public bool TryStartSouth(int valveId, int minutes)
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
                SouthTimerCallback,
                new WateringTimerState { RelayId = configuration.RelayId, TurnMainOnOff = true },
                new TimeSpan(0, minutes, 0),
                "Valve " + valveId + " South ");

            if (!key.Equals(Guid.Empty))
            {
                _manualTimerKeys.Add(key);
            }
            
            return true;
        }

        public bool TryStartNorth(DateTime dueDateTime, int cornerMinutes, int mainMinutes, bool isAutomatic)
        {
            var isOn = _relaysArray.Get(_northMainValveRelayId);

            if (isOn)
            {
                return false;
            }

            AddNorthSchedule(dueDateTime, cornerMinutes, 1, isAutomatic);

            dueDateTime = dueDateTime.AddSeconds(cornerMinutes * 60 + 5);
            AddNorthSchedule(dueDateTime, cornerMinutes, 2, isAutomatic);

            dueDateTime = dueDateTime.AddSeconds(cornerMinutes * 60 + 5);
            AddNorthSchedule(dueDateTime, cornerMinutes, 3, isAutomatic);

            dueDateTime = dueDateTime.AddSeconds(cornerMinutes * 60 + 5);
            AddNorthSchedule(dueDateTime, cornerMinutes, 4, isAutomatic);

            dueDateTime = dueDateTime.AddSeconds(cornerMinutes * 60 + 5);
            AddNorthSchedule(dueDateTime, mainMinutes, 5, isAutomatic);

            return true;
        }

        private void AddNorthSchedule(DateTime dueDateTime, int minutes, int northSwitchState, bool isAutomatic)
        {
            var key = _realTimer.TryScheduleRunAt(dueDateTime,
                NorthTimerCallback,
                new WateringTimerState { RelayId = _northMainValveRelayId },
                new TimeSpan(0, minutes, 0),
                "Valve Main North " + northSwitchState + " ");

            if (!key.Equals(Guid.Empty))
            {
                if (isAutomatic)
                {
                    _automaticTimerKeys.Add(key);
                }
                else
                {
                    _manualTimerKeys.Add(key);   
                }
            }
        }

        public void ScheduleSouthWatering()
        {
            //DebugEx.Print(DebugEx.Target.WateringService, DateTime.Now.ToString("s") + " Start");

            ArrayList availableIntervals = new ArrayList();

            for (var i = 0; i < _configuration.SouthValveConfigurations.Length; i++)
            {
                var configuration = _configuration.SouthValveConfigurations[i];

                //Debug.Print(
                //    "Configuration " + (i + 1) + ": IsValid:" + configuration.IsValid + " IsEnabled:" + configuration.IsEnabled +
                //    " ContainsToday:" + configuration.ContainsDay(DateTime.Now.DayOfWeek) + " IsDue:" + (configuration.StartTime > DateTime.Now));

                if (!configuration.IsValid || !configuration.IsEnabled || !configuration.ContainsDay(DateTime.Now.DayOfWeek) ||
                    configuration.StartTime <= DateTime.Now)
                {
                    continue;
                }

                DateTime endTime = configuration.StartTime.AddMinutes(configuration.Duration);

                availableIntervals.Add(new TimeInterval(configuration.StartTime, endTime, i));
            }

            if (availableIntervals.Count == 0)
            {
                _log.Write("No South watering for today.");
                return;
            }

            var intervals = new TimeInterval[availableIntervals.Count];
            for (var i = 0; i < availableIntervals.Count; i++)
            {
                var interval = (TimeInterval)availableIntervals[i];
                intervals[i] = interval;
            }

            var mainIntervals = GetNonOverlappingIntervals(intervals, 15);

            foreach (var timeInterval in intervals)
            {
                var name = "Valve " + (timeInterval.ConfigId + 1) + " South ";

                var configuration = _configuration.SouthValveConfigurations[timeInterval.ConfigId];

                var key = _realTimer.TryScheduleRunAt(timeInterval.Start,
                    SouthTimerCallback,
                    new WateringTimerState { RelayId = configuration.RelayId, TurnMainOnOff = false },
                    new TimeSpan(0, configuration.Duration, 0),
                    name);

                if (!key.Equals(Guid.Empty))
                {
                    _automaticTimerKeys.Add(key);
                }
            }

            foreach (var interval in mainIntervals)
            {
                var timeInterval = (TimeInterval)interval;
                var offset = new TimeSpan(0, 0, 2);
                var mainStart = timeInterval.Start.Subtract(offset);
                var period = (timeInterval.End - timeInterval.Start).Add(offset).Add(offset);

                var valveMainSouth = "Valve Main South ";
                _realTimer.TryScheduleRunAt(mainStart,
                    SouthTimerCallback,
                    new WateringTimerState { RelayId = _southMainValveRelayId, TurnMainOnOff = false },
                    period,
                    valveMainSouth);   
            }
        }

        public void ScheduleNorthWatering()
        {
            var configuration = _configuration.NorthValveConfiguration;

            if (!configuration.IsValid || !configuration.IsEnabled || !configuration.ContainsDay(DateTime.Now.DayOfWeek) ||
                configuration.StartTime <= DateTime.Now)
            {
                _log.Write("No North watering for today.");
                return;
            }

            TryStartNorth(configuration.StartTime, configuration.Duration, configuration.Duration * 3, true);
        }

        public void CancelManual()
        {
            DisposeTimers(_runningTimerKeys);
            DisposeTimers(_manualTimerKeys);

            StopAllWater();

            _log.Write("Valve all manual watering stopped.");
        }

        public void CancelAutomatic()
        {
            DisposeTimers(_runningTimerKeys);
            DisposeTimers(_automaticTimerKeys);

            StopAllWater();

            _log.Write("Valve all automatic watering stopped.");
        }

        public void StopRunning()
        {
            DisposeTimers(_runningTimerKeys);

            StopAllWater();

            _log.Write("Valve all running watering stopped.");
        }

        public void ResetVolume()
        {
            _flowRateSensor.Volume = 0;
            NorthVolume = 0;
            SouthVolume = 0;
        }

        private bool SouthTimerCallback(TimerState state)
        {
            var wateringState = (WateringTimerState)state;
            
            var isOn = _relaysArray.Get(wateringState.RelayId);

            if (!isOn)
            {
                _runningTimerKeys.Add(wateringState.TimerKey);
            }
            else
            {
                _runningTimerKeys.Remove(wateringState.TimerKey);
            }

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

            // Used by the manual watering when every time the mains is opened and closed together with the specific valve.
            if (wateringState.TurnMainOnOff)
            {
                _relaysArray.Set(_southMainValveRelayId, !isOn);
                Thread.Sleep(2000);
            }

            if (wateringState.RelayId != _southMainValveRelayId)
            {
                var isMainOn = _relaysArray.Get(_southMainValveRelayId);

                // When watering is stopped manually but there are still automatic valves to be triggered. 
                // The main is stopped by the Stopping action and auto watering starts and stops main and specific separately.
                // So I need to turn on the main again.
                if (!isOn && !isMainOn)
                {
                    _relaysArray.Set(_southMainValveRelayId, true);
                    Thread.Sleep(2000);
                }
            }

            _relaysArray.Set(wateringState.RelayId, !isOn);

            _log.Write(wateringState.Name + "is " + (isOn ? "closed - " + (int)_flowRateSensor.Volume + " l. used" : "opened") + ".");

            SouthVolume += _flowRateSensor.Volume;
            _flowRateSensor.Volume = 0;

            return !isOn;
        }

        private bool NorthTimerCallback(TimerState state)
        {
            var wateringState = (WateringTimerState)state;
            
            var isOn = _relaysArray.Get(wateringState.RelayId);

            if (!isOn)
            {
                _runningTimerKeys.Add(wateringState.TimerKey);
            }
            else
            {
                _runningTimerKeys.Remove(wateringState.TimerKey);

                NorthSwitchState = NorthSwitchState % 5 + 1;
            }

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

            _relaysArray.Set(wateringState.RelayId, !isOn);

            _log.Write(wateringState.Name + "is " + (isOn ? "closed - " + (int)_flowRateSensor.Volume + " l. used" : "opened") + ".");

            NorthVolume += _flowRateSensor.Volume;
            _flowRateSensor.Volume = 0;

            return !isOn;
        }

        private void DisposeTimers(ArrayList keys)
        {
            foreach (var timerKey in keys)
            {
                var key = (Guid)timerKey;
                _realTimer.TryDispose(key);
            }

            keys.Clear();
        }

        private void StopAllWater()
        {
            _relaysArray.Set(_southMainValveRelayId, false);
            _relaysArray.Set(_northMainValveRelayId, false);

            Thread.Sleep(2000);

            for (int i = 0; i < _configuration.SouthValveConfigurations.Length; i++)
            {
                var configuration = _configuration.SouthValveConfigurations[i];

                if (!configuration.IsValid)
                {
                    continue;
                }

                _relaysArray.Set(configuration.RelayId, false);
            }
        }

        public static ArrayList GetNonOverlappingIntervals(TimeInterval[] intervals, int thresholdMinutes)
        {
            ArrayList nonOverlappingIntervals = new ArrayList();

            QuickSortDateTime(intervals, 0, intervals.Length - 1);

            TimeInterval currentInterval = intervals[0];

            for (int i = 1; i < intervals.Length; i++)
            {
                TimeInterval nextInterval = intervals[i];

                if (nextInterval.Start > currentInterval.End.AddMinutes(thresholdMinutes))
                {
                    nonOverlappingIntervals.Add(currentInterval);
                    currentInterval = nextInterval;
                }
                else
                {
                    currentInterval.End = currentInterval.End > nextInterval.End ? currentInterval.End : nextInterval.End;
                }
            }

            nonOverlappingIntervals.Add(currentInterval);

            return nonOverlappingIntervals;
        }

        public static void QuickSortDateTime(TimeInterval[] arr, int left, int right)
        {
            while (left < right)
            {
                int i = left, j = right;
                TimeInterval pivot = arr[(i + j) / 2];
                while (i <= j)
                {
                    while (arr[i].Start < pivot.Start) i++;
                    while (arr[j].Start > pivot.Start) j--;
                    if (i <= j)
                    {
                        TimeInterval tmp = arr[i];
                        arr[i] = arr[j];
                        arr[j] = tmp;
                        i++;
                        j--;
                    }
                }
                if (j - left <= right - i)
                {
                    QuickSortDateTime(arr, left, j);
                    left = i;
                }
                else
                {
                    QuickSortDateTime(arr, i, right);
                    right = j;
                }
            }
        }

#if DEBUG_WATERING
        private Thread _thread;
        private AutoResetEvent _resetEvent;
        private bool _isWaterOn;

        private void TurnOnWater()
        {
            if (_isWaterOn)
            {
                return;
            }

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
            _isWaterOn = true;
        }

        private void TurnOffWater()
        {
            if (!_isWaterOn)
            {
                return;
            }

            Debug.Print("TurnOffWater");
            _resetEvent.Set();
            _isWaterOn = false;
        }
#endif
    }
}
