namespace HomeAutomation
{
    using System;
    using System.Threading;

    using GHIElectronics.NETMF.FEZ;
    using GHIElectronics.NETMF.Hardware;

    using Microsoft.SPOT.Hardware;

    public class Program
    {
        private static Log _log;
        private static RelaysManager _relaysManager;
        private static Configuration _config;

        public static void Main()
        {
            //RealTimeClock.SetTime(new DateTime(2022, 8, 17, 14, 48, 0));

            var sdCard = new SdCard();
            _log = new Log(sdCard);

            _config = new Configuration(sdCard, _log);
            _config.Load();

            _relaysManager = new RelaysManager();
            _relaysManager.Init();

            TimerEx timerEx = new TimerEx(_log);

            _log.Write("Starting...");
            
            InterruptPort interrupt = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.An0,
                false,
                Port.ResistorMode.Disabled,
                Port.InterruptMode.InterruptEdgeBoth);

            interrupt.OnInterrupt += Interrupt_OnInterrupt;

            var now = RealTimeClock.GetTime();

            var nextDay = now.AddDays(1);
            var nextMidnight = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day);
            
            timerEx.TryScheduleRunAt(nextMidnight, ReloadConfig, new TimeSpan(24, 0, 0));

            var nextRun = now.AddSeconds(20);

            timerEx.TryScheduleRunAt(nextRun, NextRun);

            _log.Write("Started");

            Thread.Sleep(Timeout.Infinite);
        }

        private static void NextRun(object state)
        {
            _relaysManager.Set(4, true);
        }

        private static void ReloadConfig(object state)
        {
            _config.Load();
        }

        private static void Interrupt_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            _config.Load();
            
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
    }
}
