using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Config
{
	public partial class LogAnalyzerConfiguration
	{
		private readonly Dictionary<Type, Func<object>> registeredMappings = new Dictionary<Type, Func<object>>();

		public void RegisterInstance<TContract>( TContract instance )
		{
			Register<TContract>( () => instance );
		}

		public void Register<TContract>( Func<object> createImplementationFunc )
		{
			Type contractType = typeof( TContract );
			registeredMappings[contractType] = createImplementationFunc;
		}

		public TContract Resolve<TContract>()
		{
			var func = registeredMappings[typeof( TContract )];
			var implementationUntyped = func();
			TContract implementation = (TContract)implementationUntyped;
			return implementation;
		}

		public TContract ResolveNotNull<TContract>()
		{
			var resolvedValue = Resolve<TContract>();
			if ( resolvedValue == null )
				throw new ArgumentException( String.Format( "Resolved value of type {0} should not be null.", typeof( T ) ) );

			return resolvedValue;
		}

		public bool CanResolve<TContract>()
		{
			bool canResolve = registeredMappings.ContainsKey( typeof( TContract ) );
			return canResolve;
		}
	}
}
