using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LogAnalyzer.Kernel.Parsers
{
    public class ConfigurableLineParser : ILogLineParser
    {
        private string _logLineRegexText = @"^\[(?<Type>.)] \[(?<TID>.{3,4})] (?<Time>\d{2}\.\d{2}\.\d{4} \d{1,2}:\d{2}:\d{2})\t(?<Text>.*)$";

        private Regex _logLineRegex;

        public ConfigurableLineParser()
        {
            CreateRegex();
        }

        public string LogLineRegexText
        {
            get => _logLineRegexText;
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentException();

                _logLineRegexText = value;
                CreateRegex();
            }
        }

        public int MaxTypeLength { get; set; } = 1;

        private void CreateRegex()
        {
            _logLineRegex = new Regex(_logLineRegexText, RegexOptions.Multiline | RegexOptions.Compiled);
        }

        public string DateTimeFormat { get; set; } = "dd.MM.yyyy H:mm:ss";

        protected virtual string ProcessType(string type)
        {
            return type;
        }

        public bool TryExtractLogEntryData(string line, ref string type, ref int threadId, ref DateTime time, ref string text)
        {
            // инициализация некорректными данными
            type = null;
            threadId = -1;
            time = DateTime.MinValue;
            text = null;

            Match match = _logLineRegex.Match(line);
            if (!match.Success)
            {
                return false;
            }

            // извлекаем данные
            type = ProcessType(match.Groups["Type"].Value);
            if (String.IsNullOrWhiteSpace(type) || type.Length > MaxTypeLength)
            {
                throw new InvalidOperationException();
            }

            string tidStr = match.Groups["TID"].Value;
            if (!Int32.TryParse(tidStr, out threadId))
            {
                throw new InvalidOperationException();
            }

            string timeStr = match.Groups["Time"].Value;
            time = ParseDate(timeStr);

            text = match.Groups["Text"].Value;

            return true;
        }

        protected virtual DateTime ParseDate(string dateString)
        {
            return DateTime.ParseExact(dateString, DateTimeFormat, CultureInfo.InvariantCulture);
        }
    }
}
