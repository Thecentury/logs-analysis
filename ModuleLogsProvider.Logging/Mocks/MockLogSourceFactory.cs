
using System;
using ModuleLogsProvider.Logging.Auxilliary;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging.Mocks
{
	public sealed class MockLogSourceFactory : ILogSourceServiceFactory
	{
		private readonly MockLogsSourceService service;

		public MockLogSourceFactory( MockLogsSourceService service )
		{
			if ( service == null ) throw new ArgumentNullException( "service" );
			this.service = service;
		}

		public IOptionalDisposable<ILogSourceService> CreateObject()
		{
			return new OptionalDisposable<ILogSourceService>( service );
		}
	}
}
