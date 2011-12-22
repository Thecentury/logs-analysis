using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.Extensions
{
	internal static class StringExtensions
	{
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
	}
}
