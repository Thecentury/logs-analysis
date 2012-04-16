using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Filters
{
	public sealed class ExpressionFilter<T> : IFilter<T>
	{
		private ExpressionBuilder _expressionBuilder = new AlwaysTrue();
		public ExpressionBuilder ExpressionBuilder
		{
			get { return _expressionBuilder; }
			set
			{
				if ( _expressionBuilder == null )
					throw new ArgumentNullException();

				_expressionBuilder.PropertyChanged -= OnExpressionBuilderPropertyChanged;
				_expressionBuilder = value;
				_expressionBuilder.PropertyChanged += OnExpressionBuilderPropertyChanged;

				Recompile();
			}
		}

		public ExpressionFilter()
		{
			Recompile();
		}

		private void Recompile()
		{
			Func<T, bool> temp;
			if ( _expressionBuilder.TryCompileToFilter( out temp ) )
			{
				_filter = temp;
				Changed.Raise( this );
			}
		}

		private void OnExpressionBuilderPropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			Recompile();
		}

		private Func<T, bool> _filter;
		public bool Include( T entity )
		{
			bool include = _filter( entity );
			return include;
		}

		public event EventHandler Changed;
	}
}
