using System;
using ModuleLogsProvider.Logging.Most;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging.Mocks
{
	public sealed class MockLogSourceFactory : IServiceFactory<ILogSourceService>
	{
		private readonly MockLogsSourceService service;

		public MockLogSourceFactory( MockLogsSourceService service )
		{
			if ( service == null ) throw new ArgumentNullException( "service" );
			this.service = service;
		}

		public IDisposableService<ILogSourceService> Create()
		{
			return new MockDisposableService<ILogSourceService>( service );
		}
	}

	internal sealed class MockDisposableService<T> : IDisposableService<T>
	{
		private readonly T service;

		public MockDisposableService( T service )
		{
			this.service = service;
		}

		public void Dispose()
		{
			// do nothing here
		}

		public T Service
		{
			get { return service; }
		}

		public bool IsExpectedException( Exception exc )
		{
			return false;
		}
	}
}
