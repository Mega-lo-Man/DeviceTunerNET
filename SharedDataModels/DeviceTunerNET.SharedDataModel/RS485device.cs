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
            get => _address_RS485;
            set
            {
                if (value > 0 && value <= 127) _address_RS485 = value;
            }
        }
        
        private List<byte[]> _getConfigCommandLines;
        /// <summary>
        /// Получить список команд которые содержат конфигурацию устройства в формате Болид
        /// </summary>
        public List<byte[]> GetConfigCommandLines => _getConfigCommandLines;
    }
}
