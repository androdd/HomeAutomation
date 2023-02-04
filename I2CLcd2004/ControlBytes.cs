namespace I2CLcd2004
{
    public static class ControlBytes
    {
        // commands
        public const byte LCD_CLEARDISPLAY = 0x01;
        public const byte LCD_RETURNHOME = 0x02;
        public const byte LCD_ENTRYMODESET = 0x04;
        public const byte LCD_DISPLAYCONTROL = 0x08;
        public const byte LCD_CURSORSHIFT = 0x10;
        public const byte LCD_FUNCTIONSET = 0x20;
        public const byte LCD_SETCGRAMADDR = 0x40;
        public const byte LCD_SETDDRAMADDR = 0x80;

        // flags for display entry mode
        public const byte LCD_ENTRYRIGHT = 0x00;
        public const byte LCD_ENTRYLEFT = 0x02;
        public const byte LCD_ENTRYSHIFTINCREMENT = 0x01;
        public const byte LCD_ENTRYSHIFTDECREMENT = 0x00;

        // flags for display on/off control
        public const byte LCD_DISPLAYON = 0x04;
        public const byte LCD_DISPLAYOFF = 0x00;
        public const byte LCD_CURSORON = 0x02;
        public const byte LCD_CURSOROFF = 0x00;
        public const byte LCD_BLINKON = 0x01;
        public const byte LCD_BLINKOFF = 0x00;

        // flags for display/cursor shift
        public const byte LCD_DISPLAYMOVE = 0x08;
        public const byte LCD_CURSORMOVE = 0x00;
        public const byte LCD_MOVERIGHT = 0x04;
        public const byte LCD_MOVELEFT = 0x00;

        // flags for function set
        public const byte LCD_8BITMODE = 0x10;
        public const byte LCD_4BITMODE = 0x00;
        public const byte LCD_2LINE = 0x08;
        public const byte LCD_1LINE = 0x00;
        public const byte LCD_5x10DOTS = 0x04;
        public const byte LCD_5x8DOTS = 0x00;

        // flags for backlight control
        public const byte LCD_BACKLIGHT = 0x08;
        public const byte LCD_NOBACKLIGHT = 0x00;

        public const byte En = 4; // 0000 0100 Enable bit 
        public const byte Rw = 2; // 0000 0010 Read/Write bit 
        public const byte Rs = 1; // 0000 0001 Register select bit
    }
}
