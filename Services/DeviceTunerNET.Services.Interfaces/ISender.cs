using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface ISender
    {
        /// <summary>
        /// Заливка конфига в коммутатор по протоколу Telnet или SSH
        /// </summary>
        /// <param name="ethernetDevice">Коммутатор</param>
        /// <param name="SettingsDict">Словарь с настройками коммутатора</param>
        /// <returns>Объект типа EthernetDevices с заполненными полями (которые удалось выцепить из коммутатора)</returns>
        public EthernetSwitch Send(EthernetSwitch ethernetDevice, Dictionary<string, string> SettingsDict);

        /// <summary>
        /// Создание нового сетевого соединения
        /// </summary>
        /// <param name="IPaddress">IP адрес коммутатора</param>
        /// <param name="Port">Сетевой порт (22 для SSH-соединения , 23 для Telnet-соединения)</param>
        /// <param name="UserName">Имя учетной записи на коммутаторе (например, по умолчанию admin)</param>
        /// <param name="Password">Пароль от учетной записи на коммутаторе (например, по умолчанию admin)</param>
        /// <param name="KeyFile">Путь к файлу ключа шифрования</param>
        /// <returns>True - соединение успешно создано, False - в противном случае</returns>
        public bool CreateConnection(string IPaddress, ushort Port, string Username, string Password, string KeyFile);

        /// <summary>
        /// Завершение сетевого подключения
        /// </summary>
        /// <returns>True - соединение успешно разорвано, False - в противном случае</returns>
        public bool CloseConnection();
    }
}
