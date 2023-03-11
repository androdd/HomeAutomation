namespace HomeAutomation.Tools
{
    using System;
    using System.Collections;
    using System.Threading;

    using GHIElectronics.NETMF.Hardware;

    public class RealTimer : Base
    {
        private readonly Log _log;
        private readonly Hashtable _hashtable;

        private static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);

        public RealTimer(Log log)
        {
            _log = log;
            _hashtable = new Hashtable();
        }

        public bool TryScheduleRunAt(DateTime dueDateTime, TimerCallback timerCallback, string name = "")
        {
            return TryScheduleRunAt(dueDateTime, timerCallback, InfiniteTimeSpan, name);
        }

        public bool TryScheduleRunAt(DateTime dueDateTime, TimerCallback timerCallback, TimeSpan period, string name = "")
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
                _log.Write(name + "Timer set for: " + Format(dueDateTime));
            }
            else
            {
                _log.Write(name + "Timer set for: " + Format(dueDateTime) + " with period: " + period);
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

        public void DisposeAll()
        {
            foreach (var timer in _hashtable.Values)
            {
                ((Timer)timer).Dispose();
            }

            _hashtable.Clear();
        }
    }
}