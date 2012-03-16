using System;
using System.Linq.Expressions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	[FilterTarget( typeof( IFileInfo ) )]
	public sealed class ExcludePreviousDaysFileFilter : ExpressionBuilder
	{
		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type != typeof( IFileInfo ) )
				throw new NotSupportedException( "ExcludePreviousDaysFileFilter is for IFileInfo only." );

			var filter = new Not( new StringStartsWith
			{
				Inner = new GetProperty( new Argument(), "Name" ),
				Substring = new StringConstant( DateTime.Now.Year.ToString() ),
				Comparison = StringComparison.InvariantCultureIgnoreCase
			} );
			return filter.CreateExpression( parameterExpression );
		}
	}
}
