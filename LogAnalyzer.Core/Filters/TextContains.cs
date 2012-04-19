using System;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
	[ContentProperty( "Substring" )]
	public sealed class TextContains : ExpressionBuilder
	{
		[FilterParameter( typeof( string ), "Substring" )]
		public string Substring
		{
			get { return Get<string>( "Substring" ); }
			set { Set( "Substring", value ); }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			GetProperty getPropertyBuilder = new GetProperty { Target = new Argument(), PropertyName = "UnitedText" };
			StringContains containsBuilder = new StringContains { Inner = getPropertyBuilder, Substring = new StringConstant { Value = Substring } };

			return containsBuilder.CreateExpression( parameterExpression );
		}
	}
}