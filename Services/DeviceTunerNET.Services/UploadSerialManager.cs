using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.Ports;
using DeviceTunerNET.SharedDataModel;
using System;
using System.IO.Ports;
using System.Net;
using System.Windows;
using System.Reflection;


namespace DeviceTunerNET.Services
{
    public class UploadSerialManager(IDialogCaller dialogCaller, IDataRepositoryService repositoryService) : IUploadSerialManager
    {
        private readonly IDialogCaller _dialogCaller = dialogCaller;
        private readonly IDataRepositoryService _repositoryService = repositoryService;
        private IPort _port;

        private void SaveSerial(Device device, string serialNumb)
        {
            device.Serial = serialNumb;

            if (!_repositoryService.SaveSerialNumber(device.Id, device.Serial))
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Clipboard.SetText(device.Serial ?? string.Empty);
                }));

                _dialogCaller.ShowMessage("Не удалось сохранить серийный номер! Он был скопирован в буфер обмена.");
            }
        }

        public string Protocol { get; set; } = string.Empty;
        public string PortName { get; set; } = string.Empty;
        public Action<int> UpdateProgressBar { get; set; }

        public bool QualityControl(IOrionDevice device)
        {

            if (!SetPort())
            {
                return false;
            }
            device.QualityControlPassed = false;
            device.Port = _port;

            try
            {
                device.QualityControlPassed = device.CheckDeviceType();
            }
            catch (Exception ex)
            {
                _dialogCaller.ShowMessage(ex.Message);
                return false;
            }
            device.Port.Dispose();
            if (!device.QualityControlPassed)
            {
                _dialogCaller.ShowMessage("Не удалось настроить прибор: " + device.Model + "; с обозначением: " + device.Designation);

                return false;
            }

            var wasSaved = _repositoryService.SaveQualityControlPassed(device.Id, device.QualityControlPassed);

            return device.QualityControlPassed;
        }

        public bool Upload(IOrionDevice device, string serialNumb)
        {
            if (device is OrionDevice orionDevice)
            {
                var isSutupComplete = false;
                if (!SetPort()) 
                { 
                    return false; 
                }
                orionDevice.Port = _port;
                try
                {
                    isSutupComplete = orionDevice.Setup(UpdateProgressBar);
                }
                catch (Exception ex)
                {
                    _dialogCaller.ShowMessage(ex.Message);
                    return false;
                }
                orionDevice.Port.Dispose();
                if (!isSutupComplete)
                {
                    _dialogCaller.ShowMessage("Не удалось настроить прибор: " + orionDevice.Model + "; с обозначением: " + orionDevice.Designation);
                       
                    return false;
                }
                SaveSerial(orionDevice, serialNumb);
                return true;
            }
            
            return false;
        }

        private bool SetPort()
        {
            if (Protocol.Equals("COM"))
            {
                var port = new ComPort()
                {
                    SerialPort = new SerialPort(PortName ?? "COM1")
                };
                try
                {
                    port.SerialPort.Open();
                } 
                catch (Exception ex)
                {
                    _dialogCaller.ShowMessage(ex.Message);
                    return false;
                }
                _port = port;
                return true;
            }
            if (Protocol.Equals("WIFI"))
            {
                var ip = IPAddress.Parse("10.10.10.1");
                _port = new BolidUdpClient(8100)
                {
                    RemoteServerIp = ip,
                    RemoteServerUdpPort = 12000
                };
                return true;
            }
            return false;
        }

        public void Dispose() => _port?.Dispose();
    }
}
