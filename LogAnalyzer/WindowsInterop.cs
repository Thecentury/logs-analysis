using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LogAnalyzer.GUI
{
	internal static class WindowsInterop
	{
		public static void SelectInExplorer( string path )
		{
			// todo exception handling?
			Process.Start( "explorer", "/select," + path );
		}
	}
}
