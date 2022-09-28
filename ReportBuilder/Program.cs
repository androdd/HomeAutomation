namespace ReportBuilder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            var filePath = args[0];

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var directory = Path.GetDirectoryName(filePath);

            var allLines = File.ReadAllLines(@"D:\temp\Pressure_2022_09.csv");

            var dates = new List<DateTime>();

            var rows = new List<Row>();

            // Skip header
            for (int i = 1; i < allLines.Length; i++)
            {
                var lineParts = allLines[i].Split(',');
                var dateTime = DateTime.Parse(lineParts[0]);
                var pressure = double.Parse(lineParts[1]);

                var date = dateTime.Date;
                var time = new TimeSpan(dateTime.TimeOfDay.Hours, dateTime.TimeOfDay.Minutes, 0);

                var row = rows.SingleOrDefault(r => r.Time == time);
                var dateIndex = dates.IndexOf(date);


                if (dateIndex == -1)
                {
                    dates.Add(date);
                    dateIndex = dates.Count - 1;
                }

                if (row == null)
                {
                    row = new Row { Time = time, Values = new List<double?>() };
                    rows.Add(row);
                }

                var emptyCells = dateIndex - row.Values.Count;
                for (int j = 0; j < emptyCells; j++)
                {
                    row.Values.Add(null);
                }

                row.Values.Add(pressure);
            }

            rows = rows.OrderBy(r => r.Time).ToList();

            var reportFile = Path.Combine(directory, string.Format("{0} - Report.csv", fileName));

            var header = string.Format("Time,{0}\r\n", string.Join(",", dates.Select(d => d.ToString("d")).ToArray()));

            File.WriteAllText(reportFile, header);

            var lines = rows.Select(r => r.Time.ToString("g") + "," + string.Join(",", r.Values.Select(v => v.ToString()).ToArray())).ToArray();

            File.AppendAllLines(reportFile, lines);
        }
    }

    class Row
    {
        public TimeSpan Time { get; set; }

        public List<double?> Values { get; set; }
    }
}
