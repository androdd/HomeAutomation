namespace HomeAutomation.Hardware.UI
{
    using Microsoft.SPOT;

    public abstract class Control
    {
        protected readonly Lcd2004 Screen;
        protected readonly IKeyboard Keyboard;

        protected int MaxLength;
        protected bool IsVisible;
        protected bool IsFocused;

        public int Col { get; set; }
        public int Row { get; set; }
        public string Name { get; private set; }

        public event ActionEventHandler ExitLeft;
        public event ActionEventHandler ExitRight;
        public event KeyPressedEventHandler KeyPressed;

        protected Control(string name, Lcd2004 screen, IKeyboard keyboard)
        {
            Name = name;
            Screen = screen;
            Keyboard = keyboard;
        }

        public virtual void Setup()
        {
            Debug.Print(Name + " Setup");

            IsVisible = false;
            IsFocused = false;
            MaxLength = GetLength();
        }
        
        public virtual void Show(int col, int row)
        {
            Debug.Print(Name + " Show");

            Col = col;
            Row = row;
            IsVisible = true;
        }

        public virtual void Hide()
        {
            Debug.Print(Name + " Hide");

            Unfocus();

            IsVisible = false;
            string placeHolder = "";
            for (int i = 0; i < MaxLength; i++)
            {
                placeHolder += " ";
            }

            Screen.WriteAndReturnCursor(Col, Row, placeHolder);
        }

        public virtual void Focus()
        {
            Debug.Print(Name + " Focus");

            if (!IsFocused)
            {
                Keyboard.KeyPressed += OnKeyPressed;
            }

            IsFocused = true;
        }

        public virtual void Unfocus()
        {
            Debug.Print(Name + " Unfocus");

            if (IsFocused)
            {
                Screen.CursorOff();

                Keyboard.KeyPressed -= OnKeyPressed;
            }

            IsFocused = false;
        }

        protected virtual void OnExitLeft()
        {
            Debug.Print(Name + " OnExitLeft");

            if (ExitLeft != null)
            {
                ExitLeft();
            }
        }

        protected virtual void OnExitRight()
        {
            Debug.Print(Name + " OnExitRight");

            if (ExitRight != null)
            {
                ExitRight();
            }
        }

        protected virtual void OnKeyPressed(Key key)
        {
            Debug.Print(Name + " OnKeyPressed");

            if (KeyPressed != null)
            {
                KeyPressed(key);
            }
        }
        
        protected abstract int GetLength();
    }
}