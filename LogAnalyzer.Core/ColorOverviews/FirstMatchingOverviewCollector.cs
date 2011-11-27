using System;
using JetBrains.Annotations;

namespace LogAnalyzer.ColorOverviews
{
	public sealed class FirstMatchingOverviewCollector<T> : OverviewCollectorBase<T, T>
		where T : class, IHaveTime
	{
		private readonly Func<T, bool> predicate;

		public FirstMatchingOverviewCollector( [NotNull] Func<T, bool> predicate )
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
			bool alreadySet = result[index] != null;
			if ( alreadySet )
				return;

			if ( predicate( entry ) )
				result[index] = entry;
		}
	}
}