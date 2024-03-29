namespace AdSoft.Fez.Hardware
{
    using System;
    using System.Runtime.CompilerServices;

    using AdSoft.Fez.Hardware.Interfaces;

    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    public class FlowRateSensor : IFlowRateSensor
    {
        const int MeasurementsCount = 10;

        private long _lastPulseTicks;
        private long _lastMeasurementTicks;
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
        
        public double Volume { get; set; }

        public void Init()
        {
            _interruptPort = new InterruptPort((Cpu.Pin)_portId, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            _interruptPort.OnInterrupt += OnInterrupt;
        }

#if DEBUG_FLOW_RATE || DEBUG_WATERING
        public void OnInterrupt(uint data1, uint data2, DateTime time)
#else
        private void OnInterrupt(uint data1, uint data2, DateTime time)
#endif
        {
            long microseconds  = (time.Ticks - _lastPulseTicks) / 10;

            if (microseconds > 500000)
            {
                _lastMeasurementTicks = time.Ticks;
                _lastPulseTicks = time.Ticks;
                return;
            }

            double minutes = microseconds / 60000000.0;
            double frequency = 1 / minutes;
            _flowRate += frequency / 440.0;
            
            _count++;

            if (_count % MeasurementsCount == 0)
            {
                _lastFlowRate = _flowRate / MeasurementsCount;

                _flowRate = 0;

                if (_lastMeasurementTicks == 0)
                {
                    _lastMeasurementTicks = time.Ticks;
                }

                var elapsedMicroseconds = (time.Ticks - _lastMeasurementTicks) / 10;
                var elapsedMinutes = elapsedMicroseconds / 60000000.0;
                var flowRate = _lastFlowRate * FlowRateMultiplier;
                Volume += flowRate * elapsedMinutes;

                _lastMeasurementTicks = time.Ticks;
            }

            _lastPulseTicks = time.Ticks;
        }
    }
}
