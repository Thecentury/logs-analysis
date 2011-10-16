using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.GUI.Regions
{
	public abstract class RegionAdapter
	{
		public abstract void Init( DependencyObject regionHost );
		public abstract void Add( object child );
		//public abstract void Remove(object child);
	}
}
