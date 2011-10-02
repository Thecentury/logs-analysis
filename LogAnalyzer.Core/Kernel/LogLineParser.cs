using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogAnalyzer.Kernel
{
	public sealed class LogLineParser
	{
		private const string LogLineRegexText = @"^\[(?<Type>.)] \[(?<TID>.{3,4})] (?<Time>\d{2}\.\d{2}\.\d{4} \d{1,2}:\d{2}:\d{2})\t(?<Text>.*)$";

		private static readonly Regex logLineRegex = new Regex( LogLineRegexText, RegexOptions.Compiled | RegexOptions.Multiline );
		public static readonly string DateTimeFormat = "dd.MM.yyyy H:mm:ss";

		public bool TryExtractLogEntryData( string line, out string type, out int threadId, out DateTime time, out string text )
		{
			// инициализация некорректными данными
			type = null;
			threadId = -1;
			time = DateTime.MinValue;
			text = null;

			Match match = logLineRegex.Match( line );
			if ( match.Success )
			{
				// выдираем данные
				type = match.Groups["Type"].Value;
				if ( String.IsNullOrWhiteSpace( type ) || type.Length > 1 )
				{
					// todo ошибка!!!
					throw new InvalidOperationException();
				}

				string tidStr = match.Groups["TID"].Value;
				threadId = 0;
				if ( !Int32.TryParse( tidStr, out threadId ) )
				{
					// todo ошибка!!!
					throw new InvalidOperationException();
				}

				string timeStr = match.Groups["Time"].Value;
				time = DateTime.MinValue;
				if ( !DateTime.TryParseExact( timeStr, DateTimeFormat, null, DateTimeStyles.None, out time ) )
				{
					// todo error!
					throw new InvalidOperationException();
				}

				text = match.Groups["Text"].Value;

				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
