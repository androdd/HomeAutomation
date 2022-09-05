namespace HomeAutomation
{
    using System;
    using System.Collections;
    using System.Threading;

    using GHIElectronics.NETMF.Hardware;

    internal class TimerEx : IDisposable
    {
        private readonly Log _log;
        private readonly ArrayList _timers;

        private static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);

        public TimerEx(Log log)
        {
            _log = log;
            _timers = new ArrayList();
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

            var timer = new Timer(timerCallback, null, interval, period);
            _timers.Add(timer);

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

        public void Dispose()
        {
            foreach (var timer in _timers)
            {
                ((Timer)timer).Dispose();
            }
        }
    }
}