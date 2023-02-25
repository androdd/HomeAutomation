namespace HomeAutomation.Tools
{
    using AdSoft.Fez.Hardware.SdCard;

    using GHIElectronics.NETMF.Hardware;

    using Microsoft.SPOT;

    public class Log : Base
    {
        private SdCard _sdCard;

        public void AddSdCard(SdCard sdCard)
        {
            _sdCard = sdCard;
        }

        public void Write(string message)
        {
            var text = Format(RealTimeClock.GetTime()) + " - " + message;
            Debug.Print(text);

            if (_sdCard != null && !Program.ManagementMode)
            {
                _sdCard.TryAppend("Log.txt", text + "\r\n");
            }
        }
    }
}
