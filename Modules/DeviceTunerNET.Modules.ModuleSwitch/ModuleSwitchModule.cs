using DeviceTunerNET.Core;
using DeviceTunerNET.Modules.ModuleSwitch.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DeviceTunerNET.Modules.ModuleSwitch
{
    public class ModuleSwitchModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public ModuleSwitchModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, "ViewSwitch");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ViewSwitch>();
        }
    }
}