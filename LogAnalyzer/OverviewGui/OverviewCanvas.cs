using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class OverviewCanvas : Canvas
	{
		protected override Size ArrangeOverride( Size arrangeSize )
		{
			foreach ( FrameworkElement child in InternalChildren )
			{
				double coordinate = GetCoordinate( child );
				double y = coordinate * ActualHeight;

				var desiredSize = child.DesiredSize;
				Rect bounds = new Rect( new Point( 0, y - desiredSize.Height / 2 ), desiredSize );
				child.Arrange( bounds );
			}

			return arrangeSize;
		}

		protected override void OnRenderSizeChanged( SizeChangedInfo sizeInfo )
		{
			InvalidateArrange();
		}

		#region Attached dependency properies

		public static double GetCoordinate( DependencyObject obj )
		{
			return (double)obj.GetValue( CoordinateProperty );
		}

		public static void SetCoordinate( DependencyObject obj, double value )
		{
			obj.SetValue( CoordinateProperty, value );
		}

		public static readonly DependencyProperty CoordinateProperty = DependencyProperty.RegisterAttached(
		  "Coordinate",
		  typeof( double ),
		  typeof( OverviewCanvas ),
		  new FrameworkPropertyMetadata( 0.0, FrameworkPropertyMetadataOptions.AffectsArrange ) );

		#endregion Attached dependency properies
	}
}
