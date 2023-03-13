namespace HomeAutomation.Hardware.Mocks
{
    using AdSoft.Fez.Hardware.Interfaces;

    internal class PressureSensorMock : IPressureSensor
    {
        private double _pressure;

        public double Voltage { get; set; }

        public double Pressure
        {
            get { return _pressure * PressureMultiplier; }
            set { _pressure = value; }
        }

        public double PressureMultiplier { get; set; }

        public void Init()
        {
            Voltage = 3;
            Pressure = 2;
            PressureMultiplier = 1;
        }
    }
}
