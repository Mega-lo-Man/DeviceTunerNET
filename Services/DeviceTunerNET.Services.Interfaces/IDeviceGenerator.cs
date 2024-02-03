using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IDeviceGenerator
    {
        /// <summary>
        /// Get an ICommunicationDevice instance based on its description
        /// </summary>
        /// <param name="name">Device model</param>
        /// <param name="device"></param>
        /// <returns></returns>
        bool TryGetDevice(string name, out ICommunicationDevice device);
    }
}
