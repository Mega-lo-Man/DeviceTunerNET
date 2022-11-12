using System.IO.Ports;

namespace DeviceTunerNET.SharedDataModel
{
    public interface IRS485device
    {
        uint AddressRS485 { get; set; }
        
        bool WriteConfig(SerialPort serialPort, RS485device.SearchStatus searchStatus);
    }
}