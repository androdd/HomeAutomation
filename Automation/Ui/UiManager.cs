// ReSharper disable StringIndexOfIsCultureSpecific.1 - Not applicable in .NETMF
namespace HomeAutomation.Ui
{
    using System;
    using System.Collections;

    using AdSoft.Fez.Configuration;
    using AdSoft.Fez.Ui;
    using AdSoft.Fez.Ui.Menu;

    using HomeAutomation.Services;
    using HomeAutomation.Services.Watering;
    using HomeAutomation.Tools;

    public class UiManager
    {
        private readonly Configuration _configuration;
        private readonly ConfigurationManager _configurationManager;
        private readonly HardwareManager _hardwareManager;
        private readonly LightsService _lightsService;
        private readonly WateringService _wateringService;

        private readonly ControlsManager _controlsManager;
        private readonly MiniRemoteKeyboard _keyboard;
        private readonly ScreenSaver _screenSaver;
        private readonly Menu _menu;
        private Menu _configMenu;
        private readonly StatusScreen _statusScreen;
        private readonly DatePicker _datePicker;
        private readonly TimePicker _timePicker;
        private readonly TextDrum _textDrum;
        private readonly DoublePicker _doublePicker;

        private UiStatus _status;
        private ArrayList _allSettings;

        public UiManager(
            Configuration configuration,
            ConfigurationManager configurationManager,
            HardwareManager hardwareManager,
            LightsService lightsService,
            WateringService wateringService)
        {
            _configuration = configuration;
            _configurationManager = configurationManager;
            _hardwareManager = hardwareManager;
            _lightsService = lightsService;
            _wateringService = wateringService;

            _controlsManager = new ControlsManager();

            _keyboard = new MiniRemoteKeyboard(hardwareManager.NecRemote);
            _screenSaver = new ScreenSaver(hardwareManager.Screen, _keyboard);

            _menu = (Menu)_controlsManager.Add(new Menu("Menu", hardwareManager.Screen, _keyboard));
            _statusScreen = (StatusScreen)_controlsManager.Add(new StatusScreen("StatusScr", hardwareManager.Screen, _keyboard, hardwareManager, _wateringService));
            _datePicker = (DatePicker)_controlsManager.Add(new DatePicker("Date", hardwareManager.Screen, _keyboard));
            _timePicker = new TimePicker("Time", hardwareManager.Screen, _keyboard);
            _textDrum = (TextDrum)_controlsManager.Add(new TextDrum("Drum", hardwareManager.Screen, _keyboard));
            _doublePicker = (DoublePicker)_controlsManager.Add(new DoublePicker("Double", hardwareManager.Screen, _keyboard));
        }

        public void Setup()
        {
            CreateWateringChars();

            _keyboard.Init();
            _screenSaver.Init(3 * 60, !_configuration.ManagementMode);
            _hardwareManager.ScreenPowerButton.AddScreenSaver(_screenSaver);

            _menu.Setup(new[]
            {
                new MenuItem(MenuKeys.StartWatering, "Start watering"),
                new MenuItem(MenuKeys.ToggleLights, "Lights " + (_lightsService.GetLightsState() ? "Off" : "On")),
                new MenuItem(MenuKeys.ManagementMode, "Mgmt " + (_configuration.ManagementMode ? "Off" : "On")),
                new MenuItem(MenuKeys.TunePressure, "Tune Pressure"),
                new MenuItem(MenuKeys.TuneFlowRate, "Tune Flow"),
                new MenuItem(MenuKeys.ShowConfig, "Show Config"),
                new MenuItem(MenuKeys.SetTime, "Set Time"),
                new MenuItem(MenuKeys.SetDate, "Set Date")
            });
            
            _keyboard.KeyPressed += KeyboardOnKeyPressed;
            _menu.MenuItemEnter += MenuOnMenuItemEnter;
            _menu.KeyPressed += MenuOnKeyPressed;
            
            _statusScreen.Setup();
            
            _datePicker.Setup(0, 0);
            _datePicker.KeyPressed += DatePickerOnKeyPressed;

            _timePicker.Setup(15, 0);
            _timePicker.KeyPressed += TimePickerOnKeyPressed;

            _textDrum.Setup(0, 0, 10, 4);

            _doublePicker.Setup(7, 0, 2, 11, 3);
            _doublePicker.KeyPressed += DoublePickerOnKeyPressed;

            _statusScreen.Show();

            _hardwareManager.ScreenPowerButton.StateChanged += ScreenPowerButtonOnStateChanged;
            _hardwareManager.SdCard.CardStatusChanged += _statusScreen.SetSdCardStatus;
        }

        private void ScreenPowerButtonOnStateChanged(bool isOn)
        {
            if (isOn)
            {
                CreateWateringChars();

                _hardwareManager.Screen.Clear();

                _controlsManager.Show();
                _controlsManager.Start();
            }
            else
            {
                _controlsManager.Stop();
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

        private void MenuOnMenuItemEnter(byte key)
        {
            _menu.Show(false);

            switch (key)
            {
                case MenuKeys.SetTime:
                    _status = UiStatus.SetTime;

                    _timePicker.Value = DateTime.Now;
                    _timePicker.Show();
                    _timePicker.Focus();
                    break;
                case MenuKeys.SetDate:
                    _status = UiStatus.SetDate;

                    _datePicker.Value = Program.Now;
                    _datePicker.Show();
                    _datePicker.Focus();
                    break;
                case MenuKeys.ManagementMode:
                    _status = UiStatus.None;

                    _configuration.ManagementMode = !_configuration.ManagementMode;
                    _configurationManager.SetManagementMode(_configuration.ManagementMode);
                    _screenSaver.Enable(!_configuration.ManagementMode);
                    _menu.ChangeTitle(MenuKeys.ManagementMode, "Mgmt " + (_configuration.ManagementMode ? "Off" : "On"));

                    _statusScreen.Show();
                    break;
                case MenuKeys.TunePressure:
                    _status = UiStatus.TunePressure;

                    _doublePicker.Value = _configuration.PressureSensorMultiplier;
                    _doublePicker.Show();
                    _doublePicker.Focus();

                    _textDrum.WriteInfinite(2 * 1000,
                        () =>
                        {
                            var pressure = _hardwareManager.PressureSensor.Pressure * _doublePicker.Value /
                                           _hardwareManager.PressureSensor.PressureMultiplier;
                            return pressure.ToString("F5");
                        });
                    break;
                case MenuKeys.TuneFlowRate:
                    _status = UiStatus.TuneFlowRate;

                    _doublePicker.Value = _configuration.FlowRateSensorMultiplier;
                    _doublePicker.Show();
                    _doublePicker.Focus();

                    _textDrum.WriteInfinite(2 * 1000,
                        () =>
                        {
                            var flowRate = _hardwareManager.FlowRateSensor.FlowRate * _doublePicker.Value /
                                           _hardwareManager.FlowRateSensor.FlowRateMultiplier;
                            return flowRate.ToString("F5");
                        });
                    break;
                case MenuKeys.ShowConfig:
                    _status = UiStatus.ShowConfig;

                    _allSettings = _configurationManager.GetAllSettings();

                    if (_configMenu == null)
                    {
                        _configMenu = (Menu)_controlsManager.Add(new Menu("ConfigMenu", _hardwareManager.Screen, _keyboard));

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
                    }

                    _configMenu.Show();
                    _configMenu.Focus();
                    break;
                case MenuKeys.ToggleLights:
                    _status = UiStatus.None;

                    var areOn = _lightsService.GetLightsState();
                    _menu.ChangeTitle(MenuKeys.ToggleLights, "Lights " + (!areOn ? "Off" : "On"));
                    _lightsService.SetLights(!areOn, "Manual ");

                    _statusScreen.Show();
                    break;
                case MenuKeys.StartWatering:
                    _status = UiStatus.None;

                    _wateringService.Start();

                    _statusScreen.Show();
                    break;
            }
        }

        private void ConfigMenuOnMenuItemSelected(byte key)
        {
            _hardwareManager.Screen.WriteLine(_hardwareManager.Screen.Rows - 1, "=" + ((Setting)_allSettings[key]).Value, true);
        }

        private void MenuOnKeyPressed(Key key)
        {
            if (key == Key.Escape)
            {
                _status = UiStatus.None;

                _statusScreen.Show();
            }
        }

        private void ConfigMenuOnKeyPressed(Key key)
        {
            if (key == Key.Escape)
            {
                _status = UiStatus.None;

                _hardwareManager.Screen.WriteLine(_hardwareManager.Screen.Rows - 1, "", true);
                _statusScreen.Show();
            }
        }

        private void DoublePickerOnKeyPressed(Key key)
        {
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

                    _configurationManager.Save();
                    break;
                }
                case Key.Escape:
                    break;
                default:
                    return;
            }

            _textDrum.Show(false);
            _doublePicker.Show(false);
            _statusScreen.Show();

            _status = UiStatus.None;
        }

        private void KeyboardOnKeyPressed(Key key)
        {
            if (key == Key.Enter && _status == UiStatus.None)
            {
                _status = UiStatus.Menu;

                _statusScreen.Show(false);

                _menu.Show();
                _menu.Focus();
            }
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

            _status = UiStatus.None;
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

            _status = UiStatus.None;
        }
    }
}
