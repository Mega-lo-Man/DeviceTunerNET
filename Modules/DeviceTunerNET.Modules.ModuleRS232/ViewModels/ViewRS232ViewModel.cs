using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.Ports;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DeviceTunerNET.Modules.ModuleRS232.ViewModels
{
    public partial class ViewRS232ViewModel : BindableBase
    {
        private string _message;
        private CancellationTokenSource _tokenSource;
        private readonly ISerialTasks _serialTasks;
        private readonly Dispatcher _dispatcher;

        #region Commands

        public DelegateCommand ChangeAddressCommand { get; }
        public DelegateCommand ShiftAddressesCommand { get; }
        public DelegateCommand CheckedScanNetworkCommand { get; }
        public DelegateCommand UncheckedScanNetworkCommand { get; }

        #endregion Commands

        #region Constructor
        public ViewRS232ViewModel(ISerialTasks serialTasks,
                                  IEventAggregator ea)
        {
            Title = "ПНР";

            _dispatcher = Dispatcher.CurrentDispatcher;

            _serialTasks = serialTasks;

            AvailableComPorts = _serialTasks.GetAvailableCOMPorts();
            CurrentRS485Port = AvailableComPorts.LastOrDefault();

            dispatcher = Dispatcher.CurrentDispatcher;

            ChangeAddressCommand = new DelegateCommand(async () => await ChangeAddressCommandExecuteAsync(), ChangeAddressCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port);

            CheckedScanNetworkCommand = new DelegateCommand(async () => await CheckedScanNetworkCommandExecuteAsync(), CheckedScanNetworkCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port);

            UncheckedScanNetworkCommand = new DelegateCommand(UncheckedScanNetworkCommandExecuteAsync, UncheckedScanNetworkCommandCanExecute);

            ShiftAddressesCommand = new DelegateCommand(async () => await ShiftAddressesCommandExecuteAsync(), ShiftAddressesCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port)
                .ObservesProperty(() => StartAddress);
        }

        private bool ChangeAddressCommandCanExecute()
        {
            throw new NotImplementedException();
        }

        private Task ChangeAddressCommandExecuteAsync()
        {
            throw new NotImplementedException();
        }

        private bool UncheckedScanNetworkCommandCanExecute()
        {
            return true;
        }

        private void UncheckedScanNetworkCommandExecuteAsync()
        {
            _tokenSource?.Cancel();
        }
        #endregion Constructor
        private bool CheckedScanNetworkCommandCanExecute()
        {
            return CurrentRS485Port != null;
        }

        private Task CheckedScanNetworkCommandExecuteAsync()
        {
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            return Task.Run(() => SearchDevices(token), token);
        }

        private void SearchDevices(CancellationToken token)
        {
            var port = new SerialPort(CurrentRS485Port);
            try
            {
                port.Open();
            }
            catch
            {
                return;
            }
            var comPort = new ComPort
            {
                SerialPort = port
            };

            var c2000M = new C2000M(comPort);
            var onlineDevices = c2000M.SearchOnlineDevices(UpdateProgressBar(), token);
            dispatcher.Invoke(() =>
            {
                OnlineDevicesList.Clear();
            });
            foreach (var item in onlineDevices)
            {
                dispatcher.Invoke(() =>
                {
                    OnlineDevicesList.Add(new ViewOnlineDeviceViewModel(item));
                });
            }
            comPort.SerialPort.Close();
            SliderIsChecked = false;
        }

        private bool ShiftAddressesCommandCanExecute()
        {
            return CurrentRS485Port != null 
                   && StartAddress.Length > 0 
                   && OnlineDevicesList.Count > 0 
                   && _addressRange > 0;
        }
        
        private Task ShiftAddressesCommandExecuteAsync()
        {
            return Task.Run(() =>
            {
                if (SelectedDevice.Device.AddressRS485 == null)
                {
                    return;
                }
                _serialTasks.ShiftDevicesAddresses(CurrentRS485Port,
                                               (int)SelectedDevice.Device.AddressRS485,
                                               _targetAddress,
                                               _addressRange);
            });
        }

        private Action<int> UpdateProgressBar()
        {
            return val =>
            {
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    SearchProgressBar = val;
                }));
            };
        }
    }
}
