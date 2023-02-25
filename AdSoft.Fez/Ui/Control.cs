namespace AdSoft.Fez.Ui
{
    using AdSoft.Fez;
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    public abstract class Control
    {
        protected readonly Lcd2004 Screen;
        protected readonly IKeyboard Keyboard;

        protected int MaxLength;
        public bool IsVisible;
        public bool IsFocused;

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

        protected void Setup(int col, int row)
        {
            DebugEx.UiPrint(Name, "Setup");

            Col = col;
            Row = row;

            IsVisible = false;
            IsFocused = false;
            MaxLength = GetLength();
        }
        
        public virtual void Show()
        {
            DebugEx.UiPrint(Name, "Show");
            
            IsVisible = true;
        }

        public virtual void Hide()
        {
            DebugEx.UiPrint(Name, "Hide");

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
            DebugEx.UiPrint(Name, "Focus");

            if (!IsFocused)
            {
                Keyboard.KeyPressed += OnKeyPressed;
            }

            IsFocused = true;
        }

        public virtual void Unfocus()
        {
            DebugEx.UiPrint(Name, "Unfocus");

            if (IsFocused)
            {
                Screen.CursorOff();

                Keyboard.KeyPressed -= OnKeyPressed;
            }

            IsFocused = false;
        }

        protected virtual void OnExitLeft()
        {
            DebugEx.UiPrint(Name, "OnExitLeft");

            if (ExitLeft != null)
            {
                ExitLeft();
            }
        }

        protected virtual void OnExitRight()
        {
            DebugEx.UiPrint(Name, "OnExitRight");

            if (ExitRight != null)
            {
                ExitRight();
            }
        }

        protected virtual void OnKeyPressed(Key key)
        {
            DebugEx.UiPrint(Name, "OnKeyPressed", key.ToString());

            if (KeyPressed != null)
            {
                KeyPressed(key);
            }
        }
        
        protected abstract int GetLength();
    }
}