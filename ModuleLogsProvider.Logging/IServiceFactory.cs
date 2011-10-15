using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModuleLogsProvider.Logging.Auxilliary;
using ModuleLogsProvider.Logging.Most;

namespace ModuleLogsProvider.Logging
{
	public interface IServiceFactory<out TService> : IFactory<IDisposableService<TService>>
	{
	}
}
