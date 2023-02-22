namespace ExperimentalBoard
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Hardware.LegoRemote;
    using AdSoft.Fez.Ui;
    using AdSoft.Fez.Ui.Menu;
    
    using GHIElectronics.NETMF.FEZ;
    using GHIElectronics.NETMF.Hardware;

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

            RealTimeClock.GetTime();


            Thread.Sleep(Timeout.Infinite);
        }
    }
}
