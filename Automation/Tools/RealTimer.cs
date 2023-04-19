namespace HomeAutomation.Tools
{
    using System;
    using System.Collections;
    using System.Threading;

    using Microsoft.SPOT;

    public class RealTimer : Base
    {
        public delegate void SingleCallback(TimerState state);

        public delegate bool Callback(TimerState state);

        private readonly Log _log;
        private readonly Hashtable _disposable;
        private readonly Hashtable _nonDisposable;

        private static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);

        public RealTimer(Log log)
        {
            _log = log;
            _disposable = new Hashtable();
            _nonDisposable = new Hashtable();
        }

        public bool TryScheduleRunAt(DateTime dueDateTime, SingleCallback timerCallback, string name = "", bool isDisposable = true)
        {
            return TryScheduleRunAt(dueDateTime,
                state =>
                {
                    timerCallback(state);
                    return false;
                },
                null,
                InfiniteTimeSpan,
                name,
                isDisposable);
        }

        public bool TryScheduleRunAt(DateTime dueDateTime, Callback timerCallback, TimeSpan period, string name = "", bool isDisposable = true)
        {
            return TryScheduleRunAt(dueDateTime, timerCallback, null, period, name, isDisposable);
        }

        public bool TryScheduleRunAt(DateTime dueDateTime, SingleCallback timerCallback, TimerState timerState, string name = "", bool isDisposable = true)
        {
            return TryScheduleRunAt(dueDateTime,
                state =>
                {
                    timerCallback(state);
                    return false;
                },
                timerState,
                InfiniteTimeSpan,
                name,
                isDisposable);
        }

        public bool TryScheduleRunAt(DateTime dueDateTime, Callback timerCallback, TimerState state, TimeSpan period, string name = "", bool isDisposable = true)
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

            var timer = new Timer(s =>
                {
                    if (!timerCallback((TimerState)s))
                    {
                        var isDisposed = TryDispose(key);
                        Debug.Print(name + " auto disposed: " + isDisposed);
                    }
                },
                timerState,
                interval,
                period);

            if (isDisposable)
            {
                _disposable.Add(key, timer);
            }
            else
            {
                _nonDisposable.Add(key, timer);
            }

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
            var timer = _disposable[key] as Timer;

            if (timer == null)
            {
                return false;
            }

            timer.Dispose();
            _disposable.Remove(key);

            return true;
        }

        public void DisposeAll()
        {
            foreach (var timer in _disposable.Values)
            {
                ((Timer)timer).Dispose();
            }

            _disposable.Clear();
        }
    }
}