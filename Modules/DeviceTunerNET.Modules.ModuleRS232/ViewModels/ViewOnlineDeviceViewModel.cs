using DeviceTunerNET.SharedDataModel;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace DeviceTunerNET.Modules.ModuleRS232.ViewModels
{
    public class ViewOnlineDeviceViewModel : BindableBase
    {
        public IOrionDevice Device { get; private set; }
        
        #region Props

        private string _model;
        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        private uint _address;
        public uint Address
        {
            get => _address;
            set
            {
                if (value <= 0 && value > 127)
                    return;

                if (_address == value) return;

                SetProperty(ref _address, value);
            }
        }

        #endregion Props

        public void Refresh()
        {
            Model = (Device.SupportedModels == null) ? Model : string.Join("; ", Device.SupportedModels);
            Address = Device.AddressRS485;
        }

        public ViewOnlineDeviceViewModel(IOrionDevice device)
        {
            Device = device;
            Refresh();
        }
    }
}
