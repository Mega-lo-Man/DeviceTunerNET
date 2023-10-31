using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs24_54 : RipRs
    {
        public RipRs24_54(IPort port) : base(port)
        {
            ModelCode = 38;
        }
    }
}
