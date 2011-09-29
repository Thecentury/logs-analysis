using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace LogAnalyzer.GUI.ViewModel
{
	internal static class UITreeHelper
	{
		public static IEnumerable<DependencyObject> GetParents( this DependencyObject visual )
		{
			DependencyObject current = visual;
			DependencyObject parent = null;
			do
			{
				parent = VisualTreeHelper.GetParent( current );

				if ( parent != null )
				{
					yield return parent;
				}
				current = parent;
			}
			while ( parent != null );
		}
	}
}

