using System;
using System.Text;
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

		private static readonly Regex _formatPlaceholderRegex = new Regex( @"(\{\d+\})", RegexOptions.Compiled );
		public static Regex FormatPlaceholderRegex
		{
			get { return _formatPlaceholderRegex; }
		}

		private static readonly Regex _digitsRegex = new Regex( @"^\{(?<DIGIT>\d+)\}$", RegexOptions.Compiled );

		private void CreateRegex()
		{
			var pattern = BuildRegexPattern( FormatString );

			_regex = new Regex( pattern, RegexOptions.Compiled );
		}

		public static string BuildRegexPattern( string input )
		{
			string[] parts = _formatPlaceholderRegex.Split( input );

			StringBuilder builder = new StringBuilder();

			builder.Append( "^" );
			foreach ( string part in parts )
			{
				var digitsMatch = _digitsRegex.Match( part );
				if ( digitsMatch.Success )
				{
					builder.Append( "(?<G" );
					string groupNumber = digitsMatch.Groups["DIGIT"].Value;
					builder.Append( groupNumber );
					builder.Append( ">.*)" );
				}
				else
				{
					if ( !String.IsNullOrEmpty( part ) )
					{
						builder.Append( "(" ).Append( part.EscapeRegexChars() ).Append( ")" );
					}
				}
			}
			builder.Append( "$" );

			return builder.ToString();

			//string pattern = FormatPlaceholderRegex.Replace( input, "§§§${1}®®®" );
			//pattern = pattern.EscapeRegexChars().Replace( "§§§", "(?<G" ).Replace( "®®®", ">.*)" );
			//pattern = "^" + pattern + "$";
			//return pattern;
		}
	}
}