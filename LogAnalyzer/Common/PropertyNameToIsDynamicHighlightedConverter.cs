using System;
using System.Windows.Data;
using System.Globalization;

namespace LogAnalyzer.GUI.Common
{
	public sealed class PropertyNameToIsDynamicHighlightedConverter : IMultiValueConverter
	{
		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
		{
			string dataPropertyName = (string)values[0];
			string selfPropertyName = (string)values[1];

			bool isDynamicHighlighted = dataPropertyName != null && dataPropertyName == selfPropertyName;
			return isDynamicHighlighted;
		}

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
		{
			throw new NotSupportedException();
		}
	}
}
