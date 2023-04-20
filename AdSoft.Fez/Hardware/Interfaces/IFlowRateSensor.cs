namespace AdSoft.Fez.Hardware.Interfaces
{
    using System;

    public interface IFlowRateSensor
    {
        double FlowRateMultiplier { get; set; }
        double FlowRate { get; }
        double Volume { get; set; }
#if DEBUG_FLOW_RATE || DEBUG_WATERING
        void OnInterrupt(uint data1, uint data2, DateTime time);
#endif
    }
}