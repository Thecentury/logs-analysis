using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels.Helpers;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.GUI.ViewModels.FilesDropping
{
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
			InitReadReporter( LogFile );
		}

		public override string Name
		{
			get { return fileName; }
		}

		public override long Length
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

		public LogFile LogFile
		{
			get { return file; }
		}

		public override void AcceptVisitor( IDroppedObjectVisitor visitor )
		{
			visitor.Visit( this );
		}
	}
}
