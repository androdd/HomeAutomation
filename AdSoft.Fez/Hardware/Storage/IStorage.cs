namespace AdSoft.Fez.Hardware.Storage
{
    using System.Collections;

    public interface IStorage
    {
        event StorageStatusChangedEventHandler StatusChanged;
        bool IsLoaded { get; set; }
        bool TryIsExists(string filename, out bool result);
        bool TryReadAllLines(string filename, out ArrayList result);
        bool TryDelete(string filename);
        bool TryRename(string filename, string newName);
        bool TryCopy(string filename, string newName);
        bool TryGetFiles(string path, out string[] files);
        bool TryReadFixedLengthLine(string filename, int lineLength, int lineNumber, out string result);
        bool TryAppend(string filename, string text);
        bool TryAppend(string filename, FileOpenedCallback fileOpenedCallback);
        void Unmount();
        void Dispose();
        string GetPath(string filename);
    }
}