namespace HomeAutomation.Services.Watering
{
    using System;

    public class ValveConfiguration
    {
        public ValveConfiguration(string configuration)
        {
            
        }

        public int RelayId { get; set; }

        public DateTime StartTime { get; set; }

        public int Duration { get; set; }

        public DaysOfWeek DaysOfWeek { get; set; }

        public bool ContainsDay(DayOfWeek day)
        {
            var mask = 1 << (int)day;

            return (mask & (int)DaysOfWeek) == mask;
        }


    }
}