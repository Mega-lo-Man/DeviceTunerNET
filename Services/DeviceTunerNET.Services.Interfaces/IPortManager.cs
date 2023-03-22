using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IPortManager
    {
        ObservableCollection<string> GetAvailableCOMPorts();
    }
}
