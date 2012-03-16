using System;
using JetBrains.Annotations;

namespace LogAnalyzer.ColorOverviews
{
	public sealed class FirstMatchingOverviewCollector<T> : TimeOverviewCollectorBase<T, T>
		where T : class, IHaveTime
	{
		private readonly Func<T, bool> _predicate;

		public FirstMatchingOverviewCollector( [NotNull] Func<T, bool> predicate )
		{
			if ( predicate == null )
			{
				throw new ArgumentNullException( "predicate" );
			}
			this._predicate = predicate;
		}

		protected override T InitResult()
		{
			return null;
		}

		protected override void Append( TimeProxy<T>[] result, int index, T entry )
		{
			bool alreadySet = result[index].Item != null;
			if ( alreadySet )
			{
				return;
			}

			if ( _predicate( entry ) )
			{
				result[index].Item = entry;
			}
		}
	}
}