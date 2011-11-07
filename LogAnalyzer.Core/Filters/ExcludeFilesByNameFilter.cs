using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Markup;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	[IgnoreBuilder]
	[ContentProperty( "FileNames" )]
	public sealed class ExcludeFilesByNameFilter : ExpressionBuilder
	{
		private readonly List<string> fileNames = new List<string>();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public List<string> FileNames
		{
			get { return fileNames; }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type != typeof( IFileInfo ) )
				throw new NotSupportedException( "ExcludeFilesByNameFilter is for IFileInfo only." );

			return
				Expression.Not(
					Expression.Call(
						Expression.Constant( fileNames ),
						typeof( List<string> ).GetMethod( "Contains" ),
						Expression.Property(
							parameterExpression, "Name" )
						)
					);
		}
	}
}
