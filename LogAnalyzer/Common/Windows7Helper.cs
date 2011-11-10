using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Windows7.DesktopIntegration;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.GUI.Common
{
	public static class Windows7Helper
	{
		#region ProgressState attached property

		public static Windows7Taskbar.ThumbnailProgressState GetProgressState( DependencyObject obj )
		{
			return (Windows7Taskbar.ThumbnailProgressState)obj.GetValue( ProgressStateProperty );
		}

		public static void SetProgressState( DependencyObject obj, Windows7Taskbar.ThumbnailProgressState value )
		{
			obj.SetValue( ProgressStateProperty, value );
		}

		public static readonly DependencyProperty ProgressStateProperty = DependencyProperty.RegisterAttached(
		  "ProgressState",
		  typeof( Windows7Taskbar.ThumbnailProgressState ),
		  typeof( Windows7Helper ),
		  new FrameworkPropertyMetadata( Windows7Taskbar.ThumbnailProgressState.NoProgress, OnProgressStateChanged ) );

		private static void OnProgressStateChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
		{
			Window window = (Window)sender;
			IntPtr handle = GetHandle( window );
			if ( handle == IntPtr.Zero )
				return;

			Windows7Taskbar.ThumbnailProgressState state = (Windows7Taskbar.ThumbnailProgressState)e.NewValue;
			Windows7Taskbar.SetProgressState( handle, state );
		}

		private static IntPtr GetHandle( Window window )
		{
			IntPtr handle = GetHwnd( window );
			if ( handle != IntPtr.Zero )
				return handle;

			var application = Application.Current;
			if ( application == null )
				return IntPtr.Zero;

			Application.Current.Dispatcher.Invoke( () =>
			{
				handle = new WindowInteropHelper( Application.Current.MainWindow ).Handle;
				SetHwnd( window, handle );
			}, DispatcherPriority.Send );

			return handle;
		}

		#endregion

		#region ProgressValue attached property

		public static int GetProgressValue( DependencyObject obj )
		{
			return (int)obj.GetValue( ProgressValueProperty );
		}

		public static void SetProgressValue( DependencyObject obj, int value )
		{
			obj.SetValue( ProgressValueProperty, value );
		}

		public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.RegisterAttached(
		  "ProgressValue",
		  typeof( int ),
		  typeof( Windows7Helper ),
		  new FrameworkPropertyMetadata( 0, OnProgressValueChanged ) );

		private static void OnProgressValueChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			Window window = (Window)d;
			IntPtr handle = GetHandle( window );
			if ( handle == IntPtr.Zero )
				return;

			int progress = (int)e.NewValue;
			Windows7Taskbar.SetProgressValue( handle, (ulong)progress, 100 );
		}

		#endregion

		#region Hwnd attached property

		public static IntPtr GetHwnd( DependencyObject obj )
		{
			return (IntPtr)obj.GetValue( HwndProperty );
		}

		public static void SetHwnd( DependencyObject obj, IntPtr value )
		{
			obj.SetValue( HwndProperty, value );
		}

		public static readonly DependencyProperty HwndProperty = DependencyProperty.RegisterAttached(
		  "Hwnd",
		  typeof( IntPtr ),
		  typeof( Windows7Helper ),
		  new FrameworkPropertyMetadata( IntPtr.Zero ) );

		#endregion
	}
}
