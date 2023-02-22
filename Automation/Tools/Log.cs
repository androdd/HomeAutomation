namespace HomeAutomation.Tools
{
    using AdSoft.Fez.Hardware.SdCard;

    using GHIElectronics.NETMF.Hardware;

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
