namespace AdSoft.Fez.Hardware.SdCard
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;

    using GHIElectronics.NETMF.IO;

    using Microsoft.SPOT;

    public class SdCard: IDisposable
    {
        private PersistentStorage _sdCard;
        private bool _isLoaded;

        public delegate void CardStatusChangedEventHandler(Status status);

        public event CardStatusChangedEventHandler CardStatusChanged;

        public bool TryReadAllLines(string filename, out ArrayList result)
        {
            InitCard();

            if (!_isLoaded)
            {
                result = null;
                return false;
            }

            string path = GetPath(filename);
            if (!File.Exists(path))
            {
                //Thread.Sleep(200);
                result = null;
                return false;
            }

            byte[] data = File.ReadAllBytes(path);

            result = GetUtf8Lines(data);

            if (result == null)
            {
                //Thread.Sleep(200);
                result = null;
                return false;
            }

            //Thread.Sleep(200);
            return true;
        }

        public bool TryIsExists(string filename, out bool result)
        {
            InitCard();

            if (!_isLoaded)
            {
                result = false;
                return false;
            }

            string path = GetPath(filename);
            result = File.Exists(path);
            return true;
        }

        public bool TryDelete(string filename)
        {
            InitCard();

            if (!_isLoaded)
            {
                return false;
            }

            string path = GetPath(filename);
            File.Delete(path);
            return true;
        }

        public bool TryReadFixedLengthLine(string filename, int lineLength, int lineNumber, out string result)
        {
            InitCard();

            if (!_isLoaded)
            {
                result = null;
                return false;
            }

            string path = GetPath(filename);
            if (!File.Exists(path))
            {
                //Thread.Sleep(200);
                result = null;
                return false;
            }

            byte[] buffer = new byte[lineLength];

            using (var stream = File.OpenRead(path))
            {
                var seekPosition = (lineLength + 2) * (lineNumber - 1);
                if (seekPosition > stream.Length)
                {
                    //Thread.Sleep(200);
                    result = null;
                    return false;
                }

                stream.Seek(seekPosition, SeekOrigin.Begin);

                stream.Read(buffer, 0, lineLength);
            }

            result = GetUtf8String(buffer);

            if (result == null)
            {
                //Thread.Sleep(200);
                result = null;
                return false;
            }

            //Thread.Sleep(200);
            return true;
        }

        public bool TryAppend(string filename, string text)
        {
            InitCard();

            if (!_isLoaded)
            {
                return false;
            }

            try
            {
                string path = GetPath(filename);

                using (var stream = File.OpenWrite(path))
                {
                    var bytes = Encoding.UTF8.GetBytes(text);

                    stream.Seek(0, SeekOrigin.End);

                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return false;
            }

            //Thread.Sleep(200);
            return true;
        }

        public void Dispose()
        {
            if (_sdCard == null)
            {
                return;
            }

            _sdCard.UnmountFileSystem();
            _sdCard.Dispose();
            _sdCard = null;
        }

        private void InitCard()
        {
            if (!PersistentStorage.DetectSDCard())
            {
                Debug.Print("SD card missing");
                _isLoaded = false;

                if (_sdCard != null)
                {
                    _sdCard.UnmountFileSystem();
                    _sdCard.Dispose();
                    _sdCard = null;
                }

                RaiseStatusChanged(Status.Unavailable);

                return;
            }

            if (_sdCard != null)
            {
                return;
            }

            try
            {
                _sdCard = new PersistentStorage("SD");
                _sdCard.MountFileSystem();
                _isLoaded = true;
                RaiseStatusChanged(Status.Available);
            }
            catch (Exception ex)
            {
                Debug.Print("Failed to mount SD card: " + ex.Message);
                _isLoaded = false;

                RaiseStatusChanged(Status.Error);
            }
        }

        private void RaiseStatusChanged(Status status)
        {
            if (CardStatusChanged != null)
            {
                CardStatusChanged(status);
            }
        }

        private static string GetPath(string filename)
        {
            if (filename[0] == '\\')
            {
                filename = filename.Substring(1);
            }

            return Path.Combine("\\SD", filename);
        }

        private static ArrayList GetUtf8Lines(byte[] data)
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
                    result.Add(line);
                    line = string.Empty;
                    continue;
                }

                line += ch;
            }

            return result;
        }

        private static string GetUtf8String(byte[] data)
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
