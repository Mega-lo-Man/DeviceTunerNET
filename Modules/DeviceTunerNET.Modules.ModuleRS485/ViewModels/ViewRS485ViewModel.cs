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
            return CurrentRS485Port != null; 
        }

        private Task CheckCommandExecuteAsync()
        {
            return Task.Run(VerificationCabinetsLoop);
        }

        private bool StartCommandCanExecute()
        {
            return CurrentRS485Port != null &&
                   SerialTextBox?.Length > 0 &&
                   DevicesForProgramming.Count > 0;
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

            if(numberOfDeviceWithoutSerial == 1)
            {
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    var tcs = new TaskCompletionSource<string>();
                    var parameters = new DialogParameters
                    {
                        {"title", "Ввод серийного номера."},
                        {"message", "Серийник: "}
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
            foreach (RS485device device in devices)
            {
                var checkAddress = Convert.ToByte(device.AddressRS485);
                if (_serialTasks.CheckOnlineDevice(CurrentRS485Port, checkAddress, device.Model) == ISerialTasks.ResultCode.ok)
                {
                    device.QualityControlPassed = true;
                    result++;
                }

                else
                {
                    device.QualityControlPassed = false;
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
            var device = (Device)GetDevicesWithoutSerial(DevicesForProgramming).FirstOrDefault();

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

        private void Download(Device device, string serialNumb)
        {
            _dispatcher.BeginInvoke(new Action(() => { CurrentDeviceModel = device.Model; }));

            if (device.GetType() == typeof(RS485device))
            {
                var sendResult = _serialTasks.SendConfig(device,
                    CurrentRS485Port,
                    DefaultRS485Address);
                SendResponseProcessing(sendResult, device, serialNumb);
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
                SendResponseProcessing(sendResult, device, serialNumb);
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

        /*
        private IEnumerable<RS485device> GetDevicesWithoutQualityControl(IEnumerable<object> devices)
        {
            return devices.Cast<RS485device>().Where(device => device.QualityControlPassed == false).ToList();
        }
        */
        private void SendResponseProcessing(ISerialTasks.ResultCode sendResult, Device device1, string serialNumb)
        {
            switch (sendResult)
            {
                case ISerialTasks.ResultCode.ok:
                    device1.Serial = serialNumb;
                    
                    if (!_dataRepositoryService.SaveSerialNumber(device1.Id, device1.Serial))
                    {
                        _dispatcher.BeginInvoke(new Action(() =>
                        {
                            Clipboard.SetText(device1.Serial ?? string.Empty);
                        }));
                        
                        MessageBox.Show("Не удалось сохранить серийный номер! Он был скопирован в буфер обмена.");
                    }
                    //erialTextBox = ""; // Очищаем строку ввода серийника для ввода следующего
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
                case ISerialTasks.ResultCode.comPortBusy:
                    MessageBox.Show("COM порт занят! Возможно запущен UProg.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sendResult), sendResult, null);
            }
        }

        /*
        private int GetCurrentDeviceStatus(string deviceSerial, bool deviceChecked)
        {
            if (!string.IsNullOrEmpty(deviceSerial) && deviceChecked)
                return 2;
            if (!string.IsNullOrEmpty(deviceSerial) && !deviceChecked)
                return 1;
            return 0;
        }
        */
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
                    SearchProgressBar = (int)message.AttachedObject;
                }));

            }
        }
    }
}
