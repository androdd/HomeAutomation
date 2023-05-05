namespace HomeAutomation.Tools
{
    using System;

    using AdSoft.Fez.Hardware.Storage;

    using GHIElectronics.NETMF.Hardware;

    using Microsoft.SPOT;

    public class Log : Base
    {
        private readonly Configuration _configuration;
        private readonly IStorage _storage;

        public Log(Configuration configuration, IStorage storage)
        {
            _configuration = configuration;
            _storage = storage;
        }

        public void Write(string message)
        {
            var text = Format(RealTimeClock.GetTime()) + " - " + message;
            Debug.Print(text);

            if (_storage != null && !_configuration.ManagementMode)
            {
                var logFile = "Log_" + DateTime.Today.ToString("yyyy_MM_dd") + ".txt";

                _storage.TryAppend(logFile, text + "\r\n");
            }
        }
    }
}
