namespace HomeAutomation.Tools
{
    using GHIElectronics.NETMF.Hardware;

    using HomeAutomation.Hardware;
    using HomeAutomation.Hardware.SdCard;
    using HomeAutomation.Services;

    using Microsoft.SPOT;

    internal class Log : Base
    {
        private readonly SdCard _sdCard;

        public Log(SdCard sdCard)
        {
            _sdCard = sdCard;
        }

        public void Write(string message)
        {
            var text = Format(RealTimeClock.GetTime()) + " - " + message;
            Debug.Print(text);
            _sdCard.TryAppend("Log.txt", text + "\r\n");
        }
    }
}
