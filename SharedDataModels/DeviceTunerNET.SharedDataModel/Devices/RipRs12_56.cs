using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs12_56 : RipRs
    {
        public RipRs12_56(IPort port) : base(port)
        {
            ModelCode = 54;
        }
    }
}
