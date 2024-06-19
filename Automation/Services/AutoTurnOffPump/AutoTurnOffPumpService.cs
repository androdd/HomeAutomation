namespace HomeAutomation.Services.AutoTurnOffPump
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware;
    using AdSoft.Fez.Hardware.Interfaces;

    using HomeAutomation.Tools;

    public delegate void PumpStatusChangedEventHandler(Status status);

    public class AutoTurnOffPumpService
    {
        private readonly int _housePumpRelayId;
        private readonly int _wateringPumpRelayId;

        private readonly Log _log;
        private readonly Tools.Configuration _configuration;
        private readonly RealTimer _realTimer;
        private readonly IPressureSensor _pressureSensor;
        private readonly IPumpStateSensor _pumpStateSensor;
        private readonly RelaysArray _relaysArray;

        private int _eventCount;
        private bool _lowPressureReacted;
        private int _housePumpCheckCycles;
        private int _cycle;

        public event PumpStatusChangedEventHandler StatusChanged;

        public AutoTurnOffPumpService(
            Log log,
            Tools.Configuration configuration,
            RealTimer realTimer,
            IPressureSensor pressureSensor,
            IPumpStateSensor pumpStateSensor,
            RelaysArray relaysArray,
            int housePumpRelayId,
            int wateringPumpRelayId)
        {
            _log = log;
            _configuration = configuration;
            _realTimer = realTimer;
            _pressureSensor = pressureSensor;
            _pumpStateSensor = pumpStateSensor;
            _relaysArray = relaysArray;
            _housePumpRelayId = housePumpRelayId;
            _wateringPumpRelayId = wateringPumpRelayId;

            _eventCount = 0;
            _lowPressureReacted = false;
        }

        public void Init()
        {
            _housePumpCheckCycles = _configuration.AutoTurnOffPumpConfiguration.Interval * 6;

            _realTimer.TryScheduleRunAt(DateTime.Now.AddSeconds(10),
                CheckPressure,
                new TimeSpan(0, 0, 10),
                "AutoTurnOffPumpService ",
                false);
        }

        private bool CheckPressure(object state)
        {
            var isLowPressure = _pressureSensor.Pressure < 0.2;
            var isWateringPumpOn = !_relaysArray.Get(_wateringPumpRelayId);

            if (isLowPressure)
            {
                if (isWateringPumpOn)
                {
                    _relaysArray.Set(_wateringPumpRelayId, true);
                    _log.Write("Low watering pressure. Watering Pump is turned Off.");
                }
            }
            else
            {
                if (!isWateringPumpOn)
                {
                    _relaysArray.Set(_wateringPumpRelayId, false);
                    _log.Write("Watering pressure restored. Watering Pump is turned On.");
                }
            }

            if (_cycle == _housePumpCheckCycles)
            {
                CheckHousePump();
                _cycle = 0;
            }

            _cycle++;

            return true;
        }

        private void CheckHousePump()
        {
            var pumpTurnedOff = !_pumpStateSensor.IsWorking;
            var normalPressure = _pressureSensor.Pressure > _configuration.AutoTurnOffPumpConfiguration.MinPressure;

            if (pumpTurnedOff || normalPressure || _lowPressureReacted)
            {
                if (normalPressure && _lowPressureReacted)
                {
                    _lowPressureReacted = false;
                    _log.Write("Pressure restored.");

                    RaiseStatusChanged(Status.Restore);
                }

                _eventCount = 0;
            }
            else
            {
                _eventCount++;

                if (_eventCount >= _configuration.AutoTurnOffPumpConfiguration.MaxEventsCount)
                {
                    _lowPressureReacted = true;

                    SendTurnOffSignal();
                }
            }
        }

        private void SendTurnOffSignal()
        {
            _relaysArray.Set(_housePumpRelayId, true);
            Thread.Sleep(_configuration.AutoTurnOffPumpConfiguration.SignalLength);
            _relaysArray.Set(_housePumpRelayId, false);

            _log.Write("Pump turned off due to low pressure.");

            RaiseStatusChanged(Status.TurnOff);
        }
        
        protected void RaiseStatusChanged(Status status)
        {
            var onStatusChanged = StatusChanged;
            if (onStatusChanged != null)
            {
                onStatusChanged(status);
            }
        }
    }
}
