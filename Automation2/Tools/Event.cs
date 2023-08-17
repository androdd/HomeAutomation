namespace HomeAutomation2.Tools
{
    using System;

    public class Event
    {
        public Event(EventType eventType, object value)
        {
            DateTime = DateTime.Now;
            EventType = eventType;
            Value = value;
        }

        public DateTime DateTime { get; private set; }

        public EventType EventType { get; set; }

        public object Value { get; set; }
    }
}