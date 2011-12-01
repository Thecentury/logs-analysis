using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Kernel
{
	public interface IFileSystem
	{
		bool FileExists( string path );
		bool DirectoryExists( string path );
	}
}
