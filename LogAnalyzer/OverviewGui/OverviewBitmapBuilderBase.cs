using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Research.DynamicDataDisplay;


namespace LogAnalyzer.GUI.OverviewGui
{
	public abstract class OverviewBitmapBuilderBase<T> : IOverviewBitmapBuilder<T>
	{
		protected abstract Color GetColor( T item );

		public BitmapSource CreateBitmap( T[] map )
		{
			if ( map == null ) throw new ArgumentNullException( "map" );

			int width = Math.Max( map.Length, 1 );
			const int height = 1;

			int[] pixels = new int[width];
			for ( int i = 0; i < map.Length; i++ )
			{
				pixels[i] = GetColor( map[i] ).ToArgb();
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
