#define CUSTOM_LIST_VIEW

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;
using JetBrains.Annotations;

namespace LogAnalyzer.GUI.ViewModels
{
#if CUSTOM_LIST_VIEW

	public sealed class LogEntryViewModelCollectionView : ListCollectionView
	{
		public LogEntryViewModelCollectionView( IList list )
			: base( list )
		{
		}
	}

#else
	public sealed class LogEntryViewModelCollectionView : CollectionView
	{
		private readonly IList<LogEntryViewModel> list;

		public LogEntryViewModelCollectionView( [NotNull] IList<LogEntryViewModel> list )
			: base( list )
		{
			if ( list == null ) throw new ArgumentNullException( "list" );
			this.list = list;
		}

		public override bool Contains( object item )
		{
			return true;
		}

		//public override void Refresh()
		//{
		//    throw new NotImplementedException();
		//}

		//public override IDisposable DeferRefresh()
		//{
		//    return base.DeferRefresh();
		//}

		public override bool MoveCurrentToFirst()
		{
			throw new NotImplementedException();
		}

		public override bool MoveCurrentToLast()
		{
			throw new NotImplementedException();
		}

		public override bool MoveCurrentToNext()
		{
			throw new NotImplementedException();
		}

		public override bool MoveCurrentToPrevious()
		{
			throw new NotImplementedException();
		}

		//public override bool MoveCurrentTo( object item )
		//{
		//    throw new NotImplementedException();
		//}

		//public override bool MoveCurrentToPosition( int position )
		//{
		//    throw new NotImplementedException();
		//}

		public override bool PassesFilter( object item )
		{
			return true;
		}

		public override int IndexOf( object item )
		{
			var vm = (IAwareOfIndex)item;
			return vm.IndexInParentCollection;
		}

		public override object GetItemAt( int index )
		{
			return list[index];
		}

		//protected override void OnPropertyChanged( PropertyChangedEventArgs e )
		//{
		//    base.OnPropertyChanged( e );
		//}

		//protected override void RefreshOverride()
		//{
		//    throw new NotImplementedException();
		//}

		protected override IEnumerator GetEnumerator()
		{
			return list.GetEnumerator();
		}

		//protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs args )
		//{
		//    base.OnCollectionChanged( args );
		//}

		//protected override void OnCurrentChanging( CurrentChangingEventArgs args )
		//{
		//    throw new NotImplementedException();
		//}

		//protected override void OnCurrentChanged()
		//{
		//    throw new NotImplementedException();
		//}

		protected override void ProcessCollectionChanged( NotifyCollectionChangedEventArgs args )
		{
			//throw new NotImplementedException();
		}

		protected override void OnBeginChangeLogging( NotifyCollectionChangedEventArgs args )
		{
			throw new NotImplementedException();
		}

		public override CultureInfo Culture
		{
			get { return base.Culture; }
			set { base.Culture = value; }
		}

		public override IEnumerable SourceCollection
		{
			get { return list; }
		}

		public override Predicate<object> Filter
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public override bool CanFilter
		{
			get { return false; }
		}

		//public override SortDescriptionCollection SortDescriptions
		//{
		//    get { throw new NotImplementedException(); }
		//}

		public override bool CanSort
		{
			get { return false; }
		}

		public override bool CanGroup
		{
			get { return false; }
		}

		//public override ObservableCollection<GroupDescription> GroupDescriptions
		//{
		//    get { throw new NotImplementedException(); }
		//}

		//public override ReadOnlyObservableCollection<object> Groups
		//{
		//    get { throw new NotImplementedException(); }
		//}

		//public override object CurrentItem
		//{
		//    get { throw new NotImplementedException(); }
		//}

		//public override int CurrentPosition
		//{
		//    get { throw new NotImplementedException(); }
		//}

		//public override bool IsCurrentAfterLast
		//{
		//    get { throw new NotImplementedException(); }
		//}

		//public override bool IsCurrentBeforeFirst
		//{
		//    get { throw new NotImplementedException(); }
		//}

		public override int Count
		{
			get { return list.Count; }
		}

		public override bool IsEmpty
		{
			get { return list.Count == 0; }
		}

		public override IComparer Comparer
		{
			get { throw new NotImplementedException(); }
		}

		//public override bool NeedsRefresh
		//{
		//    get { throw new NotImplementedException(); }
		//}

		//public override event CurrentChangingEventHandler CurrentChanging
		//{
		//    add { throw new NotImplementedException(); }
		//    remove { throw new NotImplementedException(); }
		//}

		//public override event EventHandler CurrentChanged
		//{
		//    add { throw new NotImplementedException(); }
		//    remove { throw new NotImplementedException(); }
		//}

		//protected override event NotifyCollectionChangedEventHandler CollectionChanged
		//{
		//    add { base.CollectionChanged += value; }
		//    remove { base.CollectionChanged -= value; }
		//}

		//protected override event PropertyChangedEventHandler PropertyChanged
		//{
		//    add { base.PropertyChanged += value; }
		//    remove { base.PropertyChanged -= value; }
		//}
	}
#endif
}
