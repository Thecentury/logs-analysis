using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.GUI.ViewModels.FilesDropping
{
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

			InitReadReporter( directory );
		}

		protected override long Length
		{
			get { return LogDirectory.TotalLengthInBytes; }
		}

		public override string Name
		{
			get { return LogDirectory.Path; }
		}

		public override string Icon
		{
			get { return PackUriHelper.MakePackUri( "/Resources/folder-horizontal.png" ); }
		}

		public override bool CanBeRemoved
		{
			get { return true; }
		}

		public override void AcceptVisitor( IDroppedObjectVisitor visitor )
		{
			visitor.Visit( this );
		}

		public LogDirectory LogDirectory
		{
			get { return directory; }
		}
	}
}