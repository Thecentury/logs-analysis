using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.GUI.Regions
{
	public sealed class RegionManager
	{
		private static readonly Dictionary<string, DependencyObject> regionsByName = new Dictionary<string, DependencyObject>();

		#region RegionName attached property

		public static string GetRegionName( DependencyObject obj )
		{
			return (string)obj.GetValue( RegionNameProperty );
		}

		public static void SetRegionName( DependencyObject obj, string value )
		{
			obj.SetValue( RegionNameProperty, value );
		}

		public static readonly DependencyProperty RegionNameProperty = DependencyProperty.RegisterAttached(
		  "RegionName",
		  typeof( string ),
		  typeof( RegionManager ),
		  new FrameworkPropertyMetadata( null, OnRegionNameChanged ) );

		private static void OnRegionNameChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			string regionName = (string)e.NewValue;
			regionsByName.Add( regionName, d );
		}

		#endregion

		#region RegionManager attached property

		public static RegionManager GetRegionManager( DependencyObject obj )
		{
			return (RegionManager)obj.GetValue( RegionManagerProperty );
		}

		public static void SetRegionManager( DependencyObject obj, RegionManager value )
		{
			obj.SetValue( RegionManagerProperty, value );
		}

		public static readonly DependencyProperty RegionManagerProperty = DependencyProperty.RegisterAttached(
		  "RegionManager",
		  typeof( RegionManager ),
		  typeof( RegionManager ),
		  new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits, OnRegionManagerChanged ) );

		private static void OnRegionManagerChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			string regionName = GetRegionName( d );
			if(String.IsNullOrEmpty(regionName))
				return;

			RegionManager regionManager = (RegionManager)e.NewValue;
			if ( regionManager != null )
			{
				Region region = regionManager.CreateRegion( d, regionName );
				SetRegion( d, region );
			}
		}

		#endregion

		#region Region attached property

		public static Region GetRegion( DependencyObject obj )
		{
			return (Region)obj.GetValue( RegionProperty );
		}

		public static void SetRegion( DependencyObject obj, Region value )
		{
			obj.SetValue( RegionProperty, value );
		}

		public static readonly DependencyProperty RegionProperty = DependencyProperty.RegisterAttached(
		  "Region",
		  typeof( Region ),
		  typeof( RegionManager ),
		  new FrameworkPropertyMetadata( null ) );

		#endregion

		private Region CreateRegion( DependencyObject regionHost, string regionName )
		{
			if ( regionHost == null ) throw new ArgumentNullException( "regionHost" );
			if ( regionName == null ) throw new ArgumentNullException( "regionName" );

			var adapterFactory = RegionAdapterFactory.Instance;
			var adapter = adapterFactory.CreateAdapter( regionHost );
			Region region = new Region( regionHost, regionName, adapter );

			return region;
		}
	}
}
