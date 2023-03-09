namespace AdSoft.Fez.Hardware.NecRemote
{
    using System;

    public class Message
    {
        private readonly byte[] _bits;
        private bool _isChecked;
        private bool _isValid;

        public byte this[int index]
        {
            get
            {
                return _bits[index];
            }
            set
            {
                _bits[index] = value;
            }
        }

        public byte Address
        {
            get
            {
                return (byte)GetBitsInt(0, 8);
            }
        }

        public byte Command
        {
            get
            {
                return (byte)GetBitsInt(16, 8);
            }
        }

        public bool IsValid
        {
            get
            {
                return _isChecked
                    ? _isValid
                    : _isValid = Check();
            }
        }

        public DateTime Time { get; set; }

        public Message()
        {
            _isChecked = false;
            _isValid = false;
            _bits = new byte[32];
        }

        private bool Check()
        {
            _isChecked = true;
            int byte1Int = GetBitsInt(0, 8);
            int byte2Int = GetBitsInt(8, 8);
            int byte3Int = GetBitsInt(16, 8);
            int byte4Int = GetBitsInt(24, 8);

            return (byte)byte1Int == (byte)~byte2Int && (byte)byte3Int == (byte)~byte4Int;
        }

        private int GetBitsInt(int index, int length)
        {
            int result = 0;
            int power = 1;
            for (int i = index; i < index + length; i++)
            {
                if (_bits[(index + length) - (i - index) - 1] != 0)
                    result += power;

                power *= 2;
            }
            return result;
        }

        public override string ToString()
        {
            string messageString = string.Empty;
            for (int i = 0; i < 16; i++)
                messageString += _bits[i] + " ";

            return messageString;
        }
    }
}