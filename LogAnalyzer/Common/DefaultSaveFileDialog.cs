using System;
using System.IO;
using System.Windows;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace LogAnalyzer.GUI.Common
{
	public sealed class DefaultSaveFileDialog : ISaveToStreamDialog
	{
		private readonly string _extension;

		public DefaultSaveFileDialog( [NotNull] string extension )
		{
			if ( extension == null )
			{
				throw new ArgumentNullException( "extension" );
			}
			_extension = extension;
		}

		public Stream ShowDialog()
		{
			SaveFileDialog dialog = new SaveFileDialog
										{
											AddExtension = true,
											DefaultExt = _extension,
											Filter = "LogAnalyzer Project|*.logproj"
										};

			bool? result = dialog.ShowDialog();

			if ( result != true )
			{
				return null;
			}

			return new FileStream( dialog.FileName, FileMode.Create, FileAccess.Write );
		}
	}
}