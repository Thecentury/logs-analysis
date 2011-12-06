using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class MessageTypeBitmapBuilder : OverviewBitmapBuilderBase<string>
	{
		// ReSharper disable FieldCanBeMadeReadOnly.Local
		private Color errorColor = Colors.Red;
		private Color warningColor = Colors.Orange;
		private Color infoColor = Colors.White;// Colors.LightGreen;
		private Color debugColor = Colors.White;//Colors.DarkGray;
		private Color verboseColor = Colors.White;//Colors.RoyalBlue;
		private Color otherColor = Colors.White;//Colors.PowderBlue;
		// ReSharper restore FieldCanBeMadeReadOnly.Local

		protected override Color GetColor( string item )
		{
			switch ( item )
			{
				case MessageTypes.Error:
					return errorColor;
				case MessageTypes.Warning:
					return warningColor;
				case MessageTypes.Info:
					return infoColor;
				case MessageTypes.Debug:
					return debugColor;
				case MessageTypes.Verbose:
					return verboseColor;
				default:
					return otherColor;
			}
		}
	}
}
