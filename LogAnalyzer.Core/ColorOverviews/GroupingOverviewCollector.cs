using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace LogAnalyzer.ColorOverviews
{
	public sealed class GroupingOverviewCollector<T> : OverviewCollectorBase<T, IList<T>>
		where T : IHaveTime
	{
		private readonly Func<T, bool> predicate;

		public GroupingOverviewCollector([NotNull] Func<T, bool> predicate)
		{
			if (predicate == null) throw new ArgumentNullException("predicate");
			this.predicate = predicate;
		}

		protected override IList<T> InitResult()
		{
			return new List<T>();
		}

		protected override void Append( IList<T>[] result, int index, T entry )
		{
			if ( predicate( entry ) )
			{
				var list = result[index];
				list.Add(entry);
			}
		}
	}
}
