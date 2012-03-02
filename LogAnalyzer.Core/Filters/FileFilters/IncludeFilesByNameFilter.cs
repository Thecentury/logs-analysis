﻿// ReSharper disable CheckNamespace

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Markup;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
// ReSharper restore CheckNamespace
{
	[FilterTarget( typeof( IFileInfo ) )]
	[ContentProperty( "FileNames" )]
	public abstract class FilesByNameFilterBase : ExpressionBuilder
	{
		private readonly IList<string> fileNames = new List<string>();

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public IList<string> FileNames
		{
			get { return fileNames; }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}
	}

	public sealed class IncludeFilesByNameFilter : FilesByNameFilterBase
	{
		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type != typeof( IFileInfo ) )
				throw new NotSupportedException( "IncludeFilesByNameFilter is for IFileInfo only." );

			return Expression.Call(
				Expression.Constant( FileNames ), typeof( List<string> ).GetMethod( "Contains" ),
				Expression.Property( parameterExpression, "Name" ) );
		}
	}
}