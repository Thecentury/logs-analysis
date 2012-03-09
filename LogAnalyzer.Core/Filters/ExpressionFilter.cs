﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Filters
{
	public sealed class ExpressionFilter<T> : IFilter<T>
	{
		private ExpressionBuilder expressionBuilder = new AlwaysTrue();
		public ExpressionBuilder ExpressionBuilder
		{
			get { return expressionBuilder; }
			set
			{
				if ( expressionBuilder == null )
					throw new ArgumentNullException();

				expressionBuilder.PropertyChanged -= OnExpressionBuilder_PropertyChanged;
				expressionBuilder = value;
				expressionBuilder.PropertyChanged += OnExpressionBuilder_PropertyChanged;

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
			if ( expressionBuilder.TryCompileToFilter( out temp ) )
			{
				filter = temp;
				Changed.Raise( this );
			}
		}

		private void OnExpressionBuilder_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			Recompile();
		}

		private Func<T, bool> filter;
		public bool Include( T entity )
		{
			bool include = filter( entity );
			return include;
		}

		public event EventHandler Changed;
	}
}
