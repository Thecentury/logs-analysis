using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Caching
{
	public sealed class CacheManager
	{
		private readonly string cachePath;
		private readonly DirectoryInfo cacheDirectory;
		private readonly Regex fileNameRegex;

		private CacheManager( string cachePath )
		{
			if ( String.IsNullOrWhiteSpace( cachePath ) )
				throw new ArgumentNullException( "cachePath" );

			if ( !Directory.Exists( cachePath ) )
			{
				cacheDirectory = Directory.CreateDirectory( cachePath );
			}
			else
			{
				cacheDirectory = new DirectoryInfo( cachePath );
			}

			this.cachePath = cachePath;

			// todo extract this regex into resources
			this.fileNameRegex = new Regex( @"^(?<Date>\d{4}-\d{2}-\d{2})-(?<LogName>.+)$", RegexOptions.Compiled );
		}

		public IFileInfo CreateFile( string filePath )
		{
			const string dateFormat = "yyyy-MM-dd";

			string fileName = Path.GetFileNameWithoutExtension( filePath );
			string logName = fileName;
			DateTime loggingDate = DateTime.Now.Date;

			// fileName contains date?
			if ( fileNameRegex.IsMatch( fileName ) )
			{
				Match match = fileNameRegex.Match( fileName );
				string dateStr = match.Groups["Date"].Value;
				loggingDate = DateTime.ParseExact( dateStr, dateFormat, CultureInfo.InvariantCulture );

				logName = match.Groups["LogName"].Value;
			}

			string date = loggingDate.ToString( dateFormat, CultureInfo.InvariantCulture );
			string cacheDir = Path.Combine( cachePath, date );
			if ( !Directory.Exists( cacheDir ) )
			{
				Directory.CreateDirectory( cacheDir );
			}

			string cacheFilePath = Path.Combine( cacheDir, logName + ".log" );

			FileInfo cacheFile = new FileInfo( cacheFilePath );
			FileInfo remoteFile = new FileInfo( filePath );

			// todo should we remember once created files?
			CacheFileInfo fileInfo = new CacheFileInfo( remoteFile, cacheFile, loggingDate );
			return fileInfo;
		}

		internal static CacheManager ForDirectory( LogDirectoryConfigurationInfo directory )
		{
			if ( directory == null )
				throw new ArgumentNullException( "directory" );

			string appData = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );

			string escapedDirName = EscapePath( directory.Path );

			string cacheDir = Path.Combine( appData, "AWAD.LogAnalyzer", "Cache", escapedDirName );

			CacheManager manager = new CacheManager( cacheDir );

			return manager;
		}

		private static string EscapePath( string cacheDirName )
		{
			char[] invalidCharacters = Path.GetInvalidFileNameChars();

			foreach ( char c in invalidCharacters )
			{
				cacheDirName = cacheDirName.Replace( c, '_' ).Replace( "__", "_" );
			}

			return cacheDirName;
		}
	}
}
