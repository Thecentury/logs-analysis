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
		private readonly LogDateTimeFilterBase filter;
		public LogDateTimeViewModel( LogDateTimeFilterBase filter, ParameterExpression parameter )
			: base( filter, parameter )
		{
			this.filter = filter;
		}

		public string Time
		{
			get { return filter.StringValue; }
			set { filter.StringValue = value; }
		}
	}
}
