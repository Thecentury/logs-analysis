namespace LogAnalyzer.ColorOverviews
{
	public sealed class DiscreteOverviewBuilder<T> : OverviewBuilderBase<T, double>
		where T : class
	{
		protected override double GetValue( T item )
		{
			if ( item == null )
				return 0;
			else
				return 1;
		}
	}
}