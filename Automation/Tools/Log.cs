namespace HomeAutomation.Tools
{
    using System;

    using AdSoft.Fez.Hardware.Storage;

    using GHIElectronics.NETMF.Hardware;

    using Microsoft.SPOT;

    public class Log : Base
    {
        private readonly IStorage _storage;

        public Log(IStorage storage)
        {
            _storage = storage;
        }

        public void Write(string message)
        {
            var text = Format(RealTimeClock.GetTime()) + " - " + message;
            Debug.Print(text);

            if (_storage == null)
            {
                return;
            }

            var logFile = "Log_" + DateTime.Today.ToString("yyyy_MM_dd") + ".txt";

            _storage.TryAppend(logFile, text + "\r\n");
        }
    }
}
