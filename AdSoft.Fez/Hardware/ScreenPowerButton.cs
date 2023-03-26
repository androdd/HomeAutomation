namespace AdSoft.Fez.Hardware
{
    using System;

    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT.Hardware;

    public class ScreenPowerButton
    {
        private readonly FEZ_Pin.Digital _portId;
        private readonly Lcd2004.Lcd2004 _screen;
        private InputPort _interruptPort;

        public delegate void StateChangedEventHandler(bool isOn);

        public event StateChangedEventHandler StateChanged;

        public ScreenPowerButton(FEZ_Pin.Digital portId, Lcd2004.Lcd2004 screen)
        {
            _portId = portId;
            _screen = screen;
        }

        public void Init()
        {
            DebugEx.Print(DebugEx.Target.ScreenPowerButton, "Init");

            _interruptPort = new InterruptPort((Cpu.Pin)_portId, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
            _interruptPort.OnInterrupt += InterruptPortOnOnInterrupt;

            InitScreenIfOn();
        }

        private void InterruptPortOnOnInterrupt(uint data1, uint data2, DateTime time)
        {
            InitScreenIfOn();
        }

        private void InitScreenIfOn()
        {
            bool isTurnedOn = _interruptPort.Read();

            DebugEx.Print(DebugEx.Target.ScreenPowerButton, "InitScreenIfOn: " + isTurnedOn);

            if (isTurnedOn)
            {
                _screen.Init();
                _screen.BackLightOn();
            }

            if (StateChanged != null)
            {
                StateChanged(isTurnedOn);
            }
        }
    }
}