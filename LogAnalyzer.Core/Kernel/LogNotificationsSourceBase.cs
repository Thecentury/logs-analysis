using System.IO;
using System.ComponentModel;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Kernel
{
	public abstract class LogNotificationsSourceBase : INotifyPropertyChanged
	{
		public event FileSystemEventHandler Changed;
		public event FileSystemEventHandler Created;
		public event FileSystemEventHandler Deleted;
		public event ErrorEventHandler Error;
		public event RenamedEventHandler Renamed;

		protected FileSystemEventHandler ChangedHandler
		{
			get { return Changed; }
		}

		protected FileSystemEventHandler DeletedHandler
		{
			get { return Deleted; }
		}

		protected FileSystemEventHandler CreatedHandler
		{
			get { return Created; }
		}

		public void Start()
		{
			if ( isEnabled )
				return;

			IsEnabled = true;
			StartCore();
		}

		protected virtual void StartCore() { }

		public void Stop()
		{
			if ( !isEnabled )
				return;

			IsEnabled = false;
			StopCore();
		}

		protected virtual void StopCore() { }

		private bool isEnabled;
		public bool IsEnabled
		{
			get { return isEnabled; }
			private set
			{
				isEnabled = value;
				RaisePropertyChanged( "IsEnabled" );
			}
		}

		#region Event raising helpers

		protected void RaiseChanged( FileSystemEventArgs e )
		{
			RaiseFileSystemEvent( Changed, e );
		}

		protected void RaiseCreated( FileSystemEventArgs e )
		{
			RaiseFileSystemEvent( Created, e );
		}

		protected void RaiseDeleted( FileSystemEventArgs e )
		{
			RaiseFileSystemEvent( Deleted, e );
		}

		protected void RaiseFileSystemEvent( string fileName, string directoryName, WatcherChangeTypes changeType, FileSystemEventHandler handler )
		{
			if ( !isEnabled )
				return;

			if ( handler != null )
			{
				FileSystemEventArgs args = new FileSystemEventArgs( changeType, directoryName, fileName );
				handler( this, args );
			}
		}

		protected void RaiseFileSystemEvent( FileSystemEventHandler handler, FileSystemEventArgs e )
		{
			if ( handler != null )
			{
				handler( this, e );
			}
		}

		protected void RaiseRenamed( RenamedEventArgs e )
		{
			var handler = Renamed;
			if ( handler != null )
			{
				handler( this, e );
			}
		}

		protected void RaiseError( ErrorEventArgs e )
		{
			var handler = Error;
			if ( handler != null )
			{
				handler( this, e );
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void RaisePropertyChanged( string propertyName )
		{
			PropertyChanged.Raise( this, propertyName );
		}

		#endregion
	}
}
