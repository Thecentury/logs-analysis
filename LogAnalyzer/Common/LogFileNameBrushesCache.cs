using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;

namespace LogAnalyzer.GUI.Common
{
	public abstract class LogFileNameBrushesCache
	{
		private readonly Dictionary<int, Brush> _logFileNameBrushesCache = new Dictionary<int, Brush>();

		private readonly ReaderWriterLockSlim _sync = new ReaderWriterLockSlim();

		public Brush GetBrush( int hashCode )
		{
			Brush logNameBackground;

			_sync.EnterUpgradeableReadLock();

			if ( !_logFileNameBrushesCache.TryGetValue( hashCode, out logNameBackground ) )
			{
				_sync.EnterWriteLock();

				double hue = (hashCode - (double)Int32.MinValue) / ((double)Int32.MaxValue - Int32.MinValue) * 360;
				logNameBackground = CreateBrush( hue );
				logNameBackground.Freeze();

				_logFileNameBrushesCache.Add( hashCode, logNameBackground );

				_sync.ExitWriteLock();
			}

			_sync.ExitUpgradeableReadLock();

			return logNameBackground;
		}

		protected abstract Brush CreateBrush( double hue );

		private static readonly SolidColorBrushCache solidCache = new SolidColorBrushCache();
		public static LogFileNameBrushesCache Solid
		{
			get { return solidCache; }
		}

		private static readonly GradientBrushCache gradientCache = new GradientBrushCache();
		public static LogFileNameBrushesCache Gradient
		{
			get { return gradientCache; }
		}
	}

	internal sealed class GradientBrushCache : LogFileNameBrushesCache
	{
		protected override Brush CreateBrush( double hue )
		{
			HsbColor hsbColor = new HsbColor( hue, 0.2, 1 );
			HsbColor darkerColor = new HsbColor( hue, 0.2, 0.95 );
			Brush logNameBackground = new LinearGradientBrush( hsbColor.ToArgbColor(), darkerColor.ToArgbColor(), 90 );
			return logNameBackground;
		}
	}

	internal sealed class SolidColorBrushCache : LogFileNameBrushesCache
	{
		protected override Brush CreateBrush( double hue )
		{
			HsbColor hsbColor = new HsbColor( hue, 0.4, 1 );
			return new SolidColorBrush( hsbColor.ToArgbColor() );
		}
	}
}
