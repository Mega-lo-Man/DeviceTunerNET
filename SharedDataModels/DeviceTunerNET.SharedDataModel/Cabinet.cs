using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeviceTunerNET.SharedDataModel
{
    public class Cabinet : SimplestСomponent, ISimplestComponent
    {
        private List<ISimplestComponent> objLst = new(); // контейнер для хранения всех дивайсов внутри шкафа

        public IList<ISimplestComponent> GetAllDevicesList
        {
            get
            {
                var childNodes = new List<ISimplestComponent>();

                foreach (var item in objLst)
                {
                    childNodes.Add(item);
                }

                return childNodes;
            }
        }

        #region common
        public IList<T> GetDevicesList<T>() where T : ISimplestComponent
        {
            List<T> lst = new List<T>();
            foreach (var item in objLst)
            {
                if (item is T component)
                {
                    lst.Add(component);
                }
            }
            return lst;
        }

        public void AddItem<T>(T arg) where T : ISimplestComponent
        {
            objLst.Add(arg);
        }

        public void AddItems<T>(IEnumerable<T> args) where T : ISimplestComponent
        {
            objLst.AddRange((IEnumerable<ISimplestComponent>)args);
        }

        public void ClearItems()
        {
            objLst.Clear();
        }
        #endregion
    }
}
