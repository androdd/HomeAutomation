namespace AdSoft.Fez.Hardware.Interfaces
{
    public interface IPumpStateSensor
    {
        bool IsWorking { get; }
        void Init();
    }
}