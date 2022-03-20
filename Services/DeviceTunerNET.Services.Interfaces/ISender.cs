using DeviceTunerNET.SharedDataModel;
using System.Collections.Generic;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface ISender
    {
        /// <summary>
        /// Заливка конфига в коммутатор по протоколу Telnet или SSH
        /// </summary>
        /// <param name="ethernetSwitch">Коммутатор</param>
        /// <param name="settingsDict">Словарь с настройками коммутатора</param>
        /// <returns>Объект типа EthernetDevices с заполненными полями (которые удалось выцепить из коммутатора)</returns>
        public EthernetSwitch Send(EthernetSwitch ethernetSwitch, Dictionary<string, string> settingsDict);

        /// <summary>
        /// Создание нового сетевого соединения
        /// </summary>
        /// <param name="ipAddress">IP адрес коммутатора</param>
        /// <param name="port">Сетевой порт (22 для SSH-соединения , 23 для Telnet-соединения)</param>
        /// <param name="username">Имя учетной записи на коммутаторе (например, по умолчанию admin)</param>
        /// <param name="password">Пароль от учетной записи на коммутаторе (например, по умолчанию admin)</param>
        /// <param name="rsaKeyFile">Путь к файлу ключа шифрования</param>
        /// <returns>True - соединение успешно создано, False - в противном случае</returns>
        public bool CreateConnection(string ipAddress, ushort port, string username, string password, string rsaKeyFile);

        /// <summary>
        /// Завершение сетевого подключения
        /// </summary>
        /// <returns>True - соединение успешно разорвано, False - в противном случае</returns>
        public bool CloseConnection();
    }
}
