using System;
using System.IO;
using System.ComponentModel;
using JetBrains.Annotations;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Kernel.Notifications
{
	public class LogNotificationsSourceBase : INotifyPropertyChanged
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
			if ( _isEnabled )
				return;

			IsEnabled = true;
			StartCore();
		}

		protected virtual void StartCore() { }

		public void Stop()
		{
			if ( !_isEnabled )
				return;

			IsEnabled = false;
			StopCore();
		}

		protected virtual void StopCore() { }

		private bool _isEnabled;
		public bool IsEnabled
		{
			get { return _isEnabled; }
			private set
			{
				_isEnabled = value;
				RaisePropertyChanged( "IsEnabled" );
			}
		}

		public void SetIsEnabled(bool value)
		{
			if ( value )
			{
				Start();
			}
			else
			{
				Stop();
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
			if ( !_isEnabled )
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

		protected void Raise( [NotNull] EventArgs e )
		{
			if ( e == null )
			{
				throw new ArgumentNullException( "e" );
			}

			if ( e is ErrorEventArgs )
			{
				RaiseError( (ErrorEventArgs)e );
			}
			else if ( e is RenamedEventArgs )
			{
				RaiseRenamed( (RenamedEventArgs)e );
			}
			else
			{
				FileSystemEventArgs fsArgs = e as FileSystemEventArgs;
				if ( fsArgs == null )
				{
					throw new InvalidOperationException( string.Format( "Unexpected EventArgs type '{0}'", e.GetType().Name ) );
				}

				switch ( fsArgs.ChangeType )
				{
					case WatcherChangeTypes.Created:
						RaiseCreated( fsArgs );
						break;
					case WatcherChangeTypes.Deleted:
						RaiseDeleted( fsArgs );
						break;
					case WatcherChangeTypes.Changed:
						RaiseChanged( fsArgs );
						break;
					case WatcherChangeTypes.Renamed:
					case WatcherChangeTypes.All:
					default:
						throw new ArgumentOutOfRangeException( string.Format( "Unexpected changeType '{0}'", fsArgs.ChangeType ) );
				}
			}
		}

		#endregion
	}
}
