using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Markup;

namespace LogAnalyzer.GUI.ViewModels.Colorizing
{
	[ContentProperty( "Template" )]
	public abstract class ColorizeTemplateBase : ISupportInitialize
	{
		public virtual void BeginInit() { }

		public virtual void EndInit() { }

		public double Priority { get; set; }

		public ControlTemplate Template { get; set; }

		public abstract bool Accepts( LogEntry logEntry );

		public abstract object GetDataContext( LogEntry logEntry );
	}
}