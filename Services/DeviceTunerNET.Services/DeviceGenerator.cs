using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using System;
using System.CodeDom;
using System.Collections.Generic;

namespace DeviceTunerNET.Services
{
    public class DeviceGenerator : IDeviceGenerator
    {
        

        private readonly Dictionary<string, Func<IEthernetDevice>> _ethernetSwitches = new()
        {
            {"MES3508", () => new EthernetSwitch(null) },
            {"MES3508P", () => new EthernetSwitch(null) },
            {"MES2308", () => new EthernetSwitch(null) },
            {"MES2308_AC", () => new EthernetSwitch(null) },
            {"MES2308_DC", () => new EthernetSwitch(null) },
            {"MES2308P", () => new EthernetSwitch(null) },
            {"MES2308P_AC", () => new EthernetSwitch(null) },
            {"MES2308P_DC", () => new EthernetSwitch(null) },
            {"MES2324", () => new EthernetSwitch(null) },
            {"MES2324_AC", () => new EthernetSwitch(null) },
            {"MES2324_DC", () => new EthernetSwitch(null) },
            {"MES2424_AC", () => new EthernetSwitch(null) },
            {"MES2424P_AC", () => new EthernetSwitch(null) },
            {"MES2408_AC", () => new EthernetSwitch(null) },
            {"MES2408P_AC", () => new EthernetSwitch(null) },
        };

        public DeviceGenerator()
        {

        }

        public bool TryGetDevice(string name, out ICommunicationDevice device)
        {
            device = default;
            if (_ethernetSwitches.ContainsKey(name))
            {
                device = _ethernetSwitches[name]();
                return true;
            }

            return false;
        }
    }
}
