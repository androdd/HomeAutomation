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
            Debug.Print(message);
            _sdCard.TryAppend("Log.txt", RealTimeClock.GetTime().ToString("u") + " - " + message + "\r\n");
        }
    }
}
