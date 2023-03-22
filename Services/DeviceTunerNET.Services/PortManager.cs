using DeviceTunerNET.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;

namespace DeviceTunerNET.Services
{
    public class PortManager : IPortManager
    {
        public ObservableCollection<string> GetAvailableCOMPorts()
        {
            var ports = SerialPort.GetPortNames();
            var portsList = new ObservableCollection<string>();
            foreach (var port in ports)
            {
                portsList.Add(port);
            }
            return portsList;
        }
    }
}
