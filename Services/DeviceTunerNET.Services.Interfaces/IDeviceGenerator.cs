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

        /// <summary>
        /// Get an IOrionDevice instance based on its device code
        /// </summary>
        /// <param name="code">Device code</param>
        /// <param name="device">Device instance</param>
        /// <returns>true if instance creating was succes</returns>
        public bool TryGetDeviceByCode(int code, out IOrionDevice device);
    }
}
