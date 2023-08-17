namespace HomeAutomation2
{
    using System;
    using System.Collections;

    using AdSoft.Fez.Ui;

    using GHIElectronics.NETMF.Hardware.LowLevel;

    using HomeAutomation2.Hardware;
    using HomeAutomation2.Tools;

    using Microsoft.SPOT;

    public class Program
    {
        private static HardwareManager _hardwareManager;
        private static TaskProcessor _taskProcessor;
        
        private const int Period = 5;

        public static void Main()
        {
            Watchdog.Enable(3 * Period * 1000);

            _hardwareManager = new HardwareManager(null);
            _hardwareManager.Setup();

            _taskProcessor = new TaskProcessor(Period);
            _taskProcessor.EventSent += @event =>
            {
                Debug.Print("Event received: " + @event.DateTime.ToString("T") + " - " + @event.Value);
            };


            var random = new Random();

            var count = 10;
            var offsets = new ArrayList();

            for (int i = 0; i < count; i++)
            {
                offsets.Add(i - count / 2);
            }


            AddTimerTask(40, 1);
            AddTimerTask(10, 1);
            AddTimerTask(30, 1);
            AddTimerTask(50, 1);


            MiniRemoteKeyboard keyboard = new MiniRemoteKeyboard(_hardwareManager.NecRemote);
            keyboard.Init();

            keyboard.KeyPressed += key =>
            {
                Debug.Print(key.ToString());
                Event myEvent = new Event(EventType.A, key);

                _taskProcessor.SendEvent(myEvent);
            };
            
            _taskProcessor.EndlessProcess();
        }



        private static void AddTimerTask(int secondsOffset, int value)
        {
            var timerTask = new TimerTask(DateTime.Now.AddSeconds(secondsOffset), value, TimerCallback);

            Debug.Print("Adding Timer Task: " + timerTask.Id + " - " + timerTask.DueTime.ToString("T"));

            _taskProcessor.TryAddTask(timerTask);
        }

        private static void TimerCallback(object state)
        {
            Debug.Print("Task: " + state + " - " + DateTime.Now.ToString("T"));
        }
    }
}
