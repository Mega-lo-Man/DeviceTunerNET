using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.Utils;
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
        private static byte commandCounter;

        protected OrionDevice(IPort port)
        {
            Port = port;
        }

        public int ModelCode { get; set; }     
        
        public IPort Port
        {
            get;
            set;
        }

        public bool ChangeDeviceAddress(byte newDeviceAddress)
        {

            byte[] cmdString = GetChangeAddressPacket(newDeviceAddress);

            var result = AddressTransaction((byte)AddressRS485, cmdString, Timeouts.addressChanging);

            if (result.Length <= ResponseNewAddressOffset)
                throw new Exception("Device response was not valid or null");

            return result[ResponseNewAddressOffset] == newDeviceAddress;
        }

        public bool SetAddress()
        {
            byte[] cmdString = GetChangeAddressPacket((byte)AddressRS485);

            var result = AddressTransaction((byte)_defaultAddress, cmdString, Timeouts.addressChanging);

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
                var deviceResponse = AddressTransaction(deviceAddress, cmdString, Timeouts.readModel);

                return deviceResponse[ResponseDeviceModelOffset];
            }
            return 0xFF;
        }

        public virtual void WriteBaseConfig(Action<int> progressStatus)
        {
        }

        public byte[] AddressTransaction(byte address,
                                         byte[] sendArray,
                                         IOrionNetTimeouts.Timeouts timeout)
        {

            var addr = new[] { address };
            var sendCommand = ArraysHelper.CombineArrays(addr, sendArray);
            var completePacket = GetComplitePacket(sendCommand);
            var response = Port.Send(completePacket);

            return GetResponseWithoutAuxiliaryData(response);
        }

        private static byte[] GetResponseWithoutAuxiliaryData(byte[] responseArray)
        {
            var response = responseArray.ToList();
            // Удаляем последний байт (CRC8)
            response.RemoveAt(response.Count - 1);
            // Удаляем первый байт (Адрес ответившего устройства)
            response.RemoveAt(0);
            // Удаляем второй байт (Длина посылки)
            response.RemoveAt(1);

            return response.ToArray();
        }

        private byte[] GetComplitePacket(/*SerialPort serialPort, */byte[] sendArray)
        {
            byte bytesCounter = 2; //сразу начнём считать с двойки, т.к. всё равно придётся добавить два байта(сам байт длины команды, и счётчик команд)
            var lst = new List<byte>();
            foreach (var bt in sendArray)
            {
                lst.Add(bt);
                bytesCounter++;
            }

            lst.Insert(1, bytesCounter); //вставляем вторым байтом в пакет длину всего пакета + байт длины пакета
            lst.Insert(2, commandCounter); //вставляем третьим байтом в пакет счётчик команд
            var cmd = lst.ToArray();
 //           Port.Send(cmd);
 //           var response = Port.Send(OrionCRC.GetCrc8(cmd));
            
            commandCounter += (byte)(bytesCounter + 0x01); // увеличиваем счётчик комманд на кол-во отправленных байт

            return cmd;
        }

        
    }
}
