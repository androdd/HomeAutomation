namespace AdSoft.Fez.Hardware
{
    using System.Threading;

    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT.Hardware;

    public class Led
    {
        private readonly FEZ_Pin.Digital _portId;
        private static OutputPort _led;

        public Led(FEZ_Pin.Digital portId)
        {
            _portId = portId;
        }

        public void Init()
        {
            _led = new OutputPort((Cpu.Pin)_portId, false);
        }

        public void Set(bool on)
        {
            _led.Write(on);
        }
        
        public void BlinkAsync(int count, int period = 700, bool keepState = false)
        {
            var thread = new Thread(() => Blink(count, period, keepState));
            thread.Start();
        }

        public void Blink(int count, int period = 700, bool keepState = false)
        {
            if (period < 100)
            {
                period = 100;
            }

            bool state = _led.Read();

            _led.Write(true);

            for (int i = 0; i < count * 2; i++)
            {
                Thread.Sleep(period);
                _led.Write(!_led.Read());
            }
            
            _led.Write(keepState && state);
        }
    }
}