using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Common.Palettes;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class PaletteOverviewBitmapBuilder : OverviewBitmapBuilderBase<double>
	{
		private readonly IPalette palette;

		public PaletteOverviewBitmapBuilder( [NotNull] IPalette palette )
		{
			if ( palette == null ) throw new ArgumentNullException( "palette" );
			this.palette = palette;
		}

		protected override Color GetColor( double item )
		{
			return palette.GetColor( item );
		}
	}
}
