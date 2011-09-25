using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModuleLogsProvider.Logging
{
	public interface IOptionalDisposablesFactory<out T> : IFactory<IOptionalDisposable<T>>
	{
	}
}
