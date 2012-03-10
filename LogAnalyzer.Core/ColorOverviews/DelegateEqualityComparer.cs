using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace LogAnalyzer.ColorOverviews
{
	internal sealed class DelegateEqualityComparer<T, TSelector> : IEqualityComparer<T>
	{
		private readonly Func<T, TSelector> _selector;

		public DelegateEqualityComparer( [NotNull] Func<T, TSelector> selector )
		{
			if ( selector == null ) throw new ArgumentNullException( "selector" );
			this._selector = selector;
		}

		public bool Equals( T x, T y )
		{
			var fieldX = _selector( x );
			var fieldY = _selector( y );

			bool equals = fieldX.Equals( fieldY );
			return equals;
		}

		public int GetHashCode( T obj )
		{
			var field = _selector( obj );
			int hashCode = field.GetHashCode();
			return hashCode;
		}
	}
}