using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IDeviceConfigUploader
    {
        /// <summary>
        /// Com port name
        /// </summary>
        string PortName { get; set; }

        /// <summary>
        /// Transaction protocol
        /// </summary>
        string Protocol { get; set; }

        /// <summary>
        /// Method for update progress bar
        /// </summary>
        public Action<int> Progress { get; set; }

        /// <summary>
        /// Upload config into device
        /// </summary>
        /// <param name="device">device object for config uploading</param>
        /// <param name="serialNumb">Device serial number</param>
        /// <returns>Return true if config has uploaded (without any exceptions) otherwise return false</returns>
        bool Upload(RS485device device, string serialNumb);
    }
}
