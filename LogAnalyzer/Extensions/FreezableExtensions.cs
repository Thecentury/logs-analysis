using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.GUI.Extensions
{
	internal static class FreezableExtensions
	{
		public static T AsFrozen<T>( this T freezable ) where T : Freezable
		{
			freezable.Freeze();
			return freezable;
		}
	}
}
