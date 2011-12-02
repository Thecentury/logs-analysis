using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels.Helpers;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.GUI.ViewModels.FilesDropping
{
	public abstract class DroppedObjectViewModel : BindingObject
	{
		private IReportReadProgress reporter;
		private readonly ICollection<DroppedObjectViewModel> parentCollection;

		protected DroppedObjectViewModel()
			: this( null )
		{
		}

		protected DroppedObjectViewModel( [CanBeNull] ICollection<DroppedObjectViewModel> parentCollection )
		{
			this.parentCollection = parentCollection;
		}

		protected void InitReadReporter( IReportReadProgress readReporter )
		{
			this.reporter = readReporter;
			reporter.ReadProgress += OnReadProgress;
		}

		protected virtual void OnReadProgress( object sender, FileReadEventArgs e )
		{
			bytesReadTotal += e.BytesReadSincePreviousCall;
			ReadingProcessPercents = bytesReadTotal * 100.0 / Length;
		}

		public override void Dispose()
		{
			if ( reporter != null )
			{
				reporter.ReadProgress -= OnReadProgress;
			}
			base.Dispose();
		}

		protected abstract long Length { get; }

		public abstract string Name { get; }

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
				string result = FileSizeHelper.GetFormattedLength( Length );
				return result;
			}
		}

		public abstract string Icon { get; }

		public abstract bool CanBeRemoved { get; }

		#region Commands

		private DelegateCommand removeFileCommand;
		public ICommand RemoveFileCommand
		{
			get
			{
				if ( removeFileCommand == null )
					removeFileCommand = new DelegateCommand( RemoveFileCommandExecute, RemoveFileCommandCanExecute );
				return removeFileCommand;
			}
		}

		private void RemoveFileCommandExecute()
		{
			Dispose();
			parentCollection.Remove( this );
		}

		private bool RemoveFileCommandCanExecute()
		{
			return parentCollection != null;
		}

		#endregion
	}

	public sealed class DroppedDirectoryViewModel : DroppedObjectViewModel
	{
		private readonly LogDirectory directory;
		private IDirectoryInfo dirInfo;

		public DroppedDirectoryViewModel( [NotNull] LogDirectory directory, [NotNull] IDirectoryInfo dirInfo, [NotNull] ICollection<DroppedObjectViewModel> parentCollection )
			: base( parentCollection )
		{
			if ( directory == null ) throw new ArgumentNullException( "directory" );
			if ( dirInfo == null ) throw new ArgumentNullException( "dirInfo" );

			this.directory = directory;
			this.dirInfo = dirInfo;
		}

		protected override long Length
		{
			get { return directory.TotalLengthInBytes; }
		}

		public override string Name
		{
			get { return directory.Path; }
		}

		public override string Icon
		{
			get { return PackUriHelper.MakePackUri( "/Resources/folder-horizontal.png" ); }
		}

		public override bool CanBeRemoved
		{
			get { return true; }
		}
	}

	public sealed class DroppedFileViewModel : DroppedObjectViewModel
	{
		private readonly IFileInfo fileInfo;
		private readonly string fileName;
		private readonly LogFile file;

		public DroppedFileViewModel( [NotNull] IFileInfo fileInfo, [NotNull] LogDirectory directory, [NotNull] string fileName,
			[NotNull] ICollection<DroppedObjectViewModel> parentCollection )
			: base( parentCollection )
		{
			if ( fileInfo == null ) throw new ArgumentNullException( "fileInfo" );
			if ( directory == null ) throw new ArgumentNullException( "directory" );
			if ( fileName == null ) throw new ArgumentNullException( "fileName" );
			if ( parentCollection == null ) throw new ArgumentNullException( "parentCollection" );

			this.fileInfo = fileInfo;
			this.fileName = fileName;

			file = new LogFile( fileInfo, directory );
			directory.Files.Add( file );
			InitReadReporter( file );
		}

		public override string Name
		{
			get { return fileName; }
		}

		protected override long Length
		{
			get { return fileInfo.Length; }
		}

		public override string Icon
		{
			get
			{
				string name = Path.GetFileName( fileName );
				string icon = FileNameToIconHelper.GetIcon( name );
				return PackUriHelper.MakePackUri( String.Format( "/Resources/{0}.png", icon ) );
			}
		}

		public override bool CanBeRemoved
		{
			get { return true; }
		}
	}
}
