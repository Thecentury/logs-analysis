using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace LogAnalyzer.GUI.Regions
{
	internal sealed class ItemsControlAdapter : GenericRegionAdapter<ItemsControl>
	{
		public override void Add( object child )
		{
			Host.Items.Add( child );
		}
	}
}
