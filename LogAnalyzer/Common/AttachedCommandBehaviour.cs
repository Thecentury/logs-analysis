using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.GUI.Common
{
	public class AttachedCommandBehaviour
	{
		private static readonly DependencyPropertyKey BindingsPropertyKey = DependencyProperty.RegisterAttachedReadOnly( "BindingsInternal", typeof( AttachedCommandBindingCollection ), typeof( AttachedCommandBehaviour ), new FrameworkPropertyMetadata( null ) );
		public static readonly DependencyProperty BindingsProperty = AttachedCommandBehaviour.BindingsPropertyKey.DependencyProperty;
		public static AttachedCommandBindingCollection GetBindings( DependencyObject d )
		{
			if ( d == null )
			{
				throw new InvalidOperationException( "The dependency object trying to attach to is set to null" );
			}
			AttachedCommandBindingCollection attachedCommandBindingCollection = d.GetValue( AttachedCommandBehaviour.BindingsProperty ) as AttachedCommandBindingCollection;
			if ( attachedCommandBindingCollection == null )
			{
				attachedCommandBindingCollection = new AttachedCommandBindingCollection();
				attachedCommandBindingCollection.Owner = d;
				AttachedCommandBehaviour.SetBindings( d, attachedCommandBindingCollection );
			}
			return attachedCommandBindingCollection;
		}
		private static void SetBindings( DependencyObject d, AttachedCommandBindingCollection value )
		{
			d.SetValue( AttachedCommandBehaviour.BindingsPropertyKey, value );
			((INotifyCollectionChanged)value).CollectionChanged += new NotifyCollectionChangedEventHandler( AttachedCommandBehaviour.CollectionChanged );
		}
		private static void CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			AttachedCommandBindingCollection attachedCommandBindingCollection = (AttachedCommandBindingCollection)sender;
			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
					{
						if ( e.NewItems != null )
						{
							foreach ( AttachedCommandBinding attachedCommandBinding in e.NewItems )
							{
								attachedCommandBinding.Owner = attachedCommandBindingCollection.Owner;
							}
						}
						break;
					}
				case NotifyCollectionChangedAction.Remove:
					{
						if ( e.OldItems != null )
						{
							foreach ( AttachedCommandBinding attachedCommandBinding in e.OldItems )
							{
								attachedCommandBinding.Dispose();
							}
						}
						break;
					}
				case NotifyCollectionChangedAction.Replace:
					{
						if ( e.NewItems != null )
						{
							foreach ( AttachedCommandBinding attachedCommandBinding in e.NewItems )
							{
								attachedCommandBinding.Owner = attachedCommandBindingCollection.Owner;
							}
						}
						if ( e.OldItems != null )
						{
							foreach ( AttachedCommandBinding attachedCommandBinding in e.OldItems )
							{
								attachedCommandBinding.Dispose();
							}
						}
						break;
					}
				case NotifyCollectionChangedAction.Reset:
					{
						if ( e.OldItems != null )
						{
							foreach ( AttachedCommandBinding attachedCommandBinding in e.OldItems )
							{
								attachedCommandBinding.Dispose();
							}
						}
						break;
					}
			}
		}
	}
}
