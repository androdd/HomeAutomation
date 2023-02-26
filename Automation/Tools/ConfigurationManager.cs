namespace HomeAutomation.Tools
{
    using System;
    using System.Collections;

    using AdSoft.Fez.Hardware.SdCard;

    using GHIElectronics.NETMF.Hardware;

    public class ConfigurationManager
    {
        private const string DstCfg = "dst.cfg";
        private const string DstStartCfg = "dstStart.cfg";
        private const string DstEndCfg = "dstEnd.cfg";
        private const string ManagementModeCfg = "MgmtMode.cfg";

        private readonly Configuration _configuration;
        private readonly SdCard _sdCard;
        private readonly Log _log;
        
        public ConfigurationManager(Configuration configuration, SdCard sdCard, Log log)
        {
            _configuration = configuration;
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
            ArrayList configLines;
            if (_sdCard.TryReadAllLines("config.txt", out configLines))
            {
                _log.Write("Config: loaded");
            }
            else
            {
                _log.Write("Config is missing. Fallback to hardcoded values.");
                return;
            }

            foreach (var configLine in configLines)
            {
                try
                {
                    var line = configLine.ToString().Trim();
                    if (line == "")
                    {
                        continue;
                    }

                    var configParts = line.Split(':', ';');
                    var name = configParts[0].Trim();
                    var value = configParts[1].Trim();
                    switch (name)
                    {
                        case "SunriseOffset":
                            _configuration.SunriseOffsetMin = int.Parse(value);
                            break;
                        case "SunsetOffset":
                            _configuration.SunsetOffsetMin = int.Parse(value);
                            break;
                        case "PressureLogInterval":
                            _configuration.PressureLogIntervalMin = int.Parse(value);
                            break;

                        case "AutoTurnOffPump-Interval":
                            _configuration.AutoTurnOffPumpConfiguration.Interval = byte.Parse(value);
                            break;
                        case "AutoTurnOffPump-MinPressure":
                            _configuration.AutoTurnOffPumpConfiguration.MinPressure = double.Parse(value);
                            break;
                        case "AutoTurnOffPump-MaxEventsCount":
                            _configuration.AutoTurnOffPumpConfiguration.MaxEventsCount = byte.Parse(value);
                            break;
                        case "AutoTurnOffPump-SignalLength":
                            _configuration.AutoTurnOffPumpConfiguration.SignalLength = ushort.Parse(value);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _log.Write("Config line (" + configLine + ") Err: " + ex.Message);
                }
            }
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