using DeviceTunerNET.Core;
using DeviceTunerNET.Core.Mvvm;
using DeviceTunerNET.Modules.ModuleSwitch.Models;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.Services.SwitchesStrategies;
using DeviceTunerNET.SharedDataModel;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace DeviceTunerNET.Modules.ModuleSwitch.ViewModels
{
    public partial class ViewSwitchViewModel : RegionViewModelBase
    {
        private readonly IEventAggregator _ea;
        private readonly IDataRepositoryService _dataRepositoryService;
        private readonly ISwitchConfigUploader _configUploader;
        private readonly IPrintService _printService;

        private CancellationTokenSource _tokenSource = null;
        private readonly Dispatcher _dispatcher;

        public ViewSwitchViewModel(IRegionManager regionManager,
                                   IDataRepositoryService dataRepositoryService,
                                   ISwitchConfigUploader switchFactory,
                                   IEventAggregator ea,
                                   IPrintService printService) : base(regionManager)
        {
            _ea = ea;
            _dataRepositoryService = dataRepositoryService;
            _configUploader = switchFactory;
            _printService = printService;

            _ea.GetEvent<MessageSentEvent>().Subscribe(MessageReceived);

            SwitchList = new ObservableCollection<EthernetSwitch>();

            CheckedCommand = new DelegateCommand(async () => await StartCommandExecuteAsync(), StartCommandCanExecute);
            UncheckedCommand = new DelegateCommand(StopCommandExecute, StopCommandCanExecute);
            PrintTestLabel = new DelegateCommand(async () => await PrintLabelCommandExecute(), CanPrintLabelCommandExecute);

            Title = "Switch"; // Заголовок вкладки

            _dispatcher = Dispatcher.CurrentDispatcher;

            // Fill ComboBox with available printers
            foreach (var item in _printService.CommonGetAvailablePrinters())
            {
                Printers.Add(item);
            }

            SelectedPrinter = Printers?.FirstOrDefault();

            IsCheckedByCabinets = true;

            AllowPrintLabel = true;

            AvailableStrategies = new ObservableCollection<string>() { "Switch1", "Switch2", "Switch3" };

            SelectedStrategy = AvailableStrategies?.FirstOrDefault();
        }

        

        #region Commands
        public DelegateCommand CheckedCommand { get; }
        public DelegateCommand UncheckedCommand { get; }
        public DelegateCommand PrintTestLabel { get; }

        private bool StopCommandCanExecute()
        {
            return true;//throw new NotImplementedException();
        }

        private void StopCommandExecute()
        {
            ObserveConsole += "User canceled operation." + "\r\n";
            _tokenSource?.Cancel();
        }

        private bool StartCommandCanExecute()
        {
            return IsCanDoStart;
        }

        private Task StartCommandExecuteAsync()
        {
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            return Task.Run(() => DownloadLoop(token), token);
        }

        private bool CanPrintLabelCommandExecute()
        {
            return IsCanDoPrint;
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

                if (_printService.CommonPrintLabel(SelectedPrinter, @PrintLabelPath, GetPrintingDict(
                    new EthernetSwitch(null)
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
            if (IsCheckedByCabinets)
            {
                if (SelectedDevice == null)
                    return;

                var ethernetSwitch = SelectedDevice;
                //исключаем коммутаторы уже имеющие серийник (они уже были сконфигурированны)
                if (ethernetSwitch?.Serial == null)
                {
                    if (Download(token, ethernetSwitch))
                    {
                    }
                }
                if (token.IsCancellationRequested)
                { 
                    return;
                }
            }
            if (IsCheckedByArea)
            {
                

                foreach (var ethernetSwitch in SwitchList)
                {
                    //исключаем коммутаторы уже имеющие серийник (они уже были сконфигурированны)
                    if (ethernetSwitch.Serial == null)
                    {
                        if (Download(token, ethernetSwitch)) break;
                    }
                    if (token.IsCancellationRequested)
                    {
                        
                        return;
                    }
                }
            }
            
            SliderIsChecked = false; // Всё! Залили настройки во все коммутаторы. Вырубаем слайдер (пололжение Off)
        }

        private bool Download(CancellationToken token, EthernetSwitch ethernetSwitch)
        {
            CurrentItemTextBox = ethernetSwitch.AddressIP; // Вывод адреса коммутатора в UI
            ethernetSwitch.CIDR = IPMask;

            //var completeEthernetSwitch = _strategies.GetInstance<Eltex>().SendConfig(ethernetSwitch, GetSettingsDict(), token);
            var completeEthernetSwitch = _configUploader.SendConfig(ethernetSwitch, GetSettingsDict(ethernetSwitch), token);
            if (completeEthernetSwitch == null)
            {
                // Выводим сообщение о прерывании операции
                _dispatcher.BeginInvoke(new Action(() => { MessageForUser = "Operation aborted!"; }));
                return true;
            }

            if (!_dataRepositoryService.SaveSerialNumber(completeEthernetSwitch.Id, completeEthernetSwitch.Serial))
            {
                _dispatcher.BeginInvoke(new Action(() => { Clipboard.SetText(completeEthernetSwitch.Serial ?? string.Empty); })); 
                MessageBox.Show("Не удалось сохранить серийный номер! Он был скопирован в буфер обмена.");
            }

            if (AllowPrintLabel)
            {
                _printService.CommonPrintLabel(SelectedPrinter, PrintLabelPath, GetPrintingDict(completeEthernetSwitch));
            }

            // Обновляем всю коллекцию в UI целиком
            _dispatcher.BeginInvoke(new Action(() => { CollectionViewSource.GetDefaultView(SwitchList).Refresh(); }));
            return false;
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
        private Dictionary<string, string> GetSettingsDict(EthernetSwitch ethernetSwitch)
        {
            var settingsDict = new Dictionary<string, string>
            {
                /*
                {"DefaultIPAddress", DefaultIP},
                {"DefaultAdminLogin", DefaultLogin},
                {"DefaultAdminPassword", DefaultPassword},
                {"NewAdminPassword", NewPassword},
                {"NewAdminLogin", NewLogin},
                {"IPmask", IPMask.ToString()}
                */
                {"%%DEFAULT_IP_ADDRESS%%", DefaultIP},
                {"%%DEFAULT_ADMIN_LOGIN%%", DefaultLogin},
                {"%%DEFAULT_ADMIN_PASSWORD%%", DefaultPassword},
                {"%%NEW_ADMIN_PASSWORD%%", NewPassword},
                {"%%NEW_ADMIN_LOGIN%%", NewLogin},
                {"%%IP_MASK%%", IPMask.ToString()},
                {"%%NEW_IP_ADDRESS%%", ethernetSwitch.AddressIP },
                {"%%HOST_NAME%%", ethernetSwitch.Designation }
            };
            return settingsDict;
        }

        private void MessageReceived(Message message)
        {
            if (message.ActionCode == MessageSentEvent.RepositoryUpdated)
            {
                SwitchList.Clear();
                var cabinets = (List<Cabinet>)_dataRepositoryService.GetCabinetsWithDevices<EthernetSwitch>();
                foreach (var item in cabinets.SelectMany(cabinet => cabinet.GetDevicesList<EthernetSwitch>()))
                {
                    SwitchList.Add(item);
                }

                if (SwitchList.Count > 0)
                    IsCanDoStart = true;
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
