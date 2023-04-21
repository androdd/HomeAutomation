namespace HomeAutomation.Tools
{
    using AdSoft.Fez.Hardware.Storage;

    using GHIElectronics.NETMF.Hardware;

    using Microsoft.SPOT;

    public class Log : Base
    {
        private readonly Configuration _configuration;
        private readonly SdCard _sdCard;

        public Log(Configuration configuration, SdCard sdCard)
        {
            _configuration = configuration;
            _sdCard = sdCard;
        }

        public void Write(string message)
        {
            var text = Format(RealTimeClock.GetTime()) + " - " + message;
            Debug.Print(text);

            if (_sdCard != null && !_configuration.ManagementMode)
            {
                _sdCard.TryAppend("Log.txt", text + "\r\n");
            }
        }
    }
}
