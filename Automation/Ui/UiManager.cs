// ReSharper disable StringIndexOfIsCultureSpecific.1 - Not applicable in .NETMF
namespace HomeAutomation.Ui
{
    using System;
    using System.Collections;

    using AdSoft.Fez.Configuration;
    using AdSoft.Fez.Ui;
    using AdSoft.Fez.Ui.Menu;

    using HomeAutomation.Hardware;
    using HomeAutomation.Services;
    using HomeAutomation.Services.AutoTurnOffPump;
    using HomeAutomation.Services.Watering;
    using HomeAutomation.Tools;

    using Microsoft.SPOT;

    using Configuration = HomeAutomation.Tools.Configuration;

    public class UiManager
    {
        private readonly Configuration _configuration;
        private readonly ConfigurationManager _configurationManager;
        private readonly HardwareManager _hardwareManager;
        private readonly LightsService _lightsService;
        private readonly AutoTurnOffPumpService _autoTurnOffPumpService;
        private readonly WateringService _wateringService;

        private readonly MiniRemoteKeyboard _keyboard;
        private readonly ScreenSaver _screenSaver;
        private Menu _menu;
        private Menu _configMenu;
        private readonly StatusScreen _statusScreen;
        private DatePicker _datePicker;
        private TimePicker _timePicker;
        private TextDrum _textDrum;
        private DoublePicker _doublePicker;
        private NumericBox _numericBox1;
        private NumericBox _numericBox2;

        private UiStatus _status;
        private ArrayList _allSettings;

        public UiManager(
            Configuration configuration,
            ConfigurationManager configurationManager,
            HardwareManager hardwareManager,
            LightsService lightsService,
            AutoTurnOffPumpService autoTurnOffPumpService,
            WateringService wateringService)
        {
            _configuration = configuration;
            _configurationManager = configurationManager;
            _hardwareManager = hardwareManager;
            _lightsService = lightsService;
            _autoTurnOffPumpService = autoTurnOffPumpService;
            _wateringService = wateringService;
            
            _keyboard = new MiniRemoteKeyboard(hardwareManager.NecRemote);
            _screenSaver = new ScreenSaver(hardwareManager.Screen, _keyboard);

            _statusScreen = new StatusScreen("StatusScr",
                hardwareManager.Screen,
                _keyboard,
                _hardwareManager.PressureSensor,
                _hardwareManager.FlowRateSensor,
                _wateringService);
        }

        public void Setup()
        {
            CreateWateringChars();

            _keyboard.Init();
            _keyboard.KeyPressed += KeyboardOnKeyPressed;
            
            _screenSaver.Init(3 * 60);
            _hardwareManager.ScreenPowerButton.AddScreenSaver(_screenSaver);
            _hardwareManager.ScreenPowerButton.StateChanged += ScreenPowerButtonOnStateChanged;
            _hardwareManager.ExternalStorage.StatusChanged += _statusScreen.SetExternalStorageStatus;

            _autoTurnOffPumpService.StatusChanged += _statusScreen.SetAutoTurnOffPumpStatus;
            
            _statusScreen.Setup();
            _statusScreen.Show();
        }
        
        private void ScreenPowerButtonOnStateChanged(bool isOn)
        {
            if (isOn)
            {
                CreateWateringChars();

                _hardwareManager.Screen.Clear();

                _statusScreen.Show();
            }
        }

        private void CreateWateringChars()
        {
            _hardwareManager.Screen.CreateChar(0, new byte[] { 04, 12, 04, 20, 04, 04, 14, 31 });
            _hardwareManager.Screen.CreateChar(1, new byte[] { 14, 17, 01, 18, 04, 08, 31, 31 });
            _hardwareManager.Screen.CreateChar(2, new byte[] { 31, 02, 20, 02, 01, 17, 14, 31 });
            _hardwareManager.Screen.CreateChar(3, new byte[] { 18, 06, 10, 18, 31, 02, 02, 31 });
            _hardwareManager.Screen.CreateChar(4, new byte[] { 31, 16, 30, 01, 05, 17, 14, 31 });
            _hardwareManager.Screen.CreateChar(5, new byte[] { 06, 08, 17, 30, 17, 17, 14, 31 });
            _hardwareManager.Screen.CreateChar(6, new byte[] { 31, 17, 01, 18, 04, 04, 04, 31 });
            _hardwareManager.Screen.CreateChar(7, new byte[] { 14, 17, 21, 14, 21, 17, 14, 31 });
        }

        private void KeyboardOnKeyPressed(Key key)
        {
            if (key == Key.Enter && _status == UiStatus.None)
            {
                _status = UiStatus.Menu;

                _statusScreen.Show(false);

                _menu = CreateMainMenu();
                _menu.Show();
                _menu.Focus();
            }
            else if (_status == UiStatus.TestRelays)
            {
                TestRelayOrCancel(key);
            }
        }

        #region Main Menu

        private Menu CreateMainMenu()
        {
            var menu = new Menu("Menu", _hardwareManager.Screen, _keyboard);
            menu.Setup(new[]
            {
                new MenuItem(MenuKeys.ScheduleNextSouthWatering, "Start South Watering"),
                new MenuItem(MenuKeys.ScheduleNorthWatering, "Start North Watering"),
                new MenuItem(MenuKeys.StopWatering, "Stop Running Water"),
                new MenuItem(MenuKeys.CancelAutomaticWatering, "Cancel Auto Water"),
                new MenuItem(MenuKeys.CancelManualWatering, "Cancel Manual Water"),
                new MenuItem(MenuKeys.ResetVolume, "Reset Volume"),
                new MenuItem(MenuKeys.ToggleLights, "Lights " + (_lightsService.GetLightsState() ? "Off" : "On")),
                new MenuItem(MenuKeys.ShowConfig, "Show Config"),
                new MenuItem(MenuKeys.TestRelays, "Test Relays"),
                new MenuItem(MenuKeys.TunePressure, "Tune Pressure"),
                new MenuItem(MenuKeys.TuneFlowRate, "Tune Flow"),
                new MenuItem(MenuKeys.SetTime, "Set Time"),
                new MenuItem(MenuKeys.SetDate, "Set Date")
            });
            menu.MenuItemEnter += MenuOnMenuItemEnter;
            menu.KeyPressed += MenuOnKeyPressed;
            return menu;
        }

        private void MenuOnMenuItemEnter(byte key)
        {
            _menu.Show(false);

            switch (key)
            {
                case MenuKeys.SetTime:
                    ShowSetTime();
                    break;
                case MenuKeys.SetDate:
                    ShowSetDate();
                    break;
                case MenuKeys.TunePressure:
                    ShowTunePressure();
                    break;
                case MenuKeys.TuneFlowRate:
                    ShowTuneFlowRate();
                    break;
                case MenuKeys.ShowConfig:
                    ShowConfigMenu();
                    break;
                case MenuKeys.ToggleLights:
                    _status = UiStatus.None;

                    var areOn = _lightsService.GetLightsState();
                    _menu.ChangeTitle(MenuKeys.ToggleLights, "Lights " + (!areOn ? "Off" : "On"));
                    _lightsService.SetLights(!areOn, "Manual ");

                    _statusScreen.Show();
                    break;
                case MenuKeys.ScheduleNextSouthWatering:
                    ShowScheduleNextSouthWatering();
                    break;
                case MenuKeys.ScheduleNorthWatering:
                    ShowScheduleNorthWatering();
                    break;
                case MenuKeys.ResetVolume:
                    ResetVolume();
                    break;
                case MenuKeys.StopWatering:
                    StopWatering();
                    break;
                case MenuKeys.CancelAutomaticWatering:
                    CancelAutomaticWatering();
                    break;
                case MenuKeys.CancelManualWatering:
                    CancelManualWatering();
                    break;
                case MenuKeys.TestRelays:
                    TestRelays();
                    break;
            }
        }

        private void MenuOnKeyPressed(Key key)
        {
            if (key == Key.Escape)
            {
                _status = UiStatus.None;

                _statusScreen.Show();

                _menu = null;
            }
        }

        #endregion

        #region Manual watering

        private NumericBox CreateNumericBox(int col, int row, int minValue, int maxValue)
        {
            var numericBox = new NumericBox("Num", _hardwareManager.Screen, _keyboard);
            numericBox.Setup(col, row, minValue, maxValue);
            numericBox.KeyPressed += NumericBoxOnKeyPressed;
            return numericBox;
        }

        private void ShowScheduleNextSouthWatering()
        {
            _status = UiStatus.ScheduleNextSouthWatering;

            _hardwareManager.Screen.Write(0, 0, "Valve (1..3)");
            _hardwareManager.Screen.Write(0, 2, "Minutes (max 120)");

            _numericBox2 = CreateNumericBox(0, 3, 1, 120);
            _numericBox2.Value = 10;
            _numericBox2.Show();

            _numericBox1 = CreateNumericBox(0, 1, 1, 3);
            _numericBox1.Value = 1;
            _numericBox1.Show();
            _numericBox1.Focus();
        }

        private void ShowScheduleNorthWatering()
        {
            _status = UiStatus.ScheduleNorthWatering;

            _hardwareManager.Screen.Write(0, 0, "Corners (max 15 min)");
            _hardwareManager.Screen.Write(0, 2, "Main (max 120 min)");

            _numericBox2 = CreateNumericBox(0, 3, 1, 120);
            _numericBox2.Value = 50;
            _numericBox2.Show();

            _numericBox1 = CreateNumericBox(0, 1, 1, 15);
            _numericBox1.Value = 5;
            _numericBox1.Show();
            _numericBox1.Focus();
        }

        private void ResetVolume()
        {
            _status = UiStatus.None;

            _wateringService.ResetVolume();

            _statusScreen.Show();
        }

        private void StopWatering()
        {
            _status = UiStatus.None;

            _wateringService.StopRunning();

            _statusScreen.Show();
        }

        private void CancelAutomaticWatering()
        {
            _status = UiStatus.None;

            _wateringService.CancelAutomatic();

            _statusScreen.Show();
        }

        private void CancelManualWatering()
        {
            _status = UiStatus.None;

            _wateringService.CancelManual();

            _statusScreen.Show();
        }

        private void NumericBoxOnKeyPressed(Key key)
        {
            switch (key)
            {
                case Key.Enter:
                {
                    if (_numericBox1.IsFocused)
                    {
                        _numericBox1.Unfocus();
                        _numericBox2.Focus();
                        return;
                    }

                    if (_status == UiStatus.ScheduleNextSouthWatering)
                    {
                        _wateringService.TryStartSouth(_numericBox1.Value, _numericBox2.Value);
                    }
                    else if (_status == UiStatus.ScheduleNorthWatering)
                    {
                        _wateringService.TryStartNorth(_numericBox1.Value, _numericBox2.Value);
                    }

                    break;
                }
                case Key.Escape:
                    if (_numericBox2.IsFocused)
                    {
                        _numericBox2.Unfocus();
                        _numericBox1.Focus();
                        return;
                    }

                    break;
                default:
                    return;
            }

            _numericBox1.Show(false);
            _numericBox2.Show(false);
            _statusScreen.Show();

            _numericBox1 = null;
            _numericBox2 = null;

            _status = UiStatus.None;
        }

        #endregion

        #region Test Relays

        private bool[] _relaysStatus;

        private void TestRelays()
        {
            _status = UiStatus.TestRelays;

            _statusScreen.Show();

            _statusScreen.SetStatus("Test relay");

            var relays = _hardwareManager.RelaysArray;

            _relaysStatus = new bool[relays.Count];

            for (int i = 0; i < relays.Count; i++)
            {
                var isOn = relays.Get(i);

                _relaysStatus[i] = isOn;
            }
        }

        private void TestRelayOrCancel(Key key)
        {
            var relays = _hardwareManager.RelaysArray;

            if (key == Key.Escape)
            {
                _status = UiStatus.None;

                for (int i = 0; i < relays.Count; i++)
                {
                    relays.Set(i, _relaysStatus[i]);
                }

                _relaysStatus = null;

                _statusScreen.UpdateValveStatus();
                _statusScreen.ClearStatus();
            }
            else if (key >= Key.D0 && key <= Key.D7)
            {
                int relay = key - Key.D0;

                if (relay > relays.Count - 1)
                {
                    return;
                }

                var isOn = relays.Get(relay);

                relays.Set(relay, !isOn);

                _statusScreen.UpdateValveStatus();
            }
        }

        #endregion

        #region Configuration Menu

        private void ShowConfigMenu()
        {
            _status = UiStatus.ShowConfig;

            _allSettings = _configurationManager.GetAllSettings();

            _configMenu = new Menu("ConfigMenu", _hardwareManager.Screen, _keyboard);

            var menuItems = new MenuItem[_allSettings.Count];

            for (int i = 0; i < _allSettings.Count; i++)
            {
                var title = ((Setting)_allSettings[i]).Key;

                if (title.IndexOf("AutoTurnOffPump") == 0)
                {
                    title = "AP-" + title.Substring(16);
                }

                menuItems[i] = new MenuItem((byte)i, title);
            }

            _configMenu.Setup(menuItems, _hardwareManager.Screen.Rows - 1);
            _configMenu.KeyPressed += ConfigMenuOnKeyPressed;
            _configMenu.MenuItemSelected += ConfigMenuOnMenuItemSelected;

            _configMenu.Show();
            _configMenu.Focus();
        }

        private void ConfigMenuOnMenuItemSelected(byte key)
        {
            _hardwareManager.Screen.WriteLine(_hardwareManager.Screen.Rows - 1, "=" + ((Setting)_allSettings[key]).Value, true);
        }

        private void ConfigMenuOnKeyPressed(Key key)
        {
            if (key == Key.Escape)
            {
                _status = UiStatus.None;

                _statusScreen.Show();

                _configMenu = null;
            }
        }

        #endregion

        #region Tune Water Sensors

        private TextDrum CreateTextDrum()
        {
            var textDrum = new TextDrum("Drum", _hardwareManager.Screen, _keyboard);
            textDrum.Setup(0, 0, 10, 4);
            return textDrum;
        }

        private DoublePicker CreateDoublePicker()
        {
            var doublePicker = new DoublePicker("Double", _hardwareManager.Screen, _keyboard);
            doublePicker.Setup(7, 0, 2, 11, 3);
            doublePicker.KeyPressed += DoublePickerOnKeyPressed;
            return doublePicker;
        }

        private void ShowTuneFlowRate()
        {
            _status = UiStatus.TuneFlowRate;

            _doublePicker = CreateDoublePicker();
            _doublePicker.Value = _configuration.FlowRateSensorMultiplier;
            _doublePicker.Show();
            _doublePicker.Focus();

            _textDrum = CreateTextDrum();
            _textDrum.WriteInfinite(2 * 1000,
                () =>
                {
                    var flowRate = _hardwareManager.FlowRateSensor.FlowRate * _doublePicker.Value /
                                   _hardwareManager.FlowRateSensor.FlowRateMultiplier;
                    return flowRate.ToString("F5");
                });
        }

        private void ShowTunePressure()
        {
            _status = UiStatus.TunePressure;

            _doublePicker = CreateDoublePicker();
            _doublePicker.Value = _configuration.PressureSensorMultiplier;
            _doublePicker.Show();
            _doublePicker.Focus();

            _textDrum = CreateTextDrum();
            _textDrum.WriteInfinite(2 * 1000,
                () =>
                {
                    var pressure = _hardwareManager.PressureSensor.Pressure * _doublePicker.Value /
                                   _hardwareManager.PressureSensor.PressureMultiplier;
                    return pressure.ToString("F5");
                });
        }

        private void DoublePickerOnKeyPressed(Key key)
        {
            string status = "        ";
            switch (key)
            {
                case Key.Enter:
                {
                    if(_status == UiStatus.TunePressure)
                    {
                        _configurationManager.SetPressureSensorMultiplier(_doublePicker.Value);
                        _hardwareManager.PressureSensor.PressureMultiplier = _doublePicker.Value;
                    }
                    else if (_status == UiStatus.TuneFlowRate)
                    {
                        _configurationManager.SetFlowRateSensorMultiplier(_doublePicker.Value);
                        _hardwareManager.FlowRateSensor.FlowRateMultiplier = _doublePicker.Value;
                    }

                    if (!_configurationManager.Save())
                    {
                        status = "Save-Err  ";
                    }
                    break;
                }
                case Key.Escape:
                    break;
                default:
                    return;
            }

            _textDrum.Show(false);
            _doublePicker.Show(false);
            _statusScreen.Show(status);

            _textDrum = null;
            _doublePicker = null;

            _status = UiStatus.None;
        }

        #endregion

        #region Set Date

        private DatePicker CreateDatePicker()
        {
            var datePicker = new DatePicker("Date", _hardwareManager.Screen, _keyboard);
            datePicker.Setup(0, 0);
            datePicker.KeyPressed += DatePickerOnKeyPressed;
            return datePicker;
        }

        private void ShowSetDate()
        {
            _status = UiStatus.SetDate;

            _datePicker = CreateDatePicker();
            _datePicker.Value = Program.Now;
            _datePicker.Show();
            _datePicker.Focus();
        }

        private void DatePickerOnKeyPressed(Key key)
        {
            switch (key)
            {
                case Key.Enter:
                    var now = Program.Now;
                    var newDateTime = new DateTime(_datePicker.Value.Year, _datePicker.Value.Month, _datePicker.Value.Day, now.Hour, now.Minute, now.Second);
                    Program.Now = newDateTime;
                    break;
                case Key.Escape:
                    break;
                default:
                    return;
            }
            
            _datePicker.Show(false);
            _statusScreen.Show();

            _datePicker = null;

            _status = UiStatus.None;
        }

        #endregion

        #region Set Time

        private TimePicker CreateTimePicker()
        {
            var timePicker = new TimePicker("Time", _hardwareManager.Screen, _keyboard);
            timePicker.Setup(15, 0);
            timePicker.KeyPressed += TimePickerOnKeyPressed;
            return timePicker;
        }

        private void ShowSetTime()
        {
            _status = UiStatus.SetTime;

            _timePicker = CreateTimePicker();
            _timePicker.Value = DateTime.Now;
            _timePicker.Show();
            _timePicker.Focus();
        }

        private void TimePickerOnKeyPressed(Key key)
        {
            switch (key)
            {
                case Key.Enter:
                    var now = DateTime.Now;
                    var newDateTime = new DateTime(now.Year, now.Month, now.Day, _timePicker.Value.Hour, _timePicker.Value.Minute, 0);
                    Program.Now = newDateTime;
                    break;
                case Key.Escape:
                    break;
                default:
                    return;
            }

            _timePicker.Show(false);
            _statusScreen.Show();

            _timePicker = null;

            _status = UiStatus.None;
        }

        #endregion
    }
}
