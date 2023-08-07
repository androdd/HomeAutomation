// ReSharper disable StringIndexOfIsCultureSpecific.1 - Not applicable in .NETMF
namespace AdSoft.Fez.Configuration
{
    using System;
    using System.Collections;
    using System.IO;

    using AdSoft.Fez.Hardware.Storage;

    using Microsoft.SPOT;

    public class SettingsFile
    {
        private readonly IStorage _storage;
        private readonly string _filename;
        public ArrayList Settings { get; private set; }

        public SettingsFile(IStorage storage, string filename)
        {
            _storage = storage;
            _filename = filename;
            Settings = new ArrayList();
        }

        public bool TryLoadSettings()
        {
            if (!_storage.IsLoaded)
            {
                return false;
            }

            StreamReader reader = new StreamReader(_storage.GetPath(_filename));

            Settings.Clear();
            do
            {
                var line = reader.ReadLine();

                if (line.IndexOf(":") == -1)
                {
                    continue;
                }

                var parts = line.Split(':', ';');

                if (parts.Length == 1)
                {
                    if (parts[0].Trim() == "")
                    {
                        continue;
                    }
                }

                Settings.Add(new Setting
                {
                    Key = parts[0].Trim(),
                    Value = parts.Length < 2
                        ? string.Empty
                        : parts[1].Trim(),
                    TypeCode = TypeCode.String
                });
            } while (!reader.EndOfStream);

            reader.Close();
            reader.Dispose();

            return true;
        }

        public string GetValue(string key)
        {
            var setting = GetSetting(key);
            return setting == null ? null : setting.Value;
        }

        private Setting GetSetting(string key)
        {
            for (var i = 0; i < Settings.Count; i++)
            {
                var setting = (Setting)Settings[i];
                if (setting.Key != key)
                {
                    continue;
                }

                return setting;
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
            var setting = GetSetting(key);

            int result;
            if (setting != null && Converter.TryParse(setting.Value, out result))
            {
                setting.TypeCode = TypeCode.Int32;
                return result;
            }

            Debug.Print("Settings=>" + key + " cannot be loaded as Int32. Will use:" + defaultValue);
            return defaultValue;
        }

        public bool TryGetInt32Value(string key, out int result)
        {
            var setting = GetSetting(key);

            result = 0;
            if (setting != null && Converter.TryParse(setting.Value, out result))
            {
                setting.TypeCode = TypeCode.Int32;
                return true;
            }
            
            return false;
        }

        public byte GetByteValue(string key, byte defaultValue)
        {
            var setting = GetSetting(key);

            byte result;
            if (setting != null && Converter.TryParse(setting.Value, out result))
            {
                setting.TypeCode = TypeCode.Byte;
                return result;
            }

            Debug.Print("Settings=>" + key + " cannot be loaded as Byte. Will use:" + defaultValue);
            return defaultValue;
        }

        public bool TryGetByteValue(string key, out byte result)
        {
            var setting = GetSetting(key);

            result = 0;
            if (setting != null && Converter.TryParse(setting.Value, out result))
            {
                setting.TypeCode = TypeCode.Byte;
                return true;
            }
            
            return false;
        }

        public ushort GetUshortValue(string key, ushort defaultValue)
        {
            var setting = GetSetting(key);

            ushort result;
            if (setting != null && Converter.TryParse(setting.Value, out result))
            {
                setting.TypeCode = TypeCode.UInt16;
                return result;
            }

            Debug.Print("Settings=>" + key + " cannot be loaded as UShort. Will use:" + defaultValue);
            return defaultValue;
        }

        public bool TryGetUshortValue(string key, out ushort result)
        {
            var setting = GetSetting(key);

            result = 0;
            if (setting != null && Converter.TryParse(setting.Value, out result))
            {
                setting.TypeCode = TypeCode.UInt16;
                return true;
            }
            
            return false;
        }

        public double GetDoubleValue(string key, double defaultValue)
        {
            var setting = GetSetting(key);

            double result;
            if (setting != null && Converter.TryParse(setting.Value, out result))
            {
                setting.TypeCode = TypeCode.Double;
                return result;
            }

            Debug.Print("Settings=>" + key + " cannot be loaded as Double. Will use:" + defaultValue);
            return defaultValue;
        }

        public bool TryGetDoubleValue(string key, out double result)
        {
            var setting = GetSetting(key);

            result = 0;
            if (setting != null && Converter.TryParse(setting.Value, out result))
            {
                setting.TypeCode = TypeCode.Double;
                return true;
            }
            
            return false;
        }

        public void AddOrUpdateValue(string key, string newValue, TypeCode typeCode = TypeCode.String)
        {
            var setting = GetSetting(key);

            if (setting != null)
            {
                setting.Value = newValue;
            }
            else
            {
                Settings.Add(new Setting { Key = key, Value = newValue, TypeCode = typeCode });
            }
        }

        public bool TrySaveSettings()
        {
            bool exists;
            if (!_storage.TryIsExists(_filename, out exists))
            {
                return false;
            }

            ArrayList lines = new ArrayList();
            if (exists && !_storage.TryReadAllLines(_filename, out lines))
            {
                return false;
            }

            string newFile = _filename + ".new";

            if (_storage.TryIsExists(newFile, out exists) && exists && !_storage.TryDelete(newFile))
            {
                return false;
            }

            var appendResult = _storage.TryAppend(newFile,
                stream =>
                {
                    var writtenKeys = new ArrayList();
                    var hasExistingLines = false;

                    for (var i = 0; i < lines.Count; i++)
                    {
                        var line = (string)lines[i];

                        var parts = line.Split(':');

                        string key = string.Empty;
                        string newLine = i == 0 ? string.Empty : "\r\n";
                        if (parts.Length == 1)
                        {
                            if (parts[0].Trim() != "")
                            {
                                key = parts[0].Trim();
                                var value = GetValue(key);

                                if (value == null)
                                {
                                    newLine += key;
                                }
                                else
                                {
                                    newLine += key + " : " + value;
                                }
                            }
                        }
                        else
                        {
                            key = parts[0].Trim();
                            var valueAndComment = parts[1].Trim();
                            parts = valueAndComment.Split(';');

                            var value = GetValue(key) ?? parts[0].Trim();

                            if (parts.Length == 1)
                            {
                                newLine += key + " : " + value;
                            }
                            else
                            {
                                var comment = parts[1].Trim();

                                newLine += key + " : " + value + " ; " + comment;
                            }
                        }

                        StorageBase.WriteToStream(stream, newLine);
                        hasExistingLines = true;
                        
                        if (key != string.Empty)
                        {
                            writtenKeys.Add(key);
                        }
                    }

                    for (var i = 0; i < Settings.Count; i++)
                    {
                        var setting = (Setting)Settings[i];
                        if (StringExists(writtenKeys, setting.Key))
                        {
                            continue;
                        }

                        var start = hasExistingLines || i != 0
                            ? "\r\n"
                            : String.Empty;
                        StorageBase.WriteToStream(stream, start + setting.Key + ": " + setting.Value);
                    }
                });

            if (!appendResult)
            {
                return false;
            }

            return _storage.TryDelete(_filename) && _storage.TryRename(newFile, _filename);
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