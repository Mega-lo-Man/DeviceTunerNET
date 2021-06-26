using System.Collections.Generic;

namespace DeviceTunerNET.SharedDataModel
{
    public class ConstructionSite
    {
        private List<Cabinet> _cabinets = new List<Cabinet>();

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _directoryPath;
        public string DirectoryPath
        {
            get { return _directoryPath; }
            set { _directoryPath = value; }
        }

        public List<Cabinet> GetAll()
        {
            return _cabinets;
        }

        public void SetConstructionSite(List<Cabinet> cabinets)
        {
            _cabinets = cabinets;
        }

        public void Add(Cabinet cabinet)
        {
            _cabinets.Add(cabinet);
        }
    }
}
