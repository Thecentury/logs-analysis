using System.Windows.Controls;

namespace LogAnalyzer.GUI.ViewModels.Colorizing
{
	public sealed class DefaultColorizeTemplate : ColorizeTemplateBase
	{
		public override bool Accepts( LogEntry logEntry )
		{
			return true;
		}

		public override object GetDataContext( LogEntry logEntry )
		{
			return new DefaultColorizeInfo { Template = Template, Text = logEntry.UnitedText };
		}

		private sealed class DefaultColorizeInfo
		{
			public string Text { get; set; }
			public ControlTemplate Template { get; set; }
		}
	}
}