using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Windows.Data;
using LogAnalyzer.Extensions;
using System.ComponentModel;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading;

namespace LogAnalyzer.GUI.ViewModel
{
	public sealed class CoreViewModel : LogEntriesListViewModel, IHierarchyMember<ApplicationViewModel, LogAnalyzerCore>
	{
		private readonly LogAnalyzerCore core = null;

		private readonly List<LogDirectoryViewModel> directories = null;

		public List<LogDirectoryViewModel> Directories
		{
			get { return directories; }
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
