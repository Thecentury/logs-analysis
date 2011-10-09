using System;
using ModuleLogsProvider.Logging.Auxilliary;
using ModuleLogsProvider.Logging.MostLogsServices;
using System.ServiceModel;

namespace ModuleLogsProvider.Logging.Most
{
	/// <summary>
	/// Возвращает актуальную реализацию сервиса ILogSourceService.
	/// </summary>
	public sealed class MostServerLogSourceFactory : ILogSourceServiceFactory
	{
		const string DesignTimeAddress = "net.tcp://127.0.0.1:9999/MostLogSourceService/";
		private readonly string address;

		public MostServerLogSourceFactory() : this( DesignTimeAddress ) { }

		public MostServerLogSourceFactory( string address )
		{
			if ( String.IsNullOrEmpty( address ) ) throw new ArgumentNullException( "address" );
			this.address = address;
		}

		public IOptionalDisposable<ILogSourceService> Create()
		{
			var binding = new NetTcpBinding();

			var endpoint = new EndpointAddress( address );
			var client = new LogSourceServiceClient( binding, endpoint );

			return new OptionalDisposable<ILogSourceService>( client );
		}
	}
}
