using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogAnalyzer.GUI.ViewModels.Helpers
{
	public static class FileNameToIconHelper
	{
		private static readonly Dictionary<string, string> fileNameToIconMap = new Dictionary<string, string>
		{
		    {"security", "lock"},
			{"pagebuilder-eticket_actual", "box-document"},
			{"cacheproxy", "server-cast"},
			{"schedule", "alarm-clock-blue"},
			{"modulenotification", "at-sign"},
			{"muscat", "database"},
			{"kernel", "leaf"},
			{"corporatormanager", "briefcase"},
			{"usermanager", "users"},
			{"moduleantifraud", "user-thief-baldie"},
			{"archive", "box"},
			{"moduleavia", "paper-plane"},
			{"moduleordermanager", "flask"}
		};

		public static IDictionary<string, string> FileNameToIconMap
		{
			get { return fileNameToIconMap; }
		}

		private static readonly Regex startsWithDigitsRegex = new Regex( @"^\d{4}-\d{2}-\d{2}-(?<name>.*)", RegexOptions.Compiled );

		public static string GetIcon( string logFileName )
		{
			string cleanName = logFileName;
			Match match = startsWithDigitsRegex.Match( logFileName );
			if ( match.Success )
			{
				cleanName = match.Groups["name"].Value;
			}

			cleanName = cleanName.ToLower().Replace( ".log", "" );
			if ( fileNameToIconMap.ContainsKey( cleanName ) )
			{
				string icon = fileNameToIconMap[cleanName];
				return icon;
			}
			else
			{
				return "document-globe";
			}
		}
	}
}
