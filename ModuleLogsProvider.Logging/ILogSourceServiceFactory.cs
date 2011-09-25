using ModuleLogsProvider.Logging.Auxilliary;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging
{
	public interface ILogSourceServiceFactory : IOptionalDisposablesFactory<ILogSourceService>
	{
	}
}
