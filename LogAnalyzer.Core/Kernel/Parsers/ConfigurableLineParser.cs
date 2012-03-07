using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LogAnalyzer.Kernel.Parsers
{
	public sealed class ConfigurableLineParser : ILogLineParser
	{
		private string _logLineRegexText = @"^\[(?<Type>.)] \[(?<TID>.{3,4})] (?<Time>\d{2}\.\d{2}\.\d{4} \d{1,2}:\d{2}:\d{2})\t(?<Text>.*)$";

		private Regex _logLineRegex;
		private string _dateTimeFormat = "dd.MM.yyyy H:mm:ss";

		public ConfigurableLineParser()
		{
			CreateRegex();
		}

		public string LogLineRegexText
		{
			get { return _logLineRegexText; }
			set
			{
				if ( String.IsNullOrEmpty( value ) )
					throw new ArgumentException();

				_logLineRegexText = value;
				CreateRegex();
			}
		}

		private void CreateRegex()
		{
			_logLineRegex = new Regex( _logLineRegexText, RegexOptions.Multiline | RegexOptions.Compiled );
		}

		public string DateTimeFormat
		{
			get { return _dateTimeFormat; }
			set { _dateTimeFormat = value; }
		}

		public bool TryExtractLogEntryData( string line, ref string type, ref int threadId, ref DateTime time, ref string text )
		{
			// инициализация некорректными данными
			type = null;
			threadId = -1;
			time = DateTime.MinValue;
			text = null;

			Match match = _logLineRegex.Match( line );
			if ( !match.Success )
			{
				return false;
			}

			// извлекаем данные
			type = match.Groups[1].Value;
			if ( String.IsNullOrWhiteSpace( type ) || type.Length > 1 )
			{
				throw new InvalidOperationException();
			}

			string tidStr = match.Groups[2].Value;
			if ( !Int32.TryParse( tidStr, out threadId ) )
			{
				throw new InvalidOperationException();
			}

			string timeStr = match.Groups[3].Value;
			time = Parse( timeStr );

			text = match.Groups[4].Value;

			return true;
		}

		internal DateTime Parse( string dateString )
		{
			return DateTime.ParseExact( dateString, _dateTimeFormat, CultureInfo.InvariantCulture );
		}
	}
}
