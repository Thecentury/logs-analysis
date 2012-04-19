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
			{"moduleordermanager", "flask"},
			{"controllerhandler", "compass"}
		};

		public static IDictionary<string, string> FileNameToIconMap
		{
			get { return fileNameToIconMap; }
		}

		public static string GetIcon( string logFileName )
		{
			string cleanName = LogFileNameCleaner.GetCleanedName( logFileName ).ToLower();

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
