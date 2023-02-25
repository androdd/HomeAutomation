namespace HomeAutomation
{
    using System;

    using AdSoft.Fez.Ui;
    using AdSoft.Fez.Ui.Menu;

    using GHIElectronics.NETMF.Hardware;

    using HomeAutomation.Tools;

    public class UiManager
    {
        private readonly Log _log;
        private readonly HardwareManager _hardwareManager;
        
        private readonly ControlsManager _controlsManager;
        private readonly LegoSmallRemoteKeyboard _keyboard;
        private readonly ScreenSaver _screenSaver;
        private readonly Menu _menu;
        private readonly Clock _clock;
        private readonly DatePicker _datePicker;
        public Label SdCardStatus { get; private set; }

        public UiManager(Log log, HardwareManager hardwareManager)
        {
            _log = log;
            _hardwareManager = hardwareManager;

            _controlsManager = new ControlsManager();

            _keyboard = new LegoSmallRemoteKeyboard(hardwareManager.LegoRemote);
            _screenSaver = new ScreenSaver(hardwareManager.Screen, _keyboard);

            _menu = (Menu)_controlsManager.Add(new Menu("Menu", hardwareManager.Screen, _keyboard));
            _clock = (Clock)_controlsManager.Add(new Clock("Clock", hardwareManager.Screen, _keyboard));
            _datePicker = (DatePicker)_controlsManager.Add(new DatePicker("Date", hardwareManager.Screen, _keyboard));
            SdCardStatus = (Label)_controlsManager.Add(new Label("SdStatus", hardwareManager.Screen, _keyboard));
        }

        public void Setup()
        {
            _keyboard.Init();
            _screenSaver.Init(5 * 60);

            _menu.Setup(new[]
            {
                new MenuItem(MenuKeys.SetTime, "Set Time"),
                new MenuItem(MenuKeys.SetDate, "Set Date"),
                new MenuItem(MenuKeys.Exit, "Exit")
            });

            _keyboard.KeyPressed += KeyboardOnKeyPressed;
            _menu.MenuItemEnter += MenuOnMenuItemEnter;
            
            _clock.GetTime += () => Program.Now;
            _clock.SetTime += time => { Program.Now = time; };
            _clock.Setup(15, 0);

            _datePicker.Setup(0, 0);
            _datePicker.KeyPressed += DatePickerOnKeyPressed;

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
                    _clock.Edit();
                    break;
                case MenuKeys.SetDate:
                    _datePicker.Value = Program.Now;
                    _datePicker.Show();
                    _datePicker.Focus();
                    break;
                case MenuKeys.Exit:
                    break;
            }
        }

        private void KeyboardOnKeyPressed(Key key)
        {
            if (key == Key.F8)
            {
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

                _datePicker.Hide();
            }
            else if (key == Key.Escape)
            {
                _datePicker.Hide();
            }
        }
    }

    internal static class MenuKeys
    {
        public const byte SetTime = 0;
        public const byte SetDate = 1;
        public const byte Exit = 2;
    }
}
