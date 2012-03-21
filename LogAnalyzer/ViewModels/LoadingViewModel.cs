using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.ViewModels
{
	internal sealed class PreloaderViewModel : TabViewModel
	{
		public PreloaderViewModel( ApplicationViewModel applicationViewModel )
			: base( applicationViewModel )
		{
		}

		public override string IconFile
		{
			get { return MakePackUri( "/Resources/clock-history.png" ); }
		}

		public override string Header
		{
			get { return ""; }
		}

		protected override bool CanBeClosedCore()
		{
			return false;
		}
	}

	public sealed class LoadingViewModel : TabViewModel, IHierarchyMember<ApplicationViewModel, LogEntriesList>
	{
		public override string Header
		{
			get { return "Loading..."; }
		}

		public override string IconFile
		{
			get { return MakePackUri( "/Resources/clock-history.png" ); }
		}

		private int _loadedBytes;
		private readonly ApplicationViewModel _applicationViewModel;

		public LoadingViewModel( ApplicationViewModel applicationViewModel )
			: base( applicationViewModel )
		{
			if ( applicationViewModel == null )
			{
				throw new ArgumentNullException( "applicationViewModel" );
			}

			this._applicationViewModel = applicationViewModel;

			LogAnalyzerCore core = applicationViewModel.Core;
			core.ReadProgress += OnCoreReadProgress;

			IsActive = true;
		}

		private double _loadingProgress;
		public double LoadingProgress
		{
			get { return _loadingProgress; }
			private set
			{
				_loadingProgress = value;
				RaisePropertyChanged( "LoadingProgress" );

				_applicationViewModel.ProgressValue = (int)value;
			}
		}

		private void OnCoreReadProgress( object sender, FileReadEventArgs e )
		{
			_loadedBytes += e.BytesReadSincePreviousCall;
			LoadingProgress = 100.0 * _loadedBytes / _applicationViewModel.Core.TotalLengthInBytes;
		}

		protected override bool CanBeClosedCore()
		{
			return false;
		}

		ApplicationViewModel IHierarchyMember<ApplicationViewModel, LogEntriesList>.Parent
		{
			get { return _applicationViewModel; }
		}

		LogEntriesList IHierarchyMember<ApplicationViewModel, LogEntriesList>.Data
		{
			get { return null; }
		}
	}
}
