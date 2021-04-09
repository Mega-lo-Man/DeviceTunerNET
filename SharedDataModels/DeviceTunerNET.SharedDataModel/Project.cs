using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel
{
    public class Project
    {
        private List<ConstructionSite> _constructionSites = new List<ConstructionSite>();
        

        public List<ConstructionSite> SetProject
        {
            get { return _constructionSites; }
            set { _constructionSites = value; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public List<ConstructionSite> GetAll()
        {
            return _constructionSites;
        }

        public void Add(ConstructionSite constructionSite)
        {
            _constructionSites.Add(constructionSite);
        }

        /*public bool Remove(ConstructionSite constructionSite)
        {

        }
        */
    }
}
