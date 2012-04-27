using System;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
    [FilterTargetAttribute( typeof( LogEntry ) )]
    [Icon( "document-text.png" )]
    [ContentProperty( "FileName" )]
    public abstract class FileNameFilterBase : ExpressionBuilder
    {
        [FilterParameter( typeof( string ), "FileName" )]
        public string FileName
        {
            get { return Get<string>( "FileName" ); }
            set { Set( "FileName", value ); }
        }

        public sealed override Type GetResultType(ParameterExpression target)
        {
            return typeof( bool );
        }
    }

    public sealed class FileNameNotEquals : FileNameFilterBase
    {
        public FileNameNotEquals() { }
        public FileNameNotEquals(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException( "fileName" );

            FileName = fileName;
        }

        protected override Expression CreateExpressionCore(ParameterExpression parameterExpression)
        {
            return
                Expression.NotEqual(
                    Expression.Property(
                        Expression.Property( parameterExpression, "ParentLogFile" ),
                        "Name" ),
                    Expression.Constant( FileName, typeof( string ) )
                );
        }
    }

    public sealed class FileNameEquals : FileNameFilterBase
    {
        public FileNameEquals() { }
        public FileNameEquals(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException( "fileName" );

            FileName = fileName;
        }

        protected override Expression CreateExpressionCore(ParameterExpression parameterExpression)
        {
            return Expression.Equal(
                Expression.Property(
                    Expression.Property( parameterExpression, "ParentLogFile" ),
                    "Name" ),
                Expression.Constant( FileName, typeof( string ) )
                );
        }
    }
}