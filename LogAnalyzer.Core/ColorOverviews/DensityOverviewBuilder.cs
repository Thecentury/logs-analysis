using System.Collections.Generic;
using System.Linq;

namespace LogAnalyzer.ColorOverviews
{
	public sealed class DensityOverviewBuilder<T> : IOverviewBuilder<ICollection<T>, double>
	{
		public double[] CreateOverviewMap( ICollection<T>[] source )
		{
			double[] map = new double[source.Length];
			if ( source.Length == 0 )
			{
				return map;
			}

			const double minCount = 0;
			double maxCount = source.Select( coll => coll.Count ).Max();

			for ( int i = 0; i < map.Length; i++ )
			{
				int count = source[i].Count;
				double ratio = (count - minCount) / (maxCount - minCount);
				map[i] = ratio;
			}

			return map;
		}
	}

	public sealed class AbsoluteCountOverviewBuilder<T> : IOverviewBuilder<ICollection<T>, double>
	{
		public double[] CreateOverviewMap( ICollection<T>[] source )
		{
			double[] map = new double[source.Length];
			if ( source.Length == 0 )
			{
				return map;
			}

			for ( int i = 0; i < map.Length; i++ )
			{
				int count = source[i].Count;
				map[i] = count;
			}

			return map;
		}
	}
}