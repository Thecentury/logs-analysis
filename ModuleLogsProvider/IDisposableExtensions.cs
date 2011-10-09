using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awad.Eticket.ModuleLogsProvider
{
	internal static class IDisposableExtensions
	{
		public static void SafeDispose(this IDisposable disposable)
		{
			if ( disposable == null )
				return;

			disposable.Dispose();
		}
	}
}
