using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using ModuleLogsProvider.Logging.Auxilliary;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostServiceFactory<TService> : IFactory<ClientImpl<TService>> where TService : class
	{
		private readonly IFactory<Binding> bindingFactory;
		private readonly string serviceUri;

		public MostServiceFactory( IFactory<Binding> bindingFactory, string serviceUri )
		{
			if ( bindingFactory == null ) throw new ArgumentNullException( "bindingFactory" );
			if ( serviceUri == null ) throw new ArgumentNullException( "serviceUri" );

			this.bindingFactory = bindingFactory;
			this.serviceUri = serviceUri;
		}

		public ClientImpl<TService> Create()
		{
			var binding = bindingFactory.Create();

			return new ClientImpl<TService>( binding, new EndpointAddress( serviceUri ) );
		}
	}

	public sealed class ClientImpl<TClient> : ClientBase<TClient> where TClient : class
	{
		public ClientImpl( Binding binding, EndpointAddress address ) : base( binding, address ) { }

		public TClient Service
		{
			get { return Channel; }
		}
	}
}
