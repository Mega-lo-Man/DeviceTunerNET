using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs12_51 : RipRs
    {
        public RipRs12_51(IPort port) : base(port)
        {
            ModelCode = 33;
        }
    }
}
