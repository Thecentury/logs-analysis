using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using LogAnalyzer.Extensions;
using Windows7.DesktopIntegration;

namespace LogAnalyzer.GUI.Common
{
	public sealed class RealWindowService : IWindowService
	{
		private IntPtr hwnd;
		public IntPtr GetWindowHandle()
		{
			if ( hwnd != IntPtr.Zero )
				return hwnd;

			var application = Application.Current;
			if ( application == null )
				return hwnd;

			Application.Current.Dispatcher.Invoke( () =>
			{
				hwnd = new WindowInteropHelper( Application.Current.MainWindow ).Handle;
			}, DispatcherPriority.Send );

			return hwnd;
		}

		public void SetProgressState( Windows7Taskbar.ThumbnailProgressState progressState )
		{
			var handle = GetWindowHandle();
			if ( handle == IntPtr.Zero )
				return;

			Windows7Taskbar.SetProgressState( handle, progressState );
		}

		public void SetProgressValue( int progress )
		{
			var handle = GetWindowHandle();
			if ( handle == IntPtr.Zero )
				return;

			Windows7Taskbar.SetProgressValue( handle, (ulong)progress, 100 );
		}
	}
}
