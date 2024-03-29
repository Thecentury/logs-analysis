﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
    [Serializable]
    public abstract class LayoutGroup<T> : LayoutElement, ILayoutContainer, ILayoutGroup, IXmlSerializable where T : class, ILayoutElement
    {
        internal LayoutGroup()
        {
            _children.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_children_CollectionChanged);
        }

        void _children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                {
                    foreach (LayoutElement element in e.OldItems)
                    {
                        if (element.Parent == this)
                            element.Parent = null;
                    }
                }
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                if (e.NewItems != null)
                {
                    foreach (LayoutElement element in e.NewItems)
                    {
                        if (element.Parent != this)
                        {
                            if (element.Parent != null)
                                element.Parent.RemoveChild(element);
                            element.Parent = this;
                        }
                        
                    }
                }
            }

            ComputeVisibility();
            OnChildrenCollectionChanged();
        }

        [field: NonSerialized]
        [field: XmlIgnore]
        public event EventHandler ChildrenCollectionChanged;

        protected virtual void OnChildrenCollectionChanged()
        {
            if (ChildrenCollectionChanged != null)
                ChildrenCollectionChanged(this, EventArgs.Empty);
        }

        ObservableCollection<T> _children = new ObservableCollection<T>();

        public ObservableCollection<T> Children
        {
            get { return _children; }
        }

        IEnumerable<ILayoutElement> ILayoutContainer.Children
        {
            get { return _children.Cast<ILayoutElement>(); }
        }


        #region IsVisible

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            protected set
            {
                if (_isVisible != value)
                {
                    RaisePropertyChanging("IsVisible");
                    _isVisible = value;
                    OnIsVisibleChanged();
                    RaisePropertyChanged("IsVisible");
                }
            }
        }

        protected virtual void OnIsVisibleChanged()
        { }

        public void ComputeVisibility()
        {
            IsVisible = GetVisibility();
        }

        protected abstract bool GetVisibility();

        #endregion


        public void MoveChild(int oldIndex, int newIndex)
        {
            _children.Move(oldIndex, newIndex);
        }

        public void RemoveChildAt(int childIndex)
        {
            _children.RemoveAt(childIndex);
        }

        public int IndexOfChild(ILayoutElement element)
        {
            return _children.Cast<ILayoutElement>().ToList().IndexOf(element);
        }

        public void InsertChildAt(int index, ILayoutElement element)
        {
            _children.Insert(index, (T)element);
        }

        public void RemoveChild(ILayoutElement element)
        {
            _children.Remove((T)element);
        }

        public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
        {
            int index = _children.IndexOf((T)oldElement);
            _children.Remove((T)oldElement);
            _children.Insert(index, (T)newElement);
        }

        public int ChildrenCount
        {
            get { return _children.Count; }
        }

        public void ReplaceChildAt(int index, ILayoutElement element)
        {
            _children[index] = (T)element;
        }


        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();
            if (reader.IsEmptyElement)
            {
                reader.Read();
                ComputeVisibility();
                return;
            }
            string localName = reader.LocalName;
            reader.Read();
            while (true)
            {
                if (reader.LocalName == localName &&
                    reader.NodeType == System.Xml.XmlNodeType.EndElement)
                {
                    break;
                }

                XmlSerializer serializer = null;
                if (reader.LocalName == "LayoutAnchorablePaneGroup")
                    serializer = new XmlSerializer(typeof(LayoutAnchorablePaneGroup));
                else if (reader.LocalName == "LayoutAnchorablePane")
                    serializer = new XmlSerializer(typeof(LayoutAnchorablePane));
                else if (reader.LocalName == "LayoutAnchorable")
                    serializer = new XmlSerializer(typeof(LayoutAnchorable));
                else if (reader.LocalName == "LayoutDocumentPaneGroup")
                    serializer = new XmlSerializer(typeof(LayoutDocumentPaneGroup));
                else if (reader.LocalName == "LayoutDocumentPane")
                    serializer = new XmlSerializer(typeof(LayoutDocumentPane));
                else if (reader.LocalName == "LayoutDocument")
                    serializer = new XmlSerializer(typeof(LayoutDocument));
                else if (reader.LocalName == "LayoutAnchorGroup")
                    serializer = new XmlSerializer(typeof(LayoutAnchorGroup));

                Children.Add((T)serializer.Deserialize(reader));
            }

            reader.ReadEndElement();
        }

        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var child in Children)
            {
                var type = child.GetType();
                XmlSerializer serializer = new XmlSerializer(type);
                serializer.Serialize(writer, child);
            }

        }
    }
}
