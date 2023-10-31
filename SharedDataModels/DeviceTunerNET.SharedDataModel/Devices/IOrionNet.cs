using System.IO.Ports;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public interface IOrionNet : IOrionNetTimeouts
    {
        byte[] AddressTransaction(SerialPort serialPort, byte address, byte[] sendArray, Timeouts timeout);
    }
}