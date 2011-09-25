using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows7.DesktopIntegration;

namespace LogAnalyzer.GUI
{
	internal static class TaskbarHelper
	{
		public static void SetProgressState( Windows7Taskbar.ThumbnailProgressState progressState )
		{
			IntPtr hwnd = FlashWindow.GetHwnd();
			Windows7Taskbar.SetProgressState( hwnd, progressState );
		}

		public static void SetProgressValue( int value )
		{
			IntPtr hwnd = FlashWindow.GetHwnd();
			Windows7Taskbar.SetProgressValue( hwnd, (ulong)value, 100 );
		}
	}
}
