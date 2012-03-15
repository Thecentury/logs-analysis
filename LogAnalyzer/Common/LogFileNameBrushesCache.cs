using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;

namespace LogAnalyzer.GUI.Common
{
	public static class LogFileNameBrushesCache
	{
		private static readonly Dictionary<int, Brush> logFileNameBrushesCache = new Dictionary<int, Brush>();

		private static readonly ReaderWriterLockSlim sync = new ReaderWriterLockSlim();

		public static Brush GetBrush( string name )
		{
			int hashCode = name.GetHashCode();
			Brush logNameBackground;

			sync.EnterUpgradeableReadLock();

			if ( !logFileNameBrushesCache.TryGetValue( hashCode, out logNameBackground ) )
			{
				sync.EnterWriteLock();

				double hue = (hashCode - (double)Int32.MinValue) / ((double)Int32.MaxValue - Int32.MinValue) * 360;
				HsbColor hsbColor = new HsbColor( hue, 0.2, 1 );
				HsbColor darkerColor = new HsbColor( hue, 0.2, 0.95 );
				logNameBackground = new LinearGradientBrush( hsbColor.ToArgbColor(), darkerColor.ToArgbColor(), 90 );
				logNameBackground.Freeze();

				logFileNameBrushesCache.Add( hashCode, logNameBackground );

				sync.ExitWriteLock();
			}

			sync.ExitUpgradeableReadLock();

			return logNameBackground;
		}
	}
}
