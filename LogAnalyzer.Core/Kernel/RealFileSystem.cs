using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Kernel
{
	public sealed class RealFileSystem : IFileSystem
	{
		public bool FileExists( string path )
		{
			return File.Exists( path );
		}

		public bool DirectoryExists( string path )
		{
			return Directory.Exists( path );
		}
	}
}
