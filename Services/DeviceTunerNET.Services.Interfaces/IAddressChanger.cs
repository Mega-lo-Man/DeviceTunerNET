using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IAddressChanger
    {
        IPort Port { get; set; }
        List<RS485device> FoundDevices { get; set; }

        string DefaultDeviceFoundMessage { get; }
        void ChangeDefaultAddresses(CancellationToken cancellationToken);

        void TryToChangeDeviceAddress(uint address, IOrionDevice currentDevice);

        uint GetFirstMissing(IEnumerable<uint> numbers, uint maximum);
    }
}
