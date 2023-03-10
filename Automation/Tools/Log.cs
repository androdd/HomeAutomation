namespace HomeAutomation.Tools
{
    using AdSoft.Fez;
    using AdSoft.Fez.Hardware.SdCard;

    using GHIElectronics.NETMF.Hardware;

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
            DebugEx.Print(text);

            if (_sdCard != null && !_configuration.ManagementMode)
            {
                _sdCard.TryAppend("Log.txt", text + "\r\n");
            }
        }
    }
}
