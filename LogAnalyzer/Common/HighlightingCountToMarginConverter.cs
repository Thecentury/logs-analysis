using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Research.DynamicDataDisplay.Converters;

namespace LogAnalyzer.GUI.Common
{
	internal sealed class HighlightingCountToMarginConverter : GenericValueConverter<int>
	{
		private double _offset = 3.0;
		public double Offset
		{
			get { return _offset; }
			set { _offset = value; }
		}

		public override object ConvertCore( int value, Type targetType, object parameter, CultureInfo culture )
		{
			Thickness result = new Thickness( value * _offset );
			return result;
		}
	}
}
