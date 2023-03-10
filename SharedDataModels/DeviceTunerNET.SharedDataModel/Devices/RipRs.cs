using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public abstract class RipRs : OrionDevice
    {
        public RipRs(IPort port) : base(port)
        {

        }
    }
}
