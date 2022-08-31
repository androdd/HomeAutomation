namespace HomeAutomation
{
    using System;
    using System.Collections;
    using System.Threading;

    using GHIElectronics.NETMF.FEZ;
    using GHIElectronics.NETMF.Hardware;

    using Microsoft.SPOT.Hardware;

    public class Program
    {
        private static InputPort _pressureSensor;

        private static ArrayList _pressureData;

        private static Log _log;
        private static RelaysManager _relaysManager;

        public static void Main()
        {
            //RealTimeClock.SetTime(new DateTime(2022, 8, 17, 14, 48, 0));

            var sdCard = new SdCard();
            _log = new Log(sdCard);

            Configuration config = new Configuration(sdCard, _log);
            config.Load();

            _relaysManager = new RelaysManager();
            _relaysManager.Init();

            _pressureSensor = new InputPort((Cpu.Pin)FEZ_Pin.Digital.Di9, false, Port.ResistorMode.PullUp);

            while (true)
            {
                var now = RealTimeClock.GetTime();

                if (now.Hour == 0 && now.Minute == 0)
                {
                    config.Load();
                }

                ManageLights(config, now);

                Thread.Sleep(1000);
            }


            _pressureData = new ArrayList();

            //Timer pressureSensorTimer = new Timer(pressureSensorTimer_Execute, null, 3000, 3000);

            Thread.Sleep(Timeout.Infinite);
        }

        private static void ManageLights(Configuration config, DateTime now)
        {
            var lightsOn = config.Sunrise >= now || now >= config.Sunset;

            _relaysManager.Set(0, lightsOn);
        }

        public static void MainOld()
        {
            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, false);

            LegoRemote remote = new LegoRemote((Cpu.Pin)FEZ_Pin.Interrupt.Di11);
            remote.OnLegoButtonPress += remote_OnLegoButtonPress;

            InputPort pressureSensor = new InputPort((Cpu.Pin)FEZ_Pin.Digital.Di9, false, Port.ResistorMode.PullUp);

            led.Write(true);
            Thread.Sleep(600);
            led.Write(false);
            Thread.Sleep(600);

            while (true)
            {
                var hasPressure = pressureSensor.Read();

                _relaysManager.Set(5, hasPressure);

                Thread.Sleep(1000);
            }
        }

        static void remote_OnLegoButtonPress(Message msg)
        {
            if (msg.CommandA == Command.ComboDirectForward)
            {
                _relaysManager.Set(4, true);
            }

            if (msg.CommandA == Command.ComboDirectBackward)
            {
                _relaysManager.Set(4, true);
            }
        }

        static void pressureSensorTimer_Execute(object state)
        {
            if (_pressureData.Count > 10)
            {
                _pressureData.Clear();
            }

            var hasPressure = _pressureSensor.Read();

            var waterData = new WaterData { Timestamp = RealTimeClock.GetTime(), Pressure = hasPressure ? 1 : 0 };
            _pressureData.Add(waterData);

            _log.Write(waterData.ToString());
        }
    }
}
