using DeviceTunerNET.SharedDataModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface ISerialSender
    {
        public SerialPort GetSerialPortObjectRef();

        /// <summary>
        /// Получить коллекцию установленных в системе COM-портов
        /// </summary>
        /// <returns>коллекцию названий доступных COM-портов (COM1, COM2, COM3, ...)</returns>
        public ObservableCollection<string> GetAvailableCOMPorts();

        /// <summary>
        /// Смена адреса устройства подключенного на линию RS-485.
        /// </summary>
        /// <param name="comPort">COM-порт</param>
        /// <param name="deviceAddress">Текущий адрес</param>
        /// <param name="newDeviceAddress">Новый адрес</param>
        /// <returns>true если смена адреса прошла успешно, в противном случае - false</returns>
        public bool SetDeviceRS485Address(SerialPort comPort, byte deviceAddress, byte newDeviceAddress);

        /// <summary>
        /// Заливка конфига в С2000-Ethernet на порту RS-232.
        /// </summary>
        /// <param name="comPort">COM-порт</param>
        /// <param name="deviceAddress">Текущий адрес</param>
        /// <param name="device">Новый конфиг</param>
        /// <returns>true если смена адреса прошла успешно, в противном случае - false</returns>
        public bool SetC2000EthernetConfig(SerialPort comPort, byte deviceAddress, C2000Ethernet device);

        /// <summary>
        /// Поиск всех устройств на линии RS485.
        /// </summary>
        /// <param name="comPort">COM-порт</param>
        /// <returns>Список найденных пар адрес - модель устройства</returns>
        public Dictionary<byte, string> SearchOnlineDevices(SerialPort comPort);

        /// <summary>
        /// Запрос модели устройства
        /// </summary>
        /// <param name="comPort">COM-порт на которой находится устройство</param>
        /// <param name="deviceAddress">Адрес устройства</param>
        /// <returns>строку содержащую название модели</returns>
        public string GetDeviceModel(SerialPort comPort, byte deviceAddress);

        /// <summary>
        /// "Пинг" устройства на линии RS485
        /// </summary>
        /// <param name="comPort">COM-порт на которой находится устройство</param>
        /// <param name="deviceAddress">Адрес пингуемого устройства</param>
        /// <returns>true - прибор отвечает по указанному адресу, false - нет ответа</returns>
        public bool IsDeviceOnline(SerialPort comPort, byte deviceAddress);
    }
}
