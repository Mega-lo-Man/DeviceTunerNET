using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel.Devices;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DeviceTunerNET.Services.Interfaces.IOrionCommon;

namespace DeviceTunerNET.Services
{
    public class OrionCommon : IOrionCommon
    {
        private readonly IOrionNet _orionNet;

        private readonly Dictionary<byte, string> _bolidDict = new()
        {
            { 1, "Сигнал-20" },
            { 2, "Сигнал-20П, Сигнал-20П исп.01" },
            { 3, "С2000-СП1, С2000-СП1 исп.01" },
            { 4, "С2000-4" },
            { 7, "С2000-К" },
            { 8, "С2000-ИТ" },
            { 9, "С2000-КДЛ" },
            { 10, "С2000-БИ/БКИ" },
            { 11, "Сигнал-20(вер. 02)" },
            { 13, "С2000-КС" },
            { 14, "С2000-АСПТ" },
            { 15, "С2000-КПБ" },
            { 16, "С2000-2" },
            { 19, "УО-ОРИОН" },
            { 20, "Рупор" },
            { 21, "Рупор-Диспетчер исп.01" },
            { 22, "С2000-ПТ" },
            { 24, "УО-4С" },
            { 25, "Поток-3Н" },
            { 26, "Сигнал-20М" },
            { 28, "С2000-БИ-01" },
            { 29, "С2000-Ethernet" },
            { 30, "Рупор-01" },
            { 31, "С2000-Adem" },
            { 33, "РИП-12 исп.50, РИП-12 исп.51, РИП-12 без исполнения" },
            { 34, "Сигнал-10" },
            { 36, "С2000-ПП" },
            { 38, "РИП-12 исп.54" },
            { 39, "РИП-24 исп.50, РИП-24 исп.51" },
            { 41, "С2000-КДЛ-2И" },
            { 43, "С2000-PGE" },
            { 44, "С2000-БКИ" },
            { 45, "Поток-БКИ" },
            { 46, "Рупор-200" },
            { 47, "С2000-Периметр" },
            { 48, "МИП-12" },
            { 49, "МИП-24" },
            { 53, "РИП-48 исп.01" },
            { 54, "РИП-12 исп.56" },
            { 55, "РИП-24 исп.56" },
            { 59, "Рупор исп.02" },
            { 61, "С2000-КДЛ-Modbus" },
            { 66, "Рупор исп.03" },
            { 67, "Рупор-300" }
        };
        

        public OrionCommon(IOrionNet orionNet)
        {
            _orionNet = orionNet;
        }

        public byte CurrentDeviceAdderess { get; set; }

        public bool SetDeviceAddress(SerialPort comPortName, byte deviceAddress, byte newDeviceAddress)
        {
            var sPort = comPortName;
            // формируем команду на отправку
            var cmdString = new byte[] { 0x0F, newDeviceAddress, newDeviceAddress };

            var result = _orionNet.AddressTransaction(sPort, 
                                                      deviceAddress, 
                                                      cmdString, 
                                                      IOrionNetTimeouts.Timeouts.addressChanging);

            if (result.Length <= 1)
                return false;

            return result[4] == newDeviceAddress;
        }

        public string GetDeviceModel(SerialPort comPortName, byte deviceAddress)
        {
            var sPort = comPortName;
            // формируем команду на отправку
            var cmdString = new byte[] { 0x0D, 0x00, 0x00 };

            var repeat = 3;// кол-во попыток получить модель прибора
            for (int i = 0; i < repeat; i++)
            {
                var deviceModel = _orionNet.AddressTransaction(sPort,
                                                               deviceAddress, 
                                                               cmdString, 
                                                               IOrionNetTimeouts.Timeouts.readModel);

                if (!(deviceModel?.Length > 1))
                    return "";

                var extractionSuccess = _bolidDict.TryGetValue(deviceModel[3], out var deviceName);

                if (extractionSuccess)
                {
                    return deviceName;
                }
            }

            return "";
        }

        public bool IsDeviceOnline(SerialPort comPortName, byte deviceAddress)
        {
            var sPort = comPortName;

            // формируем команду на отправку
            var cmdString = new byte[] { 0x01, 0x00, 0x00 };

            var deviceModel = _orionNet.AddressTransaction(sPort, 
                                                           deviceAddress, 
                                                           cmdString, 
                                                           IOrionNetTimeouts.Timeouts.readModel);

            return deviceModel?.Length > 1;
        }

        public Dictionary<byte, string> SearchOnlineDevices(SerialPort comPortName, Action<int> progressStatus)
        {
            var sPort = comPortName;

            var progress = 1.0;
            var progressStep = 0.7874;

            progressStatus(Convert.ToInt32(progress));
            var result = new Dictionary<byte, string>();
            for (byte devAddr = 1; devAddr <= 127; devAddr++)
            {
                var OnlineDevicesModel = GetDeviceModel(sPort, devAddr);
                if (OnlineDevicesModel != string.Empty)
                {
                    result.Add(devAddr, OnlineDevicesModel);
                }

                progressStatus(Convert.ToInt32(progress));
                progress += progressStep;
            }

            return result;
        }
    }
}
