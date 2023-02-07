namespace AdSoft.Hardware.UI
{
    public class LegoSmallRemoteKeyboard : IKeyboard
    {
        private readonly LegoRemote _legoRemote;

        public delegate void ButtonPressEventHandler(Key key);

        public event ButtonPressEventHandler OnButtonPress;

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
            if (OnButtonPress == null)
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

                    }
                    else if (msg.CommandA == Command.ComboDirectBackward)
                    {

                    }
                    else if (msg.CommandB == Command.ComboDirectForward)
                    {

                    }
                    else if (msg.CommandB == Command.ComboDirectBackward)
                    {

                    }
                    break;
                case 3:
                    if (msg.CommandA == Command.ComboDirectForward)
                    {

                    }
                    else if (msg.CommandA == Command.ComboDirectBackward)
                    {

                    }
                    else if (msg.CommandB == Command.ComboDirectForward)
                    {

                    }
                    else if (msg.CommandB == Command.ComboDirectBackward)
                    {

                    }
                    break;
                case 4:
                    if (msg.CommandA == Command.ComboDirectForward)
                    {

                    }
                    else if (msg.CommandA == Command.ComboDirectBackward)
                    {

                    }
                    else if (msg.CommandB == Command.ComboDirectForward)
                    {

                    }
                    else if (msg.CommandB == Command.ComboDirectBackward)
                    {

                    }
                    break;
            }

            OnButtonPress(key);
        }
    }
}
