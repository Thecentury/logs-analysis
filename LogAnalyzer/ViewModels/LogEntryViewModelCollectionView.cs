#define _CUSTOM_LIST_VIEW

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
	public sealed class LogEntryViewModelCollectionView : ListCollectionView
	{
		public LogEntryViewModelCollectionView( IList list )
			: base( list )
		{
			if ( !( list is IList<LogEntryViewModel> ) )
			{
				throw new ArgumentException( "list should be of type IList<LogEntryViewModel>" );
			}
		}

#if DEBUG
		protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs args )
		{
			base.OnCollectionChanged( args );

			if ( args.NewItems != null )
			{
				foreach ( var newItem in args.NewItems )
				{
					if ( !( newItem is LogEntryViewModel ) )
					{
						throw new InvalidOperationException( "Added elements should be of type LogEntryViewModel" );
					}
				}
			}

			if ( args.OldItems != null )
			{
				foreach ( var oldItem in args.OldItems )
				{
					if ( !( oldItem is LogEntryViewModel ) )
					{
						throw new InvalidOperationException( "Removed items should be of type LogEntryViewModel" );
					}
				}
			}
		}
#endif

#if DEBUG
		protected override IEnumerator GetEnumerator()
		{
			var inner = base.GetEnumerator();

			return new EnumeratorWrapper( inner );
		}

		private sealed class EnumeratorWrapper : IEnumerator
		{
			private readonly IEnumerator _inner;

			public EnumeratorWrapper( IEnumerator inner )
			{
				_inner = inner;
			}

			public bool MoveNext()
			{
				return _inner.MoveNext();
			}

			public void Reset()
			{
				_inner.Reset();
			}

			public object Current
			{
				get { return _inner.Current; }
			}
		}
#endif
	}
}
