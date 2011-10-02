using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace LogAnalyzer.Extensions
{
	public static class DispatcherHelper
	{
		public static Dispatcher GetDispatcher()
		{
			Dispatcher result = Application.Current != null ? Application.Current.Dispatcher : Dispatcher.CurrentDispatcher;

			return result;
		}
	}
}
