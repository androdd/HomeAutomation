namespace AdSoft.Fez.Hardware
{
    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT.Hardware;

    public class RelaysArray
    {
        private readonly FEZ_Pin.Digital[] _portIds;
        private const bool RelayTrue = false;
        private OutputPort[] _relays;

        public int Count
        {
            get { return _relays.Length; }
        }

        public RelaysArray(FEZ_Pin.Digital[] portIds)
        {
            _portIds = portIds;
        }

        public void Init()
        {
            _relays = new OutputPort[_portIds.Length];

            for (int i = 0; i < _portIds.Length; i++)
            {
                _relays[i] = new OutputPort((Cpu.Pin)_portIds[i], !RelayTrue);
            }
        }

        public bool Get(int relay)
        {
            return !_relays[relay].Read();
        }

        public void Set(int relay, bool value)
        {
            _relays[relay].Write(!value);
        }
    }
}
