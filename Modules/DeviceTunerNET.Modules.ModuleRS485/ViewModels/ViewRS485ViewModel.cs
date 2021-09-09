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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace DeviceTunerNET.Modules.ModuleRS485.ViewModels
{
    public class ViewRS485ViewModel : RegionViewModelBase
    {
        private int DeviceCounter = 0;

        #region Commands
        public DelegateCommand StartCommand { get; private set; }
        public DelegateCommand CheckCommand { get; private set; }
        #endregion

        #region Properties
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

        private bool _isCheckedByCabinets = true;
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

        private bool _isCheckedByArea = false;
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

        private bool _isCheckedComplexVerification = false;
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

        private ObservableCollection<Cabinet> _cabinetList = new ObservableCollection<Cabinet>();
        public ObservableCollection<Cabinet> CabinetList
        {
            get => _cabinetList;
            set => SetProperty(ref _cabinetList, value);
        }

        private ObservableCollection<CabinetViewModel> _cabsVM = new ObservableCollection<CabinetViewModel>();
        public ObservableCollection<CabinetViewModel> CabsVM
        {
            get => _cabsVM;
            set => SetProperty(ref _cabsVM, value);
        }

        private ObservableCollection<object> _devicesForProgramming = new ObservableCollection<object>();
        public ObservableCollection<object> DevicesForProgramming
        {
            get => _devicesForProgramming;
            set => SetProperty(ref _devicesForProgramming, value);
        }

        private ObservableCollection<string> _availableComPorts = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableComPorts
        {
            get => _availableComPorts;
            set => SetProperty(ref _availableComPorts, value);
        }

        private int _searchProgressBar = 0;
        public int SearchProgressBar
        {
            get => _searchProgressBar;
            set
            {
                var percent = value * 100 / 127;
                SetProperty(ref _searchProgressBar, _searchProgressBar = percent);
            }
        }

        private bool _startButtonVisibility = true;
        public bool StartButtonVisibilty
        {
            get => _startButtonVisibility;
            set => SetProperty(ref _startButtonVisibility, value);
        }

        #endregion

        private readonly IEventAggregator _ea;
        private readonly IDataRepositoryService _dataRepositoryService;
        private readonly ISerialSender _serialSender;
        private readonly ISerialTasks _serialTasks;
        private readonly Dispatcher _dispatcher;

        #region Constructor
        public ViewRS485ViewModel(IRegionManager regionManager,
                                  ISerialTasks serialTasks,
                                  IDataRepositoryService dataRepositoryService,
                                  IEventAggregator ea,
                                  ISerialSender serialSender) : base(regionManager)
        {
            _ea = ea;
            _serialSender = serialSender;
            _dataRepositoryService = dataRepositoryService;
            //_serialSender = serialSender;
            _serialTasks = serialTasks;

            _ea.GetEvent<MessageSentEvent>().Subscribe(MessageReceived);

            _dispatcher = Dispatcher.CurrentDispatcher;

            AvailableComPorts = _serialTasks.GetAvailableCOMPorts();// Заполняем коллецию с доступными COM-портами

            StartCommand = new DelegateCommand(async () => await StartCommandExecuteAsync(), StartCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port)
                .ObservesProperty(() => SerialTextBox);

            CheckCommand = new DelegateCommand(async () => await CheckCommandExecuteAsync(), CheckCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port)
                .ObservesProperty(() => SerialTextBox);

            Title = "RS485";
        }

        private bool CheckCommandCanExecute()
        {
            return CurrentRS485Port != null && DevicesForProgramming.Count > 0;
        }

        private Task CheckCommandExecuteAsync()
        {
            throw new NotImplementedException();
        }
        #endregion Constructor

        private bool StartCommandCanExecute()
        {
            return CurrentRS485Port != null && SerialTextBox.Length > 0 && DevicesForProgramming.Count > 0;
        }

        private Task StartCommandExecuteAsync()
        {

            if (IsCheckedByArea || IsCheckedByCabinets)
            {
                return Task.Run(DownloadLoop);
            }
            else
            {
                SearchProgressBar = 1;
                return Task.Run(VerificationCabinetsLoop);
            }
        }

        private void VerificationCabinetsLoop()
        {
            //SerialPort sp = SerialPortInit();
            var onlineDevices = _serialSender.SearchOnlineDevices(CurrentRS485Port);

            foreach (RS485device device in DevicesForProgramming)
            {
                var intAddr = Convert.ToByte(device.AddressRS485);
                var expectedStr = device.Model.ToUpper();

                if (!onlineDevices.ContainsKey(intAddr)) 
                    continue;
                // проверяем содержит ли название модели (в списке приборов шкафа) с моделью предоставленнной самим прибором
                if (expectedStr.Contains(onlineDevices[intAddr].ToUpper()))
                {
                    Debug.WriteLine(intAddr);
                }
            }
        }

        private void DownloadLoop()
        {
            if (DeviceCounter >= DevicesForProgramming.Count) 
                return;

            var device = DevicesForProgramming[DeviceCounter];

            var devSerial = "";

            if (!(device is Device device1))
                return;

            devSerial = device1.Serial;

            if (devSerial == null)//исключаем приборы уже имеющие серийник (они уже были сконфигурированны)
            {
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    CurrentDeviceModel = device1.Model;
                }));
                if (DevicesForProgramming[DeviceCounter].GetType() == typeof(RS485device))
                {
                    if (_serialTasks.SendConfig(DevicesForProgramming[DeviceCounter],
                        CurrentRS485Port,
                        DefaultRS485Address) == 1)
                    {
                        device1.Serial = SerialTextBox;
                        _dataRepositoryService.SaveSerialNumber(device1.Id, device1.Serial);
                        SerialTextBox = "";// Очищаем строку ввода серийника для ввода следующего
                        DeviceCounter++;
                    }
                    else
                    {
                        MessageBox.Show("Устройство с адресом 127 не отвечает!");
                    }
                }
                else if (DevicesForProgramming[DeviceCounter].GetType() == typeof(C2000Ethernet))
                {
                    var c2000Ethernet = (C2000Ethernet)DevicesForProgramming[DeviceCounter];
                    c2000Ethernet.Netmask = IPMask;
                    if (c2000Ethernet.NetworkMode == 2)
                    {
                        c2000Ethernet.RemoteIpTrasparentMode = RemoteDefaultFirstIP;
                    }
                    if (_serialTasks.SendConfig(c2000Ethernet,
                        CurrentRS485Port,
                        DefaultRS485Address) == 1)
                    {
                        device1.Serial = SerialTextBox;
                        _dataRepositoryService.SaveSerialNumber(device1.Id, device1.Serial);
                        SerialTextBox = "";// Очищаем строку ввода серийника для ввода следующего
                        DeviceCounter++;
                    }
                    else
                    {
                        MessageBox.Show("Устройство с адресом 127 не отвечает!");
                    }
                }
                // Обновляем всю коллекцию в UI целиком
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    CollectionViewSource.GetDefaultView(DevicesForProgramming).Refresh();
                }));

                if (DeviceCounter < DevicesForProgramming.Count) 
                    return;

                DeviceCounter = 0;
                MessageBox.Show("Alles!");
            }
            else //Переходим к следующему прибру из списка
            {
                DeviceCounter++;
            }
        }
        
        private void MessageReceived(Message message)
        {
            if (message.ActionCode == MessageSentEvent.RepositoryUpdated)
            {
                CabinetList.Clear();
                CabsVM.Clear();


                var cabOut = _dataRepositoryService.GetCabinetsWithTwoTypeDevices<C2000Ethernet, RS485device>();

                foreach (var cabinet in cabOut)
                {
                    CabinetList.Add(cabinet);
                    CabsVM.Add(new CabinetViewModel(cabinet, _ea));// Fill the TreeView with cabinets
                }
            }
            if (message.ActionCode == MessageSentEvent.UserSelectedItemInTreeView)
            {
                var dev = message.AttachedObject;
                if (IsCheckedByCabinets)
                {
                    if (dev.GetType() == typeof(RS485device)) // Юзер кликнул на прибор RS485 в дереве
                    {
                        DevicesForProgramming.Clear();
                        DevicesForProgramming.Add((RS485device)message.AttachedObject);
                    }
                    if (dev.GetType() == typeof(C2000Ethernet)) // Юзер кликнул на прибор RS232 в дереве
                    {
                        DevicesForProgramming.Clear();
                        DevicesForProgramming.Add((C2000Ethernet)message.AttachedObject);
                    }
                    if (dev.GetType() == typeof(Cabinet)) //Юзер кликнул на шкаф в дереве
                    {
                        DevicesForProgramming.Clear();
                        var cab = (Cabinet)message.AttachedObject;
                        foreach (var item in cab.GetAllDevicesList)//GetDevicesList<RS485device>())
                        {
                            DevicesForProgramming.Add(item);
                        }
                    }
                    DeviceCounter = 0; //Сброс счётчика запрограммированных приборов ибо юзер что то поменял
                }
                if (IsCheckedComplexVerification)
                {
                    if (dev.GetType() == typeof(Cabinet)) //Юзер кликнул на шкаф в дереве
                    {
                        DevicesForProgramming.Clear();
                        Cabinet cab = (Cabinet)message.AttachedObject;
                        foreach (RS485device item in cab.GetDevicesList<RS485device>())
                        {
                            DevicesForProgramming.Add(item);
                        }
                    }
                }
            }
            if (message.ActionCode == MessageSentEvent.UpdateRS485SearchProgressBar)
            {
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    SearchProgressBar = Convert.ToInt32((byte)message.AttachedObject);
                }));

            }
        }
    }
}
