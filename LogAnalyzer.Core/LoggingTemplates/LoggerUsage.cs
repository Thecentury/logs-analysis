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

		private static readonly Regex formatPlaceholderRegex = new Regex( @"(\{\d+\})", RegexOptions.Compiled );
		public static Regex FormatPlaceholderRegex
		{
			get { return formatPlaceholderRegex; }
		}

		private static readonly Regex digitsRegex = new Regex( @"^\{(?<DIGIT>\d+)\}$", RegexOptions.Compiled );

		private void CreateRegex()
		{
			var pattern = BuildFullRegexPattern( FormatString );

			_regex = new Regex( pattern/*, RegexOptions.Compiled*/ );
		}

		public const char TemplatedPartStart = 'G';
		public const char ConstantPartStart = 'C';

		public static string BuildFullRegexPattern( string input )
		{
			string[] parts = formatPlaceholderRegex.Split( input );

			StringBuilder builder = new StringBuilder();

			int constantPartsCounter = 0;

			builder.Append( "^" );
			foreach ( string part in parts )
			{
				var digitsMatch = digitsRegex.Match( part );
				if ( digitsMatch.Success )
				{
					string groupNumber = digitsMatch.Groups["DIGIT"].Value;
					builder
						.Append( "(?<" )
						.Append( TemplatedPartStart )
						.Append( groupNumber )
						.Append( ">.*)" );
				}
				else
				{
					if ( !String.IsNullOrEmpty( part ) )
					{
						builder
							.Append( "(?<" )
							.Append( ConstantPartStart )
							.Append( constantPartsCounter )
							.Append( ">" )
							.Append( part.EscapeRegexChars() )
							.Append( ")" );
						constantPartsCounter++;
					}
				}
			}
			builder.Append( "$" );

			return builder.ToString();
		}
	}
}