namespace AdSoft.Fez.Hardware.Storage
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;

    using GHIElectronics.NETMF.IO;

    using Microsoft.SPOT;

    public delegate void StorageStatusChangedEventHandler(Status status);

    public delegate void FileOpenedCallback(FileStream stream);

    public abstract class StorageBase : IDisposable, IStorage
    {
        protected PersistentStorage Storage;

        protected abstract string Root { get; }

        public bool IsLoaded { get; set; }

        public static void WriteToStream(Stream stream, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        public virtual event StorageStatusChangedEventHandler StatusChanged;

        public bool TryIsExists(string filename, out bool result)
        {
            InitStorage();

            if (!IsLoaded)
            {
                result = false;
                return false;
            }

            string path = GetPath(filename);
            result = File.Exists(path);

            return true;
        }

        public bool TryReadAllLines(string filename, out ArrayList result)
        {
            InitStorage();

            if (!IsLoaded)
            {
                result = null;
                return false;
            }

            string path = GetPath(filename);
            if (!File.Exists(path))
            {
                result = null;
                return false;
            }

            byte[] data = File.ReadAllBytes(path);

            result = GetUtf8Lines(data);

            if (result == null)
            {
                result = null;
                return false;
            }

            return true;
        }

        public bool TryDelete(string filename)
        {
            InitStorage();

            if (!IsLoaded)
            {
                return false;
            }

            string path = GetPath(filename);
            File.Delete(path);

            Flush();

            return true;
        }

        public bool TryRename(string filename, string newName)
        {
            InitStorage();

            if (!IsLoaded)
            {
                return false;
            }

            string source = GetPath(filename);
            string destination = GetPath(newName);
            File.Move(source, destination);

            Flush();

            return true;
        }

        public bool TryCopy(string filename, string newName)
        {
            InitStorage();

            if (!IsLoaded)
            {
                return false;
            }

            string source = GetPath(filename);
            string destination = GetPath(newName);
            File.Copy(source, destination);

            Flush();

            return true;
        }

        public bool TryGetFiles(string path, out string[] files)
        {
            InitStorage();

            if (!IsLoaded)
            {
                files = null;
                return false;
            }

            files = Directory.GetFiles(GetPath(path));

            return true;
        }

        public bool TryReadFixedLengthLine(string filename, int lineLength, int lineNumber, out string result)
        {
            InitStorage();

            if (!IsLoaded)
            {
                result = null;
                return false;
            }

            string path = GetPath(filename);
            if (!File.Exists(path))
            {
                result = null;
                return false;
            }

            byte[] buffer = new byte[lineLength];

            using (var stream = File.OpenRead(path))
            {
                var seekPosition = (lineLength + 2) * (lineNumber - 1);
                if (seekPosition > stream.Length)
                {
                    result = null;
                    return false;
                }

                stream.Seek(seekPosition, SeekOrigin.Begin);

                stream.Read(buffer, 0, lineLength);
            }

            result = GetUtf8String(buffer);

            if (result == null)
            {
                result = null;
                return false;
            }

            return true;
        }

        public bool TryAppend(string filename, string text)
        {
            return TryAppend(filename, stream => { WriteToStream(stream, text); });
        }

        public bool TryAppend(string filename, FileOpenedCallback fileOpenedCallback)
        {
            InitStorage();

            if (!IsLoaded)
            {
                return false;
            }

            try
            {
                string path = GetPath(filename);

                using (var stream = File.OpenWrite(path))
                {
                    stream.Seek(0, SeekOrigin.End);

                    fileOpenedCallback(stream);
                }

                Flush();
            }
            catch (Exception ex)
            {
                Debug.Print("SD - " + ex.Message);
                return false;
            }

            return true;
        }

        public void Unmount()
        {
            IsLoaded = false;

            if (Storage == null)
            {
                return;
            }

            Storage.UnmountFileSystem();
            Storage.Dispose();
            Storage = null;
        }

        public void Dispose()
        {
            Unmount();
        }

        protected abstract void InitStorage();

        protected abstract void Flush();

        protected string GetPath(string filename)
        {
            if (filename[0] == '\\')
            {
                filename = filename.Substring(1);
            }

            return Path.Combine(Root, filename);
        }

        protected void RaiseStatusChanged(Status status)
        {
            var onStatusChanged = StatusChanged;
            if (onStatusChanged != null)
            {
                onStatusChanged(status);
            }
        }

        protected static ArrayList GetUtf8Lines(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            ArrayList result = new ArrayList();

            var chars = Encoding.UTF8.GetChars(data);

            string line = string.Empty;
            for (int i = 0; i < chars.Length; i++)
            {
                var ch = chars[i];
                if (ch == '\r')
                {
                    
                }
                else if (ch == '\n')
                {
                    result.Add(line);
                    line = string.Empty;
                }
                else
                {
                    line += ch;
                }
            }
            result.Add(line);

            return result;
        }

        protected static string GetUtf8String(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            string result = string.Empty;

            var chars = Encoding.UTF8.GetChars(data);

            for (int i = 0; i < chars.Length; i++)
            {
                var ch = chars[i];
                result += ch;
            }

            return result;
        }
    }
}