using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs24_51 : RipRs
    {
        public RipRs24_51(IPort port) : base(port)
        {
            ModelCode = 39;
        }
    }
}
