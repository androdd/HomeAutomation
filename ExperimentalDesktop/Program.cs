namespace ExperimentalDesktop
{
    using System;
using System.Collections.Generic;

    public class TimeInterval
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public TimeInterval(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return string.Format("[{0} - {1}]", Start, End);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            TimeInterval[] intervals = new[]
            {
                new TimeInterval(new DateTime(2022, 01, 01, 7, 00, 00), new DateTime(2022, 01, 01, 7, 30, 00)),
                new TimeInterval(new DateTime(2022, 01, 01, 12, 00, 00), new DateTime(2022, 01, 01, 12, 30, 00)),
                new TimeInterval(new DateTime(2022, 01, 01, 7, 40, 00), new DateTime(2022, 01, 01, 7, 50, 00)),
                new TimeInterval(new DateTime(2022, 01, 01, 8, 40, 00), new DateTime(2022, 01, 01, 8, 50, 00)),
            };

            List<TimeInterval> nonOverlappingIntervals = GetNonOverlappingIntervals(intervals, 15);

            foreach (TimeInterval interval in nonOverlappingIntervals)
            {
                Console.WriteLine(interval);
            }

        }

        public static List<TimeInterval> GetNonOverlappingIntervals(TimeInterval[] intervals, int thresholdMinutes)
        {
            List<TimeInterval> nonOverlappingIntervals = new List<TimeInterval>();

            QuickSortDateTime(intervals, 0, intervals.Length - 1);

            TimeInterval currentInterval = intervals[0];

            for (int i = 1; i < intervals.Length; i++)
            {
                TimeInterval nextInterval = intervals[i];

                if (nextInterval.Start > currentInterval.End.AddMinutes(thresholdMinutes))
                {
                    nonOverlappingIntervals.Add(currentInterval);
                    currentInterval = nextInterval;
                }
                else
                {
                    currentInterval.End = currentInterval.End > nextInterval.End ? currentInterval.End : nextInterval.End;
                }
            }

            nonOverlappingIntervals.Add(currentInterval);

            return nonOverlappingIntervals;
        }

        public static void QuickSortDateTime(TimeInterval[] arr, int left, int right)
        {
            while (left < right)
            {
                int i = left, j = right;
                TimeInterval pivot = arr[(i + j) / 2];
                while (i <= j)
                {
                    while (arr[i].Start < pivot.Start) i++;
                    while (arr[j].Start > pivot.Start) j--;
                    if (i <= j)
                    {
                        TimeInterval tmp = arr[i];
                        arr[i] = arr[j];
                        arr[j] = tmp;
                        i++;
                        j--;
                    }
                }
                if (j - left <= right - i)
                {
                    QuickSortDateTime(arr, left, j);
                    left = i;
                }
                else
                {
                    QuickSortDateTime(arr, i, right);
                    right = j;
                }
            }
        }

        private static string ToBits(UInt16 data)
        {
            var text = "";
            text += (data / (1 << 15) % 2).ToString();
            text += (data / (1 << 14) % 2).ToString();
            text += (data / (1 << 13) % 2).ToString();
            text += (data / (1 << 12) % 2).ToString() + " ";
            text += (data / (1 << 11) % 2).ToString();
            text += (data / (1 << 10) % 2).ToString();
            text += (data / (1 << 9) % 2).ToString();
            text += (data / (1 << 8) % 2).ToString() + " ";

            text += (data / (1 << 7) % 2).ToString();
            text += (data / (1 << 6) % 2).ToString();
            text += (data / (1 << 5) % 2).ToString();
            text += (data / (1 << 4) % 2).ToString() + " ";
            text += (data / (1 << 3) % 2).ToString();
            text += (data / (1 << 2) % 2).ToString();
            text += (data / (1 << 1) % 2).ToString();
            text += (data % 2).ToString();

            return text;
        }

        public enum DebugTarget : ushort
        {
            ScreenSaver = 1 << 2,
            Ui = 1 << 14,
            Log = 1 << 6
        }
    }
}
