using System;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
	[Icon( "folder-horizontal.png" )]
	[ContentProperty( "DirectoryName" )]
	public abstract class DirectoryNameEqualsFilterBase : ExpressionBuilder
	{
		[FilterParameter( typeof( string ), "DirectoryName" )]
		public string DirectoryName
		{
			get { return Get<string>( "DirectoryName" ); }
			set { Set( "DirectoryName", value ); }
		}

		public sealed override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected Expression CreateGetParentDirectoryDisplayNameExpression( ParameterExpression parameter )
		{
			return
				Expression.Property(
					Expression.Property(
						Expression.Property(
							parameter,
							"ParentLogFile" ),
						"ParentDirectory" ),
					"DisplayName" );
		}
	}

	public sealed class DirectoryNameEquals : DirectoryNameEqualsFilterBase
	{
		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return Expression.Equal(
				CreateGetParentDirectoryDisplayNameExpression( parameterExpression ),
				Expression.Constant( DirectoryName, typeof( string ) )
				);
		}
	}

	public sealed class DirectoryNameNotEquals : DirectoryNameEqualsFilterBase
	{
		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return
				Expression.Not(
					Expression.Equal(
						CreateGetParentDirectoryDisplayNameExpression( parameterExpression ),
						Expression.Constant( DirectoryName, typeof( string ) )
						) );
		}
	}
}