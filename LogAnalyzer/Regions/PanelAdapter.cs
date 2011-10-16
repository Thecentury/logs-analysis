using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LogAnalyzer.GUI.Regions
{
	internal sealed class PanelAdapter : RegionAdapter
	{
		private Panel panel;

		public override void Init( DependencyObject regionHost )
		{
			panel = (Panel)regionHost;
		}

		public override void Add( object child )
		{
			UIElement uiElement = (UIElement)child;
			panel.Children.Add( uiElement );
		}
	}
}
