using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.Ports;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DeviceTunerNET.Services
{
    public class DeviceConfigUploader : IDeviceConfigUploader
    {
        private SerialPort _serialPort;

        public string PortName { get; set; }
        public string Protocol { get; set; }

        public Action<int> Progress { get; set; }

        public bool Upload(RS485device device, string serialNumb)
        {
            
            try
            {

                if (device is OrionDevice orionDevice)
                {
                    if (Protocol.Equals("COM"))
                    {
                        _serialPort = new SerialPort(PortName);
                        orionDevice.Port = new ComPort() { SerialPort = _serialPort };
                        _serialPort.Open();
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


                    orionDevice.SetAddress();

                    if (orionDevice.GetModelCode((byte)orionDevice.AddressRS485) != orionDevice.ModelCode)
                    {
                        throw new Exception("Device code with new address is not equal with expected code!");
                    }

                    orionDevice.WriteBaseConfig(Progress);
                    _serialPort.Close();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _serialPort?.Close();
                MessageBox.Show(ex.Message);

                return false;
            }
            finally
            {
                _serialPort?.Close();
            }
        }
    }
}
