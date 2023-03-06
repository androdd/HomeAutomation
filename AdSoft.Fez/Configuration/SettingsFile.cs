namespace AdSoft.Fez.Configuration
{
    using System;
    using System.Collections;

    using AdSoft.Fez.Hardware.SdCard;

    using Microsoft.SPOT;

    public class SettingsFile
    {
        private readonly SdCard _sdCard;
        private readonly string _filename;
        private readonly ArrayList _settings;

        public SettingsFile(SdCard sdCard, string filename)
        {
            _sdCard = sdCard;
            _filename = filename;
            _settings = new ArrayList();
        }

        public bool TryLoadSettings()
        {
            ArrayList lines;
            if (!_sdCard.TryReadAllLines(_filename, out lines))
            {
                Debug.Print(_filename + " can not be read.");
                return false;
            }

            _settings.Clear();
            for (var i = 0; i < lines.Count; i++)
            {
                var line = (string)lines[i];

                var parts = line.Split(':', ';');

                if (parts.Length == 1)
                {
                    if (parts[0].Trim() == "")
                    {
                        continue;
                    }
                }

                _settings.Add(new Setting
                {
                    Key = parts[0].Trim(),
                    Value = parts.Length < 2
                        ? string.Empty
                        : parts[1].Trim()
                });
            }

            return true;
        }

        public string GetValue(string key)
        {
            for (var i = 0; i < _settings.Count; i++)
            {
                var setting = (Setting)_settings[i];
                if (setting.Key != key)
                {
                    continue;
                }

                return setting.Value;
            }
            
            return null;
        }

        public bool TryGetValue(string key, out string result)
        {
            result = GetValue(key);
            return result != null;
        }

        public int GetInt32Value(string key, int defaultValue)
        {
            var value = GetValue(key);

            int result;
            if (value != null && Converter.TryParse(value, out result))
            {
                return result;
            }
            
            Debug.Print(_settings + "=>" + key + " cannot be loaded as Int32. Will use:" + defaultValue);
            return defaultValue;
        }

        public bool TryGetInt32Value(string key, out int result)
        {
            var value = GetValue(key);

            result = 0;
            return value != null && Converter.TryParse(value, out result);
        }

        public byte GetByteValue(string key, byte defaultValue)
        {
            var value = GetValue(key);

            byte result;
            if (value != null && Converter.TryParse(value, out result))
            {
                return result;
            }

            Debug.Print(_settings + "=>" + key + " cannot be loaded as |Byte. Will use:" + defaultValue);
            return defaultValue;
        }

        public bool TryGetByteValue(string key, out byte result)
        {
            var value = GetValue(key);

            result = 0;
            return value != null && Converter.TryParse(value, out result);
        }

        public ushort GetUshortValue(string key, ushort defaultValue)
        {
            var value = GetValue(key);

            ushort result;
            if (value != null && Converter.TryParse(value, out result))
            {
                return result;
            }

            Debug.Print(_settings + "=>" + key + " cannot be loaded as UShort. Will use:" + defaultValue);
            return defaultValue;
        }

        public bool TryGetUshortValue(string key, out ushort result)
        {
            var value = GetValue(key);

            result = 0;
            return value != null && Converter.TryParse(value, out result);
        }

        public double GetDoubleValue(string key, double defaultValue)
        {
            var value = GetValue(key);

            double result;
            if (value != null && Converter.TryParse(value, out result))
            {
                return result;
            }

            Debug.Print(_settings + "=>" + key + " cannot be loaded as Double. Will use:" + defaultValue);
            return defaultValue;
        }

        public bool TryGetDoubleValue(string key, out double result)
        {
            var value = GetValue(key);

            result = 0;
            return value != null && Converter.TryParse(value, out result);
        }

        public void AddOrUpdateValue(string key, string newValue)
        {
            for (var i = 0; i < _settings.Count; i++)
            {
                var setting = (Setting)_settings[i];
                if (setting.Key != key)
                {
                    continue;
                }

                setting.Value = newValue;
                return;
            }

            _settings.Add(new Setting { Key = key, Value = newValue });
        }

        public bool TrySaveSettings()
        {
            bool exists;
            if (!_sdCard.TryIsExists(_filename, out exists))
            {
                return false;
            }

            ArrayList lines = new ArrayList();
            if (exists && !_sdCard.TryReadAllLines(_filename, out lines))
            {
                return false;
            }

            string newFile = _filename + ".new";

            if (_sdCard.TryIsExists(newFile, out exists) && exists && !_sdCard.TryDelete(newFile))
            {
                return false;
            }

            var appendResult = _sdCard.TryAppend(newFile,
                stream =>
                {
                    var writtenKeys = new ArrayList();
                    var hasExistingLines = false;

                    for (var i = 0; i < lines.Count; i++)
                    {
                        var line = (string)lines[i];

                        var parts = line.Split(':', ';');

                        string key;
                        string newLine = i == 0 ? string.Empty : "\r\n";
                        if (parts.Length == 1 && parts[0].Trim() != "")
                        {
                            key = parts[0].Trim();
                            var value = GetValue(key);

                            if (value == null)
                            {
                                newLine += key;
                            }
                            else
                            {
                                newLine += key + ": " + value;
                            }
                        }
                        else if (parts.Length == 2)
                        {
                            key = parts[0].Trim();
                            var value = GetValue(key) ?? parts[1].Trim();

                            newLine += key + ": " + value;
                        }
                        else
                        {
                            key = parts[0].Trim();
                            var value = GetValue(key) ?? parts[1].Trim();
                            var comment = parts[2].Trim();

                            newLine += key + ": " + value + " ; " + comment;
                        }

                        SdCard.WriteToStream(stream, newLine);
                        hasExistingLines = true;
                        
                        if (key != string.Empty)
                        {
                            writtenKeys.Add(key);
                        }
                    }

                    for (var i = 0; i < _settings.Count; i++)
                    {
                        var setting = (Setting)_settings[i];
                        if (StringExists(writtenKeys, setting.Key))
                        {
                            continue;
                        }

                        var start = hasExistingLines || i != 0
                            ? "\r\n"
                            : String.Empty;
                        SdCard.WriteToStream(stream, start + setting.Key + ": " + setting.Value);
                    }
                });

            if (!appendResult)
            {
                return false;
            }

            return _sdCard.TryDelete(_filename) && _sdCard.TryRename(newFile, _filename);
        }

        private bool StringExists(ArrayList strings, string value)
        {
            foreach (var s in strings)
            {
                if ((string)s == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}