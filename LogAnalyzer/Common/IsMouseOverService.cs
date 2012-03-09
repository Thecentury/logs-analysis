using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.GUI.Common
{
	public static class IsMouseOverService
	{
		public static bool GetIsMouseOver( DependencyObject obj )
		{
			return (bool)obj.GetValue( IsMouseOverProperty );
		}

		public static void SetIsMouseOver( DependencyObject obj, bool value )
		{
			obj.SetValue( IsMouseOverProperty, value );
		}

		public static readonly DependencyProperty IsMouseOverProperty = DependencyProperty.RegisterAttached(
		  "IsMouseOver",
		  typeof( bool ),
		  typeof( IsMouseOverService ),
		  new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.Inherits ) );
	}
}
