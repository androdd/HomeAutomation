namespace HomeAutomation.Services
{
    using HomeAutomation.Tools;

    public class WateringService
    {
        private readonly Log _log;
        private readonly HardwareManager _hardwareManager;

        public int NorthVolume { get; private set; }

        public int SouthVolume { get; private set; }

        public bool[] NorthValves { get; private set; }

        public bool[] SouthValves { get; private set; }

        public WateringService(Log log, HardwareManager hardwareManager)
        {
            _log = log;
            _hardwareManager = hardwareManager;

            SouthValves = new bool[3];
        }
    }
}
