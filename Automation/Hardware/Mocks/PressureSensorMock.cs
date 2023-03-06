namespace HomeAutomation.Hardware.Mocks
{
    using AdSoft.Fez.Hardware.Interfaces;

    internal class PressureSensorMock : IPressureSensor
    {
        public double Voltage { get; set; }

        public double Pressure { get; set; }

        public double PressureMultiplier { get; set; }

        public void Init()
        {
            Voltage = 3;
            Pressure = 2;
            PressureMultiplier = 1;
        }
    }
}
