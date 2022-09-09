namespace HomeAutomation
{
    using System;

    public class WaterData
    {
        public DateTime Timestamp { get; set; }

        public int Pressure { get; set; }

        public override string ToString()
        {
            return Timestamp.ToString("u") + " - " + Pressure;
        }
    }
}