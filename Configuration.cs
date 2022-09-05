namespace HomeAutomation
{
    using System;

    using GHIElectronics.NETMF.Hardware;

    internal class Configuration
    {
        private readonly SdCard _sdCard;
        private readonly Log _log;

        public DateTime Sunrise { get; private set;  }

        public DateTime Sunset { get; private set; }

        public Configuration(SdCard sdCard, Log log)
        {
            _sdCard = sdCard;
            _log = log;
        }

        public void Load()
        {
            ReadSunConfiguration();
        }

        private void ReadSunConfiguration()
        {
            Sunrise = DateTime.MinValue;
            Sunset = DateTime.MaxValue;

            var now = RealTimeClock.GetTime();

            string month = now.Month.ToString();
            if (month.Length == 1)
            {
                month = "0" + month;
            }

            string sunToday;

            if (_sdCard.TryReadFixedLengthLine("SunDst" + month + ".txt", 19, now.Day, out sunToday))
            {
                _log.Write("Config: " + sunToday);
            }
            else
            {
                _log.Write("Config is missing");
                return;
            }
            
            try
            {
                var sunParts = sunToday.Split('\t');

                int day = int.Parse(sunParts[0]);

                if (day != now.Day)
                {
                    _log.Write("Config Wrn: Wrong day");
                }

                Sunrise = ToTime(now, sunParts[1]);
                Sunset = ToTime(now, sunParts[2]);
            }
            catch (Exception)
            {
                _log.Write("Config Err: Bad Sun config");
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
