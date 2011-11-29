using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class DropFilesViewModel : TabViewModel
	{
		public DropFilesViewModel( ApplicationViewModel applicationViewModel )
			: base( applicationViewModel )
		{
			files.CollectionChanged += OnFilesCollectionChanged;
		}

		private void OnFilesCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			RaisePropertyChanged( "IconFile" );
			RaisePropertyChanged( "HasNotFiles" );
		}

		protected override bool CanBeClosedCore()
		{
			return false;
		}

		public override string Header
		{
			get
			{
				return "Drop files";
			}
		}

		public override string IconFile
		{
			get
			{
				string icon = files.Count > 0 ? "battery-charge" : "battery-low";

				return MakePackUri( string.Format( "/Resources/{0}.png", icon ) );
			}
		}

		public bool HasNotFiles
		{
			get { return files.Count == 0; }
		}

		private readonly ObservableCollection<object> files = new ObservableCollection<object>();
	}
}
