using ModuleLogsProvider.Logging.Auxilliary;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging.Most
{
	/// <summary>
	/// Возвращает боевую реализацию сервиса ILogSourceService.
	/// </summary>
	public sealed class MostServerLogSourceFactory : ILogSourceServiceFactory
	{
		public IOptionalDisposable<ILogSourceService> CreateObject()
		{
			return new OptionalDisposable<ILogSourceService>( new LogSourceServiceClient() );
		}
	}
}
