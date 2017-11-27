using System;
using System.Globalization;
using System.Windows.Media;
using LogAnalyzer.GUI.Common;
using Microsoft.Research.DynamicDataDisplay.Converters;

namespace LogAnalyzer.GUI.OverviewGui
{
    public sealed class LogFileToBackgroundConverter : GenericValueConverter<LogEntry>
    {
        public override object ConvertCore(LogEntry value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = LogFileNameBrushesCache.Solid.GetBrush(CalculateHashCode(value.ParentLogFile.Name));
            return brush;
        }

        private static int CalculateHashCode(string s)
        {
            int hashCode = 0;

            for (var i = 0; i < s.Length; i++)
            {
                hashCode = hashCode * 397 ^ s[i];
            }

            return hashCode;
        }
    }
}