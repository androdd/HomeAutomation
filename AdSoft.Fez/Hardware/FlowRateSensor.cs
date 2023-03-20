namespace AdSoft.Fez.Hardware
{
    using System;

    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    public class FlowRateSensor
    {
        const int MeasurementsCount = 100;

        private long _lastPulseTicks;
        private int _count;
        private double _flowRate;
        private double _lastFlowRate;

        private readonly FEZ_Pin.Interrupt _portId;
        private InterruptPort _interruptPort;

        public FlowRateSensor(FEZ_Pin.Interrupt portId)
        {
            _portId = portId;

            _lastFlowRate = 0;
        }

        public double FlowRateMultiplier { get; set; }

        public double FlowRate
        {
            get
            {
                if(_lastFlowRate > 0)
                {
                    long milliseconds = (DateTime.Now.Ticks - _lastPulseTicks) / 10000;

                    if (milliseconds > 500)
                    {
                        _lastFlowRate = 0;
                    }
                }

                return _lastFlowRate * FlowRateMultiplier;
            }
        }

        public void Init()
        {
            _interruptPort = new InterruptPort((Cpu.Pin)_portId, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            _interruptPort.OnInterrupt += OnInterrupt;
        }

        private void OnInterrupt(uint data1, uint data2, DateTime time)
        {
            long microseconds  = (time.Ticks - _lastPulseTicks) / 10;
            double minutes = microseconds / 60000000.0;
            double frequency = 1 / minutes;
            _flowRate += frequency / 440.0;
            
            _count++;

            if (_count % MeasurementsCount == 0)
            {

                _lastFlowRate = _flowRate / MeasurementsCount;

                _flowRate = 0;
            }

            _lastPulseTicks = time.Ticks;
        }
    }
}
