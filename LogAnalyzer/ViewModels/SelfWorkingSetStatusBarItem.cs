using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using JetBrains.Annotations;
using LogAnalyzer.GUI.Properties;

namespace LogAnalyzer.GUI.ViewModels
{
	/// <summary>
	/// Отображает на статус баре собственное потребление памяти.
	/// </summary>
	internal sealed class SelfWorkingSetStatusBarItem : BindingObject
	{
		private readonly Timer timer;

		public SelfWorkingSetStatusBarItem( )
		{
			timer = new Timer( Settings.Default.SelfMemoryStatusBarItemUpdateInterval.TotalMilliseconds );
			timer.Elapsed += OnTimer_Elapsed;
			timer.Start();
		}

		private void OnTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			RaisePropertyChanged( "MemoryWorkingSet" );
		}

		public double MemoryWorkingSet
		{
			get { return Math.Round( Environment.WorkingSet / 1024.0 / 1024.0, 1 ); }
		}
	}
}
