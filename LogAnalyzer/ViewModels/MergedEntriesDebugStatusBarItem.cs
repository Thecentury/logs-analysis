using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace LogAnalyzer.GUI.ViewModels
{
	internal sealed class MergedEntriesDebugStatusBarItem : BindingObject
	{
		private readonly CompositeObservableListWrapper<LogEntry> list;

		public MergedEntriesDebugStatusBarItem( [NotNull] CompositeObservableListWrapper<LogEntry> list )
		{
			this.list = list;
			if ( list == null ) throw new ArgumentNullException( "list" );

			list.CollectionChanged += OnList_CollectionChanged;
			UpdateText();
		}

		private void OnList_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			UpdateText();
		}

		private void UpdateText()
		{
			Text = list.First.Count + "+" + list.Second.Count;
		}

		private string text;
		public string Text
		{
			get { return text; }
			set
			{
				if ( text == value )
					return;

				text = value;
				RaisePropertyChanged( "Text" );
			}
		}
	}
}
