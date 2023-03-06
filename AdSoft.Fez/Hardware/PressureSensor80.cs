namespace AdSoft.Fez.Hardware
{
    using AdSoft.Fez.Hardware.Interfaces;

    using GHIElectronics.NETMF.FEZ;
    using GHIElectronics.NETMF.Hardware;

    public class PressureSensor80 : IPressureSensor
    {
        private readonly FEZ_Pin.AnalogIn _portId;
        private AnalogIn _pressureSensor;

        public PressureSensor80(FEZ_Pin.AnalogIn portId)
        {
            _portId = portId;
        }

        public double Voltage
        {
            get { return _pressureSensor.Read() / 1000.0; }
        }

        public double Pressure
        {
            get { return (Voltage - 0.5) * PressureMultiplier / 0.756833333; }

            // Formula: P = (V - b) / a
            // Sensor                       b       a
            // 80 psi Experimental (air)    0.48    0.756833333
            // 80 psi Factory               0.5     0.727272727
        }

        public double PressureMultiplier { get; set; }

        public void Init()
        {
            _pressureSensor = new AnalogIn((AnalogIn.Pin)_portId);
            _pressureSensor.SetLinearScale(0, 3300 * 2); // Voltage divider is installed
        }
    }
}