using DeviceTunerNET.Modules.ModulePnr.Interfaces;
using Prism.Commands;
using Prism.Mvvm;

namespace DeviceTunerNET.Modules.ModulePnr.ViewModels
{
    public class ViewSingleCommon : BindableBase, IControlViewModel
    {
        private bool _isControlEnabled = false;
        public bool IsControlEnabled
        {
            get => _isControlEnabled;
            set
            {
                _isControlEnabled = value;
                SetProperty(ref _isControlEnabled, value);
            }
        }

        public DelegateCommand<ViewOnlineDeviceViewModel> RebootDevice { get; }
    }
}
