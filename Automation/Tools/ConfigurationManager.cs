namespace HomeAutomation.Tools
{
    using System;
    using System.Collections;

    using AdSoft.Fez;
    using AdSoft.Fez.Configuration;
    using AdSoft.Fez.Hardware.SdCard;

    using GHIElectronics.NETMF.Hardware;

    public class ConfigurationManager
    {
        private const string DstCfg = "dst.cfg";
        private const string DstStartCfg = "dstStart.cfg";
        private const string DstEndCfg = "dstEnd.cfg";
        private const string ManagementModeCfg = "MgmtMode.cfg";

        private const string SunriseOffset = "SunriseOffset";
        private const string SunsetOffset = "SunsetOffset";
        private const string PressureLogInterval = "PressureLogInterval";
        private const string PressureSensorMultiplier = "PressureSensorMultiplier";
        private const string WaterFlowSensorMultiplier = "WaterFlowSensorMultiplier";

        private readonly Configuration _configuration;
        private readonly SettingsFile _settingsFile;
        private readonly SdCard _sdCard;
        private readonly Log _log;

        public ConfigurationManager(Configuration configuration, SettingsFile settingsFile, SdCard sdCard, Log log)
        {
            _configuration = configuration;
            _settingsFile = settingsFile;
            _sdCard = sdCard;
            _log = log;
        }

        public void Load()
        {
            ReadSun();
            ReadDst();
            Read();

            bool managementModeCfgExists;
            _configuration.ManagementMode = _sdCard.TryIsExists(ManagementModeCfg, out managementModeCfgExists) && managementModeCfgExists;
        }

        public void Save()
        {
            if (_settingsFile.TrySaveSettings())
            {
                DebugEx.Print("Configuration saved to file.");
            }
        }

        public void SaveDst()
        {
            _sdCard.TryAppend(DstCfg, "");
            _sdCard.TryDelete(DstStartCfg);
        }

        public void DeleteDst()
        {
            _sdCard.TryDelete(DstCfg);
            _sdCard.TryDelete(DstEndCfg);
        }

        public void SetManagementMode(bool on)
        {
            if (on)
            {
                _sdCard.TryAppend(ManagementModeCfg, "");
            }
            else
            {
                _sdCard.TryDelete(ManagementModeCfg);
            }
        }
        
        public ArrayList GetAllSettings()
        {
            var result = new ArrayList();

            foreach (var setting in _settingsFile.Settings)
            {
                result.Add(setting);
            }

            result.Add(new Setting { Key = "Sunrise", Value = _configuration.Sunrise.ToString("T"), TypeCode = TypeCode.Empty });
            result.Add(new Setting { Key = "Sunset", Value = _configuration.Sunset.ToString("T"), TypeCode = TypeCode.Empty });
            result.Add(new Setting { Key = "IsDst", Value = _configuration.IsDst.ToString(), TypeCode = TypeCode.Empty });
            result.Add(new Setting { Key = "ManualStartDst", Value = _configuration.ManualStartDst.ToString(), TypeCode = TypeCode.Empty });
            result.Add(new Setting { Key = "ManualEndDst", Value = _configuration.ManualEndDst.ToString(), TypeCode = TypeCode.Empty });
            result.Add(new Setting { Key = "ManagementMode", Value = _configuration.ManagementMode.ToString(), TypeCode = TypeCode.Empty });

            return result;
        }

        private void ReadSun()
        {
            var now = RealTimeClock.GetTime();
 
            string month = now.Month.ToString();
            if (month.Length == 1)
            {
                month = "0" + month;
            }

            string sunToday;

            if (_sdCard.TryReadFixedLengthLine("Sun" + month + ".txt", 19, now.Day, out sunToday))
            {
                _log.Write("Config Sun: " + sunToday);
            }
            else
            {
                _log.Write("Config Sun is missing. Fallback to hardcoded values.");
                return;
            }
            
            try
            {
                var sunParts = sunToday.Split('\t');

                int day = int.Parse(sunParts[0]);

                if (day != now.Day)
                {
                    _log.Write("Config SunDst Wrn: Wrong day");
                }

                _configuration.Sunrise = ToTime(now, sunParts[1]);
                _configuration.Sunset = ToTime(now, sunParts[2]);
            }
            catch (Exception ex)
            {
                _log.Write("Config Sun Err: " + ex.Message);
            }
        }

        private void ReadDst()
        {
            bool dstExists;
            if (_sdCard.TryIsExists(DstCfg, out dstExists))
            {
                _configuration.IsDst = dstExists;
            }

            bool dstStartExists;
            if (_sdCard.TryIsExists(DstStartCfg, out dstStartExists))
            {
                _configuration.ManualStartDst = dstStartExists;
            }

            bool dstEndExists;
            if (_sdCard.TryIsExists(DstEndCfg, out dstEndExists))
            {
                _configuration.ManualEndDst = dstEndExists;
            }
        }

        private void Read()
        {
            if (!_settingsFile.TryLoadSettings())
            {
                return;
            }

            _configuration.SunriseOffsetMin = _settingsFile.GetInt32Value(SunriseOffset, _configuration.SunriseOffsetMin);
            _configuration.SunsetOffsetMin = _settingsFile.GetInt32Value(SunsetOffset, _configuration.SunsetOffsetMin);
            _configuration.PressureLogIntervalMin = _settingsFile.GetInt32Value(PressureLogInterval, _configuration.PressureLogIntervalMin);
            _configuration.PressureSensorMultiplier = _settingsFile.GetDoubleValue(PressureSensorMultiplier, _configuration.PressureSensorMultiplier);
            _configuration.WaterFlowSensorMultiplier = _settingsFile.GetDoubleValue(WaterFlowSensorMultiplier, _configuration.WaterFlowSensorMultiplier);

            _configuration.AutoTurnOffPumpConfiguration.Interval =
                _settingsFile.GetByteValue("AutoTurnOffPump-Interval", _configuration.AutoTurnOffPumpConfiguration.Interval);
            _configuration.AutoTurnOffPumpConfiguration.MinPressure = 
                _settingsFile.GetDoubleValue("AutoTurnOffPump-MinPressure", _configuration.AutoTurnOffPumpConfiguration.MinPressure);
            _configuration.AutoTurnOffPumpConfiguration.MaxEventsCount = 
                _settingsFile.GetByteValue("AutoTurnOffPump-MaxEventsCount", _configuration.AutoTurnOffPumpConfiguration.MaxEventsCount);
            _configuration.AutoTurnOffPumpConfiguration.SignalLength = 
                _settingsFile.GetUshortValue("AutoTurnOffPump-SignalLength", _configuration.AutoTurnOffPumpConfiguration.SignalLength);
        }

        public void SetPressureSensorMultiplier(double value)
        {
            _configuration.PressureSensorMultiplier = value;
            _settingsFile.AddOrUpdateValue(PressureSensorMultiplier, value.ToString("F7"));
        }

        public void SetWaterFlowSensorMultiplier(double value)
        {
            _configuration.WaterFlowSensorMultiplier = value;
            _settingsFile.AddOrUpdateValue(WaterFlowSensorMultiplier, value.ToString("F7"));
        }

        private static DateTime ToTime(DateTime now, string text)
        {
            var parts = text.Split(':');

            var hour = int.Parse(parts[0]);
            var minute = int.Parse(parts[1]);
            var second = int.Parse(parts[2]);

            return new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
        }
    }
}