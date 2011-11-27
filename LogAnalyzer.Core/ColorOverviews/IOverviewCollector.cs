using System.Collections.Generic;

namespace LogAnalyzer.ColorOverviews
{
	public interface IOverviewCollector<TItem, out TResult>
	{
		TResult[] Build( IList<TItem> items );
	}
}