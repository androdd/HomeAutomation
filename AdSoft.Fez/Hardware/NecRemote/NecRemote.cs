namespace AdSoft.Fez.Hardware.NecRemote
{
    using System;
    using System.Collections;
    using System.Threading;

    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    public class NecRemote
    {
        private readonly FEZ_Pin.Interrupt _portId;

        private long _lastPulseTime;
        private long _lastValidMessage;
        private int _messageIndex = 32;
        private Message _message = new Message();

        private Queue _mQ;
        private InterruptPort _interruptPort;
        private Thread _messageDispatcherThread;

        public delegate void NecButtonPressEventHandler(Message msg);

        public event NecButtonPressEventHandler NecButtonPressed;

        public NecRemote(FEZ_Pin.Interrupt portId)
        {
            _portId = portId;
        }

        public void Init()
        {
            _interruptPort = new InterruptPort((Cpu.Pin)_portId, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            _interruptPort.OnInterrupt += irm_OnInterrupt;

            _mQ = new Queue();

            _messageDispatcherThread = new Thread(MessageDispatcher);
            _messageDispatcherThread.Start();
        }

        private void MessageDispatcher()
        {
            while (true)
            {
                Thread.Sleep(100);
                if (_mQ.Count == 0)
                    continue;
                
                Message msg = (Message)_mQ.Dequeue();
                if (!msg.IsValid)
                    continue;
                
                if (NecButtonPressed == null)
                    continue;

                var elapsedMs = (msg.Time.Ticks - _lastValidMessage) / 10000; // ms
                if (elapsedMs < 300)
                    continue;

                _lastValidMessage = msg.Time.Ticks;

                NecButtonPressed(msg);
            }
        }

        private void irm_OnInterrupt(uint port, uint state, DateTime time)
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
                _mQ.Enqueue(_message);
            }

            if (_messageIndex < 32 && bit != 10)
            {
                _message[_messageIndex] = bit;

                if (_messageIndex == 31)
                {
                    _message.Time = time;
                    _mQ.Enqueue(_message);
                }

                _messageIndex++;
            }
            _lastPulseTime = time.Ticks;
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
