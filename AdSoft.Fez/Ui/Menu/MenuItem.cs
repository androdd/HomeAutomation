namespace AdSoft.Fez.Ui.Menu
{
    public struct MenuItem
    {
        public MenuItem(byte key, string title) : this()
        {
            Key = key;
            Title = title;
        }

        public byte Key { get; private set; }

        public string Title { get; private set; }
    }
}
