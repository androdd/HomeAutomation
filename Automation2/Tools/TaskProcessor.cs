namespace HomeAutomation2.Tools
{
    using System;
    using System.Collections;
    using System.Threading;

    using GHIElectronics.NETMF.Hardware.LowLevel;

    using Microsoft.SPOT;

    public class TaskProcessor
    {
        private const int TicksPerMillisecond = 10000;

        private readonly int _period;

        private readonly object _eventQueueLock = new object();
        private readonly object _timerTasksLock = new object();

        private readonly AutoResetEvent _autoResetEvent;
        private readonly Queue _eventQueue;
        private readonly ArrayList _timerTasks;

        public delegate void EventHandler(Event @event);

        public event EventHandler EventSent;
        
        public TaskProcessor(int seconds)
        {
            _period = seconds * 1000;

            _autoResetEvent = new AutoResetEvent(false);
            _eventQueue = new Queue();
            _timerTasks = new ArrayList();
        }

        public void EndlessProcess()
        {
            lock (_timerTasksLock)
            {
                foreach (var timerTask in _timerTasks)
                {
                    Debug.Print("To be processed: " + ((TimerTask)timerTask).Id + " - " + ((TimerTask)timerTask).DueTime.ToString("T"));
                }
            }


            while (true)
            {
                Debug.Print("Process: " + DateTime.Now.ToString("T"));

                Watchdog.ResetCounter();

                var nextEvent = DequeueEvent();

                if (nextEvent != null)
                {
                    Debug.Print("Process event " + nextEvent.EventType + ": " + nextEvent.DateTime.ToString("T"));

                    if (EventSent != null)
                    {
                        EventSent(nextEvent);
                    }
                }

                var nextTimerTask = PeekTask();

                if (nextTimerTask != null)
                {
                    long seconds = DateTime.Now.Subtract(nextTimerTask.DueTime).Ticks / TicksPerMillisecond / 1000;
                    Debug.Print("Seconds until next TimerTask: " + seconds);

                    if (seconds > 0 && seconds <= _period)
                    {
                        nextTimerTask.Callback(nextTimerTask.Value);
                        RemoveTask();
                    }
                }

                if (GetQueueCount() == 0)
                {
                    _autoResetEvent.WaitOne(_period, false);
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public void SendEvent(Event @event)
        {
            EnqueueEvent(@event);
            _autoResetEvent.Set();
        }

        private void EnqueueEvent(Event @event)
        {
            lock (_eventQueueLock)
            {
                _eventQueue.Enqueue(@event);
            }
        }

        private Event DequeueEvent()
        {
            lock (_eventQueueLock)
            {
                if (_eventQueue.Count > 0)
                {
                    return (Event)_eventQueue.Dequeue();
                }
                return null;
            }
        }

        private int GetQueueCount()
        {
            lock (_eventQueueLock)
            {
                return _eventQueue.Count;
            }
        }

        public bool TryAddTask(TimerTask timerTask)
        {
            if (timerTask.DueTime <= DateTime.Now)
            {
                return false;
            }

            lock (_timerTasksLock)
            {
                if (_timerTasks.Count == 0)
                {
                    _timerTasks.Add(timerTask);
                    return true;
                }

                for (int i = 0; i < _timerTasks.Count; i++)
                {
                    var next = (TimerTask)_timerTasks[i];

                    if (next.DueTime > timerTask.DueTime)
                    {
                        _timerTasks.Insert(i, timerTask);
                        break;
                    }
                    
                    if (i == _timerTasks.Count - 1)
                    {
                        _timerTasks.Add(timerTask);
                    }
                }
            }

            return true;
        }

        private TimerTask PeekTask()
        {
            lock (_timerTasksLock)
            {
                return _timerTasks.Count > 0
                    ? (TimerTask)_timerTasks[0]
                    : null;
            }
        }

        private void RemoveTask()
        {
            lock (_timerTasksLock)
            {
                if (_timerTasks.Count > 0)
                {
                    _timerTasks.RemoveAt(0);
                }
            }
        }
    }
}