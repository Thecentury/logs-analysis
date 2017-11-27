using System;
using System.Globalization;

namespace LogAnalyzer.Kernel.Parsers
{
    public sealed class Log4NetLogLineParser : ConfigurableLineParser
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss,fff";

        public Log4NetLogLineParser()
        {
            LogLineRegexText = @"^(?<Time>[\d\-]+\s+[\d:.,]+)\s+\[(?<TID>\d+)\]\s+(?<Type>[^\s]+)\s+(?<Text>.*)$";
            MaxTypeLength = 10;
        }


        protected override DateTime ParseDate(string dateString)
        {
            return DateTime.ParseExact(dateString, DateFormat, CultureInfo.InvariantCulture);
        }

        protected override string ProcessType(string type)
        {
            string firstLetter = type.Substring(0, 1);

            if (firstLetter == "F")
            {
                firstLetter = "E";
            }

            return string.Intern(firstLetter);
        }
    }
}