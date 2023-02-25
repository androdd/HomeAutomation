namespace AdSoft.Fez.Ui.Menu
{
    public class MenuItem
    {
        public MenuItem(byte key, string title)
        {
            Key = key;
            Title = title;
        }

        public byte Key { get; private set; }

        public string Title { get; set; }
    }
}
