namespace ExperimentalBoard
{
    using System.Threading;

    using GHIElectronics.NETMF.FEZ;

    using I2CLcd2004;

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

            _lcd2004.SetCursor(0, 0);

            _lcd2004.WriteLine(1, "Very long text with a lot a bla bla bla");
            _lcd2004.WriteLine(2, "Another extremely long bla bla bla text");
        }

        private static void Test()
        {
            var interval = 2 * 1000;

            _lcd2004.SetCursor(0, 0);
            _lcd2004.Write("DisplayOff");
            _lcd2004.DisplayOff();
            Thread.Sleep(interval);

            _lcd2004.SetCursor(0, 0);
            _lcd2004.Write("DisplayOn");
            _lcd2004.DisplayOn();
            Thread.Sleep(interval);

            _lcd2004.SetCursor(0, 0);
            _lcd2004.Write("BackLightOff");
            _lcd2004.BackLightOff();
            Thread.Sleep(interval);

            _lcd2004.SetCursor(0, 0);
            _lcd2004.Write("BackLightOn");
            _lcd2004.BackLightOn();
            Thread.Sleep(interval);

            _lcd2004.SetCursor(0, 0);
            _lcd2004.Write("Clear");
            _lcd2004.Clear();
            Thread.Sleep(interval);

            _lcd2004.SetCursor(0, 0);
            _lcd2004.Write("Home");
            _lcd2004.Home();
            Thread.Sleep(interval);

            _lcd2004.SetCursor(0, 0);
            _lcd2004.Write("CursorOn");
            _lcd2004.CursorOn();
            Thread.Sleep(interval);

            _lcd2004.SetCursor(0, 0);
            _lcd2004.Write("CursorOff");
            _lcd2004.CursorOff();
            Thread.Sleep(interval);

            _lcd2004.SetCursor(0, 0);
            _lcd2004.Write("BlinkOn");
            _lcd2004.BlinkOn();
            Thread.Sleep(interval);

            _lcd2004.SetCursor(0, 0);
            _lcd2004.Write("BlinkOff");
            _lcd2004.BlinkOff();
            Thread.Sleep(interval);
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
