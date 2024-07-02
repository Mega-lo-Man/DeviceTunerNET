using DeviceTunerNET.SharedDataModel.CustomExceptions;
using DeviceTunerNET.SharedDataModel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static DeviceTunerNET.SharedDataModel.Devices.IOrionNetTimeouts;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class OrionDevice : RS485device, IOrionDevice
    {
        private const int ResponseNewAddressOffset = 2;
        private const int ResponseDeviceModelOffset = 1;
        protected readonly uint defaultAddress = 127;
        private static byte commandCounter;

        public OrionDevice(IPort port)
        {
            Port = port;
        }

        public static int Code;
        public int ModelCode { get => Code; }

        public byte[] Response { get; private set; }

        public bool ChangeDeviceAddress(byte newDeviceAddress)
        {

            byte[] cmdString = GetChangeAddressPacket(newDeviceAddress);

            var result = AddressTransaction((byte)AddressRS485, cmdString, Timeouts.addressChanging);

            if (result.Length <= ResponseNewAddressOffset)
            {
                throw new InvalidDeviceResponseException(result, "Device response was not valid or null");
            }
            var success = result[ResponseNewAddressOffset] == newDeviceAddress;

            return success;
        }

        public bool SetAddress()
        {
            byte[] cmdString = GetChangeAddressPacket((byte)AddressRS485);

            var result = AddressTransaction((byte)defaultAddress, cmdString, Timeouts.addressChanging);

            if (result == null || result?.Length <= ResponseNewAddressOffset)
                return false;

            return result[ResponseNewAddressOffset] == (byte)AddressRS485;
        }

        public void Reboot()
        {
            byte[] completePacket = GetCompletePacket((byte)AddressRS485, GetRebootPacket());
            Port.Timeout = (int)Timeouts.readModel;
            Port.SendWithoutСonfirmation(completePacket);
        }

        private byte[] GetRebootPacket()
        {
            return
            [
                (byte)OrionCommands.Reboot,
                0x00,
                0x00,
            ];
        }

        private byte[] GetChangeAddressPacket(byte address)
        {
            // формируем команду на отправку
            return
            [
                (byte)OrionCommands.ChangeAddress,
                address,
                address
            ];
        }

        public bool IsDeviceOnline()
        {
            Port.MaxRepetitions = 3;
            var result = GetModelCode((byte)AddressRS485, out var deviceCode);
            Port.MaxRepetitions = 15;
            if(deviceCode != ModelCode)
                return false;
            return result;
        }

        public bool GetModelCode(byte deviceAddress, out byte deviceCode)
        {
            // формируем команду на отправку
            var cmdString = new byte[] { (byte)OrionCommands.GetModel, 0x00, 0x00 };
            
            deviceCode = 0;

            Response = AddressTransaction(deviceAddress, cmdString, Timeouts.readModel);

            if(Response.Length == 0)
                return false;
                
            deviceCode = Response[ResponseDeviceModelOffset];
            return true;
        }

        public virtual void WriteBaseConfig(Action<int> progressStatus)
        {

        }

        public virtual void WriteConfig(Action<int> progressStatus)
        {

        }

        public byte[] AddressTransaction(byte address,
                                         byte[] sendArray,
                                         IOrionNetTimeouts.Timeouts timeout)
        {
            byte[] completePacket = GetCompletePacket(address, sendArray);
            Port.Timeout = (int)timeout;
            var response = Port.Send(completePacket);

            return GetResponseWithoutAuxiliaryData(response);
        }

        private byte[] GetCompletePacket(byte address, byte[] sendArray)
        {
            var addr = new[] { address };
            var sendCommand = ArraysHelper.CombineArrays(addr, sendArray);
            var completePacket = GetComplitePacket(sendCommand);
            return completePacket;
        }

        private static byte[] GetResponseWithoutAuxiliaryData(byte[] responseArray)
        {
            var response = responseArray.ToList();
            if (responseArray.Length < 2)
                return responseArray;
            // Удаляем последний байт (CRC8)
            response.RemoveAt(response.Count - 1);
            // Удаляем первый байт (Адрес ответившего устройства)
            response.RemoveAt(0);
            // Удаляем второй байт (Длина посылки)
            response.RemoveAt(1);

            return response.ToArray();
        }

        private byte[] GetComplitePacket(byte[] sendArray)
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

            commandCounter += (byte)(bytesCounter + 0x01); // увеличиваем счётчик комманд на кол-во отправленных байт

            return cmd;
        }

        public virtual bool Setup(
            Action<int> updateProgressBar, 
            int modelCode = 0)
        {
            Code = modelCode;
            if(!GetModelCode((byte)defaultAddress, out var deviceCode))
                return false;

            if (deviceCode != ModelCode)
                return false;

            SetAddress();

            if (!GetModelCode((byte)AddressRS485, out var deviceCodeWithNewAddress))
                return false;

            if (deviceCodeWithNewAddress != ModelCode)
                return false;

            WriteBaseConfig(updateProgressBar);

            return true;
        }
    }
}
