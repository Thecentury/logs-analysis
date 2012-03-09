using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.Controls
{
	internal sealed class HighlightingBorderControl : FrameworkElement
	{
		#region HighlightingViewModels dependency property

		public ObservableCollection<HighlightingViewModel> HighlightingViewModels
		{
			get { return (ObservableCollection<HighlightingViewModel>)GetValue( HighlightingViewModelsProperty ); }
			set { SetValue( HighlightingViewModelsProperty, value ); }
		}

		public static readonly DependencyProperty HighlightingViewModelsProperty = DependencyProperty.Register(
		  "HighlightingViewModels",
		  typeof( ObservableCollection<HighlightingViewModel> ),
		  typeof( HighlightingBorderControl ),
		  new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.AffectsRender, OnHighlightingViewModelsReplaced ) );

		private static void OnHighlightingViewModelsReplaced( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			HighlightingBorderControl owner = (HighlightingBorderControl)d;

			var prevValue = (ObservableCollection<HighlightingViewModel>)e.OldValue;
			if ( prevValue != null )
			{
				prevValue.CollectionChanged -= owner.OnHighlightingViewModelsCollectionChanged;

				foreach ( var viewModel in prevValue )
				{
					viewModel.PropertyChanged -= owner.OnOneHighlightingViewModelPropertyChanged;
				}
			}

			var currValue = (ObservableCollection<HighlightingViewModel>)e.NewValue;
			if ( currValue != null )
			{
				currValue.CollectionChanged += owner.OnHighlightingViewModelsCollectionChanged;

				foreach ( var viewModel in currValue )
				{
					viewModel.PropertyChanged += owner.OnOneHighlightingViewModelPropertyChanged;
				}
			}
		}

		private void OnHighlightingViewModelsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.NewItems != null )
			{
				foreach ( var vm in e.NewItems.Cast<HighlightingViewModel>() )
				{
					vm.PropertyChanged += OnOneHighlightingViewModelPropertyChanged;
				}
			}
			if ( e.OldItems != null )
			{
				foreach ( var vm in e.OldItems.Cast<HighlightingViewModel>() )
				{
					vm.PropertyChanged -= OnOneHighlightingViewModelPropertyChanged;
				}
			}

			InvalidateVisual();
		}

		private void OnOneHighlightingViewModelPropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			InvalidateVisual();
		}

		#endregion

		#region Offset dependency property

		public double Offset
		{
			get { return (double)GetValue( OffsetProperty ); }
			set { SetValue( OffsetProperty, value ); }
		}

		public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
		  "Offset",
		  typeof( double ),
		  typeof( HighlightingBorderControl ),
		  new FrameworkPropertyMetadata( 3.0, FrameworkPropertyMetadataOptions.AffectsRender ) );

		#endregion

		protected override void OnRender( DrawingContext dc )
		{
			base.OnRender( dc );

			var hvms = HighlightingViewModels;

			if ( hvms == null )
			{
				return;
			}

			double offset = Offset;
			double width = ActualWidth;
			double height = ActualHeight;

			Rect rect;

			for ( int i = 0; i < hvms.Count; i++ )
			{
				var highlightingViewModel = hvms[i];
				int index = i;
				double size = offset * index;

				rect = new Rect( 0 + size, 0 + size, Math.Max( width - 2 * size, 0 ), Math.Max( height - 2 * size, 0 ) );

				dc.DrawRectangle( highlightingViewModel.Brush, null, rect );
			}

			int count = hvms.Count;
			if ( count > 0 )
			{
				rect = new Rect( 0 + offset * count, 0 + offset * count, Math.Max( width - 2 * offset * count, 0 ), Math.Max( height - 2 * offset * count, 0 ) );
				dc.DrawRectangle( null, null, rect );
			}
		}
	}
}
