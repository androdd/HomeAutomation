namespace AdSoft.Fez.Ui
{
    using AdSoft.Fez.Hardware.NecRemote;
    using AdSoft.Fez.Ui.Interfaces;

    using Microsoft.SPOT;

    using Message = AdSoft.Fez.Hardware.NecRemote.Message;

    public class MiniRemoteKeyboard : IKeyboard
    {
        private readonly NecRemote _necRemote;

        public event KeyPressedEventHandler KeyPressed;

        public MiniRemoteKeyboard(NecRemote necRemote)
        {
            _necRemote = necRemote;
        }

        public void Init()
        {
            _necRemote.NecButtonPressed += NecRemoteOnNecButtonPressed;
        }

        private void NecRemoteOnNecButtonPressed(Message msg)
        {
            if (KeyPressed == null)
            {
                return;
            }

            Key key = Key.NoName;

            switch (msg.Command)
            {
                case 162:
                    key = Key.D1;
                    break;
                case 98:
                    key = Key.D2;
                    break;
                case 226:
                    key = Key.D3;
                    break;
                case 34:
                    key = Key.D4;
                    break;
                case 2:
                    key = Key.D5;
                    break;
                case 194:
                    key = Key.D6;
                    break;
                case 224:
                    key = Key.D7;
                    break;
                case 168:
                    key = Key.D8;
                    break;
                case 144:
                    key = Key.D9;
                    break;
                case 152:
                    key = Key.D0;
                    break;
                case 104:
                    key = Key.Multiply;
                    break;
                case 176:
                    key = Key.Escape;
                    break;
                case 56:
                    key = Key.Enter;
                    break;
                case 24:
                    key = Key.UpArrow;
                    break;
                case 74:
                    key = Key.DownArrow;
                    break;
                case 16:
                    key = Key.LeftArrow;
                    break;
                case 90:
                    key = Key.RightArrow;
                    break;
            }

#if DEBUG_UI
            Debug.Print("UI - KeyPressed: " + KeyEx.KeyToString(key));
#endif

            KeyPressed(key);
        }
    }
}