using System;
using System.Collections.Generic;
using System.Linq;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.Kernel.Notifications;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class CoreViewModel : LogEntriesListViewModel, IHierarchyMember<ApplicationViewModel, LogAnalyzerCore>
	{
		private readonly LogAnalyzerCore _core;

		private readonly List<LogDirectoryViewModel> _directories;

		public List<LogDirectoryViewModel> Directories
		{
			get { return _directories; }
		}

		private readonly MessageSeverityCountViewModel _messageSeverityCount;
		public override MessageSeverityCountViewModel MessageSeverityCount
		{
			get { return _messageSeverityCount; }
		}

		public CoreViewModel( LogAnalyzerCore core, ApplicationViewModel applicationViewModel )
			: base( applicationViewModel )
		{
			if ( core == null )
			{
				throw new ArgumentNullException( "core" );
			}
			if ( !core.IsLoaded )
			{
				throw new InvalidOperationException( "Core should be loaded by this moment." );
			}
			if ( applicationViewModel == null )
			{
				throw new ArgumentNullException( "applicationViewModel" );
			}

			this._core = core;

			this._directories = core.Directories.Select( d => new LogDirectoryViewModel( d, this ) ).ToList();

			Init( core.MergedEntries );

			_messageSeverityCount = new MessageSeverityCountViewModel( core.MessageSeverityCount );
		}

		protected internal override LogFileViewModel GetFileViewModel( LogEntry logEntry )
		{
			LogFile logFile = logEntry.ParentLogFile;

			LogDirectory logDirectory = logFile.ParentDirectory;
			var directoryViewModel = _directories.First( d => d.LogDirectory == logDirectory );

			var fileViewModel = directoryViewModel.Files.First( f => f.LogFile == logFile );
			return fileViewModel;
		}

		public override LogEntriesListViewModel Clone()
		{
			CoreViewModel clone = new CoreViewModel( _core, ApplicationViewModel );
			return clone;
		}

		protected override void PopulateToolbarItems( IList<object> collection )
		{
			base.PopulateToolbarItems( collection );

			collection.Insert( 1,
				new ToggleButtonViewModel(
					() => _directories.Any( d => d.IsNotificationSourceEnabled ),
					value => _directories.ForEach( d => d.IsNotificationSourceEnabled = value ),
					"Toggle file updates notification",
					PackUriHelper.MakePackUri( "/Resources/control-record.png" ) ) );
		}

		protected override LogNotificationsSourceBase GetNotificationSource()
		{
			CompositeLogNotificationsSource composite = new CompositeLogNotificationsSource( _directories.Select( d => d.LogDirectory.NotificationsSource ) );
			return composite;
		}

		public override string Header
		{
			get { return "Main"; }
		}

		public override string IconFile
		{
			get { return MakePackUri( "/Resources/home.png" ); }
		}

		protected override bool CanBeClosedCore()
		{
			return false;
		}

		protected override void PopulateStatusBarItems( ICollection<object> collection )
		{
			base.PopulateStatusBarItems( collection );

#if DEBUG
			collection.Add( new MergedEntriesDebugStatusBarItem( _core.MergedEntries ) );
#endif
		}

		ApplicationViewModel IHierarchyMember<ApplicationViewModel, LogAnalyzerCore>.Parent
		{
			get { return ApplicationViewModel; }
		}

		LogAnalyzerCore IHierarchyMember<ApplicationViewModel, LogAnalyzerCore>.Data
		{
			get { return _core; }
		}
	}
}
