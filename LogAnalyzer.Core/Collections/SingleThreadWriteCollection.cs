using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Collections
{
	internal sealed class SingleThreadWriteCollection<T> : Collection<T>
	{
		private bool entered = false;

		private void Enter()
		{
			Condition.DebugAssert( !entered );
			entered = true;
		}

		private void Exit()
		{
			entered = false;
		}

		protected override void InsertItem( int index, T item )
		{
			Enter();
			base.InsertItem( index, item );
			Exit();
		}

		protected override void SetItem( int index, T item )
		{
			Enter();
			base.SetItem( index, item );
			Exit();
		}

		protected override void ClearItems()
		{
			Enter();
			base.ClearItems();
			Exit();
		}

		protected override void RemoveItem( int index )
		{
			Enter();
			base.RemoveItem( index );
			Exit();
		}
	}
}
