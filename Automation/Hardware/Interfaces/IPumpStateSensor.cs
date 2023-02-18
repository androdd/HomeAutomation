namespace HomeAutomation.Hardware.Interfaces
{
    internal interface IPumpStateSensor
    {
        bool IsWorking { get; }
        void Init();
    }
}