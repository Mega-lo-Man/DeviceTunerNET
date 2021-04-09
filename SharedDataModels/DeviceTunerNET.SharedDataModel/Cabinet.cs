using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DeviceTunerNET.SharedDataModel
{
    public class Cabinet : SimplestСomponent
    {
        private List<object> objLst = new List<object>(); // крнтейнер для хранения всех дивайсов внутри шкафа

        public IList<object> GetAllDevicesList
        {
            get
            {
                ObservableCollection<object> childNodes = new ObservableCollection<object>();

                foreach (var item in objLst)
                    childNodes.Add(item);

                return childNodes;
            }
        }

        #region common
        public IList<T> GetDevicesList<T>() where T : SimplestСomponent
        {
            List<T> lst = new List<T>();
            foreach (var item in objLst)
            {
                if (item.GetType() == typeof(T))
                { 
                    lst.Add((T)item); 
                }
            }
            return lst;
        }

        public void AddItem<T>(T arg) where T : SimplestСomponent
        {
            objLst.Add(arg);
        }

        public void ClearItems()
        {
            objLst.Clear();
        }
        #endregion
        private bool _isExpanded = false;
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    //SetProperty(ref _isExpanded, value);
                }
            }
        }


        
        private bool _isSelected = false;
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    //SetProperty(ref _isSelected, value);
                }
            }
        }
    }
}
