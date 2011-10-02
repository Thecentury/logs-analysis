using System;
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
		const string DesignTimeAddress = "http://127.0.0.1:9999/MostLogSourceService/";
		private readonly string address;

		public MostServerLogSourceFactory() : this( DesignTimeAddress ) { }

		public MostServerLogSourceFactory( string address )
		{
			if ( String.IsNullOrEmpty( address ) ) throw new ArgumentNullException( "address" );
			this.address = address;
		}

		public IOptionalDisposable<ILogSourceService> CreateObject()
		{
			//string address = "http://localhost:8732/Design_Time_Addresses/LogAnalysisServer.Dev/LogServer/";

			const int size = Int32.MaxValue;

			var binding = new BasicHttpBinding
							{
								MaxBufferPoolSize = size,
								MaxBufferSize = size,
								MaxReceivedMessageSize = size
							};

			var endpoint = new EndpointAddress( address );
			var client = new LogSourceServiceClient( binding, endpoint );

			return new OptionalDisposable<ILogSourceService>( client );
		}
	}
}
