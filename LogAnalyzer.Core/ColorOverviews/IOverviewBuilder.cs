using System;
using System.Text;

namespace LogAnalyzer.ColorOverviews
{
	public interface IOverviewBuilder<in T>
	{
		double[] CreateOverviewMap( T[] source );
	}
}
