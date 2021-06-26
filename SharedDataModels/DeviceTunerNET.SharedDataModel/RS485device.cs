using System.Collections.Generic;

namespace DeviceTunerNET.SharedDataModel
{
    public class RS485device : Device
    {
        /// <summary>
        /// Адрес прибора на линии RS-485 ("23")
        /// </summary>
        private int? _address_RS485;
        public int? AddressRS485
        {
            get { return _address_RS485; }
            set { if (value > 0 && value <= 127) _address_RS485 = value; }
        }
        /// <summary>
        /// Получить список команд которые содержат конфигурацию устройства в формате Болид
        /// </summary>
        private List<byte[]> _getConfigCommandLines;
        public List<byte[]> GetConfigCommandLines
        {
            get { return _getConfigCommandLines; }
            private set { _getConfigCommandLines = value; }
        }
    }
}
