using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services
{
    public class SerialTasks : ISerialTasks
    {
        enum resultCode 
        {
            undefinedError = 0,
            deviceTypeMismatch = -1,
            addressFieldNotValid = -2,
            deviceNotRespond = -3,
            ok = 1
        }

        private ISerialSender _serialSender;

        private byte _rsAddress = 127;
        private string _comPort = "";
        private const int LAST_USAGE_ADDRESS = 126;

        public SerialTasks(ISerialSender serialSender)
        {
            _serialSender = serialSender;
        }

        public int SendConfig<T>(T device, string comPort, int rsAddress)
        {
            _comPort = comPort;
            _rsAddress = Convert.ToByte(rsAddress);
            object dev = device;
            if (device.GetType() == typeof(RS485device))
                return SendConfigRS485((RS485device)dev);
            if (device.GetType() == typeof(C2000Ethernet))
                return SendConfigC2000Ethernet((C2000Ethernet)dev);
            return 0;
        }

        private int SendConfigRS485(RS485device device)
        {
            if (device.AddressRS485 == null)
            {
                return (int)resultCode.addressFieldNotValid;
            }
            
            string deviceModel = _serialSender.GetDeviceModel(_comPort, _rsAddress);
            if (deviceModel.Length == 0)
            {
                return (int)resultCode.deviceNotRespond;
            }
            
            if (!device.Model.Contains(deviceModel))
            {
                return (int)resultCode.deviceTypeMismatch;
            }

            byte newAddress = Convert.ToByte(device.AddressRS485);
            if(_serialSender.SetDeviceRS485Address(_comPort, _rsAddress, newAddress))
            {
                return (int)resultCode.ok;
            }

            return (int)resultCode.undefinedError;
        }

        private int SendConfigC2000Ethernet(C2000Ethernet device)
        {
            if (device.AddressRS232 == null)
            {
                return (int)resultCode.addressFieldNotValid;
            }

            string deviceModel = _serialSender.GetDeviceModel(_comPort, _rsAddress);
            if (deviceModel.Length == 0)
            {
                return (int)resultCode.deviceNotRespond;
            }

            if (!device.Model.Contains(deviceModel))
            {
                return (int)resultCode.deviceTypeMismatch;
            }

            if (_serialSender.SetC2000EthernetConfig(_comPort, _rsAddress, device))
            {
                return (int)resultCode.ok;
            }

            return (int)resultCode.undefinedError;
        }

        public ObservableCollection<string> GetAvailableCOMPorts()
        {
            return _serialSender.GetAvailableCOMPorts();
        }

        public int ShiftDevicesAddresses(string ComPort, int StartAddress, int TargetAddress, int Range)
        {
            if((TargetAddress + Range) <= LAST_USAGE_ADDRESS)
            {
                string _comPort = ComPort;
                byte _startAddress = Convert.ToByte(StartAddress);
                byte _targetAddress = Convert.ToByte(TargetAddress);
                byte _range = Convert.ToByte(Range);
                byte oldEndAddress = (byte)(_startAddress + _range);

                for(byte currAddr = oldEndAddress; currAddr >= _startAddress; currAddr--)
                {
                    string deviceModel = _serialSender.GetDeviceModel(_comPort, currAddr);
                    if (deviceModel.Length == 0)
                    {
                        return (int)resultCode.deviceNotRespond;
                    }

                    byte newAddr = (byte)(currAddr + _range);
                    if (_serialSender.SetDeviceRS485Address(_comPort, currAddr, newAddr))
                    {
                        return (int)resultCode.ok;
                    }
                }
            }
            return (int)resultCode.undefinedError;
        }
    }
}
