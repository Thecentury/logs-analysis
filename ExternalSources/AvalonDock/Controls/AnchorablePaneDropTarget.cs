﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
    internal class AnchorablePaneDropTarget : DropTarget<LayoutAnchorablePaneControl>
    {
        internal AnchorablePaneDropTarget(LayoutAnchorablePaneControl paneControl, Rect detectionRect, DropTargetType type)
            : base(paneControl, detectionRect, type)
        {
            _targetPane = paneControl;
        }

        internal AnchorablePaneDropTarget(LayoutAnchorablePaneControl paneControl, Rect detectionRect, DropTargetType type, int tabIndex)
            : base(paneControl, detectionRect, type)
        {
            _targetPane = paneControl;
            _tabIndex = tabIndex;
        }


        LayoutAnchorablePaneControl _targetPane;

        int _tabIndex = -1;

        protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow)
        {
            ILayoutAnchorablePane targetModel = _targetPane.Model as ILayoutAnchorablePane;
            
            switch (Type)
            {
                case DropTargetType.AnchorablePaneDockBottom:
                    #region DropTargetType.AnchorablePaneDockBottom
                    {
                        var parentModel = targetModel.Parent as ILayoutGroup;
                        var parentModelOrientable = targetModel.Parent as ILayoutOrientableGroup;
                        int insertToIndex = parentModel.IndexOfChild(targetModel);

                        if (parentModelOrientable.Orientation != System.Windows.Controls.Orientation.Vertical &&
                            parentModel.ChildrenCount == 1)
                            parentModelOrientable.Orientation = System.Windows.Controls.Orientation.Vertical;

                        if (parentModelOrientable.Orientation == System.Windows.Controls.Orientation.Vertical)
                        {
                            var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
                            if (layoutAnchorablePaneGroup != null &&
                                (layoutAnchorablePaneGroup.Children.Count == 1 ||
                                    layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical))
                            {
                                var anchorablesToMove = layoutAnchorablePaneGroup.Children.ToArray();
                                for (int i = 0; i < anchorablesToMove.Length; i++)
                                    parentModel.InsertChildAt(insertToIndex + 1 + i, anchorablesToMove[i]);
                            }
                            else
                                parentModel.InsertChildAt(insertToIndex + 1, floatingWindow.RootPanel);
                        }
                        else
                        {
                            var newOrientedPanel = new LayoutAnchorablePaneGroup()
                            {
                                Orientation = System.Windows.Controls.Orientation.Vertical
                            };

                            newOrientedPanel.Children.Add(targetModel);
                            newOrientedPanel.Children.Add(floatingWindow.RootPanel);

                            parentModel.InsertChildAt(insertToIndex, newOrientedPanel);
                        }
                    }
                    break;
                    #endregion
                case DropTargetType.AnchorablePaneDockTop:
                    #region DropTargetType.AnchorablePaneDockTop
                    {
                        var parentModel = targetModel.Parent as ILayoutGroup;
                        var parentModelOrientable = targetModel.Parent as ILayoutOrientableGroup;
                        int insertToIndex = parentModel.IndexOfChild(targetModel);

                        if (parentModelOrientable.Orientation != System.Windows.Controls.Orientation.Vertical &&
                            parentModel.ChildrenCount == 1)
                            parentModelOrientable.Orientation = System.Windows.Controls.Orientation.Vertical;

                        if (parentModelOrientable.Orientation == System.Windows.Controls.Orientation.Vertical)
                        {
                            var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
                            if (layoutAnchorablePaneGroup != null &&
                                (layoutAnchorablePaneGroup.Children.Count == 1 ||
                                    layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical))
                            {
                                var anchorablesToMove = layoutAnchorablePaneGroup.Children.ToArray();
                                for (int i = 0; i < anchorablesToMove.Length; i++)
                                    parentModel.InsertChildAt(insertToIndex + i, anchorablesToMove[i]);
                            }
                            else
                                parentModel.InsertChildAt(insertToIndex, floatingWindow.RootPanel);
                        }
                        else
                        {
                            var newOrientedPanel = new LayoutAnchorablePaneGroup()
                            {
                                Orientation = System.Windows.Controls.Orientation.Vertical
                            };

                            newOrientedPanel.Children.Add(floatingWindow.RootPanel);
                            newOrientedPanel.Children.Add(targetModel);

                            parentModel.InsertChildAt(insertToIndex, newOrientedPanel);
                        }
                    }
                    break;
                    #endregion
                case DropTargetType.AnchorablePaneDockLeft:
                    #region DropTargetType.AnchorablePaneDockLeft
                    {
                        var parentModel = targetModel.Parent as ILayoutGroup;
                        var parentModelOrientable = targetModel.Parent as ILayoutOrientableGroup;
                        int insertToIndex = parentModel.IndexOfChild(targetModel);

                        if (parentModelOrientable.Orientation != System.Windows.Controls.Orientation.Horizontal &&
                            parentModel.ChildrenCount == 1)
                            parentModelOrientable.Orientation = System.Windows.Controls.Orientation.Horizontal;

                        if (parentModelOrientable.Orientation == System.Windows.Controls.Orientation.Horizontal)
                        {
                            var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
                            if (layoutAnchorablePaneGroup != null &&
                                (layoutAnchorablePaneGroup.Children.Count == 1 ||
                                    layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal))
                            {
                                var anchorablesToMove = layoutAnchorablePaneGroup.Children.ToArray();
                                for (int i = 0; i < anchorablesToMove.Length; i++)
                                    parentModel.InsertChildAt(insertToIndex + i, anchorablesToMove[i]);
                            }
                            else
                                parentModel.InsertChildAt(insertToIndex, floatingWindow.RootPanel);
                        }
                        else
                        {
                            var newOrientedPanel = new LayoutAnchorablePaneGroup()
                            {
                                Orientation = System.Windows.Controls.Orientation.Horizontal
                            };

                            newOrientedPanel.Children.Add(floatingWindow.RootPanel);
                            newOrientedPanel.Children.Add(targetModel);

                            parentModel.InsertChildAt(insertToIndex, newOrientedPanel);
                        }
                    }
                    break;
                    #endregion
                case DropTargetType.AnchorablePaneDockRight:
                    #region DropTargetType.AnchorablePaneDockRight
                    {
                        var parentModel = targetModel.Parent as ILayoutGroup;
                        var parentModelOrientable = targetModel.Parent as ILayoutOrientableGroup;
                        int insertToIndex = parentModel.IndexOfChild(targetModel);

                        if (parentModelOrientable.Orientation != System.Windows.Controls.Orientation.Horizontal &&
                            parentModel.ChildrenCount == 1)
                            parentModelOrientable.Orientation = System.Windows.Controls.Orientation.Horizontal;

                        if (parentModelOrientable.Orientation == System.Windows.Controls.Orientation.Horizontal)
                        {
                            var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
                            if (layoutAnchorablePaneGroup != null &&
                                (layoutAnchorablePaneGroup.Children.Count == 1 ||
                                    layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal))
                            {
                                var anchorablesToMove = layoutAnchorablePaneGroup.Children.ToArray();
                                for (int i = 0; i < anchorablesToMove.Length; i++)
                                    parentModel.InsertChildAt(insertToIndex + 1 + i, anchorablesToMove[i]);
                            }
                            else
                                parentModel.InsertChildAt(insertToIndex + 1, floatingWindow.RootPanel);
                        }
                        else
                        {
                            var newOrientedPanel = new LayoutAnchorablePaneGroup()
                            {
                                Orientation = System.Windows.Controls.Orientation.Horizontal
                            };

                            newOrientedPanel.Children.Add(targetModel);
                            newOrientedPanel.Children.Add(floatingWindow.RootPanel);

                            parentModel.InsertChildAt(insertToIndex, newOrientedPanel);
                        }
                    }
                    break;
                    #endregion


                case DropTargetType.AnchorablePaneDockInside:
                    #region DropTargetType.AnchorablePaneDockInside
                    {
                        var paneModel = targetModel as LayoutAnchorablePane;
                        var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;

                        int i = _tabIndex == -1 ? 0 : _tabIndex;
                        foreach (var anchorableToImport in 
                            layoutAnchorablePaneGroup.Descendents().OfType<LayoutAnchorable>().ToArray())
                        {
                            paneModel.Children.Insert(i, anchorableToImport);
                            i++;
                        }
                    }
                    break;
                    #endregion


            }
            base.Drop(floatingWindow);
        }

        public override System.Windows.Media.Geometry GetPreviewPath(OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindowModel)
        {
            //var anchorablePaneDropTarget = target as AnchorablePaneDropTarget;
            var anchorableFloatingWindowModel = floatingWindowModel as LayoutAnchorableFloatingWindow;
            var layoutAnchorablePane = anchorableFloatingWindowModel.RootPanel as ILayoutPositionableElement;
            var layoutAnchorablePaneWithActualSize = anchorableFloatingWindowModel.RootPanel as ILayoutPositionableElementWithActualSize;

            switch (Type)
            {
                case DropTargetType.AnchorablePaneDockBottom:
                    {
                        var targetScreenRect = TargetElement.GetScreenArea();
                        targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

                        targetScreenRect.Offset(0.0, targetScreenRect.Height / 2.0);
                        targetScreenRect.Height /= 2.0;

                        return new RectangleGeometry(targetScreenRect);
                    }
                case DropTargetType.AnchorablePaneDockTop:
                    {
                        var targetScreenRect = TargetElement.GetScreenArea();
                        targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

                        targetScreenRect.Height /= 2.0;

                        return new RectangleGeometry(targetScreenRect);
                    }
                case DropTargetType.AnchorablePaneDockLeft:
                    {
                        var targetScreenRect = TargetElement.GetScreenArea();
                        targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

                        targetScreenRect.Width /= 2.0;

                        return new RectangleGeometry(targetScreenRect);
                    }
                case DropTargetType.AnchorablePaneDockRight:
                    {
                        var targetScreenRect = TargetElement.GetScreenArea();
                        targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

                        targetScreenRect.Offset(targetScreenRect.Width / 2.0, 0.0);
                        targetScreenRect.Width /= 2.0;

                        return new RectangleGeometry(targetScreenRect);
                    }
                case DropTargetType.AnchorablePaneDockInside:
                    {
                        var targetScreenRect = TargetElement.GetScreenArea();
                        targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

                        if (_tabIndex == -1)
                        {
                            return new RectangleGeometry(targetScreenRect);
                        }
                        else
                        {
                            var translatedDetectionRect = new Rect(DetectionRects[0].TopLeft, DetectionRects[0].BottomRight);
                            translatedDetectionRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
                            var pathFigure = new PathFigure();
                            pathFigure.StartPoint = targetScreenRect.TopLeft;
                            pathFigure.Segments.Add(new LineSegment() { Point = new Point(targetScreenRect.Left, translatedDetectionRect.Top) });
                            pathFigure.Segments.Add(new LineSegment() { Point = translatedDetectionRect.TopLeft });
                            pathFigure.Segments.Add(new LineSegment() { Point = translatedDetectionRect.BottomLeft });
                            pathFigure.Segments.Add(new LineSegment() { Point = translatedDetectionRect.BottomRight });
                            pathFigure.Segments.Add(new LineSegment() { Point = translatedDetectionRect.TopRight });
                            pathFigure.Segments.Add(new LineSegment() { Point = new Point(targetScreenRect.Right, translatedDetectionRect.Top) });
                            pathFigure.Segments.Add(new LineSegment() { Point = targetScreenRect.TopRight });
                            pathFigure.IsClosed = true;
                            pathFigure.IsFilled = true;
                            pathFigure.Freeze();
                            return new PathGeometry(new PathFigure[] { pathFigure });
                        }
                    }
            }

            return null;
        }
    }
}
