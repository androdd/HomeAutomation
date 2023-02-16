namespace AdSoft.Hardware
{
    using System.Runtime.CompilerServices;
    using System.Threading;

    using Microsoft.SPOT.Hardware;

    using CB = AdSoft.Hardware.ControlBytes;

    public class Lcd2004
    {
        private I2CDevice.Configuration _config;
        private I2CDevice _busI2C;

        private byte _backLight;
        private readonly byte _lcdFunction;
        private byte _displayControl;

        private int _currentRow;
        private int _currentCol;
        private bool _isCursorVisible;
        private readonly ushort _address;

        public int Rows
        {
            get { return 4; }
        }

        public int Cols
        {
            get { return 20; }
        }

        public Lcd2004(ushort address)
        {
            _backLight = CB.LCD_NOBACKLIGHT;
            _lcdFunction = CB.LCD_4BITMODE | CB.LCD_2LINE | CB.LCD_5x8DOTS;
            _displayControl = CB.LCD_DISPLAYOFF | CB.LCD_CURSOROFF | CB.LCD_BLINKOFF;

            _isCursorVisible = false;

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
        public void WriteAndReturnCursor(int col, int row, string text)
        {
            int oldCol = _currentCol;
            int oldRow = _currentRow;
            bool isCursorVisible = _isCursorVisible;

            if (isCursorVisible)
            {
                CursorOff();
            }

            SetCursor(col, row);
            Write(text);
            SetCursor(oldCol, oldRow);

            if (isCursorVisible)
            {
                CursorOn();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void WriteLine(int row, string text)
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
        public void Write(int col, int row, string text)
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
        public void WriteCharAtCursor(byte code)
        {
            int col = _currentCol;
            int row = _currentRow;

            WriteChar(code);
            SetCursor(col, row);
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
        public void SetCursor(int col, int row, bool showCursor)
        {
            SetCursor(col, row);

            if (_isCursorVisible && !showCursor)
            {
                CursorOff();
            }
            else if (!_isCursorVisible && showCursor)
            {
                CursorOn();
            }
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetCursor(int col, int row)
        {
            int[] rowOffsets = { 0x00, 0x40, 0x14, 0x54 };

            if (row < 0)
            {
                row = 0;
            }

            if (row >= Rows)
            {
                row = Rows - 1;
            }

            if (col < 0)
            {
                col = 0;
            }

            if (col >= Cols)
            {
                col = Cols - 1;
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
        public void Clear(int fromCol, int fromRow, int toCol, int toRow)
        {
            int oldCol = _currentCol;
            int oldRow = _currentRow;
            bool isCursorVisible = _isCursorVisible;

            if (isCursorVisible)
            {
                CursorOff();
            }

            if (fromCol > toCol || fromRow > toRow || toCol > Cols - 1 || toRow > Rows - 1 || fromRow < 0 || fromCol < 0)
            {
                return;
            }

            for (int r = fromRow; r <= toRow - fromRow; r++)
            {
                SetCursor(fromCol, r);
                for (int c = fromCol; c <= toCol - fromCol; c++)
                {
                    WriteChar(' ');
                }   
            }

            SetCursor(oldCol, oldRow);

            if (isCursorVisible)
            {
                CursorOn();
            }
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
            _isCursorVisible = false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CursorOn()
        {
            _displayControl |= CB.LCD_CURSORON;
            Command(CB.LCD_DISPLAYCONTROL | _displayControl);
            _isCursorVisible = true;
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
