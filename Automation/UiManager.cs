namespace HomeAutomation
{
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
        private readonly Menu _menu;
        private readonly Clock _clock;
        public Label SdCardStatus { get; private set; }

        public UiManager(Log log, HardwareManager hardwareManager)
        {
            _log = log;
            _hardwareManager = hardwareManager;

            _controlsManager = new ControlsManager();

            _keyboard = new LegoSmallRemoteKeyboard(hardwareManager.LegoRemote);
            _menu = (Menu)_controlsManager.Add(new Menu("Menu", hardwareManager.Screen, _keyboard));
            _clock = (Clock)_controlsManager.Add(new Clock("Clock", hardwareManager.Screen, _keyboard));
            SdCardStatus = (Label)_controlsManager.Add(new Label("SdStatus", hardwareManager.Screen, _keyboard));
        }

        public void Setup()
        {
            _keyboard.Init();

            _menu.Setup(new[]
            {
                new MenuItem(MenuKeys.SetClock, "Set Clock"),
                new MenuItem(MenuKeys.ShowDate, "Show Date"),
                new MenuItem(MenuKeys.Exit, "Exit")
            });

            _keyboard.KeyPressed += KeyboardOnKeyPressed;
            _menu.MenuItemEnter += MenuOnMenuItemEnter;
            
            _clock.GetTime += () => Program.Now;
            _clock.SetTime += time => { Program.Now = time; };
            _clock.Setup(15, 0);

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
                case MenuKeys.SetClock:
                    _clock.Edit();
                    break;
                case MenuKeys.ShowDate:
                    _hardwareManager.Screen.Write(0, 0, Program.Now.ToString("yyyy-MM-dd"));
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
    }

    internal static class MenuKeys
    {
        public const byte SetClock = 0;
        public const byte ShowDate = 1;
        public const byte Exit = 2;
    }
}
