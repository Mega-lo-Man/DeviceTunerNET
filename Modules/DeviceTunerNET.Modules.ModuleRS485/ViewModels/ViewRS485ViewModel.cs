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
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Prism.Services.Dialogs;
using DeviceTunerNET.SharedDataModel.Devices;
using System.Diagnostics;
using System.IO.Ports;
using DeviceTunerNET.SharedDataModel.Ports;
using System.Net;
using System.Threading;

namespace DeviceTunerNET.Modules.ModuleRS485.ViewModels
{
    public partial class ViewRS485ViewModel : RegionViewModelBase
    {
        private readonly IEventAggregator _ea;
        private readonly IDataRepositoryService _dataRepositoryService;
        private readonly ISerialTasks _serialTasks;
        private readonly IDialogService _dialogService;
        private readonly Dispatcher _dispatcher;

        private enum VerificationStart
        {
            waitFor = 2, // Waiting for user input serial
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
                                  IEventAggregator ea,
                                  IDialogService dialogService) : base(regionManager)
        {
            _ea = ea;
            _dataRepositoryService = dataRepositoryService;
            _serialTasks = serialTasks;
            _ea.GetEvent<MessageSentEvent>().Subscribe(MessageReceived);
            _dialogService = dialogService;
            _dispatcher = Dispatcher.CurrentDispatcher;

            AvailableComPorts = _serialTasks.GetAvailableCOMPorts();// Заполняем коллецию с доступными COM-портами

            SetupRelayCommands();

            AvailableProtocols.Add("COM");
            AvailableProtocols.Add("WIFI");
            CurrentProtocol = AvailableProtocols.FirstOrDefault();
            CurrentRS485Port = AvailableComPorts.LastOrDefault();

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
                return Task.Run(DownloadSettings);
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
                MessageBox.Show("В списке приборов более одного прибора не имеют серийника!");
                return;
            }

            StartButtonEnable = false;// Lock start button

            // Displaying a window asking user to enter the serial number.
            string serial = "";
            VerificationCanStart = VerificationStart.waitFor;

            if (numberOfDeviceWithoutSerial == 1)
            {
                var device = GetDevicesWithoutSerial(DevicesForProgramming).First();
                var model = device.Model;
                var designation = device.Designation;

                _dispatcher.BeginInvoke(new Action(() =>
                {
                    //var tcs = new TaskCompletionSource<string>();
                    var parameters = new DialogParameters
                    {
                        {"title", "Ввод серийного номера."},
                        {"message", "Серийник: "},
                        {"model", model},
                        {"designation", designation}
                    };
                    _dialogService.ShowDialog("SerialDialog", parameters, dialogResult =>
                    {
                        if (dialogResult.Result == ButtonResult.OK)
                        {
                            serial = dialogResult.Parameters.GetValue<string>("Serial");
                            if (serial == null)
                            {
                                StartButtonEnable = true;// Unlock start button
                                return;
                            }

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
                MessageBox.Show("Количество не ответивших приборов имеющих серийник: " + (devsWithSerial.Count() - passedQcNumb));
                StartButtonEnable = true; // Unlock start button
                return;
            }

            // Если только один прибор не настроен, надо его настроить
            // и провести контроль качества
            if (GetDevicesWithoutSerial(DevicesForProgramming).Count() == 1)
            {
                var device = GetDevicesWithoutSerial(DevicesForProgramming).First();

                Download(device, serial);

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

                var passedQc = QualityControl(new List<RS485device>() { device });
            }

            _dispatcher.BeginInvoke(new Action(() =>
            {
                CollectionViewSource.GetDefaultView(DevicesForProgramming).Refresh();
            }));

            var numberOfDeviceWithoutQcPassed = GetNumberOfDeviceWithoutQcPassed(DevicesForProgramming);
            if (numberOfDeviceWithoutQcPassed != 0)
            {
                MessageBox.Show("Колчиство не прошедших проверку приборов: " + numberOfDeviceWithoutQcPassed);
                StartButtonEnable = true;// unlock start button
                return;
            }

            MessageBox.Show("All good!");
            SerialTextBox = "";
            StartButtonEnable = true;// unlock start button
        }

        private int QualityControl(IEnumerable<RS485device> devices)
        {
            var result = 0;
            foreach (var device in devices)
            {

                var checkAddress = Convert.ToByte(device.AddressRS485);
                if(device is OrionDevice orionDevice)
                {
                    var serialPort = new SerialPort(CurrentRS485Port ?? "COM1");
                    orionDevice.Port = new ComPort() { SerialPort = serialPort };
                    serialPort.Open();
                    if (orionDevice.IsDeviceOnline())
                    {
                        device.QualityControlPassed = true;
                        result++;
                    }
                    else
                    {
                        device.QualityControlPassed = false;
                    }
                    serialPort.Close();
                }
                
                _dataRepositoryService.SaveQualityControlPassed(device.Id, device.QualityControlPassed);

                // Обновляем всю коллекцию в UI целиком
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    CollectionViewSource.GetDefaultView(DevicesForProgramming).Refresh();
                }));
            }
            return result;
        }

        private void DownloadSettings()
        {
            var device = GetDevicesWithoutSerial(DevicesForProgramming).FirstOrDefault();

            if (device == null)
            {
                MessageBox.Show("Nothing to program!");
                return;
            }

            StartButtonEnable = false; // Lock start button
            //var devSerial = "";

            Download(device, SerialTextBox);

            // Is Device Was Last?
            if (GetDevicesWithoutSerial(DevicesForProgramming) == null)
            {
                MessageBox.Show("Alles!");
            }

            SerialTextBox = "";
            StartButtonEnable = true; // unlock start button
        }

        private void Download(RS485device device, string serialNumb)
        {
            _dispatcher.BeginInvoke(new Action(() => { CurrentDeviceModel = device.Model; }));
            var serialPort = new SerialPort(CurrentRS485Port ?? "COM1");
            try
            {    
                
                if (device is OrionDevice orionDevice)
                {
                    if (CurrentProtocol.Equals("COM"))
                    {
                        
                        orionDevice.Port = new ComPort() { SerialPort = serialPort };
                        serialPort.Open();
                    }
                
                    else
                    {
                        var ip = IPAddress.Parse("10.10.10.1");
                        orionDevice.Port = new BolidUdpClient(8100)
                        {
                            
                            RemoteServerIp = ip,
                            RemoteServerUdpPort = 12000

                        };
                    }

                    var isSutupComplite = orionDevice.Setup(UpdateProgressBar());

                    serialPort.Close();
                    if(!isSutupComplite)
                    {
                        MessageBox.Show("Не удалось настроить прибор: " + orionDevice.Model + "; с обозначением: " + orionDevice.Designation);
                        StartButtonEnable = true;// unlock start button
                        return;
                    }
                    SaveSerial(orionDevice, serialNumb);
                }
            }
            catch (Exception ex) 
            { 
                serialPort?.Close();
                MessageBox.Show(ex.Message);
                StartButtonEnable = true;// unlock start button

                return;
            }

            // Обновляем всю коллекцию в UI целиком
            _dispatcher.BeginInvoke(new Action(() =>
            {
                CollectionViewSource.GetDefaultView(DevicesForProgramming).Refresh();
            }));
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
        
        private int GetNumberOfDeviceWithoutSerial(IEnumerable<object> devices)
        {
            return devices.Cast<RS485device>().Count(device => string.IsNullOrEmpty(device.Serial));
        }

        private int GetNumberOfDeviceWithoutQcPassed(IEnumerable<object> devices)
        {
            //return devices.Cast<RS485device>().Count(d => d.QualityControlPassed == false);
            var counter = 0;
            foreach (Device device in devices)
            {
                if (!device.QualityControlPassed)
                    counter++;
            }

            return counter;
        }

        private IEnumerable<RS485device> GetDevicesWithoutSerial(IEnumerable<object> devices)
        {
            //исключаем приборы уже имеющие серийник (они уже были сконфигурированны)
            return devices.Cast<RS485device>().Where(device => string.IsNullOrEmpty(device.Serial));
        }

        private IEnumerable<RS485device> GetDevicesWithSerial(IEnumerable<object> devices)
        {
            //исключаем приборы уже имеющие серийник (они уже были сконфигурированны)
            return devices.Cast<RS485device>().Where(device => !string.IsNullOrEmpty(device.Serial));
        }

        private void SaveSerial(Device device, string serialNumb)
        {
            device.Serial = serialNumb;

            if (!_dataRepositoryService.SaveSerialNumber(device.Id, device.Serial))
            {
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    Clipboard.SetText(device.Serial ?? string.Empty);
                }));

                MessageBox.Show("Не удалось сохранить серийный номер! Он был скопирован в буфер обмена.");
            }
        }

        private void MessageReceived(Message message)
        {
            if (message.ActionCode == MessageSentEvent.RepositoryUpdated)
            {
                CabinetList.Clear();
                CabsVM.Clear();

                var cabOut = _dataRepositoryService.GetCabinetsWithoutExcludeDevices<EthernetSwitch>();

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
                    else if (dev is RS485device) // Юзер кликнул на прибор RS485 в дереве
                    {
                        DevicesForProgramming.Clear();
                        DevicesForProgramming.Add((RS485device)message.AttachedObject);
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
