namespace AdSoft.Fez.Ui
{
    using AdSoft.Fez;
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    using Microsoft.SPOT;

    public abstract class Control
    {
        protected readonly Lcd2004 Screen;
        protected readonly IKeyboard Keyboard;

        public int MaxLength;
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
#if DEBUG_UI
            Debug.Print("UI - " + Name + ".Setup");
#endif

            Col = col;
            Row = row;

            IsVisible = false;
            IsFocused = false;
            MaxLength = GetLength();
        }
        
        public virtual void Show(bool show = true)
        {
            if (show)
            {
#if DEBUG_UI
                Debug.Print("UI - " + Name + ".Show");
#endif
            
                IsVisible = true;
            }
            else
            {
#if DEBUG_UI
                Debug.Print("UI - " + Name + ".Hide");
#endif

                Unfocus();

                IsVisible = false;
                string placeHolder = "";
                for (int i = 0; i < MaxLength; i++)
                {
                    placeHolder += " ";
                }

                Screen.WriteAndReturnCursor(Col, Row, placeHolder);
            }
        }

        public virtual void Focus()
        {
#if DEBUG_UI
            Debug.Print("UI - " + Name + ".Focus");
#endif

            if (!IsFocused)
            {
                Keyboard.KeyPressed += OnKeyPressed;
            }

            IsFocused = true;
        }

        public virtual void Unfocus()
        {
#if DEBUG_UI
            Debug.Print("UI - " + Name + ".Unfocus");
#endif

            if (IsFocused)
            {
                Screen.CursorOff();

                Keyboard.KeyPressed -= OnKeyPressed;
            }

            IsFocused = false;
        }

        protected virtual void OnExitLeft()
        {
#if DEBUG_UI
            Debug.Print("UI - " + Name + ".OnExitLeft");
#endif

            if (ExitLeft != null)
            {
                ExitLeft();
            }
        }

        protected virtual void OnExitRight()
        {
#if DEBUG_UI
            Debug.Print("UI - " + Name + ".OnExitRight");
#endif

            if (ExitRight != null)
            {
                ExitRight();
            }
        }

        protected virtual void OnKeyPressed(Key key)
        {
#if DEBUG_UI
            Debug.Print("UI - " + Name + ".OnKeyPressed: " + KeyEx.KeyToString(key));
#endif

            if (KeyPressed != null)
            {
                KeyPressed(key);
            }
        }
        
        protected abstract int GetLength();
    }
}