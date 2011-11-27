using System;
using JetBrains.Annotations;

namespace LogAnalyzer.ColorOverviews
{
	public sealed class LastMatchingOverviewBuilder<T> : OverviewCollectorBase<T, T>
		where T : class, IHaveTime
	{
		private readonly Func<T, bool> predicate;

		public LastMatchingOverviewBuilder( [NotNull] Func<T, bool> predicate )
		{
			if ( predicate == null ) throw new ArgumentNullException( "predicate" );
			this.predicate = predicate;
		}

		protected override T InitResult()
		{
			return null;
		}

		protected override void Append( T[] result, int index, T entry )
		{
			if ( predicate( entry ) )
				result[index] = entry;
		}
	}
}