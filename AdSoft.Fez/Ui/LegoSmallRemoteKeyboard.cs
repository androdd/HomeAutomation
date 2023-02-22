namespace AdSoft.Fez.Ui
{
    using AdSoft.Fez.Hardware.LegoRemote;
    using AdSoft.Fez.Ui.Interfaces;

    public class LegoSmallRemoteKeyboard : IKeyboard
    {
        private readonly LegoRemote _legoRemote;

        public event KeyPressedEventHandler KeyPressed;

        public LegoSmallRemoteKeyboard(LegoRemote legoRemote)
        {
            _legoRemote = legoRemote;
        }

        public void Init()
        {
            _legoRemote.OnLegoButtonPress += LegoRemoteOnOnLegoButtonPress;
        }

        private void LegoRemoteOnOnLegoButtonPress(Message msg)
        {
            if (KeyPressed == null)
            {
                return;
            }

            Key key = Key.NoName;

            switch (msg.Channel)
            {
                case 1:
                    if (msg.CommandA == Command.ComboDirectForward)
                    {
                        key = Key.UpArrow;
                    }
                    else if (msg.CommandA == Command.ComboDirectBackward)
                    {
                        key = Key.DownArrow;
                    }
                    else if (msg.CommandB == Command.ComboDirectForward)
                    {
                        key = Key.Enter;
                    }
                    else if (msg.CommandB == Command.ComboDirectBackward)
                    {
                        key = Key.Escape;
                    }
                    break;
                case 2:
                    if (msg.CommandA == Command.ComboDirectForward)
                    {
                        key = Key.UpArrow;
                    }
                    else if (msg.CommandA == Command.ComboDirectBackward)
                    {
                        key = Key.DownArrow;
                    }
                    else if (msg.CommandB == Command.ComboDirectForward)
                    {
                        key = Key.RightArrow;
                    }
                    else if (msg.CommandB == Command.ComboDirectBackward)
                    {
                        key = Key.LeftArrow;
                    }
                    break;
                case 3:
                    if (msg.CommandA == Command.ComboDirectForward)
                    {
                        key = Key.F1;
                    }
                    else if (msg.CommandA == Command.ComboDirectBackward)
                    {
                        key = Key.F2;
                    }
                    else if (msg.CommandB == Command.ComboDirectForward)
                    {
                        key = Key.F3;
                    }
                    else if (msg.CommandB == Command.ComboDirectBackward)
                    {
                        key = Key.F4;
                    }
                    break;
                case 4:
                    if (msg.CommandA == Command.ComboDirectForward)
                    {
                        key = Key.F5;
                    }
                    else if (msg.CommandA == Command.ComboDirectBackward)
                    {
                        key = Key.F5;
                    }
                    else if (msg.CommandB == Command.ComboDirectForward)
                    {
                        key = Key.F7;
                    }
                    else if (msg.CommandB == Command.ComboDirectBackward)
                    {
                        key = Key.F8;
                    }
                    break;
            }

            KeyPressed(key);
        }
    }
}
