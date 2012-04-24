using System;
using System.Linq.Expressions;
using System.Windows.Markup;
using JetBrains.Annotations;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Filters
{
	[Icon( "folder-horizontal.png" )]
	[ContentProperty( "DirectoryName" )]
	public abstract class DirectoryNameEqualsFilterBase : ExpressionBuilder
	{
		protected DirectoryNameEqualsFilterBase() { }
		protected DirectoryNameEqualsFilterBase( [NotNull] string directoryDisplayName )
		{
			if ( directoryDisplayName == null )
			{
				throw new ArgumentNullException( "directoryDisplayName" );
			}
			DirectoryName = directoryDisplayName;
		}

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
		public DirectoryNameEquals() { }
		public DirectoryNameEquals( [NotNull] string directoryDisplayName ) : base( directoryDisplayName ) { }

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
		public DirectoryNameNotEquals() { }
		public DirectoryNameNotEquals( [NotNull] string directoryDisplayName ) : base( directoryDisplayName ) { }

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			string directoryName = DirectoryName;
			Expression<Func<LogEntry, bool>> expression =
				entry => entry.ParentLogFile.ParentDirectory.DisplayName != directoryName;

			return expression.ReplaceParameter( parameterExpression );
		}
	}
}