using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows.Controls;

namespace DeviceTunerNET.Services
{
    public class SerialTasks : ISerialTasks
    {
        

        private ISerialSender _serialSender;

        private byte _rsAddress = 127;
        private SerialPort _comPort;
        private const int LAST_ADDRESS = 127;
        private const int FIRST_ADDRESS = 1;

        public SerialTasks(ISerialSender serialSender)
        {
            _serialSender = serialSender;
        }

        public ISerialTasks.ResultCode SendConfig<T>(T device, string comPort, int rsAddress)
        {
            _comPort = new SerialPort(comPort);
            try
            {
                _comPort.Open();
            }
            catch
            {
                return ISerialTasks.ResultCode.comPortBusy;
            }
            
            _rsAddress = Convert.ToByte(rsAddress);
            object dev = device;
            if (device.GetType() == typeof(RS485device))
            {
                var result = SendConfigRS485((RS485device)dev);
                _comPort.Close();
                return result;
            }

            if (device.GetType() == typeof(C2000Ethernet))
            {
                var result = SendConfigC2000Ethernet((C2000Ethernet) dev);
                _comPort.Close();
                return result;
            }
            _comPort.Close();
            return ISerialTasks.ResultCode.undefinedError;
        }

        private ISerialTasks.ResultCode SendConfigRS485(RS485device device)
        {
            //var result = CheckOnlineDevice(_comPort, _rsAddress, device.Model);
            var deviceModel = _serialSender.GetDeviceModel(_comPort, _rsAddress);

            var result = CheckDevice(device.Model, deviceModel);

            if (result != ISerialTasks.ResultCode.ok)
            {
                return result;
            }

            var newAddress = Convert.ToByte(device.AddressRS485);
            if (_serialSender.SetDeviceRS485Address(_comPort, _rsAddress, newAddress))
            {
                return ISerialTasks.ResultCode.ok;
            }
            
            return ISerialTasks.ResultCode.undefinedError;
        }

        public ISerialTasks.ResultCode CheckOnlineDevice(string comPort, byte address, string expectedModel)
        {
            _comPort = new SerialPort(comPort);
            try
            {
                _comPort.Open();
            }
            catch
            {
                return ISerialTasks.ResultCode.comPortBusy;
            }

            var deviceModel = _serialSender.GetDeviceModel(_comPort, address);
            _comPort.Close();
            return CheckDevice(expectedModel, deviceModel);
        }

        private ISerialTasks.ResultCode CheckDevice(string expectedModel, string deviceModel)
        {
            if (deviceModel.Length == 0)
            {
                return ISerialTasks.ResultCode.deviceNotRespond;
            }

            if (!IsModelRight(expectedModel, deviceModel))
            {
                return ISerialTasks.ResultCode.deviceTypeMismatch;
            }

            return ISerialTasks.ResultCode.ok;
        }

        private bool IsModelRight(string expectedModel, string receivedModel)
        {
            return receivedModel.ToUpper().Contains(expectedModel.ToUpper());
        }



        private ISerialTasks.ResultCode SendConfigC2000Ethernet(C2000Ethernet device)
        {
            if (device.AddressRS485 == null)
            {
                return ISerialTasks.ResultCode.addressFieldNotValid;
            }
            
            if (!_serialSender.SetC2000EthernetConfig(_comPort, _rsAddress, device))
            {
                return ISerialTasks.ResultCode.errorConfigDownload;
            }

            var newAddress = Convert.ToByte(device.AddressRS485);
            if (_serialSender.SetDeviceRS485Address(_comPort, _rsAddress, newAddress)) 
                return ISerialTasks.ResultCode.ok;

            return ISerialTasks.ResultCode.deviceNotRespond;
        }

        public ObservableCollection<string> GetAvailableCOMPorts()
        {
            return _serialSender.GetAvailableCOMPorts();
        }

        public ISerialTasks.ResultCode ShiftDevicesAddresses(string comPort, int startAddress, int targetAddress, int range)
        {
            if (targetAddress + range >= LAST_ADDRESS ||
                targetAddress <= 0 ||
                targetAddress >= startAddress &&
                targetAddress <= startAddress + range)
                return ISerialTasks.ResultCode.ok;

            _comPort = new SerialPort(comPort) { PortName = comPort };
            try
            {
                _comPort.Open();
            }
            catch
            {
                return ISerialTasks.ResultCode.comPortBusy;
            }
            
            //var _comPort = comPort;
            var _startAddress = Convert.ToByte(startAddress);
            var _targetAddress = Convert.ToByte(targetAddress);
            var _range = Convert.ToByte(range);
            //byte oldEndAddress = (byte)(_startAddress + _range);
            //для сдвига адресов вправо
            for (var counter = _range; counter >= 0; counter--)
            {
                var currentAddress = (byte)(counter + _startAddress);
                var deviceModel = _serialSender.GetDeviceModel(_comPort, currentAddress);
                if (deviceModel.Length == 0)
                {
                    _comPort.Close();
                    return ISerialTasks.ResultCode.deviceNotRespond;
                }

                var newAddr = (byte)(_targetAddress + counter);
                if (!_serialSender.SetDeviceRS485Address(_comPort, currentAddress, newAddr))
                {
                    _comPort.Close();
                    return ISerialTasks.ResultCode.deviceNotRespond;
                }
            }
            //для сдвига адресов вправо
            _comPort.Close();
            return ISerialTasks.ResultCode.ok;
        }

        public IEnumerable<RS485device> GetOnlineDevices(string comPort)
        {
            _comPort = new SerialPort(comPort);
            try
            {
                _comPort.Open();
            }
            catch
            {
                yield break;
            }
            //var _comPort = ComPort;

            for (byte currAddr = FIRST_ADDRESS; currAddr <= LAST_ADDRESS; currAddr++)
            {
                var devType = _serialSender.GetDeviceModel(_comPort, currAddr);
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
