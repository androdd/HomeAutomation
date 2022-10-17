namespace HomeAutomation.Hardware
{
    using System;

    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    internal class WaterFlowSensor
    {
        private long _lastPulseTime;

        private readonly FEZ_Pin.Interrupt _portId;
        private InterruptPort _interruptPort;

        public WaterFlowSensor(FEZ_Pin.Interrupt portId)
        {
            _portId = portId;
        }

        public void Init()
        {
            _interruptPort = new InterruptPort((Cpu.Pin)_portId, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            _interruptPort.OnInterrupt += OnInterrupt;
        }

        private int _count;
        private double _flowRate;

        private void OnInterrupt(uint data1, uint data2, DateTime time)
        {
            long microseconds  = (time.Ticks - _lastPulseTime) / 10;
            double minutes = microseconds / 60000000.0;
            double frequency = 1 / minutes;
            _flowRate += frequency / 440.0;
            
            _count++;
            var x = 100;
            if (_count % x == 0)
            {
                
                Debug.Print((_flowRate/x).ToString("F"));

                _flowRate = 0;
            }

            _lastPulseTime = time.Ticks;
        }
    }
}
