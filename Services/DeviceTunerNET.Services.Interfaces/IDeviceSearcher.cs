using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IDeviceSearcher
    {
        IPort Port { get; set; }

        IEnumerable<IOrionDevice> SearchDevices(CancellationToken cancellationToken, Action<int> updateProgressBar);
    }
}
