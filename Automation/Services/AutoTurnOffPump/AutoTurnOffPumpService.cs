namespace HomeAutomation.Services.AutoTurnOffPump
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware;
    using AdSoft.Fez.Hardware.Interfaces;

    using HomeAutomation.Tools;

    internal class AutoTurnOffPumpService
    {
        private readonly int _relayId;

        private readonly Log _log;
        private readonly Tools.Configuration _configuration;
        private readonly RealTimer _realTimer;
        private readonly IPressureSensor _pressureSensor;
        private readonly IPumpStateSensor _pumpStateSensor;
        private readonly RelaysArray _relaysArray;

        private int _eventCount;
        private bool _lowPressureReacted;

        public AutoTurnOffPumpService(
            Log log,
            Tools.Configuration configuration,
            RealTimer realTimer,
            IPressureSensor pressureSensor,
            IPumpStateSensor pumpStateSensor,
            RelaysArray relaysArray,
            int relayId)
        {
            _log = log;
            _configuration = configuration;
            _realTimer = realTimer;
            _pressureSensor = pressureSensor;
            _pumpStateSensor = pumpStateSensor;
            _relaysArray = relaysArray;
            _relayId = relayId;

            _eventCount = 0;
            _lowPressureReacted = false;
        }

        public void Init()
        {
            _realTimer.TryScheduleRunAt(DateTime.Now.AddMinutes(2),
                CheckPressure,
                new TimeSpan(0, _configuration.AutoTurnOffPumpConfiguration.Interval, 0),
                "AutoTurnOffPumpService ");
        }

        private void CheckPressure(object state)
        {
            var pumpTurnedOff = !_pumpStateSensor.IsWorking;
            var normalPressure = _pressureSensor.Pressure > _configuration.AutoTurnOffPumpConfiguration.MinPressure;

            if (pumpTurnedOff || normalPressure || _lowPressureReacted)
            {
                if (normalPressure && _lowPressureReacted)
                {
                    _lowPressureReacted = false;
                    _log.Write("Pressure restored.");
                }

                _eventCount = 0;
                return;
            }

            _eventCount++;
            
            if (_eventCount >= _configuration.AutoTurnOffPumpConfiguration.MaxEventsCount)
            {
                _lowPressureReacted = true;

                SendTurnOffSignal();
            }
        }

        private void SendTurnOffSignal()
        {
            _relaysArray.Set(_relayId, true);
            Thread.Sleep(_configuration.AutoTurnOffPumpConfiguration.SignalLength);
            _relaysArray.Set(_relayId, false);

            _log.Write("Pump turned off due to low pressure.");
        }
    }
}
