using System;
using LogAnalyzer.Filters;
using System.Linq.Expressions;

namespace LogAnalyzer.GUI.FilterEditing
{
	[IgnoreBuilder]
	public sealed class TransparentBuilder : ExpressionBuilder
	{
		[FilterParameter( typeof( ExpressionBuilder ), "Inner" )]
		public ExpressionBuilder Inner
		{
			get { return GetExpressionBuilder( "Inner" ); }
			set { Set( "Inner", value ); }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			if ( Inner == null )
				return parameterExpression;
			else
				return Inner.CreateExpression( parameterExpression );
		}
	}
}
