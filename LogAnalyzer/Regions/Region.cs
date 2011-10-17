using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.GUI.Regions
{
	internal sealed class Region : IRegion
	{
		private readonly string name;
		private readonly RegionAdapter adapter;

		internal Region( DependencyObject regionHost, string name, RegionAdapter adapter )
		{
			if ( regionHost == null ) throw new ArgumentNullException( "regionHost" );
			if ( name == null ) throw new ArgumentNullException( "name" );
			if ( adapter == null ) throw new ArgumentNullException( "adapter" );

			this.name = name;
			this.adapter = adapter;
		}

		public string Name
		{
			get { return name; }
		}

		public void Add( object child )
		{
			adapter.Add( child );
		}
	}
}
