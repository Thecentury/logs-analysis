using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LogAnalyzer
{
	internal sealed class CollectionDebugView<T>
	{
		private ICollection<T> collection;
		[DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
		public T[] Items
		{
			get
			{
				T[] array = collection.ToArray();
				return array;
			}
		}
		public CollectionDebugView( ICollection<T> collection )
		{
			if ( collection == null )
				throw new ArgumentNullException( "collection" );

			this.collection = collection;
		}
	}
}
