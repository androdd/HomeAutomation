namespace HomeAutomation.Hardware.Mocks
{
    using AdSoft.Fez.Hardware.Interfaces;

    internal class PumpStateSensorMock : IPumpStateSensor
    {
        public bool IsWorking { get; set; }

        public void Init()
        {
            IsWorking = true;
        }
    }
}