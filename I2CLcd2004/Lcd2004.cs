namespace I2CLcd2004
{
    using System.Runtime.CompilerServices;
    using System.Threading;

    using Microsoft.SPOT.Hardware;

    using CB = ControlBytes;

    public class Lcd2004
    {
        private I2CDevice.Configuration _config;
        private I2CDevice _busI2C;
        private byte _backLight;
        private readonly byte _lcdFunction;
        private byte _displayControl;
        private byte _currentRow;
        private byte _currentCol;
        private readonly ushort _address;

        public Lcd2004(ushort address)
        {
            _backLight = CB.LCD_NOBACKLIGHT;
            _lcdFunction = CB.LCD_4BITMODE | CB.LCD_2LINE | CB.LCD_5x8DOTS;
            _displayControl = CB.LCD_DISPLAYOFF | CB.LCD_CURSOROFF | CB.LCD_BLINKOFF;

            _address = address;
        }
        
        public void Init()
        {
            Thread.Sleep(50);

            _config = new I2CDevice.Configuration(_address, 100);
            _busI2C = new I2CDevice(_config);

            SendI2C(_backLight);
            Thread.Sleep(1000);

            SendNibble(0x03 << 4);
            Thread.Sleep(5);

            SendNibble(0x03 << 4);
            Thread.Sleep(5);

            SendNibble(0x03 << 4);
            Thread.Sleep(1);

            SendNibble(0x02 << 4);
            Thread.Sleep(1);

            Command(CB.LCD_FUNCTIONSET | _lcdFunction);

            DisplayOn();

            Clear();

            Command(CB.LCD_ENTRYMODESET | CB.LCD_ENTRYLEFT | CB.LCD_ENTRYSHIFTDECREMENT);

            Home();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void WriteAndReturnCursor(byte col, byte row, string text)
        {
            byte oldCol = _currentCol;
            byte oldRow = _currentRow;

            SetCursor(col, row);
            Write(text);
            SetCursor(oldCol, oldRow);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void WriteLine(byte row, string text)
        {
            SetCursor(0, row);
            if (text.Length > 20)
            {
                text = text.Substring(0, 20);
            }

            Write(text);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Write(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                WriteChar(text[i]);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Write(byte col, byte row, string text)
        {
            SetCursor(col, row);
            for (int i = 0; i < text.Length; i++)
            {
                WriteChar(text[i]);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void WriteChar(char @char)
        {
            WriteChar((byte)@char);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void WriteChar(byte code)
        {
            Send(code, CB.Rs);

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

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetCursor(byte col, byte row)
        {
            int[] rowOffsets = { 0x00, 0x40, 0x14, 0x54 };

            if (row > 4)
            {
                row = 3;
            }

            if (col > 20)
            {
                col = 20;
            }

            Command(CB.LCD_SETDDRAMADDR | (col + rowOffsets[row]));

            _currentRow = row;
            _currentCol = col;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DisplayOff()
        {
            unchecked
            {
                _displayControl &= (byte)~CB.LCD_DISPLAYON;
            }
            Command(CB.LCD_DISPLAYCONTROL | _displayControl);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DisplayOn()
        {
            _displayControl |= CB.LCD_DISPLAYON;
            Command(CB.LCD_DISPLAYCONTROL | _displayControl);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void BackLightOff()
        {
            _backLight = CB.LCD_NOBACKLIGHT;
            SendI2C(0);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void BackLightOn()
        {
            _backLight = CB.LCD_BACKLIGHT;
            SendI2C(0);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Clear()
        {
            Command(CB.LCD_CLEARDISPLAY);  // clear display, set cursor position to zero
            Thread.Sleep(2);            // this command takes a long time!
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Home()
        {
            Command(CB.LCD_RETURNHOME);    // set cursor position to zero
            Thread.Sleep(2);            // this command takes a long time!
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CursorOff()
        {
            unchecked
            {
                _displayControl &= (byte)~CB.LCD_CURSORON;
            }
            Command(CB.LCD_DISPLAYCONTROL | _displayControl);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CursorOn()
        {
            _displayControl |= CB.LCD_CURSORON;
            Command(CB.LCD_DISPLAYCONTROL | _displayControl);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void BlinkOff()
        {
            unchecked
            {
                _displayControl &= (byte)~CB.LCD_BLINKON;
            }
            Command(CB.LCD_DISPLAYCONTROL | _displayControl);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void BlinkOn()
        {
            _displayControl |= CB.LCD_BLINKON;
            Command(CB.LCD_DISPLAYCONTROL | _displayControl);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ScrollDisplayLeft()
        {
            Command(CB.LCD_CURSORSHIFT | CB.LCD_DISPLAYMOVE | CB.LCD_MOVELEFT);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ScrollDisplayRight()
        {
            Command(CB.LCD_CURSORSHIFT | CB.LCD_DISPLAYMOVE | CB.LCD_MOVERIGHT);
        }



        private void Command(int data)
        {
            Send(data, 0);
        }

        private void Command(byte data)
        {
            Send(data, (byte)0);
        }

        private void Send(int data, int mode)
        {
            Send((byte)data, (byte)mode);
        }

        private void Send(byte data, byte mode)
        {
            byte highNibble = (byte)(data & 0xf0);
            byte lowNibble = (byte)((data << 4) & 0xf0);
            SendNibble(highNibble | mode);
            SendNibble(lowNibble | mode);
        }

        private int SendI2C(int data)
        {
            return SendI2C((byte)data);
        }

        private int SendI2C(byte data)
        {
            var xActions = new I2CDevice.I2CTransaction[1];
            byte[] registerNum = { (byte)(data | _backLight) };
            xActions[0] = I2CDevice.CreateWriteTransaction(registerNum);
            var result = _busI2C.Execute(xActions, 1000);
            return result;
        }

        private void SendNibble(int data)
        {
            SendNibble((byte)data);
        }

        private void SendNibble(byte data)
        {
            SendI2C(data);

            #region Pulse Enable

            SendI2C(data | CB.En);    // En high
            Thread.Sleep(1);        // enable pulse must be >450ns

            SendI2C(data & ~CB.En);   // En low
            Thread.Sleep(1);        // commands need > 37us to settle

            #endregion
        }
    }
}
