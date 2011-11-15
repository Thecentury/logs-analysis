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
			return FilterBuilder.BuildFilter<T>( builder.CreateExpression );
		}

		public static Type GetResultType<T>( this ExpressionBuilder builder )
		{
			ParameterExpression parameter = Expression.Parameter( typeof( T ) );

			return builder.GetResultType( parameter );
		}

		public static IFilter<LogEntry> BuildLogEntriesFilter( this ExpressionBuilder builder )
		{
			return FilterBuilder.BuildLogEntryFilter( builder.CreateExpression );
		}

		public static Type GetLogEntriesFilterResultType( this ExpressionBuilder builder )
		{
			return GetResultType<LogEntry>( builder );
		}

		public static Func<T, bool> Compile<T>( this ExpressionBuilder builder )
		{
			return FilterBuilder.Compile<T>( builder.CreateExpression );
		}

		public static bool TryCompile<T>( this ExpressionBuilder builder, out Func<T, bool> filter )
		{
			filter = null;

			if ( !builder.ValidateProperties() )
			{
				return false;
			}

			try
			{
				filter = Compile<T>( builder );
				return true;
			}
			catch ( Exception )
			{
				// todo перехватывать не все исключения
				return false;
			}
		}

		public static string ToExpressionString( this ExpressionBuilder builder )
		{
			ParameterExpression param = Expression.Parameter( typeof( LogEntry ), "record" );

			Expression expr = builder.CreateExpression( param );

			var str = expr.ToString()
				.Replace( " AndAlso ", " & " )
				.Replace( " OrElse ", " | " );

			return str;
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
