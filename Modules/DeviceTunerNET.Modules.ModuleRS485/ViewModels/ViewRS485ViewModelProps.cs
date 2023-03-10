using DeviceTunerNET.Core;
using DeviceTunerNET.Core.Mvvm;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Prism.Services.Dialogs;
using DeviceTunerNET.SharedDataModel.Devices;

namespace DeviceTunerNET.Modules.ModuleRS485.ViewModels
{
    public partial class ViewRS485ViewModel : RegionViewModelBase
    {
        
        #region Properties

        private VerificationStart VerificationCanStart { get; set; }

        private int _deviceStatus;
        public int DeviceStatus
        {
            get => _deviceStatus;
            set => SetProperty(ref _deviceStatus, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private string _remoteDefaultFirstIP;
        public string RemoteDefaultFirstIP
        {
            get => _remoteDefaultFirstIP;
            set => SetProperty(ref _remoteDefaultFirstIP, value);
        }

        private string _ipMask = "255.255.252.0";
        public string IPMask
        {
            get => _ipMask;
            set => SetProperty(ref _ipMask, value);
        }

        private int _defaultRS485Address = 127;
        public int DefaultRS485Address
        {
            get => _defaultRS485Address;
            set
            {
                if (value <= 127)
                {
                    SetProperty(ref _defaultRS485Address, value);
                }
            }
        }

        private string _currentDeviceModel = "";
        public string CurrentDeviceModel
        {
            get => _currentDeviceModel;
            set => SetProperty(ref _currentDeviceModel, value);
        }

        private string _serialTextBox = "";
        public string SerialTextBox
        {
            get => _serialTextBox;
            set => SetProperty(ref _serialTextBox, value);
        }

        private bool _isCheckedByCabinets;
        public bool IsCheckedByCabinets
        {
            get => _isCheckedByCabinets;
            set
            {
                if (value)
                {
                    DevicesForProgramming.Clear(); // При переключении режима работы надо очистить список приборов для программирования
                    StartButtonVisibilty = true; // и показать кнопку DownloadAddressButton, если погашена.
                }

                SetProperty(ref _isCheckedByCabinets, value);
            }
        }

        private bool _isCheckedByArea;
        public bool IsCheckedByArea
        {
            get => _isCheckedByArea;
            set
            {
                if (value)
                {
                    DevicesForProgramming.Clear();// При переключении режима работы надо очистить список приборов для программирования

                    foreach (var item in _dataRepositoryService.GetAllDevices<RS485device>())
                    {
                        DevicesForProgramming.Add(item);
                    }
                    foreach (var item in _dataRepositoryService.GetAllDevices<C2000Ethernet>())
                    {
                        DevicesForProgramming.Add(item);
                    }
                    //CollectionViewSource.GetDefaultView(DevicesForProgramming).Refresh();
                    StartButtonVisibilty = true; // показать кнопку DownloadAddressButton, если погашена.
                }

                SetProperty(ref _isCheckedByArea, value);
            }
        }

        private bool _isCheckedComplexVerification = true;
        public bool IsCheckedComplexVerification
        {
            get => _isCheckedComplexVerification;
            set
            {
                if (value)
                {
                    DevicesForProgramming.Clear();// При переключении режима работы надо очистить список приборов для программирования
                    StartButtonVisibilty = false; // и скрыть кнопку DownloadAddressButton и показать кнопку CheckButton (она покажется с помощью св-ва IsCheckedComplexVerification.
                }
                SetProperty(ref _isCheckedComplexVerification, value);
            }
        }

        private string _currentRS485Port;
        public string CurrentRS485Port
        {
            get => _currentRS485Port;
            set => SetProperty(ref _currentRS485Port, value);
        }

        private ObservableCollection<Cabinet> _cabinetList = new();
        public ObservableCollection<Cabinet> CabinetList
        {
            get => _cabinetList;
            set => SetProperty(ref _cabinetList, value);
        }

        private ObservableCollection<CabinetViewModel> _cabsVM = new();
        public ObservableCollection<CabinetViewModel> CabsVM
        {
            get => _cabsVM;
            set => SetProperty(ref _cabsVM, value);
        }

        private ObservableCollection<object> _devicesForProgramming = new();
        public ObservableCollection<object> DevicesForProgramming
        {
            get => _devicesForProgramming;
            set => SetProperty(ref _devicesForProgramming, value);
        }

        private ObservableCollection<string> _availableComPorts = new();
        public ObservableCollection<string> AvailableComPorts
        {
            get => _availableComPorts;
            set => SetProperty(ref _availableComPorts, value);
        }

        private int _searchProgressBar;
        public int SearchProgressBar
        {
            get => _searchProgressBar;
            set => SetProperty(ref _searchProgressBar, value);
        }

        private bool _startButtonVisibility = true;
        public bool StartButtonVisibilty
        {
            get => _startButtonVisibility;
            set => SetProperty(ref _startButtonVisibility, value);
        }

        private bool _startButtonEnable = true;
        public bool StartButtonEnable
        {
            get => _startButtonEnable;
            set => SetProperty(ref _startButtonEnable, value);
        }

        public ObservableCollection<string> AvailableProtocols { get; set; } = new();

        private string _currentProtocol;
        public string CurrentProtocol 
        { 
            get => _currentProtocol; 
            set => SetProperty(ref _currentProtocol, value); 
        }

        #endregion


    }
}
