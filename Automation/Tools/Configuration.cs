namespace HomeAutomation.Tools
{
    using System;
    using System.Collections;

    using AdSoft.Fez.Hardware.SdCard;

    using GHIElectronics.NETMF.Hardware;

    using HomeAutomation.Services.AutoTurnOffPump;

    internal class Configuration
    {
        private const string DstCfg = "dst.cfg";
        private const string DstStartCfg = "dstStart.cfg";
        private const string DstEndCfg = "dstEnd.cfg";

        private readonly SdCard _sdCard;
        private readonly Log _log;

        public DateTime Sunrise { get; private set;  }
        public DateTime Sunset { get; private set; }
        
        public int SunriseOffsetMin { get; private set; }
        public int SunsetOffsetMin { get; private set; }
        public int PressureLogIntervalMin { get; set; }

        public Services.AutoTurnOffPump.Configuration AutoTurnOffPumpConfiguration { get; private set; }

        public bool IsDst { get; private set; }
        public bool ManualStartDst { get; private set; }
        public bool ManualEndDst { get; private set; }

        public Configuration(SdCard sdCard, Log log)
        {
            _sdCard = sdCard;
            _log = log;

            AutoTurnOffPumpConfiguration = new Services.AutoTurnOffPump.Configuration();

            // Hardcoded values if config is missing

            SunriseOffsetMin = 0;
            SunsetOffsetMin = 0;
            PressureLogIntervalMin = 5;
            AutoTurnOffPumpConfiguration.Interval = 1;
            AutoTurnOffPumpConfiguration.MinPressure = 0.5;
            AutoTurnOffPumpConfiguration.MaxEventsCount = 2;
            AutoTurnOffPumpConfiguration.SignalLength = 500;
        }

        public void Load()
        {
            ReadSun();
            ReadDst();
            Read();
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

        private void ReadSun()
        {
            var now = RealTimeClock.GetTime();

            // Hardcoded values if config is missing
            Sunrise = new DateTime(now.Year, now.Month, now.Day, 6, 0, 0);
            Sunset = new DateTime(now.Year, now.Month, now.Day, 19, 0, 0); 
            
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

                Sunrise = ToTime(now, sunParts[1]);
                Sunset = ToTime(now, sunParts[2]);
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
                IsDst = dstExists;
            }

            bool dstStartExists;
            if (_sdCard.TryIsExists(DstStartCfg, out dstStartExists))
            {
                ManualStartDst = dstStartExists;
            }

            bool dstEndExists;
            if (_sdCard.TryIsExists(DstEndCfg, out dstEndExists))
            {
                ManualEndDst = dstEndExists;
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
                            SunriseOffsetMin = int.Parse(value);
                            break;
                        case "SunsetOffset":
                            SunsetOffsetMin = int.Parse(value);
                            break;
                        case "PressureLogInterval":
                            PressureLogIntervalMin = int.Parse(value);
                            break;

                        case "AutoTurnOffPump-Interval":
                            AutoTurnOffPumpConfiguration.Interval = byte.Parse(value);
                            break;
                        case "AutoTurnOffPump-MinPressure":
                            AutoTurnOffPumpConfiguration.MinPressure = double.Parse(value);
                            break;
                        case "AutoTurnOffPump-MaxEventsCount":
                            AutoTurnOffPumpConfiguration.MaxEventsCount = byte.Parse(value);
                            break;
                        case "AutoTurnOffPump-SignalLength":
                            AutoTurnOffPumpConfiguration.SignalLength = ushort.Parse(value);
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
