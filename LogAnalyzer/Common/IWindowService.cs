using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows7.DesktopIntegration;

namespace LogAnalyzer.GUI.Common
{
	public interface IWindowService
	{
		IntPtr GetWindowHandle();
		//void FlashUntillFocused(IntPtr hwnd);
		//void FlashTimes(IntPtr hwnd, int flashTimes);

		void SetProgressState( Windows7Taskbar.ThumbnailProgressState progressState );
		void SetProgressValue( int value );
	}
}
