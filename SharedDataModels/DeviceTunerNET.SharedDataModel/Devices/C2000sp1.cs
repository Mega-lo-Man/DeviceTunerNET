using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000sp1 : OrionDevice
    {
        public C2000sp1(IPort port) : base(port)
        {
            ModelCode = 3;
        }
    }
}
