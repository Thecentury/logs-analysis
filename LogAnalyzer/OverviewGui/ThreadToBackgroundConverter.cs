using System;
using System.Globalization;
using System.Windows.Media;
using LogAnalyzer.GUI.Common;
using Microsoft.Research.DynamicDataDisplay.Converters;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class ThreadToBackgroundConverter : GenericValueConverter<LogEntry>
	{
		public override object ConvertCore( LogEntry value, Type targetType, object parameter, CultureInfo culture )
		{
			int hashCode = value.ThreadId.ToString().GetHashCode();

			Brush brush = LogFileNameBrushesCache.Solid.GetBrush( hashCode );
			return brush;
		}
	}
}