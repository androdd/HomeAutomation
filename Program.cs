namespace HomeAutomation
{
    using System.Collections;
    using System.Threading;

    using GHIElectronics.NETMF.FEZ;
    using GHIElectronics.NETMF.Hardware;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    public class Program
    {
        static OutputPort[] relays;

        static InputPort pressureSensor;

        static ArrayList pressureData;
        
        public static void Main()
        {
            //RealTimeClock.SetTime(new DateTime(2022, 8, 17, 14, 48, 0));

            var sdCard = new SdCard();

            string sunToday;

            var now = RealTimeClock.GetTime();

            string month = now.Month.ToString();
            if (month.Length == 1)
            {
                month = "0" + month;
            }

            if (sdCard.TryReadFixedLengthLine("SunDst" + month + ".txt", 19, now.Day, out sunToday))
            {
                Debug.Print(sunToday);
            }
            
            pressureSensor = new InputPort((Cpu.Pin)FEZ_Pin.Digital.Di9, false, Port.ResistorMode.PullUp);

            pressureData = new ArrayList();

            //Timer pressureSensorTimer = new Timer(new TimerCallback(pressureSensorTimer_Execute), null, 3000, 3000);

            Thread.Sleep(Timeout.Infinite);
        }

        public static void MainOld()
        {
            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, false);

            bool relayTrue = false;
            relays = new OutputPort[8];
            Cpu.Pin[] diPins = new Cpu.Pin[8] {
                (Cpu.Pin)FEZ_Pin.Digital.Di0,
                (Cpu.Pin)FEZ_Pin.Digital.Di1,
                (Cpu.Pin)FEZ_Pin.Digital.Di2,
                (Cpu.Pin)FEZ_Pin.Digital.Di3,
                (Cpu.Pin)FEZ_Pin.Digital.Di4,
                (Cpu.Pin)FEZ_Pin.Digital.Di5,
                (Cpu.Pin)FEZ_Pin.Digital.Di6,
                (Cpu.Pin)FEZ_Pin.Digital.Di7
            };
            
            for (int i = 0; i < relays.Length; i++)
            {
                relays[i] = new OutputPort(diPins[i], !relayTrue);
            }

            LegoRemote remote = new LegoRemote((Cpu.Pin)FEZ_Pin.Interrupt.Di11);
            remote.OnLegoButtonPress += remote_OnLegoButtonPress;

            InputPort pressureSensor = new InputPort((Cpu.Pin)FEZ_Pin.Digital.Di9, false, Port.ResistorMode.PullUp);

            led.Write(true);
            Thread.Sleep(600);
            led.Write(false);
            Thread.Sleep(600);

            for (int i = 0; i < relays.Length; i++)
            {
                relays[i].Write(relayTrue);
                Thread.Sleep(600);
                relays[i].Write(!relayTrue);
                Thread.Sleep(600);
            }

            while (true)
            {
                var hasPressure = pressureSensor.Read();

                relays[5].Write(hasPressure);

                Thread.Sleep(1000);
            }
        }

        static void remote_OnLegoButtonPress(Message msg)
        {
            if (msg.CommandA == Command.ComboDirectForward)
            {
                relays[4].Write(false);
            }

            if (msg.CommandA == Command.ComboDirectBackward)
            {
                relays[4].Write(true);
            }
        }

        static void pressureSensorTimer_Execute(object state)
        {
            if (pressureData.Count > 10)
            {
                pressureData.Clear();
            }

            var hasPressure = pressureSensor.Read();

            var waterData = new WaterData { Timestamp = RealTimeClock.GetTime(), Pressure = hasPressure ? 1 : 0 };
            pressureData.Add(waterData);

            Debug.Print(waterData.ToString());
        }
    }
}
