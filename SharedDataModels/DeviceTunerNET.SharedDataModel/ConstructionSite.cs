using System.Collections.Generic;

namespace DeviceTunerNET.SharedDataModel
{
    public class ConstructionSite
    {
        private List<Cabinet> _cabinets = new List<Cabinet>();

        public string Name { get; set; }

        public string DirectoryPath { get; set; }

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
