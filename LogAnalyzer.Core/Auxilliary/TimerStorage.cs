using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Auxilliary
{
	public sealed class TimerStorage
	{
		private TimerStorage() { }

		private static readonly TimerStorage instance = new TimerStorage();
		public static TimerStorage Instance
		{
			get { return instance; }
		}

		private static readonly object sync = new object();
		private static readonly Dictionary<string, Stopwatch> nameToTimerMap = new Dictionary<string, Stopwatch>();

		public Stopwatch GetTimer( string name )
		{
			lock ( sync )
			{
				Stopwatch timer;

				if ( !nameToTimerMap.TryGetValue( name, out timer ) )
				{
					timer = Stopwatch.StartNew();
					nameToTimerMap.Add( name, timer );
				}

				return timer;
			}
		}
	}

	public static class TimerStorageExtensions
	{
		public static void StartTimer( this TimerStorage timerStorage, string timerName )
		{
			timerStorage.GetTimer( timerName );
		}

		public static long GetElapsedMilliseconds( this TimerStorage timerStorage, string timerName )
		{
			var timer = timerStorage.GetTimer( timerName );
			return timer.ElapsedMilliseconds;
		}
	}
}
