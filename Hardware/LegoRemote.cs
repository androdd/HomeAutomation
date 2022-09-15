namespace HomeAutomation.Hardware
{
    using System;
    using System.Collections;
    using System.Threading;

    using GHIElectronics.NETMF.FEZ;

    using HomeAutomation.Models;

    using Microsoft.SPOT.Hardware;

    public class LegoRemote
    {
        private readonly FEZ_Pin.Interrupt _portId;

        private long _lastPulseTime;
        private int _messageIndex = 16;
        private Message _message = new Message();

        private Queue _mQ;
        private InterruptPort _interruptPort;
        private Thread _messageDispatcherThread;

        public delegate void LegoButtonPressEventHandler(Message msg);

        public event LegoButtonPressEventHandler OnLegoButtonPress;

        public LegoRemote(FEZ_Pin.Interrupt portId)
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
            int toggle = -1;

            while (true)
            {
                Thread.Sleep(100);
                if (_mQ.Count == 0)
                    continue;

                Message msg = (Message)_mQ.Dequeue();
                if (!msg.IsValid)
                    continue;

                if (OnLegoButtonPress == null)
                    continue;

                if (msg.Mode == Mode.ComboDirect &&
                    (msg.CommandA == Command.ComboDirectForward ||
                    msg.CommandA == Command.ComboDirectBackward ||
                    msg.CommandB == Command.ComboDirectForward ||
                    msg.CommandB == Command.ComboDirectBackward))
                {
                    OnLegoButtonPress(msg);
                    continue;
                }

                if (_message.Toggle != toggle)
                    OnLegoButtonPress(msg);

                toggle = msg.Toggle;
            }
        }

        private void irm_OnInterrupt(uint port, uint state, DateTime time)
        {
            long uSeconds = (time.Ticks - _lastPulseTime) / 10;
            byte bit = GetBit(uSeconds);

            if (bit == 8)
            {
                _messageIndex = 0;
                _message = new Message();
            }
            if (_messageIndex < 16 && bit != 8)
            {
                _message[_messageIndex] = bit;

                if (_messageIndex == 15)
                    _mQ.Enqueue(_message);

                _messageIndex++;
            }
            _lastPulseTime = time.Ticks;
        }

        private static byte GetBit(long pulseTime)
        {
            if (pulseTime < 316)
                return 11;
            if (pulseTime > 316 && pulseTime < 526)
                return 0;
            if (pulseTime > 526 && pulseTime < 947)
                return 1;
            if (pulseTime > 947 && pulseTime < 1579)
                return 8;
            return 10;
        }
    }
}
