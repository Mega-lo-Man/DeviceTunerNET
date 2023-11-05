using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.Ports;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Windows;
using System.Windows.Threading;

namespace DeviceTunerNET.Modules.ModulePnr.ViewModels
{
    public partial class ViewPnrViewModel : BindableBase
    {
        private string _message;
        private MyCancellationTokenSource _token;
        private SerialPort _port;
        private readonly ISerialTasks _serialTasks;
        private readonly IDeviceGenerator _deviceGenerator;
        private readonly Dispatcher _dispatcher;

        #region Commands

        public DelegateCommand WaitingNewDeviceCommand { get; }
        public DelegateCommand StopWaitingNewDeviceCommand { get; }
        public DelegateCommand<ViewOnlineDeviceViewModel> ChangeAddressCommand { get; }
        public DelegateCommand<ViewOnlineDeviceViewModel> SetFirstFreeAddressCommand { get; }
        public DelegateCommand ShiftAddressesCommand { get; }
        public DelegateCommand CheckedScanNetworkCommand { get; }
        public DelegateCommand UncheckedScanNetworkCommand { get; }

        #endregion Commands

        #region Constructor
        public ViewPnrViewModel(ISerialTasks serialTasks,
                                  IDeviceGenerator deviceGenerator,
                                  IEventAggregator ea)
        {
            Title = "ПНР";

            _dispatcher = Dispatcher.CurrentDispatcher;

            _serialTasks = serialTasks;
            _deviceGenerator = deviceGenerator;

            AvailableComPorts = _serialTasks.GetAvailableCOMPorts();
            CurrentRS485Port = AvailableComPorts.LastOrDefault();

            dispatcher = Dispatcher.CurrentDispatcher;

            SetFirstFreeAddressCommand = new DelegateCommand<ViewOnlineDeviceViewModel>(async (param) => await SetFirstFreeAddressCommandExecuteAsync(param))
                .ObservesProperty(() => CurrentRS485Port);

            WaitingNewDeviceCommand = new DelegateCommand(async () => await WaitingNewDeviceCommandExecuteAsync(), WaitingNewDeviceCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port);

            StopWaitingNewDeviceCommand = new DelegateCommand(async () => await StopWaitingNewDeviceCommandExecuteAsync(), StopWaitingNewDeviceCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port);

            ChangeAddressCommand = new DelegateCommand<ViewOnlineDeviceViewModel>(async (param) => await ChangeAddressCommandExecuteAsync(param))
                .ObservesProperty(() => CurrentRS485Port);

            CheckedScanNetworkCommand = new DelegateCommand(async () => await CheckedScanNetworkCommandExecuteAsync(), CheckedScanNetworkCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port);

            UncheckedScanNetworkCommand = new DelegateCommand(UncheckedScanNetworkCommandExecuteAsync, UncheckedScanNetworkCommandCanExecute);

            ShiftAddressesCommand = new DelegateCommand(async () => await ShiftAddressesCommandExecuteAsync(), ShiftAddressesCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port)
                .ObservesProperty(() => StartAddress);
        }

        private Task SetFirstFreeAddressCommandExecuteAsync(ViewOnlineDeviceViewModel param)
        {
            TryToCancelCurrentNetWorking();
            return Task.Run(() => { ChangeAddressToFirstFree(param); });
        }

        private bool StopWaitingNewDeviceCommandCanExecute()
        {
            return true;
        }

        private Task StopWaitingNewDeviceCommandExecuteAsync()
        {
            TryToCancelCurrentNetWorking();
            CanDoStartScan = true;

            return Task.Run(() => { });
        }

        private bool WaitingNewDeviceCommandCanExecute()
        {
            return CurrentRS485Port != null;
        }

        private Task WaitingNewDeviceCommandExecuteAsync()
        {
            CanDoStartScan = false;
            _token = new MyCancellationTokenSource();
            var token = _token.Token;
            return Task.Run(() => { WaitingNewDeviceLoop(token); });
        }

        private Task ChangeAddressCommandExecuteAsync(ViewOnlineDeviceViewModel param)
        {
            TryToCancelCurrentNetWorking();
            return Task.Run(() => { ChangeAddress(param); });
        }

        private bool UncheckedScanNetworkCommandCanExecute()
        {
            return true;
        }

        private void UncheckedScanNetworkCommandExecuteAsync()
        {
            TryToCancelCurrentNetWorking();

            CanDoStartWaiting = true;
        }
        #endregion Constructor
        private bool CheckedScanNetworkCommandCanExecute()
        {
            return CurrentRS485Port != null;
        }

        private Task CheckedScanNetworkCommandExecuteAsync()
        {
            CanDoStartWaiting = false;

            _token = new MyCancellationTokenSource();
            var token = _token.Token;
            return Task.Run(() => SearchDevices(token), token);
        }

        private void WaitingNewDeviceLoop(CancellationToken token)
        {
            _port = new SerialPort(CurrentRS485Port);
            var result = TryOpenSerialPort(_port, out var port);
            if (!result)
                return;

            dispatcher.Invoke(() =>
            {
                OnlineDevicesList.Clear();
            });

            try
            {

                var c2000M = new C2000M(port);

                while (!_token.IsCancellationRequested)
                {
                    WaitDefaultDevice(c2000M);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _port.Close();
                _port.Dispose();
            }
            
        }

        private void WaitDefaultDevice(IOrionDevice c2000M)
        {
            var response = c2000M.GetModelCode(127, out var deviceCode);
            if(response)
            {
                if (!_deviceGenerator.TryGetDeviceByCode(deviceCode, out var orionDevice))
                    return;

                var busyAddresses = OnlineDevicesList.Select(o => o.Device.AddressRS485).ToList();

                orionDevice.Port = c2000M.Port;
                orionDevice.AddressRS485 = GetFirstMissing(busyAddresses);
                orionDevice.SetAddress();

                dispatcher.Invoke(() =>
                {
                    OnlineDevicesList.Add(new ViewOnlineDeviceViewModel(orionDevice));
                });
            }
        }

        private void ChangeAddress(ViewOnlineDeviceViewModel onlineDeviceViewModel)
        {
            var result = TryOpenSerialPort(_port, out var port);
            if (!result)
                return;

            var currentDevice = onlineDeviceViewModel.Device;

            currentDevice.Port = port;
            var allAddresses = OnlineDevicesList.Select(o => o.Device.AddressRS485).ToList();

            if (allAddresses.Contains(onlineDeviceViewModel.Address))
            {
                MessageBox.Show("Address " + onlineDeviceViewModel.Address + " is busy!");
                onlineDeviceViewModel.Refresh();
                return;
            }
            if (onlineDeviceViewModel.Address <= 0 || onlineDeviceViewModel.Address > 127)
            {
                MessageBox.Show("Address is wrong!");
                return;
            }
            TryToChangeDeviceAddress(onlineDeviceViewModel.Address, currentDevice);
        }

        private void ChangeAddressToFirstFree(ViewOnlineDeviceViewModel onlineDeviceViewModel)
        {
            var result = TryOpenSerialPort(_port, out var port);
            if (!result)
                return;

            var currentDevice = onlineDeviceViewModel.Device;

            currentDevice.Port = port;
            var allAddresses = OnlineDevicesList.Select(o => o.Device.AddressRS485).ToList();
            var newAddress = GetFirstMissing(allAddresses);
            TryToChangeDeviceAddress(newAddress, currentDevice);
            dispatcher.Invoke(() =>
            {
                onlineDeviceViewModel.Refresh();
            });
        }

        private void TryToChangeDeviceAddress(uint address, IOrionDevice currentDevice)
        {
            bool changeAddressSuccess = false;
            try
            {
                changeAddressSuccess = currentDevice.ChangeDeviceAddress((byte)address);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                StopWaitingNewDeviceCommandExecuteAsync();
                UncheckedScanNetworkCommandExecuteAsync();
                _port.Close();
                _port.Dispose();
            }
            if (changeAddressSuccess)
            {
                currentDevice.AddressRS485 = address;
            }
        }


        private void SearchDevices(CancellationToken token)
        {
            _port = new SerialPort(CurrentRS485Port);

            var result = TryOpenSerialPort(_port, out var port);
            if (!result)
                return;

            var c2000M = new C2000M(port);
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
            port.SerialPort.Close();
            port.SerialPort.Dispose();
            ScanSliderIsChecked = false;
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

        private bool TryOpenSerialPort(SerialPort serialPort, out ComPort port)
        {
            var counter = 0;
            port = default;
            serialPort ??= new SerialPort(CurrentRS485Port);

            while (serialPort.IsOpen && counter < 10)
            {
                Thread.Sleep(500);
                counter++;
            }
            
            try
            {
                serialPort.Open();
            }
            catch
            {
                MessageBox.Show("Port " + serialPort.PortName + " is busy.");
                return false;
            }
            var comPort = new ComPort
            {
                SerialPort = serialPort
            };
            port = comPort;
            return true;
        }

        private void TryToCancelCurrentNetWorking()
        {
            if (!_token.IsDisposed)
            {
                _token?.Cancel();
                _token?.Dispose();
            }
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

        private static uint GetFirstMissing(IEnumerable<uint> numbers)
        {
            for (uint i = 1; i < 127;  i++)
            {
                if(!numbers.Contains(i))
                    return i;
            }
            return 127;
        }
    }
}
