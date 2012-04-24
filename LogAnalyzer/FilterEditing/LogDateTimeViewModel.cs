using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LogAnalyzer.Filters;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class LogDateTimeViewModel : ExpressionBuilderViewModel
	{
		private readonly LogDateTimeFilterBase _filter;
		public LogDateTimeViewModel( LogDateTimeFilterBase filter, ParameterExpression parameter )
			: base( filter, parameter )
		{
			this._filter = filter;
		}

		public string Time
		{
			get { return _filter.StringValue; }
			set { _filter.StringValue = value; }
		}
	}
}
