namespace HomeAutomation.Services.Watering
{
    using System;

    using AdSoft.Fez;

    public class ValveConfiguration
    {
        public ValveConfiguration(string configuration)
        {
            if (configuration == null)
            {
                IsValid = false;
                return;
            }

            var parts = configuration.Split(' ');

            if (parts.Length != 5)
            {
                IsValid = false;
                return;
            }

            int isEnabled;
            if (!Converter.TryParse(parts[0], out isEnabled))
            {
                IsValid = false;
                return;
            }

            IsEnabled = isEnabled > 0;

            int relayId;
            if (!Converter.TryParse(parts[1], out relayId) || relayId < 0)
            {
                IsValid = false;
                return;
            }

            RelayId = relayId;

            DateTime starTime;
            if (!Converter.TryParseTime(parts[2], out starTime))
            {
                IsValid = false;
                return;
            }

            StartTime = starTime;

            int duration;
            if (!Converter.TryParse(parts[3], out duration) || duration <= 0 || duration > 60)
            {
                IsValid = false;
                return;
            }

            Duration = duration;

            var days = parts[4];
            if (days.Length != 7)
            {
                IsValid = false;
                return;
            }

            Watering.DaysOfWeek daysOfWeek = DaysOfWeek.None;
            for (int i = 0; i < 7; i++)
            {
                if (days[i] == '0')
                {
                    continue;
                }

                daysOfWeek = (DaysOfWeek)((int)daysOfWeek | (1 << i));
            }

            DaysOfWeek = daysOfWeek;

            IsValid = true;
        }

        public bool IsValid { get; private set; }

        public bool IsEnabled { get; private set; }

        public int RelayId { get; private set; }

        public DateTime StartTime { get; private set; }

        public int Duration { get; private set; }

        public DaysOfWeek DaysOfWeek { get; private set; }

        public bool ContainsDay(DayOfWeek day)
        {
            var mask = 1 << (int)day;

            return (mask & (int)DaysOfWeek) == mask;
        }


    }
}