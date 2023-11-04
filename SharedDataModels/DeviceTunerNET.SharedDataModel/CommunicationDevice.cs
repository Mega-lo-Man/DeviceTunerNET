using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel
{
    public abstract class CommunicationDevice : Device, ICommunicationDevice
    {
        public IPort Port { get; set; }
    }
}
