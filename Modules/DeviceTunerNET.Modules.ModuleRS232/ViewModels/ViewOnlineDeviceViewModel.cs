using DeviceTunerNET.SharedDataModel;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Modules.ModuleRS232.ViewModels
{
    public class ViewOnlineDeviceViewModel : BindableBase
    {
        public IRS485device Device { get; private set; }
        
        #region Props

        private string _model;
        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        private string _address;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        #endregion Props

        public ViewOnlineDeviceViewModel(IRS485device device)
        {
            Device = device;
            Model = string.Join("; ", Device.SupportedModels);
            Address = Device.AddressRS485.ToString();
        }
    }
}
