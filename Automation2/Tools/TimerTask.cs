namespace HomeAutomation2.Tools
{
    using System;
    using System.Threading;

    public class TimerTask
    {
        public TimerTask(DateTime dueTime, object value, TimerCallback callback)
        {
            Id = Guid.NewGuid();
            DueTime = dueTime;
            Value = value;
            Callback = callback;
        }

        public Guid Id { get; private set; }

        public DateTime DueTime { get; set; }

        public object Value { get; set; }

        public TimerCallback Callback { get; set; }
    }
}