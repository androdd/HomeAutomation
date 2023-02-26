namespace ExperimentalBoard
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware;
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Hardware.LegoRemote;
    using AdSoft.Fez.Ui;
    using AdSoft.Fez.Ui.Menu;
    
    using GHIElectronics.NETMF.FEZ;
    using GHIElectronics.NETMF.Hardware;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    public class Program
    {
        private static Lcd2004 _lcd2004;
        private static Led _led;

        public static void Main()
        {
            Debug.EnableGCMessages(false);

            _led = new Led(FEZ_Pin.Digital.LED);
            _led.Init();
            
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

            LegoRemote legoRemote = new LegoRemote(FEZ_Pin.Interrupt.Di11);
            legoRemote.Init();
            LegoSmallRemoteKeyboard keyboard = new LegoSmallRemoteKeyboard(legoRemote);
            keyboard.Init();

            var screenSaver = new ScreenSaver(_lcd2004, keyboard);
            screenSaver.Init(10);

            Clock clock = new Clock("Clock", _lcd2004, keyboard);
            clock.GetTime += () => DateTime.Now;
            clock.SetTime += Utility.SetLocalTime;
            clock.Setup(15, 0);
            clock.Show();

            Menu menu = new Menu("Menu", _lcd2004, keyboard);
            menu.Setup(new[]
            {
                new MenuItem(0, "Set Clock"),
                new MenuItem(2, "Screen Saver Off"), 
                new MenuItem(3, "Screen Saver On"), 
                new MenuItem(1, "Exit")
            });

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
                menu.Hide();
                switch (key)
                {
                    case 0:
                        clock.Edit();
                        break;
                    case 2:
                        screenSaver.Disable();
                        break;
                    case 3:
                        screenSaver.Enable();
                        break;
                    case 1:
                        
                        break;
                }
            };

            _led.BlinkAsync(4, 600);

            Thread.Sleep(Timeout.Infinite);
        }

        
    }
}
