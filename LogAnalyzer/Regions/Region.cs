using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.GUI.Regions
{
	public sealed class Region
	{
		private readonly DependencyObject regionHost;
		private readonly string name;
		private readonly RegionAdapter adapter;

		internal Region( DependencyObject regionHost, string name, RegionAdapter adapter )
		{
			if (regionHost == null) throw new ArgumentNullException("regionHost");
			if (name == null) throw new ArgumentNullException("name");
			if (adapter == null) throw new ArgumentNullException("adapter");

			this.regionHost = regionHost;
			this.name = name;
			this.adapter = adapter;
		}

		public string Name
		{
			get { return name; }
		}

		public DependencyObject RegionHost
		{
			get { return regionHost; }
		}

		public void Add( object child )
		{
			adapter.Add( child );
		}
	}
}
