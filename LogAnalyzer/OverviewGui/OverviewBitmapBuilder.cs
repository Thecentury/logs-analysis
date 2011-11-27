using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Common.Palettes;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class OverviewBitmapBuilder
	{
		public BitmapSource CreateBitmap( double[] map, IPalette palette )
		{
			int width = map.Length;
			const int height = 1;

			int[] pixels = new int[width];
			for ( int i = 0; i < map.Length; i++ )
			{
				var color = palette.GetColor( map[i] );
				pixels[i] = color.ToArgb();
			}

			WriteableBitmap bmp = new WriteableBitmap( width, height, 96, 96, PixelFormats.Pbgra32, null );
			bmp.WritePixels( new Int32Rect( 0, 0, width, height ), pixels, bmp.BackBufferStride, 0, 0 );
			bmp.Freeze();

			TransformedBitmap transformedBitmap = new TransformedBitmap( bmp, new RotateTransform( 90 ) );
			transformedBitmap.Freeze();

			return transformedBitmap;
		}
	}
}
