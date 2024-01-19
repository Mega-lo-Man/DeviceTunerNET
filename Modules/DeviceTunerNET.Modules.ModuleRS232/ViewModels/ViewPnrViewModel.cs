using DeviceTunerNET.Core;
using DeviceTunerNET.Core.CustomExceptions;
using DeviceTunerNET.Modules.ModulePnr.Views;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.ElectricModules;
using DeviceTunerNET.SharedDataModel.Ports;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Serilog;

namespace DeviceTunerNET.Modules.ModulePnr.ViewModels
{
    public partial class ViewPnrViewModel : BindableBase
    {
        private string _message;
        private MyCancellationTokenSource _token;
        private readonly ISerialTasks _serialTasks;
        private readonly IDeviceSearcher _bolidDeviceSearcher;
        private readonly IAddressChanger _bolidAddressChanger;
        private readonly IEventAggregator _ea;

        private readonly Dispatcher _dispatcher;
        private readonly SpeechSynthesizer _synth;
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
                                IDeviceSearcher bolidDeviceSearcher,
                                IAddressChanger BolidAddressChanger,
                                IEventAggregator ea
                                )
        {
            Title = "ПНР";

            _dispatcher = Dispatcher.CurrentDispatcher;
            _synth = new SpeechSynthesizer();
            _synth.SetOutputToDefaultAudioDevice();
            var voices = _synth.GetInstalledVoices(new CultureInfo("ru-RU"));
            
            if(voices.Count > 0)
                _synth.SelectVoice(voices[0].VoiceInfo.Name);

            _serialTasks = serialTasks;
            _bolidDeviceSearcher = bolidDeviceSearcher;
            _bolidAddressChanger = BolidAddressChanger;
            _ea = ea;
            _ea.GetEvent<MessageSentEvent>().Subscribe(MessageReceived);

            AvailableComPorts = _serialTasks.GetAvailableCOMPorts();
            CurrentRS485Port = AvailableComPorts.LastOrDefault();

            IsModeSwitchEnable = true;

            #region InitializeCommands
            
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

            #endregion InitializeCommands
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
                _dispatcher.Invoke(() => 
                {
                    RelayViewModels.Clear();
                    ShleifViewModels.Clear();
                });

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

            try
            {
                serialPort.Open();

                _bolidAddressChanger.Port = comPort;
                _bolidAddressChanger.ChangeDefaultAddresses(token);
            }
            catch (InvalidDeviceResponseException ex)
            {
                Log.Error(ex, $"Response: '{Convert.ToHexString(ex.Response)}' {ex.Message}");
                MessageBox.Show("Exception: " + ex.Message);
            }
            finally
            {
                serialPort.Close();               
            }
        }

        private void ChangeAddress(ViewOnlineDeviceViewModel onlineDeviceViewModel)
        {
            var currentDevice = (OrionDevice)onlineDeviceViewModel.Device;

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
                _bolidAddressChanger.TryToChangeDeviceAddress(onlineDeviceViewModel.Address, currentDevice);
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
            var currentDevice = (OrionDevice)onlineDeviceViewModel.Device;

            var allAddresses = OnlineDevicesList.Select(o => o.Device.AddressRS485).ToList();
            var newAddress = _bolidAddressChanger.GetFirstMissing(allAddresses, 127);

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
                    _bolidAddressChanger.TryToChangeDeviceAddress(newAddress, currentDevice);
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

                    _bolidDeviceSearcher.Port = comPort;
                    var onlineDevices = _bolidDeviceSearcher.SearchDevices(token, UpdateProgressBar());

                    foreach (var item in onlineDevices)
                    {
                        _dispatcher.Invoke(() =>
                        {
                            OnlineDevicesList.Add(new ViewOnlineDeviceViewModel((RS485device)item));
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

            CreateRelaysControl();
            CreateShleifsControl();
        }

        private void CreateRelaysControl()
        {
            RelayViewModels.Clear();

            var device = SelectedDevice.Device;
            var deviceType = device.GetType();

            var relayViewModels = GetAllAvailableRelayViewModels(device, deviceType);

            foreach (var item in relayViewModels)
            {
                item.IsControlEnabled = false;
                RelayViewModels.Add(item);
            }
        }

        private void CreateShleifsControl()
        {
            ShleifViewModels.Clear();

            var device = SelectedDevice.Device;
            var deviceType = device.GetType();

            var shleifViewModels = GetAllAvailableShleifViewModel(device, deviceType);

            foreach (var item in shleifViewModels)
            {
                item.IsControlEnabled = false;
                ShleifViewModels.Add(item);
            }
        }

        private static IEnumerable<ViewSingleShleifViewModel> GetAllAvailableShleifViewModel(RS485device device, Type deviceType)
        {
            var shleifPropertyInfo = deviceType.GetProperty(nameof(Signal20P.Shleifs));

            if (shleifPropertyInfo != null)
            {
                var propertyValue = (IEnumerable<Shleif>)shleifPropertyInfo.GetValue(device);

                foreach (var shleif in propertyValue)
                {
                    yield return new ViewSingleShleifViewModel(shleif);
                }
            }
        }

        private static IEnumerable<ViewSingleRelayViewModel> GetAllAvailableRelayViewModels(RS485device device, Type deviceType)
        {
            var relaysPropertyInfo = deviceType.GetProperty(nameof(Signal20P.Relays));
            if (relaysPropertyInfo != null)
            {
                var propertyValue = (IEnumerable<Relay>)relaysPropertyInfo.GetValue(device);

                foreach (var relay in propertyValue)
                {
                    yield return new ViewSingleRelayViewModel(relay);
                }
            }

            var superRelaysPropertyInfo = deviceType.GetProperty(nameof(Signal20P.SupervisedRelays));
            if (superRelaysPropertyInfo != null)
            {
                var propertyValue = (IEnumerable<Relay>)superRelaysPropertyInfo.GetValue(device);

                foreach (var relay in propertyValue)
                {
                    yield return new ViewSingleRelayViewModel(relay);
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

        private void MessageReceived(Message message)
        {
            if (message.ActionCode == MessageSentEvent.FoundNewOnlineDevice)
            {
                var lastDevice = _bolidAddressChanger.FoundDevices.LastOrDefault();
                _dispatcher.Invoke(() =>
                {
                    OnlineDevicesList.Add(new ViewOnlineDeviceViewModel(lastDevice));
                });

                if(message.MessageString.Equals(_bolidAddressChanger.DefaultDeviceFoundMessage))
                    _synth.SpeakAsync(lastDevice.Model);
            }
        }
    }
}
