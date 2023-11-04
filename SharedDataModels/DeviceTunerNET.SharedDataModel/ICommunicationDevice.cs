using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel
{
    public interface ICommunicationDevice : IDevice
    {
        /// <summary>
        /// Порт
        /// </summary>
        IPort Port { get; set; }
    }
}
