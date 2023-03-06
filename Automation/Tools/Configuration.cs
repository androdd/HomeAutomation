namespace HomeAutomation.Tools
{
    using System;

    public class Configuration
    {
        public bool ManagementMode { get; set; }

        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }

        public int SunriseOffsetMin { get; set; }
        public int SunsetOffsetMin { get; set; }
        public int PressureLogIntervalMin { get; set; }
        public double PressureSensorMultiplier { get; set; }
        public double WaterFlowSensorMultiplier { get; set; }

        public Services.AutoTurnOffPump.Configuration AutoTurnOffPumpConfiguration { get; private set; }

        public bool IsDst { get; set; }
        public bool ManualStartDst { get; set; }
        public bool ManualEndDst { get; set; }
        
        public Configuration()
        {
            // Until there is no real configuration loaded ManagementMode is in effect
            ManagementMode = true;

            AutoTurnOffPumpConfiguration = new Services.AutoTurnOffPump.Configuration();

            // Hardcoded values if config is missing

            Sunrise = new DateTime(2022, 2, 22, 6, 0, 0);
            Sunset = new DateTime(2022, 2, 22, 19, 0, 0);

            SunriseOffsetMin = 0;
            SunsetOffsetMin = 0;
            PressureLogIntervalMin = 5;
            PressureSensorMultiplier = 1;
            WaterFlowSensorMultiplier = 1;

            AutoTurnOffPumpConfiguration.Interval = 1;
            AutoTurnOffPumpConfiguration.MinPressure = 0.5;
            AutoTurnOffPumpConfiguration.MaxEventsCount = 2;
            AutoTurnOffPumpConfiguration.SignalLength = 500;
        }
    }
}
