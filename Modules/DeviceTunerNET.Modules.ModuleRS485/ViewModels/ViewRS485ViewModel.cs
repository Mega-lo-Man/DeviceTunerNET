using DeviceTunerNET.Core;
using DeviceTunerNET.Core.Mvvm;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using DeviceTunerNET.SharedDataModel.Devices;
using System.IO.Ports;
using DeviceTunerNET.SharedDataModel.Ports;


namespace DeviceTunerNET.Modules.ModuleRS485.ViewModels
{
    public partial class ViewRS485ViewModel : RegionViewModelBase
    {
        private readonly IEventAggregator _ea;
        private readonly IDataRepositoryService _dataRepositoryService;
        private readonly ISerialTasks _serialTasks;
        private readonly IDialogCaller _dialogCaller;
        private readonly IUploadSerialManager _uploadManager;
        private readonly IAuthLoader _authLoader;
        private readonly Dispatcher _dispatcher;

        private enum VerificationStart
        {
            canExecute = 1,
            cantExecute = 0
        }

        #region Commands
        public DelegateCommand StartCommand { get; private set; }
        public DelegateCommand CheckCommand { get; private set; }
        #endregion

        #region Constructor
        public ViewRS485ViewModel(IRegionManager regionManager,
                                  ISerialTasks serialTasks,
                                  IDataRepositoryService dataRepositoryService,
                                  IDialogCaller dialogCaller,
                                  IUploadSerialManager uploadSerialManager,
                                  IEventAggregator ea,
                                  IAuthLoader authLoader) : base(regionManager)
        {
            _ea = ea;
            _dataRepositoryService = dataRepositoryService;
            _serialTasks = serialTasks;
            _dialogCaller = dialogCaller;
            _uploadManager = uploadSerialManager;
            _ea.GetEvent<MessageSentEvent>().Subscribe(MessageReceived);
            _authLoader = authLoader;
            _dispatcher = Dispatcher.CurrentDispatcher;

            AvailableComPorts = _serialTasks.GetAvailableCOMPorts();// Заполняем коллецию с доступными COM-портами

            SetupRelayCommands();

            AvailableProtocols.Add("COM");
            AvailableProtocols.Add("WIFI");
            CurrentProtocol = AvailableProtocols.FirstOrDefault();
            CurrentRS485Port = AvailableComPorts.LastOrDefault();

            IsCheckedByCabinetsEnabled = _authLoader.AvailableServicesNames.Contains("BYCABINETSCHECKBOX");
            IsCheckedByAreaEnabled = _authLoader.AvailableServicesNames.Contains("BYAREASCHECKBOX");
            IsCheckedComplexVerificationEnabled = _authLoader.AvailableServicesNames.Contains("CHECKRS485CHECKBOX");

            Title = "RS485";
        }

        #endregion Constructor

        private void SetupRelayCommands()
        {
            StartCommand = new DelegateCommand(async () => await StartCommandExecuteAsync(), StartCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port)
                .ObservesProperty(() => CurrentProtocol)
                .ObservesProperty(() => IsCheckedByArea)
                .ObservesProperty(() => IsCheckedByCabinets)
                .ObservesProperty(() => IsCheckedComplexVerification)
                .ObservesProperty(() => SerialTextBox)
                .ObservesProperty(() => DevicesForProgramWasFill);

            CheckCommand = new DelegateCommand(async () => await CheckCommandExecuteAsync(), CheckCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port)
                .ObservesProperty(() => CurrentProtocol)
                .ObservesProperty(() => IsCheckedByArea)
                .ObservesProperty(() => IsCheckedByCabinets)
                .ObservesProperty(() => IsCheckedComplexVerification)
                .ObservesProperty(() => DevicesForProgramWasFill);
        } 

        private bool CheckCommandCanExecute()
        {
            if (IsCheckedByArea || IsCheckedByCabinets)
            {
                if (CurrentProtocol.Contains("WIFI"))
                    return DevicesForProgramming.Count > 0;

                return CurrentRS485Port != null &&
                       DevicesForProgramming.Count > 0;
            }

            if (CurrentProtocol.Contains("WIFI"))
                return DevicesForProgramming.Count > 0;

            return DevicesForProgramming.Count > 0 &&
                   CurrentRS485Port != null;
        }

        private Task CheckCommandExecuteAsync()
        {
            return Task.Run(VerificationCabinetsLoop);
        }

        private bool StartCommandCanExecute()
        {
            if(IsCheckedByArea || IsCheckedByCabinets)
            {
                if (CurrentProtocol.Contains("WIFI"))
                    return SerialTextBox?.Length > 0 &&
                           DevicesForProgramming.Count > 0;

                return CurrentRS485Port != null &&
                       SerialTextBox?.Length > 0 &&
                       DevicesForProgramming.Count > 0;
            }

            if(CurrentProtocol.Contains("WIFI"))
                return DevicesForProgramming.Count > 0;

            return DevicesForProgramming.Count > 0 &&
                   CurrentRS485Port != null;
        }

        private Task StartCommandExecuteAsync()
        {

            if (IsCheckedByArea || IsCheckedByCabinets)
            {
                return Task.Run(UploadSettings);
            }

            SearchProgressBar = 1;

            return Task.Run(VerificationCabinetsLoop);
        }

        private void VerificationCabinetsLoop()
        {
            // Если кол-во приборов с адресом по умолчанию (определяется по наличию серийника) более одного - их уже не настроить без демонтажа
            var numberOfDeviceWithoutSerial = GetNumberOfDeviceWithoutSerial(DevicesForProgramming);
            if (numberOfDeviceWithoutSerial > 1)
            {
                _dialogCaller.ShowMessage("В списке приборов более одного прибора не имеют серийника!");
                return;
            }

            StartButtonEnable = false;// Lock start button

            // Displaying a window asking user to enter the serial number.
            string serial = "";

            if (numberOfDeviceWithoutSerial == 1)
            {
                var device = GetDevicesWithoutSerial(DevicesForProgramming).First();
                var model = device.Model;
                var designation = device.Designation;

                serial = _dialogCaller.GetSerialNumber(model, designation);
                
                if (string.IsNullOrEmpty(serial))
                {
                    StartButtonEnable = true;// Unlock start button
                    return;
                }
            }

            // Провести контроль качества всех приборов с серийниками и если какой-то прибор отсутсвует
            // вывести предупреждение об этом
            var devsWithSerial = GetDevicesWithSerial(DevicesForProgramming);
            var passedQcNumb = QualityControl(devsWithSerial);

            // Обновляем всю коллекцию в UI целиком
            _dispatcher.BeginInvoke(new Action(() =>
            {
                CollectionViewSource.GetDefaultView(DevicesForProgramming).Refresh();
            }));

            if (passedQcNumb != devsWithSerial.Count())
            {
                _dialogCaller.ShowMessage("Количество не ответивших приборов имеющих серийник: " + (devsWithSerial.Count() - passedQcNumb));
                StartButtonEnable = true; // Unlock start button
                return;
            }

            // Если только один прибор не настроен, надо его настроить
            // и провести контроль качества
            if (GetDevicesWithoutSerial(DevicesForProgramming).Count() == 1)
            {
                var device = GetDevicesWithoutSerial(DevicesForProgramming).First();

                Upload(device, serial);

                // Обновляем всю коллекцию в UI целиком
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    CollectionViewSource.GetDefaultView(DevicesForProgramming).Refresh();
                }));

                if (device.Serial == null)
                {
                    StartButtonEnable = true; // Unlock start button
                    return;
                }

                var passedQc = QualityControl(new List<IOrionDevice>() { device });
            }

            _dispatcher.BeginInvoke(new Action(() =>
            {
                CollectionViewSource.GetDefaultView(DevicesForProgramming).Refresh();
            }));

            var numberOfDeviceWithoutQcPassed = GetNumberOfDeviceWithoutQcPassed(DevicesForProgramming);
            if (numberOfDeviceWithoutQcPassed != 0)
            {
                _dialogCaller.ShowMessage("Колчиство не прошедших проверку приборов: " + numberOfDeviceWithoutQcPassed);
                StartButtonEnable = true;// unlock start button
                return;
            }

            _dialogCaller.ShowMessage("All systems are go!");
            SerialTextBox = "";
            StartButtonEnable = true;// unlock start button
        }

        private int QualityControl(IEnumerable<IOrionDevice> devices)
        {
            var result = 0;
            var serialPort = new SerialPort(CurrentRS485Port ?? "COM1");
            serialPort.Open();

            foreach (var device in devices)
            {

                var checkAddress = Convert.ToByte(device.AddressRS485);
                if(device is OrionDevice orionDevice)
                {
                    orionDevice.Port = new ComPort() { SerialPort = serialPort };
                    if (orionDevice.IsDeviceOnline())
                    {
                        device.QualityControlPassed = true;
                        result++;
                    }
                    else
                    {
                        device.QualityControlPassed = false;
                    }
                    
                }
                
                _dataRepositoryService.SaveQualityControlPassed(device.Id, device.QualityControlPassed);

                // Обновляем всю коллекцию в UI целиком
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    CollectionViewSource.GetDefaultView(DevicesForProgramming).Refresh();
                }));
            }
            serialPort.Close();
            return result;
        }

        private void UploadSettings()
        {
            var device = GetDevicesWithoutSerial(DevicesForProgramming).FirstOrDefault();

            if (device == null)
            {
                _dialogCaller.ShowMessage("Nothing to program!");
                return;
            }

            StartButtonEnable = false; // Lock start button

            Upload(device, SerialTextBox);

            // Is Device Was Last?
            if (GetDevicesWithoutSerial(DevicesForProgramming) == null)
            {
                _dialogCaller.ShowMessage("Alles!");
            }

            SerialTextBox = "";
            StartButtonEnable = true; // unlock start button
        }

        private void Upload(IOrionDevice device, string serialNumb)
        {
            _dispatcher.BeginInvoke(new Action(() =>
            {
                CurrentDeviceModel = device.Model;
            }));

            UploadToDevice(device, serialNumb);

            // Обновляем всю коллекцию в UI целиком
            _dispatcher.BeginInvoke(new Action(() =>
            {
                CollectionViewSource.GetDefaultView(DevicesForProgramming).Refresh();
                StartButtonEnable = true;// unlock start button
            }));
        }

        private void UploadToDevice(IOrionDevice device, string serialNumb)
        {
            _uploadManager.PortName = CurrentRS485Port;
            _uploadManager.Protocol = CurrentProtocol;
            _uploadManager.UpdateProgressBar = UpdateProgressBar();
            _uploadManager.Upload(device, serialNumb);
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
        
        private static int GetNumberOfDeviceWithoutSerial(IEnumerable<object> devices)
        {
            return devices.Cast<IOrionDevice>().Count(device => string.IsNullOrEmpty(device.Serial));
        }

        private static int GetNumberOfDeviceWithoutQcPassed(IEnumerable<object> devices)
        {
            var counter = 0;
            foreach (var device in devices.Cast<Device>())
            {
                if (!device.QualityControlPassed)
                    counter++;
            }

            return counter;
        }

        private IEnumerable<IOrionDevice> GetDevicesWithoutSerial(IEnumerable<object> devices)
        {
            //исключаем приборы уже имеющие серийник (они уже были сконфигурированны)
            return devices.Cast<IOrionDevice>().Where(device => string.IsNullOrEmpty(device.Serial));
        }

        private IEnumerable<IOrionDevice> GetDevicesWithSerial(IEnumerable<object> devices)
        {
            //исключаем приборы уже имеющие серийник (они уже были сконфигурированны)
            return devices.Cast<IOrionDevice>().Where(device => !string.IsNullOrEmpty(device.Serial));
        }

        private void AddToFilteredCabsVM(string text)
        {
            FilteredCabsVM.Clear();
            foreach (CabinetViewModel item in _cabsVM)
            {
                var designation = item.GetCombinedName;
                if (designation == null)
                {
                    continue;
                }
                if (text.Length == 0 || designation.ToUpper().Contains(text.ToUpper()))
                {
                    FilteredCabsVM.Add(item);
                }
            }
        }

        private void MessageReceived(Message message)
        {
            if (message.ActionCode == MessageSentEvent.RepositoryUpdated)
            {
                CabinetList.Clear();
                CabsVM.Clear();
                FilteredCabsVM.Clear();

                var cabOut = _dataRepositoryService.GetCabinetsWithoutExcludeDevices<EthernetSwitch>();

                foreach (var cabinet in cabOut)
                {
                    CabinetList.Add(cabinet);
                    var cabinetViewModel = new CabinetViewModel(cabinet, _ea);
                    CabsVM.Add(cabinetViewModel);// Fill the TreeView with cabinets
                    FilteredCabsVM.Add(cabinetViewModel);
                }
            }
            if (message.ActionCode == MessageSentEvent.UserSelectedItemInTreeView)
            {
                var dev = message.AttachedObject;
                if (IsCheckedByCabinets)
                {
                    if (dev.GetType() == typeof(C2000Ethernet)) // Юзер кликнул на прибор RS232 в дереве
                    {
                        DevicesForProgramming.Clear();
                        DevicesForProgramming.Add((C2000Ethernet)message.AttachedObject);
                        DevicesForProgramWasFill = true;
                    }
                    else if (dev.GetType() == typeof(Cabinet)) //Юзер кликнул на шкаф в дереве
                    {
                        DevicesForProgramming.Clear();
                        var cab = (Cabinet)message.AttachedObject;
                        foreach (var item in cab.GetAllDevicesList)//GetDevicesList<RS485device>())
                        {
                            DevicesForProgramming.Add(item);
                            DevicesForProgramWasFill = true;
                        }
                    }
                    else if (dev is IOrionDevice) // Юзер кликнул на прибор RS485 в дереве
                    {
                        DevicesForProgramming.Clear();
                        DevicesForProgramming.Add((IOrionDevice)message.AttachedObject);
                        DevicesForProgramWasFill = true;
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
                            DevicesForProgramWasFill = true;
                        }
                    }
                }
            }
            if (message.ActionCode == MessageSentEvent.UpdateRS485SearchProgressBar)
            {
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    SearchProgressBar = (int)message.AttachedObject;
                }));

            }
        }
    }
}
