using DeviceTunerNET.SharedDataModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface ISerialTasks
    {
        /// <summary>
        /// Send config into RS-devices
        /// </summary>
        /// <typeparam name="T">Device type</typeparam>
        /// <param name="device">Device</param>
        /// <param name="comPort">Com port name</param>
        /// <param name="rsAddress">Device address </param>
        /// <returns>Error code</returns>
        public int SendConfig<T>(T device, string comPort, int rsAddress);

        /// <summary>
        /// Shifting devices addresses
        /// </summary>
        /// <param name="comPort">Com port name</param>
        /// <param name="StartAddress"></param>
        /// <param name="TargetAddress"></param>
        /// <param name="Range">Address range</param>
        /// <returns></returns>
        public int ShiftDevicesAddresses(string ComPort, int StartAddress, int TargetAddress, int Range);

        /// <summary>
        /// Get all online devices on the RS-485 Line
        /// </summary>
        /// <param name="ComPort">Com port name</param>
        /// <returns>Collection of all found RS485-devices</returns>
        public IEnumerable<RS485device> GetOnlineDevices(string ComPort);

        public ObservableCollection<string> GetAvailableCOMPorts();
    }
}
