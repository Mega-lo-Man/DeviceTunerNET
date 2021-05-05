using DeviceTunerNET.Core;
using DeviceTunerNET.Core.Mvvm;
using DeviceTunerNET.Modules.ModuleSwitch.Models;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private string _defaultLogin = "admin";
        public string DefaultLogin
        {
            get { return _defaultLogin; }
            set { SetProperty(ref _defaultLogin, value); }
        }

        private string _newLogin = "admin";
        public string NewLogin
        {
            get { return _newLogin; }
            set { SetProperty(ref _newLogin, value); }
        }

        private string _defaultPassword = "admin";
        public string DefaultPassword
        {
            get { return _defaultPassword; }
            set { SetProperty(ref _defaultPassword, value); }
        }

        private string _newPassword = "admin123";
        public string NewPassword
        {
            get { return _newPassword; }
            set { SetProperty(ref _newPassword, value); }
        }

        private string _defaultIP = "192.168.1.239";
        public string DefaultIP
        {
            get { return _defaultIP; }
            set { SetProperty(ref _defaultIP, value); }
        }

        private int _ipMask = 22;
        public int IPMask
        {
            get { return _ipMask; }
            set { SetProperty(ref _ipMask, value); }
        }

        private string _selectedDevice;
        public string SelectedDevice
        {
            get { return _selectedDevice; }
            set { SetProperty(ref _selectedDevice, value); }
        }

        private string _selectedPrinter;
        public string SelectedPrinter
        {
            get { return _selectedPrinter; }
            set { SetProperty(ref _selectedPrinter, value); }
        }

        private string _currentItemTextBox = "0";
        public string CurrentItemTextBox
        {
            get { return _currentItemTextBox; }
            set { SetProperty(ref _currentItemTextBox, value); }
        }

        private string _messageForUser = "Подключи \r\n коммутатор";
        public string MessageForUser
        {
            get { return _messageForUser; }
            set { SetProperty(ref _messageForUser, value); }
        }

        private bool _sliderIsChecked = false;
        public bool SliderIsChecked
        {
            get { return _sliderIsChecked; }
            set { SetProperty(ref _sliderIsChecked, value); }
        }

        private string _observeConsole;
        public string ObserveConsole
        {
            get { return _observeConsole; }
            set { SetProperty(ref _observeConsole, value); }
        }

        private ObservableCollection<EthernetSwitch> _switchList;
        public ObservableCollection<EthernetSwitch> SwitchList //Список коммутаторов
        {
            get { return _switchList; }
            set { SetProperty(ref _switchList, value); }
        }

        private ObservableCollection<string> _printers = new ObservableCollection<string>();
        public ObservableCollection<string> Printers
        {
            get { return _printers; }
            set { SetProperty(ref _printers, value); }
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

            Title = "Switch"; // Заголовок вкладки

            _dispatcher = Dispatcher.CurrentDispatcher;

            // Fill ComboBox with available printers
            foreach (string item in _printerService.CommonGetAvailablePrinters())
            {
                Printers.Add(item);
            }

            //Message = messageService.GetMessage();
        }

        #region Commands
        public DelegateCommand CheckedCommand { get; private set; }
        public DelegateCommand UncheckedCommand { get; private set; }

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
            if (SwitchList.Count > 0) // В списке для настройки есть коммутаторы?
                return true;
            else return false;
        }

        private Task StartCommandExecuteAsync()
        {
            _tokenSource = new CancellationTokenSource();
            CancellationToken token = _tokenSource.Token;
            return Task.Run(() => DownloadLoop(token));
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
                        _printerService.CommonPrintLabel(SelectedPrinter, 0, GetPrintingDict(ethernetSwitch));
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
            Dictionary<string, string> printDict = new Dictionary<string, string>();
            printDict.Add("ITextObjectIPaddress", ethSwitch.AddressIP);
            printDict.Add("ITextObjectDesignation", ethSwitch.Designation);
            printDict.Add("ITextObjectMask", ethSwitch.CIDR.ToString()); ;
            printDict.Add("ITextObjectSerial", ethSwitch.Serial);
            return printDict;        
        }

        // Формирование словаря с необходимыми данными для настройки коммутаторов (логин, пароль, адрес по умолчанию и т.п.)
        private Dictionary<string, string> GetSettingsDict()
        {
            Dictionary<string, string> settingsDict = new Dictionary<string, string>();
            settingsDict.Add("DefaultIPAddress", DefaultIP);
            settingsDict.Add("DefaultAdminLogin", DefaultLogin);
            settingsDict.Add("DefaultAdminPassword", DefaultPassword);
            settingsDict.Add("NewAdminPassword", NewPassword);
            settingsDict.Add("NewAdminLogin", NewLogin);
            settingsDict.Add("IPmask", IPMask.ToString());
            return settingsDict;
        }

        private void MessageReceived(Message message)
        {
            if (message.ActionCode == MessageSentEvent.RepositoryUpdated)
            {
                SwitchList.Clear();
                List<Cabinet> cabinets = (List<Cabinet>)_dataRepositoryService.GetCabinetsWithDevices<EthernetSwitch>();
                foreach (Cabinet cabinet in cabinets)
                {
                    foreach (EthernetSwitch item in cabinet.GetDevicesList<EthernetSwitch>()) // масло масляное, в шкафах cabinets не может быть приборов отличных от EthernetSwitch
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
