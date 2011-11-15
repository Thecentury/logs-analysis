using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.Windows.Controls;

namespace LogAnalyzer.GUI.Common
{
	internal static class SortedColors
	{
		private static readonly ObservableCollection<ColorItem> sortedColors;
		public static ObservableCollection<ColorItem> Colors
		{
			get { return sortedColors; }
		}

		static SortedColors()
		{
			var colors = from property in typeof( Colors ).GetProperties()
						 where property.PropertyType == typeof( Color )
						 let name = property.Name
						 let color = (Color)property.GetValue( null, null )
						 let hsbColor = HsbColor.FromArgbColor( color )
						 orderby hsbColor.Hue
						 where hsbColor.Saturation > 0.2
						 select new ColorItem( color, name );

			sortedColors = new ObservableCollection<ColorItem>( colors );
		}
	}
}
