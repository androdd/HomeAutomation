namespace HomeAutomation.Hardware
{
    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT.Hardware;

    internal class RelaysArray
    {
        private const bool RelayTrue = false;
        private OutputPort[] _relays;

        public int Count
        {
            get { return _relays.Length; }
        }

        public void Init()
        {
            Cpu.Pin[] diPins = {
                (Cpu.Pin)FEZ_Pin.Digital.Di0,
                (Cpu.Pin)FEZ_Pin.Digital.Di1,
                (Cpu.Pin)FEZ_Pin.Digital.Di2,
                (Cpu.Pin)FEZ_Pin.Digital.Di3,
                (Cpu.Pin)FEZ_Pin.Digital.Di4,
                (Cpu.Pin)FEZ_Pin.Digital.Di5,
                (Cpu.Pin)FEZ_Pin.Digital.Di6,
                (Cpu.Pin)FEZ_Pin.Digital.Di7
            };
            
            _relays = new OutputPort[diPins.Length];

            for (int i = 0; i < diPins.Length; i++)
            {
                _relays[i] = new OutputPort(diPins[i], !RelayTrue);
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
