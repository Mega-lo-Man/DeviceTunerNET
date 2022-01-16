using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using static System.Int32;

namespace DeviceTunerNET.Modules.ModuleRS232.ViewModels
{
    public partial class ViewRS232ViewModel : BindableBase
    {
        #region Properties

        private ObservableCollection<string> _availableComPorts = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableComPorts
        {
            get => _availableComPorts;
            set => SetProperty(ref _availableComPorts, value);
        }

        private readonly Dispatcher dispatcher;
        private ObservableCollection<RS485device> _onlineDevicesList = new ObservableCollection<RS485device>();
        public ObservableCollection<RS485device> OnlineDevicesList
        {
            get => _onlineDevicesList;
            set => SetProperty(ref _onlineDevicesList, value);
        }

        private RS485device _selectedDevice;
        public RS485device SelectedDevice
        {
            get => _selectedDevice;
            set => _selectedDevice = value;
        }

        private string _currentRS485Port;
        public string CurrentRS485Port
        {
            get => _currentRS485Port;
            set => SetProperty(ref _currentRS485Port, value);
        }

        private int _startAddress;
        public string StartAddress
        {
            get => _startAddress.ToString();
            set => SetProperty(ref _startAddress, Parse(value));
        }

        private int _targetAddress;
        public string TargetAddress
        {
            get => _targetAddress.ToString();
            set => SetProperty(ref _targetAddress, Parse(value));
        }

        private int _addressRange;
        public string AddressRange
        {
            get => _addressRange.ToString();
            set => SetProperty(ref _addressRange, Parse(value));
        }
        

        public string Title { get; private set; }



        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        #endregion Properties
    }
}
