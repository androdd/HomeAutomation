namespace HomeAutomation.Ui
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Hardware.SdCard;
    using AdSoft.Fez.Ui;
    using AdSoft.Fez.Ui.Interfaces;

    using HomeAutomation.Services.Watering;

    public class StatusScreen : Control
    {
        private readonly HardwareManager _hardwareManager;
        private readonly WateringService _wateringService;
        
        private readonly Timer _timer;
        
        
        private double _oldPressure;
        private string _oldTime;
        private double _oldFlowRate;
        private bool _oldValveMainNorth;
        private char _oldNorthSwitchState;
        private bool _oldValveMainSouth;
        private bool _oldValveSouth1;
        private bool _oldValveSouth2;
        private bool _oldValveSouth3;
        private bool _oldValveSouth4;

        public StatusScreen(string name, Lcd2004 screen, IKeyboard keyboard, HardwareManager hardwareManager, WateringService wateringService) 
            : base(name, screen, keyboard)
        {
            _hardwareManager = hardwareManager;
            _wateringService = wateringService;

            _timer = new Timer(Update, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Setup()
        {
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
            if (show)
            {
                Screen.Sync(() =>
                {
                    Screen.Clear();

                    Screen.Write(0, 0, "Pr:");
                    Screen.Write(0, 1, "FR:");
                    Screen.Write(15, 1, "S:");
                    Screen.Write(0, 3, "North:");
                    Screen.Write(9, 3, "South:");

                    Screen.Write(3, 0, _hardwareManager.PressureSensor.Pressure.ToString("F2"));
                    Screen.Write(15, 0, DateTime.Now.ToString("HH:mm"));
                    Screen.Write(3, 1, _hardwareManager.FlowRateSensor.FlowRate.ToString("F1"));
                    Screen.SetCursor(6, 3);
                    Screen.WriteChar(_wateringService.GetValveMainNorth() ? '*' : (char)219);
                    Screen.WriteChar(_wateringService.NorthSwitchState.ToString()[0]);
                    Screen.SetCursor(15, 3);
                    Screen.WriteChar(_wateringService.GetValveMainSouth() ? '*' : (char)219);
                    Screen.WriteChar(_wateringService.GetValveSouth(1) ? (char)0 : '1');
                    Screen.WriteChar(_wateringService.GetValveSouth(2) ? (char)1 : '2');
                    Screen.WriteChar(_wateringService.GetValveSouth(3) ? (char)2 : '3');
                    Screen.WriteChar(_wateringService.GetValveSouth(4) ? (char)3 : '4');
                });

                _timer.Change(0, 10 * 1000);

                // ReSharper disable once RedundantArgumentDefaultValue
                base.Show(true);
            }
            else
            {
                Screen.Clear();

                _timer.Change(Timeout.Infinite, Timeout.Infinite);

#if DEBUG_UI
                Debug.Print("UI - " + Name + ".Hide");
#endif

                IsVisible = false;
            }
        }

        private void Update(object state)
        {
            Screen.Sync(() =>
            {
                WriteIfChanged(3, 0, ref _oldPressure, _hardwareManager.PressureSensor.Pressure, "F2");
                WriteIfChanged(15, 0, ref _oldTime, DateTime.Now.ToString("HH:mm"));
                WriteIfChanged(3, 1, ref _oldFlowRate, _hardwareManager.FlowRateSensor.FlowRate, "F1");
                WriteIfChanged(6, 3, ref _oldValveMainNorth, _wateringService.GetValveMainNorth(), '*', (char)219);
                WriteIfChanged(7, 3, ref _oldNorthSwitchState, _wateringService.NorthSwitchState.ToString()[0]);
                WriteIfChanged(15, 3, ref _oldValveMainSouth, _wateringService.GetValveMainSouth(), '*', (char)219);
                WriteIfChanged(16, 3, ref _oldValveSouth1, _wateringService.GetValveSouth(1), (char)0, '1');
                WriteIfChanged(17, 3, ref _oldValveSouth2, _wateringService.GetValveSouth(2), (char)1, '2');
                WriteIfChanged(18, 3, ref _oldValveSouth3, _wateringService.GetValveSouth(3), (char)2, '3');
                WriteIfChanged(19, 3, ref _oldValveSouth4, _wateringService.GetValveSouth(4), (char)3, '4');
            });
        }

        private void WriteIfChanged(int col, int row, ref double oldValue, double newValue, string format)
        {
            var diff = newValue - oldValue;
            if (diff > 0.01 || diff < -0.01)
            {
                oldValue = newValue;
                Screen.Write(col, row, newValue.ToString(format));
            }
        }

        private void WriteIfChanged(int col, int row, ref string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                oldValue = newValue;
                Screen.Write(col, row, newValue);
            }
        }

        private void WriteIfChanged(int col, int row, ref bool oldValue, bool newValue, char trueChar, char falseChar)
        {
            if (oldValue != newValue)
            {
                oldValue = newValue;
                Screen.WriteChar(col, row, newValue ? trueChar : falseChar);
            }
        }

        private void WriteIfChanged(int col, int row, ref char oldValue, char newValue)
        {
            if (oldValue != newValue)
            {
                oldValue = newValue;
                Screen.WriteChar(col, row, newValue);
            }
        }

        public void SetSdCardStatus(Status status)
        {
            string statusText;
            switch (status)
            {
                case Status.Available:
                    statusText = "   ";
                    break;
                case Status.Unavailable:
                    statusText = "N/A";
                    break;
                case Status.Error:
                    statusText = "Err";
                    break;
                default:
                    return;
            }

            Screen.Write(17, 1, statusText);
        }

        protected override int GetLength()
        {
            return Screen.Cols;
        }
    }
}
