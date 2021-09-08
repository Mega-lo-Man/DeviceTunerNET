using DeviceTunerNET.Core;
using DeviceTunerNET.Core.Mvvm;
using DeviceTunerNET.Modules.ModuleSwitch.Models;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace DeviceTunerNET.Modules.ModuleSwitch.ViewModels
{
    public class ViewSwitchViewModel : RegionViewModelBase
    {
        #region Properties
        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private string _defaultLogin = "admin";
        public string DefaultLogin
        {
            get => _defaultLogin;
            set => SetProperty(ref _defaultLogin, value);
        }

        private string _newLogin = "admin";
        public string NewLogin
        {
            get => _newLogin;
            set => SetProperty(ref _newLogin, value);
        }

        private string _defaultPassword = "admin";
        public string DefaultPassword
        {
            get => _defaultPassword;
            set => SetProperty(ref _defaultPassword, value);
        }

        private string _newPassword = "admin123";
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private string _defaultIP = "192.168.1.239";
        public string DefaultIP
        {
            get => _defaultIP;
            set => SetProperty(ref _defaultIP, value);
        }

        private int _ipMask = 22;
        public int IPMask
        {
            get => _ipMask;
            set => SetProperty(ref _ipMask, value);
        }

        private string _selectedDevice;
        public string SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }

        private string _selectedPrinter;
        public string SelectedPrinter
        {
            get => _selectedPrinter;
            set => SetProperty(ref _selectedPrinter, value);
        }

        private string _printLabelPath = "Resources\\Files\\label25x25switch.dymo";
        public string PrintLabelPath
        {
            get => _printLabelPath;
            set => SetProperty(ref _printLabelPath, value);
        }

        private string _currentItemTextBox = "0";
        public string CurrentItemTextBox
        {
            get => _currentItemTextBox;
            set => SetProperty(ref _currentItemTextBox, value);
        }

        private string _messageForUser = "Подключи \r\n коммутатор";
        public string MessageForUser
        {
            get => _messageForUser;
            set => SetProperty(ref _messageForUser, value);
        }

        private bool _sliderIsChecked = false;
        public bool SliderIsChecked
        {
            get => _sliderIsChecked;
            set => SetProperty(ref _sliderIsChecked, value);
        }

        private string _observeConsole;
        public string ObserveConsole
        {
            get => _observeConsole;
            set => SetProperty(ref _observeConsole, value);
        }

        private ObservableCollection<EthernetSwitch> _switchList;
        public ObservableCollection<EthernetSwitch> SwitchList //Список коммутаторов
        {
            get => _switchList;
            set => SetProperty(ref _switchList, value);
        }

        private ObservableCollection<string> _printers = new ObservableCollection<string>();
        public ObservableCollection<string> Printers
        {
            get => _printers;
            set => SetProperty(ref _printers, value);
        }

        #endregion

        private readonly IEventAggregator _ea;
        private readonly IDataRepositoryService _dataRepositoryService;
        private readonly INetworkTasks _networkTasks;
        private readonly IPrintService _printerService;

        private CancellationTokenSource _tokenSource = null;
        private readonly Dispatcher _dispatcher;
        //private IMessageService _messageService;

        public ViewSwitchViewModel(IRegionManager regionManager,
                                    //IMessageService messageService,
                                    IDataRepositoryService dataRepositoryService,
                                    INetworkTasks networkTasks,
                                    IEventAggregator ea,
                                    IPrintService printService) : base(regionManager)
        {
            _ea = ea;
            _dataRepositoryService = dataRepositoryService;
            _networkTasks = networkTasks;
            _printerService = printService;

            _ea.GetEvent<MessageSentEvent>().Subscribe(MessageReceived);

            SwitchList = new ObservableCollection<EthernetSwitch>();

            CheckedCommand = new DelegateCommand(async () => await StartCommandExecuteAsync(), StartCommandCanExecute);
            UncheckedCommand = new DelegateCommand(StopCommandExecute, StopCommandCanExecute);
            PrintTestLabel = new DelegateCommand(async () => await PrintLabelCommandExecute(), CanPrintLabelCommandExecute);

            Title = "Switch"; // Заголовок вкладки

            _dispatcher = Dispatcher.CurrentDispatcher;

            // Fill ComboBox with available printers
            foreach (var item in _printerService.CommonGetAvailablePrinters())
            {
                Printers.Add(item);
            }

            //Message = messageService.GetMessage();
        }

        #region Commands
        public DelegateCommand CheckedCommand { get; private set; }
        public DelegateCommand UncheckedCommand { get; private set; }
        public DelegateCommand PrintTestLabel { get; private set; }

        private bool StopCommandCanExecute()
        {
            return true;//throw new NotImplementedException();
        }

        private void StopCommandExecute()
        {
            _tokenSource.Cancel();
        }

        private bool StartCommandCanExecute()
        {
            return SwitchList.Count > 0;
        }

        private Task StartCommandExecuteAsync()
        {
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            return Task.Run(() => DownloadLoop(token), token);
        }

        private bool CanPrintLabelCommandExecute()
        {
            return true;
        }

        private Task PrintLabelCommandExecute()
        {
            return Task.Run(() =>
            {
                // Выводим в консоль Printing...
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    ObserveConsole += "Starting print..." + "\r\n";
                }));

                if (_printerService.CommonPrintLabel(SelectedPrinter, @PrintLabelPath, GetPrintingDict(
                    new EthernetSwitch()
                    {
                        AddressIP = "192.168.1.239",
                        CIDR = 22,
                        Designation = "SW1",
                        Serial = "ES50003704",
                        Cabinet = "ШКО1"
                    })))
                {
                    _dispatcher.BeginInvoke(new Action(() =>
                    {
                        ObserveConsole += "Printing module return \"Complete\"" + "\r\n";
                    }));
                }
                else
                {
                    _dispatcher.BeginInvoke(new Action(() =>
                    {
                        ObserveConsole += "Printing module return \"False\"";
                    }));
                }
            });
        }
        #endregion

        // Основной цикл - заливка в каждый коммутатор настроек из списка SwitchList
        private void DownloadLoop(CancellationToken token)
        {

            foreach (EthernetSwitch ethernetSwitch in SwitchList)
            {
                //исключаем коммутаторы уже имеющие серийник (они уже были сконфигурированны)
                if (ethernetSwitch.Serial == null)
                {
                    CurrentItemTextBox = ethernetSwitch.AddressIP;// Вывод адреса коммутатора в UI
                    ethernetSwitch.CIDR = IPMask;

                    if (!_networkTasks.UploadConfigStateMachine(ethernetSwitch, GetSettingsDict(), token))
                    {
                        // Выводим сообщение о прерывании операции
                        _dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageForUser = "Operation aborted!";
                        }));
                        break;
                    }
                    else
                    {
                        _dataRepositoryService.SaveSerialNumber(ethernetSwitch.Id, ethernetSwitch.Serial);
                        _printerService.CommonPrintLabel(SelectedPrinter, PrintLabelPath, GetPrintingDict(ethernetSwitch));
                    }
                    // Обновляем всю коллекцию d UI целиком
                    _dispatcher.BeginInvoke(new Action(() =>
                    {
                        CollectionViewSource.GetDefaultView(SwitchList).Refresh();
                    }));


                }
                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
            SliderIsChecked = false; // Всё! Залили настройки во все коммутаторы. Вырубаем слайдер (пололжение Off)
        }

        private Dictionary<string, string> GetPrintingDict(EthernetSwitch ethSwitch)
        {
            var printDict = new Dictionary<string, string>
            {
                {"ITextObjectIPaddress", ethSwitch.AddressIP},
                {"ITextObjectDesignation", ethSwitch.Designation},
                {"ITextObjectMask", ethSwitch.CIDR.ToString()},
                {"ITextObjectSerial", ethSwitch.Serial},
                {"ITextObjectCabinet", ethSwitch.Cabinet}
            };
            return printDict;
        }

        // Формирование словаря с необходимыми данными для настройки коммутаторов (логин, пароль, адрес по умолчанию и т.п.)
        private Dictionary<string, string> GetSettingsDict()
        {
            var settingsDict = new Dictionary<string, string>
            {
                {"DefaultIPAddress", DefaultIP},
                {"DefaultAdminLogin", DefaultLogin},
                {"DefaultAdminPassword", DefaultPassword},
                {"NewAdminPassword", NewPassword},
                {"NewAdminLogin", NewLogin},
                {"IPmask", IPMask.ToString()}
            };
            return settingsDict;
        }

        private void MessageReceived(Message message)
        {
            if (message.ActionCode == MessageSentEvent.RepositoryUpdated)
            {
                SwitchList.Clear();
                var cabinets = (List<Cabinet>)_dataRepositoryService.GetCabinetsWithDevices<EthernetSwitch>();
                foreach (var cabinet in cabinets)
                {
                    foreach (var item in cabinet.GetDevicesList<EthernetSwitch>()) // масло масляное, в шкафах cabinets не может быть приборов отличных от EthernetSwitch
                    {
                        SwitchList.Add(item);
                    }
                }
            }
            if (message.ActionCode == MessageSentEvent.NeedOfUserAction)
            {
                MessageForUser = message.MessageString;// Обновим информацию для пользователя 
            }
            if (message.ActionCode == MessageSentEvent.StringToConsole)
            {
                ObserveConsole += message.MessageString + "\r\n";// Ответы коммутатора в консоль
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            //do something
        }
    }
}
