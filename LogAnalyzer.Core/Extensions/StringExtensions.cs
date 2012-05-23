using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace LogAnalyzer.Extensions
{
	public static class StringExtensions
	{
		[StringFormatMethod( "formatString" )]
		public static string Format2( this string formatString, params object[] parameters )
		{
			return String.Format( formatString, parameters );
		}

		public static bool IsNullOrEmpty( this string str )
		{
			return String.IsNullOrWhiteSpace( str );
		}

		public static string Escape( this string source, string str )
		{
			string result = source.Replace( str, @"\" + str );
			return result;
		}

		public static string Escape( this string source, params string[] chars )
		{
			string result = source;
			foreach ( var c in chars )
			{
				result = result.Replace( c, @"\" + c );
			}
			return result;
		}

		public static string EscapeRegexChars( this string source )
		{
			return Escape( source, @"\", "[", "]", "(", ")", ".", "+", "*", "?" );
		}
	}
}
