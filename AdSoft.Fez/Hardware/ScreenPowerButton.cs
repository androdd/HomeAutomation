namespace AdSoft.Fez.Hardware
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Ui;

    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    public class ScreenPowerButton
    {
        private readonly FEZ_Pin.Interrupt _portId;
        private readonly Lcd2004.Lcd2004 _screen;
        private ScreenSaver _screenSaver;
        private InputPort _interruptPort;

        public delegate void StateChangedEventHandler(bool isOn);

        public event StateChangedEventHandler StateChanged;

        public ScreenPowerButton(FEZ_Pin.Interrupt portId, Lcd2004.Lcd2004 screen)
        {
            _portId = portId;
            _screen = screen;
        }

        public void Init()
        {
            _interruptPort = new InterruptPort((Cpu.Pin)_portId, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
            _interruptPort.OnInterrupt += InterruptPortOnOnInterrupt;

            InitScreenIfOn();
        }

        public void AddScreenSaver(ScreenSaver screenSaver)
        {
            _screenSaver = screenSaver;
        }

        private void InterruptPortOnOnInterrupt(uint data1, uint data2, DateTime time)
        {
            InitScreenIfOn();
        }

        private void InitScreenIfOn()
        {
            bool isTurnedOn = _interruptPort.Read();

            if (isTurnedOn)
            {
                _screen.Init();
                _screen.BackLightOn();
                EnableScreenSaver();
            }
            else
            {
                EnableScreenSaver(false);
            }

            if (StateChanged != null)
            {
                StateChanged(isTurnedOn);
            }
        }

        private void EnableScreenSaver(bool isEnabled = true)
        {
            if (_screenSaver != null)
            {
                _screenSaver.Enable(isEnabled);
            }
        }
    }
}