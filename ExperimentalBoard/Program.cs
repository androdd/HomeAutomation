namespace ExperimentalBoard
{
    using System.Threading;

    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    public class Program
    {
        private static I2CDevice.Configuration _config;
        private static I2CDevice _busI2C;
        private static byte _backLight;
        private static byte _lcdFunction;
        private static byte _displayControl;
        private static byte _currentRow;
        private static byte _currentCol;

        #region Control Constants

        // commands
        private const byte LCD_CLEARDISPLAY = 0x01;
        private const byte LCD_RETURNHOME = 0x02;
        private const byte LCD_ENTRYMODESET = 0x04;
        private const byte LCD_DISPLAYCONTROL = 0x08;
        private const byte LCD_CURSORSHIFT = 0x10;
        private const byte LCD_FUNCTIONSET = 0x20;
        private const byte LCD_SETCGRAMADDR = 0x40;
        private const byte LCD_SETDDRAMADDR = 0x80;

        // flags for display entry mode
        private const byte LCD_ENTRYRIGHT = 0x00;
        private const byte LCD_ENTRYLEFT = 0x02;
        private const byte LCD_ENTRYSHIFTINCREMENT = 0x01;
        private const byte LCD_ENTRYSHIFTDECREMENT = 0x00;

        // flags for display on/off control
        private const byte LCD_DISPLAYON = 0x04;
        private const byte LCD_DISPLAYOFF = 0x00;
        private const byte LCD_CURSORON = 0x02;
        private const byte LCD_CURSOROFF = 0x00;
        private const byte LCD_BLINKON = 0x01;
        private const byte LCD_BLINKOFF = 0x00;

        // flags for display/cursor shift
        private const byte LCD_DISPLAYMOVE = 0x08;
        private const byte LCD_CURSORMOVE = 0x00;
        private const byte LCD_MOVERIGHT = 0x04;
        private const byte LCD_MOVELEFT = 0x00;

        // flags for function set
        private const byte LCD_8BITMODE = 0x10;
        private const byte LCD_4BITMODE = 0x00;
        private const byte LCD_2LINE = 0x08;
        private const byte LCD_1LINE = 0x00;
        private const byte LCD_5x10DOTS = 0x04;
        private const byte LCD_5x8DOTS = 0x00;

        // flags for backlight control
        private const byte LCD_BACKLIGHT = 0x08;
        private const byte LCD_NOBACKLIGHT = 0x00;

        private const byte En = 4; // 0000 0100 Enable bit 
        private const byte Rw = 2; // 0000 0010 Read/Write bit 
        private const byte Rs = 1; // 0000 0001 Register select bit

        #endregion

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

            _backLight = LCD_NOBACKLIGHT;
            _lcdFunction = LCD_4BITMODE | LCD_2LINE | LCD_5x8DOTS;
            _displayControl = LCD_DISPLAYOFF | LCD_CURSOROFF | LCD_BLINKOFF;

            _config = new I2CDevice.Configuration(0x27, 100);
            _busI2C = new I2CDevice(_config);

            Thread.Sleep(50);

            Init();
            BackLightOn();

            SetCursor(0, 0);

            CursorOn();

            
        }

        private static void Test()
        {
            var interval = 2 * 1000;

            SetCursor(0, 0);
            Write("DisplayOff");
            DisplayOff();
            Thread.Sleep(interval);

            SetCursor(0, 0);
            Write("DisplayOn");
            DisplayOn();
            Thread.Sleep(interval);

            SetCursor(0, 0);
            Write("BackLightOff");
            BackLightOff();
            Thread.Sleep(interval);

            SetCursor(0, 0);
            Write("BackLightOn");
            BackLightOn();
            Thread.Sleep(interval);

            SetCursor(0, 0);
            Write("Clear");
            Clear();
            Thread.Sleep(interval);

            SetCursor(0, 0);
            Write("Home");
            Home();
            Thread.Sleep(interval);

            SetCursor(0, 0);
            Write("CursorOn");
            CursorOn();
            Thread.Sleep(interval);

            SetCursor(0, 0);
            Write("CursorOff");
            CursorOff();
            Thread.Sleep(interval);

            SetCursor(0, 0);
            Write("BlinkOn");
            BlinkOn();
            Thread.Sleep(interval);

            SetCursor(0, 0);
            Write("BlinkOff");
            BlinkOff();
            Thread.Sleep(interval);
        }

        private static void Init()
        {
            var result = SendI2C(_backLight);
            Debug.Print("Result:" + result);
            Thread.Sleep(1000);

            SendNibble(0x03 << 4);
            Thread.Sleep(5);

            SendNibble(0x03 << 4);
            Thread.Sleep(5);

            SendNibble(0x03 << 4);
            Thread.Sleep(1);

            SendNibble(0x02 << 4);
            Thread.Sleep(1);

            Command(LCD_FUNCTIONSET | _lcdFunction);

            DisplayOn();

            Clear();

            Command(LCD_ENTRYMODESET | LCD_ENTRYLEFT | LCD_ENTRYSHIFTDECREMENT);

            Home();
        }
        
        private static void Write(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                var b = (byte)text[i];
                WriteChar(b);
            }
        }

        private static void WriteChar(char @char)
        {
            WriteChar((byte)@char);
        }

        private static void WriteChar(byte code)
        {
            Send(code, Rs);
            if (_currentCol == 19)
            {
                _currentCol = 0;
                switch (_currentRow)
                {
                    case 0:
                        _currentRow = 2;
                        break;
                    case 1:
                        _currentRow = 3;
                        break;
                    case 2:
                        _currentRow = 1;
                        break;
                    case 3:
                        _currentRow = 0;
                        break;
                }
            }
            else
            {
                _currentCol++;
            }
        }

        private static void DisplayOff()
        {
            DebugWrite(_displayControl);
            unchecked
            {
                _displayControl &= (byte)~LCD_DISPLAYON;
            }
            DebugWrite(_displayControl);
            Command(LCD_DISPLAYCONTROL | _displayControl);
        }

        private static void DisplayOn()
        {
            _displayControl |= LCD_DISPLAYON;
            Command(LCD_DISPLAYCONTROL | _displayControl);
        }

        private static void BackLightOff()
        {
            _backLight = LCD_NOBACKLIGHT;
            SendI2C(0);
        }

        private static void BackLightOn()
        {
            _backLight = LCD_BACKLIGHT;
            SendI2C(0);
        }

        private static void Clear()
        {
            Command(LCD_CLEARDISPLAY);  // clear display, set cursor position to zero
            Thread.Sleep(2);            // this command takes a long time!
        }

        private static void Home()
        {
            Command(LCD_RETURNHOME);    // set cursor position to zero
            Thread.Sleep(2);            // this command takes a long time!
        }

        private static void SetCursor(byte col, byte row)
        {
            int[] rowOffsets = { 0x00, 0x40, 0x14, 0x54 };
            
            if (row > 4)
            {
                row = 3;
            }

            if (col> 20)
            {
                col = 20;
            }

            Command(LCD_SETDDRAMADDR | (col + rowOffsets[row]));

            _currentRow = row;
            _currentCol = col;
        }

        private static void CursorOff()
        {
            DebugWrite(_displayControl);
            unchecked
            {
                _displayControl &= (byte)~LCD_CURSORON;
            }
            DebugWrite(_displayControl);
            Command(LCD_DISPLAYCONTROL | _displayControl);
        }

        private static void CursorOn()
        {
            _displayControl |= LCD_CURSORON;
            Command(LCD_DISPLAYCONTROL | _displayControl);
        }

        private static void BlinkOff()
        {
            DebugWrite(_displayControl);
            unchecked
            {
                _displayControl &= (byte)~LCD_BLINKON;
            }
            DebugWrite(_displayControl);
            Command(LCD_DISPLAYCONTROL | _displayControl);
        }

        private static void BlinkOn()
        {
            _displayControl |= LCD_BLINKON;
            Command(LCD_DISPLAYCONTROL | _displayControl);
        }
        
        private static void ScrollDisplayLeft() {
            Command(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVELEFT);
        }

        private static void ScrollDisplayRight() {
            Command(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVERIGHT);
        }



        private static void Command(int data)
        {
            Send(data, 0);
        }

        private static void Command(byte data)
        {
            Send(data, (byte)0);
        }

        private static void Send(int data, int mode)
        {
            Send((byte)data, (byte)mode);
        }

        private static void Send(byte data, byte mode)
        {
            byte highNibble = (byte)(data & 0xf0);
            byte lowNibble = (byte)((data << 4) & 0xf0);
            SendNibble(highNibble | mode);
            SendNibble(lowNibble | mode);
        }

        private static int SendI2C(int data)
        {
            return SendI2C((byte)data);
        }

        private static int SendI2C(byte data)
        {
            var xActions = new I2CDevice.I2CTransaction[1];
            byte[] registerNum = { (byte)(data | _backLight) };
            xActions[0] = I2CDevice.CreateWriteTransaction(registerNum);
            var result = _busI2C.Execute(xActions, 1000);
            return result;
        }

        private static void SendNibble(int data)
        {
            SendNibble((byte)data);
        }

        private static void SendNibble(byte data)
        {
            SendI2C(data);

            #region Pulse Enable

            SendI2C(data | En);    // En high
            Thread.Sleep(1);        // enable pulse must be >450ns

            SendI2C(data & ~En);   // En low
            Thread.Sleep(1);        // commands need > 37us to settle

            #endregion
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
