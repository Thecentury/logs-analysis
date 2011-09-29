using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using AdTech.Common.WPF;
using System.Windows.Input;

namespace LogAnalyzer.GUI.ViewModel
{
	public class ExceptionLineViewModel : MessageLineViewModel
	{
		private readonly FileLineInfo exceptionLine = null;

		public ExceptionLineViewModel(FileLineInfo exceptionLine, string textLine)
			: base(textLine)
		{
			if (exceptionLine == null)
				throw new ArgumentNullException("exceptionLine");

			this.exceptionLine = exceptionLine;
		}

		#region Properties

		public string FileName { get { return exceptionLine.FileName; } }
		public string MethodName { get { return exceptionLine.MethodName; } }
		public int LineNumber { get { return exceptionLine.LineNumber; } }

		#endregion

		private DelegateCommand<RoutedEventArgs> openFileInVSCommand = null;
		public ICommand OpenFileInVSCommand
		{
			get
			{
				// todo implement me
				if (openFileInVSCommand == null)
				{

				}
				return openFileInVSCommand;
			}
		}
	}
}
