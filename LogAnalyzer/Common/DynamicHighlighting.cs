using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.GUI
{
	public static class DynamicHighlighting
	{
		public static string GetPropertyName( DependencyObject obj )
		{
			return (string)obj.GetValue( PropertyNameProperty );
		}

		public static void SetPropertyName( DependencyObject obj, string value )
		{
			obj.SetValue( PropertyNameProperty, value );
		}

		public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.RegisterAttached(
		  "PropertyName",
		  typeof( string ),
		  typeof( DynamicHighlighting ),
		  new FrameworkPropertyMetadata( null ) );


		public static bool GetIsHighlighted( DependencyObject obj )
		{
			return (bool)obj.GetValue( IsHighlightedProperty );
		}

		public static void SetIsHighlighted( DependencyObject obj, bool value )
		{
			obj.SetValue( IsHighlightedProperty, value );
		}

		public static readonly DependencyProperty IsHighlightedProperty = DependencyProperty.RegisterAttached(
		  "IsHighlighted",
		  typeof( bool ),
		  typeof( DynamicHighlighting ),
		  new FrameworkPropertyMetadata( false ) );
	}
}
