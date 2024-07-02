using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Modules.ModulePnr.ViewModels.DeviceViewModels
{
    public class C2000_2ViewModel : BindableBase, INavigationAware
    {
        public C2000_2ViewModel()
        {
        }

        public string Model { get; set; } = "poiuytrewqlkjhgfdsazxcvbnm";

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Model = ((ViewOnlineDeviceViewModel)navigationContext.Parameters["SelectedDevice"]).Model;
        }
    }
}
