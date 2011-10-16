using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LogAnalyzer.GUI.Regions
{
	public sealed class RegionAdapterFactory
	{
		private readonly Dictionary<Type, Func<RegionAdapter>> typeToCreateRegionAdapterMapping =
			new Dictionary<Type, Func<RegionAdapter>>();

		private RegionAdapterFactory()
		{
			RegisterAdapter<Panel>( () => new PanelAdapter() );
		}

		private static readonly RegionAdapterFactory instance = new RegionAdapterFactory();
		public static RegionAdapterFactory Instance
		{
			get { return instance; }
		}

		public void RegisterAdapter<THost>( Func<RegionAdapter> createAdapterFunc )
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

			RegionAdapter adapter = createAdapterFunc();
			adapter.Init( host );

			return adapter;
		}
	}
}
