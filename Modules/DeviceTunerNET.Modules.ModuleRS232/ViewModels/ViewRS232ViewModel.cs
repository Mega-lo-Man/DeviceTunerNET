using DeviceTunerNET.Services.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Modules.ModuleRS232.ViewModels
{
    public class ViewRS232ViewModel : BindableBase
    {
        private string _message;
        private readonly ISerialTasks _serialTasks;


        #region Commands
        //ShiftAddressesCommand
        public DelegateCommand ShiftAddressesCommand { get; private set; }

        #endregion Commands

        #region Properties

        private ObservableCollection<string> _availableComPorts = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableComPorts
        {
            get { return _availableComPorts; }
            set { SetProperty(ref _availableComPorts, value); }
        }

        private string _currentRS485Port;
        public string CurrentRS485Port
        {
            get { return _currentRS485Port; }
            set
            {
                SetProperty(ref _currentRS485Port, value);
            }
        }

        private string _startAddressTextBox = "";
        public string StartAddressTextBox
        {
            get { return _startAddressTextBox; }
            set { SetProperty(ref _startAddressTextBox, value); }
        }
        #endregion Properties

        public string Title { get; private set; }

        

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        #region Constructor
        public ViewRS232ViewModel(IRegionManager regionManager,
                                  ISerialSender serialSender,
                                  ISerialTasks serialTasks,
                                  IEventAggregator ea)
        {
            Title = "ПНР";

            _serialTasks = serialTasks;

            AvailableComPorts = _serialTasks.GetAvailableCOMPorts();

            ShiftAddressesCommand = new DelegateCommand(async () => await ShiftAddressesCommandExecuteAsync(), ShiftAddressesCommandCanExecute)
                .ObservesProperty(() => CurrentRS485Port)
                .ObservesProperty(() => StartAddressTextBox);
        }

        private bool ShiftAddressesCommandCanExecute()
        {
            if (CurrentRS485Port != null && StartAddressTextBox.Length > 0) return true;
            return false;
        }

        private Task ShiftAddressesCommandExecuteAsync()
        {
            throw new NotImplementedException();
        }
        #endregion Constructor


    }
}
