using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.GUI.Regions
{
	public sealed class RegionCollection
	{
		private readonly Dictionary<string, DependencyObject> regionHostsByName;

		internal RegionCollection( Dictionary<string, DependencyObject> regionHostsByName )
		{
			this.regionHostsByName = regionHostsByName;
		}

		public Region this[string regionName]
		{
			get
			{
				DependencyObject regionHost = regionHostsByName[regionName];
				throw new NotImplementedException();
			}
		}
	}
}
