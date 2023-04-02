namespace AdSoft.Fez.Hardware.Interfaces
{
    public interface IFlowRateSensor
    {
        double FlowRateMultiplier { get; set; }
        double FlowRate { get; }
        double Volume { get; set; }
    }
}