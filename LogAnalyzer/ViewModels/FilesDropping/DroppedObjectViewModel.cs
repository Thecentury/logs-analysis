using System.Collections.Generic;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels.Helpers;

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

		// todo brinchuk is this neccessary?
		public abstract bool CanBeRemoved { get; }

		public abstract void AcceptVisitor( IDroppedObjectVisitor visitor );

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
}