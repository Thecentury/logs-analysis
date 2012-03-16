using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	[FilterTarget( typeof( IFileInfo ) )]
	public sealed class SizeLessThanFilter : ExpressionBuilder
	{
		[FilterParameter( typeof( double ), "Megabytes" )]
		public double Megabytes
		{
			get { return Get<double>( "Megabytes" ); }
			set { Set( "Megabytes", value ); }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type != typeof( IFileInfo ) )
				throw new NotSupportedException( "PreviousDayFilter is for IFileInfo only." );

			var filter = new LessThan(
							new GetProperty( new Argument(), "Length" ),
							new LongConstant( (long)(Megabytes * 1024 * 1024) ) );

			return filter.CreateExpression( parameterExpression );
		}
	}
}
