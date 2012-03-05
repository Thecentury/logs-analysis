using System.Collections;
using System.Collections.Generic;

namespace LogAnalyzer.Collections
{
	public static class BidirectionalEnumerable
	{
		public static IBidirectionalEnumerable<T> CreateForwardOnly<T>( IEnumerable<T> enumerable )
		{
			return new ForwardOnlyEnumerable<T>( enumerable );
		}

		private sealed class ForwardOnlyEnumerable<T> : IBidirectionalEnumerable<T>
		{
			private readonly IEnumerable<T> _enumerable;

			public ForwardOnlyEnumerable( IEnumerable<T> enumerable )
			{
				_enumerable = enumerable;
			}

			private sealed class Enumerator : IBidirectionalEnumerator<T>
			{
				private readonly IEnumerator<T> _enumerator;

				public Enumerator( IEnumerator<T> enumerator )
				{
					_enumerator = enumerator;
				}

				public void Dispose()
				{
					_enumerator.Dispose();
				}

				public bool MoveNext()
				{
					return _enumerator.MoveNext();
				}

				public void Reset()
				{
					_enumerator.Reset();
				}

				public T Current
				{
					get { return _enumerator.Current; }
				}

				public bool MoveBack()
				{
					return false;
				}

				object IEnumerator.Current
				{
					get { return Current; }
				}
			}

			public IBidirectionalEnumerator<T> GetEnumerator()
			{
				return new Enumerator( _enumerable.GetEnumerator() );
			}

			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}

	public interface IBidirectionalEnumerable<out T> : IEnumerable<T>
	{
		IBidirectionalEnumerator<T> GetEnumerator();
	}

	public interface IRandomAccessEnumerable<out T> : IBidirectionalEnumerable<T>
	{
		IRandomAccessEnumerator<T> GetEnumerator();
	}
}