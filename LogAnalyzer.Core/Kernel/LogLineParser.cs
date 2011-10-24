using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogAnalyzer.Kernel
{
	public static class LogLineParser
	{
		private const string LogLineRegexText = @"^\[(?<Type>.)] \[(?<TID>.{3,4})] (?<Time>\d{2}\.\d{2}\.\d{4} \d{1,2}:\d{2}:\d{2})\t(?<Text>.*)$";

		private static readonly Regex logLineRegex = new Regex( LogLineRegexText, RegexOptions.Compiled | RegexOptions.Multiline );
		public static readonly string DateTimeFormat = "dd.MM.yyyy H:mm:ss";

		public static bool TryExtractLogEntryData( string line, out string type, out int threadId, out DateTime time, out string text )
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

		internal static DateTime Parse( string dateString )
		{
			int day = (dateString[0] - '0') * 10 + (dateString[1] - '0');
			int month = (dateString[3] - '0') * 10 + (dateString[4] - '0');
			int year = (dateString[6] - '0') * 1000 + (dateString[7] - '0') * 100 + (dateString[8] - '0') * 10 + (dateString[9] - '0');

			int pos = 12;
			int hour = (dateString[11] - '0');
			char nextAfterFirstHourLetter = dateString[12];
			if ( nextAfterFirstHourLetter != ':' )
			{
				hour *= 10;
				hour += (nextAfterFirstHourLetter - '0');
				pos = 13;
			}

			int minute = (dateString[pos + 1] - '0') * 10 + (dateString[pos + 2] - '0');
			int second = (dateString[pos + 4] - '0') * 10 + (dateString[pos + 5] - '0');

			DateTime result = new DateTime( year, month, day, hour, minute, second );
			return result;
		}
	}
}
