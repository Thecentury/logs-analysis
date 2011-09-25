using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace LogAnalyzer.Extensions
{
	public static class DispatcherExtensions
	{
		public static void BeginInvoke(this Dispatcher dispatcher, Action action, DispatcherPriority priority = DispatcherPriority.Background)
		{
			dispatcher.BeginInvoke((Delegate)action, priority);
		}

		public static void Invoke( this Dispatcher dispatcher, Action action, DispatcherPriority priority )
		{
			dispatcher.Invoke( (Delegate)action, priority );
		}
	}
}
