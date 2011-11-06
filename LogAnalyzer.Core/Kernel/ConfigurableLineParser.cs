using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogAnalyzer.Kernel
{
	public sealed class ConfigurableLineParser : ILogLineParser
	{
		private string logLineRegexText = @"^\[(?<Type>.)] \[(?<TID>.{3,4})] (?<Time>\d{2}\.\d{2}\.\d{4} \d{1,2}:\d{2}:\d{2})\t(?<Text>.*)$";

		private Regex logLineRegex;
		private string dateTimeFormat = "dd.MM.yyyy H:mm:ss";

		public ConfigurableLineParser()
		{
			CreateRegex();
		}

		public string LogLineRegexText
		{
			get { return logLineRegexText; }
			set
			{
				if ( String.IsNullOrEmpty( value ) )
					throw new ArgumentException();

				logLineRegexText = value;
				CreateRegex();
			}
		}

		private void CreateRegex()
		{
			logLineRegex = new Regex( logLineRegexText, RegexOptions.Multiline | RegexOptions.Compiled );
		}

		public string DateTimeFormat
		{
			get { return dateTimeFormat; }
			set { dateTimeFormat = value; }
		}

		public bool TryExtractLogEntryData( string line, out string type, out int threadId, out DateTime time, out string text )
		{
			// инициализация некорректными данными
			type = null;
			threadId = -1;
			time = DateTime.MinValue;
			text = null;

			Match match = logLineRegex.Match( line );
			if ( !match.Success )
			{
				return false;
			}

			// выдираем данные
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
			return DateTime.ParseExact( dateString, dateTimeFormat, CultureInfo.InvariantCulture );
		}
	}
}
