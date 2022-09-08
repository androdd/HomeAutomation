namespace HomeAutomation
{
    using System;
    using System.Collections;
    using System.Threading;

    using GHIElectronics.NETMF.Hardware;

    internal class TimerEx : IDisposable
    {
        private readonly Log _log;
        private readonly Hashtable _hashtable;

        private static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);

        public TimerEx(Log log)
        {
            _log = log;
            _hashtable = new Hashtable();
        }

        public bool TryScheduleRunAt(DateTime dueDateTime, TimerCallback timerCallback)
        {
            return TryScheduleRunAt(dueDateTime, timerCallback, InfiniteTimeSpan);
        }

        public bool TryScheduleRunAt(DateTime dueDateTime, TimerCallback timerCallback, TimeSpan period)
        {
            var now = RealTimeClock.GetTime();

            if (now > dueDateTime)
            {
                return false;
            }

            var interval = dueDateTime - now;

            Guid key = Guid.NewGuid();
            var timer = new Timer(timerCallback, key, interval, period);
            _hashtable.Add(key, timer);

            if (period == InfiniteTimeSpan)
            {
                _log.Write("Timer set for: " + dueDateTime.ToString("G"));
            }
            else
            {
                _log.Write("Timer set for: " + dueDateTime.ToString("G") + " with period: " + period);
            }

            return true;
        }

        public bool TryDispose(Guid key)
        {
            var timer = _hashtable[key] as Timer;

            if (timer == null)
            {
                return false;
            }

            timer.Dispose();
            _hashtable.Remove(key);

            return true;
        }

        public void Dispose()
        {
            foreach (var timer in _hashtable.Values)
            {
                ((Timer)timer).Dispose();
            }
        }
    }
}