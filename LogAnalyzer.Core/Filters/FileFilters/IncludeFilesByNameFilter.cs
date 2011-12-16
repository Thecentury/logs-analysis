// ReSharper disable CheckNamespace

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
		protected readonly IList<string> fileNames = new List<string>();

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public IList<string> FileNames
		{
			get { return fileNames; }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof ( bool );
		}
	}

	internal class MyList<T> : IList<T>
	{
		public IEnumerator<T> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add( T item )
		{
			
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains( T item )
		{
			return true;
			return false;
		}

		public void CopyTo( T[] array, int arrayIndex )
		{
			throw new NotImplementedException();
		}

		public bool Remove( T item )
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsReadOnly
		{
			get { throw new NotImplementedException(); }
		}

		public int IndexOf( T item )
		{
			throw new NotImplementedException();
		}

		public void Insert( int index, T item )
		{
			throw new NotImplementedException();
		}

		public void RemoveAt( int index )
		{
			throw new NotImplementedException();
		}

		public T this[ int index ]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}

	public sealed class IncludeFilesByNameFilter : FilesByNameFilterBase
	{
		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type != typeof( IFileInfo ) )
				throw new NotSupportedException( "ExcludeFilesByNameFilter is for IFileInfo only." );

			Expression<Func<IFileInfo, bool>> expTree = file => fileNames.Contains( file.Name );

			return expTree;

		}
	}
}