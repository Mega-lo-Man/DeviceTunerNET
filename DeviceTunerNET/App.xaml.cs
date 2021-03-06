using DeviceTunerNET.Core;
using DeviceTunerNET.DymoModules;
using DeviceTunerNET.Modules.ModuleRS232;
using DeviceTunerNET.Modules.ModuleRS485;
using DeviceTunerNET.Modules.ModuleSwitch;
using DeviceTunerNET.Services;
using DeviceTunerNET.Services.SwitchesStrategies;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.Views;
using DryIoc;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using System.IO.Ports;
using System.Windows;
using DeviceTunerNET.ViewModels;
using System.Collections.Generic;
using System.Collections;

namespace DeviceTunerNET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IEventAggregator _ea;
        private SerialPort _sp;
        /*
        public List<System.Type> MyProperty 
        { 
            get => new List<System.Type>() { typeof(Eltex) };
            
        }

        private List<System.Type> _strategies = new List<System.Type>() { typeof(Eltex) };

        //enum SrvKey { telnetKey, sshKey };
        enum Strategies { eltex };
        */

        protected override Window CreateShell()
        {
            _sp = new SerialPort();
            _ea = Container.Resolve<IEventAggregator>();
            _ea.GetEvent<MessageSentEvent>().Subscribe(MessageReceived);
            return Container.Resolve<MainWindow>();
        }

        private void MessageReceived(Message message)
        {   // A new SerialPort type object needs to be created.
            if (message.ActionCode == MessageSentEvent.UpdateRS485ComPort)
            {
                _sp.PortName = message.MessageString;
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IMessageService, MessageService>();
            containerRegistry.RegisterSingleton<IDataRepositoryService, DataRepositoryService>();
            containerRegistry.Register<IFileDialogService, FileDialogService>();
            containerRegistry.Register<IExcelDataDecoder, ExcelDataDecoder>();
            containerRegistry.Register<IPrintService, DymoModule>();
            containerRegistry.Register<ISwitchConfigUploader, Eltex>();
            containerRegistry.Register<ISerialSender, SerialSender>();
            containerRegistry.Register<ISerialTasks, SerialTasks>();
            
            /*
            containerRegistry.GetContainer().Register<ISender, Telnet_Sender>(serviceKey: SrvKey.telnetKey);
            containerRegistry.GetContainer().Register<ISender, SSH_Sender>(serviceKey: SrvKey.sshKey);
            */
            containerRegistry.RegisterDialog<SerialDialog, SerialDialogViewModel>("SerialDialog");
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ModuleSwitchModule>();
            moduleCatalog.AddModule<ModuleRS485Module>();
            moduleCatalog.AddModule<ModuleRS232Module>();
        }
    }
}
