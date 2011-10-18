using System;
using System.Collections.Generic;
using System.Linq;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class CoreViewModel : LogEntriesListViewModel, IHierarchyMember<ApplicationViewModel, LogAnalyzerCore>
	{
		private readonly LogAnalyzerCore core;

		private readonly List<LogDirectoryViewModel> directories;

		public List<LogDirectoryViewModel> Directories
		{
			get { return directories; }
		}

		public override MessageSeverityCount MessageSeverityCount
		{
			get { return core.MessageSeverityCount; }
		}

		public CoreViewModel( LogAnalyzerCore core, ApplicationViewModel applicationViewModel )
			: base( applicationViewModel )
		{
			if ( core == null )
				throw new ArgumentNullException( "core" );
			if ( !core.IsLoaded )
				throw new InvalidOperationException( "Core should be loaded by this moment." );
			if ( applicationViewModel == null )
				throw new ArgumentNullException( "applicationViewModel" );

			this.core = core;

			this.directories = core.Directories.Select( d => new LogDirectoryViewModel( d, this ) ).ToList();

			Init( core.MergedEntries );
		}

		// todo не нужно ли оптимизировать поиск?
		protected internal override LogFileViewModel GetFileViewModel( LogEntry logEntry )
		{
			LogFile logFile = logEntry.ParentLogFile;

			LogDirectory logDirectory = logFile.ParentDirectory;
			var directoryViewModel = directories.First( d => d.LogDirectory == logDirectory );

			var fileViewModel = directoryViewModel.Files.First( f => f.LogFile == logFile );
			return fileViewModel;
		}

		public override LogEntriesListViewModel Clone()
		{
			CoreViewModel clone = new CoreViewModel( core, ApplicationViewModel );
			return clone;
		}

		public override string Header
		{
			get
			{
				return "Main";
			}
		}

		public override string IconFile
		{
			get
			{
				return MakePackUri( "/Resources/home.png" );
			}
		}

		protected override bool CanBeClosedCore()
		{
			return false;
		}

		ApplicationViewModel IHierarchyMember<ApplicationViewModel, LogAnalyzerCore>.Parent
		{
			get { return ApplicationViewModel; }
		}

		LogAnalyzerCore IHierarchyMember<ApplicationViewModel, LogAnalyzerCore>.Data
		{
			get { return core; }
		}
	}
}
