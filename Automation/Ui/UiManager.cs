namespace HomeAutomation.Ui
{
    using System;
    using System.Collections;

    using AdSoft.Fez.Configuration;
    using AdSoft.Fez.Ui;
    using AdSoft.Fez.Ui.Menu;

    using HomeAutomation.Tools;

    using Microsoft.SPOT;

    using Configuration = HomeAutomation.Tools.Configuration;

    public class UiManager
    {
        private readonly Configuration _configuration;
        private readonly ConfigurationManager _configurationManager;
        private readonly Log _log;
        private readonly HardwareManager _hardwareManager;
        
        private readonly ControlsManager _controlsManager;
        private readonly LegoSmallRemoteKeyboard _keyboard;
        private readonly ScreenSaver _screenSaver;
        private readonly Menu _menu;
        private Menu _configMenu;
        private readonly Clock _clock;
        private readonly DatePicker _datePicker;
        private readonly TextDrum _textDrum;
        private readonly DoublePicker _doublePicker;

        private UiStatus _status;
        private ArrayList _allSettings;

        public Label SdCardStatus { get; private set; }

        public UiManager(Configuration configuration, ConfigurationManager configurationManager, Log log, HardwareManager hardwareManager)
        {
            _configuration = configuration;
            _configurationManager = configurationManager;
            _log = log;
            _hardwareManager = hardwareManager;

            _controlsManager = new ControlsManager();

            _keyboard = new LegoSmallRemoteKeyboard(hardwareManager.LegoRemote);
            _screenSaver = new ScreenSaver(hardwareManager.Screen, _keyboard);

            _menu = (Menu)_controlsManager.Add(new Menu("Menu", hardwareManager.Screen, _keyboard));
            _clock = (Clock)_controlsManager.Add(new Clock("Clock", hardwareManager.Screen, _keyboard));
            _datePicker = (DatePicker)_controlsManager.Add(new DatePicker("Date", hardwareManager.Screen, _keyboard));
            _textDrum = (TextDrum)_controlsManager.Add(new TextDrum("Drum", hardwareManager.Screen, _keyboard));
            _doublePicker = (DoublePicker)_controlsManager.Add(new DoublePicker("Double", hardwareManager.Screen, _keyboard));

            SdCardStatus = (Label)_controlsManager.Add(new Label("SdStatus", hardwareManager.Screen, _keyboard));
        }

        public void Setup()
        {
            _keyboard.Init();
            _screenSaver.Init(5 * 60, !_configuration.ManagementMode);

            _menu.Setup(new[]
            {
                new MenuItem(MenuKeys.ShowConfig, "Show Config"),
                new MenuItem(MenuKeys.SetTime, "Set Time"),
                new MenuItem(MenuKeys.SetDate, "Set Date"),
                new MenuItem(MenuKeys.ManagementMode, "Mgmt " + (_configuration.ManagementMode ? "Off" : "On")),
                new MenuItem(MenuKeys.Exit, "Exit")
            });
            
            _keyboard.KeyPressed += KeyboardOnKeyPressed;
            _menu.MenuItemEnter += MenuOnMenuItemEnter;
            
            _clock.GetTime += () => Program.Now;
            _clock.SetTime += time =>
            {
                Program.Now = time;

                _status = UiStatus.None;
            };
            _clock.Setup(15, 0);

            _datePicker.Setup(0, 0);
            _datePicker.KeyPressed += DatePickerOnKeyPressed;

            _textDrum.Setup(0, 0, 10, 4);

            _doublePicker.Setup(7, 0, 2, 11, 3);
            _doublePicker.KeyPressed += DoublePickerOnKeyPressed;

            SdCardStatus.Setup("     ", 15, 1);

            _clock.Show();
            SdCardStatus.Show();

            _hardwareManager.ScreenPowerButton.StateChanged += ScreenPowerButtonOnStateChanged;
        }

        private void ScreenPowerButtonOnStateChanged(bool isOn)
        {
            if (isOn)
            {
                _controlsManager.Show();
                _controlsManager.Start();
            }
            else
            {
                _controlsManager.Stop();
            }
        }

        private void MenuOnMenuItemEnter(byte key)
        {
            _menu.Hide();

            switch (key)
            {
                case MenuKeys.SetTime:
                    _status = UiStatus.SetTime;

                    _clock.Edit();
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
                    if (_configuration.ManagementMode)
                    {
                        _screenSaver.Disable();
                        _menu.ChangeTitle(MenuKeys.ManagementMode, "Mgmt Off");
                    }
                    else
                    {
                        _screenSaver.Enable();
                        _menu.ChangeTitle(MenuKeys.ManagementMode, "Mgmt On");

                    }
                    break;
                case MenuKeys.TunePressure:
                    _status = UiStatus.TunePressure;

                    _doublePicker.Value = _configuration.PressureSensorMultiplier;
                    _doublePicker.Show();
                    _doublePicker.Focus();

                    _textDrum.WriteInfinite(2 * 1000,
                        () =>
                        {
                            var pressure = _hardwareManager.PressureSensor.Pressure * _doublePicker.Value;
                            return pressure.ToString("F5");
                        });
                    break;
                case MenuKeys.ShowConfig:
                    _status = UiStatus.ShowConfig;

                    if (_configMenu == null)
                    {
                        _configMenu = (Menu)_controlsManager.Add(new Menu("ConfigMenu", _hardwareManager.Screen, _keyboard));
                        _allSettings = _configurationManager.GetAllSettings();

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
                case MenuKeys.Exit:
                    _status = UiStatus.None;
                    break;
            }
        }

        private void ConfigMenuOnMenuItemSelected(byte key)
        {
            _hardwareManager.Screen.WriteLine(_hardwareManager.Screen.Rows - 1, " => " + ((Setting)_allSettings[key]).Value + "                    ");
        }

        private void ConfigMenuOnKeyPressed(Key key)
        {
            if (key == Key.Escape)
            {
                _configMenu.Hide();
                _hardwareManager.Screen.WriteLine(_hardwareManager.Screen.Rows - 1, "                     ");
                _status = UiStatus.None;
                _clock.Show();
            }
        }

        private void DoublePickerOnKeyPressed(Key key)
        {
            if (key == Key.Enter)
            {
                _configuration.PressureSensorMultiplier = _doublePicker.Value;
                _hardwareManager.PressureSensor.PressureMultiplier = _doublePicker.Value;
            }
            else if (key == Key.Escape)
            {
                _hardwareManager.PressureSensor.PressureMultiplier = _configuration.PressureSensorMultiplier;
            }
            else
            {
                _hardwareManager.PressureSensor.PressureMultiplier = _doublePicker.Value;
                return;
            }

            _textDrum.Hide();
            _doublePicker.Hide();
            _status = UiStatus.None;
        }

        private void KeyboardOnKeyPressed(Key key)
        {
            if (key == Key.F8 && _status == UiStatus.None)
            {
                _status = UiStatus.Menu;

                _menu.Show();
                _menu.Focus();
            }
        }

        private void DatePickerOnKeyPressed(Key key)
        {
            if (key == Key.Enter)
            {
                var now = Program.Now;

                var newDateTime = new DateTime(_datePicker.Value.Year,
                    _datePicker.Value.Month,
                    _datePicker.Value.Day,
                    now.Hour,
                    now.Minute,
                    now.Second);

                Program.Now = newDateTime;
            }
            else if (key == Key.Escape)
            {
            }
            else
            {
                return;
            }
            
            _datePicker.Hide();
            _status = UiStatus.None;
        }
    }
}