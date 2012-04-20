using System;
using System.Linq.Expressions;
using System.Windows.Markup;
using LogAnalyzer.Extensions;

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
	}

	public sealed class DirectoryNameEquals : DirectoryNameEqualsFilterBase
	{
		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			string directoryName = DirectoryName;
			Expression<Func<LogEntry, bool>> expression =
				entry => entry.ParentLogFile.ParentDirectory.DisplayName == directoryName;

			return expression.ReplaceParameter( parameterExpression );
		}
	}

	public sealed class DirectoryNameNotEquals : DirectoryNameEqualsFilterBase
	{
		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			string directoryName = DirectoryName;
			Expression<Func<LogEntry, bool>> expression =
				entry => entry.ParentLogFile.ParentDirectory.DisplayName != directoryName;

			return expression.ReplaceParameter( parameterExpression );
		}
	}
}