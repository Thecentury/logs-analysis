using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using System.Windows.Threading;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Properties;
using Windows7.DesktopIntegration;

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

			JumpList.AddToRecentCategory( projectFile );
		}

		public static void RemoveProjectFromRecent(string projectFile)
		{
			var recentProjects = Settings.Default.LatestProjectPaths;
			if ( recentProjects != null )
			{
				recentProjects.Remove( projectFile );
			}

			Settings.Default.Save();
		}
	}
}
