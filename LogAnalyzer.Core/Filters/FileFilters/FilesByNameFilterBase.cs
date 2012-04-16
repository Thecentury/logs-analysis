using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Markup;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	[FilterTarget( typeof( IFileInfo ) )]
	[ContentProperty( "FileNames" )]
	public abstract class FilesByNameFilterBase : ExpressionBuilder
	{
		protected FilesByNameFilterBase() { }

		protected FilesByNameFilterBase( params string[] fileNames )
		{
			_fileNames.AddRange( fileNames );
		}

		private readonly HashSet<string> _fileNames = new HashSet<string>( StringComparer.InvariantCultureIgnoreCase );

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public ICollection<string> FileNames
		{
			get { return _fileNames; }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}
	}
}