using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        private const int LAST_ADDRESS = 127;
        private const int FIRST_ADDRESS = 1;

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
            if (_serialSender.SetDeviceRS485Address(_comPort, _rsAddress, newAddress))
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
            if (((TargetAddress + Range) < LAST_ADDRESS) && (TargetAddress > 0) && ((TargetAddress < StartAddress) || (TargetAddress > StartAddress + Range)))
            {
                string _comPort = ComPort;
                byte _startAddress = Convert.ToByte(StartAddress);
                byte _targetAddress = Convert.ToByte(TargetAddress);
                byte _range = Convert.ToByte(Range);
                byte oldEndAddress = (byte)(_startAddress + _range);
                //для сдвига адресов вправо
                for (byte counter = _range; counter >= 0; counter--)
                {
                    byte currentAddress = (byte)(counter + _startAddress);
                    string deviceModel = _serialSender.GetDeviceModel(_comPort, currentAddress);
                    if (deviceModel.Length == 0)
                    {
                        return (int)resultCode.deviceNotRespond;
                    }

                    byte newAddr = (byte)(_targetAddress + counter);
                    if (!_serialSender.SetDeviceRS485Address(_comPort, currentAddress, newAddr))
                    {
                        return (int)resultCode.deviceNotRespond;
                    }
                }
                //для сдвига адресов вправо
            }
            return (int)resultCode.ok;
        }

        public Dictionary<int, string> GetOnlineDevicesDict(string ComPort)
        {
            string _comPort = ComPort;
            var deviceDict = new Dictionary<int, string>();
            for (byte currAddr = FIRST_ADDRESS; currAddr <= LAST_ADDRESS; currAddr++)
            {
                string devType = _serialSender.GetDeviceModel(_comPort, currAddr);
                if (devType.Length > 0)
                {
                    deviceDict.Add(currAddr, devType);
                }
            }
            return deviceDict;
        }

        public IEnumerable<RS485device> GetOnlineDevices(string ComPort)
        {
            var _comPort = ComPort;

            for (byte currAddr = FIRST_ADDRESS; currAddr <= LAST_ADDRESS; currAddr++)
            {
                string devType = _serialSender.GetDeviceModel(_comPort, currAddr);
                if (devType.Length > 0)
                {
                    yield return new RS485device()
                    {
                        AddressRS485 = currAddr,
                        DeviceType = devType
                    };
                }
            }
        }
    }
}
