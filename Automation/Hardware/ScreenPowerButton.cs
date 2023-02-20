namespace HomeAutomation.Hardware
{
    using System;

    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT.Hardware;

    internal class ScreenPowerButton
    {
        private readonly FEZ_Pin.Digital _portId;
        private readonly Lcd2004 _screen;
        private InputPort _interruptPort;

        public ScreenPowerButton(FEZ_Pin.Digital portId, Lcd2004 screen)
        {
            _portId = portId;
            _screen = screen;
        }

        public void Init()
        {
            _interruptPort = new InterruptPort((Cpu.Pin)_portId, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
            _interruptPort.OnInterrupt += InterruptPortOnOnInterrupt;
        }

        private void InterruptPortOnOnInterrupt(uint data1, uint data2, DateTime time)
        {
            bool isTurnedOn = _interruptPort.Read();

            if (isTurnedOn)
            {
                _screen.Init();
                _screen.BackLightOn();
            }
        }
    }
}