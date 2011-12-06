using System.Collections.Generic;

namespace LogAnalyzer.ColorOverviews
{
	public abstract class OverviewCollectorBase<TItem, TResult> : IOverviewCollector<TItem, TResult>
	{
		private int segmentsCount = 1000;
		public int SegmentsCount
		{
			get { return segmentsCount; }
			protected set { segmentsCount = value; }
		}

		public abstract TResult[] Build( IList<TItem> items );

		protected abstract TResult InitResult();
		protected abstract void Append( TResult[] result, int index, TItem entry );
	}
}