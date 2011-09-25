using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awad.Eticket.ModuleLogsProvider.Types.Auxilliary;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging
{
	public interface ILogSourceServiceFactory : IOptionalDisposablesFactory<ILogSourceService>
	{
	}
}
