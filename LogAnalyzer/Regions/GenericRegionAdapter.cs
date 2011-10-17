using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.GUI.Regions
{
	public abstract class GenericRegionAdapter<T> : RegionAdapter where T : DependencyObject
	{
		private T host;
		public T Host
		{
			get { return host; }
		}

		public sealed override void Init( DependencyObject regionHost )
		{
			if ( regionHost == null ) throw new ArgumentNullException( "regionHost" );

			host = (T)regionHost;
			InitCore( host );
		}

		protected virtual void InitCore( T regionHost )
		{
		}
	}
}
