using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceTunerNET.Modules.ModuleSwitch.Models
{
    public interface INetworkTasks
    {
        /// <summary>
        /// Upload config to switch
        /// </summary>
        /// <param name="nDevice">Switch object</param>
        /// <param name="settings">Dictionary of parameters transferred to the switch</param>
        /// <returns>true if success</returns>
        public bool UploadConfigStateMachine(EthernetSwitch nDevice,
                                             Dictionary<string, string> settings,
                                             CancellationToken tokenSource);

        /// <summary>
        /// Send ping
        /// </summary>
        /// <param name="IPAddress">IP address</param>
        /// <returns>true if ping success</returns>
        public bool SendPing(string IPAddress);

        /// <summary>
        /// Send multiple ping
        /// </summary>
        /// <param name="IPAddress">IP address</param>
        /// <param name="NumberOfRepetitions">Number of repetitions ping</param>
        /// <returns>true if number of success repetitions >= 50%</returns>
        public bool SendMultiplePing(string IPAddress, int NumberOfRepetitions);
    }
}
