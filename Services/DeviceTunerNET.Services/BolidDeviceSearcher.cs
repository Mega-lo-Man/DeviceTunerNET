using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.Ports;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DeviceTunerNET.Services
{
    public class BolidDeviceSearcher : IDeviceSearcher
    {
        public IPort Port { get; set; }

        public IEnumerable<IOrionDevice> SearchDevices(CancellationToken cancellationToken, Action<int> updateProgressBar)
        {
            var c2000M = new C2000M(Port);
            var onlineDevices = c2000M.SearchOnlineDevices(updateProgressBar, cancellationToken);

            return onlineDevices;
        }
    }
}
