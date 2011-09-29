using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;

namespace LogAnalyzer.GUI.ViewModels
{
	/// <summary>
	/// Просто типизированный View.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class GenericListView<T> : ListCollectionView
	{
		public GenericListView( IList<T> list ) : base( (IList)list ) { }

		// todo probably override some methods or properties
	}
}
