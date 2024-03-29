﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel
{
    public interface Irs485device : ICommunicationDevice
    {
        /// <summary>
        /// Адрес прибора на линии RS-485 ("23").
        /// </summary>
        public uint AddressRS485 { get; set; }
    }
}
