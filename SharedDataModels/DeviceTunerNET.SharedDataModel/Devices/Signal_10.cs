using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class Signal_10 : OrionDevice
    {
        public Signal_10(IPort port) : base(port)
        {
            ModelCode = 34;
        }
    }
}
