namespace HomeAutomation.Ui
{
    using System;
    using System.Threading;

    using AdSoft.Fez;
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Hardware.SdCard;
    using AdSoft.Fez.Ui;
    using AdSoft.Fez.Ui.Interfaces;

    using Microsoft.SPOT;

    public class StatusScreen : Control
    {
        private readonly HardwareManager _hardwareManager;

        private readonly Clock _clock;
        private readonly Label _pressureRow;
        private readonly Label _flowRateRow;
        private readonly Label _waterRow;
        private readonly Label _waterSwitchRow;
        private readonly Timer _timer;
        private readonly object _sdCardStatusLock;

        private int _updateSeconds;

        public event Clock.SetTimeEventHandler SetTime;

        public StatusScreen(string name, Lcd2004 screen, IKeyboard keyboard, HardwareManager hardwareManager) 
            : base(name, screen, keyboard)
        {
            _hardwareManager = hardwareManager;

            _clock = new Clock(name + "_Clock", screen, keyboard);
            _pressureRow = new Label(name + "_PR", screen, keyboard);
            _flowRateRow = new Label(name + "_FR", screen, keyboard);
            _waterRow = new Label(name + "_WR", screen, keyboard);
            _waterSwitchRow = new Label(name + "_SR", screen, keyboard);

            _timer = new Timer(Update, null, Timeout.Infinite, Timeout.Infinite);
            _sdCardStatusLock = new object();
            _updateSeconds = 1;
        }

        public void Setup(int updateSeconds)
        {
            if (updateSeconds < 1)
            {
                updateSeconds = 1;
            }

            _clock.Setup(15, 0);
            _clock.GetTime += () => Program.Now;
            _clock.SetTime += time => { if (SetTime != null) SetTime(time); };

            _pressureRow.Setup("               ", 0, 0);
            _flowRateRow.Setup("                    ", 0, 1);
            _waterRow.Setup("                    ", 0, 1);
            _waterSwitchRow.Setup("                    ", 0, 1);

            _updateSeconds = updateSeconds;

            base.Setup(0, 0);
        }

        public override void Focus()
        {
        }

        public override void Unfocus()
        {
        }

        public override void Show(bool show = true)
        {
            Screen.Sync(() =>
            {
                _clock.Show(show);
                _pressureRow.Show(show);
                _flowRateRow.Show(show);
                _waterRow.Show(show);
                _waterSwitchRow.Show(show);
            });

            if (show)
            {
                _timer.Change(0, _updateSeconds * 1000);

                // ReSharper disable once RedundantArgumentDefaultValue
                base.Show(true);
            }
            else
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);

                DebugEx.Print(DebugEx.Target.Ui, Name + ".Hide");

                IsVisible = false;
            }
        }

        public void EditTime()
        {
            _clock.Edit();
        }

        private void Update(object state)
        {
            lock (_sdCardStatusLock)
            {
                Screen.Sync(() =>
                {
                    _pressureRow.Text = "Pr:" + _hardwareManager.PressureSensor.Pressure.ToString("F2");
                    
                    var sdCardStatusText = _flowRateRow.Text.Substring(15, 5);
                    var flowRateText = "FR:" + _hardwareManager.FlowRateSensor.FlowRate.ToString("F1") + "        ";

                    if (flowRateText.Length == 14)
                    {
                        flowRateText += " ";
                    }

                    _flowRateRow.Text = flowRateText + sdCardStatusText;
                });
            }
        }

        public void SetSdCardStatus(Status status)
        {
            string statusText;
            switch (status)
            {
                case Status.Available:
                    statusText = "     ";
                    break;
                case Status.Unavailable:
                    statusText = "S:N/A";
                    break;
                case Status.Error:
                    statusText = "S:Err";
                    break;
                default:
                    return;
            }

            lock (_sdCardStatusLock)
            {
                var flowRateText = _flowRateRow.Text.Substring(0, 15);

                _flowRateRow.Text = flowRateText + statusText;
            }
        }

        protected override int GetLength()
        {
            return Screen.Cols;
        }
    }
}
