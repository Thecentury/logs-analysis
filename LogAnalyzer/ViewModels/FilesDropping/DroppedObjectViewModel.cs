using System.Collections.Generic;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels.Helpers;

namespace LogAnalyzer.GUI.ViewModels.FilesDropping
{
	public abstract class DroppedObjectViewModel : BindingObject
	{
		private IReportReadProgress _reporter;
		private readonly ICollection<DroppedObjectViewModel> _parentCollection;

		protected DroppedObjectViewModel()
			: this( null )
		{
		}

		protected DroppedObjectViewModel( [CanBeNull] ICollection<DroppedObjectViewModel> parentCollection )
		{
			this._parentCollection = parentCollection;
		}

		protected void InitReadReporter( IReportReadProgress readReporter )
		{
			this._reporter = readReporter;
			_reporter.ReadProgress += OnReadProgress;
		}

		protected virtual void OnReadProgress( object sender, FileReadEventArgs e )
		{
			_bytesReadTotal += e.BytesReadSincePreviousCall;
			ReadingProcessPercents = _bytesReadTotal * 100.0 / Length;
		}

		public override void Dispose()
		{
			if ( _reporter != null )
			{
				_reporter.ReadProgress -= OnReadProgress;
			}
			base.Dispose();
		}

		protected abstract long Length { get; }

		public abstract string Name { get; }

		private int _bytesReadTotal;
		private double _readingProgressPercents;

		public double ReadingProcessPercents
		{
			get { return _readingProgressPercents; }
			private set
			{
				_readingProgressPercents = value;
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

		public virtual bool IsDirectory
		{
			get { return false; }
		}

		public abstract void AcceptVisitor( IDroppedObjectVisitor visitor );

		#region Commands

		private DelegateCommand _removeFileCommand;
		public ICommand RemoveFileCommand
		{
			get
			{
				if ( _removeFileCommand == null )
					_removeFileCommand = new DelegateCommand( RemoveFileCommandExecute, RemoveFileCommandCanExecute );
				return _removeFileCommand;
			}
		}

		private void RemoveFileCommandExecute()
		{
			Dispose();
			_parentCollection.Remove( this );
		}

		private bool RemoveFileCommandCanExecute()
		{
			return _parentCollection != null;
		}

		#endregion
	}
}