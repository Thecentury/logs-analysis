using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.Windows.Controls;

namespace LogAnalyzer.GUI.Common
{
	internal static class ColorHelper
	{
		private static readonly ObservableCollection<ColorItem> sortedColors;
		public static ObservableCollection<ColorItem> SortedColors
		{
			get { return sortedColors; }
		}

		static ColorHelper()
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

		private static readonly Random rnd = new Random();

		public static Color GetRandomColor()
		{
			double hue = rnd.NextDouble() * 360;
			double saturation = rnd.NextDouble() * 0.2 + 0.8;
			HsbColor color = new HsbColor( hue, saturation, 1 );
			return color.ToArgbColor();
		}
	}
}
