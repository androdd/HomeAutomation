namespace HomeAutomation.Ui
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware.Interfaces;
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Hardware.Storage;
    using AdSoft.Fez.Ui;
    using AdSoft.Fez.Ui.Interfaces;

    using HomeAutomation.Services.Watering;

    public class StatusScreen : Control
    {
        private readonly IPumpStateSensor _pumpStateSensor;
        private readonly IPressureSensor _pressureSensor;
        private readonly IFlowRateSensor _flowRateSensor;
        private readonly IStorage _externalStorage;
        private readonly WateringService _wateringService;

        private readonly Timer _timer;
        
        
        private double _oldPressure;
        private string _oldTime;

        private double _oldFlowRate;

        private double _oldVolumeNorth;
        private double _oldVolumeSouth;
        private string _oldPumpState;
        
        private bool _oldValveMainNorth;
        private char _oldNorthSwitchState;
        private bool _oldValveMainSouth;
        private char _oldValveSouth1;
        private char _oldValveSouth2;
        private char _oldValveSouth3;
        private char _oldValveSouth4;

        public StatusScreen(
            string name,
            Lcd2004 screen,
            IKeyboard keyboard,
            IStorage externalStorage,
            IPumpStateSensor pumpStateSensor,
            IPressureSensor pressureSensor,
            IFlowRateSensor flowRateSensor,
            WateringService wateringService) 
            : base(name, screen, keyboard)
        {
            _externalStorage = externalStorage;
            _wateringService = wateringService;
            _pumpStateSensor = pumpStateSensor;
            _pressureSensor = pressureSensor;
            _flowRateSensor = flowRateSensor;

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

                    Screen.Write(0, 2, "VN:");
                    Screen.Write(8, 2, "VS:");
                    Screen.Write(15, 2, "P:");

                    Screen.Write(0, 3, "North:");
                    Screen.Write(9, 3, "South:");

                    Screen.Write(3, 0, GetPressure().ToString("F2"));
                    Screen.Write(15, 0, DateTime.Now.ToString("HH:mm"));

                    Screen.Write(3, 1, _flowRateSensor.FlowRate.ToString("F1"));
                    SetExternalStorageStatus(_externalStorage.IsLoaded ? Status.Available : Status.Unavailable);

                    Screen.Write(3, 2, _wateringService.NorthVolume.ToString("F0"));
                    Screen.Write(11, 2, _wateringService.SouthVolume.ToString("F0"));
                    Screen.Write(17, 2, _pumpStateSensor.IsWorking ? "On " : "Off");
                    
                    Screen.SetCursor(6, 3);
                    Screen.WriteChar(_wateringService.GetValveMainNorth() ? '*' : (char)219);
                    Screen.WriteChar(_wateringService.NorthSwitchState.ToString()[0]);
                    Screen.SetCursor(15, 3);
                    Screen.WriteChar(_wateringService.GetValveMainSouth() ? '*' : (char)219);
                    Screen.WriteChar(GetValveChar(1));
                    Screen.WriteChar(GetValveChar(2));
                    Screen.WriteChar(GetValveChar(3));
                    Screen.WriteChar(GetValveChar(4));
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

        private char GetValveChar(int valveId)
        {
            switch (_wateringService.GetValveSouth(valveId))
            {
                case ValveState.On:
                    return (char)(valveId - 1);
                case ValveState.Off:
                    return valveId.ToString()[0];
                case ValveState.Disabled:
                    return 'D';
                case ValveState.Invalid:
                    return 'I';
                default:
                    return 'E';
            }
        }

        private void Update(object state)
        {
            Screen.Sync(() =>
            {
                WriteIfChanged(3, 0, ref _oldPressure, GetPressure(), "F2", 5);
                WriteIfChanged(15, 0, ref _oldTime, DateTime.Now.ToString("HH:mm"));

                WriteIfChanged(3, 1, ref _oldFlowRate, _flowRateSensor.FlowRate, "F1", 5);

                WriteIfChanged(3, 2, ref _oldVolumeNorth, _wateringService.NorthVolume, "F0", 5);
                WriteIfChanged(11, 2, ref _oldVolumeSouth, _wateringService.SouthVolume, "F0", 4);
                WriteIfChanged(17, 2, ref _oldPumpState, _pumpStateSensor.IsWorking ? "On " : "Off");

                WriteIfChanged(6, 3, ref _oldValveMainNorth, _wateringService.GetValveMainNorth(), '*', (char)219);
                WriteIfChanged(7, 3, ref _oldNorthSwitchState, _wateringService.NorthSwitchState.ToString()[0]);
                WriteIfChanged(15, 3, ref _oldValveMainSouth, _wateringService.GetValveMainSouth(), '*', (char)219);
                WriteIfChanged(16, 3, ref _oldValveSouth1, GetValveChar(1));
                WriteIfChanged(17, 3, ref _oldValveSouth2, GetValveChar(2));
                WriteIfChanged(18, 3, ref _oldValveSouth3, GetValveChar(3));
                WriteIfChanged(19, 3, ref _oldValveSouth4, GetValveChar(4));
            });
        }

        private double GetPressure()
        {
            var pressure = _pressureSensor.Pressure;
            return pressure < 0 ? 0 : pressure;
        }

        private void WriteIfChanged(int col, int row, ref double oldValue, double newValue, string format, int maxLength = 0)
        {
            var diff = newValue - oldValue;
            if (diff > 0.01 || diff < -0.01)
            {
                oldValue = newValue;

                Screen.Sync(() =>
                {
                    Screen.Write(col, row, newValue.ToString(format));

                    if (maxLength > 0)
                    {
                        int newCol;
                        int newRow;
                        Screen.GetCursor(out newCol, out newRow);

                        var spaces = maxLength - (newCol - col);
                        for (int i = 0; i < spaces; i++)
                        {
                            Screen.WriteChar(' ');
                        }
                    }
                });
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

        public void SetExternalStorageStatus(Status status)
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
