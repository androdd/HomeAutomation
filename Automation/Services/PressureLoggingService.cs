namespace HomeAutomation.Services
{
    using System;
    using System.Threading;

    using AdSoft.Fez;
    using AdSoft.Fez.Hardware.Interfaces;
    using AdSoft.Fez.Hardware.Storage;

    using GHIElectronics.NETMF.Hardware;

    using HomeAutomation.Tools;

    using Configuration = HomeAutomation.Tools.Configuration;

    internal class PressureLoggingService : Base
    {
        private readonly Log _log;
        private readonly IStorage _storage;
        private readonly IPressureSensor _pressureSensor;
        private readonly RealTimer _realTimer;
        private readonly Configuration _configuration;

        public PressureLoggingService(Configuration configuration, Log log, IStorage storage, IPressureSensor pressureSensor, RealTimer realTimer)
        {
            _log = log;
            _storage = storage;
            _pressureSensor = pressureSensor;
            _realTimer = realTimer;
            _configuration = configuration;
        }

        public void Init()
        {
            _realTimer.TryScheduleRunAt(DateTime.Now.AddMinutes(1),
                Log,
                new TimeSpan(0, _configuration.PressureLogIntervalMin, 0),
                "PressureLoggingService ",
                false);
        }

        private bool Log(object state)
        {
            var now = RealTimeClock.GetTime();

            var month = now.Month.ToString();
            if (month.Length == 1)
            {
                month = "0" + month;
            }

            string pressureLog = "Pressure_" + now.Year + "_" + month + ".csv";

            bool logExists;
            if (!_storage.TryIsExists(pressureLog, out logExists))
            {
                return true;
            }

            if (!logExists)
            {
                const string PressureLogHeader = "Time,Pressure (bar)\r\n";
                _storage.TryAppend(pressureLog, PressureLogHeader);
                _log.Write("Created " + pressureLog + " file.");
            }

            var pressureLogText = Format(now) + "," + _pressureSensor.Pressure.ToString("F2");
            
            if (!_configuration.ManagementMode)
            {
                pressureLogText += "\r\n";
                _storage.TryAppend(pressureLog, pressureLogText);
            }

            return true;
        }
    }
}
