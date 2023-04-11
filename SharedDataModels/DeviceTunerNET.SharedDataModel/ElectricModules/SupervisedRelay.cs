using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.ElectricModules
{
    public class SupervisedRelay : OptoRelay
    {
        public enum MonitorFor : byte
        {
            NoMonitoring = 0x00,
            OpenFaults = 0x01,
            ShortFaults = 0x02,
            OpenShortsFaults = 0x03,
        }

        public MonitorFor Monitoring { get; set; } = MonitorFor.NoMonitoring;

        public SupervisedRelay(IOrionDevice orionDevice, byte relayIndex) : base(orionDevice, relayIndex)
        {
        }
    }
}
