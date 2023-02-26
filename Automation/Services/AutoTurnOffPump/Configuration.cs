namespace HomeAutomation.Services.AutoTurnOffPump
{
    public class Configuration
    {
        public byte Interval { get; set; }

        public double MinPressure { get; set; }

        public byte MaxEventsCount { get; set; }

        public ushort SignalLength { get; set; }
    }
}
