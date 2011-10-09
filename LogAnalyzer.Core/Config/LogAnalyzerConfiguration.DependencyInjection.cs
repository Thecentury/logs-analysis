using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;

namespace LogAnalyzer.Config
{
	public partial class LogAnalyzerConfiguration : IDependencyInjectionContainer
	{
		private void RegisterCommonDependencies()
		{
			RegisterInstance<OperationScheduler>( OperationScheduler.TaskScheduler );

			Dispatcher currentDispatcher = DispatcherHelper.GetDispatcher();

			IScheduler scheduler = new DispatcherScheduler( currentDispatcher );
			RegisterInstance<IScheduler>( scheduler );
		}

		private readonly Dictionary<Type, Func<object>> registeredMappings = new Dictionary<Type, Func<object>>();

		public LogAnalyzerConfiguration RegisterInstance<TContract>( object instance )
		{
			TContract contract = (TContract)instance;
			if ( contract == null )
				throw new ArgumentException();

			Register<TContract>( () => instance );

			return this;
		}

		public LogAnalyzerConfiguration Register<TContract>( Func<object> createImplementationFunc )
		{
			if ( createImplementationFunc == null ) throw new ArgumentNullException( "createImplementationFunc" );

			Type contractType = typeof( TContract );
			registeredMappings[contractType] = createImplementationFunc;

			return this;
		}

		public TContract Resolve<TContract>()
		{
			Type key = typeof( TContract );

			if ( !registeredMappings.ContainsKey( key ) )
			{
				throw new InvalidOperationException( String.Format( "Config is expected to contain registered type '{0}'", key.Name ) );
			}

			var func = registeredMappings[key];
			var implementationUntyped = func();
			TContract implementation = (TContract)implementationUntyped;
			return implementation;
		}

		public bool CanResolve<TContract>()
		{
			bool canResolve = registeredMappings.ContainsKey( typeof( TContract ) );
			return canResolve;
		}

		#region IDependencyInjectionContainer Members

		void IDependencyInjectionContainer.Register<T>( Func<object> createImplementationFunc )
		{
			Register<T>(createImplementationFunc);
		}

		#endregion
	}
}
