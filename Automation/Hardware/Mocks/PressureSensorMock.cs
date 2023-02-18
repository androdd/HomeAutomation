namespace HomeAutomation.Hardware.Mocks
{
    using HomeAutomation.Hardware.Interfaces;

    internal class PressureSensorMock : IPressureSensor
    {
        public double Voltage { get; set; }

        public double Pressure { get; set; }

        public void Init()
        {
            Voltage = 3;
            Pressure = 2;
        }
    }
}
