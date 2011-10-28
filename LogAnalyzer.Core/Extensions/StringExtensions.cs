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
	}
}
