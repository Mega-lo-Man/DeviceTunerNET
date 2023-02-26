using DeviceTunerNET.SharedDataModel.Devices;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DeviceTunerNET.SharedDataModel.Devices.IOrionNetTimeouts;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public abstract class OrionDevice : RS485device, IOrionDevice
    {
        private const int ResponseNewAddressOffset = 2;
        private const int ResponseDeviceModelOffset = 1;
        private const int TransmissionAttempts = 3; // кол-во попыток получить модель прибора
        private readonly uint _defaultAddress = 127;

        public int ModelCode { get; set; }     
        
        public SerialPort ComPort
        {
            get;
            set;
        }

        public bool ChangeDeviceAddress(byte newDeviceAddress)
        {

            byte[] cmdString = GetChangeAddressPacket(newDeviceAddress);

            var result = OrionNet.AddressTransaction(ComPort, (byte)AddressRS485, cmdString, Timeouts.addressChanging);

            if (result.Length <= ResponseNewAddressOffset)
                throw new Exception("Device response was not valid or null");

            return result[ResponseNewAddressOffset] == newDeviceAddress;
        }

        public bool SetAddress()
        {
            byte[] cmdString = GetChangeAddressPacket((byte)AddressRS485);

            var result = OrionNet.AddressTransaction(ComPort, (byte)_defaultAddress, cmdString, Timeouts.addressChanging);

            if (result == null || result?.Length <= ResponseNewAddressOffset)
                return false;

            return result[ResponseNewAddressOffset] == (byte)AddressRS485;
        }

        private byte[] GetChangeAddressPacket(byte address)
        {
            // формируем команду на отправку
            return new byte[]
            {
                (byte)OrionCommands.ChangeAddress,
                address,
                address
            };
        }

        public byte GetModelCode(byte deviceAddress)
        {
            // формируем команду на отправку
            var cmdString = new byte[] { (byte)OrionCommands.GetModel, 0x00, 0x00 };

            for (int i = 0; i < TransmissionAttempts; i++)
            {
                var deviceResponse = OrionNet.AddressTransaction(ComPort, deviceAddress, cmdString, Timeouts.readModel);

                return deviceResponse[ResponseDeviceModelOffset];
            }
            return 0xFF;
        }

        public virtual void WriteBaseConfig(SerialPort serialPort, Action<int> progressStatus)
        {
        }
    }
}
