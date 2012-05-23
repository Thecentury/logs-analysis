using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.LoggingTemplates
{
	[Serializable]
	public sealed class LoggerUsage
	{
		public string ClassName { get; set; }
		public string MethodName { get; set; }
		public string FileName { get; set; }
		public int LineNumber { get; set; }
		public string MessageSeverity { get; set; }
		public string FormatString { get; set; }

		public static readonly int LineNotFound = -1;

		private Regex _regex;

		[XmlIgnore]
		public Regex Regex
		{
			get
			{
				if ( _regex == null )
				{
					CreateRegex();
				}

				return _regex;
			}
		}

		private static readonly Regex _formatPlaceholderRegex = new Regex( @"\{(\d+)\}", RegexOptions.Compiled );
		public static Regex FormatPlaceholderRegex
		{
			get { return _formatPlaceholderRegex; }
		}

		private void CreateRegex()
		{
			string pattern = FormatPlaceholderRegex.Replace( FormatString, "§§§${1}®®®" );
			pattern = pattern.EscapeRegexChars().Replace( "§§§", "(?<G" ).Replace( "®®®", ">.*)" );
			pattern = "^" + pattern + "$";

			_regex = new Regex( pattern, RegexOptions.Compiled );
		}
	}
}