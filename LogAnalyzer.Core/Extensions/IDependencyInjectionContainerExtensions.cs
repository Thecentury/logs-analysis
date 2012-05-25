using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Extensions
{
	public static class IDependencyInjectionContainerExtensions
	{
		public static void RegisterInstance<TContract>( this IDependencyInjectionContainer container, object instance ) where TContract : class
		{
			TContract contract = (TContract)instance;
			if ( contract == null )
			{
				throw new ArgumentException();
			}

			container.Register<TContract>( () => instance );
		}

		public static T ResolveNotNull<T>( this IDependencyInjectionContainer container )
		{
			var resolvedValue = container.Resolve<T>();
			if ( resolvedValue == null )
			{
				throw new ArgumentException( String.Format( "Resolved value of type {0} should not be null.", typeof( T ) ) );
			}

			return resolvedValue;
		}

	}
}
