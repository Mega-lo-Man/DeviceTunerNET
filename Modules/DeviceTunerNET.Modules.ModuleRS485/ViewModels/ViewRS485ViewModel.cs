﻿using DeviceTunerNET.Core;
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

namespace DeviceTunerNET.Modules.ModuleRS485.ViewModels
{
    public class ViewRS485ViewModel : RegionViewModelBase
    {
        private readonly IEventAggregator _ea;
        private readonly IDataRepositoryService _dataRepositoryService;
        private readonly ISerialSender _serialSender;
        private readonly ISerialTasks _serialTasks;
        private readonly IDialogService _dialogService;
        private readonly Dispatcher _dispatcher;

        private enum VerificationStart
        {
            waitFor = 2,
            canExecute = 1,
            cantExecute = 0
        }

        #region Properties

        private VerificationStart VerificationCanStart { get; set; }

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

        #endregion

        #region Commands
        public DelegateCommand StartCommand { get; private set; }
        public DelegateCommand CheckCommand { get; private set; }
        #endregion

        #region Constructor
        public ViewRS485ViewModel(IRegionManager regionManager,
                                  ISerialTasks serialTasks,
                                  IDataRepositoryService dataRepositoryService,
                                  IEventAggregator ea,
                                  ISerialSender serialSender,
                                  IDialogService dialogService) : base(regionManager)
        {
            _ea = ea;
            _serialSender = serialSender;
            _dataRepositoryService = dataRepositoryService;
            //_serialSender = serialSender;
            _serialTasks = serialTasks;

            _ea.GetEvent<MessageSentEvent>().Subscribe(MessageReceived);

            _dialogService = dialogService;

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
        #endregion Constructor

        private bool CheckCommandCanExecute()
        {
            return CurrentRS485Port != null; //&& DevicesForProgramming.Count > 0;
        }

        private Task CheckCommandExecuteAsync()
        {
            return Task.Run(VerificationCabinetsLoop);
        }

        private bool StartCommandCanExecute()
        {
            return CurrentRS485Port != null &&
                   SerialTextBox.Length > 0 &&
                   DevicesForProgramming.Count > 0;
        }

        private Task StartCommandExecuteAsync()
        {
            
            if (IsCheckedByArea || IsCheckedByCabinets)
            {
                return Task.Run(DownloadLoop);
            }

            SearchProgressBar = 1;
            
            return Task.Run(VerificationCabinetsLoop);
        }

        private void VerificationCabinetsLoop()
        {
            // Если кол-во приборов с адресом по умолчанию (определяется по наличию серийника) более одного - их уже не настроить без демонтажа
            if (GetNumberOfDeviceWithoutSerial(DevicesForProgramming) > 1)
            {
                MessageBox.Show("В списке приборов более одного прибора не имеют серийника!");
                StartButtonEnable = true;// unlock start button
                return;
            }

            VerificationCanStart = VerificationStart.waitFor;

            _dispatcher.BeginInvoke(new Action(() =>
            {
                var tcs = new TaskCompletionSource<string>();
                var parameters = new DialogParameters
                {
                    { "title", "Connection Lost!" },
                    { "message", "We seem to have lost network connectivity" }
                };
                _dialogService.ShowDialog("SerialDialog", parameters, dialogResult =>
                {
                    if (dialogResult.Result == ButtonResult.OK)
                    {
                        SerialTextBox = dialogResult.Parameters.GetValue<string>("Serial");
                        VerificationCanStart = VerificationStart.canExecute;
                    }

                    else
                    {
                        VerificationCanStart = VerificationStart.cantExecute;
                    }
                });
            }));
            while (VerificationCanStart == VerificationStart.waitFor)
            {

            }

            if (VerificationCanStart == VerificationStart.cantExecute)
                return;

            StartButtonEnable = false;// Lock start button
            

            // Если только один прибор не настроен, надо его настроить
            if (GetNumberOfDeviceWithoutSerial(DevicesForProgramming) == 1)
            {
                DownloadLoop();
            }

            foreach (RS485device device in DevicesForProgramming)
            {
                if (_serialTasks.CheckOnlineDevice(CurrentRS485Port, device) != ISerialTasks.ResultCode.ok) 
                    continue;
                device.QualityControlPassed = true;
                
                // Обновляем всю коллекцию в UI целиком
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    CollectionViewSource.GetDefaultView(DevicesForProgramming).Refresh();
                }));
            }

            // Если всё ещё как-либо прибор не настроен - ничего не вышло. Пусть проверяющий включает голову
            if (GetNumberOfDeviceWithoutQcPassed(DevicesForProgramming) != 0)
            {
                MessageBox.Show("Некоторые приборы не прошли проверку");
            }
            else
            {
                MessageBox.Show("All good!");
            }

            SerialTextBox = "";
            StartButtonEnable = true;// unlock start button
        }

        private void DownloadLoop()
        {
            StartButtonEnable = false; // Lock start button
           
            var device = (Device)GetDeviceWithoutSerial(DevicesForProgramming);

            if (device == null)
            {
                StartButtonEnable = true; // unlock start button
                MessageBox.Show("Nothing to programming!");
                return;
            }

            //var devSerial = "";

            _dispatcher.BeginInvoke(new Action(() =>
            {
                CurrentDeviceModel = device.Model;
            }));

            if (device.GetType() == typeof(RS485device))
            {
                var sendResult = _serialTasks.SendConfig(device,
                    CurrentRS485Port,
                    DefaultRS485Address);
                SendResponseProcessing(sendResult, device);
            }
            else if (device.GetType() == typeof(C2000Ethernet))
            {
                var c2000Ethernet = (C2000Ethernet) device;
                c2000Ethernet.Netmask = IPMask;
                if (c2000Ethernet.NetworkMode == C2000Ethernet.Mode.master)
                {
                    c2000Ethernet.RemoteIpTrasparentMode = RemoteDefaultFirstIP;
                }

                var sendResult = _serialTasks.SendConfig(c2000Ethernet,
                    CurrentRS485Port,
                    DefaultRS485Address);
                SendResponseProcessing(sendResult, device);
            }
            else
            {
                MessageBox.Show("Устройство в очереди неизвестного типа!");
            }

            // Обновляем всю коллекцию в UI целиком
            _dispatcher.BeginInvoke(new Action(() =>
            {
                CollectionViewSource.GetDefaultView(DevicesForProgramming).Refresh();
            }));

            // Is Device Was Last?
            if (GetDeviceWithoutSerial(DevicesForProgramming) == null)
            {
                MessageBox.Show("Alles!");
            }
            
            StartButtonEnable = true; // unlock start button
        }

        private int GetNumberOfDeviceWithoutSerial(IEnumerable<object> devices)
        {
            return devices.Cast<RS485device>().Count(device => string.IsNullOrEmpty(device.Serial));
        }

        private int GetNumberOfDeviceWithoutQcPassed(IEnumerable<object> devices)
        {
            return devices.Cast<RS485device>().Count(device => device.QualityControlPassed == false);
        }

        private object GetDeviceWithoutSerial(IEnumerable<object> devices)
        {
            //исключаем приборы уже имеющие серийник (они уже были сконфигурированны)
            return devices.Cast<RS485device>().FirstOrDefault(device => string.IsNullOrEmpty(device.Serial));
        }

        private void SendResponseProcessing(ISerialTasks.ResultCode sendResult, Device device1)
        {
            switch (sendResult)
            {
                case ISerialTasks.ResultCode.ok:
                    device1.Serial = SerialTextBox;
                    _dataRepositoryService.SaveSerialNumber(device1.Id, device1.Serial);
                    SerialTextBox = ""; // Очищаем строку ввода серийника для ввода следующего
                    break;
                case ISerialTasks.ResultCode.deviceNotRespond:
                    MessageBox.Show("Прибор с адресом 127 не отвечает!");
                    break;
                case ISerialTasks.ResultCode.deviceTypeMismatch:
                    MessageBox.Show("Тип обнаруженного прибора не совпадает с ожидаемым типом!");
                    break;
                case ISerialTasks.ResultCode.addressFieldNotValid:
                    MessageBox.Show("Неверный адрес!");
                    break;
                case ISerialTasks.ResultCode.undefinedError:
                    MessageBox.Show("Неопознанная ошибка!");
                    break;
                case ISerialTasks.ResultCode.errorConfigDownload:
                    MessageBox.Show("Ошибка заливки конфигурации в С2000-Ethernet!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sendResult), sendResult, null);
            }
        }

        private int GetAddressSearchProgressBar(int address)
        {
            return 100 * address / 127;
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
                }
                if (IsCheckedComplexVerification)
                {
                    if (dev.GetType() == typeof(Cabinet)) //Юзер кликнул на шкаф в дереве
                    {
                        DevicesForProgramming.Clear();
                        var cab = (Cabinet)message.AttachedObject;
                        foreach (var item in cab.GetAllDevicesList)//GetDevicesList<RS485device>())
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
                    SearchProgressBar = GetAddressSearchProgressBar(Convert.ToInt32((byte)message.AttachedObject));
                }));

            }
        }
    }
}
