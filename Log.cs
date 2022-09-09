namespace HomeAutomation
{
    using GHIElectronics.NETMF.Hardware;

    using Microsoft.SPOT;

    internal class Log
    {
        private readonly SdCard _sdCard;

        public Log(SdCard sdCard)
        {
            _sdCard = sdCard;
        }

        public void Write(string message)
        {
            var text = RealTimeClock.GetTime().ToString("u") + " - " + message;
            Debug.Print(text);
            _sdCard.TryAppend("Log.txt", text + "\r\n");
        }
    }
}
