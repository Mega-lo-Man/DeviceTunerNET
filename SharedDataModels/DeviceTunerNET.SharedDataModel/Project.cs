using System.Collections.Generic;

namespace DeviceTunerNET.SharedDataModel
{
    public class Project
    {
        public List<ConstructionSite> SetProject { get; set; } = new List<ConstructionSite>();

        public string Name { get; set; }

        public List<ConstructionSite> GetAll()
        {
            return SetProject;
        }

        public void Add(ConstructionSite constructionSite)
        {
            SetProject.Add(constructionSite);
        }
    }
}
