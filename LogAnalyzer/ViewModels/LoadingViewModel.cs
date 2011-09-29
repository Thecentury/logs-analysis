using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class LoadingViewModel : TabViewModel, IHierarchyMember<ApplicationViewModel, LogEntriesList>
	{
		public override string Header
		{
			get
			{
				return "Loading...";
			}
		}

		public override string IconFile
		{
			get
			{
				return MakePackUri( "/Resources/clock-history.png" );
			}
		}

		private int loadedBytes;
		private readonly ApplicationViewModel applicationViewModel;

		public LoadingViewModel( ApplicationViewModel applicationViewModel )
			: base( applicationViewModel )
		{
			if ( applicationViewModel == null )
				throw new ArgumentNullException( "applicationViewModel" );

			this.applicationViewModel = applicationViewModel;

			LogAnalyzerCore core = applicationViewModel.Core;
			core.ReadProgress += OnCore_ReadProgress;

			IsActive = true;
		}

		private double loadingProgress = 0;
		public double LoadingProgress
		{
			get { return loadingProgress; }
			private set
			{
				loadingProgress = value;
				RaisePropertyChanged( "LoadingProgress" );
				TaskbarHelper.SetProgressValue( (int)value );
			}
		}

		private void OnCore_ReadProgress( object sender, FileReadEventArgs e )
		{
			BeginInvokeInUIDispatcher( () =>
			{
				UpdateProgress( e );
			} );
		}

		private void UpdateProgress( FileReadEventArgs e )
		{
			loadedBytes += e.BytesReadSincePreviousCall;
			LoadingProgress = 100.0 * loadedBytes / applicationViewModel.Core.TotalLengthInBytes;
		}

		protected override bool CanBeClosedCore()
		{
			return false;
		}

		ApplicationViewModel IHierarchyMember<ApplicationViewModel, LogEntriesList>.Parent
		{
			get { return applicationViewModel; }
		}

		LogEntriesList IHierarchyMember<ApplicationViewModel, LogEntriesList>.Data
		{
			get { return null; }
		}
	}
}
