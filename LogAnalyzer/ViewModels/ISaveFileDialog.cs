using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace LogAnalyzer.GUI.ViewModels
{
	public interface ISaveFileDialog
	{
		bool? ShowDialog();
		string FileName { get; }
	}

	internal sealed class UISaveFileDialog : ISaveFileDialog
	{
		private readonly SaveFileDialog _dialog;
		public UISaveFileDialog()
		{
			_dialog = new SaveFileDialog { AddExtension = true, DefaultExt = ".log" };
		}

		public bool? ShowDialog()
		{
			bool? result = _dialog.ShowDialog();
			return result;
		}

		public string FileName
		{
			get { return _dialog.FileName; }
		}
	}
}
