namespace HomeAutomation.Services.Watering
{
    using System;

    public class TimeInterval
    {
        public TimeInterval(DateTime start, DateTime end, int configId)
        {
            Start = start;
            End = end;
            ConfigId = configId;
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int ConfigId { get; set; }
    }
}