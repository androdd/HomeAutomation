namespace HomeAutomation.Services
{
    using System;
    using System.Threading;

    using GHIElectronics.NETMF.Hardware;

    using HomeAutomation.Hardware;

    using Microsoft.SPOT;

    internal class PressureLoggingService : IDisposable
    {
        private readonly SdCard _sdCard;
        private readonly PressureSensor _pressureSensor;
        private Timer _timer;

        public PressureLoggingService(SdCard sdCard, PressureSensor pressureSensor)
        {
            _sdCard = sdCard;
            _pressureSensor = pressureSensor;
        }

        public void Init(int minutes)
        {
            _timer = new Timer(Log, null, new TimeSpan(0, 0, 3), new TimeSpan(0, minutes, 0));
        }

        private void Log(object state)
        {
            var now = RealTimeClock.GetTime();

            string pressureLog = "Pressure_" + now.Year + "_" + now.Month.ToString("D2") + ".csv";

            bool logExists;
            if (!_sdCard.TryIsExists(pressureLog, out logExists))
            {
                return;
            }

            if (!logExists)
            {
                const string PressureLogHeader = "Time,Pressure (bar)\r\n";
                _sdCard.TryAppend(pressureLog, PressureLogHeader);
                Debug.Print(PressureLogHeader);
            }

            var pressureLogText = now.ToString("u").TrimEnd('Z') + "," + _pressureSensor.Pressure.ToString("F2") + "\r\n";
            _sdCard.TryAppend(pressureLog, pressureLogText);
            Debug.Print(pressureLogText);
        }

        public void Dispose()
        {
            if (_sdCard != null)
            {
                _sdCard.Dispose();
            }

            if (_timer != null)
            {
                _timer.Dispose();
            }
        }
    }
}
