namespace HomeAutomation.Hardware
{
    using GHIElectronics.NETMF.FEZ;
    using GHIElectronics.NETMF.Hardware;

    internal class PressureSensor
    {
        private readonly FEZ_Pin.AnalogIn _portId;
        private AnalogIn _pressureSensor;

        public PressureSensor(FEZ_Pin.AnalogIn portId)
        {
            _portId = portId;
        }

        public double Voltage
        {
            get { return _pressureSensor.Read() / 1000.0; }
        }

        public double Pressure
        {
            get { return (Voltage - 0.5) / 0.756833333; }
        }

        public void Init()
        {
            _pressureSensor = new AnalogIn((AnalogIn.Pin)_portId);
            _pressureSensor.SetLinearScale(0, 3300 * 2); // Voltage divider is installed
        }
    }
}