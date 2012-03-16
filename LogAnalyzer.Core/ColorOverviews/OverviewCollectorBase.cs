using System.Collections.Generic;

namespace LogAnalyzer.ColorOverviews
{
	public abstract class OverviewCollectorBase<TItem, TResult> : IOverviewCollector<TItem, TResult>
	{
		private int _segmentsCount = 1000;
		public int SegmentsCount
		{
			get { return _segmentsCount; }
			set { _segmentsCount = value; }
		}

		public abstract TResult[] Build( IList<TItem> items );
	
		protected abstract void Append( TResult[] result, int index, TItem entry );
	}
}