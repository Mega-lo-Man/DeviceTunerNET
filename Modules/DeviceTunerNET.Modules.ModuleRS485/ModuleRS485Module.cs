using DeviceTunerNET.Core;
using DeviceTunerNET.Modules.ModuleRS485.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DeviceTunerNET.Modules.ModuleRS485
{
    public class ModuleRS485Module : IModule
    {
        private readonly IRegionManager _regionManager;

        public ModuleRS485Module(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, "ViewRS485");

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ViewRS485>();

        }
    }
}