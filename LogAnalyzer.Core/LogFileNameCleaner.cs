using System.Text.RegularExpressions;

namespace LogAnalyzer
{
	public static class LogFileNameCleaner
	{
		private static readonly Regex startsWithDigitsRegex = new Regex( @"^\d{4}-\d{2}-\d{2}-(?<name>.*)",
																		RegexOptions.Compiled );

		public static string GetCleanedName( string logFileName )
		{
			string cleanName = logFileName;
			Match match = startsWithDigitsRegex.Match( logFileName );
			if ( match.Success )
			{
				cleanName = match.Groups["name"].Value;
			}

			cleanName = cleanName.Replace( ".log", "" );
			return cleanName;
		}
	}
}