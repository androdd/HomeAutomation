namespace ExperimentalBoard
{
    using System;
    using System.Collections;
    using System.Threading;

    using AdSoft.Fez;
    using AdSoft.Fez.Configuration;
    using AdSoft.Fez.Hardware;
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Hardware.LegoRemote;
    using AdSoft.Fez.Hardware.NecRemote;
    using AdSoft.Fez.Hardware.SdCard;
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

            _lcd2004 = new Lcd2004(0x27);

            _lcd2004.Init();
            _lcd2004.BackLightOn();

            TextDrum textDrum = new TextDrum("TD", _lcd2004, null);
            textDrum.Setup(0, 0, 20, 4);

            NecRemote necRemote = new NecRemote(FEZ_Pin.Interrupt.Di11);
            necRemote.Init();
            necRemote.NecButtonPressed += msg =>
            {
                textDrum.Write(msg.Address + " - " + msg.Command);
            };

            Thread.Sleep(Timeout.Infinite);
        }

        private static void PinCapture()
        {
            PinCapture cap = new PinCapture((Cpu.Pin)FEZ_Pin.Digital.Di11, Port.ResistorMode.Disabled);
            uint[] buffer = new uint[300];

            int edges = cap.Read(false, buffer, 0, buffer.Length, 3000);
            if (edges == 0)
            {
                Debug.Print("Nothing was captured");
            }
            else
            {
                Debug.Print("We have " + edges + " edges");
                for (int i = 0; i < edges; i++)
                {
                    Debug.Print("Edge #" + i + "= " + buffer[i]);
                }
            }
        }

        private static void SettingsTest()
        {
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

            SdCard sdCard = new SdCard();

            SettingsFile settingsFile = new SettingsFile(sdCard, "test.txt");

            LegoRemote legoRemote = new LegoRemote(FEZ_Pin.Interrupt.Di11);
            legoRemote.Init();
            LegoSmallRemoteKeyboard keyboard = new LegoSmallRemoteKeyboard(legoRemote);
            keyboard.Init();

            var doublePicker = new DoublePicker("", _lcd2004, keyboard);
            doublePicker.Setup(4, 0, 2, 0, 0);
            doublePicker.Value = 1;
            doublePicker.KeyPressed += key =>
            {
                if (key == Key.Enter)
                {
                    doublePicker.Hide();
                }
            };

            Menu menu = new Menu("Menu", _lcd2004, keyboard);
            menu.Setup(new[]
            {
                new MenuItem(10, "Reload"),
                new MenuItem(20, "Delete"),
                new MenuItem(30, "Save"),
                new MenuItem(40, "Print"),
                new MenuItem(45, "Unmount"),
                new MenuItem(50, "Set Dbl")
            });

            keyboard.KeyPressed += key =>
            {
                if (key == Key.F8)
                {
                    menu.Show();
                    menu.Focus();
                }
            };

            menu.KeyPressed += key => Debug.Print("KeyPressed:" + DebugEx.KeyToString(key));

            menu.MenuItemEnter += key =>
            {
                menu.Hide();
                switch (key)
                {
                    case 10:
                        if (settingsFile.TryLoadSettings())
                        {
                            byte b = settingsFile.GetByteValue("Pre", 100); // 5
                            int i = settingsFile.GetInt32Value("Set1", -1); // -10
                            double d = settingsFile.GetDoubleValue("Sun2", -0.1); // 10
                            ushort u = settingsFile.GetUshortValue("u", 123);

                            _lcd2004.WriteLine(0, "b:" + b);
                            _lcd2004.WriteLine(1, "i:" + i);
                            _lcd2004.WriteLine(2, "d:" + d);

                            settingsFile.AddOrUpdateValue("Sun2", 0.123.ToString());
                            settingsFile.AddOrUpdateValue("Sun3", 123.ToString());

                            d = settingsFile.GetDoubleValue("Sun2", -0.1);
                            _lcd2004.WriteLine(3, "d:" + d);

                            if (settingsFile.TrySaveSettings())
                            {
                                Debug.Print("Saved");
                            }
                        }

                        break;
                    case 20:
                        sdCard.TryDelete("test.txt");
                        break;
                    case 30:
                        settingsFile.AddOrUpdateValue("Pre", "5");
                        settingsFile.AddOrUpdateValue("Set1", "-10");
                        settingsFile.AddOrUpdateValue("Sun2", "10");

                        if (settingsFile.TrySaveSettings())
                        {
                            Debug.Print("Saved New");
                        }

                        break;
                    case 40:
                        bool exists;
                        ArrayList lines;
                        if (sdCard.TryIsExists("test.txt", out exists) && exists && sdCard.TryReadAllLines("test.txt", out lines))
                        {
                            foreach (var line in lines)
                            {
                                Debug.Print((string)line);
                            }
                        }

                        break;
                    case 45:
                        sdCard.Unmount();
                        break;
                    case 50:
                        doublePicker.Show();
                        doublePicker.Focus();
                        break;
                }
            };

            var drum = new TextDrum("TD", _lcd2004, keyboard);
            drum.Setup(10, 0, 10, 4);
            drum.Show();

            var thread = new Thread(() =>
            {
                int num = 0;
                while (true)
                {
                    num++;
                    if (num == 10)
                    {
                        num = 1;
                    }

                    var value = num * doublePicker.Value;
                    drum.Write(value.ToString("N5"));

                    Thread.Sleep(1000);
                }
            });
            thread.Start();


            _led.BlinkAsync(4, 600);
        }
    }
}
