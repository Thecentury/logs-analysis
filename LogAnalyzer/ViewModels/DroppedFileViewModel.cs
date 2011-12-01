using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels.Helpers;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class DroppedFileViewModel : BindingObject
	{
		private readonly IFileInfo fileInfo;
		private readonly string fileName;
		private readonly ICollection<DroppedFileViewModel> parentCollection;
		private readonly LogFile file;

		public DroppedFileViewModel( [NotNull] IFileInfo fileInfo, [NotNull] LogDirectory directory, [NotNull] string fileName,
			[NotNull] ICollection<DroppedFileViewModel> parentCollection )
		{
			if ( fileInfo == null ) throw new ArgumentNullException( "fileInfo" );
			if ( directory == null ) throw new ArgumentNullException( "directory" );
			if ( fileName == null ) throw new ArgumentNullException( "fileName" );
			if ( parentCollection == null ) throw new ArgumentNullException( "parentCollection" );

			this.fileInfo = fileInfo;
			this.fileName = fileName;
			this.parentCollection = parentCollection;

			file = new LogFile( fileInfo, directory );
			file.ReadProgress += OnFileReadProgress;
		}

		private void OnFileReadProgress( object sender, FileReadEventArgs e )
		{
			bytesReadTotal += e.BytesReadSincePreviousCall;
			ReadingProcessPercents = bytesReadTotal * 100.0 / file.FileInfo.Length;
		}

		public string FileName
		{
			get { return fileName; }
		}

		private int bytesReadTotal;
		private double readingProgressPercents;
		public double ReadingProcessPercents
		{
			get { return readingProgressPercents; }
			private set
			{
				readingProgressPercents = value;
				RaisePropertyChanged( "ReadingProcessPercents" );
			}
		}

		public string LengthString
		{
			get
			{
				long length = fileInfo.Length;
				string result = FileSizeHelper.GetFormattedLength( length );
				return result;
			}
		}

		#region Commands

		private DelegateCommand removeFileCommand;
		public ICommand RemoveFileCommand
		{
			get
			{
				if ( removeFileCommand == null )
					removeFileCommand = new DelegateCommand( RemoveFileCommandExecute );
				return removeFileCommand;
			}
		}

		private void RemoveFileCommandExecute()
		{
			parentCollection.Remove( this );
		}

		#endregion
	}
}
