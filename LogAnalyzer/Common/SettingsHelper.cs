using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using LogAnalyzer.GUI.Properties;

namespace LogAnalyzer.GUI.Common
{
	public static class SettingsHelper
	{
		public static void AddProjectToRecent( string projectFile )
		{
			var projectPaths = Settings.Default.LatestProjectPaths;
			if ( projectPaths == null )
			{
				projectPaths = new StringCollection();
			}

			if ( projectPaths.Contains( projectFile ) )
			{
				projectPaths.Remove( projectFile );
			}

			projectPaths.Add( projectFile );

			Settings.Default.LatestProjectPaths = projectPaths;

			Settings.Default.Save();
		}
	}
}
