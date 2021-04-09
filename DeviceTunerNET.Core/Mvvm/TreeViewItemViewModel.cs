using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Core.Mvvm
{
    /// <summary>
    /// Базовый класс для всех представлений в TreeView
    /// Он действует как адаптер между объектом модели и TreeViewItem.
    /// </summary>
    public class TreeViewItemViewModel : ViewModelBase
    {
        static readonly TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();
        private TreeViewItemViewModel _parent;
        private readonly ObservableCollection<TreeViewItemViewModel> _children;

        #region Constructor

        public TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren)
        {
            _parent = parent;
            _children = new ObservableCollection<TreeViewItemViewModel>();

            if (lazyLoadChildren)
            {
                _children.Add(DummyChild);
            }
        }

        public TreeViewItemViewModel()
        {
        }

        #endregion Constructor

        /// <summary>
        /// Возвращает дочерние элементы узла TreeView
        /// </summary>
        public ObservableCollection<TreeViewItemViewModel> Children
        {
            get { return _children; }
        }

        /// <summary>
        /// Возвращает true, если дочерние объекты этого объекта еще не заполнены.
        /// </summary>
        public bool HasDummyChild
        {
            get { return Children.Count == 1 && Children[0] == DummyChild; }
        }


        private bool _isExpanded;
        /// <summary>
        /// Свойство обрабатывающее разворачивание списка
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    SetProperty(ref _isExpanded, value);
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                {
                    _parent._isExpanded = true;
                }

                // Lazy load the child items, if necessary.
                if (HasDummyChild)
                {
                    Children.Remove(DummyChild);
                    LoadChildren();
                }
            }
        }

        private object _selectedItem = null;
        // This is public get-only here but you could implement a public setter which
        // also selects the item.
        // Also this should be moved to an instance property on a VM for the whole tree, 
        // otherwise there will be conflicts for more than one tree.
        public object SelectedItem
        {
            get { return _selectedItem; }
            private set
            {
                //if(_selectedItem != value)
                //{
                _selectedItem = value;
                OnSelectedItemChanged();
                //}
            }
        }

        protected virtual void OnSelectedItemChanged()
        {
        }

        private bool _isSelected;
        /// <summary>
        /// Свойство обрабатывающее выделение элемента в дереве
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetProperty(ref _isSelected, value);
                if (_isSelected)
                {
                    SelectedItem = this;
                }
            }
        }

        /// <summary>
        /// Вызывается, когда дочерние элементы необходимо загрузить по запросу.
        /// Подклассы могут переопределить этот метод, чтобы заполнить коллекцию Children.
        /// </summary>
        protected virtual void LoadChildren()
        {
        }

        public TreeViewItemViewModel Parent
        {
            get { return _parent; }
        }
    }
}
