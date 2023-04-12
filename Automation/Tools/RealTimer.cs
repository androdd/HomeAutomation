namespace HomeAutomation.Tools
{
    using System;
    using System.Collections;
    using System.Threading;

    public class RealTimer : Base
    {
        public delegate void Callback(TimerState state);

        private readonly Log _log;
        private readonly Hashtable _hashtable;

        private static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);

        public RealTimer(Log log)
        {
            _log = log;
            _hashtable = new Hashtable();
        }

        public bool TryScheduleRunAt(DateTime dueDateTime, Callback timerCallback, string name = "")
        {
            return TryScheduleRunAt(dueDateTime, timerCallback, null, InfiniteTimeSpan, name);
        }

        public bool TryScheduleRunAt(DateTime dueDateTime, Callback timerCallback, TimeSpan period, string name = "")
        {
            return TryScheduleRunAt(dueDateTime, timerCallback, null, period);
        }

        public bool TryScheduleRunAt(DateTime dueDateTime, Callback timerCallback, TimerState timerState, string name = "")
        {
            return TryScheduleRunAt(dueDateTime, timerCallback, timerState, InfiniteTimeSpan, name);
        }

        public bool TryScheduleRunAt(DateTime dueDateTime, Callback timerCallback, TimerState state, TimeSpan period, string name = "")
        {
            var now = DateTime.Now;

            if (now > dueDateTime)
            {
                return false;
            }

            var interval = dueDateTime - now;

            Guid key = Guid.NewGuid();
            TimerState timerState;

            if (state == null)
            {
                timerState = new TimerState { TimerKey = key, Name = name };
            }
            else
            {
                state.TimerKey = key;
                state.Name = name;

                timerState = state;
            }

            var timer = new Timer(s => { timerCallback((TimerState)s); }, timerState, interval, period);
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