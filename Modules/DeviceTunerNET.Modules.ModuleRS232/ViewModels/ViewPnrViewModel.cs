using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.ElectricModules;
using DeviceTunerNET.SharedDataModel.Ports;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Media;
using System.Reflection;
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

            IsModeSwitchEnable = true;

            ButtonLabels = new ObservableCollection<string>
            {
            };

            SetFirstFreeAddressCommand = new DelegateCommand<ViewOnlineDeviceViewModel>(async (param) => await SetFirstFreeAddressCommandExecuteAsync(param))
                .ObservesProperty(() => CurrentRS485Port);

            ChangeAddressCommand = new DelegateCommand<ViewOnlineDeviceViewModel>(async (param) => await ChangeAddressCommandExecuteAsync(param))
                .ObservesProperty(() => CurrentRS485Port);

            CheckedScanNetworkCommand = new DelegateCommand(async () => await CheckedScanNetworkCommandExecuteAsync(), CheckedScanNetworkCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port)
                .ObservesProperty(() => IsModeSwitchEnable);

            UncheckedScanNetworkCommand = new DelegateCommand(UncheckedScanNetworkCommandExecuteAsync, UncheckedScanNetworkCommandCanExecute);

            ShiftAddressesCommand = new DelegateCommand(async () => await ShiftAddressesCommandExecuteAsync(), ShiftAddressesCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port)
                .ObservesProperty(() => StartAddress);
        }
        #endregion Constructor

        private Task SetFirstFreeAddressCommandExecuteAsync(ViewOnlineDeviceViewModel param)
        {
            IsAddressChangeButtonsEnable = false;
            IsSliderEnable = false;
            return Task.Run(() =>
            {
                ChangeAddressToFirstFree(param);
                IsAddressChangeButtonsEnable = true;
                IsSliderEnable = true;
                IsModeSwitchEnable = true;
            });
        }

        private Task ChangeAddressCommandExecuteAsync(ViewOnlineDeviceViewModel param)
        {
            IsAddressChangeButtonsEnable = false;
            IsSliderEnable = false;

            return Task.Run(() =>
            {
                ChangeAddress(param);
                IsAddressChangeButtonsEnable = true;
                IsSliderEnable = true;
                IsModeSwitchEnable = true;
            });
        }

        private bool UncheckedScanNetworkCommandCanExecute()
        {
            return true;
        }

        private void UncheckedScanNetworkCommandExecuteAsync()
        {
            IsSliderEnable = false;
            TryToCancelCurrentNetWorking();
        }
        
        private bool CheckedScanNetworkCommandCanExecute()
        {
            return CurrentRS485Port != null;
        }

        private Task CheckedScanNetworkCommandExecuteAsync()
        {
            IsAddressChangeButtonsEnable = false;
            IsModeSwitchEnable = false;

            SearchProgressBar = 0;
            OnlineDevicesList.Clear();

            _token = new MyCancellationTokenSource();
            var token = _token.Token;

            return Task.Run(() =>
            {
                ScanningModeSelector(token);
                _dispatcher.Invoke(() =>
                {
                    IsSliderEnable = true;
                    IsAddressChangeButtonsEnable = true;
                    ScanSliderIsChecked = false;
                    IsModeSwitchEnable = true;
                    IsProgressIndeterminate = false;
                });
            });
        }

        private void ScanningModeSelector(CancellationToken token)
        {
            if (IsCheckedSearching)
            {
                SearchDevices(token);
            }
            if (IsCheckedWaiting) 
            {
                _dispatcher.Invoke(() => IsProgressIndeterminate = true);
                WaitingNewDeviceLoop(token);
            };
        }


        private void WaitingNewDeviceLoop(CancellationToken token)
        {
            using SerialPort serialPort = new(CurrentRS485Port);
            var comPort = new ComPort
            {
                SerialPort = serialPort
            };
            var c2000M = new C2000M(comPort);

            try
            {
                serialPort.Open();
                
                while (!_token.IsCancellationRequested)
                {
                    WaitDefaultDevice(c2000M);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
            finally
            {
                serialPort.Close();               
            }
        }

        private void WaitDefaultDevice(IOrionDevice c2000M)
        {
            var response = c2000M.GetModelCode(127, out var deviceCode);
            if(response)
            {
                if (!_deviceGenerator.TryGetDeviceByCode(deviceCode, out var orionDevice))
                    return;

                orionDevice.AddressRS485 = FindFreeAddress(c2000M);

                // We created new device with empty port!
                orionDevice.Port = c2000M.Port;

                orionDevice.SetAddress();

                _dispatcher.Invoke(() =>
                {
                    OnlineDevicesList.Add(new ViewOnlineDeviceViewModel(orionDevice));
                    SystemSounds.Beep.Play();
                });
            }
        }

        private uint FindFreeAddress(IOrionDevice c2000M)
        {
            while (!_token.IsCancellationRequested)
            {
                var busyAddresses = OnlineDevicesList.Select(o => o.Device.AddressRS485).ToList();
                var AddressRS485 = GetFirstMissing(busyAddresses);

                var result = IsAddressFree(c2000M, AddressRS485);
                if (result)
                    return AddressRS485;
            }
            throw new Exception("There aren't free addresses!");
        }

        private bool IsAddressFree(IOrionDevice c2000M, uint addressRS485)
        {
            var response = c2000M.GetModelCode((byte)addressRS485, out var deviceCode);
            if(response)
            {
                if (!_deviceGenerator.TryGetDeviceByCode(deviceCode, out var orionDevice))
                    throw new Exception("Unknow device was found. Address: " + addressRS485);
                orionDevice.AddressRS485 = addressRS485;
                orionDevice.Port = c2000M.Port;
                _dispatcher.Invoke(() =>
                {
                    OnlineDevicesList.Add(new ViewOnlineDeviceViewModel(orionDevice));
                });
                return false;
            }
            return true;
        }

        private void ChangeAddress(ViewOnlineDeviceViewModel onlineDeviceViewModel)
        {
            var currentDevice = onlineDeviceViewModel.Device;

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

            using SerialPort serialPort = new(CurrentRS485Port);
            var comPort = new ComPort
            {
                SerialPort = serialPort
            };
            try
            {
                IsModeSwitchEnable = false;
                serialPort.Open();
                currentDevice.Port = comPort;
                TryToChangeDeviceAddress(onlineDeviceViewModel.Address, currentDevice);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
            finally
            {
                serialPort.Close();                
            }
        }

        private void ChangeAddressToFirstFree(ViewOnlineDeviceViewModel onlineDeviceViewModel)
        {
            var currentDevice = onlineDeviceViewModel.Device;

            var allAddresses = OnlineDevicesList.Select(o => o.Device.AddressRS485).ToList();
            var newAddress = GetFirstMissing(allAddresses);

            using (SerialPort serialPort = new(CurrentRS485Port))
            {
                var comPort = new ComPort
                {
                    SerialPort = serialPort
                };
                try
                {
                    IsModeSwitchEnable = false;
                    serialPort.Open();
                    currentDevice.Port = comPort;
                    TryToChangeDeviceAddress(newAddress, currentDevice);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);
                }
                finally
                {
                    serialPort.Close();
                }
            }
                        
            _dispatcher.Invoke(() =>
            {
                onlineDeviceViewModel.Refresh();
            });
        }

        private void TryToChangeDeviceAddress(uint address, IOrionDevice currentDevice)
        {
            bool changeAddressSuccess = false;
            changeAddressSuccess = currentDevice.ChangeDeviceAddress((byte)address);

            if (changeAddressSuccess)
            {
                currentDevice.AddressRS485 = address;
            }
        }


        private void SearchDevices(CancellationToken token)
        {
            using (SerialPort serialPort = new(CurrentRS485Port))
            {
                var comPort = new ComPort
                {
                    SerialPort = serialPort
                };
                try
                {
                    serialPort.Open();
                    var c2000M = new C2000M(comPort);
                    var onlineDevices = c2000M.SearchOnlineDevices(UpdateProgressBar(), token);

                    foreach (var item in onlineDevices)
                    {
                        _dispatcher.Invoke(() =>
                        {
                            OnlineDevicesList.Add(new ViewOnlineDeviceViewModel(item));
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);
                }
                finally
                {
                    serialPort.Close();
                    
                }
            }
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

        private void TryToCancelCurrentNetWorking()
        {
            if (!_token.IsDisposed)
            {
                _token?.Cancel();
                _token?.Dispose();
            }
        }


        private void PresentSelectedDevice()
        {
            if (SelectedDevice == null)
                return;
            if (SelectedDevice.Device == null)
                return;

            ButtonLabels.Clear();
            var device = SelectedDevice.Device;
            var deviceType = device.GetType();

            var relaysPropertyInfo = deviceType.GetProperty(nameof(Signal20P.Relays));

            if (relaysPropertyInfo != null)
            {
                var propertyValue = (IEnumerable<Relay>)relaysPropertyInfo.GetValue(device);
                
                foreach( var relay in propertyValue)
                {
                    ButtonLabels.Add(relay.RelayIndex.ToString());
                }
            }

            var superRelaysPropertyInfo = deviceType.GetProperty(nameof(Signal20P.SupervisedRelays));
            if (superRelaysPropertyInfo != null)
            {
                var propertyValue = (IEnumerable<Relay>)superRelaysPropertyInfo.GetValue(device);

                foreach (var relay in propertyValue)
                {
                    ButtonLabels.Add(relay.RelayIndex.ToString());
                }
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
