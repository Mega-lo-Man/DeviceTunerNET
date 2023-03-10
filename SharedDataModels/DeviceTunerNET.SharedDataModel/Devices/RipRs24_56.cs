using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs24_56 : RipRs
    {
        public RipRs24_56(IPort port) : base(port)
        {
            ModelCode = 55;
        }
    }
}
