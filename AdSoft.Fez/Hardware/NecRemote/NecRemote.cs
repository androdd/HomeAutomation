namespace AdSoft.Fez.Hardware.NecRemote
{
    using System;

    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT.Hardware;

    public class NecRemote
    {
        private readonly FEZ_Pin.Interrupt _portId;

        private long _lastPulseTime;
        private long _lastValidMessage;
        private int _messageIndex = 32;
        private Message _message = new Message();

        private InterruptPort _interruptPort;

        public delegate void NecButtonPressEventHandler(Message msg);

        public event NecButtonPressEventHandler NecButtonPressed;

        public NecRemote(FEZ_Pin.Interrupt portId)
        {
            _portId = portId;
        }

        public void Init()
        {
            _interruptPort = new InterruptPort((Cpu.Pin)_portId, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            _interruptPort.OnInterrupt += InterruptPortOnInterrupt;
        }

        private void InterruptPortOnInterrupt(uint port, uint state, DateTime time)
        {
            try
            {
                long uSeconds = (time.Ticks - _lastPulseTime) / 10;
                byte bit = GetBit(uSeconds);

                if (bit == 10)
                {
                    _messageIndex = 0;
                    _message = new Message();
                }

                if (bit == 40 && _message.IsValid)
                {
                    _message.Time = time;
                    OnMessage(_message);
                }

                if (_messageIndex < 32 && bit != 10)
                {
                    _message[_messageIndex] = bit;

                    if (_messageIndex == 31)
                    {
                        _message.Time = time;
                        OnMessage(_message);
                    }

                    _messageIndex++;
                }
                _lastPulseTime = time.Ticks;
            }
            catch (Exception ex)
            {
                DebugEx.Print("NecRemote.InterruptPortOnInterrupt Exception", ex);
            }
        }

        private void OnMessage(Message message)
        {
            if (!message.IsValid)
                return;

            if (NecButtonPressed == null)
                return;

            var elapsedMs = (message.Time.Ticks - _lastValidMessage) / 10000; // ms
            if (elapsedMs < 300)
                return;

            _lastValidMessage = message.Time.Ticks;

            NecButtonPressed(message);
        }

        private static byte GetBit(long pulseTime)
        {
            if (pulseTime > 1000 && pulseTime < 2000)
                return 0;
            if (pulseTime > 2000 && pulseTime < 2500)
                return 1;

            if (pulseTime > 2500 && pulseTime < 3000)
                return 30; // End2
            if (pulseTime > 100000 && pulseTime < 110000)
                return 40; // Repeat

            if (pulseTime > 4900 && pulseTime < 5200)
                return 10; // Start
            if (pulseTime > 45000 && pulseTime < 50000)
                return 20; // End

            return 100; // Wrong
        }
    }
}
