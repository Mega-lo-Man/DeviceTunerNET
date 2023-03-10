using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000M : OrionDevice
    {
        public C2000M(IPort port) : base(port)
        {
            ModelCode = 0;
        }
    }
}
