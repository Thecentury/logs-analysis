namespace LogAnalyzer.ColorOverviews
{
	public abstract class OverviewBuilderBase<T, TResult> : IOverviewBuilder<T, TResult>
	{
		public TResult[] CreateOverviewMap( T[] source )
		{
			TResult[] map = new TResult[source.Length];

			for ( int i = 0; i < map.Length; i++ )
			{
				TResult value = GetValue( source[i] );
				map[i] = value;
			}

			return map;
		}

		protected abstract TResult GetValue( T item );
	}
}