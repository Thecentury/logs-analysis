using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Filters
{
	public sealed class DelegateFilter<T> : IFilter<T>
	{
		private readonly Func<T, bool> _predicate;
		public DelegateFilter( Func<T, bool> predicate )
		{
			if ( predicate == null )
			{
				throw new ArgumentNullException( "predicate" );
			}

			this._predicate = predicate;
		}

		public bool Include( T argument )
		{
			bool include = _predicate( argument );
			return include;
		}

		public event EventHandler Changed;
	}
}
