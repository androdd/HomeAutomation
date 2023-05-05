namespace HomeAutomation.Services
{
    using System;

    using AdSoft.Fez.Hardware.Interfaces;
    using AdSoft.Fez.Hardware.Storage;

    using HomeAutomation.Tools;

    using Microsoft.SPOT;

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
            var now = DateTime.Now;

            var add = 5 - now.Minute % 5;
            var schedule = now.AddMinutes(add);
            schedule = new DateTime(schedule.Year, schedule.Month, schedule.Day, schedule.Hour, schedule.Minute, 0);

            _realTimer.TryScheduleRunAt(schedule,
                Log,
                new TimeSpan(0, _configuration.PressureLogIntervalMin, 0),
                "PressureLoggingService ",
                false);
        }

        private bool Log(object state)
        {
            var now = DateTime.Now;

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

            var pressureLogText = now.Day + "," + now.ToString("T") + "," + _pressureSensor.Pressure.ToString("F2");

            pressureLogText += "\r\n";
            _storage.TryAppend(pressureLog, pressureLogText);

            return true;
        }
    }
}
