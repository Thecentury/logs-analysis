namespace LogAnalyzer.ColorOverviews
{
	public abstract class OverviewBuilderBase<T> : IOverviewBuilder<T>
	{
		public double[] CreateOverviewMap( T[] source )
		{
			double[] map = new double[source.Length];

			for ( int i = 0; i < map.Length; i++ )
			{
				double value = GetValue( source[i] );
				map[i] = value;
			}

			return map;
		}

		protected abstract double GetValue( T item );
	}
}