using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DeviceTunerNET.Modules.ModuleRS232.ViewModels
{
    public partial class ViewRS232ViewModel : BindableBase
    {
        private string _message;
        private readonly ISerialTasks _serialTasks;

        #region Commands

        public DelegateCommand ShiftAddressesCommand { get; }
        public DelegateCommand ScanNetworkCommand { get; }

        #endregion Commands

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
            return CurrentRS485Port != null;
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
            return CurrentRS485Port != null 
                   && StartAddress.Length > 0 
                   && OnlineDevicesList.Count > 0 
                   && _addressRange > 0;
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
