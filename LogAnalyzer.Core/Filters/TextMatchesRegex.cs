using System;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
	[Icon( "regular-expression-search.png" )]
	[ContentProperty( "Pattern" )]
	public sealed class TextMatchesRegex : ExpressionBuilder
	{
		[FilterParameter( typeof( string ), "Pattern" )]
		public string Pattern
		{
			get { return Get<string>( "Pattern" ); }
			set { Set( "Pattern", value ); }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			GetProperty getPropertyBuilder = new GetProperty { Target = new Argument(), PropertyName = "UnitedText" };
			var regex = new RegexMatchesFilterBuilder( Pattern, getPropertyBuilder );

			return regex.CreateExpression( parameterExpression );
		}
	}
}