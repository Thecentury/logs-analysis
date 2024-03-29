﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Media;
using LogAnalyzer.GUI.Extensions;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Converters;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class MessageTypeToBackgroundConverter : GenericValueConverter<LogEntry>
	{
		public MessageTypeToBackgroundConverter()
		{
			Error = Brushes.Red;
			Warning = Brushes.Orange;
			Info = Brushes.LimeGreen.MakeTransparent( 0.5 ).AsFrozen();
			Debug = Brushes.RoyalBlue;
			Verbose = Brushes.BlueViolet.MakeTransparent( 0.4 ).AsFrozen();
			Other = Brushes.LightGray;
		}

		public Brush Error { get; set; }
		public Brush Warning { get; set; }
		public Brush Info { get; set; }
		public Brush Debug { get; set; }
		public Brush Verbose { get; set; }
		public Brush Other { get; set; }

		public override object ConvertCore( LogEntry value, Type targetType, object parameter, CultureInfo culture )
		{
			switch ( value.Type )
			{
				case MessageTypes.Error:
					return Error;
				case MessageTypes.Warning:
					return Warning;
				case MessageTypes.Info:
					return Info;
				case MessageTypes.Debug:
					return Debug;
				case MessageTypes.Verbose:
					return Verbose;
				default:
					return Other;
			}
		}
	}
}
