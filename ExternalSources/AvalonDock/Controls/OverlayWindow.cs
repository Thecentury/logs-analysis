﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AvalonDock.Layout;
using System.Diagnostics;

namespace AvalonDock.Controls
{
    public class OverlayWindow : Window, IOverlayWindow
    {
        static OverlayWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OverlayWindow), new FrameworkPropertyMetadata(typeof(OverlayWindow)));

            OverlayWindow.AllowsTransparencyProperty.OverrideMetadata(typeof(OverlayWindow), new FrameworkPropertyMetadata(true));
            OverlayWindow.WindowStyleProperty.OverrideMetadata(typeof(OverlayWindow), new FrameworkPropertyMetadata(WindowStyle.None));
            OverlayWindow.ShowInTaskbarProperty.OverrideMetadata(typeof(OverlayWindow), new FrameworkPropertyMetadata(false));
            OverlayWindow.ShowActivatedProperty.OverrideMetadata(typeof(OverlayWindow), new FrameworkPropertyMetadata(false));
            OverlayWindow.VisibilityProperty.OverrideMetadata(typeof(OverlayWindow), new FrameworkPropertyMetadata(Visibility.Hidden));
        }


        internal OverlayWindow(IOverlayWindowHost host)
        {
            _host = host;
            FlowDirection = System.Windows.FlowDirection.LeftToRight;
        }

        Canvas _mainCanvasPanel;
        Grid _gridDockingManagerDropTargets;
        Grid _gridAnchorablePaneDropTargets;
        Grid _gridDocumentPaneDropTargets;
        Grid _gridDocumentPaneFullDropTargets;

        FrameworkElement _dockingManagerDropTargetBottom;
        FrameworkElement _dockingManagerDropTargetTop;
        FrameworkElement _dockingManagerDropTargetLeft;
        FrameworkElement _dockingManagerDropTargetRight;

        FrameworkElement _anchorablePaneDropTargetBottom;
        FrameworkElement _anchorablePaneDropTargetTop;
        FrameworkElement _anchorablePaneDropTargetLeft;
        FrameworkElement _anchorablePaneDropTargetRight;
        FrameworkElement _anchorablePaneDropTargetInto;

        FrameworkElement _documentPaneDropTargetBottom;
        FrameworkElement _documentPaneDropTargetTop;
        FrameworkElement _documentPaneDropTargetLeft;
        FrameworkElement _documentPaneDropTargetRight;
        FrameworkElement _documentPaneDropTargetInto;

        FrameworkElement _documentPaneDropTargetBottomAsAnchorablePane;
        FrameworkElement _documentPaneDropTargetTopAsAnchorablePane;
        FrameworkElement _documentPaneDropTargetLeftAsAnchorablePane;
        FrameworkElement _documentPaneDropTargetRightAsAnchorablePane;

        FrameworkElement _documentPaneFullDropTargetBottom;
        FrameworkElement _documentPaneFullDropTargetTop;
        FrameworkElement _documentPaneFullDropTargetLeft;
        FrameworkElement _documentPaneFullDropTargetRight;
        FrameworkElement _documentPaneFullDropTargetInto;

        Path _previewBox;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _mainCanvasPanel = GetTemplateChild("PART_DropTargetsContainer") as Canvas;
            _gridDockingManagerDropTargets = GetTemplateChild("PART_DockingManagerDropTargets") as Grid;
            _gridAnchorablePaneDropTargets = GetTemplateChild("PART_AnchorablePaneDropTargets") as Grid;
            _gridDocumentPaneDropTargets = GetTemplateChild("PART_DocumentPaneDropTargets") as Grid;
            _gridDocumentPaneFullDropTargets = GetTemplateChild("PART_DocumentPaneFullDropTargets") as Grid;

            _gridDockingManagerDropTargets.Visibility = System.Windows.Visibility.Hidden;
            _gridAnchorablePaneDropTargets.Visibility = System.Windows.Visibility.Hidden;
            _gridDocumentPaneDropTargets.Visibility = System.Windows.Visibility.Hidden;
            if (_gridDocumentPaneFullDropTargets != null)
                _gridDocumentPaneFullDropTargets.Visibility = System.Windows.Visibility.Hidden;

            _dockingManagerDropTargetBottom = GetTemplateChild("PART_DockingManagerDropTargetBottom") as FrameworkElement;
            _dockingManagerDropTargetTop = GetTemplateChild("PART_DockingManagerDropTargetTop") as FrameworkElement;
            _dockingManagerDropTargetLeft = GetTemplateChild("PART_DockingManagerDropTargetLeft") as FrameworkElement;
            _dockingManagerDropTargetRight = GetTemplateChild("PART_DockingManagerDropTargetRight") as FrameworkElement;
                                                                
            _anchorablePaneDropTargetBottom = GetTemplateChild("PART_AnchorablePaneDropTargetBottom") as FrameworkElement;
            _anchorablePaneDropTargetTop = GetTemplateChild("PART_AnchorablePaneDropTargetTop") as FrameworkElement;
            _anchorablePaneDropTargetLeft = GetTemplateChild("PART_AnchorablePaneDropTargetLeft") as FrameworkElement;
            _anchorablePaneDropTargetRight = GetTemplateChild("PART_AnchorablePaneDropTargetRight") as FrameworkElement;
            _anchorablePaneDropTargetInto = GetTemplateChild("PART_AnchorablePaneDropTargetInto") as FrameworkElement;

            _documentPaneDropTargetBottom = GetTemplateChild("PART_DocumentPaneDropTargetBottom") as FrameworkElement;
            _documentPaneDropTargetTop = GetTemplateChild("PART_DocumentPaneDropTargetTop") as FrameworkElement;
            _documentPaneDropTargetLeft = GetTemplateChild("PART_DocumentPaneDropTargetLeft") as FrameworkElement;
            _documentPaneDropTargetRight = GetTemplateChild("PART_DocumentPaneDropTargetRight") as FrameworkElement;
            _documentPaneDropTargetInto = GetTemplateChild("PART_DocumentPaneDropTargetInto") as FrameworkElement;

            _documentPaneDropTargetBottomAsAnchorablePane = GetTemplateChild("PART_DocumentPaneDropTargetBottomAsAnchorablePane") as FrameworkElement;
            _documentPaneDropTargetTopAsAnchorablePane = GetTemplateChild("PART_DocumentPaneDropTargetTopAsAnchorablePane") as FrameworkElement;
            _documentPaneDropTargetLeftAsAnchorablePane = GetTemplateChild("PART_DocumentPaneDropTargetLeftAsAnchorablePane") as FrameworkElement;
            _documentPaneDropTargetRightAsAnchorablePane = GetTemplateChild("PART_DocumentPaneDropTargetRightAsAnchorablePane") as FrameworkElement;

            _documentPaneFullDropTargetBottom = GetTemplateChild("PART_DocumentPaneFullDropTargetBottom") as FrameworkElement;
            _documentPaneFullDropTargetTop = GetTemplateChild("PART_DocumentPaneFullDropTargetTop") as FrameworkElement;
            _documentPaneFullDropTargetLeft = GetTemplateChild("PART_DocumentPaneFullDropTargetLeft") as FrameworkElement;
            _documentPaneFullDropTargetRight = GetTemplateChild("PART_DocumentPaneFullDropTargetRight") as FrameworkElement;
            _documentPaneFullDropTargetInto = GetTemplateChild("PART_DocumentPaneFullDropTargetInto") as FrameworkElement;

            _previewBox = GetTemplateChild("PART_PreviewBox") as Path;
        }


        internal void EnableDropTargets()
        {
            Debug.WriteLine("EnableDropTargets()");
            if (_mainCanvasPanel != null)
                _mainCanvasPanel.Visibility = System.Windows.Visibility.Visible;
        }

        internal void HideDropTargets()
        {
            Debug.WriteLine("HideDropTargets()");
            if (_mainCanvasPanel != null)
                _mainCanvasPanel.Visibility = System.Windows.Visibility.Hidden;
        
        }

        IOverlayWindowHost _host;

        IEnumerable<IDropTarget> IOverlayWindow.GetTargets()
        {
            foreach (var visibleArea in _visibleAreas)
            {
                switch (visibleArea.Type)
                {
                    case DropAreaType.DockingManager:
                        {
                            var dropAreaDockingManager = visibleArea as DropArea<DockingManager>;
                            yield return new DockingManagerDropTarget(dropAreaDockingManager.AreaElement, _dockingManagerDropTargetLeft.GetScreenArea(), DropTargetType.DockingManagerDockLeft);
                            yield return new DockingManagerDropTarget(dropAreaDockingManager.AreaElement, _dockingManagerDropTargetTop.GetScreenArea(), DropTargetType.DockingManagerDockTop);
                            yield return new DockingManagerDropTarget(dropAreaDockingManager.AreaElement, _dockingManagerDropTargetBottom.GetScreenArea(), DropTargetType.DockingManagerDockBottom);
                            yield return new DockingManagerDropTarget(dropAreaDockingManager.AreaElement, _dockingManagerDropTargetRight.GetScreenArea(), DropTargetType.DockingManagerDockRight);
                        }
                        break;
                    case DropAreaType.AnchorablePane:
                        {
                            var dropAreaAnchorablePane = visibleArea as DropArea<LayoutAnchorablePaneControl>;
                            yield return new AnchorablePaneDropTarget(dropAreaAnchorablePane.AreaElement, _anchorablePaneDropTargetLeft.GetScreenArea(), DropTargetType.AnchorablePaneDockLeft);
                            yield return new AnchorablePaneDropTarget(dropAreaAnchorablePane.AreaElement, _anchorablePaneDropTargetTop.GetScreenArea(), DropTargetType.AnchorablePaneDockTop);
                            yield return new AnchorablePaneDropTarget(dropAreaAnchorablePane.AreaElement, _anchorablePaneDropTargetRight.GetScreenArea(), DropTargetType.AnchorablePaneDockRight);
                            yield return new AnchorablePaneDropTarget(dropAreaAnchorablePane.AreaElement, _anchorablePaneDropTargetBottom.GetScreenArea(), DropTargetType.AnchorablePaneDockBottom);
                            yield return new AnchorablePaneDropTarget(dropAreaAnchorablePane.AreaElement, _anchorablePaneDropTargetInto.GetScreenArea(), DropTargetType.AnchorablePaneDockInside);

                            var parentPaneModel = dropAreaAnchorablePane.AreaElement.Model as LayoutAnchorablePane;
                            LayoutAnchorableTabItem lastAreaTabItem = null;
                            foreach (var dropAreaTabItem in dropAreaAnchorablePane.AreaElement.FindVisualChildren<LayoutAnchorableTabItem>())
                            {
                                var tabItemModel = dropAreaTabItem.Model as LayoutAnchorable;
                                lastAreaTabItem = lastAreaTabItem == null || lastAreaTabItem.GetScreenArea().Right < dropAreaTabItem.GetScreenArea().Right ?
                                    dropAreaTabItem : lastAreaTabItem;
                                int tabIndex = parentPaneModel.Children.IndexOf(tabItemModel);
                                yield return new AnchorablePaneDropTarget(dropAreaAnchorablePane.AreaElement, dropAreaTabItem.GetScreenArea(), DropTargetType.AnchorablePaneDockInside, tabIndex);
                            }

                            if (lastAreaTabItem != null)
                            {
                                var lastAreaTabItemScreenArea = lastAreaTabItem.GetScreenArea();
                                var newAreaTabItemScreenArea = new Rect(lastAreaTabItemScreenArea.TopRight, new Point(lastAreaTabItemScreenArea.Right + lastAreaTabItemScreenArea.Width, lastAreaTabItemScreenArea.Bottom));
                                if (newAreaTabItemScreenArea.Right < dropAreaAnchorablePane.AreaElement.GetScreenArea().Right)
                                    yield return new AnchorablePaneDropTarget(dropAreaAnchorablePane.AreaElement, newAreaTabItemScreenArea, DropTargetType.AnchorablePaneDockInside, parentPaneModel.Children.Count);
                            }

                            var dropAreaTitle = dropAreaAnchorablePane.AreaElement.FindVisualChildren<AnchorablePaneTitle>().FirstOrDefault();
                            if (dropAreaTitle != null)
                                yield return new AnchorablePaneDropTarget(dropAreaAnchorablePane.AreaElement, dropAreaTitle.GetScreenArea(), DropTargetType.AnchorablePaneDockInside);
                        }
                        break;
                    case DropAreaType.DocumentPane:
                        {
                            bool isDraggingAnchorables = _floatingWindow.Model is LayoutAnchorableFloatingWindow;
                            if (isDraggingAnchorables && _gridDocumentPaneFullDropTargets != null)
                            {
                                var dropAreaDocumentPane = visibleArea as DropArea<LayoutDocumentPaneControl>;
                                if (_documentPaneFullDropTargetLeft.IsVisible)
                                    yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, _documentPaneFullDropTargetLeft.GetScreenArea(), DropTargetType.DocumentPaneDockLeft);
                                if (_documentPaneFullDropTargetTop.IsVisible)
                                    yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, _documentPaneFullDropTargetTop.GetScreenArea(), DropTargetType.DocumentPaneDockTop);
                                if (_documentPaneFullDropTargetRight.IsVisible)
                                    yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, _documentPaneFullDropTargetRight.GetScreenArea(), DropTargetType.DocumentPaneDockRight);
                                if (_documentPaneFullDropTargetBottom.IsVisible)
                                    yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, _documentPaneFullDropTargetBottom.GetScreenArea(), DropTargetType.DocumentPaneDockBottom);
                                if (_documentPaneFullDropTargetInto.IsVisible)
                                    yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, _documentPaneFullDropTargetInto.GetScreenArea(), DropTargetType.DocumentPaneDockInside);

                                var parentPaneModel = dropAreaDocumentPane.AreaElement.Model as LayoutDocumentPane;
                                LayoutDocumentTabItem lastAreaTabItem = null;
                                foreach (var dropAreaTabItem in dropAreaDocumentPane.AreaElement.FindVisualChildren<LayoutDocumentTabItem>())
                                {
                                    var tabItemModel = dropAreaTabItem.Model;
                                    lastAreaTabItem = lastAreaTabItem == null || lastAreaTabItem.GetScreenArea().Right < dropAreaTabItem.GetScreenArea().Right ?
                                        dropAreaTabItem : lastAreaTabItem;
                                    int tabIndex = parentPaneModel.Children.IndexOf(tabItemModel);
                                    yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, dropAreaTabItem.GetScreenArea(), DropTargetType.DocumentPaneDockInside, tabIndex);
                                }

                                if (lastAreaTabItem != null)
                                {
                                    var lastAreaTabItemScreenArea = lastAreaTabItem.GetScreenArea();
                                    var newAreaTabItemScreenArea = new Rect(lastAreaTabItemScreenArea.TopRight, new Point(lastAreaTabItemScreenArea.Right + lastAreaTabItemScreenArea.Width, lastAreaTabItemScreenArea.Bottom));
                                    if (newAreaTabItemScreenArea.Right < dropAreaDocumentPane.AreaElement.GetScreenArea().Right)
                                        yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, newAreaTabItemScreenArea, DropTargetType.DocumentPaneDockInside, parentPaneModel.Children.Count);
                                }

                                if (_documentPaneDropTargetLeftAsAnchorablePane.IsVisible)
                                    yield return new DocumentPaneDropAsAnchorableTarget(dropAreaDocumentPane.AreaElement, _documentPaneDropTargetLeftAsAnchorablePane.GetScreenArea(), DropTargetType.DocumentPaneDockAsAnchorableLeft);
                                if (_documentPaneDropTargetTopAsAnchorablePane.IsVisible)
                                    yield return new DocumentPaneDropAsAnchorableTarget(dropAreaDocumentPane.AreaElement, _documentPaneDropTargetTopAsAnchorablePane.GetScreenArea(), DropTargetType.DocumentPaneDockAsAnchorableTop);
                                if (_documentPaneDropTargetRightAsAnchorablePane.IsVisible)
                                    yield return new DocumentPaneDropAsAnchorableTarget(dropAreaDocumentPane.AreaElement, _documentPaneDropTargetRightAsAnchorablePane.GetScreenArea(), DropTargetType.DocumentPaneDockAsAnchorableRight);
                                if (_documentPaneDropTargetBottomAsAnchorablePane.IsVisible)
                                    yield return new DocumentPaneDropAsAnchorableTarget(dropAreaDocumentPane.AreaElement, _documentPaneDropTargetBottomAsAnchorablePane.GetScreenArea(), DropTargetType.DocumentPaneDockAsAnchorableBottom);
                            }
                            else
                            {

                                var dropAreaDocumentPane = visibleArea as DropArea<LayoutDocumentPaneControl>;
                                if (_documentPaneDropTargetLeft.IsVisible)
                                    yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, _documentPaneDropTargetLeft.GetScreenArea(), DropTargetType.DocumentPaneDockLeft);
                                if (_documentPaneDropTargetTop.IsVisible)
                                    yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, _documentPaneDropTargetTop.GetScreenArea(), DropTargetType.DocumentPaneDockTop);
                                if (_documentPaneDropTargetRight.IsVisible)
                                    yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, _documentPaneDropTargetRight.GetScreenArea(), DropTargetType.DocumentPaneDockRight);
                                if (_documentPaneDropTargetBottom.IsVisible)
                                    yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, _documentPaneDropTargetBottom.GetScreenArea(), DropTargetType.DocumentPaneDockBottom);
                                if (_documentPaneDropTargetInto.IsVisible)
                                    yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, _documentPaneDropTargetInto.GetScreenArea(), DropTargetType.DocumentPaneDockInside);

                                var parentPaneModel = dropAreaDocumentPane.AreaElement.Model as LayoutDocumentPane;
                                LayoutDocumentTabItem lastAreaTabItem = null;
                                foreach (var dropAreaTabItem in dropAreaDocumentPane.AreaElement.FindVisualChildren<LayoutDocumentTabItem>())
                                {
                                    var tabItemModel = dropAreaTabItem.Model;
                                    lastAreaTabItem = lastAreaTabItem == null || lastAreaTabItem.GetScreenArea().Right < dropAreaTabItem.GetScreenArea().Right ?
                                        dropAreaTabItem : lastAreaTabItem;
                                    int tabIndex = parentPaneModel.Children.IndexOf(tabItemModel);
                                    yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, dropAreaTabItem.GetScreenArea(), DropTargetType.DocumentPaneDockInside, tabIndex);
                                }

                                if (lastAreaTabItem != null)
                                {
                                    var lastAreaTabItemScreenArea = lastAreaTabItem.GetScreenArea();
                                    var newAreaTabItemScreenArea = new Rect(lastAreaTabItemScreenArea.TopRight, new Point(lastAreaTabItemScreenArea.Right + lastAreaTabItemScreenArea.Width, lastAreaTabItemScreenArea.Bottom));
                                    if (newAreaTabItemScreenArea.Right < dropAreaDocumentPane.AreaElement.GetScreenArea().Right)
                                        yield return new DocumentPaneDropTarget(dropAreaDocumentPane.AreaElement, newAreaTabItemScreenArea, DropTargetType.DocumentPaneDockInside, parentPaneModel.Children.Count);
                                }
                            }
                        }
                        break;
                    case DropAreaType.DocumentPaneGroup:
                        {
                            var dropAreaDocumentPane = visibleArea as DropArea<LayoutDocumentPaneGroupControl>;
                            if (_documentPaneDropTargetInto.IsVisible)
                                yield return new DocumentPaneGroupDropTarget(dropAreaDocumentPane.AreaElement, _documentPaneDropTargetInto.GetScreenArea(), DropTargetType.DocumentPaneGroupDockInside);
                        }
                        break;
                }

            }
            yield break;
        }

        LayoutFloatingWindowControl _floatingWindow = null;
        void IOverlayWindow.DragEnter(LayoutFloatingWindowControl floatingWindow)
        {
            _floatingWindow = floatingWindow;
            //Visibility = System.Windows.Visibility.Visible;
            EnableDropTargets();
        }

        void IOverlayWindow.DragLeave(LayoutFloatingWindowControl floatingWindow)
        {
            Visibility = System.Windows.Visibility.Hidden;
            _floatingWindow = null;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }


        List<IDropArea> _visibleAreas = new List<IDropArea>();
        void IOverlayWindow.DragEnter(IDropArea area)
        {
            _visibleAreas.Add(area);

            FrameworkElement areaElement;
            switch (area.Type)
            {
                case DropAreaType.DockingManager:
                    areaElement = _gridDockingManagerDropTargets;
                    break;
                case DropAreaType.AnchorablePane:
                    areaElement = _gridAnchorablePaneDropTargets;
                    break;
                case DropAreaType.DocumentPaneGroup:
                    {
                        areaElement = _gridDocumentPaneDropTargets;
                        var dropAreaDocumentPaneGroup = area as DropArea<LayoutDocumentPaneGroupControl>;
                        var layoutDocumentPane = (dropAreaDocumentPaneGroup.AreaElement.Model as LayoutDocumentPaneGroup).Children.First() as LayoutDocumentPane;
                        var parentDocumentPaneGroup = layoutDocumentPane.Parent as LayoutDocumentPaneGroup;

                        _documentPaneDropTargetLeft.Visibility = Visibility.Hidden;
                        _documentPaneDropTargetRight.Visibility = Visibility.Hidden;
                        _documentPaneDropTargetTop.Visibility = Visibility.Hidden;
                        _documentPaneDropTargetBottom.Visibility = Visibility.Hidden;
                    }
                    break;
                case DropAreaType.DocumentPane:
                default:
                    {
                        bool isDraggingAnchorables = _floatingWindow.Model is LayoutAnchorableFloatingWindow;
                        if (isDraggingAnchorables && _gridDocumentPaneFullDropTargets != null)
                        {
                            areaElement = _gridDocumentPaneFullDropTargets;
                            var dropAreaDocumentPaneGroup = area as DropArea<LayoutDocumentPaneControl>;
                            var layoutDocumentPane = dropAreaDocumentPaneGroup.AreaElement.Model as LayoutDocumentPane;
                            var parentDocumentPaneGroup = layoutDocumentPane.Parent as LayoutDocumentPaneGroup;

                            if (parentDocumentPaneGroup != null &&
                                parentDocumentPaneGroup.Children.Where(c => c.IsVisible).Count() > 1)
                            {
                                _documentPaneFullDropTargetLeft.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Horizontal ? Visibility.Visible : Visibility.Hidden;
                                _documentPaneFullDropTargetRight.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Horizontal ? Visibility.Visible : Visibility.Hidden;
                                _documentPaneFullDropTargetTop.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Vertical ? Visibility.Visible : Visibility.Hidden;
                                _documentPaneFullDropTargetBottom.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Vertical ? Visibility.Visible : Visibility.Hidden;
                            }
                            else if (parentDocumentPaneGroup == null &&
                                layoutDocumentPane != null &&
                                layoutDocumentPane.ChildrenCount == 0)
                            {
                                _documentPaneFullDropTargetLeft.Visibility = Visibility.Hidden;
                                _documentPaneFullDropTargetRight.Visibility = Visibility.Hidden;
                                _documentPaneFullDropTargetTop.Visibility = Visibility.Hidden;
                                _documentPaneFullDropTargetBottom.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                _documentPaneFullDropTargetLeft.Visibility = Visibility.Visible;
                                _documentPaneFullDropTargetRight.Visibility = Visibility.Visible;
                                _documentPaneFullDropTargetTop.Visibility = Visibility.Visible;
                                _documentPaneFullDropTargetBottom.Visibility = Visibility.Visible;
                            }

                            if (parentDocumentPaneGroup != null &&
                                parentDocumentPaneGroup.Children.Where(c => c.IsVisible).Count() > 1)
                            {
                                int indexOfDocumentPane = parentDocumentPaneGroup.IndexOfChild(layoutDocumentPane);
                                bool isFirstChild = indexOfDocumentPane == 0;
                                bool isLastChild = indexOfDocumentPane == parentDocumentPaneGroup.ChildrenCount - 1;
                                
                                _documentPaneDropTargetBottomAsAnchorablePane.Visibility = 
                                    parentDocumentPaneGroup.Orientation == Orientation.Vertical ? 
                                        (isLastChild ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden) :
                                        System.Windows.Visibility.Hidden;
                                _documentPaneDropTargetTopAsAnchorablePane.Visibility =
                                    parentDocumentPaneGroup.Orientation == Orientation.Vertical ?
                                        (isFirstChild ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden) :
                                        System.Windows.Visibility.Hidden;

                                _documentPaneDropTargetLeftAsAnchorablePane.Visibility =
                                    parentDocumentPaneGroup.Orientation == Orientation.Horizontal ?
                                        (isFirstChild ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden) :
                                        System.Windows.Visibility.Hidden;


                                _documentPaneDropTargetRightAsAnchorablePane.Visibility =
                                    parentDocumentPaneGroup.Orientation == Orientation.Horizontal ?
                                        (isLastChild ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden) :
                                        System.Windows.Visibility.Hidden;
                            }
                            else
                            {
                                _documentPaneDropTargetBottomAsAnchorablePane.Visibility = System.Windows.Visibility.Visible;
                                _documentPaneDropTargetLeftAsAnchorablePane.Visibility = System.Windows.Visibility.Visible;
                                _documentPaneDropTargetRightAsAnchorablePane.Visibility = System.Windows.Visibility.Visible;
                                _documentPaneDropTargetTopAsAnchorablePane.Visibility = System.Windows.Visibility.Visible;
                            }
                        }
                        else
                        {
                            areaElement = _gridDocumentPaneDropTargets;
                            var dropAreaDocumentPaneGroup = area as DropArea<LayoutDocumentPaneControl>;
                            var layoutDocumentPane = dropAreaDocumentPaneGroup.AreaElement.Model as LayoutDocumentPane;
                            var parentDocumentPaneGroup = layoutDocumentPane.Parent as LayoutDocumentPaneGroup;

                            if (parentDocumentPaneGroup != null &&
                                parentDocumentPaneGroup.Children.Where(c => c.IsVisible).Count() > 1)
                            {
                                _documentPaneDropTargetLeft.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Horizontal ? Visibility.Visible : Visibility.Hidden;
                                _documentPaneDropTargetRight.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Horizontal ? Visibility.Visible : Visibility.Hidden;
                                _documentPaneDropTargetTop.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Vertical ? Visibility.Visible : Visibility.Hidden;
                                _documentPaneDropTargetBottom.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Vertical ? Visibility.Visible : Visibility.Hidden;
                            }
                            else if (parentDocumentPaneGroup == null &&
                                layoutDocumentPane != null &&
                                layoutDocumentPane.ChildrenCount == 0)
                            {
                                _documentPaneDropTargetLeft.Visibility = Visibility.Hidden;
                                _documentPaneDropTargetRight.Visibility = Visibility.Hidden;
                                _documentPaneDropTargetTop.Visibility = Visibility.Hidden;
                                _documentPaneDropTargetBottom.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                _documentPaneDropTargetLeft.Visibility = Visibility.Visible;
                                _documentPaneDropTargetRight.Visibility = Visibility.Visible;
                                _documentPaneDropTargetTop.Visibility = Visibility.Visible;
                                _documentPaneDropTargetBottom.Visibility = Visibility.Visible;
                            }
                        }
                    }
                    break;
            }

            Canvas.SetLeft(areaElement, area.DetectionRect.Left - Left);
            Canvas.SetTop(areaElement, area.DetectionRect.Top - Top);
            areaElement.Width = area.DetectionRect.Width;
            areaElement.Height = area.DetectionRect.Height;
            areaElement.Visibility = System.Windows.Visibility.Visible;
        }

        void IOverlayWindow.DragLeave(IDropArea area)
        {
            _visibleAreas.Remove(area);

            FrameworkElement areaElement;
            switch (area.Type)
            {
                case DropAreaType.DockingManager:
                    areaElement = _gridDockingManagerDropTargets;
                    break;
                case DropAreaType.AnchorablePane:
                    areaElement = _gridAnchorablePaneDropTargets;
                    break;
                case DropAreaType.DocumentPane:
                case DropAreaType.DocumentPaneGroup:
                default:
                    {
                        bool isDraggingAnchorables = _floatingWindow.Model is LayoutAnchorableFloatingWindow;
                        if (isDraggingAnchorables && _gridDocumentPaneFullDropTargets != null)
                            areaElement = _gridDocumentPaneFullDropTargets;
                        else
                            areaElement = _gridDocumentPaneDropTargets;
                    }
                    break;
            }

            areaElement.Visibility = System.Windows.Visibility.Hidden;
        }

        void IOverlayWindow.DragEnter(IDropTarget target)
        {
            var previewBoxPath = target.GetPreviewPath(this, _floatingWindow.Model as LayoutFloatingWindow);
            if (previewBoxPath != null)
            {
                _previewBox.Data = previewBoxPath;
                _previewBox.Visibility = System.Windows.Visibility.Visible;
            }
        }

        void IOverlayWindow.DragLeave(IDropTarget target)
        {
            _previewBox.Visibility = System.Windows.Visibility.Hidden;
        }

        void IOverlayWindow.DragDrop(IDropTarget target)
        {
            target.Drop(_floatingWindow.Model as LayoutFloatingWindow);
        }

        
    }
}
