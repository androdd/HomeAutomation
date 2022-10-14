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

        private int count = 0;

        private void OnInterrupt(uint data1, uint data2, DateTime time)
        {
            long microseconds  = (time.Ticks - _lastPulseTime) / 10;
            double minutes = microseconds / 60000000.0;
            double frequency = 1 / minutes;
            double flowRate = frequency / 6.6;
            
            count++;
            if (count % 10 == 0)
            {
                Debug.Print(flowRate.ToString("F"));
            }

            _lastPulseTime = time.Ticks;
        }
    }
}
