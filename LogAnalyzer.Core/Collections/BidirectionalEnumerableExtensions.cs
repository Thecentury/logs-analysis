using System.Collections;
using System.Collections.Generic;

namespace LogAnalyzer.Collections
{
	public static class BidirectionalEnumerableExtensions
	{
		private sealed class ForwardEnumerable<T> : IEnumerable<T>
		{
			private readonly IBidirectionalEnumerable<T> _biEnumerable;

			public ForwardEnumerable( IBidirectionalEnumerable<T> biEnumerable )
			{
				_biEnumerable = biEnumerable;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return _biEnumerable.GetBidirectionalEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private sealed class BackwardEnumerable<T> : IEnumerable<T>
		{
			private readonly IBidirectionalEnumerable<T> _biEnumerable;

			public BackwardEnumerable( IBidirectionalEnumerable<T> biEnumerable )
			{
				_biEnumerable = biEnumerable;
			}

			private sealed class Enumerator : IEnumerator<T>
			{
				private readonly IBidirectionalEnumerator<T> _biEnumerator;

				public Enumerator( IBidirectionalEnumerator<T> biEnumerator )
				{
					_biEnumerator = biEnumerator;
				}

				public void Dispose()
				{
					_biEnumerator.Dispose();
				}

				public bool MoveNext()
				{
					bool result = _biEnumerator.MoveBack();
					return result;
				}

				public void Reset()
				{
					_biEnumerator.Reset();
				}

				public T Current
				{
					get { return _biEnumerator.Current; }
				}

				object IEnumerator.Current
				{
					get { return Current; }
				}
			}

			public IEnumerator<T> GetEnumerator()
			{
				return new Enumerator( _biEnumerable.GetBidirectionalEnumerator() );
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public static IEnumerable<T> ToForwardEnumerable<T>( this IBidirectionalEnumerable<T> biEnumerable )
		{
			return new ForwardEnumerable<T>( biEnumerable );
		}

		public static IEnumerable<T> ToBackwardEnumerable<T>( this IBidirectionalEnumerable<T> biEnumerable )
		{
			return new BackwardEnumerable<T>( biEnumerable );
		}
	}
}