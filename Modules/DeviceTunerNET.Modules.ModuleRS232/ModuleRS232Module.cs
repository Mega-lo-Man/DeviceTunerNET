using DeviceTunerNET.Core;
using DeviceTunerNET.Modules.ModuleRS232.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DeviceTunerNET.Modules.ModuleRS232
{
    public class ModuleRS232Module : IModule
    {
        private readonly IRegionManager _regionManager;

        public ModuleRS232Module(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, "ViewRS232");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ViewRS232>();
        }
    }
}