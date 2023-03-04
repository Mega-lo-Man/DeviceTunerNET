using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace DeviceTunerNET.SharedDataModel
{
    public interface IRS485device
    {
        uint AddressRS485 { get; set; }

        IEnumerable<string> SupportedModels { get; set; }

        void WriteConfig(Action<int> searchStatus);
    }
}