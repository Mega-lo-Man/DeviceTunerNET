using DeviceTunerNET.SharedDataModel.Devices;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using static DeviceTunerNET.SharedDataModel.Devices.IOrionNetTimeouts;

namespace DeviceTunerNET.SharedDataModel
{
    public class RS485device : Device, IRS485device
    {
        /// <summary>
        /// Адрес прибора на линии RS-485 ("23").
        /// </summary>
        public uint AddressRS485 { get; set; }

        public IEnumerable<string> SupportedModels { get; set; }

        /// <summary>
        /// Делегат для получения текущего прогресса выполнения заливки конфига
        /// </summary>
        /// <param name="progress"></param>
        public delegate void SearchStatus(int progress);
        public virtual void WriteConfig(Action<int> searchStatus)
        {

        }
    }
}
