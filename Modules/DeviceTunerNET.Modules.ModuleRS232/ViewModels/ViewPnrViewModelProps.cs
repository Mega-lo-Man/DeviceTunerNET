using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static System.Int32;

namespace DeviceTunerNET.Modules.ModulePnr.ViewModels
{
    public partial class ViewPnrViewModel : BindableBase
    {
        #region Properties

        private ObservableCollection<string> _availableComPorts = new();
        public ObservableCollection<string> AvailableComPorts
        {
            get => _availableComPorts;
            set => SetProperty(ref _availableComPorts, value);
        }

        private ObservableCollection<ViewOnlineDeviceViewModel> _onlineDevicesList = new();
        public ObservableCollection<ViewOnlineDeviceViewModel> OnlineDevicesList
        {
            get => _onlineDevicesList;
            set => SetProperty(ref _onlineDevicesList, value);
        }

        public ObservableCollection<ViewSingleRelayViewModel> RelayViewModels { get; set; } = new();

        public ObservableCollection<ViewSingleShleifViewModel> ShleifViewModels { get; set; } = new();

        private ViewOnlineDeviceViewModel _selectedDevice;
        public ViewOnlineDeviceViewModel SelectedDevice
        {
            get => _selectedDevice;
            set
            { 
                _selectedDevice = value;
                PresentSelectedDevice();
            }
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

        private int _searchProgressBar;
        public int SearchProgressBar
        {
            get => _searchProgressBar;
            set => SetProperty(ref _searchProgressBar, value);
        }

        private bool _isCheckedSearching = false;
        public bool IsCheckedSearching
        {
            get => _isCheckedSearching;
            set => SetProperty(ref _isCheckedSearching, value);
        }

        private bool _isCheckedWaiting = true;
        public bool IsCheckedWaiting
        {
            get => _isCheckedWaiting;
            set => SetProperty(ref _isCheckedWaiting, value);
        }

        private bool _scanSliderIsChecked = false;
        public bool ScanSliderIsChecked
        {
            get => _scanSliderIsChecked;
            set => SetProperty(ref _scanSliderIsChecked, value);
        }

        private bool _canDoStartScan = true;
        public bool IsSliderEnable
        {
            get => _canDoStartScan;
            set
            {
                SetProperty(ref _canDoStartScan, value);
            }
        }

        private bool _isAddressChangeButtonsEnable = false;
        public bool IsAddressChangeButtonsEnable
        {
            get => _isAddressChangeButtonsEnable;
            set => SetProperty(ref _isAddressChangeButtonsEnable, value);
        }

        private bool _canDoStartWaiting = true;
        public bool CanDoStartWaiting
        {
            get => _canDoStartWaiting;
            set
            {
                WaitingNewDeviceCommand.RaiseCanExecuteChanged();
                SetProperty(ref _canDoStartWaiting, value);
            }
        }

        private bool _isModeSwitchEnable;
        public bool IsModeSwitchEnable
        {
            get => _isModeSwitchEnable;
            set
            {
                SetProperty(ref _isModeSwitchEnable, value);
            }
        }

        private bool _isProgressIndeterminate = false;
        public bool IsProgressIndeterminate
        {
            get => _isProgressIndeterminate;
            set
            {
                SetProperty(ref _isProgressIndeterminate, value);
            }
        }

        #endregion Properties
    }
}
