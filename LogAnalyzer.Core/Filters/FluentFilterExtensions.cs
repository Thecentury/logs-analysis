using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Filters
{
	public static class FluentFilterExtensions
	{
		public static ExpressionBuilder GetProperty( this ExpressionBuilder builder, string propertyName )
		{
			return new GetProperty( builder, propertyName );
		}

		public static ExpressionBuilder IsEqual( this ExpressionBuilder left, ExpressionBuilder right )
		{
			return new Equals( left, right );
		}
	}
}
