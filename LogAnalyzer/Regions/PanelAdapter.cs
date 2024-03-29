﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LogAnalyzer.GUI.Regions
{
	internal sealed class PanelAdapter : GenericRegionAdapter<Panel>
	{
		public override void Add( object child )
		{
			UIElement uiElement = (UIElement)child;
			Host.Children.Add( uiElement );
		}
	}
}
