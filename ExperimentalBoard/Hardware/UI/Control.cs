namespace AdSoft.Hardware.UI
{
    public abstract class Control
    {
        protected readonly Lcd2004 Screen;
        protected readonly IKeyboard Keyboard;

        protected int MaxLength;

        public int Col { get; set; }
        public int Row { get; set; }

        public event ActionEventHandler ExitLeft;
        public event ActionEventHandler ExitRight;
        public event KeyPressedEventHandler ButtonPressed;

        protected Control(Lcd2004 screen, IKeyboard keyboard)
        {
            Screen = screen;
            Keyboard = keyboard;
        }

        public virtual void Setup()
        {
            MaxLength = GetLength();
        }
        
        public virtual void Show(int col, int row)
        {
            Col = col;
            Row = row;
        }

        public void Hide()
        {
            string placeHolder = "";
            for (int i = 0; i < MaxLength; i++)
            {
                placeHolder += " ";
            }

            Screen.Write(Col, Row, placeHolder);
        }

        public virtual void Focus()
        {
            Keyboard.KeyPressed += OnKeyPressed;
        }

        public void Unfocus()
        {
            Screen.CursorOff();

            Keyboard.KeyPressed -= OnKeyPressed;
        }

        protected virtual void OnExitLeft()
        {
            if (ExitLeft != null)
            {
                ExitLeft();
            }
        }

        protected virtual void OnExitRight()
        {
            if (ExitRight != null)
            {
                ExitRight();
            }
        }

        protected virtual void OnKeyPressed(Key key)
        {
            if (ButtonPressed != null)
            {
                ButtonPressed(key);
            }
        }
        
        protected abstract int GetLength();
    }
}