using LogAnalyzer.Filters;

namespace LogAnalyzer.GUI.ViewModels.Colorizing
{
	public abstract class FilteringColorizeTemplate : ColorizeTemplateBase
	{
		public ExpressionBuilder Filter
		{
			get { return filter.ExpressionBuilder; }
			set { filter.ExpressionBuilder = value; }
		}

		private readonly ExpressionFilter<LogEntry> filter = new ExpressionFilter<LogEntry>();

		public override bool Accepts( LogEntry logEntry )
		{
			return filter.Include( logEntry );
		}
	}
}