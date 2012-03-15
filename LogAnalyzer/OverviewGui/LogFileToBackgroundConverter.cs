using System;
using System.Globalization;
using System.Windows.Media;
using LogAnalyzer.GUI.Common;
using Microsoft.Research.DynamicDataDisplay.Converters;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class LogFileToBackgroundConverter : GenericValueConverter<LogEntry>
	{
		public override object ConvertCore( LogEntry value, Type targetType, object parameter, CultureInfo culture )
		{
			Brush brush = LogFileNameBrushesCache.GetBrush( value.ParentLogFile.Name );
			return brush;
		}
	}
}