using DeviceTunerNET.SharedDataModel.Devices;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel
{
    public interface IPort
    {
        /// <summary>
        /// The maximum number of attempts to send a message
        /// </summary>
        int MaxRepetitions { get; set; }

        /// <summary>
        /// Maximum waiting time for response to a request
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// Send data to the port and get response
        /// </summary>
        /// <param name="data">Raw translate data</param>
        /// <returns>Raw response from receiver</returns>
        byte[] Send(byte[] data);

        /// <summary>
        /// Send data to the port without confirmation from device
        /// </summary>
        /// <param name="data">Raw translate data</param>
        void SendWithoutСonfirmation(byte[] data);
    }
}
