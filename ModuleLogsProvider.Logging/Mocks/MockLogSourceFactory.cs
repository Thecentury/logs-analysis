using Awad.Eticket.ModuleLogsProvider.Types.Auxilliary;
using ILogSourceService = ModuleLogsProvider.Logging.MostLogsServices.ILogSourceService;

namespace ModuleLogsProvider.Logging.Mocks
{
	public sealed class MockLogSourceFactory : ILogSourceServiceFactory
	{
		#region IFactory<IOptionalDisposable<ILogSourceService>> Members

		public IOptionalDisposable<ILogSourceService> CreateObject()
		{
			return new OptionalDisposable<ILogSourceService>( new MockLogsSourceService() );
		}

		#endregion
	}
}
