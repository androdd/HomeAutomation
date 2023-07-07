namespace HomeAutomation.Tools
{
    using System;

    using HomeAutomation.Services.Watering;

    public class Configuration
    {
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }

        public int SunriseOffsetMin { get; set; }
        public int SunsetOffsetMin { get; set; }
        public int PressureLogIntervalMin { get; set; }
        public double PressureSensorMultiplier { get; set; }
        public double FlowRateSensorMultiplier { get; set; }

        public Services.AutoTurnOffPump.Configuration AutoTurnOffPumpConfiguration { get; private set; }

        public ValveConfiguration[] SouthValveConfigurations { get; private set; }

        public ValveConfiguration NorthValveConfiguration { get; set; }

        public bool IsDst { get; set; }
        
        public Configuration()
        {
            AutoTurnOffPumpConfiguration = new Services.AutoTurnOffPump.Configuration();

            // Hardcoded values if config is missing

            Sunrise = new DateTime(2022, 2, 22, 6, 0, 0);
            Sunset = new DateTime(2022, 2, 22, 19, 0, 0);

            SunriseOffsetMin = 0;
            SunsetOffsetMin = 0;
            PressureLogIntervalMin = 5;
            PressureSensorMultiplier = 1;
            FlowRateSensorMultiplier = 1;

            AutoTurnOffPumpConfiguration.Interval = 1;
            AutoTurnOffPumpConfiguration.MinPressure = 0.5;
            AutoTurnOffPumpConfiguration.MaxEventsCount = 2;
            AutoTurnOffPumpConfiguration.SignalLength = 500;

            SouthValveConfigurations = new[]
            {
                new ValveConfiguration("0 0 00|00 1 0000000"), 
                new ValveConfiguration("0 0 00|00 1 0000000"),
                new ValveConfiguration("0 0 00|00 1 0000000"), 
                new ValveConfiguration("0 0 00|00 1 0000000")
            };

            NorthValveConfiguration = new ValveConfiguration("0 0 00|00 1 0000000");
        }
    }
}
