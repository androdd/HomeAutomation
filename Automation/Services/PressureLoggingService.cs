namespace HomeAutomation.Services
{
    using System;
    using System.Threading;

    using AdSoft.Fez;
    using AdSoft.Fez.Hardware.Interfaces;
    using AdSoft.Fez.Hardware.SdCard;

    using GHIElectronics.NETMF.Hardware;

    using HomeAutomation.Tools;

    using Configuration = HomeAutomation.Tools.Configuration;

    internal class PressureLoggingService : Base, IDisposable
    {
        private readonly Log _log;
        private readonly SdCard _sdCard;
        private readonly IPressureSensor _pressureSensor;
        private readonly Configuration _configuration;
        private Timer _timer;

        public PressureLoggingService(Configuration configuration, Log log, SdCard sdCard, IPressureSensor pressureSensor)
        {
            _log = log;
            _sdCard = sdCard;
            _pressureSensor = pressureSensor;
            _configuration = configuration;
        }

        public void Init(int minutes)
        {
            var startDelay = new TimeSpan(0, 0, 3);

            _timer = new Timer(Log, null, startDelay, new TimeSpan(0, minutes, 0));

            _log.Write("Pressure Logging Timer set for: " + Format(RealTimeClock.GetTime().Add(startDelay)) + " with period: " + minutes + " m");
        }

        private void Log(object state)
        {
            var now = RealTimeClock.GetTime();

            var month = now.Month.ToString();
            if (month.Length == 1)
            {
                month = "0" + month;
            }

            string pressureLog = "Pressure_" + now.Year + "_" + month + ".csv";

            bool logExists;
            if (!_sdCard.TryIsExists(pressureLog, out logExists))
            {
                return;
            }

            if (!logExists)
            {
                const string PressureLogHeader = "Time,Pressure (bar)\r\n";
                _sdCard.TryAppend(pressureLog, PressureLogHeader);
                _log.Write("Created " + pressureLog + " file.");
            }

            var pressureLogText = Format(now) + "," + _pressureSensor.Pressure.ToString("F2");
            DebugEx.Print(DebugEx.Target.PressureLog, pressureLogText);
            
            if (!_configuration.ManagementMode)
            {
                pressureLogText += "\r\n";
                _sdCard.TryAppend(pressureLog, pressureLogText);
            }
            
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
