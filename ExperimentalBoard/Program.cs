namespace ExperimentalBoard
{
    using System;
    using System.Collections;
    using System.IO;
    using System.IO.Ports;
    using System.Reflection;
    using System.Text;
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
    using GHIElectronics.NETMF.IO;
    using GHIElectronics.NETMF.USBClient;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    using Watchdog = GHIElectronics.NETMF.Hardware.LowLevel.Watchdog;

    public class Program
    {
        private static Lcd2004 _lcd2004;
        private static Led _led;

        public static void Main()
        {
            Debug.EnableGCMessages(false);

            SdCard sdCard = new SdCard();
            sdCard.Init();

            var path = SdCard.GetPath("ExperimentalBoardProgram.pe");

            var assembly = Assembly.Load(File.ReadAllBytes(path));

            var mainProgramType = assembly.GetType("ExperimentalBoardProgram.MainProgram");

            var method = mainProgramType.GetMethod("Start", BindingFlags.Public | BindingFlags.Static);

            method.Invoke(null, null);

            Debug.Print("Done");
            
            Thread.Sleep(Timeout.Infinite);
        }

        private static void TestCreateChar()
        {
            _lcd2004 = new Lcd2004(0x27);

            _lcd2004.Init();
            _lcd2004.BackLightOn();

            //_lcd2004.CreateChar(0, new byte[] { 04, 12, 04, 04, 04, 04, 14, 31 });
            //_lcd2004.CreateChar(1, new byte[] { 14, 17, 01, 02, 04, 08, 31, 31 });
            //_lcd2004.CreateChar(2, new byte[] { 31, 02, 04, 02, 01, 17, 14, 31 });
            //_lcd2004.CreateChar(3, new byte[] { 02, 06, 10, 18, 31, 02, 02, 31 });
            //_lcd2004.CreateChar(4, new byte[] { 31, 16, 30, 01, 01, 17, 14, 31 });
            //_lcd2004.CreateChar(5, new byte[] { 06, 08, 16, 30, 17, 17, 14, 31 });
            //_lcd2004.CreateChar(6, new byte[] { 31, 17, 01, 02, 04, 04, 04, 31 });
            //_lcd2004.CreateChar(7, new byte[] { 14, 17, 17, 14, 17, 17, 14, 31 });

            _lcd2004.CreateChar(0, new byte[] { 04, 12, 04, 20, 04, 04, 14, 31 });
            _lcd2004.CreateChar(1, new byte[] { 14, 17, 01, 18, 04, 08, 31, 31 });
            _lcd2004.CreateChar(2, new byte[] { 31, 02, 20, 02, 01, 17, 14, 31 });
            _lcd2004.CreateChar(3, new byte[] { 18, 06, 10, 18, 31, 02, 02, 31 });
            _lcd2004.CreateChar(4, new byte[] { 31, 16, 30, 01, 05, 17, 14, 31 });
            _lcd2004.CreateChar(5, new byte[] { 06, 08, 17, 30, 17, 17, 14, 31 });
            _lcd2004.CreateChar(6, new byte[] { 31, 17, 01, 18, 04, 04, 04, 31 });
            _lcd2004.CreateChar(7, new byte[] { 14, 17, 21, 14, 21, 17, 14, 31 });

            _lcd2004.Home();

            _lcd2004.Write(1, 1, "Hello!!!");

            _lcd2004.SetCursor(1, 2);
            _lcd2004.WriteChar(0);
            _lcd2004.WriteChar(1);
            _lcd2004.WriteChar(2);
            _lcd2004.WriteChar(3);
            _lcd2004.WriteChar(4);
            _lcd2004.WriteChar(5);
            _lcd2004.WriteChar(6);
            _lcd2004.WriteChar(7);
        }

        private static void TestExpander()
        {
            var config = new I2CDevice.Configuration(0x27, 100);
            var busI2C = new I2CDevice(config);

            // 1   => RS
            // 2   => RW
            // 4   => E
            // 8   => None
            // 16  => D4
            // 32  => D5
            // 64  => D6
            // 128 => D7

            var xActions = new I2CDevice.I2CTransaction[1];
            byte[] registerNum = { 0x1 };
            xActions[0] = I2CDevice.CreateWriteTransaction(registerNum);
            var result = busI2C.Execute(xActions, 1000);

            Debug.Print("Result: " + result);
        }

        private static void WatchdogTest()
        {
            Debug.Print("Starting");

            _lcd2004 = new Lcd2004(0x27);

            _lcd2004.Init();
            _lcd2004.BackLightOn();

            var cause = Watchdog.LastResetCause == Watchdog.ResetCause.WatchdogReset
                ? "WatchdogReset"
                : "HardReset";
            _lcd2004.Write(0, 0, cause);
            Debug.Print(cause);

            NecRemote necRemote = new NecRemote(FEZ_Pin.Interrupt.Di11);
            necRemote.Init();

            Key lastKey = Key.NoName;

            try
            {
                MiniRemoteKeyboard keyboard = new MiniRemoteKeyboard(necRemote);
                keyboard.Init();
                keyboard.KeyPressed += key =>
                {
                    lastKey = key;
                    _lcd2004.Write(0, 1, KeyEx.KeyToString(key));
                    Debug.Print(KeyEx.KeyToString(key));

                    throw new ApplicationException();
                };
            }
            catch (Exception e)
            {
                _lcd2004.Write(0, 3, "Ex");
            }

            Debug.Print("Started");

            //try
            //{
            //    while (true)
            //    {
            //        if (lastKey == Key.Enter)
            //        {
            //            throw new ApplicationException();
            //        }
            //        _lcd2004.Write(0, 2, KeyEx.KeyToString(lastKey));

            //        Thread.Sleep(1000);
            //    }
            //}
            //catch (Exception e)
            //{
            //    _lcd2004.Write(0, 3, "Ex");
            //    throw;
            //}
        }

        private static void SerialTest()
        {
            _lcd2004 = new Lcd2004(0x27);

            _lcd2004.Init();
            _lcd2004.BackLightOn();

            var current = Configuration.DebugInterface.GetCurrent();

            _lcd2004.WriteAndReturnCursor(0, 0, current.ToString());

            if (current == Configuration.DebugInterface.Port.USB1)
            {
                _lcd2004.WriteAndReturnCursor(0, 0, "USB");


                SerialPort UART = new SerialPort("COM1", 115200);
                UART.ReadTimeout = 2000;

                int read_count = 0;
                byte[] tx_data;
                byte[] rx_data = new byte[10];
                tx_data = Encoding.UTF8.GetBytes("FEZ");
                UART.Open();
                while (true)
                {
                    UART.Flush();
                    UART.Write(tx_data, 0, tx_data.Length);

                    Thread.Sleep(100);

                    read_count = UART.Read(rx_data, 0, rx_data.Length);

                    if (read_count != 3)
                    {
                        // we sent 3 so we should have 3 back
                        Debug.Print("Wrong size: " + read_count.ToString());
                    }
                    else
                    {
                        // the count is correct so check the values
                        // I am doing this the easy way so the code is more clear
                        if (tx_data[0] == rx_data[0])
                        {
                            if (tx_data[1] == rx_data[1])
                            {
                                if (tx_data[2] == rx_data[2])
                                {
                                    Debug.Print("Perfect data!");
                                }
                            }
                        }
                    }

                    Thread.Sleep(100);
                }
            }
            else
            {
                _lcd2004.WriteAndReturnCursor(0, 0, "StartMassStorage");
                var massStorage = USBClientController.StandardDevices.StartMassStorage();

                PersistentStorage sd;
                try
                {
                    sd = new PersistentStorage("SD");
                }
                catch
                {
                    throw new Exception("SD card not detected");
                }

                massStorage.AttachLun(0, sd, "A&D Soft", "Home Automation");
                massStorage.EnableLun(0);
            }

            while (true)
            {
                Debug.Print(DateTime.Now.ToString());
                _lcd2004.WriteAndReturnCursor(0, 3, DateTime.Now.ToString());
                Thread.Sleep(1500);
            }
        }

        private static void MiniRemoteRead()
        {
            _lcd2004 = new Lcd2004(0x27);

            _lcd2004.Init();
            _lcd2004.BackLightOn();

            TextDrum textDrum = new TextDrum("TD", _lcd2004, null);
            textDrum.Setup(0, 0, 20, 4);

            NecRemote necRemote = new NecRemote(FEZ_Pin.Interrupt.Di11);
            necRemote.Init();

            MiniRemoteKeyboard keyboard = new MiniRemoteKeyboard(necRemote);
            keyboard.Init();
            keyboard.KeyPressed += key => { textDrum.Write(KeyEx.KeyToString(key) + "           "); };
        }

        private static void NecRemoteRead()
        {
            _lcd2004 = new Lcd2004(0x27);

            _lcd2004.Init();
            _lcd2004.BackLightOn();

            TextDrum textDrum = new TextDrum("TD", _lcd2004, null);
            textDrum.Setup(0, 0, 20, 4);

            NecRemote necRemote = new NecRemote(FEZ_Pin.Interrupt.Di11);
            necRemote.Init();
            necRemote.NecButtonPressed += msg => { textDrum.Write(msg.Address + " - " + msg.Command + "     "); };
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

            SettingsFile settingsFile = new SettingsFile(sdCard, "config.txt");

            var necRemote = new NecRemote(FEZ_Pin.Interrupt.Di11);
            necRemote.Init();
            var keyboard = new MiniRemoteKeyboard(necRemote);
            keyboard.Init();

            var doublePicker = new DoublePicker("", _lcd2004, keyboard);
            doublePicker.Setup(4, 0, 2, 0, 0);
            doublePicker.Value = 1;
            doublePicker.KeyPressed += key =>
            {
                if (key == Key.Enter)
                {
                    doublePicker.Show(false);
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
                if (key == Key.Multiply)
                {
                    menu.Show();
                    menu.Focus();
                }
            };

            menu.KeyPressed += key => Debug.Print("KeyPressed:" + KeyEx.KeyToString(key));

            menu.MenuItemEnter += key =>
            {
                menu.Show(false);
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
                        bool exists;
                        if (sdCard.TryIsExists("config.txt", out exists) && exists && 
                            sdCard.TryDelete("config.txt") &&
                            sdCard.TryCopy("config.org", "config.txt"))
                        {
                            Debug.Print("Copied");
                        }

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
                        ArrayList lines;
                        if (sdCard.TryIsExists("config.txt", out exists) && exists && sdCard.TryReadAllLines("config.txt", out lines))
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
            drum.Setup(12, 0, 8, 4);
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
