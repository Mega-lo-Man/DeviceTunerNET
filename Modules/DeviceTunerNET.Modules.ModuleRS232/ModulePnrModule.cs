using DeviceTunerNET.Core;
using DeviceTunerNET.Modules.ModulePnr.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DeviceTunerNET.Modules.ModulePnr
{
    public class ModulePnrModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public ModulePnrModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, "ViewPnr");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ViewPnr>();           
        }
    }
}