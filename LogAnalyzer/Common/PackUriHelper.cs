using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.Common
{
	internal static class PackUriHelper
	{
		public static string MakePackUri( string uri )
		{
			string normalizedUri = uri;
			if ( !normalizedUri.StartsWith( "/" ) )
			{
				normalizedUri = "/" + normalizedUri;
			}

			return "pack://application:,,,/LogAnalyzer.GUI;component" + normalizedUri;
		}
	}
}
