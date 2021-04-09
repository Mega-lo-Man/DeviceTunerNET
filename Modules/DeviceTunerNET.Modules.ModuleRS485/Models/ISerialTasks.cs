using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Modules.ModuleRS485.Models
{
    public interface ISerialTasks
    {
        public int SendConfig<T>(T device, string comPort, int rsAddress);
        public ObservableCollection<string> GetAvailableCOMPorts();
    }
}
