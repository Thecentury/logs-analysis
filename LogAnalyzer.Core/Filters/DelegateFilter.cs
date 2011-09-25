using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Filters
{
	public sealed class DelegateFilter<T> : IFilter<T>
	{
		private readonly Func<T, bool> predicate = null;
		public DelegateFilter( Func<T, bool> predicate )
		{
			if ( predicate == null )
				throw new ArgumentNullException( "func" );

			this.predicate = predicate;
		}

		public bool Include( T argument )
		{
			bool include = predicate( argument );
			return include;
		}

		public event EventHandler Changed;
	}
}
