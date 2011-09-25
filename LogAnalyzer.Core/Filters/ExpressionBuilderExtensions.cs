using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xaml;
using System.IO;

namespace LogAnalyzer.Filters
{
	public static class ExpressionBuilderExtensions
	{
		public static IFilter<T> BuildFilter<T>( this ExpressionBuilder builder )
		{
			return FilterBuilder.BuildFilter<T>( p => builder.CreateExpression( p ) );
		}

		public static Type GetResultType<T>( this ExpressionBuilder builder )
		{
			ParameterExpression parameter = Expression.Parameter( typeof( T ) );

			return builder.GetResultType( parameter );
		}

		public static IFilter<LogEntry> BuildLogEntriesFilter( this ExpressionBuilder builder )
		{
			return FilterBuilder.BuildLogEntryFilter( p => builder.CreateExpression( p ) );
		}

		public static Type GetLogEntriesFilterResultType( this ExpressionBuilder builder )
		{
			return GetResultType<LogEntry>( builder );
		}

		public static Func<T, bool> Compile<T>( this ExpressionBuilder builder )
		{
			return FilterBuilder.Compile<T>( p => builder.CreateExpression( p ) );
		}

		public static bool TryCompile<T>( this ExpressionBuilder builder, out Func<T, bool> filter )
		{
			filter = null;
			try
			{
				filter = Compile<T>( builder );
				return true;
			}
			catch ( Exception )
			{
				// todo перехватывать не все исключения
				filter = null;
				return false;
			}
		}

		public static string ToExpressionString( this ExpressionBuilder builder )
		{
			ParameterExpression param = Expression.Parameter( typeof( LogEntry ), "LogEntry" );

			Expression expr = builder.CreateExpression( param );

			return expr.ToString();
		}

		public static string SerializeToXaml( this ExpressionBuilder builder )
		{
			string xaml = XamlServices.Save( builder );
			//string nl = Environment.NewLine;
			//xaml = xaml
			//    .Replace( ">", ">" + nl + '\t' )
			//    .Replace( @"</", nl + @"</" )
			//    .TrimEnd( '\r', '\n' );
			return xaml;
		}
	}
}
