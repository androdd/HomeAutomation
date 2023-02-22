namespace ExperimentalBoard
{
    using System;
    using System.Threading;

    using HomeAutomation.Hardware;
    using HomeAutomation.Hardware.UI;

    using GHIElectronics.NETMF.FEZ;

    using HomeAutomation.Hardware.LegoRemote;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    public class Program
    {
        private static Lcd2004 _lcd2004;

        public static void Main()
        {
            using (var led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, false))
            {
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(300);
                    led.Write(true);
                    Thread.Sleep(400);
                    led.Write(false);
                }
            }

            //using (var pin = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di13, false))
            //{ //Period 86us with no sleep
            //    var millisecondsTimeout = 1;

            //    while (true)
            //    {
            //        pin.Write(true);
            //        //Thread.Sleep(millisecondsTimeout);
            //        pin.Write(false);
            //        //Thread.Sleep(millisecondsTimeout);
            //    }
            //}


            _lcd2004 = new Lcd2004(0x27);

            _lcd2004.Init();
            _lcd2004.BackLightOn();

            LegoRemote legoRemote = new LegoRemote(FEZ_Pin.Interrupt.Di0);
            legoRemote.Init();
            LegoSmallRemoteKeyboard keyboard = new LegoSmallRemoteKeyboard(legoRemote);
            keyboard.Init();

            Clock clock = new Clock("Clock", _lcd2004, keyboard);
            clock.GetTime += () => DateTime.Now;
            clock.SetTime += Utility.SetLocalTime;
            clock.Setup(15, 0);
            clock.Show();

            Menu menu = new Menu("Menu", _lcd2004, keyboard);
            menu.Setup(new[] { new MenuItem(0, "Set Clock"), new MenuItem(1, "Exit") });

            keyboard.KeyPressed += key =>
            {
                if (key == Key.F8)
                {
                    menu.Show();
                    menu.Focus();
                }
            };

            menu.MenuItemEnter += key =>
            {
                switch (key)
                {
                    case 0:
                        menu.Hide();
                        clock.Edit();
                        break;
                    case 1:
                        menu.Hide();
                        break;
                }
            };


            Thread.Sleep(Timeout.Infinite);
        }

        private static void DebugWrite(byte data)
        {
            DebugWrite("", data);
        }

        private static void DebugWrite(string text, byte data)
        {
            text += (data / (1 << 7) % 2).ToString();
            text += (data / (1 << 6) % 2).ToString();
            text += (data / (1 << 5) % 2).ToString();
            text += (data / (1 << 4) % 2).ToString();
            text += (data / (1 << 3) % 2).ToString();
            text += (data / (1 << 2) % 2).ToString();
            text += (data / (1 << 1) % 2).ToString();
            text += (data % 2).ToString();
            Debug.Print(text);
        }
    }
}
