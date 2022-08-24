namespace HomeAutomation
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Threading;

    using GHIElectronics.NETMF.IO;

    using Microsoft.SPOT;

    internal class SdCard: IDisposable
    {
        private PersistentStorage _sdCard;
        private bool _isLoaded;

        public bool Exists
        {
            get { return PersistentStorage.DetectSDCard(); }
        }

        public bool TryReadAllLines(string filename, out ArrayList result)
        {
            InitCard();

            if (!_isLoaded)
            {
                result = null;
                return false;
            }

            string path;
            if (!TryGetPath(filename, out path))
            {
                Thread.Sleep(200);
                result = null;
                return false;
            }

            byte[] data = File.ReadAllBytes(path);

            result = GetUtf8Lines(data);

            if (result == null)
            {
                Thread.Sleep(200);
                result = null;
                return false;
            }

            Thread.Sleep(200);
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

            string path;
            if (!TryGetPath(filename, out path))
            {
                Thread.Sleep(200);
                result = null;
                return false;
            }

            byte[] buffer = new byte[lineLength];

            using (var stream = File.OpenRead(path))
            {
                var seekPosition = (lineLength + 2) * (lineNumber - 1);
                if (seekPosition > stream.Length)
                {
                    Thread.Sleep(200);
                    result = null;
                    return false;
                }

                stream.Seek(seekPosition, SeekOrigin.Begin);

                stream.Read(buffer, 0, lineLength);
            }

            result = GetUtf8String(buffer);

            if (result == null)
            {
                Thread.Sleep(200);
                result = null;
                return false;
            }

            Thread.Sleep(200);
            return true;
        }

        public void UnmountSdCard()
        {
            if (!_isLoaded)
            {
                return;
            }

            _sdCard.UnmountFileSystem();
            Thread.Sleep(200);
        }

        public void Dispose()
        {
            if (_sdCard == null)
            {
                return;
            }

            _sdCard.UnmountFileSystem();
            _sdCard.Dispose();
        }

        private void InitCard()
        {
            if (_sdCard != null)
            {
                return;
            }

            try
            {
                _sdCard = new PersistentStorage("SD");
                _sdCard.MountFileSystem();
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.Print("Failed to mount SD card: " + ex.Message);
                _isLoaded = false;
            }
        }

        private static bool TryGetPath(string filename, out string path)
        {
            if (filename[0] == '\\')
            {
                filename = filename.Substring(1);
            }

            path = Path.Combine("\\SD", filename);
            return File.Exists(path);
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
