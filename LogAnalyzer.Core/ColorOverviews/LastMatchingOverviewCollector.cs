using System;
using JetBrains.Annotations;

namespace LogAnalyzer.ColorOverviews
{
	public sealed class LastMatchingOverviewBuilder<T> : TimeOverviewCollectorBase<T, T>
		where T : class, IHaveTime
	{
		private readonly Func<T, bool> _predicate;

		public LastMatchingOverviewBuilder( [NotNull] Func<T, bool> predicate )
		{
			if ( predicate == null ) throw new ArgumentNullException( "predicate" );
			this._predicate = predicate;
		}

		protected override T InitResult()
		{
			return null;
		}

		protected override void Append(TimeProxy<T>[] result, int index, T entry)
		{
			if ( _predicate( entry ) )
			{
				result[index].Item = entry;
			}
		}
	}
}