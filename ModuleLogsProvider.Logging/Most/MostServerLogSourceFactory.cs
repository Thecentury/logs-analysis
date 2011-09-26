using ModuleLogsProvider.Logging.Auxilliary;
using ModuleLogsProvider.Logging.MostLogsServices;
using System.ServiceModel;

namespace ModuleLogsProvider.Logging.Most
{
	/// <summary>
	/// Возвращает боевую реализацию сервиса ILogSourceService.
	/// </summary>
	public sealed class MostServerLogSourceFactory : ILogSourceServiceFactory
	{
		public IOptionalDisposable<ILogSourceService> CreateObject()
		{
			var binding = new BasicHttpBinding();
			var endpoint = new EndpointAddress( "http://127.0.0.1:9999/MostLogSourceService/" );
			var client = new LogSourceServiceClient( binding, endpoint );

			return new OptionalDisposable<ILogSourceService>( client );
		}
	}
}
