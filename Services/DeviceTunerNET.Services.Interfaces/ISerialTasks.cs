using DeviceTunerNET.SharedDataModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface ISerialTasks
    {
        public enum ResultCode
        {
            ok = 1,
            undefinedError = 0,
            deviceTypeMismatch = -1,
            addressFieldNotValid = -2,
            deviceNotRespond = -3,
            errorConfigDownload = -4
        }

        /// <summary>
        /// Send config into RS-devices
        /// </summary>
        /// <typeparam name="T">Device type</typeparam>
        /// <param name="device">Device</param>
        /// <param name="comPort">Com port name</param>
        /// <param name="rsAddress">Device address </param>
        /// <returns>Error code</returns>
        public ResultCode SendConfig<T>(T device, string comPort, int rsAddress);

        /// <summary>
        /// Shifting devices addresses
        /// </summary>
        /// <param name="comPort">Com port name</param>
        /// <param name="startAddress"></param>
        /// <param name="targetAddress"></param>
        /// <param name="range">Address range</param>
        /// <returns></returns>
        public ResultCode ShiftDevicesAddresses(string comPort, int startAddress, int targetAddress, int range);

        /// <summary>
        /// Get all online devices on the RS-485 Line
        /// </summary>
        /// <param name="comPort">Com port name</param>
        /// <returns>Collection of all found RS485-devices</returns>
        public IEnumerable<RS485device> GetOnlineDevices(string comPort);

        public ResultCode CheckOnlineDevice(string comPort, RS485device device);

        public ObservableCollection<string> GetAvailableCOMPorts();
    }
}
