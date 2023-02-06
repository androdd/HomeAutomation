namespace HomeAutomation.Models
{
    using System;

    public enum Escape
    {
        Mode = 0,
        ComboPwm = 1
    }

    public enum Address
    {
        Default = 0,
        Extra = 1
    }

    public enum Mode
    {
        Extended = 0,
        ComboDirect = 1,
        SinglePinContinuous = 2,
        SinglePinTimeout = 3,
        SingleOutput = 4
    }

    public enum Command
    {
        Na = -1,

        ComboDirectFloat = 0,
        ComboDirectForward = 1,
        ComboDirectBackward = 2,
        ComboDirectBrake = 3,

        SingleOutputFloat = 0,
        SingleOutputPwmForwardStep1 = 1,
        SingleOutputPwmForwardStep2 = 2,
        SingleOutputPwmForwardStep3 = 3,
        SingleOutputPwmForwardStep4 = 4,
        SingleOutputPwmForwardStep5 = 5,
        SingleOutputPwmForwardStep6 = 6,
        SingleOutputPwmForwardStep7 = 7,
        SingleOutputBrake = 8,
        SingleOutputPwmBackwardStep7 = 9,
        SingleOutputPwmBackwardStep6 = 10,
        SingleOutputPwmBackwardStep5 = 11,
        SingleOutputPwmBackwardStep4 = 12,
        SingleOutputPwmBackwardStep3 = 13,
        SingleOutputPwmBackwardStep2 = 14,
        SingleOutputPwmBackwardStep1 = 15,

        SingleOutputClear = 0,
        SingleOutputSetClear = 1,
        SingleOutputClearSet = 2,
        SingleOutputSet = 3,
        SingleOutputIncrementPwm = 4,
        SingleOutputDecrementPwm = 5,
        SingleOutputFullForward = 6,
        SingleOutputFullBackward = 7,
        SingleOutputToggleFull = 8
    }

    public enum SingleOutputMode
    {
        Na = -1,
        Pwm = 0,
        Cst = 1
    }

    public class Message
    {
        private readonly byte[] bits;

        public byte this[int index]
        {
            get
            {
                return bits[index];
            }
            set
            {
                bits[index] = value;
            }
        }

        public int Toggle
        {
            get
            {
                return GetBitsInt(0, 1);
            }
        }

        public Escape Escape
        {
            get
            {
                return (Escape)GetBitsInt(1, 1);
            }
        }

        public int Channel
        {
            get
            {
                return GetBitsInt(2, 2) + 1;
            }
        }

        public Address Address
        {
            get
            {
                return (Address)GetBitsInt(4, 1);
            }
        }

        public Mode Mode
        {
            get
            {
                int mode = GetBitsInt(5, 3);

                return mode < 4 ? (Mode)mode : Mode.SingleOutput;
            }
        }

        public SingleOutputMode SingleOutputMode
        {
            get
            {
                if (Mode != Mode.SingleOutput)
                    return SingleOutputMode.Na;

                return (SingleOutputMode) GetBitsInt(6, 1);
            }
        }

        public Command CommandA
        {
            get
            {
                return GetCommand(true);
            }
        }

        public Command CommandB
        {
            get
            {
                return GetCommand(false);
            }
        }

        public bool IsValid
        {
            get
            {
                return Check();
            }
        }

        public DateTime Time { get; set; }

        public Message()
        {
            bits = new byte[16];
        }

        private Command GetCommand(bool isA)
        {
            int command;
            switch (Mode)
            {
                case Mode.Extended:
                    throw new NotImplementedException();
                case Mode.ComboDirect:
                    command = isA ? GetBitsInt(10, 2) : GetBitsInt(8, 2);
                    return (Command)command;
                case Mode.SinglePinContinuous:
                    throw new NotImplementedException();
                case Mode.SinglePinTimeout:
                    throw new NotImplementedException();
                case Mode.SingleOutput:
                    int output = GetBitsInt(7, 1);

                    if (isA && output == 1)
                        return Command.Na;
                    if (!isA && output == 0)
                        return Command.Na;

                    command = GetBitsInt(8, 4);
                    return (Command)command;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool Check()
        {
            int nibble1Int = GetBitsInt(0, 4);
            int nibble2Int = GetBitsInt(4, 4);
            int nibble3Int = GetBitsInt(8, 4);
            int nibble4Int = GetBitsInt(12, 4);

            int check = 15 ^ nibble1Int ^ nibble2Int ^ nibble3Int;

            return check == nibble4Int;
        }

/*
        private static int BinToDec(string bin)
        {
            int result = 0;
            int power = 1;
            for (int i = 0; i < bin.Length; i++)
            {
                if (bin[bin.Length - i - 1] != '0')
                    result += power;

                power *= 2;
            }
            return result;
        }
*/
        private int GetBitsInt(int index, int length)
        {
            int result = 0;
            int power = 1;
            for (int i = index; i < index + length; i++)
            {
                if (bits[(index + length) - (i - index) - 1] != 0)
                    result += power;

                power *= 2;
            }
            return result;
        }

        public override string ToString()
        {
            string messageString = string.Empty;
            for (int i = 0; i < 16; i++)
                messageString += bits[i] + " ";

            return messageString;
        }
    }
}
