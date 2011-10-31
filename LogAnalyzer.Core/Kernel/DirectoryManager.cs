using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using LogAnalyzer.Config;

namespace LogAnalyzer.Kernel
{
	public sealed class DirectoryManager : IDirectoryFactory
	{
		private readonly Collection<IDirectoryFactory> factories = new Collection<IDirectoryFactory>();
		public Collection<IDirectoryFactory> Factories
		{
			get { return factories; }
		}

		public void RegisterCommonFactories()
		{
			RegisterFactory( new DefaultDirectoryFactory() );
			RegisterFactory( new PredefinedFilesDirectoryFactory() );
		}

		public void RegisterFactory( [NotNull] IDirectoryFactory factory )
		{
			if ( factory == null ) throw new ArgumentNullException( "factory" );

			factories.Insert( 0, factory );
		}

		public IDirectoryInfo CreateDirectory( LogDirectoryConfigurationInfo config )
		{
			foreach ( var factory in factories )
			{
				var dir = factory.CreateDirectory( config );
				if ( dir != null )
					return dir;
			}

			throw new InvalidOperationException( "There were no factories able to create directory for this config." );
		}
	}
}
