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
		private readonly Dictionary<string, FutureRegion> createdRegions;

		internal RegionCollection( Dictionary<string, DependencyObject> regionHostsByName, Dictionary<string, FutureRegion> createdRegions )
		{
			if ( regionHostsByName == null ) throw new ArgumentNullException( "regionHostsByName" );
			if ( createdRegions == null ) throw new ArgumentNullException( "createdRegions" );

			this.regionHostsByName = regionHostsByName;
			this.createdRegions = createdRegions;
		}

		public IRegion this[string regionName]
		{
			get
			{
				if ( regionHostsByName.ContainsKey( regionName ) )
				{
					DependencyObject regionHost = regionHostsByName[regionName];
					var region = RegionManager.GetRegion( regionHost );

					return region;
				}
				else
				{
					FutureRegion futureRegion;
					if ( !createdRegions.TryGetValue( regionName, out futureRegion ) )
					{
						futureRegion = new FutureRegion(regionName);
						createdRegions.Add( regionName, futureRegion );
					}
					return futureRegion;
				}
			}
		}
	}
}
