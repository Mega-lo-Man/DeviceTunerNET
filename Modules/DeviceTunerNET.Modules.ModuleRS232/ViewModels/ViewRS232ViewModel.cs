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
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DeviceTunerNET.Modules.ModuleRS232.ViewModels
{
    public partial class ViewRS232ViewModel : BindableBase
    {
        private string _message;
        private readonly ISerialTasks _serialTasks;
        private readonly Dispatcher _dispatcher;

        #region Commands

        public DelegateCommand ShiftAddressesCommand { get; }
        public DelegateCommand ScanNetworkCommand { get; }

        #endregion Commands

        #region Constructor
        public ViewRS232ViewModel(ISerialTasks serialTasks,
                                  
                                  IEventAggregator ea)
        {
            Title = "ПНР";

            _dispatcher = Dispatcher.CurrentDispatcher;

            _serialTasks = serialTasks;

            AvailableComPorts = _serialTasks.GetAvailableCOMPorts();

            dispatcher = Dispatcher.CurrentDispatcher;

            ScanNetworkCommand = new DelegateCommand(async () => await ScanNetworkCommandExecuteAsync(), ScanNetworkCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port);

            ShiftAddressesCommand = new DelegateCommand(async () => await ShiftAddressesCommandExecuteAsync(), ShiftAddressesCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port)
                .ObservesProperty(() => StartAddress);
        }
        #endregion Constructor
        private bool ScanNetworkCommandCanExecute()
        {
            return CurrentRS485Port != null;
        }

        private Task ScanNetworkCommandExecuteAsync()
        {
            return Task.Run(() =>
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
                var onlineDevices = c2000M.SearchOnlineDevices(UpdateProgressBar());
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
            });
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
