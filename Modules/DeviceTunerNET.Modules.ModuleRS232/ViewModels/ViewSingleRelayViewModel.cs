using DeviceTunerNET.Modules.ModulePnr.Interfaces;
using DeviceTunerNET.SharedDataModel.ElectricModules;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Modules.ModulePnr.ViewModels
{
    public class ViewSingleRelayViewModel : BindableBase, IControlViewModel
    {
        private string _labelText = string.Empty;
        public string LabelText
        { 
            get => _labelText; 
            set
            {
                _labelText = value;
                SetProperty(ref _labelText, value);
            }
        }

        private bool _isControlEnabled = false;
        public bool IsControlEnabled
        {
            get => _isControlEnabled;
            set
            {
                _isControlEnabled = value;
                SetProperty(ref _isControlEnabled, value);
            }
        }

        public Relay RelayInstance { get; set; }

        #region Commands
        public DelegateCommand CheckedCommand { get; private set; }
        public DelegateCommand UncheckedCommand { get; private set; }
        #endregion Commands

        #region Constructor
        public ViewSingleRelayViewModel(Relay relay)
        {
            RelayInstance = relay;
            LabelText = RelayInstance.RelayIndex.ToString();

            CheckedCommand = new DelegateCommand(async () => await CheckedCommandExecuteAsync(), CheckedCommandCanExecute);

            UncheckedCommand = new DelegateCommand(async () => await UncheckedCommandExecuteAsync(), UncheckedCommandCanExecute);
        }
        #endregion Constructor

        private bool UncheckedCommandCanExecute()
        {
            return true;
        }

        private Task UncheckedCommandExecuteAsync()
        {
            return Task.Run(() => RelayInstance.TurnOff());
        }

        private bool CheckedCommandCanExecute()
        {
            return true;
        }

        private Task CheckedCommandExecuteAsync()
        {
            return Task.Run(() =>
            {
                RelayInstance.TurnOn();
            });
        }
    }
}
