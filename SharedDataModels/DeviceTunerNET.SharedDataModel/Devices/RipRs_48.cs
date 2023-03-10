using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs_48 : RipRs
    {
        public RipRs_48(IPort port) : base(port)
        {
            ModelCode = 53;
        }
    }
}
