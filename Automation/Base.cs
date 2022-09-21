namespace HomeAutomation
{
    using System;

    public class Base
    {
        public static string Format(DateTime dateTime)
        {
            return dateTime.ToString("u").TrimEnd('Z');
        }
    }
}