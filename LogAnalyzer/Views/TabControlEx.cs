﻿using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Collections;
using System.Collections.Specialized;

namespace LogAnalyzer.GUI.Views
{
	/// <summary>
	/// TabControl, который решает проблему с пересозданием детей при переключении вкладок.
	/// <para/>
	/// <remarks>
	/// http://www.pluralsight-training.net/community/blogs/eburke/archive/2009/04/30/keeping-the-wpf-tab-control-from-destroying-its-children.aspx
	/// </remarks>
	/// </summary>
	[TemplatePart( Name = "PART_ItemsHolder", Type = typeof( Panel ) )]
	public class TabControlEx : TabControl
	{
		private Panel _itemsHolder;

		public TabControlEx()
		{
			// this is necessary so that we get the initial databound selected item
			this.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
		}

		/// <summary>
		/// if containers are done, generate the selected item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ItemContainerGeneratorStatusChanged( object sender, EventArgs e )
		{
			if ( this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated )
			{
				this.ItemContainerGenerator.StatusChanged -= ItemContainerGeneratorStatusChanged;
				UpdateSelectedItem();
			}
		}

		/// <summary>
		/// get the ItemsHolder and generate any children
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_itemsHolder = GetTemplateChild( "PART_ItemsHolder" ) as Panel;
			UpdateSelectedItem();
		}

		protected override void OnItemsSourceChanged( IEnumerable oldValue, IEnumerable newValue )
		{
			base.OnItemsSourceChanged( oldValue, newValue );
			UpdateSelectedItem();
		}

		/// <summary>
		/// when the items change we remove any generated panel children and add any new ones as necessary
		/// </summary>
		/// <param name="e"></param>
		protected override void OnItemsChanged( NotifyCollectionChangedEventArgs e )
		{
			base.OnItemsChanged( e );

			if ( _itemsHolder == null )
			{
				return;
			}

			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Reset:
					_itemsHolder.Children.Clear();
					break;

				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Remove:
					if ( e.OldItems != null )
					{
						foreach ( var item in e.OldItems )
						{
							ContentPresenter cp = FindChildContentPresenter( item );
							if ( cp != null )
							{
								_itemsHolder.Children.Remove( cp );
							}
						}
					}

					// don't do anything with new items because we don't want to
					// create visuals that aren't being shown

					UpdateSelectedItem();
					break;

				case NotifyCollectionChangedAction.Replace:
					throw new NotImplementedException( "Replace not implemented yet" );
			}
		}

		/// <summary>
		/// update the visible child in the ItemsHolder
		/// </summary>
		/// <param name="e"></param>
		protected override void OnSelectionChanged( SelectionChangedEventArgs e )
		{
			base.OnSelectionChanged( e );
			UpdateSelectedItem();
		}

		/// <summary>
		/// generate a ContentPresenter for the selected item
		/// </summary>
		void UpdateSelectedItem()
		{
			if ( _itemsHolder == null )
			{
				return;
			}

			// generate a ContentPresenter if necessary
			TabItem item = GetSelectedTabItem();
			if ( item != null )
			{
				CreateChildContentPresenter( item );
			}

			// show the right child
			foreach ( ContentPresenter child in _itemsHolder.Children )
			{
				child.Visibility = ((child.Tag as TabItem).IsSelected) ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		/// <summary>
		/// create the child ContentPresenter for the given item (could be data or a TabItem)
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		ContentPresenter CreateChildContentPresenter( object item )
		{
			if ( item == null )
			{
				return null;
			}

			ContentPresenter cp = FindChildContentPresenter( item );

			if ( cp != null )
			{
				return cp;
			}

			// the actual child to be added.  cp.Tag is a reference to the TabItem
			cp = new ContentPresenter();
			cp.Content = (item is TabItem) ? (item as TabItem).Content : item;
			cp.ContentTemplate = this.SelectedContentTemplate;
			cp.ContentTemplateSelector = this.SelectedContentTemplateSelector;
			cp.ContentStringFormat = this.SelectedContentStringFormat;
			cp.Visibility = Visibility.Collapsed;
			cp.Tag = (item is TabItem) ? item : (this.ItemContainerGenerator.ContainerFromItem( item ));
			_itemsHolder.Children.Add( cp );
			return cp;
		}

		/// <summary>
		/// Find the CP for the given object.  data could be a TabItem or a piece of data
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		ContentPresenter FindChildContentPresenter( object data )
		{
			if ( data is TabItem )
			{
				data = (data as TabItem).Content;
			}

			if ( data == null )
			{
				return null;
			}

			if ( _itemsHolder == null )
			{
				return null;
			}

			foreach ( ContentPresenter cp in _itemsHolder.Children )
			{
				if ( cp.Content == data )
				{
					return cp;
				}
			}

			return null;
		}

		/// <summary>
		/// copied from TabControl; wish it were protected in that class instead of private
		/// </summary>
		/// <returns></returns>
		protected TabItem GetSelectedTabItem()
		{
			object selectedItem = base.SelectedItem;
			if ( selectedItem == null )
			{
				return null;
			}
			TabItem item = selectedItem as TabItem;
			if ( item == null )
			{
				item = base.ItemContainerGenerator.ContainerFromIndex( base.SelectedIndex ) as TabItem;
			}
			return item;
		}
	}
}
