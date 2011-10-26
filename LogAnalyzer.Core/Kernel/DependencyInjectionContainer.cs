using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Kernel
{
	public sealed class DependencyInjectionContainer : IDependencyInjectionContainer
	{
		private static readonly DependencyInjectionContainer instance = new DependencyInjectionContainer();
		public static DependencyInjectionContainer Instance
		{
			get { return instance; }
		}

		private DependencyInjectionContainer() { }

		private readonly Dictionary<Type, Func<object>> registeredMappings = new Dictionary<Type, Func<object>>();

		public void Register<TContract>( Func<object> createImplementationFunc )
		{
			if ( createImplementationFunc == null ) throw new ArgumentNullException( "createImplementationFunc" );

			Type contractType = typeof( TContract );
			registeredMappings[contractType] = createImplementationFunc;
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
	}
}
