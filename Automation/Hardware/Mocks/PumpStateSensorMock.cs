namespace HomeAutomation.Hardware.Mocks
{
    using HomeAutomation.Hardware.Interfaces;

    internal class PumpStateSensorMock : IPumpStateSensor
    {
        public bool IsWorking { get; set; }

        public void Init()
        {
            IsWorking = true;
        }
    }
}