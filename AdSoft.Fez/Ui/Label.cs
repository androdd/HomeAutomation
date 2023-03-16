namespace AdSoft.Fez.Ui
{
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Ui.Interfaces;

    public class Label : Control
    {
        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;

                if (IsVisible)
                {
                    Show();
                }
            }
        }

        public Label(string name, Lcd2004 screen, IKeyboard keyboard) : base(name, screen, keyboard)
        {
        }

        public void Setup(string text, int col, int row)
        {
            Text = text;

            base.Setup(col, row);
        }

        public override void Show(bool show = true)
        {
            Screen.WriteAndReturnCursor(Col, Row, Text);

            base.Show(show);
        }

        public override void Focus()
        {
        }

        public override void Unfocus()
        {
        }
        
        protected override int GetLength()
        {
            return Text.Length;
        }
    }
}