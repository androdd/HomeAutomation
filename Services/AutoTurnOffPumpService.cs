namespace HomeAutomation.Services
{
    using System;
    using System.Threading;

    using HomeAutomation.Hardware;
    using HomeAutomation.Tools;

    internal class AutoTurnOffPumpService : IDisposable
    {
        private readonly int _relayId;

        private readonly Log _log;
        private readonly PressureSensor _pressureSensor;
        private readonly PumpStateSensor _pumpStateSensor;
        private readonly RelaysArray _relaysArray;

        private Timer _checkPressureTimer;

        public AutoTurnOffPumpService(Log log, PressureSensor pressureSensor, PumpStateSensor pumpStateSensor, RelaysArray relaysArray, int relayId)
        {
            _log = log;
            _pressureSensor = pressureSensor;
            _pumpStateSensor = pumpStateSensor;
            _relaysArray = relaysArray;
            _relayId = relayId;
        }

        public void Init()
        {
            _checkPressureTimer = new Timer(CheckPressure, null, 3 * 1000, 1 * 60 * 1000);
        }

        private void CheckPressure(object state)
        {
            if (!_pumpStateSensor.IsWorking || _pressureSensor.Pressure > 0.5)
            {
                return;
            }

            _relaysArray.Set(_relayId, true);
            Thread.Sleep(500);
            _relaysArray.Set(_relayId, false);

            _log.Write("Pump turned off due to low pressure.");
        }

        public void Dispose()
        {
            if (_checkPressureTimer != null)
            {
                _checkPressureTimer.Dispose();
            }
        }
    }
}
