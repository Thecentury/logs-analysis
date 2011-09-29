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
			string address = "http://127.0.0.1:9999/MostLogSourceService/";
			//string address = "http://localhost:8732/Design_Time_Addresses/DevelopmentLogAnalysysServer/Service1/";

			var binding = new BasicHttpBinding();
			var endpoint = new EndpointAddress( address );
			var client = new LogSourceServiceClient( binding, endpoint );

			return new OptionalDisposable<ILogSourceService>( client );
		}
	}
}
