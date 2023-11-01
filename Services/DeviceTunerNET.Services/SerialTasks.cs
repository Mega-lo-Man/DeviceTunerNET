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

        
    }
}
