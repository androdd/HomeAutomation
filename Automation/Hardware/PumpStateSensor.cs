namespace HomeAutomation.Hardware
{
    using GHIElectronics.NETMF.FEZ;

    using HomeAutomation.Hardware.Interfaces;

    using Microsoft.SPOT.Hardware;

    internal class PumpStateSensor : IPumpStateSensor
    {
        private readonly FEZ_Pin.Digital _portId;
        private InputPort _inputPort;

        public PumpStateSensor(FEZ_Pin.Digital portId)
        {
            _portId = portId;
        }

        public bool IsWorking
        {
            get
            {
                return _inputPort.Read();
            }
        }

        public void Init()
        {
            _inputPort = new InputPort((Cpu.Pin)_portId, false, Port.ResistorMode.PullUp);
        }
    }
}
