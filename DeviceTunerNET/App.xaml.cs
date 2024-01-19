using DeviceTunerNET.Core;
using DeviceTunerNET.DymoModules;
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using DeviceTunerNET.Modules.ModulePnr;
using Serilog;
using Serilog.Formatting.Compact;

namespace DeviceTunerNET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IEventAggregator _ea;
        private SerialPort _sp;

        enum SrvKey { telnetKey, sshKey };
        protected override Window CreateShell()
        {
            _sp = new SerialPort();
            _ea = Container.Resolve<IEventAggregator>();
            _ea.GetEvent<MessageSentEvent>().Subscribe(MessageReceived);
            return Container.Resolve<MainWindow>();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool turnOn);
        
        protected override void OnStartup(StartupEventArgs e)
        {
            var appName = Process.GetCurrentProcess().ProcessName;
            var sameProcesses = Process.GetProcessesByName(appName);
            
            if(sameProcesses != null && sameProcesses.Length > 1)
            {
                SwitchToThisWindow(sameProcesses[1].MainWindowHandle, true);
                Application.Current.Shutdown();
            }

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt")
                .CreateLogger();
            Log.Information($"Application startup.");

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Error($"The application is closed.");
            // Close and flush the Serilog logger when the application exits
            Log.CloseAndFlush();

            base.OnExit(e);
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
            containerRegistry.Register<INetworkUtils, NetworkUtils>();
            containerRegistry.Register<IDeviceGenerator, DeviceGenerator>();
            containerRegistry.Register<IDeviceSearcher, BolidDeviceSearcher>();
            containerRegistry.Register<IAddressChanger, BolidAddressChanger>();

            containerRegistry.GetContainer().Register<ISender, EltexTelnet>(serviceKey: SrvKey.telnetKey);
            containerRegistry.GetContainer().Register<ISender, EltexSsh>(serviceKey: SrvKey.sshKey);

            containerRegistry.Register<ITftpServerManager, TftpServerManager>();
            containerRegistry.Register<IConfigParser, ConfigParser>();
            
            containerRegistry.RegisterDialog<SerialDialog, SerialDialogViewModel>("SerialDialog");
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ModuleSwitchModule>();
            moduleCatalog.AddModule<ModuleRS485Module>();
            moduleCatalog.AddModule<ModulePnrModule>();
        }
    }
}
