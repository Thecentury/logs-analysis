using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.GUI.Regions
{
	public sealed class RegionAdapterFactory
	{
		private readonly Dictionary<Type, Func<DependencyObject, RegionAdapter>> typeToCreateRegionAdapterMapping =
			new Dictionary<Type, Func<DependencyObject, RegionAdapter>>();

		private RegionAdapterFactory() { }

		private static readonly RegionAdapterFactory instance = new RegionAdapterFactory();
		public static RegionAdapterFactory Instance
		{
			get { return instance; }
		}

		public void RegisterAdapter<THost>( Func<DependencyObject, RegionAdapter> createAdapterFunc )
		{
			if ( createAdapterFunc == null ) throw new ArgumentNullException( "createAdapterFunc" );

			typeToCreateRegionAdapterMapping.Add( typeof( THost ), createAdapterFunc );
		}

		public RegionAdapter CreateAdapter( DependencyObject host )
		{
			if ( host == null ) throw new ArgumentNullException( "host" );

			Type hostType = host.GetType();
			Type regionHostBaseType = typeToCreateRegionAdapterMapping.Keys.Single( type => type.IsAssignableFrom( hostType ) );
			var createAdapterFunc = typeToCreateRegionAdapterMapping[regionHostBaseType];
			RegionAdapter adapter = createAdapterFunc( host );
			return adapter;
		}
	}
}
