using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using System.Collections.Generic;
using System.IO.Ports;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IOrionCommon : IOrionNetTimeouts
    {
        public delegate void SearchStatus(int progress);
        byte CurrentDeviceAdderess { get; set; }

        string GetDeviceModel(SerialPort comPortName, byte deviceAddress);
        bool IsDeviceOnline(SerialPort comPortName, byte deviceAddress);
        Dictionary<byte, string> SearchOnlineDevices(SerialPort comPortName, SearchStatus searchStatus);
        bool SetDeviceAddress(SerialPort comPortName, byte deviceAddress, byte newDeviceAddress);
    }
}