using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using ModuleLogsProvider.Logging.Auxilliary;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostServiceFactory<TService> : IServiceFactory<TService> where TService : class
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

		public IDisposableService<TService> Create()
		{
			var binding = bindingFactory.Create();

			return new ClientWrapper<TService>( new ClientImpl<TService>( binding, new EndpointAddress( serviceUri ) ) );
		}
	}

	public interface IDisposableService<out T> : IDisposable
	{
		T Service { get; }
		bool IsExpectedException( Exception exc );
	}

	internal sealed class ClientWrapper<T> : IDisposableService<T> where T : class
	{
		private readonly ClientImpl<T> client;

		public ClientWrapper( ClientImpl<T> client )
		{
			this.client = client;
		}

		public void Dispose()
		{
			if ( client.State == CommunicationState.Faulted )
				return;

			try
			{
				client.Close();
			}
			catch ( CommunicationObjectFaultedException exc ) { }
		}

		public T Service
		{
			get { return client.Service; }
		}

		public bool IsExpectedException( Exception exc )
		{
			bool isExpected = exc is CommunicationException;
			return isExpected;
		}
	}

	internal sealed class ClientImpl<TClient> : ClientBase<TClient> where TClient : class
	{
		public ClientImpl( Binding binding, EndpointAddress address )
			: base( binding, address )
		{
		}

		public TClient Service
		{
			get { return Channel; }
		}
	}
}
