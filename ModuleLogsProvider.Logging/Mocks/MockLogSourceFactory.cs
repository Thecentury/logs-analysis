
using ModuleLogsProvider.Logging.Auxilliary;
using ModuleLogsProvider.Logging.MostLogsServices;

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
