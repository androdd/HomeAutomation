namespace HomeAutomation.Services.Watering
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware;

    using HomeAutomation.Tools;

    using Microsoft.SPOT;

    public class WateringService
    {
        private readonly Log _log;
        private readonly RealTimer _realTimer;
        private readonly HardwareManager _hardwareManager;

        public int NorthVolume { get; private set; }

        public int SouthVolume { get; private set; }

        public int NorthSwitchState { get; private set; }
        
        public WateringService(Log log, RealTimer realTimer, HardwareManager hardwareManager)
        {
            _log = log;
            _realTimer = realTimer;
            _hardwareManager = hardwareManager;
        }

        public void Start()
        {
            _realTimer.TryScheduleRunAt(Program.Now.AddSeconds(5), TimerCallback, "Watering");
        }

        private void TimerCallback(object state)
        {
            var _wateringThread = new Thread(StartWatering);
            _wateringThread.Start();
        }
        
        private void StartWatering()
        {
            SetValve(Valve.SouthMain, true);
            Thread.Sleep(100);
            
            Thread.Sleep(30 * 1000);
            
            Thread.Sleep(3 * 1000);
            SetValve(Valve.FlowersDrip, false);

            int flowerCycles = 0, flowersMaxCycles = 5;


            while (true)
            {
                if (flowerCycles == 0)
                {
                    SetValve(Valve.FlowersDrip, true);
                }
                
                if (flowerCycles == flowersMaxCycles)
                {
                    SetValve(Valve.FlowersDrip, false);
                }

                flowerCycles++;
                
                Thread.Sleep(10 * 1000);
            }

            SetValve(Valve.SouthMain, false);
        }

        public bool GetValve(Valve valve)
        {
            var relayId = GetRelayId(valve);
            var state = _hardwareManager.RelaysArray.Get(relayId);

            return state;
        }

        private void SetValve(Valve valve, bool isOpen)
        {
            var relayId = GetRelayId(valve);

            _hardwareManager.RelaysArray.Set(relayId, isOpen);

            _log.Write(ValveToString(valve) + " valve is " + (isOpen ? "opened" : "closed") + ".");
        }

        private int GetRelayId(Valve valve)
        {
            int relayId;
            switch (valve)
            {
                case Valve.SouthMain:
                    relayId = _hardwareManager.SouthValveRelayId;
                    break;
                case Valve.FlowersDrip:
                    relayId = _hardwareManager.FlowersDripRelayId;
                    break;
                case Valve.VegetablesDrip:
                    relayId = _hardwareManager.VegetablesDripRelayId;
                    break;
                case Valve.GrassDrip:
                    relayId = _hardwareManager.GrassDripRelayId;
                    break;
                case Valve.NorthMain:
                    relayId = _hardwareManager.NorthValveRelayId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("valve");
            }

            return relayId;
        }

        private string ValveToString(Valve valve)
        {
            switch (valve)
            {
                case Valve.SouthMain:
                    return "South Main";
                case Valve.FlowersDrip:
                    return "Flowers";
                case Valve.VegetablesDrip:
                    return "Vegetables";
                case Valve.GrassDrip:
                    return "Grass";
                case Valve.NorthMain:
                    return "North Main";
                default:
                    throw new ArgumentOutOfRangeException("valve");
            }
        }
    }
}
