using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.ViewModels.FilesDropping
{
	public interface IDroppedObjectVisitor
	{
		void Visit( DroppedFileViewModel file );
		void Visit( DroppedDirectoryViewModel directory );
	}

	public static class DroppedObjectVisitorExtensions
	{
		public static void Visit( this IDroppedObjectVisitor visitor, dynamic obj )
		{
			visitor.Visit( obj );
		}
	}
}
