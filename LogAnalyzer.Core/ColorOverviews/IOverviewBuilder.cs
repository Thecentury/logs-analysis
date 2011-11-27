using System;
using System.Text;

namespace LogAnalyzer.ColorOverviews
{
	public interface IOverviewBuilder<in T, out TResult>
	{
		TResult[] CreateOverviewMap( T[] source );
	}
}
