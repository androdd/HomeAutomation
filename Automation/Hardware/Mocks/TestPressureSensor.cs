namespace HomeAutomation.Hardware.Mocks
{
    using HomeAutomation.Hardware.Interfaces;

    internal class TestPressureSensor : IPressureSensor
    {
        private int _count;

        private readonly double[] _pressure = { 1, 0.7, 0.4, 0.3, 0.8 };

        public double Voltage
        {
            get { return 1; }
        }

        public double Pressure
        {
            get
            {
                return _pressure[_count++ % _pressure.Length];
            }
        }

        public void Init()
        {
            _count = 0;
        }
    }
}
