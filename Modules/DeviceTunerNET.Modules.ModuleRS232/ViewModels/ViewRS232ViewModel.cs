using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DeviceTunerNET.Modules.ModuleRS232.ViewModels
{
    public class ViewRS232ViewModel : BindableBase
    {
        private string _message;
        private readonly ISerialTasks _serialTasks;


        #region Commands
        
        public DelegateCommand ShiftAddressesCommand { get; private set; }
        public DelegateCommand ScanNetworkCommand { get; private set; }

        #endregion Commands

        #region Properties

        private ObservableCollection<string> _availableComPorts = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableComPorts
        {
            get { return _availableComPorts; }
            set { SetProperty(ref _availableComPorts, value); }
        }

        private readonly Dispatcher dispatcher;
        private ObservableCollection<RS485device> _onlineDevicesList = new ObservableCollection<RS485device>();
        public ObservableCollection<RS485device> OnlineDevicesList
        {
            get { return _onlineDevicesList; }
            set { SetProperty(ref _onlineDevicesList, value); }
        }

        private RS485device _selectedDevice;
        public RS485device SelectedDevice
        {
            get { return _selectedDevice; }
            set { _selectedDevice = value; }
        }

        private string _currentRS485Port;
        public string CurrentRS485Port
        {
            get { return _currentRS485Port; }
            set
            {
                SetProperty(ref _currentRS485Port, value);
            }
        }

        private int  _startAddress;
        public string StartAddress
        {
            get { return _startAddress.ToString(); }
            set { SetProperty(ref _startAddress, Int32.Parse(value)); }
        }

        private int _targetAddress;
        public string TargetAddress
        {
            get { return _targetAddress.ToString(); }
            set { SetProperty(ref _targetAddress, Int32.Parse(value)); }
        }

        private int _addressRange;
        public string AddressRange
        {
            get { return _addressRange.ToString(); }
            set { SetProperty(ref _addressRange, Int32.Parse(value)); }
        }
        #endregion Properties

        public string Title { get; private set; }

        

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        #region Constructor
        public ViewRS232ViewModel(IRegionManager regionManager,
                                  ISerialSender serialSender,
                                  ISerialTasks serialTasks,
                                  IEventAggregator ea)
        {
            Title = "ПНР";

            _serialTasks = serialTasks;

            AvailableComPorts = _serialTasks.GetAvailableCOMPorts();

            dispatcher = Dispatcher.CurrentDispatcher;

            ScanNetworkCommand = new DelegateCommand(async () => await ScanNetworkCommandExecuteAsync(), ScanNetworkCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port); 

            ShiftAddressesCommand = new DelegateCommand(async () => await ShiftAddressesCommandExecuteAsync(), ShiftAddressesCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port)
                .ObservesProperty(() => StartAddress);
        }

        private bool ScanNetworkCommandCanExecute()
        {
            if (CurrentRS485Port != null)
                return true;
            return false;
        }

        private Task ScanNetworkCommandExecuteAsync()
        {
            return Task.Run(() =>
            {
                var enumerator = _serialTasks.GetOnlineDevices(CurrentRS485Port);
                dispatcher.Invoke(() =>
                {
                    OnlineDevicesList.Clear();
                });
                foreach (var item in enumerator)
                {
                    dispatcher.Invoke(() =>
                    {
                        OnlineDevicesList.Add(item);
                    });
                }
            });
        }

        private bool ShiftAddressesCommandCanExecute()
        {
            if (CurrentRS485Port != null && StartAddress.Length > 0) return true;
            return false;
        }

        private Task ShiftAddressesCommandExecuteAsync()
        {
            return Task.Run(() =>
            {
                if (SelectedDevice.AddressRS485 != null)
                {
                    _serialTasks.ShiftDevicesAddresses(CurrentRS485Port,
                                                   (int)SelectedDevice.AddressRS485,
                                                   _targetAddress,
                                                   _addressRange);
                }
            });
        }

        #endregion Constructor


    }
}
